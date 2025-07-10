using System.Windows.Forms;

namespace Enterprise.Accounting.Registry.GUI
{
	internal partial class JournalEntriesClassificationGroupCodeControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.journalEntriesClassificationGroupCodeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.journalEntriesClassificationGroupCodeGrid)).BeginInit();
			this.journalEntriesClassificationGroupCodeGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.JournalEntriesClassificationGroupCodeCollection);
			// 
			// journalEntriesClassificationGroupCodeGrid
			// 
			this.journalEntriesClassificationGroupCodeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.journalEntriesClassificationGroupCodeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.JournalEntriesClassificationGroupCode)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.JournalEntriesClassificationGroupCode)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.JournalEntriesClassificationGroupCode)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.JournalEntriesClassificationGroupCode)(null)).Prefix)));
			this.journalEntriesClassificationGroupCodeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DC7DC39E-5514-489B-A946-79E6EA72A15A", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CharacterCasing = CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("FFABF0AD-42F0-4F3B-AE77-908203224BE2", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("48729D2F-2D82-441B-93FE-C7C4896EFB8E", "Prefix");
			zTextBoxColumnStyleInfo3.ColumnName = "Prefix";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.journalEntriesClassificationGroupCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.journalEntriesClassificationGroupCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.journalEntriesClassificationGroupCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.journalEntriesClassificationGroupCodeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.journalEntriesClassificationGroupCodeGrid.GridId = "10DCB84A-8C71-49F0-AC48-5B87A6629744";
			this.journalEntriesClassificationGroupCodeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.journalEntriesClassificationGroupCodeGrid.LayoutKey = "zGrid1";
			this.journalEntriesClassificationGroupCodeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.journalEntriesClassificationGroupCodeGrid.Name = "journalEntriesClassificationGroupCodeGrid";
			this.journalEntriesClassificationGroupCodeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 342, true);
			this.journalEntriesClassificationGroupCodeGrid.TabIndex = 0;
			this.journalEntriesClassificationGroupCodeGrid.Dock = DockStyle.Fill;
			// 
			// JournalEntriesClassificationGroupCodeControl
			// 
			this.Controls.Add(this.journalEntriesClassificationGroupCodeGrid);
			this.Name = "JournalEntriesClassificationGroupCodeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 342, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.journalEntriesClassificationGroupCodeGrid)).EndInit();
			this.journalEntriesClassificationGroupCodeGrid.ResumeLayout(false);
			this.journalEntriesClassificationGroupCodeGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZArchitecture.ZGrid journalEntriesClassificationGroupCodeGrid;
	}
}
