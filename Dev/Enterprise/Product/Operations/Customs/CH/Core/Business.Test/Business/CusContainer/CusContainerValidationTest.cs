using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.CH;

namespace Enterprise.Customs.CH.Business.Testing;

class CusContainerValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheck_CO_Seal_NP70021() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var container = declaration.CusContainers.AddNew();

		container.Validation.ValidateCO_Seal();
		AssertHasMessageError("CO_ContainerNumber, CO_Seal and CO_SecondSeal are empty", container.CO_SealInfo, PassarValidationMessages.MessageNP70021);

		container.CO_ContainerNumber = "XXX";
		container.Validation.ValidateCO_Seal();
		AssertNoMessageError("CO_ContainerNumber not empty", container.CO_SealInfo, PassarValidationMessages.MessageNP70021);

		container.CO_ContainerNumber = ZString.Empty;
		container.CO_Seal = "XXX";
		AssertNoMessageError("CO_Seal not empty", container.CO_SealInfo, PassarValidationMessages.MessageNP70021);

		container.CO_Seal = ZString.Empty;
		container.CO_SecondSeal = "XXX";
		container.Validation.ValidateCO_Seal();
		AssertNoMessageError("CO_SecondSeal not empty", container.CO_SealInfo, PassarValidationMessages.MessageNP70021);
	});

	public void TestCheckCO_Seal_NP70043() => CombineAssertions(() =>
	{
		var messageError = PassarValidationMessages.MessageNP70043;
		AssertNoMessageError("Seal empty", CusContainer.CO_SealInfo, messageError);

		CusContainer.CO_Seal = "Seal";
		AssertNoMessageError("Single container", CusContainer.CO_SealInfo, messageError);

		CusContainer.CO_SecondSeal = "Seal";
		AssertHasMessageError("Single container, Seal = SecondSeal", CusContainer.CO_SealInfo, messageError);
		AssertHasMessageError("Single container, Seal = SecondSeal", CusContainer.CO_SecondSealInfo, messageError);

		CusContainer.CO_SecondSeal = ZString.Empty;
		var cuContainer2 = Declaration.CusContainers.AddNew();
		CusContainer.Validation.ValidateCO_Seal();
		AssertNoMessageError("Multiple container with different Seals", CusContainer.CO_SealInfo, messageError);

		cuContainer2.CO_Seal = "Seal";
		CusContainer.Validation.ValidateCO_Seal();
		AssertHasMessageError("Multiple container with same Seal", CusContainer.CO_SealInfo, messageError);

		cuContainer2.CO_Seal = "Different Seal";
		cuContainer2.CO_SecondSeal = "Seal";
		CusContainer.Validation.ValidateCO_Seal();
		AssertHasMessageError("Multiple container with same Seal (On Seal)", CusContainer.CO_SealInfo, messageError);

		CusContainer.CO_Seal = ZString.Empty;
		CusContainer.CO_SecondSeal = "Seal";
		AssertHasMessageError("Multiple container with same Seal (On SecondSeal)", CusContainer.CO_SecondSealInfo, messageError);
	});

	public void TestCheckContainerNoHasPackages() => CombineAssertions(() =>
	{
		var messageError = "Container Has No Packing Lines - You must enter some packing lines for this container.";

		CusContainer.CO_ContainerNumber = "BICU1234565";

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		CusContainer.Validation.ValidateCO_ContainerNumber();
		AssertHasMessageError("Declaration Import has messageError", CusContainer.CO_ContainerNumberInfo, messageError);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		CusContainer.Validation.ValidateCO_ContainerNumber();
		AssertHasMessageError("Declaration Export has messageError", CusContainer.CO_ContainerNumberInfo, messageError);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		CusContainer.Validation.ValidateCO_ContainerNumber();
		AssertNoMessageError("Declaration EDA has no messageError", CusContainer.CO_ContainerNumberInfo, messageError);
	});

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	CusContainer CusContainer => cusContainer ??= Declaration.CusContainers.AddNew();
	CusContainer cusContainer;
}
