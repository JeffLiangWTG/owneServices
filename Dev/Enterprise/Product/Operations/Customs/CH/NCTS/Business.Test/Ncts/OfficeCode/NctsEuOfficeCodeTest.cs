using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsEuOfficeCode))]
sealed class NctsEuOfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<NctsEuOfficeCode>, Integration.Customs.CH.INctsEuOfficeCode
{
	public void TestMovementHeader() => AssertType<NctsDepartureMovementHeader>(CustomsOffice.MovementHeader);

	public void TestEstimatedNumberOfDays() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(CustomsOffice.EstimatedNumberOfDaysInfo, caption: "Estimated Days", shortCaption: "Est. Days");
		AssertEquals("Max Length", 2, CustomsOffice.EstimatedNumberOfDaysInfo.MaxLength);
		AssertEquals("Initial value", ZString.Empty, CustomsOffice.EstimatedNumberOfDays);

		CustomsOffice.EstimatedNumberOfDays = "34";
		Factory.Save();
		AssertEquals("Property persisted", "34", new BusinessObjectFactory().Load<NctsEuOfficeCode>(CustomsOffice.PK).EstimatedNumberOfDays);
	});

	public void TestIsEstimatedNumberOfDaysReadOnly() => CombineAssertions(() =>
	{
		var writableValues = new[] { OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit };
		var availableValues = CustomsOffice.Lookups.CY_CodeList.GetAllCodes();

		foreach (var value in availableValues)
		{
			CustomsOffice.CY_Code = value;
			AssertEquals($"Office code'{value}'", writableValues.Contains(value), !CustomsOffice.IsEstimatedNumberOfDaysReadOnly);
		}
	});

	public void TestEstimatedNumberOfDaysResetWhenReadOnly()
	{
		CustomsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
		CustomsOffice.EstimatedNumberOfDays = "5";
		CustomsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfLocation;
		AssertEquals($"Reset of EstimatedNumberOfDays when disabled", ZString.Empty, CustomsOffice.EstimatedNumberOfDays);
	}

	public void TestCY_Order_ReadOnly() => CombineAssertions(() =>
	{
		CustomsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
		AssertEquals("DEP: Seq. readOnly ", true, CustomsOffice.CY_OrderInfo.ReadOnly);

		CustomsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
		AssertEquals("DEP: Seq. readOnly ", true, CustomsOffice.CY_OrderInfo.ReadOnly);

		CustomsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
		AssertEquals("DEP: Seq. editable ", false, CustomsOffice.CY_OrderInfo.ReadOnly);

		CustomsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
		AssertEquals("DEP: Seq. readOnly ", false, CustomsOffice.CY_OrderInfo.ReadOnly);
	});

	public void TestCY_Order_RecalculateWhenRenumbered() => CombineAssertions(() =>
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var movementHeader = header.MovementHeader;
		var customsOffice1 = movementHeader.CustomsOfficesForDeparture.AddNew();
		var customsOffice2 = movementHeader.CustomsOfficesForDeparture.AddNew();
		var customsOffice3 = movementHeader.CustomsOfficesForDeparture.AddNew();
		var customsOffice4 = movementHeader.CustomsOfficesForDeparture.AddNew();
		var customsOffice5 = movementHeader.CustomsOfficesForDeparture.AddNew();

		customsOffice1.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
		customsOffice1.CY_Data = "CO1 DEP";

		customsOffice2.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
		customsOffice2.CY_Data = "CO2 DES";

		customsOffice3.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
		customsOffice3.CY_Data = "CO3 TRA";

		customsOffice4.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
		customsOffice4.CY_Data = "CO4 TRA";

		customsOffice5.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
		customsOffice5.CY_Data = "CO5 TRA";

		AssertEquals("CO1 DEP Seq 1", (short)1, customsOffice1.CY_Order);
		AssertEquals("CO2 DES Seq 1", (short)1, customsOffice2.CY_Order);
		AssertEquals("CO3 TRA Seq 1", (short)1, customsOffice3.CY_Order);
		AssertEquals("CO4 TRA Seq 2", (short)2, customsOffice4.CY_Order);
		AssertEquals("CO5 TRA Seq 3", (short)3, customsOffice5.CY_Order);

		customsOffice5.CY_Order = 2;

		AssertEquals("CO1 DEP Seq 1", (short)1, customsOffice1.CY_Order);
		AssertEquals("CO2 DES Seq 1", (short)1, customsOffice2.CY_Order);
		AssertEquals("CO3 TRA Seq 1", (short)1, customsOffice3.CY_Order);
		AssertEquals("CO5 TRA Seq 2", (short)2, customsOffice5.CY_Order);
		AssertEquals("CO4 TRA Seq 3", (short)3, customsOffice4.CY_Order);
	});

	protected override BusinessObject GetNewBusinessObject() => CreateBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateBusinessObject(factory);

	NctsEuOfficeCode CreateBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var customsOffice = header.MovementHeader.CustomsOfficesForDeparture.AddNew();
		return customsOffice;
	}

	protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new LightValidationTesterNctsEuOfficeCode(bizObjToTest);

	NctsEuOfficeCode CustomsOffice => customsOffice ??= GetNewBusinessObject() as NctsEuOfficeCode;
	NctsEuOfficeCode customsOffice;
}
