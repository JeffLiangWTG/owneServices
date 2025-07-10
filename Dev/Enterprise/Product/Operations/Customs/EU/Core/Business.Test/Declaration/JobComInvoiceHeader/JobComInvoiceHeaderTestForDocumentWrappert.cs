using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

public class JobComInvoiceHeaderTestForDocumentWrappert : BaseJobComInvoiceHeaderTestForDocumentWrapper
{
	public override void TestCheckingValueOfJZ_Calc_ConversionFactor()
	{
		var uSDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Constants.CurrencyCodes.UnitedStates);
		SetExchangeRate(ZDateTime.Now, ZDateTime.Now.AddYears(10), 0.5m, uSDCurrency, "CUS");

		invoice.JZ_InvoiceAmount = 1000m;
		invoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
		invoice.JZ_IncoTerm = "CIF";

		invoice.Charges.RemoveAll();
		var oFT = invoice.Charges.AddNew(OverseasFreightCode, 50m);
		oFT.J7_IsIncludedInITOT = true;
		invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 5m).J7_IsIncludedInITOT = true;
		AssertEquals(2m, Enterprise.ZArchitecture.Core.Utilities.Round(invoice.JZ_Calc_ConversionFactor, 4));
	}

	public override void TestDutiableChargesNotIncludedInLinesWorksCorrectly()
	{
		invoice.Charges.RemoveAll();
		var charge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
		charge.J7_IsDutiable = true;
		charge.J7_IsIncludedInITOT = false;

		var overseasFreightCharge = invoice.Charges.AddNew(OverseasFreightCode, 200m, invoice.JobDeclaration.LocalCurrencyCode);
		var overseasInsuranceCharge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 350m, invoice.JobDeclaration.LocalCurrencyCode);

		AssertEquals(650m, invoice.DutiableChargesNotIncludedInLines.Amount);
	}

	public override void TestDutiableChargesNotIncludedInLinesInLocalCurrencyWorksCorrectly()
	{
		invoice.Charges.RemoveAll();
		var charge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
		charge.J7_IsDutiable = true;
		charge.J7_IsIncludedInITOT = false;

		var overseasFreightCharge = invoice.Charges.AddNew(OverseasFreightCode, 200m, invoice.JobDeclaration.LocalCurrencyCode);
		var overseasInsuranceCharge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 350m, invoice.JobDeclaration.LocalCurrencyCode);

		AssertEquals(650m, invoice.DutiableChargesNotIncludedInLinesInLocalCurrency.Amount);
	}

	public override void TestNonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceWorksCorrectly()
	{
		invoice.Charges.RemoveAll();
		var charge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
		charge.J7_IsIncludedInITOT = false;
		charge.J7_IsDutiable = false;

		var overseasFreightCharge = invoice.Charges.AddNew(OverseasFreightCode, 200m, invoice.JobDeclaration.LocalCurrencyCode);
		var overseasInsuranceCharge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 350m, invoice.JobDeclaration.LocalCurrencyCode);

		AssertEquals(-450m, invoice.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance.Amount);
	}

	public override void TestNonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInLocalCurrencyWorksCorrectly()
	{
		invoice.Charges.RemoveAll();
		var charge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
		charge.J7_IsIncludedInITOT = false;
		charge.J7_IsDutiable = false;

		var overseasFreightCharge = invoice.Charges.AddNew(OverseasFreightCode, 200m, invoice.JobDeclaration.LocalCurrencyCode);
		var overseasInsuranceCharge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 350m, invoice.JobDeclaration.LocalCurrencyCode);

		AssertEquals(-450m, invoice.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInLocalCurrency.Amount);
	}

	public override void TestNonDutiableChargesNotIncludedInLinesWorksCorrectly()
	{
		invoice.Charges.RemoveAll();
		var charge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
		charge.J7_IsIncludedInITOT = false;
		charge.J7_IsDutiable = false;

		var overseasFreightCharge = invoice.Charges.AddNew(OverseasFreightCode, 200m, invoice.JobDeclaration.LocalCurrencyCode);
		var overseasInsuranceCharge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 350m, invoice.JobDeclaration.LocalCurrencyCode);

		AssertEquals(100m, invoice.NonDutiableChargesNotIncludedInLines.Amount);
	}

	public override void TestNonDutiableChargesNotIncludedInLinesInLocalCurrencyWorksCorrectly()
	{
		invoice.Charges.RemoveAll();
		var charge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
		charge.J7_IsIncludedInITOT = false;
		charge.J7_IsDutiable = false;

		var overseasFreightCharge = invoice.Charges.AddNew(OverseasFreightCode, 200m, invoice.JobDeclaration.LocalCurrencyCode);
		var overseasInsuranceCharge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 350m, invoice.JobDeclaration.LocalCurrencyCode);

		AssertEquals(100m, invoice.NonDutiableChargesNotIncludedInLinesInLocalCurrency.Amount);
	}

	#region Implementation

	protected virtual string OverseasFreightCode => ChargeTypeList.Codes.InternationalFreight;

	protected override BaseJobDeclaration GetNewDeclaration()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		return dec;
	}
	#endregion
}
