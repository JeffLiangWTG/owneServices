using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(ExplanationOnDelaySendingActionParent))]
	sealed class ExplanationOnDelaySendingActionParentTest : EMCSMessageSendingActionParentAbstractTest<ExplanationOnDelaySendingActionParent, ExplanationOnDelaySendingAction>
	{
		protected override Type ExpectedSendingObjectCollectionType => typeof(ExplanationOnDelaySendingActionCollection);

		protected override BusinessObject GetNewBusinessObject() => new ExplanationOnDelaySendingActionParent(Factory.New<EMCSJobDeclaration>());
	}
}
