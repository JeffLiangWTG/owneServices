using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class LPCOGridUserControl : ZUserControl
	{
		public LPCOGridUserControl()
		{
			InitializeComponent();
		}

		internal void RemoveExceptAvailableColumns(IEnumerable<string> availableColumnsNames)
		{
			if (availableColumnsNames != null)
			{
				var irrelevantColumns = GetIrrelevantColumns(availableColumnsNames);
				using (LpcoGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					LpcoGrid.RemoveFromAvailableColumns(irrelevantColumns.ToArray());
					LpcoGrid.RefreshTableStyles();
				}
			}
		}

		static IEnumerable<string> GetIrrelevantColumns(IEnumerable<string> availableColumnsNames)
		{
			foreach (var field in CusCALPCO.AllLPCOFields)
			{
				if (!availableColumnsNames.Contains(field))
				{
					yield return field;
				}
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (DataSource is IDeclarationProvider declarationProvider)
			{
				var columnStyle = LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation);
				if (!(columnStyle != null &&
					!columnStyle.IsUnavailable &&
					declarationProvider.Declaration is JobDeclaration declaration &&
					declaration.IsPersistent &&
					declaration is IDISHost declarationHost &&
					declarationHost.ShowDISFeatures))
				{
					BottomPanel.Visible = false;
					LpcoGrid.Dock = System.Windows.Forms.DockStyle.Fill;
				}
			}
		}

		void AddOrEditDIFButton_Click(object sender, EventArgs e)
		{
			var mainForm = FindForm() as ZForm;
			var disHost = (mainForm?.BusinessEntity as IDISHostProvider)?.DISHost;
			Action<bool> showDISForm = isEditAllowed =>
			{
				if (LpcoGrid.VisibleRowCount > 0 && LpcoGrid.SelectedRowCount == 0)
				{
					LpcoGrid.Select(0);
				}

				var selectedBO = LpcoGrid.GetFirstSelectedRow();
				if (selectedBO is LPCOView lpcoView && !lpcoView.IsDeleted)
				{
					ShowDISForm(mainForm, lpcoView, isEditAllowed, lpcoView.LPCO.DIFDocument);
					lpcoView.Validation.ValidateCLP_DIFRefNumberOrLocation();
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("4B8BAF8A-C808-4B97-914A-210C0FBC8F32", "Please select a LPCO first."));
				}
			};
			DISHelper.DISButtonClick(mainForm, disHost, showDISForm);
		}

		void ShowDISForm(Form mainForm, LPCOView lpco, bool isEditAllowed, JobRequiredDocumentAddInfo difDocument)
		{
			if (dIFController == null)
			{
				var controllerID = ControllerIDs.Customs.DocumentImageSystem;
				dIFController = ZControllerFactory.Create(controllerID);
				dIFController.ShowChildrenAsDialog = true;
				dIFController.SetFormsModalTo(mainForm);
			}
			var lpcoBO = lpco.LPCO;
			if (difDocument != null)
			{
				if (isEditAllowed)
				{
					dIFController.ShowEditForm(lpcoBO);
				}
				else
				{
					dIFController.ShowViewForm(lpcoBO);
				}
			}
			else
			{
				var needCreateNew = false;
				if (lpco.CLP_DIFRefNumberOrLocation.IsEmpty)
				{
					needCreateNew = true;
				}
				else
				{
					var msg = Res.GetString("5209e197-49fb-437d-86f4-f249ada6b232", "A DIF with a matching URN number and PGA could not be found, do you want to create a new DIF?");
					var caption = Res.GetString("b50fb5f8-a5e8-46b3-a2d7-a671d0ac2b02", "Continue With Creating New DIF");
					if (Globals.Message.Show(msg, caption, MessageBoxButtons.OKCancel, DialogResult.OK) == DialogResult.OK)
					{
						needCreateNew = true;
					}
				}

				if (needCreateNew)
				{
					dIFController.ShowFormForNewEntity(lpcoBO);
				}
			}
		}
		ZController dIFController;
	}
}
