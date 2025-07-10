using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class JPAFRContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJPC_ContainerNum()
		{
			ResetJPAFRBillContainer();

			Container.JPC_ContainerNum = string.Empty;
			AssertHasMessageErrorContaining(Container.JPC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberIsRequired);

			Container.JPC_ContainerNum = "123456789_12";
			AssertHasMessageErrorContaining(Container.JPC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberShouldContainOnlyAlphanumeric);

			Container.JPC_ContainerNum = "1234567890123";
			AssertHasMessageErrorContaining(Container.JPC_ContainerNumInfo, ValidationConstants.Container.MaximumContainerNumberLengthExceeded);

			Container.JPC_ContainerNum = "TEST1234562";
			AssertHasWarningContaining(Container.JPC_ContainerNumInfo, "Container number does not have a valid check (last) digit. The check digit should be 0.");

			Container.JPC_ContainerNum = "123456789012";
			AssertHasWarningContaining(Container.JPC_ContainerNumInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");

			var secondContainer = Bill.Containers.AddNew();
			secondContainer.JPC_ContainerNum = "123456789012";
			AssertHasErrorContaining(secondContainer.JPC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberIsDuplicated);

			secondContainer.JPC_ContainerNum = "TEST1234560";
			AssertNoMessageErrors(secondContainer.JPC_ContainerNumInfo);
			AssertNoErrors(secondContainer.JPC_ContainerNumInfo);
			AssertNoWarnings(secondContainer.JPC_ContainerNumInfo);
		}

		public void TestCheckJPC_RC_ContainerType()
		{
			ResetJPAFRBillContainer();

			Container.JPC_RC_ContainerType = ZGuid.Empty;
			AssertHasMessageErrorContaining(Container.JPC_RC_ContainerTypeInfo, MandatoryValidation.YouHaveNotEntered);

			var testContainerType = Factory.NewWithValidTestData<RefContainer>();
			Container.JPC_RC_ContainerType = testContainerType.PK;
			AssertNoMessageErrors(Container.JPC_RC_ContainerTypeInfo);

			testContainerType.RC_Code = "20[G";
			Container.JPC_RC_ContainerType = testContainerType.PK;
			AssertHasMessageErrorContaining(Container.JPC_RC_ContainerTypeInfo, ValidationConstants.Shared.InvalidNACCSChar(Container.JPC_RC_ContainerTypeInfo.HumanReadableName));
		}

		public void TestCheckJPC_OwnershipCode()
		{
			ResetJPAFRBillContainer();

			Container.JPC_OwnershipCode = string.Empty;
			AssertHasMessageErrorContaining(Container.JPC_OwnershipCodeInfo, MandatoryValidation.YouHaveNotEntered);

			Container.JPC_OwnershipCode = "9";
			AssertHasMessageErrorContaining(Container.JPC_OwnershipCodeInfo, ListValidation.InvalidCodeMessageError);

			Container.JPC_OwnershipCode = "1";
			AssertNoMessageErrors(Container.JPC_OwnershipCodeInfo);
		}

		public void TestCheckJPC_Seal1_Seal2()
		{
			ResetJPAFRBillContainer();
			Container.JPC_Seal1 = ZString.Empty;
			Container.JPC_Seal2 = ZString.Empty;
			AssertNoMessageErrors(Container.JPC_Seal1Info);
			AssertNoErrors(Container.JPC_Seal1Info);
			AssertHasWarningContaining(Container.JPC_Seal1Info, ValidationConstants.Container.NoSealRequiredWhenNoSealNumberPresents);
			AssertNoMessageErrors(Container.JPC_Seal2Info);
			AssertNoErrors(Container.JPC_Seal2Info);
			AssertNoWarnings(Container.JPC_Seal2Info);

			Container.JPC_Seal1 = ZString.Empty;
			Container.JPC_Seal2 = "Test";
			AssertNoMessageErrors(Container.JPC_Seal1Info);
			AssertNoErrors(Container.JPC_Seal1Info);
			AssertNoWarnings(Container.JPC_Seal1Info);
			AssertNoMessageErrors(Container.JPC_Seal2Info);
			AssertNoErrors(Container.JPC_Seal2Info);
			AssertNoWarnings(Container.JPC_Seal2Info);

			Container.JPC_Seal1 = "Test";
			Container.JPC_Seal2 = ZString.Empty;
			AssertNoMessageErrors(Container.JPC_Seal1Info);
			AssertNoErrors(Container.JPC_Seal1Info);
			AssertNoWarnings(Container.JPC_Seal1Info);
			AssertNoMessageErrors(Container.JPC_Seal2Info);
			AssertNoErrors(Container.JPC_Seal2Info);
			AssertNoWarnings(Container.JPC_Seal2Info);

			Container.JPC_Seal1 = "Test";
			Container.JPC_Seal2 = "Test";
			AssertNoMessageErrors(Container.JPC_Seal1Info);
			AssertNoErrors(Container.JPC_Seal1Info);
			AssertNoWarnings(Container.JPC_Seal1Info);
			AssertNoMessageErrors(Container.JPC_Seal2Info);
			AssertNoErrors(Container.JPC_Seal2Info);
			AssertNoWarnings(Container.JPC_Seal2Info);

			Container.JPC_Seal1 = "Test[";
			Container.JPC_Seal2 = "Test]";
			AssertHasMessageErrorContaining(Container.JPC_Seal1Info, ValidationConstants.Shared.InvalidNACCSChar(Container.JPC_Seal1Info.HumanReadableName));
			AssertNoErrors(Container.JPC_Seal1Info);
			AssertNoWarnings(Container.JPC_Seal1Info);
			AssertHasMessageErrorContaining(container.JPC_Seal2Info, ValidationConstants.Shared.InvalidNACCSChar(container.JPC_Seal2Info.HumanReadableName));
			AssertNoErrors(Container.JPC_Seal2Info);
			AssertNoWarnings(Container.JPC_Seal2Info);
		}

		public void TestCheckJPC_TypeOfService()
		{
			CombineAssertions("Test For NVOCC", () =>
			{
				ResetJPAFRBillContainer();

				Container.JPC_TypeOfService = string.Empty;
				AssertNoMessageErrors(Container.JPC_TypeOfServiceInfo);

				Container.JPC_TypeOfService = "9";
				AssertNoMessageErrors(Container.JPC_TypeOfServiceInfo);

				Container.JPC_TypeOfService = "51";
				AssertNoMessageErrors(Container.JPC_TypeOfServiceInfo);

				Container.JPC_TypeOfService = "52";
				AssertNoMessageErrors(Container.JPC_TypeOfServiceInfo);

				Container.JPC_TypeOfService = "53";
				AssertNoMessageErrors(Container.JPC_TypeOfServiceInfo);

				Container.JPC_TypeOfService = "x";
				AssertNoMessageErrors(Container.JPC_TypeOfServiceInfo);
			});

			CombineAssertions("Test For VOCC", () =>
			{
				ResetJPAFRBillContainer();
				Header.JPH_IsShippingLineEntry = true;

				Container.JPC_TypeOfService = string.Empty;
				AssertNoMessageErrors(Container.JPC_TypeOfServiceInfo);

				Container.JPC_TypeOfService = "9";
				AssertHasMessageErrorContaining(Container.JPC_TypeOfServiceInfo, ListValidation.InvalidCodeMessageError);

				Container.JPC_TypeOfService = "51";
				AssertNoMessageErrors(Container.JPC_TypeOfServiceInfo);

				Container.JPC_TypeOfService = "52";
				AssertNoMessageErrors(Container.JPC_TypeOfServiceInfo);

				Container.JPC_TypeOfService = "53";
				AssertNoMessageErrors(Container.JPC_TypeOfServiceInfo);

				Container.JPC_TypeOfService = "x";
				AssertHasMessageErrorContaining(Container.JPC_TypeOfServiceInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckJPC_VanningType()
		{
			CombineAssertions("Test For NVOCC", () =>
			{
				ResetJPAFRBillContainer();

				Container.JPC_VanningType = string.Empty;
				AssertNoMessageErrors(Container.JPC_VanningTypeInfo);

				Container.JPC_VanningType = "9";
				AssertNoMessageErrors(Container.JPC_VanningTypeInfo);

				Container.JPC_VanningType = "1";
				AssertNoMessageErrors(Container.JPC_VanningTypeInfo);
				Container.JPC_VanningType = "4";
				AssertNoMessageErrors(Container.JPC_VanningTypeInfo);
				Container.JPC_VanningType = "16";
				AssertNoMessageErrors(Container.JPC_VanningTypeInfo);

				Container.JPC_VanningType = "x";
				AssertNoMessageErrors(Container.JPC_VanningTypeInfo);
			});

			CombineAssertions("Test For VOCC", () =>
			{
				ResetJPAFRBillContainer();
				Header.JPH_IsShippingLineEntry = true;

				Container.JPC_VanningType = string.Empty;
				AssertNoMessageErrors(Container.JPC_VanningTypeInfo);

				Container.JPC_VanningType = "9";
				AssertHasMessageErrorContaining(Container.JPC_VanningTypeInfo, ListValidation.InvalidCodeMessageError);

				Container.JPC_VanningType = "1";
				AssertNoMessageErrors(Container.JPC_VanningTypeInfo);
				Container.JPC_VanningType = "4";
				AssertNoMessageErrors(Container.JPC_VanningTypeInfo);
				Container.JPC_VanningType = "16";
				AssertNoMessageErrors(Container.JPC_VanningTypeInfo);

				Container.JPC_VanningType = "x";
				AssertHasMessageErrorContaining(Container.JPC_VanningTypeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckJPC_CCCApplicationId()
		{
			CombineAssertions("Test for NVOCC", () =>
			{
				ResetJPAFRBillContainer();

				Container.JPC_CCCApplicationId = string.Empty;
				AssertNoMessageErrors(Container.JPC_CCCApplicationIdInfo);

				Container.JPC_CCCApplicationId = "9";
				AssertNoMessageErrors(Container.JPC_CCCApplicationIdInfo);

				Container.JPC_CCCApplicationId = "1";
				AssertNoMessageErrors(Container.JPC_CCCApplicationIdInfo);

				Container.JPC_CCCApplicationId = "2";
				AssertNoMessageErrors(Container.JPC_CCCApplicationIdInfo);

				Container.JPC_CCCApplicationId = "3";
				AssertNoMessageErrors(Container.JPC_CCCApplicationIdInfo);

				Container.JPC_CCCApplicationId = "x";
				AssertNoMessageErrors(Container.JPC_CCCApplicationIdInfo);
			});

			CombineAssertions("Test for VOCC", () =>
			{
				ResetJPAFRBillContainer();
				Header.JPH_IsShippingLineEntry = true;

				Container.JPC_CCCApplicationId = string.Empty;
				AssertNoMessageErrors(Container.JPC_CCCApplicationIdInfo);

				Container.JPC_CCCApplicationId = "9";
				AssertHasMessageErrorContaining(Container.JPC_CCCApplicationIdInfo, ListValidation.InvalidCodeMessageError);

				Container.JPC_CCCApplicationId = "1";
				AssertNoMessageErrors(Container.JPC_CCCApplicationIdInfo);

				Container.JPC_CCCApplicationId = "2";
				AssertNoMessageErrors(Container.JPC_CCCApplicationIdInfo);

				Container.JPC_CCCApplicationId = "3";
				AssertNoMessageErrors(Container.JPC_CCCApplicationIdInfo);

				Container.JPC_CCCApplicationId = "x";
				AssertHasMessageErrorContaining(Container.JPC_CCCApplicationIdInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		#region Preperation

		JPAFRContainer Container
		{
			get { return container ?? (container = Bill.Containers.AddNew()); }
		}
		JPAFRContainer container;

		JPAFRBills Bill
		{
			get { return bill ?? (bill = Header.Bills.AddNew()); }
		}
		JPAFRBills bill;

		JPAFRHeader Header
		{
			get { return header ?? (header = Factory.New<JPAFRHeader>()); }
		}
		JPAFRHeader header;

		void ResetJPAFRBillContainer()
		{
			this.header = null;
			this.bill = null;
			this.container = null;
		}

		#endregion
	}
}
