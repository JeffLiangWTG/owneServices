using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.CA;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DutyAndTaxValidationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestQuantity()
		{
			var tax = invoiceLine.DutiesAndTaxes.AddNew();
			tax.C1_RateType = RateTypes.Codes.Specific;
			tax.C1_UnitOfMeasure = "MIL";
			tax.C1_Rate = 1m;
			AssertHasMessageError(tax.QuantityInfo, "No Quantity has been found for this specific calculation line. Are you missing a second quantity with units MIL?");

			invoiceLine.JI_CustomsSecondUnitQty = "MIL";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			AssertNoMessageError(tax.QuantityInfo, "No Quantity has been found for this specific calculation line. Are you missing a second quantity with units MIL?");

			var pivot = Factory.New<CusClassPartPivot>();
			tax = pivot.DutiesAndTaxes.AddNew();
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			tax.C1_RateType = RateTypes.Codes.AdValorem;
			tax.C1_Rate = 10m;
			AssertNoMessageError(tax.QuantityInfo, "No Quantity has been found for this specific calculation line. Are you missing a second quantity with units MIL?");

			var classification = Factory.New<CusClassification>();
			tax = classification.DutiesAndTaxes.AddNew();
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			tax.C1_RateType = RateTypes.Codes.AdValorem;
			tax.C1_Rate = 10m;
			AssertNoMessageError(tax.QuantityInfo, "No Quantity has been found for this specific calculation line. Are you missing a second quantity with units MIL?");
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
		}

		JobComInvoiceLine invoiceLine;
		JobDeclaration declaration;

		#endregion
	}
}
