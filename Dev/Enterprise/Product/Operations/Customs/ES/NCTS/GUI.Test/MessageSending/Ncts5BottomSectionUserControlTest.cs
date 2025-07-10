using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	sealed class Ncts5BottomSectionUserControlTest : TestCaseWithFactory
	{
		public void TestCancellationReasonTextBox()
		{
			AssertType<ZTextBox>(control.CancellationReasonTextBox);
		}

		[RequiresSTA]
		public void TestCancellationReason()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(new NctsMessageSendingObject(header, GlbStaff.CurrentUser), SendingType.None);

			using (var form = new MessageSendingForm(messageSendingObjectParent))
			{
				form.Show();

				var sendingObject = form.BusinessEntity.SendingObjectsCollection[0];
				form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox").Checked = true;
				form.FindSingle<ZButton>("SendButton").PerformClick();
				AssertCancellationReasonNoUserNotification(form);
			}

			using (var form = new MessageSendingForm(messageSendingObjectParent))
			{
				form.Show();

				var sendingObject = form.BusinessEntity.SendingObjectsCollection[0];
				header.MovementHeader.BM_CustomsStatus = "DGP";
				sendingObject.MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation;

				form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox").Checked = true;
				var sendBtn = form.FindSingle<ZButton>("SendButton");
				sendBtn.PerformClick();
				AssertCancellationReasonWithUserNotification(form);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendingObject.ReasonForCancellation = "Some reason";
				sendBtn.PerformClick();
				AssertCancellationReasonNoUserNotification(form);
			}
		}

		void AssertCancellationReasonWithUserNotification(ZChildForm form)
		{
			CombineAssertions(() =>
			{
				AssertContains("Cancellation reason is needed", "Please enter a cancellation reason.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.None, form.DialogResult);
			});
		}

		void AssertCancellationReasonNoUserNotification(ZChildForm form)
		{
			CombineAssertions(() =>
			{
				AssertNull("No user notification", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.OK, form.DialogResult);
			});
		}

		[RequiresSTA]
		public void TestCancellationVisibility()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(new NctsMessageSendingObject(header, GlbStaff.CurrentUser));
			using (var form = new MessageSendingForm(messageSendingObjectParent))
			{
				form.Show();

				var grid = (ZGrid)form.Controls.Find("MessageSendingObjectsGrid", true)[0];

				grid.ListManager.Position = 0;
				Application.DoEvents();
				var action = (NctsHeaderMessageSendingObject)grid.ListManager.Current;

				CombineAssertions(() =>
				{
					action.MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation;
					var cancellationGroupBox = form.Controls.Find("CancellationGroupBox", true)[0];
					AssertEquals("CancellationGroupBox is visible when Message Type is DPC", true, cancellationGroupBox.Visible);

					action.MessageType = DeclarationMessageTypeList.Codes.Ncts5Departure;
					cancellationGroupBox = form.Controls.Find("CancellationGroupBox", true)[0];
					AssertEquals("CancellationGroupBox is not visible when Message Type is not DPC", false, cancellationGroupBox.Visible);
				});
			}
		}

		[RequiresSTA]
		public void TestRequestDispatchDropEdit()
		{
			AssertType<ZDropEdit>(control.RequestDispatchDropEdit);
		}

		[RequiresSTA]
		public void TestRequestDispatchVisibility()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(new NctsMessageSendingObject(header, GlbStaff.CurrentUser));
			using (var form = new MessageSendingForm(messageSendingObjectParent))
			{
				form.Show();

				var grid = (ZGrid)form.Controls.Find("MessageSendingObjectsGrid", true)[0];

				grid.ListManager.Position = 0;
				Application.DoEvents();
				var action = (NctsHeaderMessageSendingObject)grid.ListManager.Current;

				CombineAssertions(() =>
				{
					action.MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes;
					var requestDispatchDropEdit = form.Controls.Find("RequestDispatchDropEdit", true)[0];
					AssertEquals("RequestDispatchDropEdit is visible when Message Type is DPA", true, requestDispatchDropEdit.Visible);

					action.MessageType = DeclarationMessageTypeList.Codes.Ncts5Departure;
					requestDispatchDropEdit = form.Controls.Find("RequestDispatchDropEdit", true)[0];
					AssertEquals("RequestDispatchDropEdit is not visible when Message Type is not DPA", false, requestDispatchDropEdit.Visible);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new Ncts5BottomSectionUserControl();
		}
		Ncts5BottomSectionUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
