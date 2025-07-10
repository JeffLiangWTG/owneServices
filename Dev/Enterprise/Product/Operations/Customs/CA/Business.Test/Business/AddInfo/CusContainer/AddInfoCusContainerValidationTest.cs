using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AddInfoCusContainerValidationTest : CAAddInfoValidationTest<AddInfoCusContainer>
	{
		protected override AddInfoCusContainer GetNewAddInfo()
		{
			return new AddInfoCusContainer(Factory.New<CusContainer>().CO_AddInfoInfo);
		}

		public void TestCheckCA_ContainerSizeOrISOCode()
		{
			container.CA_ContainerSizeOrISOCode = "20G";
			AssertHasMessageErrorContaining(container.CA_ContainerSizeOrISOCodeInfo, "Container ISO Size must be four characters long.");
			container.CA_ContainerSizeOrISOCode = "20G0";
			AssertNoNotifications(container.CA_ContainerSizeOrISOCodeInfo);
			container.CA_ContainerSizeOrISOCode = "20G?";
			AssertHasMessageErrorContaining(container.CA_ContainerSizeOrISOCodeInfo, "Unknown ISO Code.");
			container.CA_ContainerSizeOrISOCode = "";
			AssertNoNotifications(container.CA_ContainerSizeOrISOCodeInfo);
		}

		public void TestCheckCA_RN_NKCountryOfRegistration()
		{
			container.CA_RN_NKCountryOfRegistration = "AU";
			AssertNoMessageErrorContaining(container.CA_RN_NKCountryOfRegistrationInfo, ListValidation.InvalidCodeMessageError);
			container.CA_RN_NKCountryOfRegistration = "??";
			AssertHasMessageErrorContaining(container.CA_RN_NKCountryOfRegistrationInfo, ListValidation.InvalidCodeMessageError);
			container.CA_RN_NKCountryOfRegistration = "";
			AssertNoNotifications(container.CA_RN_NKCountryOfRegistrationInfo);
		}

		JobDeclaration declaration;
		CusContainer container;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			container = declaration.CusContainers.AddNew();
		}
	}
}
