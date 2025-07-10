using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IMHeaderWarehouseIdentificationWrapperTest : TestCaseWithFactory
{
	public void TestType()
	{
		AssertEquals(ZString.Empty, warehouseIdentificationWrapper.Type);
		cusCode.OK_CustomsRegNo = "A123456NIT";
		entryInstruction.CEI_OA_Warehouse2 = warehouse.PK;
		AssertEquals("A", warehouseIdentificationWrapper.Type);
	}

	public void TestIdentification()
	{
		AssertEquals(ZString.Empty, warehouseIdentificationWrapper.Identification);
		cusCode.OK_CustomsRegNo = "A123456NIT";
		entryInstruction.CEI_OA_Warehouse2 = warehouse.PK;
		AssertEquals("123456", warehouseIdentificationWrapper.Identification);
	}

	public void TestCinIdentification()
	{
		AssertEquals(ZString.Empty, warehouseIdentificationWrapper.CinIdentification);
		cusCode.OK_CustomsRegNo = "A123456NIT";
		entryInstruction.CEI_OA_Warehouse2 = warehouse.PK;
		AssertEquals("N", warehouseIdentificationWrapper.CinIdentification);
	}

	public void TestAuthorizingCountry()
	{
		AssertEquals(ZString.Empty, warehouseIdentificationWrapper.AuthorizingCountry);
		cusCode.OK_CustomsRegNo = "A123456NIT";
		entryInstruction.CEI_OA_Warehouse2 = warehouse.PK;
		AssertEquals("IT", warehouseIdentificationWrapper.AuthorizingCountry);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderWarehouseIdentificationWrapper(null));
		AssertNoExceptionThrown(() => new IMHeaderWarehouseIdentificationWrapper(entryInstruction));
	}

	protected override void SetUp()
	{
		base.SetUp();

		var jobDeclaration = Factory.New<JobDeclaration>();

		var itCompany = Factory.New<GlbCompany>();
		itCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		var itBranch = itCompany.Branches.AddNew();
		jobDeclaration.JE_GB = itBranch.PK;
		entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		var orgHeader = Factory.New<OrgHeader>();
		warehouse = orgHeader.Addresses.AddNew();
		cusCode = warehouse.CustomsCodes.AddNew();
		cusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
		cusCode.OK_RN_NKCodeCountry = "IT";
		warehouseIdentificationWrapper = new IMHeaderWarehouseIdentificationWrapper(entryInstruction);
	}

	CusEntryInstruction entryInstruction;
	IMHeaderWarehouseIdentificationWrapper warehouseIdentificationWrapper;
	OrgAddress warehouse;
	OrgCusCode cusCode;
}
