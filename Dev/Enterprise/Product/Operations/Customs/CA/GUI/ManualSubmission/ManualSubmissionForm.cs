using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using IManualSubmissionSupport = Enterprise.Integration.Customs.CA.IManualSubmissionSupport;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class ManualSubmissionForm : ZChildForm
	{
		#region Constructor

		public ManualSubmissionForm()
		{
		}

		public ManualSubmissionForm(IManualSubmissionSupport support, string messageType, BusinessObjectFactory factory)
			: base(new ManualSubmissionBO(support, messageType, factory))
		{
			this.support = Argument.NotNull(support, "support");
			ButtonDeleteRELSubmission.Enabled = !BusinessEntity.CurrentEntrySubmissionBO.ManualSubmissionDate.IsEmpty;
		}

		readonly IManualSubmissionSupport support;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormHeading => Res.GetString("E7209E17-5B5B-4EAD-8BFF-DDB6FC930038", "Manual Submission");

		#endregion

		#region Properties

		public new ManualSubmissionBO BusinessEntity
		{
			get { return base.BusinessEntity as ManualSubmissionBO; }
		}

		#endregion

		#region Methods

		void OnButtonSaveSubmissionClick(object sender, EventArgs eventArgs)
		{
			if (IsValidateSupportBeforSaving())
			{
				if (ValidateAndSave() == ContinueWithSave.Yes)
				{
					DialogResult = System.Windows.Forms.DialogResult.OK;
					Close();
				}
			}
			else
			{
				ShowErrorAndClose();
			}
		}

		protected override void SaveInternal()
		{
			support.ManualSubmission(BusinessEntity.GetCusEntrySubmissionBOs());
			base.SaveInternal();
		}

		void OnButtonDeleteRelSubmissionClick(object sender, EventArgs eventArgs)
		{
			if (IsValidateSupportBeforSaving())
			{
				var entrySubmissionBo = BusinessEntity.CurrentEntrySubmissionBO;
				if (entrySubmissionBo != null)
				{
					var entryType = entrySubmissionBo.MessageType;
					var confirmationMessage = Res.GetString(
						"03900BB3-DC7C-4C22-8730-20E13D21816F",
						"Do you want to remove manual submission data on the {0} entry?",
						entryType);

					var confirmCaption = Res.GetString("0F6B622B-0E8F-46D9-B0AA-7826AE56B1AE", "Confirm");

					if (DialogResult.Yes == Globals.Message.Show(
						confirmationMessage,
						confirmCaption,
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question,
						DialogResult.No))
					{
						entrySubmissionBo.Delete();
						if (ValidateAndSave() == ContinueWithSave.Yes)
						{
							DialogResult = System.Windows.Forms.DialogResult.OK;
							Close();
						}
					}
				}
			}
			else
			{
				ShowErrorAndClose();
			}
		}

		void OnButtonCloseClick(object sender, EventArgs eventArgs)
		{
			Close();
		}

		bool IsValidateSupportBeforSaving()
		{
			var bo = support as BusinessObject;
			if (bo != null)
			{
				bo.RunPreSaveValidationWithFetchHints();
			}
			return bo == null || !bo.HasErrors;
		}

		void ShowErrorAndClose()
		{
			Globals.Message.ShowError(Res.GetString("A829F1D7-93D1-40E3-A609-EDA03B5A83AC", "Please clear all errors before saving."));
			Close();
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			var result = ContinueWithSave.No;
			try
			{
				result = base.ValidateAndSave();
			}
			catch (ZSaveConcurrencyException ex)
			{
				HandleSaveException(ex);
			}
			return result;
		}

		#endregion
	}
}
