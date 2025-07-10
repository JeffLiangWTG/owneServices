using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class InvoiceApportionChargeValidationTest : TestCaseWithFactory
	{
		public void TestCheckJ7_IsIncludedInITOT()
		{
			const string message = "cannot include this charge in lines.";
			var info = invoiceApportionCharge.J7_IsIncludedInITOTInfo;
			invoiceApportionCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
			invoiceApportionCharge.J7_IsIncludedInITOT = true;

			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("Code '010'", info, message);

				foreach (var code in new[] { ImportChargeCodeList.Codes._014, ImportChargeCodeList.Codes._015, ImportChargeCodeList.Codes._016, ImportChargeCodeList.Codes._017 })
				{
					invoiceApportionCharge.J7_ChargeType = code;
					invoiceApportionCharge.Validation.ValidateJ7_IsIncludedInITOT();
					AssertNoMessageErrorContaining($"Code '{code}'", info, message);
				}
			});
		}

		public void TestCheckJ7_IsGSTApplicable_IsFreightChargeToEUBorderByAirInsideEU()
		{
			const string message = "is GST-applicable.";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			invoice.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._3;
			invoiceApportionCharge.J7_ChargeType = ImportChargeCodeList.Codes._011;
			invoiceApportionCharge.J7_IsGSTApplicable = false;
			var info = invoiceApportionCharge.J7_IsGSTApplicableInfo;

			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("IsFreightChargeToEUBorderByAirInsideEU = False", info, message);

				invoiceApportionCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
				invoiceApportionCharge.J7_IsGSTApplicable = false;
				AssertNoMessageErrorContaining("IsFreightChargeToEUBorderByAirInsideEU = True", info, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoiceApportionCharge = Factory.New<InvoiceApportionCharge>();
			invoiceApportionCharge.J7_ParentTableCode = invoice.TablePrefix;
			invoiceApportionCharge.J7_ParentID = invoice.PK;
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		InvoiceApportionCharge invoiceApportionCharge;
	}
}
