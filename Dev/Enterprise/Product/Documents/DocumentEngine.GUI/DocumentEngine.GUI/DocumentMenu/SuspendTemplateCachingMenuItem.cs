using System;
using System.Windows.Forms;
using Enterprise.DocumentEngine.DocBuilder;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	class SuspendTemplateCachingMenuItem : MenuItem
	{
		internal SuspendTemplateCachingMenuItem()
		{
			Text = Res.GetString("2989a135-883f-4f1c-9e29-a24ba814bbf5", "Suspend Template Caching");
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
