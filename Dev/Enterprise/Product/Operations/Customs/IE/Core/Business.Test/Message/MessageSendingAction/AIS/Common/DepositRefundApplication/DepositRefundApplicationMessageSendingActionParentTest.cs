using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(DepositRefundApplicationMessageSendingActionParent))]
	sealed class DepositRefundApplicationMessageSendingActionParentTest : CusEntryHeaderMessageSendingActionParentTest<DepositRefundApplicationMessageSendingActionParent, DepositRefundApplicationMessageSendingAction>
	{
		protected override Type ExpectedSendingObjectCollectionType => typeof(DepositRefundApplicationMessageSendingActionCollection);

		protected override BusinessObject GetNewBusinessObject() => new DepositRefundApplicationMessageSendingActionParent(Factory.New<JobDeclaration>());
	}
}
