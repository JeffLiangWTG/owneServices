using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(ReasonForShortageSendingActionParent))]
	sealed class ReasonForShortageSendingActionParentTest : EMCSMessageSendingActionParentAbstractTest<ReasonForShortageSendingActionParent, ReasonForShortageSendingAction>
	{
		protected override Type ExpectedSendingObjectCollectionType => typeof(ReasonForShortageSendingActionCollection);

		protected override BusinessObject GetNewBusinessObject() => new ReasonForShortageSendingActionParent(Factory.New<EMCSJobDeclaration>());
	}
}
