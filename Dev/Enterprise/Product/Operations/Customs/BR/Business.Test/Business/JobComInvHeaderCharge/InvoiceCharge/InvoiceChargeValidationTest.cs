using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class InvoiceChargeValidationTest : Customs.Business.Testing.InvoiceChargeValidationTest
	{
		public void TestCheckJ7_DistributeBy_Apportionment()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 100m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Weight = 10m;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.JI_Volume = 20m;
			invoiceLine1.JI_VolumeUQ = "m3";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Volume = 20m;
			invoiceLine2.JI_VolumeUQ = "m3";
			var charge = invoice.Charges.AddNew();
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.NetWeight;
			AssertHasMessageErrors("There are invoice lines that don't have a net weight. The apportionment of this charge won't be correct for the invoice lines.", charge.J7_DistributeByInfo);
			invoiceLine1.JI_InvoiceQuantity = 1.2m;
			invoiceLine1.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
			invoiceLine1.JI_NetWeight = 20m;
			invoiceLine1.JI_NetWeightUQ = Core.Constants.Weight.Tonnes;
			invoiceLine2.JI_InvoiceQuantity = 10m;
			invoiceLine2.JI_InvoiceUQ = Core.Constants.Weight.Decitons;
			invoiceLine2.JI_NetWeight = 10m;
			invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Milligrams;
			charge.Validation.ValidateJ7_DistributeBy();
			AssertNoMessageErrors("There are invoice lines that don't have a net weight. The apportionment of this charge won't be correct for the invoice lines.", charge.J7_DistributeByInfo);

			charge.J7_DistributeBy = ChargeDistributeByList.Codes.FOB;
			charge.Validation.ValidateJ7_DistributeBy();
			AssertHasMessageErrors("There are invoice lines that don't have a fob value. The apportionment of this charge won't be correct for the invoice lines.", charge.J7_DistributeByInfo);

			invoiceLine1.JI_LinePrice = 50m;
			invoiceLine2.JI_LinePrice = 50m;
			charge.Validation.ValidateJ7_DistributeBy();
			AssertNoMessageErrors("There are invoice lines that don't have a fob value. The apportionment of this charge won't be correct for the invoice lines.", charge.J7_DistributeByInfo);
		}

		public void TestCheckJ7_DistributeBy_List()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceOFT = invoice.Charges.AddNew();
			invoiceOFT.J7_DistributeBy = "XXX";
			AssertHasMessageError(invoiceOFT.J7_DistributeByInfo, ListValidation.InvalidCodeMessageError);
			invoiceOFT.J7_DistributeBy = ChargeDistributeByList.Codes.NetWeight;
			AssertNoMessageError(invoiceOFT.J7_DistributeByInfo, ListValidation.InvalidCodeMessageError);
			invoiceOFT.J7_DistributeBy = ChargeDistributeByList.Codes.FOB;
			AssertNoMessageError(invoiceOFT.J7_DistributeByInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJ7_DistributeBy()
		{
			#region Import Siscomex

			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportSiscomex, ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, ChargeDistributeByList.Codes.FOB, ChargeDistributeByList.Codes.NetWeight);
			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportSiscomex, ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid, ChargeDistributeByList.Codes.FOB, ChargeDistributeByList.Codes.NetWeight);
			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportSiscomex, ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory, ChargeDistributeByList.Codes.FOB, ChargeDistributeByList.Codes.NetWeight);

			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportSiscomex, CustomsChargeTypeList.Codes.OverseasInsurance, ChargeDistributeByList.Codes.NetWeight, ChargeDistributeByList.Codes.FOB);

			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportSiscomex, ImportCustomsChargeTypeList.Codes.PackingCosts, ChargeDistributeByList.Codes.NetWeight, ChargeDistributeByList.Codes.FOB);
			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.ImportSiscomex, ImportCustomsChargeTypeList.Codes.OtherDeductionsCustomsValue, ChargeDistributeByList.Codes.NetWeight, ChargeDistributeByList.Codes.FOB);

			#endregion

			#region Import

			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.Import, ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, ChargeDistributeByList.Codes.FOB, ChargeDistributeByList.Codes.NetWeight);
			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.Import, ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid, ChargeDistributeByList.Codes.FOB, ChargeDistributeByList.Codes.NetWeight);
			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.Import, ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory, ChargeDistributeByList.Codes.FOB, ChargeDistributeByList.Codes.NetWeight);

			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.Import, CustomsChargeTypeList.Codes.OverseasInsurance, ChargeDistributeByList.Codes.NetWeight, ChargeDistributeByList.Codes.FOB);

			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.Import, ImportCustomsChargeTypeList.Codes.PackingCosts, ChargeDistributeByList.Codes.NetWeight, ChargeDistributeByList.Codes.FOB);
			AssertMessageErrorDistributeBy(BRJobMessageTypeList.Codes.Import, ImportCustomsChargeTypeList.Codes.OtherDeductionsCustomsValue, ChargeDistributeByList.Codes.NetWeight, ChargeDistributeByList.Codes.FOB);

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

			var invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = chargeType;
			invoiceCharge.J7_Amount = 100m;
			invoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceCharge.J7_DistributeBy = distributeBy;

			invoiceCharge.Validation.ValidateJ7_DistributeBy();
			AssertHasMessageError("Distribute Type must be " + validDistributeBy, invoiceCharge.J7_DistributeByInfo, "Distribute Type must be " + validDistributeBy);

			invoiceCharge.J7_DistributeBy = validDistributeBy;
			invoiceCharge.Validation.ValidateJ7_DistributeBy();
			AssertNoMessageError("Distribute Type must be " + validDistributeBy, invoiceCharge.J7_DistributeByInfo, "Distribute Type must be " + validDistributeBy);
		}

		public void TestCheckJ7_ChargeType()
		{
			#region Import Siscomex

			AssertHasMessageErrorAllChargesShouldHaveTheSameCurrency(BRJobMessageTypeList.Codes.ImportSiscomex, ImportCustomsChargeTypeList.OverseasFreightChargeTypes);
			AssertHasMessageErrorAllChargesShouldHaveTheSameCurrency(BRJobMessageTypeList.Codes.ImportSiscomex, CustomsChargeTypeList.Codes.OverseasInsurance);

			#endregion

			#region Import

			AssertHasMessageErrorAllChargesShouldHaveTheSameCurrency(BRJobMessageTypeList.Codes.Import, ImportCustomsChargeTypeList.OverseasFreightChargeTypes);
			AssertHasMessageErrorAllChargesShouldHaveTheSameCurrency(BRJobMessageTypeList.Codes.Import, CustomsChargeTypeList.Codes.OverseasInsurance);

			#endregion
		}

		void AssertHasMessageErrorAllChargesShouldHaveTheSameCurrency(ZString messageType, params ZString[] chargeTypes)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;

			var invoice = declaration.Invoices.AddNew();

			var message = $"There is at least one {string.Join("/", chargeTypes)} charge with a different currency among the Invoices. The system will convert the amount to the local currency before sending it to Customs.";

			foreach (var chargeType in chargeTypes)
			{
				var invoiceCharge = invoice.Charges.AddNew(chargeType, 50m, Core.Constants.CurrencyCodes.UnitedStates);
				invoiceCharge.Validation.ValidateJ7_RX_NKCurrency();
				invoiceCharge.Validation.ValidateJ7_ChargeType();
				AssertNoMessageError("One Invoice Charge", invoiceCharge.J7_ChargeTypeInfo, message);

				var groupCharge = invoice.GroupHeader.Charges.AddNew(chargeType, 50m, Core.Constants.CurrencyCodes.EuropeanUnion);
				invoiceCharge.Validation.ValidateJ7_RX_NKCurrency();
				AssertHasMessageError("Group Charge has different currency", invoiceCharge.J7_ChargeTypeInfo, message);

				groupCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				invoiceCharge.Validation.ValidateJ7_RX_NKCurrency();
				AssertNoMessageError(invoiceCharge.J7_ChargeTypeInfo, message);

				invoiceCharge = invoice.Charges.AddNew(chargeType, 50m, Core.Constants.CurrencyCodes.EuropeanUnion);
				AssertHasMessageErrors("Invoice Charge has different currency", invoiceCharge.J7_ChargeTypeInfo);
				invoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				AssertNoMessageError(invoiceCharge.J7_ChargeTypeInfo, message);

				var invoiceLine = invoice.InvoiceLines.AddNew();
				var lineCharge = invoiceLine.Charges.AddNew(chargeType, 50m, Core.Constants.CurrencyCodes.EuropeanUnion);
				invoiceCharge.Validation.ValidateJ7_RX_NKCurrency();
				AssertHasMessageError("Line Charge has different currency", invoiceCharge.J7_ChargeTypeInfo, message);

				lineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				invoiceCharge.Validation.ValidateJ7_RX_NKCurrency();
				AssertNoMessageError(invoiceCharge.J7_ChargeTypeInfo, message);
			}

			invoice.JobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var charge = invoice.Charges.Where(c => chargeTypes.Contains(c.J7_ChargeType)).First();
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			charge.Validation.ValidateJ7_RX_NKCurrency();
			AssertNoMessageError(charge.J7_ChargeTypeInfo, message);
		}
	}
}
