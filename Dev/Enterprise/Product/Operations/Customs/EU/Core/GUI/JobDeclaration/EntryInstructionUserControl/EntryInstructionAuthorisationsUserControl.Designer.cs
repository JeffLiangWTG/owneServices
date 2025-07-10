using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class EntryInstructionAuthorisationsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AuthorisationsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AuthorisationsGrid)).BeginInit();
			this.AuthorisationsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// AuthorisationsGrid
			// 
			this.AuthorisationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AuthorisationsGrid, "CustomsEntryInstructions.CusAuthorizationUsages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CusAuthorizationUsages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CusAuthorizationUsages)).SyncRoot)).AGC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CusAuthorizationUsages)).SyncRoot)).CustomsCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CusAuthorizationUsages)).SyncRoot)).AGC_Number)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CusAuthorizationUsages)).SyncRoot)).AGC_NumberFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CusAuthorizationUsages)).SyncRoot)).AGC_OH_Owner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CusAuthorizationUsages)).SyncRoot)).Lookups.Owners)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CusAuthorizationUsages)).SyncRoot)).AGC_CPH_Authorization)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CusAuthorizationUsages)).SyncRoot)).Lookups.NumberList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CusAuthorizationUsages)).SyncRoot)).ReferenceNumberOf_RelatedAuthorisationHeaderIgnoringReferenceNumber)));
			this.AuthorisationsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "AGC_Code";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.ColumnName = "CustomsCode";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.ColumnName = "AGC_Number";
			zMultiControlColumnStyleInfo1.DefaultCollectionIndex = 0;
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "AGC_NumberFieldType";
			zMultiControlColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.CusAuthorisations;
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Lookups+Owners";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AGC_OH_Owner";
			zOrganisationFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo1.IsMandatory = true;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups+NumberList";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AGC_CPH_Authorization";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo2.ColumnName = "ReferenceNumberOf_RelatedAuthorisationHeaderIgnoringReferenceNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.AuthorisationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AuthorisationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AuthorisationsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.AuthorisationsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.AuthorisationsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.AuthorisationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AuthorisationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AuthorisationsGrid.GridId = "0D5091BE-4932-47AC-9912-195F23FC0D8F";
			this.AuthorisationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AuthorisationsGrid.LayoutKey = "AuthorisationsGrid";
			this.AuthorisationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AuthorisationsGrid.Name = "AuthorisationsGrid";
			this.AuthorisationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 274, true);
			this.AuthorisationsGrid.TabIndex = 0;
			// 
			// EntryInstructionAuthorisationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AuthorisationsGrid);
			this.Name = "EntryInstructionAuthorisationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 274, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AuthorisationsGrid)).EndInit();
			this.AuthorisationsGrid.ResumeLayout(false);
			this.AuthorisationsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.ZGrid AuthorisationsGrid;
	}
}
