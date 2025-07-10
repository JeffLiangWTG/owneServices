using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.MFI.Data;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.MFI.Testing
{
	public class EStatementImporterTest : TestCaseWithFactory
	{
		public void TestCreateConverter()
		{
			AssertEquals(typeof(EStatementConverter), fImporter.InternalCreateConverterTest(null).GetType());
		}

		public void TestCreateXsd()
		{
			AssertEquals(typeof(Xsd.TxnHeaderCollection), fImporter.InternalCreateXsdTest().GetType());
		}

		public void TestFlatFileFormat()
		{
			AssertEquals(typeof(CsvFlatFileFormat), fImporter.InternalIFlatFileFormatTest.GetType());
		}

		#region Implementation

		EStatementImporter fImporter;
		protected override void SetUp()
		{
			base.SetUp();
			fImporter = new EStatementImporter();
		}
		#endregion

		public void TestExtractToDataAdapter()
		{
			SetupData();
			Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NotificationBuffer notify = new NotificationBuffer();
			EStatementImporterForTesting importer = new EStatementImporterForTesting(Factory);
			Xsd.TxnHeaderCollection valueObject = new Xsd.TxnHeaderCollection();
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile("DataImportExport.ValidData_Sample.csv");
				using (TextReader reader = new StreamReader(testFilePath))
				{
					EStatementConverter converter = new EStatementConverter(notify, Factory);
					converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
					importer.ExtractToDataAdapter(valueObject, notify);
				}

				AssertNotNull(importer.ImportedInvoice);
				AssertEquals("Imported Invoice Type:", "INV", importer.ImportedInvoice.AH_TransactionType);
				AssertEquals("Imported Invoice Number:", "maiicu1-200137", importer.ImportedInvoice.InvoiceNumber);
				AssertEquals("Imported Invoice Amount", 521.47m, importer.ImportedInvoice.AH_OSExTaxAmount);
				AssertEquals("Imported Invoice GSTAmount:", 65.19m, importer.ImportedInvoice.AH_OSTaxAmount);
				AssertEquals("Imported Invoice Lines Count:", 3, importer.ImportedInvoice.Lines.Count);
			}
		}

		void SetupData()
		{
			JobDeclaration dec = JobDeclaration.New(Factory);
			dec.JE_DeclarationReference = "B00001000";
			OrgHeader org = OrgHeader.New(Factory);
			org.OH_Code = "ORGTEST1";
			org.OH_FullName = "Org Test1";
			org.OH_IsCreditor = ZBool.True;
			org.CompanyData.SetARTaxApplicable(true);
			org.CompanyData.SetAPTaxApplicable(true);
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "CHARGE1";
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_Desc = "New test charge code";
			AccChargeCode defaultChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			defaultChargeCode.AC_Code = "CHARGE2";
			defaultChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			defaultChargeCode.AC_Desc = "New test default charge code";
			Factory.Save();
		}

		class EStatementImporterForTesting : EStatementImporter
		{
			public EStatementImporterForTesting(BusinessObjectFactory factory) : base(factory)
			{
			}

			public new bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
			{
				return base.ExtractToDataAdapter(xSD, notifications);
			}
		}
	}
}
