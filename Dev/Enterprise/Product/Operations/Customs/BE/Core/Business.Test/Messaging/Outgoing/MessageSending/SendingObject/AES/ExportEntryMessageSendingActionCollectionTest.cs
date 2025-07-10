using System;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(ExportEntryMessageSendingActionCollection))]
sealed class ExportEntryMessageSendingActionCollectionTest : BEJobDeclarationMessageSendingObjectCollectionTest<ExportDeclarationMessageSendingActionParent, ExportEntryMessageSendingActionCollection, ExportEntryMessageSendingAction>
{
	protected override Type ExpectedSendingObjectCollectionType => typeof(ExportEntryMessageSendingAction);
}
