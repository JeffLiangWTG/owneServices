using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class AccountingNumberRangeUserControl : RuntimeOptionUserControl
	{
		ZLabel fieldLabel;
		ZArchitecture.GUI.ZIntEdit fromEdit;
		ZArchitecture.GUI.ZIntEdit toEdit;
		ZPanel wrapper;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.wrapper = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.toEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			this.fromEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			this.fieldLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.wrapper.SuspendLayout();
			this.SuspendLayout();
			// 
			// wrapper
			// 
			this.wrapper.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right)));
			this.wrapper.Controls.Add(this.toEdit);
			this.wrapper.Controls.Add(this.fromEdit);
			this.wrapper.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 1, true);
			this.wrapper.Name = "wrapper";
			this.wrapper.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 26, true);
			this.wrapper.TabIndex = 0;
			// 
			// toEdit
			// 
			this.toEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right)));
			this.toEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("AccountingNumberRangeUserControl|0f0dbfae-7b38-45e2-9c5c-0ec0a0e09149", "To");
			this.toEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 2, true);
			this.toEdit.Name = "toEdit";
			this.toEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.toEdit.TabIndex = 2;
			this.toEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// fromEdit
			// 
			this.fromEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.fromEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("AccountingNumberRangeUserControl|dec4bd53-30da-4e38-8375-13505cbe0f16", "From");
			this.fromEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 2, true);
			this.fromEdit.Name = "fromEdit";
			this.fromEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.fromEdit.TabIndex = 1;
			this.fromEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// fieldLabel
			// 
			this.fieldLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.fieldLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 1, true);
			this.fieldLabel.Name = "fieldLabel";
			this.fieldLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 26, true);
			this.fieldLabel.TabIndex = 0;
			this.fieldLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.fieldLabel.UseCompatibleTextRendering = true;
			// 
			// AccountingNumberRangeUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.wrapper);
			this.Controls.Add(this.fieldLabel);
			this.Name = "AccountingNumberRangeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 27, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.wrapper.ResumeLayout(false);
			this.wrapper.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
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
