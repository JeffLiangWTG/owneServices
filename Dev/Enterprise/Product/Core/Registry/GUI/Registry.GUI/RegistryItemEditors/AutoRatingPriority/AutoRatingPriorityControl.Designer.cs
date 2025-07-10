using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class AutoRatingPriorityControl : ZUserControl
	{
		CargoWise.Windows.UI.KListBox PriorityListBox;
		Enterprise.ZArchitecture.GUI.ZButton MoveUpButton;
		Enterprise.ZArchitecture.GUI.ZButton MoveDownButton;

		void InitializeComponent()
		{
			this.PriorityListBox = new CargoWise.Windows.UI.KListBox();
			this.MoveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MoveDownButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PriorityListBox
			// 
			this.PriorityListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PriorityListBox.Name = "PriorityListBox";
			this.PriorityListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 134, true);
			this.PriorityListBox.TabIndex = 0;
			// 
			// MoveUpButton
			// 
			this.MoveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 80, true);
			this.MoveUpButton.Name = "MoveUpButton";
			this.MoveUpButton.CaptionResourceString = Res.GetData("0af92e28-f6a8-4718-83fa-5a1de2e6e843", "Move Up");
			this.MoveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.MoveUpButton.TabIndex = 1;
			this.MoveUpButton.Click += new System.EventHandler(this.MoveUpButton_Click);
			// 
			// MoveDown
			// 
			this.MoveDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 112, true);
			this.MoveDownButton.Name = "MoveDownButton";
			this.MoveDownButton.CaptionResourceString = Res.GetData("142af7b1-9a5f-4455-ab00-b566fdb72667", "Move Down");
			this.MoveDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.MoveDownButton.TabIndex = 2;
			this.MoveDownButton.Click += new System.EventHandler(this.MoveDown_Click);
			// 
			// AutoRatingPriorityControl
			// 
			this.Controls.Add(this.MoveDownButton);
			this.Controls.Add(this.MoveUpButton);
			this.Controls.Add(this.PriorityListBox);
			this.Name = "AutoRatingPriorityControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 136, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		System.ComponentModel.Container components = null;
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
