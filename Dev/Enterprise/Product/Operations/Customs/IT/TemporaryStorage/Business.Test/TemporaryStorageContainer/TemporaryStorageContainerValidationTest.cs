using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageContainerValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckACN_RC_ContainerType_NoMandatoryValidation()
	{
		ValidationTestHelper.AssertFieldIsNotMandatory(container.ACN_RC_ContainerTypeInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckACN_Seal1_NoSealAvailable()
	{
		const string expectedMessage = "No seal available or bulk goods. In the message will be written 0 (zero)";
		var propertyInfo = container.ACN_Seal1Info;

		container.ACN_ContainerNumber = "Container 1";
		container.ACN_Seal1 = "Seal 1";
		AssertNoWarning("ACN_ContainerNumber has value, ACN_Seal1 has value", propertyInfo, expectedMessage);

		container.ACN_Seal2 = "Seal 2";
		container.ACN_Seal1 = ZString.Empty;
		AssertNoWarning("ACN_ContainerNumber has value, ACN_Seal2 has value", propertyInfo, expectedMessage);

		container.ACN_Seal3 = "Seal 3";
		container.ACN_Seal2 = ZString.Empty;
		var validation = container.Validation;
		validation.ValidateACN_Seal1();
		AssertNoWarning("ACN_ContainerNumber has value, ACN_Seal3 has value", propertyInfo, expectedMessage);

		container.ACN_Seal3 = ZString.Empty;
		validation.ValidateACN_Seal1();
		AssertHasWarning("ACN_ContainerNumber has value, ACN_Seal1, ACN_Seal2 and ACN_Seal3 are all empty", propertyInfo, expectedMessage);

		container.ACN_ContainerNumber = ZString.Empty;
		validation.ValidateACN_Seal1();
		AssertNoWarning("ACN_ContainerNumber, ACN_Seal1, ACN_Seal2 and ACN_Seal3 are all empty", propertyInfo, expectedMessage);
	}

	public void TestCheckACN_Seal1Expression()
	{
		AssertSealValidation(container.ACN_Seal1Info, (value) => container.ACN_Seal1 = value);
	}

	public void TestCheckACN_Seal2Expression()
	{
		AssertSealValidation(container.ACN_Seal2Info, (value) => container.ACN_Seal2 = value);
	}

	public void TestCheckACN_Seal3Expression()
	{
		AssertSealValidation(container.ACN_Seal3Info, (value) => container.ACN_Seal3 = value);
	}

	void AssertSealValidation(ZPropertyInfo propertyInfo, Action<ZString> valueSetter)
	{
		const string expectedMessage = "The field does not match validation pattern [A-Z0-9]{1,20}: it must contain only uppercase letters and numbers, and have length from 1 to 20 characters";

		valueSetter("");
		AssertNoMessageError("Valid input, empty string", propertyInfo, expectedMessage);

		valueSetter("A");
		AssertNoMessageError("Valid input, seal contains uppercase letters", propertyInfo, expectedMessage);

		valueSetter("TEST1234");
		AssertNoMessageError("Valid input, seal contains uppercase letters and numbers", propertyInfo, expectedMessage);

		valueSetter("TEST");
		AssertNoMessageError("Valid input, seal contains uppercase letters", propertyInfo, expectedMessage);

		valueSetter("123456");
		AssertNoMessageError("Valid input, seal contains numbers", propertyInfo, expectedMessage);

		valueSetter("test012");
		AssertHasMessageError("Invalid input, seal contains lowercase letters", propertyInfo, expectedMessage);

		valueSetter("TEST@1234");
		AssertHasMessageError("Invalid input, seal contains special characters", propertyInfo, expectedMessage);

		valueSetter("TEST 1234");
		AssertHasMessageError("Invalid input, seal contains special characters", propertyInfo, expectedMessage);

		valueSetter("ABCDEFGHIJKLMNOPQRST");
		AssertNoMessageError("Valid input, seal length is equal to 20 characters", propertyInfo, expectedMessage);
	}

	protected override void SetUp()
	{
		base.SetUp();
		container = Factory.New<TemporaryStorageContainer>();
	}

	TemporaryStorageContainer container;
}
