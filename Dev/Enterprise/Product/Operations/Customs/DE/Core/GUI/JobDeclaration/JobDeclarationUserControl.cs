using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class JobDeclarationUserControl : EUJobDeclarationUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public JobDeclarationUserControl()
		{
			InitializeComponent();
			StyleOfEntrySOEDropDown.Visible = false;
			CTStatusIDDropEdit.Visible = false;
			BadgeCodeDropEdit.Visible = false;
			GatewayDropEdit.Visible = false;

			TransportDetailsGroupBox.AllowOverlap(ShipmentDetailsGroupBox);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ShipmentTypeLayoutPanel.FindSingle<Control>("RepresentationDropEdit").AllowOutsideOfParent();
		}

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();

			var declaration = JobDeclaration;
			var isImport = declaration.IsImport;
			var isExport = declaration.IsExport;
			var isWarehouseAdjustment = declaration.IsWarehouseAdjustment;

			SpecificCircumstanceDropEdit.Visible = !isImport;
			AgentsReference.Visible = !isImport;
			GoodsLocationDropEdit.Visible = !isImport;
			GoodsLocationTextBox.Visible = isImport;
			PresentationGroupBox.Visible = isExport;
			PresentationStartDateEdit.Enabled = isExport;
			PresentationEndDateEdit.Enabled = isExport;
			ImporterDocAddress.Enabled = !isWarehouseAdjustment;
			IncoTermExplainButton.Enabled = !isWarehouseAdjustment;

			UpdateEXPCTLinkLabelProperties(isExport);
		}

		protected override int DeclarationTypeMaxLength => 5;

		protected override Type GetOrganizationImportUserControlType() => typeof(ImportOrganizationUserControl);

		protected override Type GetOrganizationExportUserControlType() => typeof(ExportOrganizationUserControl);

		protected override void SetRightTabControlSelectTab()
		{
			RightTabControl.SelectedTab = OrganisationsTabPage;
		}

		void UpdateEXPCTLinkLabelProperties(ZBool isExport)
		{
			var controlMessageUnreadEDocStatus = JobDeclaration.ControlMessageUnreadEDocStatus;
			EXPCTLLinkLabel.Enabled = controlMessageUnreadEDocStatus == YesNoList.Codes.Yes;
			EXPCTLLinkLabel.Visible = isExport && !controlMessageUnreadEDocStatus.IsEmpty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Confirmation String")]
		void EXPCTLLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var exportControlMessageConfirmationText = Res.GetString("4B5E4C69-9A0E-41E2-95B8-FA01E3F190A6", "Customs has sent a control message. Please confirm that you have read the eDoc for Message E_EXP_CTL attached to this declaration.");
			var confirmed = Globals.Message.ShowConfirmation(exportControlMessageConfirmationText, Res.GetString("D042FCB2-0770-4337-ACE6-BE4B447F9B55", "Export Control Message received"), "Yes", MessageBoxIcon.Question) == DialogResult.OK;
			if (confirmed)
			{
				JobDeclaration.ControlMessageUnreadEDocStatus = YesNoList.Codes.No;
				JobDeclaration.Logs.AddNew(AutoEvents.RelatedEDocsRead, "Export Control Message");
				UpdateEXPCTLinkLabelProperties(true);
			}
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control is ZAddressControl && previousControl is ZDocAddressControl;
		}
	}
}
