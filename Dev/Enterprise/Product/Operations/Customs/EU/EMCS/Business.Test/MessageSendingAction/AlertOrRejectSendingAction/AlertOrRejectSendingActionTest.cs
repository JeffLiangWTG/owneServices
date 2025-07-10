using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(AlertOrRejectSendingAction))]
	sealed class AlertOrRejectSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRejectedFlag()
		{
			AssertEquals("Rejected?", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.RejectedFlagInfo).Caption);

			messageSendingAction.RejectedFlag = true;
			AssertEquals(true, messageSendingAction.RejectedFlag);

			messageSendingAction.RejectedFlag = false;
			AssertEquals(false, messageSendingAction.RejectedFlag);
		}

		public void TestDateOfAlertOrRejection()
		{
			AssertEquals("Date", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.DateOfAlertOrRejectionInfo).Caption);
			AssertEquals(ZDateTime.Empty, messageSendingAction.DateOfAlertOrRejection);

			var dateOfAlertOrRejection = new ZDateTime(2020, 01, 08);
			messageSendingAction.DateOfAlertOrRejection = dateOfAlertOrRejection;
			AssertEquals(dateOfAlertOrRejection, messageSendingAction.DateOfAlertOrRejection);
		}

		public void TestAlertOrRejectionReasons()
		{
			CombineAssertions(() =>
			{
				for (var i = 1; i < 6; i++)
				{
					messageSendingAction.AlertOrRejectionReasons.AddNew();
				}
				var alertOrRejectionReasons = messageSendingAction.AlertOrRejectionReasons;
				AssertEquals("5 records", 5, alertOrRejectionReasons.Count);
				AssertSame(alertOrRejectionReasons, messageSendingAction.AlertOrRejectionReasons);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			messageSendingAction = new AlertOrRejectSendingAction(Factory.New<EMCSJobDeclaration>());
		}

		protected override BusinessObject GetNewBusinessObject() => new AlertOrRejectSendingAction(Factory.New<EMCSJobDeclaration>());

		AlertOrRejectSendingAction messageSendingAction;
	}
}
