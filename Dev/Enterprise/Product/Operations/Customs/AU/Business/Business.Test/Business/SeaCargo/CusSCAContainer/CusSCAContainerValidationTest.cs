using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CusSCAContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestContainerSealNumberValidation()
		{
			CusSCAContainer container = Factory.New<CusSCAContainer>();
			container.Validation.ValidateCN_SealNumber();
			Assert("PreCondition, Container has errors as the Seal Number is Mandatory", container.CN_SealNumberInfo.HasWarnings());
			container.CN_SealNumber = "1234567890";
			Assert("PreCondition, Container has no errors", !container.CN_SealNumberInfo.HasWarnings());
			container.CN_SealNumber = "123456$890";
			Assert("Container Seal Number contains invalid characters and they should be stripped", !container.CN_SealNumberInfo.HasMessageErrors());
		}

		public void TestContainerTypeValidation()
		{
			CusSCAContainer container = Factory.New<CusSCAContainer>();
			container.Validation.ValidateCN_RC_NKContainerType();
			Assert("PreCondition, Container has errors as the ISO Type is Mandatory", container.CN_RC_NKContainerTypeInfo.HasMessageErrors() && !container.CN_RC_NKContainerTypeInfo.HasErrors());

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, FortyFootGeneralPurpose);
			refContainer.RC_ISOType = "4000";

			container.CN_RC_NKContainerType = FortyFootGeneralPurpose;
			Assert("Container ISO Type should have no errors", !container.CN_RC_NKContainerTypeInfo.HasMessageErrors());
			container.CN_RC_NKContainerType = "1234";
			Assert("Container ISO Type should have errors as the container type is not in the list", container.CN_RC_NKContainerTypeInfo.HasMessageErrors());
		}

		public void TestContainerValidation()
		{
			CusSCAContainer container = Factory.New<CusSCAContainer>();
			container.Validation.ValidateCN_ContainerNumber();
			Assert("PreCondition, Container has errors as the Number is Mandatory", container.CN_ContainerNumberInfo.HasMessageErrors());
			container.CN_ContainerNumber = "12345678905";
			Assert("Container Number is not in the correct format", container.CN_ContainerNumberInfo.HasWarnings());
			container.CN_ContainerNumber = "12345";
			Assert("Container Number is not the correct length", container.CN_ContainerNumberInfo.HasWarnings());
			container.CN_ContainerNumber = "MLCU2765224";
			Assert("Container Number has no errors as it is correct", !container.CN_ContainerNumberInfo.HasWarnings());
			container.CN_ContainerNumber = "MLCU_234234";
			Assert("Container Number contains invalid characters", !container.CN_ContainerNumberInfo.HasMessageErrors());
		}

		public void TestCFSOrgValidation()
		{
			var cfsOrg = CreateOrganisation("ORG111", "AUSYD");
			var address11 = cfsOrg.Addresses.AddNew();
			address11.AddressCapability.SetCapabilityEnabled("PAD");
			address11.OA_Code = "Test PAD 11";
			address11.OA_IsActive = false;
			var address12 = cfsOrg.Addresses.AddNew();
			address12.AddressCapability.SetCapabilityEnabled("PAD");
			address12.OA_Code = "Test PAD 12";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_UnpackDepotAddress = address11.PK;

			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var testContainer = oceanBill.Containers.AddNew();
			testContainer.CN_OA_UnderbondToOrg = cfsOrg.PK;
			testContainer.CN_OA_UnderbondToCode = address11.OA_Code;
			AssertEquals("Address 11", address11.PK, testContainer.CN_OA_UnderbondTo);
			testContainer.CN_ContainerNumber = "MLCU2765224";
			AssertHasWarning(testContainer.CN_ContainerNumberInfo, CusSCAContainerValidation.ArrivalCFSAddressIsInactive);

			consol.JK_OA_UnpackDepotAddress = address12.PK;
			testContainer.Validation.ValidateCN_ContainerNumber();
			AssertNoWarning(testContainer.CN_ContainerNumberInfo, CusSCAContainerValidation.ArrivalCFSAddressIsInactive);
		}

		const string FortyFootGeneralPurpose = "40GP";

		OrgHeader CreateOrganisation(ZString name, ZString uNLOCO)
		{
			var result = Factory.New<OrgHeader>();
			result.OH_FullName = name;
			result.OH_RL_NKClosestPort = uNLOCO;
			result.MainAddress.OA_Address1 = new ZString("Address " + name).SubstringSafe(0, 50);
			return result;
		}

		#region Old CMRCusSCAContainerValidation test - adjusted

		public void TestContainerTypeValidationNotMandatoryIfOverrideSpecified()
		{
			container.CN_TypeOfContainer = "ZZ";
			container.CN_ContainerSizeOrISOCode = "AA";
			AssertNoMessageErrors("Not mandatory if type and size manually specified", container.CN_RC_NKContainerTypeInfo);

			container.CN_ContainerSizeOrISOCode = "";
			AssertHasMessageErrors("Mandatory if either type or size not specified", container.CN_RC_NKContainerTypeInfo);

			container.CN_TypeOfContainer = "";
			container.CN_ContainerSizeOrISOCode = "DD";
			AssertHasMessageErrors("Mandatory if either type or size not specified", container.CN_RC_NKContainerTypeInfo);

			container.CN_RC_NKContainerType = "20GP";
			AssertNoMessageErrors("Specified so no errors", container.CN_RC_NKContainerTypeInfo);
		}

		public void TestValidateContainerNumberEmptyWhenBulkOrBreakBulk()
		{
			container.CN_ContainerNumber = ZString.Empty;

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertHasMessageErrors("should have a message error when empty", container.CN_ContainerNumberInfo);
		}

		public void TestValidateTypeOfContainer()
		{
			container.Validation.ValidateCN_TypeOfContainer();
			AssertHasMessageErrors("by default", container.CN_TypeOfContainerInfo);

			container.CN_TypeOfContainer = CMRContainerTypes.Codes.RefrigeratedAContainerWhichIsUsedToTransportRefrigeratedCargo;
			AssertNoNotifications("when set", container.CN_TypeOfContainerInfo);

			container.CN_TypeOfContainer = "BLA";
			AssertHasMessageErrors("when invalid", container.CN_TypeOfContainerInfo);

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.BreakBulk;
			container.RunPreSaveValidation();
			AssertNoMessageErrors("BBK No Errors", container.CN_TypeOfContainerInfo);
			container.CN_ContainerMode = CMRImportCargoTypes.Codes.Bulk;
			container.RunPreSaveValidation();
			AssertNoMessageErrors("BULK No Errors", container.CN_TypeOfContainerInfo);
		}

		public void TestValidateContainerSizeOrISOCode()
		{
			container.Validation.ValidateCN_ContainerSizeOrISOCode();
			AssertHasMessageErrors("by default", container.CN_ContainerSizeOrISOCodeInfo);

			container.CN_ContainerSizeOrISOCode = CMRContainerSizes.Codes._20X8X8;
			AssertNoNotifications("when set", container.CN_ContainerSizeOrISOCodeInfo);

			container.CN_ContainerSizeOrISOCode = "4321";
			AssertHasMessageErrors("when invalid", container.CN_ContainerSizeOrISOCodeInfo);

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.BreakBulk;
			container.RunPreSaveValidation();
			AssertNoMessageErrors("BBK should not display errors", container.CN_ContainerSizeOrISOCodeInfo);

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.Bulk;
			container.RunPreSaveValidation();
			AssertNoMessageErrors("Bulk should not display errors", container.CN_ContainerSizeOrISOCodeInfo);
		}

		public void TestOnlyOneBulkContainer()
		{
			container.CN_ContainerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			container2.CN_ContainerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertNoNotifications("on d1 when fcl", container.CN_ContainerModeInfo);
			AssertNoNotifications("on d2 when fcl", container2.CN_ContainerModeInfo);

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.Bulk;
			AssertNoNotifications("on d1 when d1 bulk", container.CN_ContainerModeInfo);
			AssertNoNotifications("on d2 when d1 bulk", container2.CN_ContainerModeInfo);

			container2.CN_ContainerMode = CMRImportCargoTypes.Codes.Bulk;
			container.Validation.ValidateCN_ContainerMode();
			AssertHasErrors("on d1 when d1 & d2 bulk", container.CN_ContainerModeInfo);
			AssertHasErrors("on d2 when d1 & d2 bulk", container2.CN_ContainerModeInfo);

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			container2.Validation.ValidateCN_ContainerMode();
			AssertNoNotifications("on d1 when d1 fcl", container.CN_ContainerModeInfo);
			AssertNoNotifications("on d2 when d1 fcl", container2.CN_ContainerModeInfo);
		}

		public void TestOnlyOneBreakBulkContainer()
		{
			container.CN_ContainerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			container2.CN_ContainerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertNoNotifications("on d1 when fcl", container.CN_ContainerModeInfo);
			AssertNoNotifications("on d2 when fcl", container2.CN_ContainerModeInfo);

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.BreakBulk;
			AssertNoNotifications("on d1 when d1 breakbulk", container.CN_ContainerModeInfo);
			AssertNoNotifications("on d2 when d1 breakbulk", container2.CN_ContainerModeInfo);

			container2.CN_ContainerMode = CMRImportCargoTypes.Codes.BreakBulk;
			container.Validation.ValidateCN_ContainerMode();
			AssertHasErrors("on d1 when d1 & d2 breakbulk", container.CN_ContainerModeInfo);
			AssertHasErrors("on d2 when d1 & d2 breakbulk", container2.CN_ContainerModeInfo);

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			container2.Validation.ValidateCN_ContainerMode();
			AssertNoNotifications("on d1 when d1 fcl", container.CN_ContainerModeInfo);
			AssertNoNotifications("on d2 when d1 fcl", container2.CN_ContainerModeInfo);
		}

		public void TestUniquenessByContainerNumber()
		{
			container.CN_ContainerNumber = "ABCD1234560";
			container2.CN_ContainerNumber = "DCBA6543219";

			AssertNoNotifications(container.CN_ContainerNumberInfo);
			AssertNoNotifications(container2.CN_ContainerNumberInfo);

			container2.CN_ContainerNumber = "ABCD1234560";
			container.Validation.ValidateCN_ContainerNumber();
			AssertHasErrors(container.CN_ContainerNumberInfo);
			AssertHasErrors(container2.CN_ContainerNumberInfo);

			container.CN_ContainerNumber = "DCBA6543219";
			container2.Validation.ValidateCN_ContainerNumber();
			AssertNoNotifications(container.CN_ContainerNumberInfo);
			AssertNoNotifications(container2.CN_ContainerNumberInfo);
		}

		#region Implementation

		CusSCAOceanBill oceanBill;
		CusSCAContainer container;
		CusSCAContainer container2;
		protected override void SetUp()
		{
			base.SetUp();
			oceanBill = Factory.New<CusSCAOceanBill>();
			container = oceanBill.Containers.AddNew();
			container2 = oceanBill.Containers.AddNew();
		}

		#endregion

		#endregion
	}
}
