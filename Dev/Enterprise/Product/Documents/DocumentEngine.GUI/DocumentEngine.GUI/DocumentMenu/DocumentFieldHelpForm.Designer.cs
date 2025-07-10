using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	public partial class DocumentFieldHelpForm : ZChildForm
	{
		Enterprise.ZArchitecture.ZLabel TagLabel;
		Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		Enterprise.ZArchitecture.ZLabel ExplanationLabel;
		Enterprise.ZArchitecture.GUI.ZPanel CodeListPanel;
		Enterprise.ZArchitecture.ZGrid FieldDefinitionGrid;
		Enterprise.ZArchitecture.ZLabel FieldsForCurrentDocumentLabel;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CodeListPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FieldDefinitionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExplanationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TagLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FieldsForCurrentDocumentLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CodeListPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FieldDefinitionGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 477, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 22, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(336);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(337);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngineCore.DocumentParsing.DocumentFieldDefinitionFormBizo);
			// 
			// CodeListPanel
			// 
			this.CodeListPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.CodeListPanel.Controls.Add(this.FieldDefinitionGrid);
			this.CodeListPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 151, true);
			this.CodeListPanel.Name = "CodeListPanel";
			this.CodeListPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 290, true);
			this.CodeListPanel.TabIndex = 1;
			// 
			// FieldDefinitionGrid
			// 
			this.FieldDefinitionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FieldDefinitionGrid, "Fields");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.DocumentParsing.DocumentFieldDefinitionFormBizo)(null)).Fields)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.DocumentParsing.DocumentFieldDefinition)(((System.Collections.IList)(((Enterprise.DocumentEngineCore.DocumentParsing.DocumentFieldDefinitionFormBizo)(null)).Fields)).SyncRoot)).FieldName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.DocumentParsing.DocumentFieldDefinition)(((System.Collections.IList)(((Enterprise.DocumentEngineCore.DocumentParsing.DocumentFieldDefinitionFormBizo)(null)).Fields)).SyncRoot)).FieldDescription)));
			this.FieldDefinitionGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocumentFieldHelpForm|2372649b-63c7-4d29-adaa-e183b4edc7ee", "Field");
			zTextBoxColumnStyleInfo1.ColumnName = "FieldNameForDisplay";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocumentFieldHelpForm|4e217dc9-f8e2-45df-87f1-234994344530", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "FieldDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(450);
			this.FieldDefinitionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FieldDefinitionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FieldDefinitionGrid.GridId = "733b211f-1a8a-409f-b85a-81b1b8e3c9e2";
			this.FieldDefinitionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FieldDefinitionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FieldDefinitionGrid.LayoutKey = "zGrid1";
			this.FieldDefinitionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FieldDefinitionGrid.Name = "FieldDefinitionGrid";
			this.FieldDefinitionGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.FieldDefinitionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 290, true);
			this.FieldDefinitionGrid.TabIndex = 0;
			// 
			// ExplanationLabel
			// 
			this.ExplanationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ExplanationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.ExplanationLabel.Name = "ExplanationLabel";
			this.ExplanationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 38, true);
			this.ExplanationLabel.TabIndex = 2;
			// 
			// TagLabel
			// 
			this.TagLabel.IsFontBold = true;
			this.TagLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 45, true);
			this.TagLabel.Name = "TagLabel";
			this.TagLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 88, true);
			this.TagLabel.TabIndex = 3;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocumentFieldHelpForm|ce36460a-305a-4cba-980a-91e90595312c", "&Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 449, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 4;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// FieldsForCurrentDocumentLabel
			// 
			this.FieldsForCurrentDocumentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.FieldsForCurrentDocumentLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocumentFieldHelpForm|371dd525-f649-408e-9358-5a9064e9e274", "Document Fields defined on documents available from this menu");
			this.FieldsForCurrentDocumentLabel.IsFontBold = true;
			this.FieldsForCurrentDocumentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 133, true);
			this.FieldsForCurrentDocumentLabel.Name = "FieldsForCurrentDocumentLabel";
			this.FieldsForCurrentDocumentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 15, true);
			this.FieldsForCurrentDocumentLabel.TabIndex = 5;
			// 
			// DocumentFieldHelpForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 499, true);
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocumentFieldHelpForm|a9ae4562-77fb-4f2d-b8d8-81b47d1b9ab2", "Document Field Definitions");
			this.Controls.Add(this.FieldsForCurrentDocumentLabel);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ExplanationLabel);
			this.Controls.Add(this.CodeListPanel);
			this.Controls.Add(this.TagLabel);
			this.DataSourceAssemblyName = "Enterprise.DocumentEngine";
			this.DataSourceType = typeof(Enterprise.DocumentEngineCore.DocumentParsing.DocumentFieldDefinitionFormBizo);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.DocumentFieldDefinitionFormBizo";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 535, true);
			this.Name = "DocumentFieldHelpForm";
			this.Controls.SetChildIndex(this.TagLabel, 0);
			this.Controls.SetChildIndex(this.CodeListPanel, 0);
			this.Controls.SetChildIndex(this.ExplanationLabel, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FieldsForCurrentDocumentLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CodeListPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.FieldDefinitionGrid)).EndInit();
			this.ResumeLayout(false);
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
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
