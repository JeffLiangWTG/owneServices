using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.EU.H7.Business.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	public abstract class MessageSendingFormTest : MessageSendingObjectFormTest
	{
		public void TestMessageSendingObjectsGridColumns()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var messageSendingObjectsGrid = form.Controls.Find("MessageSendingObjectsGrid", searchAllChildren: true).Single() as ZGrid;

				EUH7GUITestHelper.AssertGridLayout(messageSendingObjectsGrid, ExpectedGridColumnNames);
			}
		}

		public void TestPreviewMessageCheckboxVisible()
		{
			using (var form = GetFormToBash())
			{
				var previewMessageCheckboxVisibleProperty = typeof(MessageSendingForm).GetProperty("PreviewMessageCheckboxVisible", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
				AssertEquals("Should set to true", true, previewMessageCheckboxVisibleProperty.GetValue(form, null));
			}
		}

		[RequiresSTA]
		public void TestSelectAllButton()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var branch = header.Branch.Company.Branches.AddNew();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);

			using (var form = GetTestingForm(sendingObjectParent))
			{
				form.Show();
				sendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObjectForTest>().First().ShouldSend = false;

				var selectAllButton = form.Controls.Find("SelectAllButton", true).Single() as ZButton;
				AssertEquals("Button Text", "Select/Deselect All", selectAllButton.Text);

				selectAllButton.PerformClick();
				var selectedMessageSendingObjectCount = sendingObjectParent.SendingObjectsCollection.OfType<BaseMessageSendingObject>().Count(mso => mso.ShouldSend);
				AssertEquals("Button should select all if there are any non-selected object", 3, selectedMessageSendingObjectCount);

				selectAllButton.PerformClick();
				selectedMessageSendingObjectCount = sendingObjectParent.SendingObjectsCollection.OfType<BaseMessageSendingObject>().Count(mso => mso.ShouldSend);
				AssertEquals("Button should deselect all if all objects are selected", 0, selectedMessageSendingObjectCount);
			}
		}

		public virtual void TestOverrideDefaultMessageTypeControl()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);

			using (var form = GetTestingForm(sendingObjectParent))
			{
				form.Show();
				var control = form.Controls.Find("OverrideMessageTypeControl", true).FirstOrDefault() as OverrideUserControl;

				if (ExpectedAllowOverrideMessageType)
				{
					AssertNotNull("OverrideMessageTypeControl should be added", control);
					AssertEquals("CheckBox BindTo", "OverrideDefaultAction", control.BindToOverride);
					AssertEquals("DropEdit BindTo", "Action", control.BindToCode);
				}
				else
				{
					AssertNull("OverrideMessageTypeControl should not be added", control);
				}
			}
		}

		public virtual void TestOverrideAmendmentReasonControl()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);

			using (var form = GetTestingForm(sendingObjectParent))
			{
				form.Show();
				var control = form.Controls.Find("OverrideAmendmentReasonControl", true).FirstOrDefault() as OverrideUserControl;

				if (ExpectedAllowOverrideAmendmentReason)
				{
					AssertNotNull("OverrideAmendmentReasonControl should be added", control);
					AssertEquals("CheckBox BindTo", "OverrideAmendmentReason", control.BindToOverride);
					AssertEquals("DropEdit BindTo", "AmendmentReason", control.BindToCode);
				}
				else
				{
					AssertNull("OverrideAmendmentReasonControl should not be added", control);
				}
			}
		}

		public virtual void TestOverrideCancellationReasonControl()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);

			using (var form = GetTestingForm(sendingObjectParent))
			{
				form.Show();
				var control = form.Controls.Find("OverrideCancellationReasonControl", true).FirstOrDefault() as OverrideUserControl;

				if (ExpectedAllowOverrideCancellationReason)
				{
					AssertNotNull("OverrideCancellationReasonControl should be added", control);
					AssertEquals("CheckBox BindTo", "OverrideCancellationReason", control.BindToOverride);
					AssertEquals("DropEdit BindTo", "CancellationReason", control.BindToCode);
				}
				else
				{
					AssertNull("OverrideCancellationReasonControl should not be added", control);
				}
			}
		}

		[RequiresSTA]
		public void TestMessageSendingGridColumnLayoutProvider()
		{
			using (var form = GetFormToBash())
			{
				var layoutProvider = typeof(MessageSendingForm).GetField("messageSendingGridColumnLayoutProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(form);
				AssertEquals(MessageSendingGridColumnLayoutType, layoutProvider.GetType());
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var testingParent = new MessageSendingObjectParent<MessageSendingObject>(header);
			return new MessageSendingForm(testingParent);
		}

		protected virtual IReadOnlyList<string> ExpectedGridColumnNames => new[] { "ShouldSend", "BillNumber", "Action", "SubStyle", "LocalReferenceNumber", "MRN", "MessageStatus", "CustomsStatus" };

		protected virtual Form GetTestingForm(BaseMessageSendingObjectParent testingParent) => new MessageSendingForm(testingParent);

		protected virtual Type MessageSendingGridColumnLayoutType => typeof(MessageSendingGridColumnLayout);

		public override Type FormToBashType => typeof(MessageSendingForm);

		protected virtual bool ExpectedAllowOverrideMessageType => true;

		protected virtual bool ExpectedAllowOverrideAmendmentReason => false;

		protected virtual bool ExpectedAllowOverrideCancellationReason => false;
	}
}
