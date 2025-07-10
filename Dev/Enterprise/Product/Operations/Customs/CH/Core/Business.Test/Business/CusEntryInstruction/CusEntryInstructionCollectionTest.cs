using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusEntryInstructionCollection))]
sealed class CusEntryInstructionCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestMaxCountValidation() => CombineAssertions(() =>
	{
		JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		var collection = (ISupportMaxCountValidation)GetCollectionToTest();
		AssertEquals(-1, collection.MaxCountValidator.MaxCount);

		JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		collection = GetCollectionToTest();
		AssertEquals(-1, collection.MaxCountValidator.MaxCount);

		JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		collection = GetCollectionToTest();
		AssertEquals(1, collection.MaxCountValidator.MaxCount);
	});

	public void TestMaxRows() => CombineAssertions(() =>
	{
		const string errorMessage = "You cannot enter more than one Entry Instruction when Shipment Type is EDA";

		var collection = JobDeclaration.CustomsEntryInstructions;
		JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;

		var row1 = collection.AddNew();
		AssertNoRowErrors("No error for 1st row", row1);

		var row2 = collection.AddNew();
		AssertHasRowError(row2, errorMessage);

		JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertNoRowErrors("Not EDA", row2);
	});

	protected override BusinessObjectCollection GetCollectionToTest() => new CusEntryInstructionCollection(JobDeclaration);

	JobDeclaration JobDeclaration => jobDeclaration ??= Factory.New<JobDeclaration>();
	JobDeclaration jobDeclaration;
}
