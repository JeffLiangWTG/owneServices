using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.ZClientCCP.Kawasaki.Testing
{
	class KawasakiInvoiceDataImporterTest : TransactionedTestCase
	{
		public void TestNumberOfImportedInvoicesAndLines()
		{
			AssertAnInvoiceIsCreated("MAJN04", 9);
			AssertAnInvoiceIsCreated("MAJN06", 3);
			AssertAnInvoiceIsCreated("MSJN02", 6);
			AssertAnInvoiceIsCreated("MSJN03", 5);
			AssertAnInvoiceIsCreated("PAJN03", 4);
			AssertAnInvoiceIsCreated("PSJN01", 9);
		}

		public void TestDeclarationDoesNotExist()
		{
			AssertEquals("There should be no JobDeclarations", false, jobDec.IsPersistent);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var factoryLocal = new BusinessObjectFactory();
			var branch = factoryLocal.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, SQLComparisonOperator.Equal, "SYD"));
			buyerOrg = factoryLocal.New<OrgHeader>();
			buyerOrg.OH_FullName = "KAWMOT";
			buyerOrg.OH_RL_NKClosestPort = "AUSYD";
			buyerOrg.CompanyData.OB_GB_ControllingBranch = branch.PK;
			buyerOrg.OH_IsConsignor = true;
			buyerOrg.OH_IsActive = true;
			buyerOrg.MainAddress.OA_Address1 = "123 Fake St";
			buyerOrg.MainAddress.OA_Address2 = "Sydney Town";
			buyerOrg.RunPreSaveValidation();
			factoryLocal.Save();
			jobDec = factoryLocal.New<BaseJobDeclaration>();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var path = resourceRetriever.SaveResourceToFile("Enterprise.Client.ZClientCCP.Testing.Kawasaki.TestFiles.KawasakiCustomsData.txt");
			importer = new KawasakiInvoiceDataImporter(path, jobDec);
			importer.Import();
			factoryLocal.Save();
			factory = new BusinessObjectFactory();
		}
		KawasakiInvoiceDataImporter importer;
		BaseJobDeclaration jobDec;
		BusinessObjectFactory factory;
		OrgHeader buyerOrg;
		EmbeddedResourceRetriever resourceRetriever;

		void AssertAnInvoiceIsCreated(ZString invoiceNumber, int numberOfInvoiceLines)
		{
			ZQuery query = new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, invoiceNumber);
			BaseJobComInvoiceHeader invoice = factory.LoadTop1<BaseJobComInvoiceHeader>(query);
			AssertNotNull("Invoice is created for the number, " + invoiceNumber, invoice);
			AssertEquals("Number of invoice lines", numberOfInvoiceLines, invoice.JobComInvoiceLines.Count);
			AssertEquals("JZ_JE is cleared", ZGuid.Empty, invoice.JZ_JE);
			AssertEquals("JZ_JZ_GroupInvoiceFK is cleared", ZGuid.Empty, invoice.JZ_JZ_GroupInvoiceFK);
			AssertEquals("Buyer must be KAWMOTSYD", OrgHeader.LoadFromCode(factory, "KAWMOTSYD").PK, invoice.JZ_OH_Buyer);
			RefCurrency audCurrency = factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.Equal, "AUD"));
			AssertEquals("Invoice " + invoiceNumber + " should have a Currency of AUD", audCurrency.RX_Code, invoice.JZ_RX_NKInvoice_Currency);
		}

		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}
	}
}
