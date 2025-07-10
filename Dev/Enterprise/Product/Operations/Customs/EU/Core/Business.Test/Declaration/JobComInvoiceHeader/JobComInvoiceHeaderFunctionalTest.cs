using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

public class JobComInvoiceHeaderFunctionalTest : Customs.Business.Testing.BaseJobComInvoiceHeaderFunctionalTest
{
	public override void TestCalculateFOB_CIFNonDutiablePreFOB()
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "INV1";
		invoice.JZ_InvoiceAmount = 180848.58m;
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		invoice.JZ_IncoTerm = invoice.IncotermEquivalentToCFRForTesting;

		var oFT = invoice.Charges.AddNew(OverseasFreightCode);
		oFT.J7_Amount = 9280m;

		var nonDutiableFIFT = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight);
		nonDutiableFIFT.J7_Amount = 2755.90m;
		nonDutiableFIFT.J7_IsDutiable = false;
		nonDutiableFIFT.J7_IsGSTApplicable = true;
		nonDutiableFIFT.J7_IsIncludedInITOT = true;

		var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
		invoiceLine.JI_Calc_Invoice = invoice.JZ_InvoiceNumber;
		invoiceLine.JI_LinePrice = 171568.58m;

		var fOBExpected = invoice.JZ_InvoiceAmount - nonDutiableFIFT.J7_Amount;
		declaration.ResumeApportionment();
		AssertEquals("FOB value", fOBExpected, invoice.JZ_Calc_FOBAmount);
		AssertEquals("FOB line", fOBExpected, invoiceLine.JI_Calc_FOB);
	}

	public override void TestILandedCostChargeHolder()
	{
		var dec = Factory.New<JobDeclaration>();
		var invoice = dec.Invoices.AddNew();

		var validCharge = invoice.Charges.AddNew();
		validCharge.J7_ChargeType = OverseasFreightCode;
		validCharge.J7_Amount = 100m;
		validCharge.J7_RX_NKCurrency = dec.LocalCurrencyCode;

		var chargeWithoutDescription = invoice.Charges.AddNew();
		chargeWithoutDescription.J7_ChargeType = "";
		chargeWithoutDescription.J7_Amount = 200m;
		chargeWithoutDescription.J7_RX_NKCurrency = dec.LocalCurrencyCode;

		var chargeWithoutAmount = invoice.Charges.AddNew();
		chargeWithoutAmount.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
		chargeWithoutAmount.J7_Amount = 0m;
		chargeWithoutAmount.J7_RX_NKCurrency = dec.LocalCurrencyCode;

		var chargeWithoutCurrency = invoice.Charges.AddNew();
		chargeWithoutCurrency.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
		chargeWithoutCurrency.J7_Amount = 300m;
		chargeWithoutCurrency.J7_RX_NKCurrency = "";

		var chargeIncludedInLines = invoice.Charges.AddNew();
		chargeIncludedInLines.J7_ChargeType = CustomsChargeTypeList.Codes.PackingCost;
		chargeIncludedInLines.J7_Amount = 300m;
		chargeIncludedInLines.J7_RX_NKCurrency = dec.LocalCurrencyCode;
		chargeIncludedInLines.J7_IsIncludedInITOT = true;
		dec.ResumeApportionment();
		AssertEquals("There should be only one charge", 1, new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)invoice).ChargesToImportForLandedCosting).Count);
		AssertEquals("It should the valid charge", OverseasFreightDesc.ToUpper() + " from Entry", new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)invoice).ChargesToImportForLandedCosting)[0].ChargeDescription);
	}

	#region Implementation

	protected virtual string OverseasFreightCode => ChargeTypeList.Codes.InternationalFreight;
	protected virtual string OverseasFreightDesc => ChargeTypeList.Descriptions.InternationalFreight;

	protected override BaseJobDeclaration GetNewDeclaration()
	{
		var dec = JobDeclaration.New(Factory);
		dec.JE_MessageType = MessageTypeList.Codes.Import;
		return dec;
	}
	#endregion
}
