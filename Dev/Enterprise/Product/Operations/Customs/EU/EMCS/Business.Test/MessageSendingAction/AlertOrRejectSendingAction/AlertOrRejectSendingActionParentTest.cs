using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(AlertOrRejectSendingActionParent))]
	sealed class AlertOrRejectSendingActionParentTest : EMCSMessageSendingActionParentAbstractTest<AlertOrRejectSendingActionParent, AlertOrRejectSendingAction>
	{
		protected override Type ExpectedSendingObjectCollectionType => typeof(AlertOrRejectSendingActionCollection);

		protected override BusinessObject GetNewBusinessObject() => new AlertOrRejectSendingActionParent(Factory.New<EMCSJobDeclaration>());
	}
}
