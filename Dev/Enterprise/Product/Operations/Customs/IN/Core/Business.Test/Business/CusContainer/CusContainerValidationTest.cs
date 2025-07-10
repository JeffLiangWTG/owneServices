using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CusContainerValidation))]
sealed class CusContainerValidationTest : CusContainerValidationTest<JobDeclaration>
{
	public void TestCheckCO_SealType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var container = declaration.CusContainers.AddNew();

		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(container.CO_SealTypeInfo);

			container.CO_ContainerNumber = "CRXU1234568";
			container.SealNumberForBinding = "123";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(container.CO_SealTypeInfo);
		});
	}
}
