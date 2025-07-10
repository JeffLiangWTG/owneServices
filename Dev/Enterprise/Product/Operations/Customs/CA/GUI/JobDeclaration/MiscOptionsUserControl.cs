using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CAMiscOptionsUserControl : BaseMiscOptionsUserControl
	{
		public CAMiscOptionsUserControl()
		{
			InitializeComponent();
		}

		new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		protected override void ChangeControlVisibilityOnMessageTypeChanged()
		{
			base.ChangeControlVisibilityOnMessageTypeChanged();
			var isImport = JobDeclaration?.IsImport ?? ZBool.False;
			var isB2Common = JobDeclaration?.IsB2OrIM2OrB3X ?? ZBool.False;
			var isLVS = JobDeclaration?.IsLVS ?? ZBool.False;
			var mergeByVisible = isImport && !isLVS && !(JobDeclaration.JE_MessageType == JobMessageTypeList.Codes.Import && JobDeclaration.IsCADEnabled);
			importDeclarationOptionsGroupBox.Visible = isImport && !isB2Common;
			cAMergeByDropEdit.Visible = mergeByVisible;
			if (isB2Common)
			{
				MiscOptionsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1F387E40-FAB1-4E52-8067-B00B459330AF", "Administered By");
				BrokerCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BAD634EF-A9EC-48A3-8042-16EC27DC79B6", "B2 Signed By");
				cAMergeByDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("F77D92EA-2176-479E-B5BF-E0052C569F78", "B2 Merge By");
				cAMergeByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 99, true);
				anySightDepositAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 125, true);
			}
			else
			{
				MiscOptionsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("44BB7EE0-E95A-4EB7-8647-0E1E2FFD4809", "Miscellaneous Options");
				BrokerCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4BE2C71A-516D-4BDC-82C6-BAD5C6E6FF3C", "Broker");
				cAMergeByDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("C557BA64-42D7-4586-8C0E-E85CAC00C72E", "Entry Merge By");
				if (mergeByVisible)
				{
					cAMergeByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 125, true);
					anySightDepositAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 154, true);
				}
				else
				{
					anySightDepositAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 125, true);
					defaultFreightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 154, true);
					commentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 146, true);
				}
			}

			MergeByDropEdit.CaptionResourceString = isImport && !isLVS
				? Enterprise.Customs.CA.GUI.Res.GetData("87ba0d29-a61b-4836-8f25-0b7539b1fd67", "EDI Release Merge By")
				: Enterprise.Customs.CA.GUI.Res.GetData("edf281c3-3b25-44b9-b959-ad9bea1e3fe2", "Merge By");
			var isIID = JobDeclaration != null && JobDeclaration.IsIID;
			oGDNRCheckBox.Visible = !isIID && !isLVS;
			oGDTCCheckBox.Visible = !isIID && !isLVS;
			oGDICCheckBox.Visible = !isIID && !isLVS;
			oGDCFIACheckBox.Visible = !isIID && !isLVS;
			woodPackagingIndCheckBox.Visible = isIID;
			permitApplicationCheckBox.Visible = isIID;
			inspectionArrangementsCompleteCheckBox.Visible = isIID;
			cSAEntryCheckBox.Visible = (OrgImpAddInfo.Get(JobDeclaration.Importer)?.ZO_IsCSAApprovedImporter ?? false);
			cSAEntryCheckBox.Location = isIID ? ControlDpiScalingHelper.NewScaledPoint(20, 101) : ControlDpiScalingHelper.NewScaledPoint(20, 126);
			pGAOptionsGroupBox.Visible = isIID && !isB2Common;
			lPCOGroupBox.Visible = isIID;
			MergeByDropEdit.Visible = !isB2Common;
			defaultFreightCalcEdit.Visible = !isB2Common;
			commentLabel.Visible = !isB2Common;
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			bondGroupBox.Visible = JobDeclaration.IsImport && UniversalReferenceConstants.IsCarmR2;
		}

		void RefreshBondButton_Click(object sender, EventArgs e)
		{
			if (Globals.Message.Show(Res.GetString("3D9AA0B2-B119-4424-B65F-90959F8FC80B", "Are you sure you want to refresh the Bond details?"), Res.GetString("B0A8364C-6D74-4A77-AB67-F84620BE6F2C", "Warning"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
			{
				new BondDetailsDefaulter().Default(JobDeclaration, ZString.Empty);
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			JobDeclaration.JE_OH_ImporterInfo.ValueChanged -= JE_OH_ImporterInfo_ValueChanged;
			JobDeclaration.JE_OH_ImporterInfo.ValueChanged += JE_OH_ImporterInfo_ValueChanged;
		}

		void JE_OH_ImporterInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeControlVisibilityOnMessageTypeChanged();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (JobDeclaration != null)
			{
				JobDeclaration.JE_OH_ImporterInfo.ValueChanged -= JE_OH_ImporterInfo_ValueChanged;
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
