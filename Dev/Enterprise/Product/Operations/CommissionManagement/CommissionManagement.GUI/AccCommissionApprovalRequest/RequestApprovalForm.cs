using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CommissionManagement.GUI
{
	public partial class RequestApprovalForm : ZChildForm
	{
		#region Constructors

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public RequestApprovalForm()
		{
		}

		public RequestApprovalForm(AccCommissionApprovalRequest approvalRequest)
			: base(approvalRequest)
		{
			AddMenuItems();
		}

		#endregion

		#region Initialize

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#endregion

		#region Form Caption

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Menu Items

		void AddMenuItems()
		{
			if (GlbStaff.CurrentUser.IsSupportUser)
			{
				Menu = new ZMainMenu();
				IFileMenuItemsProvider menuItemsProvider = this;
				menuItemsProvider.MainMenu = Menu;
				PlugIns.Add(ControllerIDs.DocDataPlugIn);

				ControlDpiScalingHelper.SetHeight(this, this.Height + SystemInformation.CaptionHeight, false);
			}
		}

		#endregion

		#region BusinessEntity

		new AccCommissionApprovalRequest BusinessEntity
		{
			get { return (AccCommissionApprovalRequest)base.BusinessEntity; }
		}

		#endregion

		#region Buttons

		void PostButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();

			if (BusinessEntity.HasErrors)
			{
				ShowErrorsDialog();
				DialogResult = DialogResult.None;
				return;
			}

			if (IncludeSummaryAsEmailAttachmentCheckBox.Checked && BusinessEntity.EmailSender.CommissionApprovalRequestTemplate == null)
			{
				Globals.Message.ShowError(
					Res.GetString("2ab027e9-21b5-4adc-b910-0f83799ae590", "Could not find Commission Approval Request Template to use for email."),
					Res.GetString("8ba853de-9a8e-43f3-b649-1f332c9c3d89", "Unable to post this Commission Approval Request..."));

				DialogResult = DialogResult.None;
				return;
			}

			try
			{
				BusinessEntity.Factory.Save();
			}
			catch (ZSaveException ex)
			{
				HandleSaveException(ex);
			}

			Close();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Save

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation,
				businessEntityForValidation.HumanReadableName,
				Res.GetString("12bb5bc2-007b-4c33-80c1-d43819f06008", "post"),
				Res.GetString("82b41afd-8ada-41c5-9b98-9a1e44d7a74f", "posted"),
				includeIgnoreOption);
		}

		#endregion
	}
}
