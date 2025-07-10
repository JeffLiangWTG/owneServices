using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(MinimalSendingActionParent))]
	sealed class MinimalSendingActionParentTest : EMCSMessageSendingActionParentAbstractTest<MinimalSendingActionParent, EMCSMessageSendingAction>
	{
		protected override Type ExpectedSendingObjectCollectionType => typeof(MinimalSendingActionCollection);

		protected override BusinessObject GetNewBusinessObject() => new MinimalSendingActionParent(Factory.New<EMCSJobDeclaration>());
	}
}
