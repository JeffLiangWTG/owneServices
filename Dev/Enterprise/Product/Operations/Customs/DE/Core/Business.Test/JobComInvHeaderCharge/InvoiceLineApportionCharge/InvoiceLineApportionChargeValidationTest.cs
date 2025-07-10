using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class InvoiceLineApportionChargeValidationTest : TestCaseWithFactory
	{
		public void TestCheckJ7_IsGSTApplicable_IsFreightChargeToEUBorderByAirInsideEU()
		{
			const string message = "is GST-applicable.";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			invoice.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._3;
			invoiceLineApportionCharge.J7_ChargeType = ImportChargeCodeList.Codes._011;
			invoiceLineApportionCharge.J7_IsGSTApplicable = false;
			var info = invoiceLineApportionCharge.J7_IsGSTApplicableInfo;

			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("IsFreightChargeToEUBorderByAirInsideEU = False", info, message);

				invoiceLineApportionCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
				invoiceLineApportionCharge.J7_IsGSTApplicable = false;
				AssertNoMessageErrorContaining("IsFreightChargeToEUBorderByAirInsideEU = True", info, message);
			});
		}

		public void TestCheckJ7_AmountGreaterThan0()
		{
			const string message = "Amount cannot be zero.";
			var info = invoiceLineApportionCharge.J7_AmountInfo;
			CombineAssertions(() =>
			{
				invoiceLineApportionCharge.J7_Amount = 0.0;
				AssertHasMessageErrorContaining("In Import Declaration,J7_Amount should greater than 0", info, message);

				invoiceLineApportionCharge.J7_Amount = 1.2;
				AssertNoMessageErrorContaining("In Import Declaration,J7_Amount should greater than 0", info, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLineApportionCharge = Factory.New<InvoiceLineApportionCharge>();
			invoiceLineApportionCharge.J7_ParentTableCode = invoiceLine.TablePrefix;
			invoiceLineApportionCharge.J7_ParentID = invoiceLine.PK;
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		InvoiceLineApportionCharge invoiceLineApportionCharge;
	}
}
