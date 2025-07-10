using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Enterprise.DocumentEngine.DocBuilder;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	class SuspendTemplateCachingToolStripMenuItem : ToolStripMenuItem
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		internal SuspendTemplateCachingToolStripMenuItem()
		{
			var text = Res.GetString("2989a135-883f-4f1c-9e29-a24ba814bbf5", "Suspend Template Caching");
			Text = text;
			Name = text;
			UpdateChecked();
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			TemplateCache.SuspendTemplateCache = !Checked;
			UpdateChecked();
		}

		void UpdateChecked()
		{
			Checked = TemplateCache.SuspendTemplateCache;
		}
	}
}
