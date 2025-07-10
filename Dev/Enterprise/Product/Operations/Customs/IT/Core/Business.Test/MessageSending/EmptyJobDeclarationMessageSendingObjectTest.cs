using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(EmptyJobDeclarationMessageSendingObject))]
sealed class EmptyJobDeclarationMessageSendingObjectTest : JobDeclarationMessageSendingObjectTest
{
	protected override ZString ExpectedMessageSubType => ZString.Empty;

	public override void TestCombinedCustomsMessageSubType()
	{
		var emptyMessageSendingObject = new EmptyJobDeclarationMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
		AssertEquals("CombinedCustomsMessageSubType", "", emptyMessageSendingObject.CombinedCustomsMessageSubType);
	}
}
