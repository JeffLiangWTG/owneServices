using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(CancellationSendingActionParent))]
	sealed class CancellationSendingActionParentTest : EMCSMessageSendingActionParentAbstractTest<CancellationSendingActionParent, CancellationSendingAction>
	{
		protected override Type ExpectedSendingObjectCollectionType => typeof(CancellationSendingActionCollection);

		protected override BusinessObject GetNewBusinessObject() => new CancellationSendingActionParent(Factory.New<EMCSJobDeclaration>());
	}
}
