using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckACN_ContainerNumber_WithLastDigitAndFormat()
		{
			var container = Factory.New<TemporaryStorageContainer>();

			container.ACN_ContainerNumber = "";
			AssertHasMessageError(container.ACN_ContainerNumberInfo, MandatoryValidation.YouHaveNotEnteredMessage("Container Number"));

			container.ACN_ContainerNumber = "123456123456";
			AssertNoMessageError(container.ACN_ContainerNumberInfo, MandatoryValidation.YouHaveNotEnteredMessage("Container Number"));
			AssertHasWarning(container.ACN_ContainerNumberInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");

			container.ACN_ContainerNumber = "QWER1234561";
			AssertNoWarning(container.ACN_ContainerNumberInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			AssertHasWarning(container.ACN_ContainerNumberInfo, "Container number does not have a valid check (last) digit. The check digit should be 4.");

			container.ACN_ContainerNumber = "QWER1234564";
			AssertNoWarning(container.ACN_ContainerNumberInfo, "Container number does not have a valid check (last) digit. The check digit should be 4.");

			container.ACN_ContainerNumber = ZString.Empty;
			AssertNoErrors("Container number can be empty, not expecting errors", container.ACN_ContainerNumberInfo);
		}

		public void TestCheckACN_ContainerNumber()
		{
			var container = Factory.New<TemporaryStorageContainer>();

			container.Validation.ValidateAll();
			var expectedErrorMessage = MandatoryValidation.YouHaveNotEnteredMessage(MandatoryValidation.GetErrorFieldFromProperyInfo(container.ACN_ContainerNumberInfo));
			AssertHasMessageError("Container number must be entered", container.ACN_ContainerNumberInfo, expectedErrorMessage);
			container.ACN_ContainerNumber = "11";
			AssertNoMessageError("Container number entered and error should be cleared", container.ACN_ContainerNumberInfo, expectedErrorMessage);
		}

		public void TestCheckACN_Seal1_WithContainerSeals()
		{
			var expectedErrorMessage = "Please fill Seal 1 before filling Seals 2 or 3";
			var container = Factory.New<TemporaryStorageContainer>();
			container.Validation.ValidateAll();
			AssertNoMessageError("If Seal 2 and 3 don't have values then Seal 1 can be empty", container.ACN_Seal1Info, expectedErrorMessage);

			container.ACN_Seal2 = "22";
			container.Validation.ValidateAll();

			AssertHasMessageError("Seal 1 has to have value before Seal 2", container.ACN_Seal1Info, expectedErrorMessage);

			container.ACN_Seal2 = ZString.Empty;
			container.ACN_Seal3 = "33";
			container.Validation.ValidateAll();
			AssertHasMessageError("Seal 1 has to have value before Seal 3", container.ACN_Seal1Info, expectedErrorMessage);

			container.ACN_Seal2 = "22";
			container.ACN_Seal3 = "33";
			container.Validation.ValidateAll();
			AssertHasMessageError("Seal 1 has to have value before Seals 2 or 3", container.ACN_Seal1Info, expectedErrorMessage);

			container.ACN_Seal1 = "11";
			container.ACN_Seal2 = "22";
			container.ACN_Seal3 = "33";
			container.Validation.ValidateAll();
			AssertNoMessageError("When Seal 1 has a value and Seals 2 and 3 have values too then no error should appear", container.ACN_Seal1Info, expectedErrorMessage);
		}

		public void TestCheckACN_Seal2_WithContainerSeals()
		{
			var expectedErrorMessage = "Please fill Seal 2 before filling Seal 3";

			var container = Factory.New<TemporaryStorageContainer>();
			container.Validation.ValidateAll();
			AssertNoMessageError("If Seal 3 doesn't have value then Seal 2 can be empty", container.ACN_Seal2Info, expectedErrorMessage);

			container.ACN_Seal3 = "33";
			container.Validation.ValidateAll();

			AssertHasMessageError("Seal 2 has to have value before Seal 3", container.ACN_Seal2Info, expectedErrorMessage);

			container.ACN_Seal2 = "22";
			container.ACN_Seal3 = "33";
			container.Validation.ValidateAll();
			AssertNoMessageError("When Seal 2 has a value and Seal 3 has a value too then no error should appear", container.ACN_Seal2Info, expectedErrorMessage);
		}

		public void TestCheckACN_Seal3_WithContainerSeals()
		{
			var container = Factory.New<TemporaryStorageContainer>();
			container.Validation.ValidateAll();
			AssertNoMessageErrors("If Seals 1 and 2 are empty then Seal 3 can be empty", container.ACN_Seal3Info);

			container.ACN_Seal1 = "11";
			container.Validation.ValidateAll();

			AssertNoMessageErrors("When Seal 1 has value and Seal 2 doesn't, Seal 3 can be empty", container.ACN_Seal3Info);

			container.ACN_Seal1 = ZString.Empty;
			container.ACN_Seal2 = "22";
			container.Validation.ValidateAll();
			AssertNoMessageErrors("When Seal 1 doesn't have value and Seal 2 has, Seal 3 can be empty", container.ACN_Seal3Info);

			container.ACN_Seal1 = "11";
			container.ACN_Seal2 = "22";
			container.Validation.ValidateAll();
			AssertNoMessageErrors("When Seal 1 has a value and Seal 2 has a value, Seal 3 can be empty", container.ACN_Seal3Info);
		}

		public void TestCheckACN_Seal1_WithAdditionalSeals()
		{
			var container = Factory.New<TemporaryStorageContainer>();
			AssertContainersSealShouldBeFilledFirst(ZString.Empty, "2", "3", true, container.ACN_Seal1Info, container);
			container = Factory.New<TemporaryStorageContainer>();
			AssertContainersSealShouldBeFilledFirst("1", "2", "3", false, container.ACN_Seal1Info, container);
		}

		public void TestCheckACN_Seal2_WithAdditionalSeals()
		{
			var container = Factory.New<TemporaryStorageContainer>();
			AssertContainersSealShouldBeFilledFirst("1", ZString.Empty, "3", true, container.ACN_Seal2Info, container);
			container = Factory.New<TemporaryStorageContainer>();
			AssertContainersSealShouldBeFilledFirst("1", "2", "3", false, container.ACN_Seal2Info, container);
		}

		public void TestCheckACN_Seal3_WithAdditionalSeals()
		{
			var container = Factory.New<TemporaryStorageContainer>();
			AssertContainersSealShouldBeFilledFirst("1", "2", ZString.Empty, true, container.ACN_Seal3Info, container);
			container = Factory.New<TemporaryStorageContainer>();
			AssertContainersSealShouldBeFilledFirst("1", "2", "3", false, container.ACN_Seal3Info, container);
		}

		void AssertContainersSealShouldBeFilledFirst(ZString seal1, ZString seal2, ZString seal3, bool shouldHaveError, ZPropertyInfo sealToTest, TemporaryStorageContainer container)
		{
			var message = "Please use this seal before using the additional seals.";

			Assert("prerequisite : ", container.ACN_Seal1.IsEmpty && container.ACN_Seal2.IsEmpty && container.ACN_Seal3.IsEmpty && container.AdditionalSeals.Count == 0);

			container.ACN_Seal1 = seal1;
			container.ACN_Seal2 = seal2;
			container.ACN_Seal3 = seal3;

			if (shouldHaveError)
			{
				AssertNoMessageError($"{sealToTest.Name} should have no error as the field is empty but there is no additionalSeals.", sealToTest, message);
				container.AdditionalSeals.AddNew();
				AssertEquals(1, container.AdditionalSeals.Count);
				container.Validation.ValidateAll();
				AssertHasMessageError($"{sealToTest.Name} should have an error as the field is empty and there is an additionalSeals.", sealToTest, message);
			}
			else
			{
				AssertNoMessageError($"{sealToTest.Name} should have no error as the field is not empty and there is no additionalSeals.", sealToTest, message);
				container.AdditionalSeals.AddNew();
				AssertNoMessageError($"{sealToTest.Name} should have no error as the field is not empty and there is an additionalSeals.", sealToTest, message);
			}
		}

		public void TestACN_EmptyFullIndicator()
		{
			var container = Factory.New<TemporaryStorageContainer>();
			container.ACN_EmptyFullIndicator = "ERR";
			AssertHasMessageErrorContaining("Value is not in the list.", container.ACN_EmptyFullIndicatorInfo, ListValidation.InvalidCodeMessageError.ToString());

			container.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.NotEmpty;
			AssertNoMessageErrorContaining("Value is in the list.", container.ACN_EmptyFullIndicatorInfo, ListValidation.InvalidCodeMessageError.ToString());
		}

		public void TestACN_RC_ContainerType()
		{
			var container = Factory.New<TemporaryStorageContainer>();
			container.ACN_RC_ContainerType = ZGuid.Invalid;

			AssertHasErrorContaining("Value is not in the list.", container.ACN_RC_ContainerTypeInfo, ListValidation.InvalidCodeError);

			container.ACN_RC_ContainerType = ZGuid.Empty;
			var expectedErrorMessage = MandatoryValidation.YouHaveNotEnteredMessage(MandatoryValidation.GetErrorFieldFromProperyInfo(container.ACN_RC_ContainerTypeInfo));
			AssertHasMessageError("No value is entered.", container.ACN_RC_ContainerTypeInfo, expectedErrorMessage);

			var refContainer = container.Lookups.ContainerTypes.AddNew();
			refContainer.FillWithValidTestData();
			container.ACN_RC_ContainerType = refContainer.PK;
			AssertNoMessageError("A value has been entered.", container.ACN_EmptyFullIndicatorInfo, expectedErrorMessage);
			AssertNoErrorContaining("Value is in the list.", container.ACN_EmptyFullIndicatorInfo, ListValidation.InvalidCodeError);
		}

		public void TestPackAssignedToContainer()
		{
			var expectedWarning1 = "The container CONT1 is not assigned to any Pack.";
			var expectedWarning2 = "The container  is not assigned to any Pack.";

			var header = Factory.New<TemporaryStorageHeader>();
			var container1 = header.Containers.AddNew();
			container1.ACN_ContainerNumber = "CONT1";

			var container2 = header.Containers.AddNew();

			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container1.PK;

			CombineAssertions(() =>
			{
				container1.Validation.ValidateAll();
				container2.Validation.ValidateAll();

				AssertNoRowWarningContaining("When there are Packs and at least one is associated to container1", container1, expectedWarning1);
				AssertHasRowWarning("When there are Packs and none are associated to container2", container2, expectedWarning2);

				pack.ContainerPK = ZGuid.Empty;
				container1.Validation.ValidateAll();
				container2.Validation.ValidateAll();

				AssertHasRowWarning("When there are Packs and none are associated to container1", container1, expectedWarning1);
				AssertHasRowWarning("When there are Packs and none are associated to container2, second check", container2, expectedWarning2);

				pack.ContainerPK = container2.PK;
				container1.Validation.ValidateAll();
				container2.Validation.ValidateAll();

				AssertHasRowWarning("When there are Packs and none are associated to container1, third check", container1, expectedWarning1);
				AssertNoRowWarningContaining("When there are Packs and at least one is associated to container2", container2, expectedWarning2);

				bill.Packs.RemoveAndDeleteAll();
				container1.Validation.ValidateAll();
				container2.Validation.ValidateAll();
				AssertNoRowWarningContaining("When no Packs are declared no warning is shown, container1", container1, expectedWarning1);
				AssertNoRowWarningContaining("When no Packs are declared no warning is shown, container2", container2, expectedWarning2);
			});
		}
	}
}
