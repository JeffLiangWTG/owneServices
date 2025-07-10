using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(RefundApplicationMessageSendingActionParent))]
	class RefundApplicationMessageSendingActionParentTest : CusEntryHeaderMessageSendingActionParentTest<RefundApplicationMessageSendingActionParent, RefundApplicationMessageSendingAction>
	{
		protected override Type ExpectedSendingObjectCollectionType => typeof(RefundApplicationMessageSendingActionCollection);

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RefundApplicationMessageSendingActionParent(Factory.New<JobDeclaration>());
		}
	}
}
