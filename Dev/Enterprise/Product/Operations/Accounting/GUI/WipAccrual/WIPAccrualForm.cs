using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.WipAccrual
{
	public abstract partial class WIPAccrualForm : ZForm, IButtonDeleteTextOverride
	{
		protected WIPAccrualForm()
		{
		}

		protected WIPAccrualForm(BaseWIPAccrual wIPAccrual)
			: base(wIPAccrual)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostButton, CloseButton);
			this.WipAccrual = wIPAccrual;
			this.Name = "WIPAndAccrualForm"; //this is set to the same name as WIPForm so that previous and next button will bring up forms in the same position.

			ZFormMenuStrategy.SetMenuItemText(this, ZFormMenuStrategy.FileSaveAndCloseMenuItemName, Res.GetString("Accounting|WIPAccrualForm|PostMenu", "&Post"));

			var dataExportBatchSource = BusinessEntity as IDataExportBatchSource;
			if (dataExportBatchSource != null && dataExportBatchSource.IsDataExportBatchSupported)
			{
				PlugIns.Add(ControllerIDs.DataExportBatchPlugin);
			}

			PlugIns.Add(ControllerIDs.Audit);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected void SetupReversing()
		{
			MakeReversingFieldsVisible();
			WipAccrual.SetModeToReversing();
		}

		protected override void DeleteCore()
		{
			// Do nothing
		}

		protected void MakeReversingFieldsVisible()
		{
			ReverseDateEdit.Visible = true;
		}

		protected override void SetReadOnlyIncludingChildren()
		{
			if (DisplayMode == ODisplayMode.Delete)
			{
				WipAccrual.ReadOnly = false;
				var writableProperties = new List<string>();
				if (WipAccrual.IsEditingReverseDateAllowed)
				{
					writableProperties.Add(nameof(WipAccrual.AL_ReverseDate));
				}
				WipAccrual.AddWritableProperties(writableProperties.ToArray());
			}
			else
			{
				base.SetReadOnlyIncludingChildren();
			}
		}

		protected internal BaseWIPAccrual WipAccrual;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (DisplayMode == ODisplayMode.Delete)
			{
				SetupReversing();
			}
			else if (DisplayMode == ODisplayMode.ReadOnly && WipAccrual.IsReversed)
			{
				MakeReversingFieldsVisible();
			}
		}

		string IButtonDeleteTextOverride.DeleteButtonText
		{
			get { return Res.GetString("Accounting|WIPAccrualForm|ReverseButton", "&Reverse"); }
		}

		public override string FormVerb
		{
			get
			{
				if (DisplayMode == ODisplayMode.Delete)
				{
					return Res.GetString("Accounting|WIPAccrualForm|FromVerbReverse", "Reverse");
				}
				else
				{
					return base.FormVerb;
				}
			}
		}

		protected override DialogResult ShowConfirmationForDelete()
		{
			return DialogResult.Yes;
		}

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			ContinueWithDelete result = ContinueWithDelete.Yes;

			WipAccrual.RunPreSaveValidation();

			if (WipAccrual.HasErrors)
			{
				result = ContinueWithDelete.No;
				ShowErrorsDialog();
			}

			return result;
		}

		protected override void HandleSaveException(Exception ex)
		{
			if (BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation))
			{
				this.DisplayMode = ODisplayMode.ReadOnly;
				SetReadOnlyIncludingChildren();
			}

			base.HandleSaveException(ex);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			WipAccrual.IsReversing = false;
			WipAccrual.Factory.RemoveContext(BusinessContext.SkipJobHeaderRefreshParentDuringWIPAccrualReversing);

			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}

