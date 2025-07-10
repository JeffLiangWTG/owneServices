using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusContainerValidation))]
	public class CusContainerValidationTest : Customs.Business.Testing.CusContainerValidationTest<JobDeclaration>
	{
		public override void TestContainersRequirePackagesValidation()
		{
			var mockDeclaration = Factory.NewMoq<JobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			var declaration = mockDeclaration.Object;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.DisableDefaultPackingInformation = true;
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HouseBill";
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
			AssertEquals("HasMessageError(CusContainerValidation.ContainersRequirePackages)", false, container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.ContainersRequirePackages));

			var package = declaration.Packages.AddNew();
			package.CW_HouseBill = houseBill.CU_HouseBill;
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			AssertEquals("HasMessageError(CusContainerValidation.ContainersRequirePackages)", false, container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.ContainersRequirePackages));

			package.CW_ContainerNoOrEquipmentNo = "";
			AssertEquals("HasMessageError(CusContainerValidation.ContainersRequirePackages)", false, container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.ContainersRequirePackages));
		}

		public void TestImportContainer()
		{
			var mockDeclaration = Factory.NewMoq<JobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			var declaration = mockDeclaration.Object;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var container = declaration.CusContainers.AddNew();

			container.Validation.ValidateCO_ContainerNumber();
			AssertNoMessageErrorContaining(container.CO_ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			container.Validation.ValidateCO_ContainerNumber();
			AssertHasMessageErrorContaining(container.CO_ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);

			container.CO_ContainerNumber = "CRUX34987432";
			AssertNoMessageErrorContaining(container.CO_ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
