using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Contra
{
	public partial class ContraForm : AccountingZForm
	{
		public ContraForm(Business.ARAP.Contra contra) : base(contra)
		{
			if (contra.AreContraRowsInDB)
			{
				HideControls();
			}
			else
			{
				HideCreatingUserAndDate();
			}

			AH_NumberOfSupportingDocumentsCalcEdit.Visible = contra.ARRow.AH_NumberOfSupportingDocumentsVisible_ReadOnly;
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);

			ZFormMenuStrategy.SetMenuItemText(this, ZFormMenuStrategy.FileSaveAndCloseMenuItemName, Res.GetString("Accounting|ContraForm|FileSaveAndCloseMenuItemName", "S&ave"));
		}

		IContainer components;

		#region Implementation

		#region System stuff

		Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		ZGroupBox PayGroupBox;
		ZCalcFindBox AH_OSTotalBoundCalcEdit;
		protected internal ZGroupBox ARGroupBox;
		protected internal ZGroupBox AccountsPayableGroupBox;
		ZDateEdit PostDateDateEdit;
		ZArchitecture.ZTextBox ContraNumberEdit;
		ZExchangeRateControl ExchangeRateControl;
		ZArchitecture.ZTextBox DescriptionTextEdit;
		ZDateEdit InvoiceDateDateEdit;
		ZCalcFindBox LocalAmountEdit;
		ZCalcFindBox AH_Calc_RecAfterContraEdit;
		ZCalcFindBox AH_Calc_RecBeforeContraEdit;
		ZGuidFindBox ARAccountBindBox;
		ZCalcFindBox AH_Calc_PayAfterContraEdit;
		ZCalcFindBox AH_Calc_PayBeforeContraEdit;
		ZGuidFindBox AH_APAccountFindBox;
		ZTabPage ContraTabPage;
		ZLogsTabPage zEventTabPage1;
		ZArchitecture.ZCalcEdit AH_NumberOfSupportingDocumentsCalcEdit;
		ZTemplateTabControl MainTabControl;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (IsReversingMode && !BusinessEntity.Factory.HasContext(BusinessContext.ReverseDateForm))
			{
				BusinessEntity.Factory.SetContext(BusinessContext.ReverseDateForm);
			}
		}
		#endregion

		protected void HideControls() // for View Form
		{
			ControlDpiScalingHelper.SetHeight(ref ARGroupBox, ARAccountBindBox.Top + ARAccountBindBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(8), false);
			AH_Calc_RecBeforeContraEdit.GetExtension<LabelCaptionRenderer>().Visible = false;
			AH_Calc_RecBeforeContraEdit.Visible = false;
			AH_Calc_RecAfterContraEdit.GetExtension<LabelCaptionRenderer>().Visible = false;
			AH_Calc_RecAfterContraEdit.Visible = false;

			ControlDpiScalingHelper.SetHeight(ref AccountsPayableGroupBox, AH_APAccountFindBox.Top + AH_APAccountFindBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(8), false);
			AH_Calc_PayBeforeContraEdit.GetExtension<LabelCaptionRenderer>().Visible = false;
			AH_Calc_PayBeforeContraEdit.Visible = false;
			AH_Calc_PayAfterContraEdit.GetExtension<LabelCaptionRenderer>().Visible = false;
			AH_Calc_PayAfterContraEdit.Visible = false;

			ControlDpiScalingHelper.SetTop(ref AccountsPayableGroupBox, ARGroupBox.Top + ARGroupBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(8), false);
			int newHeight = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(this.ClientSize.Height) - (ControlDpiScalingHelper.UnscaleFromCurrentDpiY(AH_Calc_RecBeforeContraEdit.Height * 2) + 8);
			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(this.ClientSize.Width), newHeight);

			//this.ClientSize = new Size(this.ClientSize.Width, AccountsPayableGroupBox.Bottom + ButtonsUserControl.Height + MainStatusBar.Height + 56);
		}

		protected void HideCreatingUserAndDate() // for New Form
		{
			//this.ClientSize = new Size(this.ClientSize.Width, AccountsPayableGroupBox.Bottom + ButtonsUserControl.Height + MainStatusBar.Height + 30);
			//ButtonsUserControl.Top = AccountsPayableGroupBox.Top + AccountsPayableGroupBox.Height + 8;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			MainTabControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
		}

		protected override bool ShowAuditTab => true;

		#endregion
	}
}

