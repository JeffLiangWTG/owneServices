using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.PlugIn
{
	sealed partial class ZFormForPlugInTest : ZForm
	{
		public Enterprise.ZArchitecture.GUI.ZTemplateTabControl TabControl;

		protected override void InitializeComponent()
		{
			base.InitializeComponent();
			this.TabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
																			| System.Windows.Forms.AnchorStyles.Right)));
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 12, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 584, true);
			this.TabControl.TabIndex = 7;

			this.Controls.Add(this.TabControl);
		}
	}
}
