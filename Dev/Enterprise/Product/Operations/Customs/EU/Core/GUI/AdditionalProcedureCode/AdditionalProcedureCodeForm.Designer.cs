using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.GUI
{
	partial class AdditionalProcedureCodeForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zGridAdditionalProcedureCode = new Enterprise.ZArchitecture.ZGrid();
			this.zButtonOK = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButtonClose = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGridAdditionalProcedureCode)).BeginInit();
			this.zGridAdditionalProcedureCode.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 318, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(IAdditionalProcedureParent);
			// 
			// zGridAdditionalProcedureCode
			// 
			this.zGridAdditionalProcedureCode.AllowNavigation = false;
			this.zGridAdditionalProcedureCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGridAdditionalProcedureCode, "AdditionalProcedureCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((IAdditionalProcedureParent)(null)).AdditionalProcedureCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.AdditionalProcedureCode)(((System.Collections.IList)(((IAdditionalProcedureParent)(null)).AdditionalProcedureCodes)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.AdditionalProcedureCode)(((System.Collections.IList)(((IAdditionalProcedureParent)(null)).AdditionalProcedureCodes)).SyncRoot)).Description)));
			this.zGridAdditionalProcedureCode.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("7E4DCD37-3632-463D-9B84-F34CB8D8735F", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.zGridAdditionalProcedureCode.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGridAdditionalProcedureCode.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGridAdditionalProcedureCode.GridId = "e39b0b5a-fe1d-4baf-a16b-f653cc61c81f";
			this.zGridAdditionalProcedureCode.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGridAdditionalProcedureCode.LayoutKey = "zGridAdditionalProcedureCode";
			this.zGridAdditionalProcedureCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGridAdditionalProcedureCode.Name = "zGridAdditionalProcedureCode";
			this.zGridAdditionalProcedureCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 283, true);
			this.zGridAdditionalProcedureCode.TabIndex = 1;
			// 
			// zButtonOK
			// 
			this.zButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zButtonOK.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("4082c75f-709c-44db-96e0-df6d0fcd0bc4", "OK");
			this.zButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.zButtonOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 289, true);
			this.zButtonOK.Name = "zButtonOK";
			this.zButtonOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButtonOK.TabIndex = 2;
			this.zButtonOK.ToolTipCaption = null;
			this.zButtonOK.UseVisualStyleBackColor = true;
			this.zButtonOK.Click += new System.EventHandler(this.ZButtonOK_Click);
			// 
			// zButtonClose
			// 
			this.zButtonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zButtonClose.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("5867307a-4461-4618-978f-befb5978e255", "Cancel");
			this.zButtonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.zButtonClose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 289, true);
			this.zButtonClose.Name = "zButtonClose";
			this.zButtonClose.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButtonClose.TabIndex = 3;
			this.zButtonClose.ToolTipCaption = null;
			this.zButtonClose.UseVisualStyleBackColor = true;
			this.zButtonClose.Click += new System.EventHandler(this.ZButtonClose_Click);
			// 
			// AdditionalProcedureCodeForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("b6aadb56-0ece-43c3-8abb-b43ed0a819dd", "Additional Procedure Codes");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 342, true);
			this.Controls.Add(this.zGridAdditionalProcedureCode);
			this.Controls.Add(this.zButtonClose);
			this.Controls.Add(this.zButtonOK);
			this.DataSourceType = typeof(IAdditionalProcedureParent);
			this.Name = "AdditionalProcedureCodeForm";
			this.Text = "Additional Procedure Codes";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zButtonOK, 0);
			this.Controls.SetChildIndex(this.zButtonClose, 0);
			this.Controls.SetChildIndex(this.zGridAdditionalProcedureCode, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGridAdditionalProcedureCode)).EndInit();
			this.zGridAdditionalProcedureCode.ResumeLayout(false);
			this.zGridAdditionalProcedureCode.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid zGridAdditionalProcedureCode;
		private ZArchitecture.GUI.ZButton zButtonOK;
		private ZArchitecture.GUI.ZButton zButtonClose;
	}
}
