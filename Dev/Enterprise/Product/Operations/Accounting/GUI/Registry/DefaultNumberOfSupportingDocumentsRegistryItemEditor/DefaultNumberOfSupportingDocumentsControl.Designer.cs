using System;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class DefaultNumberOfSupportingDocumentsControl
	{


		#region Component Designer Generated Code

		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.DefaultNumberOfSupportingDocumentsGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultNumberOfSupportingDocumentsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(DefaultNumberOfSupportingDocuments);
			// 
			// DefaultNumberOfSupportingDocumentsGrid
			// 
			this.DefaultNumberOfSupportingDocumentsGrid.AllowCopyToNewRowMenuItem = false;
			this.DefaultNumberOfSupportingDocumentsGrid.AllowNavigation = false;
			this.DefaultNumberOfSupportingDocumentsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DefaultNumberOfSupportingDocumentsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((DefaultNumberOfSupportingDocuments)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DefaultNumberOfSupportingDocuments)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DefaultNumberOfSupportingDocuments)(null)).DefaultDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DefaultNumberOfSupportingDocuments)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DefaultNumberOfSupportingDocuments)(null)).NumberOfDefault)));
			this.DefaultNumberOfSupportingDocumentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("54561ba3-aff6-4390-ad5b-b5d5995965b2", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("44eab61a-4b27-4876-a3e6-4b162f0b21e0", "Transaction Type / Function");
			zTextBoxColumnStyleInfo2.ColumnName = "DefaultDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo3.Caption = "";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b11ce235-dc07-4b52-903b-13a23dba59df", "Default Description");
			zTextBoxColumnStyleInfo3.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bf97d2a4-2fbd-4966-bc99-024309181456", "Number of Documents");
			zCalcEditColumnStyleInfo1.ColumnName = "NumberOfDefault";
			zCalcEditColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.DefaultNumberOfSupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DefaultNumberOfSupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DefaultNumberOfSupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DefaultNumberOfSupportingDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DefaultNumberOfSupportingDocumentsGrid.GridId = "026DA86A-B6F1-4957-BE5E-032D461C4A14";
			this.DefaultNumberOfSupportingDocumentsGrid.CopySelectedRowsAllowed = false;
			this.DefaultNumberOfSupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DefaultNumberOfSupportingDocumentsGrid.LayoutKey = "DefaultNumberOfSupportingDocumentsGrid";
			this.DefaultNumberOfSupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DefaultNumberOfSupportingDocumentsGrid.Name = "DefaultNumberOfSupportingDocumentsGrid";
			this.DefaultNumberOfSupportingDocumentsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.DefaultNumberOfSupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 236, true);
			this.DefaultNumberOfSupportingDocumentsGrid.TabIndex = 0;
			this.DefaultNumberOfSupportingDocumentsGrid.RowsDeleting += new EventHandler<ZArchitecture.RowsDeletingEventArgs>(this.DefaultNumberOfSupportingDocumentsGrid_RowDeleting);
			// 
			// DefaultNumberOfSupportingDocumentsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DefaultNumberOfSupportingDocumentsGrid);
			this.Name = "DefaultNumberOfSupportingDocumentsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 236, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultNumberOfSupportingDocumentsGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}