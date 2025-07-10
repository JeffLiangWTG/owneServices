using System;
using System.Windows.Forms;
using Enterprise.DocumentEngine.DocBuilder;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	class SuspendDocBuilderCustomizationsToolStripMenuItem : ToolStripMenuItem
	{
		internal SuspendDocBuilderCustomizationsToolStripMenuItem()
		{
			var text = Res.GetString("35a1c985-c6f6-40b9-b7bb-bcc728756ccc", "Suspend DocBuilder Customizations");
			Text = text;
			Name = text;
			UpdateChecked();
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations = !Checked;
			UpdateChecked();
		}

		void UpdateChecked()
		{
			Checked = TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations;
		}
	}
}
