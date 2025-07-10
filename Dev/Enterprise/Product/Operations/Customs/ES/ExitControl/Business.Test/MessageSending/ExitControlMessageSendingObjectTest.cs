using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	[TestedType(typeof(ExitControlMessageSendingObject))]
	class ExitControlMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestExitReport()
		{
			var sendingObject = (ExitControlMessageSendingObject)GetNewBusinessObject();
			AssertSame(exitReport, sendingObject.MessagingObject);
		}

		public void TestSendColumn()
		{
			CombineAssertions(() =>
			{
				exitReport.CER_MessageStatus = "ACC";
				testItem = new ExitControlMessageSendingObject(exitReport);
				AssertEquals("With message status ACC report and customs status Empty", false, testItem.ShouldSendInfo.ReadOnly);

				exitReport.CER_MessageStatus = "SNT";
				testItem = new ExitControlMessageSendingObject(exitReport);
				AssertEquals("With message status SNT report and customs status Empty", true, testItem.ShouldSendInfo.ReadOnly);

				exitReport.CER_MessageStatus = ZString.Empty;
				testItem = new ExitControlMessageSendingObject(exitReport);
				AssertEquals("With message status empty report and customs status Empty", false, testItem.ShouldSendInfo.ReadOnly);

				exitReport.CER_Status = "AAA";
				testItem = new ExitControlMessageSendingObject(exitReport);
				AssertEquals("With message status empty report and customs status not Empty", true, testItem.ShouldSendInfo.ReadOnly);

				exitReport.CER_Status = ZString.Empty;
				testItem = new ExitControlMessageSendingObject(exitReport);
				AssertEquals("With message status empty report and customs status is Empty", false, testItem.ShouldSendInfo.ReadOnly);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExitControlMessageSendingObject(exitReport);
		}

		protected override void SetUp()
		{
			base.SetUp();
			exitReport = Factory.New<CusExitReport>();
			testItem = new ExitControlMessageSendingObject(exitReport);
		}
		CusExitReport exitReport;
		ExitControlMessageSendingObject testItem;
	}
}
