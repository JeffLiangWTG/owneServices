using System.Linq;
using CargoWise.Types;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class CommoditySpecificationDataProviderTest : BasePassarDataProviderTest<CommoditySpecificationDataProvider>
{
	public void TestConstructorNullArgument()
	{
		AssertNull(CommoditySpecificationDataProvider.New(null));
	}

	public void TestProvider() => CombineAssertions(() =>
	{
		const int borderValue = 123;
		const bool nonTradingGoods = true;
		const bool repair = false;
		const bool goodsReturned = true;

		EntryLine.CL_StatisticalValue = 123.0;

		var invoiceLine = (JobComInvoiceLine)EntryLine.InvoiceLines.FirstOrDefault();
		invoiceLine.JI_NonTradingGoods = nonTradingGoods;
		invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
		invoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.OtherRefunds;
		invoiceLine.InAndOutwardProcessing.Repair = repair;
		invoiceLine.JI_GoodsReturned = goodsReturned;

		AssertEquals("BorderValue", borderValue, DataProvider.BorderValue);
		AssertEquals("InvoiceCurrency", Core.Constants.CurrencyCodes.Switzerland, DataProvider.InvoiceCurrency);
		AssertEquals("NonTradingGoods", nonTradingGoods, DataProvider.NonTradingGoods);
		AssertEquals("CompensationType", 8, DataProvider.CompensationType);
		AssertEquals("Repair", repair, DataProvider.Repair);
		AssertEquals("GooodsReturned", goodsReturned, DataProvider.GoodsReturned);
		invoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.Refund;

		invoiceLine.JI_RefundType = string.Empty;
		AssertNull("CompensationType", DataProvider.CompensationType);

		AssertEquals("EXP - OwnPropulsion not specified", false, DataProvider.OwnPropulsion);
	});

	public void TestUnusedProperties() => CombineAssertions(() =>
	{
		AssertNull("CountryOfOrigin", DataProvider.CountryOfOrigin);
		AssertNull("CountryOfProduction", DataProvider.CountryOfProduction);
		AssertNull("InvoiceValue", DataProvider.InvoiceValue);
		AssertNull("NetAssessment", DataProvider.NetAssessment);
		AssertNull("Preference", DataProvider.Preference);
		AssertNull("AdditionalTaxes", DataProvider.AdditionalTaxes);
		AssertNull("Fees", DataProvider.Fees);
		AssertNull("NetWeightAssessment", DataProvider.NetWeightAssessment);
		AssertNull("TaxInformation", DataProvider.TaxInformation);
	});

	public void TestRestrictionObligation() => CombineAssertions(() =>
	{
		EntryInstruction.CEI_Style = InputControlCodes.Simplified;
		AssertNull("Declaration Type = Simplified", DataProvider.RestrictionObligation);

		EntryInstruction.CEI_Style = InputControlCodes.Ordinary;
		AssertEquals("Declaration Type = Ordinary, RestrictionObligation default value =", false, DataProvider.RestrictionObligation);

		var invoiceLine = (JobComInvoiceLine)EntryLine.InvoiceLines.FirstOrDefault();
		invoiceLine.Restrictions.AddNew();
		ResetDataProvider();
		AssertEquals("Declaration Type = Ordinary, Restrictions not empty", true, DataProvider.RestrictionObligation);

		EntryInstruction.CEI_Style = InputControlCodes.Simplified;
		AssertEquals("Declaration Type = Simplified, Restrictions not empty", true, DataProvider.RestrictionObligation);
	});

	public void TestRestriction() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.Restrictions);
		AssertSame("cached", DataProvider.Restrictions, DataProvider.Restrictions);
	});

	public void TestOwnPropulsion() => CombineAssertions(() =>
	{
		EntryInstruction.CEI_Style = InputControlCodes.Ordinary;
		AssertEquals("EXP - not OwnPropulsion", false, DataProvider.OwnPropulsion);
		ResetDataProvider();

		EntryLine.Declaration.JE_TransportMode = TransportModes.OwnPropulsion;
		AssertEquals("EXP - OwnPropulsion", true, (bool)DataProvider.OwnPropulsion);
		ResetDataProvider();

		EntryInstruction.CEI_Style = InputControlCodes.Simplified;
		AssertNull("EXP - IsSimplified", DataProvider.OwnPropulsion);
	});

	public void TestAdditionalInformation() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.AdditionalInformations);
		AssertSame("cached", DataProvider.AdditionalInformations, DataProvider.AdditionalInformations);

		var invoiceLine = (JobComInvoiceLine)EntryLine.InvoiceLines.FirstOrDefault();
		var tobacco = invoiceLine.Tobaccos.AddNew();

		tobacco.CSI_Code = "1";
		tobacco.CSI_SubType = "2";
		tobacco.CSI_ItemNumber = 3000;
		tobacco.CSI_ReferenceNumber = "4";
		tobacco.CSI_Value = 5000.3m;
		tobacco.CSI_UnitOfQuantity = "KG";
		ResetDataProvider();
		AssertEquals("Tobacco CSI_Code mapped", true, DataProvider.AdditionalInformations.Any(addInfo => addInfo.Text == "1" && addInfo.Code == "A1402"));
		AssertEquals("Tobacco CSI_SubType mapped", true, DataProvider.AdditionalInformations.Any(addInfo => addInfo.Text == "2" && addInfo.Code == "A1403"));
		AssertEquals("Tobacco CSI_ItemNumber mapped", true, DataProvider.AdditionalInformations.Any(addInfo => addInfo.Text == "3000" && addInfo.Code == "A1404"));
		AssertEquals("Tobacco CSI_ReferenceNumber mapped", true, DataProvider.AdditionalInformations.Any(addInfo => addInfo.Text == "4" && addInfo.Code == "A1401"));
		AssertEquals("Tobacco CSI_Value mapped", true, DataProvider.AdditionalInformations.Any(addInfo => addInfo.Text == "5000.3" && addInfo.Code == "A1405"));
		AssertEquals("Tobacco CSI_UnitOfQuantity is mapped kg if KG", true, DataProvider.AdditionalInformations.Any(addInfo => addInfo.Text == "kg" && addInfo.Code == "A1406"));
		AssertEquals("Tobacco CSI_UnitOfQuantity is not mapped Stück if KG", false, DataProvider.AdditionalInformations.Any(addInfo => addInfo.Text == "Stück" && addInfo.Code == "A1406"));

		tobacco.CSI_UnitOfQuantity = "NAR";
		ResetDataProvider();
		AssertEquals("Tobacco CSI_UnitOfQuantity is not mapped kg if NAR", false, DataProvider.AdditionalInformations.Any(addInfo => addInfo.Text == "kg" && addInfo.Code == "A1406"));
		AssertEquals("Tobacco CSI_UnitOfQuantity is mapped Stück if NAR", true, DataProvider.AdditionalInformations.Any(addInfo => addInfo.Text == "Stück" && addInfo.Code == "A1406"));
	});

	public void TestGoodsReturned() => CombineAssertions(() =>
	{
		EntryInstruction.CEI_Style = InputControlCodes.Ordinary;
		AssertEquals("CEI_Style not Simplified", false, DataProvider.GoodsReturned);
		EntryInstruction.CEI_Style = InputControlCodes.Simplified;
		AssertNull("CEI_Style is Simplified", DataProvider.GoodsReturned);
	});

	public void TestCompensationType() => CombineAssertions(() =>
	{
		var invoiceLine = (JobComInvoiceLine)EntryLine.InvoiceLines.FirstOrDefault();
		invoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.OtherRefunds;
		EntryInstruction.CEI_Style = InputControlCodes.Ordinary;
		AssertEquals("CEI_Style not Simplified", 8, DataProvider.CompensationType.Value);
		EntryInstruction.CEI_Style = InputControlCodes.Simplified;
		AssertNull("CEI_Style is Simplified", DataProvider.CompensationType);
	});

	public void TestNonTradingGoods() => CombineAssertions(() =>
	{
		EntryInstruction.CEI_Style = InputControlCodes.Ordinary;
		AssertNotNull("Ordinary", DataProvider.NonTradingGoods);
		ResetDataProvider();
		EntryInstruction.CEI_Style = InputControlCodes.Simplified;
		AssertNull("Simplified", DataProvider.NonTradingGoods);
	});

	public void TestRepair() => CombineAssertions(() =>
	{
		EntryInstruction.CEI_Style = InputControlCodes.Ordinary;
		InvoiceLine.InAndOutwardProcessingRepair = ZBool.True;
		AssertEquals("Ordinary Repair=True", DataProvider.Repair, true);
		ResetDataProvider();
		InvoiceLine.InAndOutwardProcessingRepair = ZBool.False;
		AssertEquals("Ordinary Repair=False", DataProvider.Repair, false);
		ResetDataProvider();

		EntryInstruction.CEI_Style = InputControlCodes.Simplified;
		InvoiceLine.InAndOutwardProcessingRepair = ZBool.True;
		AssertEquals("Simplified Repair=True", DataProvider.Repair, true);
		ResetDataProvider();
		InvoiceLine.InAndOutwardProcessingRepair = ZBool.False;
		AssertNull("Simplified Repair=False", DataProvider.Repair);
	});

	public void TestAdditionalInformationsWithAdditionalInformationsAndVehicles() => CombineAssertions(() =>
	{
		var additionalInfo = InvoiceLine.AdditionalInformations.AddNew();
		additionalInfo.CSI_Code = "321";
		additionalInfo.CSI_Description = "ABC";

		var vehicle = InvoiceLine.Vehicles.AddNew();
		vehicle.CVH_ModelName = "123";
		vehicle.CVH_VehicleIdentificationNumber = "1FMJK2A51DEF50218";
		vehicle.CVH_RegistrationNumber = "123456789";

		var dataProviderAdditionalInfos = DataProvider.AdditionalInformations.ToArray();

		AssertEquals(4, dataProviderAdditionalInfos.Length);
		AssertEquals("AdditionalInfo SequenceNumber", 1, dataProviderAdditionalInfos[0].SequenceNumber);
		AssertEquals("AdditionalInfo Code", additionalInfo.CSI_Code, dataProviderAdditionalInfos[0].Code);
		AssertEquals("AdditionalInfo Text", additionalInfo.CSI_Description, dataProviderAdditionalInfos[0].Text);

		AssertEquals("Vehicle CVH_VehicleIdentificationNumber SequenceNumber", 2, dataProviderAdditionalInfos[1].SequenceNumber);
		AssertEquals("Vehicle CVH_VehicleIdentificationNumber Code", "W1101", dataProviderAdditionalInfos[1].Code);
		AssertEquals("Vehicle CVH_VehicleIdentificationNumber Text", vehicle.CVH_VehicleIdentificationNumber, dataProviderAdditionalInfos[1].Text);

		AssertEquals("Vehicle CVH_RegistrationNumber SequenceNumber", 3, DataProvider.AdditionalInformations.ElementAt(2).SequenceNumber);
		AssertEquals("Vehicle CVH_RegistrationNumber Code", "W1102", dataProviderAdditionalInfos[2].Code);
		AssertEquals("Vehicle CVH_RegistrationNumber Text", vehicle.CVH_RegistrationNumber, dataProviderAdditionalInfos[2].Text);

		AssertEquals("Vehicle CVH_ModelName SequenceNumber", 4, dataProviderAdditionalInfos[3].SequenceNumber);
		AssertEquals("Vehicle CVH_ModelName Code", "W1103", dataProviderAdditionalInfos[3].Code);
		AssertEquals("Vehicle CVH_ModelName Text", vehicle.CVH_ModelName, dataProviderAdditionalInfos[3].Text);
	});

	public void TestAdditionalInformationsWithVehicles() => CombineAssertions(() =>
	{
		var vehicle = InvoiceLine.Vehicles.AddNew();
		vehicle.CVH_ModelName = "123";
		vehicle.CVH_VehicleIdentificationNumber = "1FMJK2A51DEF50218";
		vehicle.CVH_RegistrationNumber = "123456789";

		var dataProviderAdditionalInfos = DataProvider.AdditionalInformations.ToArray();

		AssertEquals(3, dataProviderAdditionalInfos.Length);
		AssertEquals("Vehicle CVH_VehicleIdentificationNumber SequenceNumber", 1, dataProviderAdditionalInfos[0].SequenceNumber);
		AssertEquals("Vehicle CVH_VehicleIdentificationNumber Code", "W1101", dataProviderAdditionalInfos[0].Code);
		AssertEquals("Vehicle CVH_VehicleIdentificationNumber Text", vehicle.CVH_VehicleIdentificationNumber, dataProviderAdditionalInfos[0].Text);

		AssertEquals("Vehicle CVH_RegistrationNumber SequenceNumber", 2, dataProviderAdditionalInfos[1].SequenceNumber);
		AssertEquals("Vehicle CVH_RegistrationNumber Code", "W1102", dataProviderAdditionalInfos[1].Code);
		AssertEquals("Vehicle CVH_RegistrationNumber Text", vehicle.CVH_RegistrationNumber, dataProviderAdditionalInfos[1].Text);

		AssertEquals("Vehicle CVH_ModelName SequenceNumber", 3, dataProviderAdditionalInfos[2].SequenceNumber);
		AssertEquals("Vehicle CVH_ModelName Code", "W1103", dataProviderAdditionalInfos[2].Code);
		AssertEquals("Vehicle CVH_ModelName Text", vehicle.CVH_ModelName, dataProviderAdditionalInfos[2].Text);
	});

	public void TestAdditionalInformationsWithAdditionalInformations() => CombineAssertions(() =>
	{
		var additionalInfo = InvoiceLine.AdditionalInformations.AddNew();
		additionalInfo.CSI_Code = "321";
		additionalInfo.CSI_Description = "ABC";

		var dataProviderAdditionalInfos = DataProvider.AdditionalInformations.ToArray();

		AssertEquals(1, dataProviderAdditionalInfos.Length);
		AssertEquals("AdditionalInfo SequenceNumber", 1, dataProviderAdditionalInfos[0].SequenceNumber);
		AssertEquals("AdditionalInfo Code", additionalInfo.CSI_Code, dataProviderAdditionalInfos[0].Code);
		AssertEquals("AdditionalInfo Text", additionalInfo.CSI_Description, dataProviderAdditionalInfos[0].Text);
	});

	public void TestAdditionalInformationsWithPartialVehicles() => CombineAssertions(() =>
	{
		var vehicle1 = InvoiceLine.Vehicles.AddNew();
		vehicle1.CVH_ModelName = "123";

		var vehicle2 = InvoiceLine.Vehicles.AddNew();
		vehicle2.CVH_RegistrationNumber = "123456789";

		var vehicle3 = InvoiceLine.Vehicles.AddNew();
		vehicle3.CVH_VehicleIdentificationNumber = "1FMJK2A51DEF50218";

		var dataProviderAdditionalInfos = DataProvider.AdditionalInformations.ToArray();

		AssertEquals(3, dataProviderAdditionalInfos.Length);
		AssertEquals("Vehicle 1 CVH_ModelName SequenceNumber", 1, dataProviderAdditionalInfos[0].SequenceNumber);
		AssertEquals("Vehicle 1 CVH_ModelName Code", "W1103", dataProviderAdditionalInfos[0].Code);
		AssertEquals("Vehicle 1 CVH_ModelName Text", vehicle1.CVH_ModelName, dataProviderAdditionalInfos[0].Text);

		AssertEquals("Vehicle 2 CVH_RegistrationNumber SequenceNumber", 2, dataProviderAdditionalInfos[1].SequenceNumber);
		AssertEquals("Vehicle 2 CVH_RegistrationNumber Code", "W1102", dataProviderAdditionalInfos[1].Code);
		AssertEquals("Vehicle 2 CVH_RegistrationNumber Text", vehicle2.CVH_RegistrationNumber, dataProviderAdditionalInfos[1].Text);

		AssertEquals("Vehicle 3 CVH_VehicleIdentificationNumber SequenceNumber", 3, dataProviderAdditionalInfos[2].SequenceNumber);
		AssertEquals("Vehicle 3 CVH_VehicleIdentificationNumber Code", "W1101", dataProviderAdditionalInfos[2].Code);
		AssertEquals("Vehicle 3 CVH_VehicleIdentificationNumber Text", vehicle3.CVH_VehicleIdentificationNumber, dataProviderAdditionalInfos[2].Text);
	});

	protected override CommoditySpecificationDataProvider CreateDataProvider() => CommoditySpecificationDataProvider.New(EntryLine);
}
