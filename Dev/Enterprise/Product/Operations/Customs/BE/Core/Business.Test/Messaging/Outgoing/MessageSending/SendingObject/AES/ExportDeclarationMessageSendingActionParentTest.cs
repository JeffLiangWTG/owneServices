using System;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(ExportDeclarationMessageSendingActionParent))]
sealed class ExportDeclarationMessageSendingActionParentTest : BEJobDeclarationMessageSendingObjectParentTest<ExportDeclarationMessageSendingActionParent, ExportEntryMessageSendingActionCollection, ExportEntryMessageSendingAction>
{
	protected override Type ExpectedSendingObjectCollectionType => typeof(ExportEntryMessageSendingActionCollection);
}
