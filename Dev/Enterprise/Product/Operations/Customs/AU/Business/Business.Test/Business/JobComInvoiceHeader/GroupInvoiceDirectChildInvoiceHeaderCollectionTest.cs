using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	public class GroupInvoiceDirectChildInvoiceHeaderCollectionTest : Customs.Business.Testing.BaseGroupInvoiceDirectChildInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection>
	{
		public void TestInvoiceHeaderHasJZ_JESet()
		{
			JobDeclaration testJobDeclaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader header = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			AssertEquals("Header has JZ_JE set", testJobDeclaration.PK, header.JZ_JE);
		}

		protected override InvoiceHeaderActiveCollection GetCollectionToTest()
		{
			return (InvoiceHeaderActiveCollection)TestJobDeclaration.TopGroupInvoice.JobComInvoiceHeaders;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			JobComInvoiceHeader result = Factory.New<JobComInvoiceHeader>();
			result.JZ_JE = TestJobDeclaration.PK;
			result.JZ_JZ_GroupInvoiceFK = TestJobDeclaration.TopGroupInvoice.PK;
			return result;
		}

		public void TestDefaultIncoTermFromDeclaration()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_INCO = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			JobDeclaration testJobDeclaration = Factory.New<JobDeclaration>();
			testJobDeclaration.JE_JS = shipment.PK;
			testJobDeclaration.JE_ShipmentIncoTerm = shipment.JS_INCO;
			JobComInvoiceHeader header = testJobDeclaration.Invoices.AddNew();

			AssertEquals("Header.JZ_IncoTerm", Core.Constants.IncoTerms.CostInsuranceAndFreight, header.JZ_IncoTerm);
		}

		#region Implementation

		protected JobDeclaration TestJobDeclaration
		{
			get { return fTestJobDeclaration ?? (fTestJobDeclaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration fTestJobDeclaration;

		protected RefCurrency aUDCurrency;
		protected override void SetUp()
		{
			base.SetUp();
			aUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
		}

		#endregion

	}
}
