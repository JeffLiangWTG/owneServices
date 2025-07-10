using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceLine))]
	class WarehouseAdjustmentAddInfoJobComInvoiceLineValidationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCheckZG_CountryOfSupply()
		{
			invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
			AssertNoNotifications(invoiceLine.ZG_CountryOfSupplyInfo);
		}

		public void TestCheckZG_CessionFlag()
		{
			invoiceLine.AddInfoValidation.ValidateZG_CessionFlag();
			AssertNoNotifications(invoiceLine.JI_CessionFlagInfo);
		}

		public void TestCheckZG_QuotaQty()
		{
			invoiceLine.AddInfoValidation.ValidateZG_QuotaQty();
			AssertNoNotifications(invoiceLine.ZG_QuotaQtyInfo);
		}

		public void TestCheckZG_QuotaUQ()
		{
			invoiceLine.AddInfoValidation.ValidateZG_QuotaUQ();
			AssertNoNotifications(invoiceLine.ZG_QuotaUQInfo);
		}

		public void TestCheckZG_TobaccoStamp()
		{
			invoiceLine.AddInfoValidation.ValidateZG_TobaccoStamp();
			AssertNoNotifications(invoiceLine.JI_TobaccoStampInfo);
		}

		protected override BusinessObject GetNewBusinessObject() => new AddInfoJobComInvoiceLine(Factory.New<JobComInvoiceLine>().JI_AddInfoInfo);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
	}
}
