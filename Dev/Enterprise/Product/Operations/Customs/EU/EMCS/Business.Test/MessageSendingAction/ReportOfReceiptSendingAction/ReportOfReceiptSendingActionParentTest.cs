using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(ReportOfReceiptSendingActionParent))]
	sealed class ReportOfReceiptSendingActionParentTest : EMCSMessageSendingActionParentAbstractTest<ReportOfReceiptSendingActionParent, ReportOfReceiptSendingAction>
	{
		protected override Type ExpectedSendingObjectCollectionType => typeof(ReportOfReceiptSendingActionCollection);

		protected override BusinessObject GetNewBusinessObject() => new ReportOfReceiptSendingActionParent(Factory.New<EMCSJobDeclaration>());
	}
}
