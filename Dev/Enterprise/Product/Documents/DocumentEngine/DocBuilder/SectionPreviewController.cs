using System;
using System.ComponentModel;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public class SectionPreviewController : Component
	{
		public SectionPreviewController()
		{
		}

		public SectionPreviewController(IContainer container)
		{
			container.Add(this);
		}

		SectionPreviewManager manager;
		ISectionPreviewView view;

		public void Bind(SectionPreviewManager manager, ISectionPreviewView view)
		{
			UnBind();

			this.manager = manager;
			this.view = view;

			manager.LanguageInfo.ValueChanged += new EventHandler(HandleLanguageChanged);
			manager.ZoomInfo.ValueChanged += new EventHandler(HandleZoomChanged);

			view.UpdateZoom(manager.Zoom);

			manager.Language = Enterprise.Core.Constants.Languages.EnglishAmerican;
		}

		public void UnBind()
		{
			if (manager != null)
			{
				manager.LanguageInfo.ValueChanged -= new EventHandler(HandleLanguageChanged);
				manager.ZoomInfo.ValueChanged -= new EventHandler(HandleZoomChanged);
				manager = null;
				view = null;
			}
		}

		void HandleLanguageChanged(object sender, EventArgs e)
		{
			view.UpdateSectionViews(manager.DocumentTitle, [.. manager.DocDataProviders], manager.SystemExcelTemplate, manager.CustomizedExcelTemplate, manager.ContactType, manager.DocumentDirection);
		}

		void HandleZoomChanged(object sender, EventArgs e)
		{
			view.UpdateZoom(manager.Zoom);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnBind();
			}

			base.Dispose(disposing);
		}
	}
}
