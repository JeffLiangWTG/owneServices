using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing.Message.MessageSendingAction.FinalizeAVABRDeclaration;

[TestedType(typeof(FinalizeAVABREntryMessageSendingActionCollection))]
public class FinalizeAVABREntryMessageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FinalizeAVABREntryMessageSendingActionCollection>
{
	public void TestGetSendingAction()
	{
		Declaration.CustomsEntryHeaders.AddNew();
		var collection = GetCollectionToTest();
		collection.PopulateElements();
		AssertEquals(typeof(FinalizeAVABREntryMessageSendingAction), collection[0].GetType());
	}
	protected override FinalizeAVABREntryMessageSendingActionCollection GetCollectionToTest() => new(Declaration, null);

	protected override BusinessObject GetNewElementToAddToTheCollection() => new FinalizeAVABREntryMessageSendingAction(Declaration.CustomsEntryHeaders.AddNew(), null);

	JobDeclaration jobDeclaration;
	JobDeclaration Declaration => jobDeclaration ??= Factory.New<JobDeclaration>();
}
