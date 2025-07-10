using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	class CusExitContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSequenceNumberNo0NoDuplicatedNoNegativeWithoutSequence()
		{
			var cusExitHeader = Factory.New<CusExitHeaderWithoutSequenceForTest>();
			var container1 = cusExitHeader.CusExitContainers.AddNew();
			var container2 = cusExitHeader.CusExitContainers.AddNew();
			var targetInfo1 = container1.CXN_SequenceInfo;
			var targetInfo2 = container2.CXN_SequenceInfo;
			CombineAssertions(() =>
			{
				container1.CXN_Sequence = 0;
				AssertNoErrors(targetInfo1);
				container1.CXN_Sequence = 1;

				container2.CXN_Sequence = 1;
				container2.Validation.ValidateCXN_Sequence();
				AssertNoErrors(targetInfo2);

				container2.CXN_Sequence = -1;
				AssertNoErrors(targetInfo2);
			});
		}

		public void TestSequenceNumberNo0NoDuplicatedNoNegativeWithSequence()
		{
			var valueCannotBeZeroMessage = "Sequence Number cannot be zero.";
			var messageErrorNotBeDuplicated = "Sequence Number (1) should not be duplicated";
			var messageErrorNotBeNegative = "Sequence Number cannot be negative.";

			var cusExitHeader = Factory.New<CusExitHeaderWithSequenceForTest>();
			var container1 = cusExitHeader.CusExitContainers.AddNew();
			var container2 = cusExitHeader.CusExitContainers.AddNew();
			var targetInfo1 = container1.CXN_SequenceInfo;
			var targetInfo2 = container2.CXN_SequenceInfo;
			CombineAssertions(() =>
			{
				container1.CXN_Sequence = 0;
				AssertHasErrorContaining("The test is for control of sequence 0", targetInfo1, valueCannotBeZeroMessage);
				container1.CXN_Sequence = 1;
				AssertNoErrorContaining("No test error 0", targetInfo1, valueCannotBeZeroMessage);

				container2.CXN_Sequence = 1;
				AssertHasError("The test is for control of sequence duplicated", targetInfo2, messageErrorNotBeDuplicated);
				container2.CXN_Sequence = 2;
				AssertNoError("No test sequence duplicated", targetInfo2, messageErrorNotBeDuplicated);

				container2.CXN_Sequence = -1;
				AssertHasErrorContaining("The test cannot be negative", targetInfo2, messageErrorNotBeNegative);
				container2.CXN_Sequence = 2;
				AssertNoErrorContaining("No test cannot be negative", targetInfo2, messageErrorNotBeNegative);
			});
		}

		public void TestCheckCXN_IsEquipment()
		{
			(var container, _) = CreateData(Factory);
			CombineAssertions(() =>
			{
				container.CXN_IsEquipment = ZBool.False;
				AssertNoMessageErrors(container.CXN_IsEquipmentInfo);
				container.CXN_IsEquipment = ZBool.True;
				var messageError = "At least one Seal must be specified for Equipment.";
				AssertHasMessageError(container.CXN_IsEquipmentInfo, messageError);
				container.AllSealNumbers.AddNew();
				container.Validation.ValidateCXN_IsEquipment();
				AssertNoMessageErrors(container.CXN_IsEquipmentInfo);
			});
		}

		public void TestCheckCXN_ContainerNumber()
		{
			(var container, _) = CreateData(Factory);
			CombineAssertions(() =>
			{
				container.CXN_ContainerNumber = ZString.Empty;
				AssertHasMessageErrorContaining(container.CXN_ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);
				container.CXN_ContainerNumber = "OFRI1007156";
				AssertNoMessageErrors(container.CXN_ContainerNumberInfo);
				AssertNoWarnings(container.CXN_ContainerNumberInfo);
				container.CXN_ContainerNumber = "C";
				AssertNoMessageErrorContaining(container.CXN_ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);
				var invalidCheckDigit = "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.";
				AssertHasWarning(container.CXN_ContainerNumberInfo, invalidCheckDigit);
				container.CXN_IsEquipment = ZBool.True;
				container.Validation.ValidateCXN_ContainerNumber();
				AssertNoMessageErrors(container.CXN_ContainerNumberInfo);
				AssertNoWarnings(container.CXN_ContainerNumberInfo);
			});
		}

		public void TestCheckCXN_Status_ListValidation() => CombineAssertions(() =>
		{
			(var container, _) = CreateData(Factory);
			container.CXN_Status = "AH3";
			AssertListValidationInvalidCodeMessageError(container.CXN_StatusInfo, false);

			container = Factory.GetUcc6ExitHeader().CusExitContainers.AddNew();
			container.CXN_Status = "AH3";
			AssertListValidationInvalidCodeMessageError(container.CXN_StatusInfo, true);

			container.CXN_Status = "DIF";
			AssertListValidationInvalidCodeMessageError(container.CXN_StatusInfo, false);
		});

		(CusExitContainer Container, CusExitHeader Header) CreateData(BusinessObjectFactory factory) => CusExitContainerTest.GetNewBusinessObject(factory);

		class CusExitHeaderWithSequenceForTest : CusExitHeader
		{
			public CusExitHeaderWithSequenceForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool ShouldHaveSeqNumInContainersOrEquipmentsAndSealsCore => true;
		}

		class CusExitHeaderWithoutSequenceForTest : CusExitHeader
		{
			public CusExitHeaderWithoutSequenceForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool ShouldHaveSeqNumInContainersOrEquipmentsAndSealsCore => false;
		}
	}
}
