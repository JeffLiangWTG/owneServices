using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(ReportOfReceiptSendingAction))]
	sealed class ReportOfReceiptSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var reportOfReceipt = (ReportOfReceiptSendingAction)GetNewBusinessObject();
			AssertEquals("Report of Receipt", reportOfReceipt.HumanReadableName);
		}

		public void TestLookups()
		{
			var reportOfReceipt = (ReportOfReceiptSendingAction)GetNewBusinessObject();
			AssertType<ReportOfReceiptSendingActionLookups>(reportOfReceipt.Lookups);
		}

		public void TestValidation()
		{
			var reportOfReceipt = (ReportOfReceiptSendingAction)GetNewBusinessObject();
			AssertType<ReportOfReceiptSendingActionValidation>(reportOfReceipt.Validation);
		}

		public void TestMaxLength()
		{
			var reportOfReceipt = (ReportOfReceiptSendingAction)GetNewBusinessObject();
			AssertEquals(350, reportOfReceipt.ComplementaryInformationInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject() => new ReportOfReceiptSendingAction(Factory.New<EMCSJobDeclaration>());
	}
}
