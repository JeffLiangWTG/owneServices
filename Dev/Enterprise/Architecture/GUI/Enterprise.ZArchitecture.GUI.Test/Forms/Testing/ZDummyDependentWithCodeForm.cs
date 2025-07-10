using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDummyDependentWithCodeForm : ZForm
	{
		public ZTextBox TextBox;
		public Enterprise.Core.Forms.ZPostingButtonsUserControl SaveUserControl;
		readonly System.ComponentModel.Container components;

		public ZDummyDependentWithCodeForm()
		{
		}

		public ZDummyDependentWithCodeForm(IBusiness entity)
			: base(entity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveUserControl);

			FormLoadedWithArgs += (s, e) => LoadedFormArgs = e.Args;
		}

		public new void FireSaved()
		{
			base.FireSaved();
		}

		public IEnumerable<string> LoadedFormArgs { get; private set; }

		public override string FormCaption
		{
			get { return "ZDummyDependentWithCodeForm"; }
		}

		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			this.TextBox = new ZTextBox();
			this.SaveUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.SuspendLayout();
			// 
			// TextBox
			// 
			this.TextBox.BindTo = "ZD1_Code";
			this.TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 48, true);
			this.TextBox.Name = "TextBox";
			this.TextBox.TabIndex = 1;
			this.TextBox.Text = "ZTEXTBOX1";
			//			
			// SaveUserControl
			// 
			this.SaveUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 248, true);
			this.SaveUserControl.Name = "SaveUserControl";
			this.SaveUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.SaveUserControl.TabIndex = 5;
			// 
			// ZTestForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 308, true);
			this.Controls.Add(this.SaveUserControl);
			this.Controls.Add(this.TextBox);
			this.Name = "ZTestForm";
			this.Text = "TestForm";
			this.ResumeLayout(false);
		}
		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
