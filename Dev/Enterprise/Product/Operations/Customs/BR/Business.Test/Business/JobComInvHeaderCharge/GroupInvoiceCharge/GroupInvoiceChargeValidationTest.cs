using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class GroupInvoiceChargeValidationTest : TestCaseWithFactory
	{
		public void TestCheckJ7_DistributeBy_Apportionment()
		{
			var testDec = Factory.New<JobDeclaration>();
			var topGroup = testDec.JobComInvoiceGroupHeaders[0];
			var charge = topGroup.Charges.AddNew();
			charge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			var invoice = topGroup.JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.NetWeight;
			AssertHasMessageError(charge.J7_DistributeByInfo, "There are invoice lines that don't have a net weight. The apportionment of this charge won't be correct for the invoice lines.");

			charge.J7_DistributeBy = ChargeDistributeByList.Codes.FOB;
			AssertHasMessageError(charge.J7_DistributeByInfo, "There are invoice lines that don't have a fob value. The apportionment of this charge won't be correct for the invoice lines.");
		}

		public void TestJ7_DistributeBy_List()
		{
			#region Import Siscomex

			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportSiscomex, ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, ChargeDistributeByList.Codes.FOB, ChargeDistributeByList.Codes.NetWeight);
			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportSiscomex, ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid, ChargeDistributeByList.Codes.FOB, ChargeDistributeByList.Codes.NetWeight);
			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportSiscomex, ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory, ChargeDistributeByList.Codes.FOB, ChargeDistributeByList.Codes.NetWeight);

			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportSiscomex, CustomsChargeTypeList.Codes.OverseasInsurance, ChargeDistributeByList.Codes.NetWeight, ChargeDistributeByList.Codes.FOB);

			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportSiscomex, ImportCustomsChargeTypeList.Codes.PackingCosts, ChargeDistributeByList.Codes.NetWeight, ChargeDistributeByList.Codes.FOB);
			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportSiscomex, ImportCustomsChargeTypeList.Codes.OtherDeductionsCustomsValue, ChargeDistributeByList.Codes.NetWeight, ChargeDistributeByList.Codes.FOB);

			#endregion

			#region Import License

			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportLicense, ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, ChargeDistributeByList.Codes.FOB, ChargeDistributeByList.Codes.NetWeight);
			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportLicense, ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid, ChargeDistributeByList.Codes.FOB, ChargeDistributeByList.Codes.NetWeight);
			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportLicense, ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory, ChargeDistributeByList.Codes.FOB, ChargeDistributeByList.Codes.NetWeight);

			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportLicense, CustomsChargeTypeList.Codes.OverseasInsurance, ChargeDistributeByList.Codes.NetWeight, ChargeDistributeByList.Codes.FOB);

			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportLicense, ImportCustomsChargeTypeList.Codes.PackingCosts, ChargeDistributeByList.Codes.NetWeight, ChargeDistributeByList.Codes.FOB);
			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportLicense, ImportCustomsChargeTypeList.Codes.OtherDeductionsCustomsValue, ChargeDistributeByList.Codes.NetWeight, ChargeDistributeByList.Codes.FOB);

			#endregion
		}

		void AssertMessageErrorDistributeBy(ZString messageType, ZString chargeType, ZString distributeBy, ZString validDistributeBy)
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			invoice.JobDeclaration.JE_MessageType = messageType;

			var groupCharge = invoice.GroupHeader.Charges.AddNew();
			groupCharge.J7_ChargeType = chargeType;
			groupCharge.J7_Amount = 100m;
			groupCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			groupCharge.J7_DistributeBy = distributeBy;

			groupCharge.Validation.ValidateJ7_DistributeBy();
			AssertHasMessageError("Distribute Type must be " + validDistributeBy, groupCharge.J7_DistributeByInfo, "Distribute Type must be " + validDistributeBy);

			groupCharge.J7_DistributeBy = validDistributeBy;
			groupCharge.Validation.ValidateJ7_DistributeBy();
			AssertNoMessageError("Distribute Type must be " + validDistributeBy, groupCharge.J7_DistributeByInfo, "Distribute Type must be " + validDistributeBy);
		}

		public void TestCheckJ7_ChargeType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			AssertHasMessageErrorAllChargesShouldHaveTheSameCurrency(invoice, ImportCustomsChargeTypeList.OverseasFreightChargeTypes);
			AssertHasMessageErrorAllChargesShouldHaveTheSameCurrency(invoice, CustomsChargeTypeList.Codes.OverseasInsurance);
		}

		void AssertHasMessageErrorAllChargesShouldHaveTheSameCurrency(JobComInvoiceHeader invoice, params ZString[] chargeTypes)
		{
			var message = $"There is at least one {string.Join("/", chargeTypes)} charge with a different currency among the Invoices. The system will convert the amount to the local currency before sending it to Customs.";

			invoice.JobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			foreach (var chargeType in chargeTypes)
			{
				var groupCharge = invoice.GroupHeader.Charges.AddNew(chargeType, 50m, Core.Constants.CurrencyCodes.UnitedStates);
				AssertNoMessageError("One Group Charge", groupCharge.J7_ChargeTypeInfo, message);

				groupCharge = invoice.GroupHeader.Charges.AddNew(chargeType, 50m, Core.Constants.CurrencyCodes.EuropeanUnion);
				groupCharge.Validation.ValidateJ7_RX_NKCurrency();
				AssertHasMessageError("Group Charge has different currency", groupCharge.J7_ChargeTypeInfo, message);

				groupCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				AssertNoMessageError(groupCharge.J7_ChargeTypeInfo, message);

				var invoiceCharge = invoice.Charges.AddNew(chargeType, 50m, Core.Constants.CurrencyCodes.EuropeanUnion);
				groupCharge.Validation.ValidateJ7_RX_NKCurrency();
				AssertHasMessageError("Invoice Charge has different currency", groupCharge.J7_ChargeTypeInfo, message);
				invoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				groupCharge.Validation.ValidateJ7_RX_NKCurrency();
				AssertNoMessageError(groupCharge.J7_ChargeTypeInfo, message);

				var invoiceLine = invoice.InvoiceLines.AddNew();
				var lineCharge = invoiceLine.Charges.AddNew(chargeType, 50m, Core.Constants.CurrencyCodes.EuropeanUnion);
				groupCharge.Validation.ValidateJ7_RX_NKCurrency();
				AssertHasMessageError("Line Charge has different currency", groupCharge.J7_ChargeTypeInfo, message);

				lineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				groupCharge.Validation.ValidateJ7_RX_NKCurrency();
				AssertNoMessageError(groupCharge.J7_ChargeTypeInfo, message);
			}

			invoice.JobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var charge = invoice.GroupHeader.Charges.Where(c => chargeTypes.Contains(c.J7_ChargeType)).First();
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			charge.Validation.ValidateJ7_RX_NKCurrency();
			AssertNoMessageError(charge.J7_ChargeTypeInfo, message);
		}
	}
}
