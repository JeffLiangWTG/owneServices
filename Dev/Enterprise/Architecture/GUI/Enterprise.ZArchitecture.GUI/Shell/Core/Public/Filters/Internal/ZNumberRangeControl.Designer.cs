using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	partial class ZNumberRangeControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.PropertySearchDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZFilterStripDropEdit();
			this.FromCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AndLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ToCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UpButton = new ZButton();
			this.DownButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// PropertySearchDropEdit
			//
			this.PropertySearchDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PropertySearchDropEdit, false);
			this.PropertySearchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 3, true);
			this.PropertySearchDropEdit.MaxItemsToShowInDropDown = 10;
			this.PropertySearchDropEdit.Name = "PropertySearchDropEdit";
			this.PropertySearchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 20, true);
			this.PropertySearchDropEdit.TabIndex = 0;
			//
			// FromCalcEdit
			//
			this.FromCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 3, true);
			this.FromCalcEdit.Name = "FromCalcEdit";
			this.FromCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.FromCalcEdit.TabIndex = 3;
			this.FromCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.FromCalcEdit.Visible = false;
			//
			// AndLabel
			//
			this.AndLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 8, true);
			this.AndLabel.Name = "AndLabel";
			this.AndLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.AndLabel.TabIndex = 0;
			this.AndLabel.Text = Res.GetString("ZNumberRangeControl|C9C07C5D-FD55-458C-838F-FD1BCCEAF5A6", "and");
			this.AndLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.AndLabel.Visible = false;
			//
			// ToCalcEdit
			//
			this.ToCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 3, true);
			this.ToCalcEdit.Name = "ToCalcEdit";
			this.ToCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ToCalcEdit.TabIndex = 5;
			this.ToCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ToCalcEdit.Visible = false;
			//
			// UpButton
			//
			this.UpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(540, 3, true);
			this.UpButton.Name = "UpButton";
			this.UpButton.Text = "↑";
			this.UpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 23, true);
			this.UpButton.TabIndex = 6;
			this.UpButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.UpButton.Visible = false;
			//
			// DownButton
			//
			this.DownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 3, true);
			this.DownButton.Name = "DownButton";
			this.DownButton.Text = "↓";
			this.DownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 23, true);
			this.DownButton.TabIndex = 7;
			this.DownButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.DownButton.Visible = false;
			//
			// ZNumberRangeControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PropertySearchDropEdit);
			this.Controls.Add(this.FromCalcEdit);
			this.Controls.Add(this.AndLabel);
			this.Controls.Add(this.ToCalcEdit);
			this.Controls.Add(this.UpButton);
			this.Controls.Add(this.DownButton);
			this.Name = "NumberRangeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 48, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		GUI.Internal.ZFilterStripDropEdit PropertySearchDropEdit;
		ZCalcEdit FromCalcEdit;
		ZLabel AndLabel;
		ZCalcEdit ToCalcEdit;
		ZButton UpButton;
		ZButton DownButton;
	}
}
