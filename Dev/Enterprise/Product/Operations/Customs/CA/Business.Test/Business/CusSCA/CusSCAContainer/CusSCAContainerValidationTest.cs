using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCAContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCN_ContainerModeValidation()
		{
			container.CN_ContainerMode = ZString.Empty;
			container.Validation.ValidateCN_ContainerMode();
			AssertHasMessageErrorContaining(container.CN_ContainerModeInfo, MandatoryValidation.YouHaveNotEntered);
			container.CN_ContainerMode = "AIR";
			AssertHasMessageErrorContaining(container.CN_ContainerModeInfo, "Please enter a Containerized mode (CNT or EMP).");
			container.CN_ContainerMode = "NCT";
			AssertHasMessageErrorContaining(container.CN_ContainerModeInfo, "Please enter a Containerized mode (CNT or EMP).");
			container.CN_ContainerMode = "CNT";
			AssertNoMessageErrors(container.CN_ContainerModeInfo);
			container.CN_ContainerMode = "EMP";
			AssertNoMessageErrors(container.CN_ContainerModeInfo);
			container.CN_TypeOfContainer = CusSCAHouse.NonContaineriseID;
			container.CN_ContainerMode = "CNT";
			AssertHasMessageErrorContaining(container.CN_ContainerModeInfo, "Please enter a Non-containerized mode (AIR or NCT).");
			container.CN_ContainerMode = "EMP";
			AssertHasMessageErrorContaining(container.CN_ContainerModeInfo, "Please enter a Non-containerized mode (AIR or NCT).");
			container.CN_ContainerMode = "AIR";
			AssertNoMessageErrors(container.CN_ContainerModeInfo);
			container.CN_ContainerMode = "NCT";
			AssertNoMessageErrors(container.CN_ContainerModeInfo);
		}

		public void TestCN_ContainerTypeValidation()
		{
			container.CN_RC_NKContainerType = ZString.Empty;
			container.Validation.ValidateCN_RC_NKContainerType();
			AssertHasMessageErrorContaining(container.CN_RC_NKContainerTypeInfo, MandatoryValidation.YouHaveNotEntered);
			container.CN_RC_NKContainerType = "XXXX";
			AssertHasMessageErrorContaining(container.CN_RC_NKContainerTypeInfo, ListValidation.InvalidCodeMessageError);
			container.CN_RC_NKContainerType = "20GP";
			AssertNoMessageErrors(container.CN_RC_NKContainerTypeInfo);
		}

		public void TestCN_RN_NKCountryOfRegistrationValidation()
		{
			container.CN_RN_NKCountryOfRegistration = ZString.Empty;
			container.Validation.ValidateCN_RC_NKContainerType();
			AssertNoMessageErrors(container.CN_RN_NKCountryOfRegistrationInfo);
			container.CN_RN_NKCountryOfRegistration = "??";
			AssertHasMessageErrorContaining(container.CN_RN_NKCountryOfRegistrationInfo, ListValidation.InvalidCodeMessageError);
			container.CN_RN_NKCountryOfRegistration = "CA";
			AssertNoMessageErrors(container.CN_RN_NKCountryOfRegistrationInfo);
		}

		CusSCAContainer container;
		protected override void SetUp()
		{
			base.SetUp();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "OCLU3213214";
			container.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
		}
	}
}
