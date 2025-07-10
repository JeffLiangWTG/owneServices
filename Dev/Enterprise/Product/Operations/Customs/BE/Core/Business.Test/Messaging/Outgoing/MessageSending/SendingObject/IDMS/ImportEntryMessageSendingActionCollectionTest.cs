using System;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(ImportEntryMessageSendingActionCollection))]
class ImportEntryMessageSendingActionCollectionTest : BEJobDeclarationMessageSendingObjectCollectionTest<ImportDeclarationMessageSendingActionParent, ImportEntryMessageSendingActionCollection, ImportEntryMessageSendingAction>
{
	protected override Type ExpectedSendingObjectCollectionType => typeof(ImportEntryMessageSendingAction);
}
