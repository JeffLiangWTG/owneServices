using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	public partial class GLPresentationJournalCategoryControl
	{


		#region Component Designer generated code

		private ZGrid GLPresentationJournalCategoryGrid;
		ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2;

		private void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new ZCheckBoxColumnStyleInfo();
			this.GLPresentationJournalCategoryGrid = new ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GLPresentationJournalCategoryGrid)).BeginInit();
			this.GLPresentationJournalCategoryGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GLPresentationJournalCategoryCollection);
			// 
			// GLPresentationJournalCategoryGrid
			// 
			this.GLPresentationJournalCategoryGrid.AllowNavigation = false;
			this.GLPresentationJournalCategoryGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.GLPresentationJournalCategoryGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GLPresentationJournalCategory)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GLPresentationJournalCategory)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GLPresentationJournalCategory)(null)).ParentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GLPresentationJournalCategory)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((GLPresentationJournalCategory)(null)).Bool)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((GLPresentationJournalCategory)(null)).Bool2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((GLPresentationJournalCategory)(null)).Bool3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((GLPresentationJournalCategory)(null)).Bool4)));
			this.GLPresentationJournalCategoryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLPresentationJournalCategoryControl|54be7897-7a0b-4324-badf-39db90fbc961", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLPresentationJournalCategoryControl|EC4E6B35-3837-428F-A86E-3F5E399AA64A", "Parent");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "ParentCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLPresentationJournalCategoryControl|90ba5e8e-8adb-4280-a578-392de7f7d22d", "Description");
			zTextBoxColumnStyleInfo3.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLPresentationJournalCategoryControl|87ed1e56-ad59-4634-b5f5-db14d2ae6794", "Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "Bool";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLPresentationJournalCategoryControl|d9d051ac-edfc-4952-8bb6-7efb1dfa7c3f", "Elimination");
			zCheckBoxColumnStyleInfo2.ColumnName = "Bool2";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLPresentationJournalCategoryControl|6ee05acd-085b-4548-99e0-e03123fe23c7", "Closing");
			zCheckBoxColumnStyleInfo3.ColumnName = "Bool3";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLPresentationJournalCategoryControl|96b0d6ba-3d50-4890-832e-1cf073a86e54", "Opening");
			zCheckBoxColumnStyleInfo4.ColumnName = "Bool4";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.GLPresentationJournalCategoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GLPresentationJournalCategoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.GLPresentationJournalCategoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.GLPresentationJournalCategoryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.GLPresentationJournalCategoryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.GLPresentationJournalCategoryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.GLPresentationJournalCategoryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.GLPresentationJournalCategoryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GLPresentationJournalCategoryGrid.GridId = "9dd70936-3960-46ba-9a76-74c548a199b2";
			this.GLPresentationJournalCategoryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GLPresentationJournalCategoryGrid.LayoutKey = "GLPresentationJournalCategoryGrid";
			this.GLPresentationJournalCategoryGrid.LimitedColumns = null;
			this.GLPresentationJournalCategoryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GLPresentationJournalCategoryGrid.Name = "GLPresentationJournalCategoryGrid";
			this.GLPresentationJournalCategoryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 152, true);
			this.GLPresentationJournalCategoryGrid.TabIndex = 0;
			// 
			// GLPresentationJournalCategoryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GLPresentationJournalCategoryGrid);
			this.Name = "GLPresentationJournalCategoryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 152, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GLPresentationJournalCategoryGrid)).EndInit();
			this.GLPresentationJournalCategoryGrid.ResumeLayout(false);
			this.GLPresentationJournalCategoryGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}