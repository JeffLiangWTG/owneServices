using System;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(ImportDeclarationMessageSendingActionParent))]
class ImportDeclarationMessageSendingActionParentTest : BEJobDeclarationMessageSendingObjectParentTest<ImportDeclarationMessageSendingActionParent, ImportEntryMessageSendingActionCollection, ImportEntryMessageSendingAction>
{
	protected override Type ExpectedSendingObjectCollectionType => typeof(ImportEntryMessageSendingActionCollection);
}
