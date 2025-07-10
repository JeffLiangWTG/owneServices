using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Module;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.ReleaseBuilds.GUI
{
	public partial class ReleaseBuildForm : ZTemplateForm
	{
		public ReleaseBuildForm(ReleaseBuild releaseBuild)
			: base(releaseBuild)
		{
			releaseBuild.DeleteBuild += new CancelEventHandler(ReleaseBuild_DeleteBuild);
			releaseBuild.FTPDeleteFailed += new ReleaseBuild.FTPDeleteFailedEventHandler(ReleaseBuild_FTPDeleteFailed);

			if (releaseBuild.HL_Superceded)
			{
				ReleaseClientsTabPage.TabVisible = false;
			}

			PlugIns.Add(ControllerIDs.Audit);
		}

		ReleaseBuild Build
		{
			get { return this.BusinessEntity as ReleaseBuild; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Build.HL_ProductInfo.ValueChanged += HL_ProductInfo_ValueChanged;
			SetupDownloadPackageLink();
		}

		void HL_ProductInfo_ValueChanged(object sender, EventArgs e)
		{
			if (Build.IsCargoWiseProduct)
			{
				Globals.Message.ShowError("Product should not be set to 'ENT' or 'CWN' or 'CGW'");
				Build.HL_Product = ZString.Empty;
				ProductDropEdit.Text = ZString.Empty;
			}
		}

		void SetupDownloadPackageLink()
		{
			if (Build.HL_PackagePath.IsEmpty)
			{
				return;
			}

			downloadPackageLink.Text = (NoResString)$"Download {Build.ExeVersion} Package";
			downloadPackageLink.Visible = true;
		}

		void downloadPackageLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var packageFilePath = Build.HL_PackagePath.ToString();
			if (!File.Exists(packageFilePath))
			{
				Globals.Message.ShowError($"File does not exist at path '{packageFilePath}'");
				return;
			}

			WebUrlLauncher.Launch(packageFilePath);
		}

		#region Superceded Build

		void ReleaseBuild_DeleteBuild(object sender, CancelEventArgs e)
		{
			DialogResult result = Globals.Message.Show("Do you want to delete this build from the Generic and Client Specific FTP folders, as well as cancelling any queued upgrades?", "Superseded Release Build", MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
			e.Cancel = result != DialogResult.Yes;
		}

		void ReleaseBuild_FTPDeleteFailed(System.Collections.Generic.List<string> clientCodesAndReasons)
		{
			string message = @"This release build could not be deleted from the following FTP locations.
Please review the list and delete the files manually." + System.Environment.NewLine;

			foreach (string error in clientCodesAndReasons)
			{
				message += System.Environment.NewLine;
				message += "  - " + error;
			}

			Globals.Message.Show(message);
		}

		#endregion

		#region Implementation

		public override string FormCaption
		{
			get { return "Release Build"; }
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		#endregion

		#region Double-clicking on Grids

		internal void ShowRelatedBusinessObjectForm(ZController controller, BusinessObject sourceEntity)
		{
			if (DisplayMode == ODisplayMode.ReadOnly)
			{
				controller.ShowViewForm(sourceEntity);
			}
			else
			{
				controller.ShowEditForm(sourceEntity);
			}
		}

		void ReleaseClientsGrid_DoubleClick(object sender, EventArgs e)
		{
			ShowLicenceHeaderClient(ReleaseClientsGrid);
		}

		void PatchClientsGrid_DoubleClick(object sender, EventArgs e)
		{
			ShowLicenceHeaderClient(PatchClientsGrid);
		}

		void SentPatchClientsGrid_DoubleClick(object sender, EventArgs e)
		{
			ShowClient();
		}

		void IncidentsReportedGrid_DoubleClick(object sender, EventArgs e)
		{
			if (IsThereARowAtMouseCursorPosition(IncidentsReportedGrid))
			{
				ShowSupportIncident(IncidentsReportedGrid);
			}
		}

		void ShowLicenceHeaderClient(ZGrid grid)
		{
			if (IsThereARowAtMouseCursorPosition(grid))
			{
				LicenceHeader header = (LicenceHeader)grid.SelectedElements[0];
				ShowRelatedBusinessObjectForm(new EDIOrganisationControllerOverride(), header.Company.Header);
			}
		}

		void ShowClient()
		{
			if (IsThereARowAtMouseCursorPosition(SentPatchClientsGrid))
			{
				OrgHeader client = (OrgHeader)SentPatchClientsGrid.SelectedElements[0];
				ShowRelatedBusinessObjectForm(new EDIOrganisationControllerOverride(), client);
			}
		}

		internal ZController ShowSupportIncident(ZGrid grid)
		{
			if (grid.SelectedElements.Length > 0)
			{
				AutoIncidentMain incident = grid.SelectedElements[0] as AutoIncidentMain;
				if (incident != null && incident.IM_IncidentType == IncidentConstants.IncidentType.SupportIncident)
				{
					var controller = ZControllerFactory.Create(ClientControllerRegistration.SupportIncident);
					ShowRelatedBusinessObjectForm(controller, incident);
					return controller;
				}
			}
			return null;
		}

		bool IsThereARowAtMouseCursorPosition(ZGrid grid)
		{
			return (grid.SelectedElements.Length > 0) && (grid.List[grid.HitTest(grid.PointToClient(Cursor.Position)).Row] != null);
		}

		#endregion

		#region Release Build Content Links

		void patchedWorkItemsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowPatchedToUpgradeModule((ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.WorkItem));
		}

		void patchedIncidents_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowPatchedToUpgradeModule((ZFilterGridModule)ZModuleFactory.Instance.Create(Modules.ClientModuleRegistration.SupportIncident));
		}

		void ShowPatchedToUpgradeModule(ZFilterGridModule module)
		{
			FilterBusinessObjectDefaults result = new FilterBusinessObjectDefaults();
			result.Add(new FilterBusinessObjectDefault("Patched to Upgrade", "Property1", ((ReleaseBuild)BusinessEntity).PreviousReleaseBuild.PK));
			result.Add(new FilterBusinessObjectDefault("Patched to Upgrade", "Property2", ((ReleaseBuild)BusinessEntity).PK));
			module.FilterBusinessObject.SetExternalDefaults(result);
			module.ShowPopup();
			module.FilterBusinessObject.FilterStrips.ClearValues();
			module.FilterBusinessObject.LoadLayout(null);
		}

		#endregion

		// internals for tests
		internal ZGrid InternalIncidentsReportedGrid => IncidentsReportedGrid;
		internal ZTabPage InternalIncidentsReportedTabPage => IncidentsReportedTabPage;
		internal ZTabPage InternalReleaseClientsTabPage => ReleaseClientsTabPage;
		internal ZTemplateTabControl InternalMainTabControl => MainTabControl;
		internal ZTabControl InternalTopLevelTabControl => TopLevelTabControl;

		#region Dispose
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && !IsDisposed)
			{
				Build.HL_ProductInfo.ValueChanged -= HL_ProductInfo_ValueChanged;
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
