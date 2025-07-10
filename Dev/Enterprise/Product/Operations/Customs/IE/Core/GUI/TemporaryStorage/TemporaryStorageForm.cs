using System;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class TemporaryStorageForm : EU.TemporaryStorage.GUI.TemporaryStorageForm
	{
		public TemporaryStorageForm(TemporaryStorageHeader header)
			: base(header)
		{
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (Header != null)
			{
				Header.AMA_ManifestTypeInfo.ValueChanged -= ReloadLayoutProviderAndUpdateLayout;
				Header.AMA_ManifestTypeInfo.ValueChanged += ReloadLayoutProviderAndUpdateLayout;
			}
		}

		protected override ZMenuItem GetNewMessagingMenu() => new TemporaryStorageMessagesMenu(this);

		protected override void Dispose(bool disposing)
		{
			if (Header != null)
			{
				Header.AMA_ManifestTypeInfo.ValueChanged -= ReloadLayoutProviderAndUpdateLayout;
			}
			base.Dispose(disposing);
		}

		void ReloadLayoutProviderAndUpdateLayout(object sender, EventArgs e)
		{
			var layoutProvider = TemporaryStorageLayoutProviderHelper.GetLayoutProvider(Header);
			if (layoutProvider?.GetTemporaryStorageDetailsLayout() is IPanelLayoutProvider layout)
			{
				MainDynamicLayoutPanel.UpdateLayout(layout);
			}
		}
	}
}
