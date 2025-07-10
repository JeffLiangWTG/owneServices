using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AISUCC5MessageSendingActionParent))]
	sealed class AISUCC5MessageSendingActionParentTest : CusEntryHeaderMessageSendingActionParentTest<AISUCC5MessageSendingActionParent, AISUCC5MessageSendingAction>
	{
		protected override Type ExpectedSendingObjectCollectionType => typeof(AISUCC5MessageSendingActionCollection);

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AISUCC5MessageSendingActionParent(Factory.New<JobDeclaration>());
		}
	}
}
