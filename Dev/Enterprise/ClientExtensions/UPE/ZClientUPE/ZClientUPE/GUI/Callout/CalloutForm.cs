using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.GUI
{
	public partial class CalloutForm : CusHAWBForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		[Obsolete("Design-time only", true)]
		public CalloutForm() { }

		public CalloutForm(Callout businessEntity) : base(businessEntity)
		{
			if (ShowProcessQueuePlugIn)
			{
				PlugIns.Add(ControllerIDs.ProcessQueue);
				SetActiveProcessQueue();
			}

			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			HookEvents();
			MainTabControl.SelectedIndexChanged += new EventHandler(MainTabControl_SelectedIndexChanged);
		}

		public new Callout BusinessEntity
		{
			get { return (Callout)base.BusinessEntity; }
		}

		void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			BISIDownloadNotRequiredOrForcedCaptionTextBox.Bounds = BisiDownloadDateEdit.Bounds;
			BISIDownloadNotRequiredOrForcedCaptionTextBox.BringToFront();
			BISIDownloadNotRequiredOrForcedCaptionTextBox.Visible = !Callout.BISIDownloadNotRequiredOrForcedCaption.IsEmpty;
		}

		public override string FormCaption
		{
			get { return Callout == null ? "" : "Finance Item - " + Callout.CS_HAWB; }
		}

		protected virtual bool ShowProcessQueuePlugIn
		{
			get { return true; }
		}

		protected override void OnLoad(EventArgs e)
		{
			if (!this.IsDesignMode())
			{
				base.OnLoad(e);
				DisableNewAction();
				ShowAppropriateTab();

				RefundEnquiryButton.Visible = Callout.Refund == null;

				if (Callout.RefundManager != null && Callout.Refund == null)
				{
					Callout.RefundManager.OnRefundCreated += (r) =>
						r.T10_ControlNumberInfo.ValueChanged += delegate
						{
							RefundEnquiryButton.Visible = Callout.Refund.T10_ControlNumber.IsEmpty;
							zLabelRefundEnquiry.Text = Callout.Refund.T10_ControlNumber;
						};
				}
				BeginInvoke(new MethodInvoker(ShowAlertForm));
			}
		}

		void ShowAlertForm()
		{
			if (Callout.HasAlerts)
			{
				alertForm = new AlertForm(new Alert(Callout.AlertsList, BusinessEntity.Factory));
				alertForm.Show();
			}
		}

		void SetActiveProcessQueue()
		{
			if (Callout.ActiveProcessQueueForBinding.Count > 0)
			{
				Callout.ActiveProcessQueueForBinding[0].QueueType = ProcessQueueType.Enum.Commercial;
			}
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control.Name == "LineChargesGrid" && previousControl.Name == "BisiDownloadDateEdit";
		}

		protected AlertForm alertForm;

		#region Automatically generated

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeActionsMenuItem();

			BISIDownloadNotRequiredOrForcedCaptionTextBox.AllowOverlap(BisiDownloadDateEdit);
		}

		void InitializeActionsMenuItem()
		{
			if (ActionsMenuItem != null)
			{
				ActionsMenuItem.MenuItems.Add("View Entry Print", delegate { GetEntryPrintDocumentHelper().Run(Callout); });
				ActionsMenuItem.MenuItems.Add("-");
			}
		}

		#endregion

		#region Event Handlers

		CusHAWBGuiEventHandlers GuiEventHelper;

		void HookEvents()
		{
			GuiEventHelper = GetCusHAWBGuiEventHandlers();
			GuiEventHelper.HookEvents();
			Callout.Payment.PaymentMethodChanged += new PaymentMethodChangedEventHandler(OnCallout_PaymentMethodChanged);
			UPEProcessQueueGuiEventHelper = new UPEProcessQueueGuiEventHandlers(BusinessEntity.CurrentQueue);
			UPEProcessQueueGuiEventHelper.HookEvents();
		}
		UPEProcessQueueGuiEventHandlers UPEProcessQueueGuiEventHelper;

		void UnhookEvents()
		{
			if (GuiEventHelper != null)
			{
				GuiEventHelper.UnhookEvents();
			}
			if (Callout != null)
			{
				Callout.Payment.PaymentMethodChanged -= new PaymentMethodChangedEventHandler(OnCallout_PaymentMethodChanged);
			}
			if (UPEProcessQueueGuiEventHelper != null)
			{
				UPEProcessQueueGuiEventHelper.UnhookEvents();
			}
		}

		protected virtual CusHAWBGuiEventHandlers GetCusHAWBGuiEventHandlers()
		{
			return new CusHAWBGuiEventHandlers(BusinessEntity);
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = GuiEventHelper.RunPreSaveDialogs(BusinessEntity);
			if (result == ContinueWithSave.Yes)
			{
				result = base.ValidateAndSave();
				Activate();
			}
			return result;
		}

		void OnCallout_PaymentMethodChanged(object sender, PaymentMethodChangedEventArgs e)
		{
			if (((UPECalloutQueue)Callout.CurrentQueue).AccountNumberAccountClass == "13")
			{
				if (e.PaymentMethod != UPECargoPaymentMethod.Cheque && e.PaymentMethod != UPECargoPaymentMethod.CreditCard && e.PaymentMethod != UPECargoPaymentMethod.EFT)
				{
					Globals.Message.ShowInformation("Payment method must be Cheque, Credit or EFT if Rebill Account Class is 13");
					e.Cancel = true;
				}
			}
			else
			{
				using (CalloutPaymentDetailsForm form = CalloutPaymentDetailsForm.New(Callout, e.PaymentMethod))
				{
					if (form != null)
					{
						e.Cancel = (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.Cancel);
					}
				}
			}
		}

		void OnRefundEnquiryButton_Click(object sender, EventArgs e)
		{
			if (Callout.RelatedOwner != null)
			{
				if (DialogResult.OK == ZFormModaliser.ShowDialogAndDispose(new RefundEnquiryForm(Callout.RefundWrapper)))
				{
					Callout.IsRefundEnquiry = Callout.HasChanges = true;
				}
			}
		}

		protected virtual EntryPrintDocumentHelper GetEntryPrintDocumentHelper()
		{
			return new EntryPrintDocumentHelper();
		}

		void PartPaymentButton_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new PartPaymentForm(Callout.CalloutPartPayment));
			if (Callout.CalloutPartPayment.MustAddNote)
			{
				Callout.HasChanges = true;
			}
		}

		#endregion

		#region Implementation

		protected MenuItem QueuePrintInvoiceMenuItem;

		Callout Callout
		{
			get { return BusinessEntity; }
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (e.KeyCode == Keys.D4 && FormModifierKeys == (Keys.Control | Keys.Alt))
			{
				Callout.EnsureJobHeaderExists();
				Callout.JobHeader.Charges.SetReadOnlyIncludingChildren(false);
				LineChargesGrid.ReadOnly = false;
			}
		}

		protected virtual Keys FormModifierKeys
		{
			get { return Form.ModifierKeys; }
		}

		#endregion

		#region Show Related Invoice Preferences Note

		void ShowAppropriateTab()
		{
			if (!ShowRelatedInvoicingPreferencesNote())
			{
				MainTabControl.SelectedTab = FinanceTabPage;
				FinanceTabPage.Focus();
			}
		}

		bool ShowRelatedInvoicingPreferencesNote()
		{
			bool result = false;
			StmNote invoicingPreferencesNote = Callout.GetInvoicingPreferencesNote();
			if (invoicingPreferencesNote != null)
			{
				UserIdleWorker.Flush();
				Callout.Notes.ShowRelatedNotes = true;
				//NotesTabPage.FocusOnTabPage();
				//NotesTabPage.SelectedNote = InvoicingPreferencesNote;
				Callout.AlertsList.Add("Invoicing Preferences Note Exists.");
				result = true;
			}
			return result;
		}

		#endregion

		#region Dispose

		System.ComponentModel.IContainer components;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (alertForm != null)
				{
					alertForm.Close();
					alertForm.Dispose();
				}

				UnhookEvents();
				if (GuiEventHelper != null)
				{
					GuiEventHelper.Dispose();
				}
				if (UPEProcessQueueGuiEventHelper != null)
				{
					UPEProcessQueueGuiEventHelper.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
