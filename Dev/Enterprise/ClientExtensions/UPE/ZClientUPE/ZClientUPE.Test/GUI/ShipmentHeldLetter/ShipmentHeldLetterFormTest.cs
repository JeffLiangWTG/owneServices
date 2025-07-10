using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(ShipmentHeldLetterForm))]
	public class ShipmentHeldLetterFormTest : ZFormBasherTest
	{
		public void TestFormHeadingText()
		{
			using (ShipmentHeldLetterForm form = (ShipmentHeldLetterForm)GetFormToBashCore())
			{
				AssertEquals("Form heading", "Customer Notification", form.Text);
			}
		}

		public void TestHeadingLabelText()
		{
			TestHeadingLabelText(ShipmentHeldLetterRecipient.Unknown, "Please confirm Customer Notification details");
			TestHeadingLabelText(ShipmentHeldLetterRecipient.Consignee, "Customer Notification for Consignee");
			TestHeadingLabelText(ShipmentHeldLetterRecipient.Consignor, "Customer Notification for Consignor");
		}

		void TestHeadingLabelText(ShipmentHeldLetterRecipient recipient, string caption)
		{
			ShipmentHeldLetterBusinessObject bizObj = new ShipmentHeldLetterBusinessObject(CusHAWB);
			using (TestShipmentHeldLetterForm form = new TestShipmentHeldLetterForm(bizObj, recipient))
			{
				AssertEquals(caption, form.HeadingLabel.Text);
			}
		}

		public void TestDeliveryMethodGroupBoxVisibility()
		{
			TestDeliveryMethodGroupBoxVisibility("DeliveryMethodGroupBox should be hidden for 'Unknown' recipient, it is designed to confirm details for auto-delivery only", ShipmentHeldLetterRecipient.Unknown, false);
			TestDeliveryMethodGroupBoxVisibility("DeliveryMethodGroupBox visible for 'Consignee' recipient", ShipmentHeldLetterRecipient.Consignee, true);
			TestDeliveryMethodGroupBoxVisibility("DeliveryMethodGroupBox visible for 'Consignor' recipient", ShipmentHeldLetterRecipient.Consignor, true);
		}

		void TestDeliveryMethodGroupBoxVisibility(ZString failureMessage, ShipmentHeldLetterRecipient recipient, bool expectDeliveryMethodGroupBoxVisible)
		{
			using (TestShipmentHeldLetterForm form = new TestShipmentHeldLetterForm(CusHAWB.ShipmentHeldLetterDetails, recipient))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(failureMessage, !expectDeliveryMethodGroupBoxVisible, form.DeliveryMethodGroupBox.IsDisposed);
			}
		}

		public void TestQueueForBatchPrintBoundCheckBoxText()
		{
			using (TestShipmentHeldLetterForm form = new TestShipmentHeldLetterForm(CusHAWB.ShipmentHeldLetterDetails, ShipmentHeldLetterRecipient.Consignee))
			{
				AssertEquals("Queue for Batch Print (Batch #1)", form.QueueForBatchPrintBoundCheckBox.Text);
			}
		}

		public void TestSetDefaultsForAutoDelivery_WhenRecipientTypeUnknown()
		{
			CusHAWB.CurrentQueue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.EIR;
			CusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription;
			using (TestShipmentHeldLetterForm form = new TestShipmentHeldLetterForm(CusHAWB.ShipmentHeldLetterDetails, ShipmentHeldLetterRecipient.Unknown))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("When confirming auto-delivery details (Recipient=Unknown), default the reason before showing the form", ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription, form.BusinessEntity.ReasonCode);
			}
		}

		public void TestCancelButton_UserEditsReverted()
		{
			using (TestShipmentHeldLetterForm form = new TestShipmentHeldLetterForm(CusHAWB.ShipmentHeldLetterDetails, ShipmentHeldLetterRecipient.Consignee))
			{
				form.BusinessEntity.ReasonText = "original";
				form.Show();
				Application.DoEvents();
				form.BusinessEntity.ReasonText = "user_editted";
				form.CloseButton.PerformClick();
				AssertEquals("Form should be closed after Cancel is clicked", true, form.IsDisposed);
				AssertEquals("User edits should be reverted after cancel is clicked", "original", ((ShipmentHeldLetterBusinessObject)form.LastDataSourceForTest).ReasonText);
			}
		}

		public void TestOKButton_UserEditsCommitted()
		{
			using (TestShipmentHeldLetterForm form = new TestShipmentHeldLetterForm(CusHAWB.ShipmentHeldLetterDetails, ShipmentHeldLetterRecipient.Consignee))
			{
				form.BusinessEntity.ReasonText = "original";
				form.Show();
				Application.DoEvents();
				PopulateDataRequiredToPassValidation(form.BusinessEntity);
				form.BusinessEntity.ReasonText = "user_editted";
				form.OKButton.PerformClick();
				AssertEquals("Form should be closed after OK is clicked", true, form.IsDisposed);
				AssertEquals("User edits should remain after OK is clicked", "user_editted", ((ShipmentHeldLetterBusinessObject)form.LastDataSourceForTest).ReasonText);
			}
		}

		public void TestOKButton_WithNoValidationErrors()
		{
			ShipmentHeldLetterBusinessObjectWithoutValidation bizObj = new ShipmentHeldLetterBusinessObjectWithoutValidation(CusHAWB);
			using (TestShipmentHeldLetterForm form = new TestShipmentHeldLetterForm(bizObj, ShipmentHeldLetterRecipient.Unknown))
			{
				form.Show();
				Application.DoEvents();
				form.OKButton.PerformClick();
				Application.DoEvents();
				AssertEquals("Form should close when OK is clicked with no validation errors", false, form.Visible);
				AssertEquals("Form should return the correct DialogResult", DialogResult.OK, form.DialogResult);
			}
		}

		public void TestOKButton_WithValidationErrors()
		{
			ShipmentHeldLetterBusinessObject bizObj = new ShipmentHeldLetterBusinessObject(CusHAWB);
			using (TestShipmentHeldLetterForm form = new TestShipmentHeldLetterForm(bizObj, ShipmentHeldLetterRecipient.Unknown))
			{
				form.Show();
				Application.DoEvents();
				form.OKButton.PerformClick();
				Application.DoEvents();
				AssertEquals("Form should not close when there are validation errors", true, form.Visible);
				AssertEquals("Form DialogResult should be reverted to None instead of OK", DialogResult.None, form.DialogResult);
			}
		}

		#region Test Classes
		class TestShipmentHeldLetterForm : ShipmentHeldLetterForm
		{
			public TestShipmentHeldLetterForm(ShipmentHeldLetterBusinessObject businessEntity, ShipmentHeldLetterRecipient recipient) : base(businessEntity, recipient)
			{
			}

			public new ZButton OKButton
			{
				get
				{
					return base.OKButton;
				}
			}

			public new ZButton CloseButton
			{
				get
				{
					return base.CloseButton;
				}
			}

			public new ZLabel HeadingLabel
			{
				get
				{
					return base.HeadingLabel;
				}
			}

			public new ZGroupBox DeliveryMethodGroupBox
			{
				get
				{
					return base.DeliveryMethodGroupBox;
				}
			}

			public new ZTextBox ReasonTextBoundTextBox
			{
				get
				{
					return base.ReasonTextBoundTextBox;
				}
			}

			public new ZCheckBox QueueForBatchPrintBoundCheckBox
			{
				get
				{
					return base.QueueForBatchPrintBoundCheckBox;
				}
			}
		}

		class ShipmentHeldLetterBusinessObjectWithoutValidation : ShipmentHeldLetterBusinessObject
		{
			public ShipmentHeldLetterBusinessObjectWithoutValidation(UPECusHAWB cusHAWB) : base(cusHAWB)
			{
			}

			protected override void RunPreSaveValidationCore()
			{
				// no validation
				ClearAllNotifications();
			}
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		UPECusHAWB CusHAWB
		{
			get
			{
				if (fCusHAWB == null)
				{
					fCusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
				}

				return fCusHAWB;
			}
		}

		UPECusHAWB fCusHAWB;
		void PopulateDataRequiredToPassValidation(ShipmentHeldLetterBusinessObject businessEntity)
		{
			businessEntity.UPSContactName = "kinty";
			businessEntity.UPSContactPhone = "9025 1173";
			businessEntity.RunPreSaveValidation();
			AssertEquals("Should not have errors for the test", false, businessEntity.HasErrors);
		}

		protected override Form GetFormToBashCore()
		{
			ShipmentHeldLetterBusinessObject bizObj = new ShipmentHeldLetterBusinessObject(CusHAWB);
			return new ShipmentHeldLetterForm(bizObj, ShipmentHeldLetterRecipient.Unknown);
		}
		#endregion
	}
}
