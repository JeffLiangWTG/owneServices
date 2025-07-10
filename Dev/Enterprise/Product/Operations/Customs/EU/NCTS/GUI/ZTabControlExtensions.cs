using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	static class ZTabControlExtensions
	{
		public static void AddAdditionalTabs(this ZTabControl tabControl, ZBindingSource bindingSource, IEnumerable<ITabPage> additionalTabPages)
		{
			if (additionalTabPages.Any())
			{
				foreach (var tabPageDetail in additionalTabPages)
				{
					var tabPage = new ZTabPage();
					var userControl = tabPageDetail.CreateUserControl();

					tabControl.Controls.Add(tabPage);

					tabPage.CaptionResourceString = tabPageDetail.Caption;
					tabPage.Controls.Add(userControl);
					tabPage.Dock = DockStyle.Fill;
					tabPage.Name = tabPageDetail.GetType().Name;

					bindingSource.SetBindingMember(userControl, tabPageDetail.UserControlBindingMember);
					userControl.Dock = DockStyle.Fill;
					userControl.Name = userControl.GetType().Name;
				}
			}
		}

		public static void ReorderTabs(this ZTabControl tabControl, Func<IEnumerable<string>, IEnumerable<string>> reorderTabPageNames)
		{
			var defaultTabPages = tabControl.TabPages.Cast<ZTabPage>().Select(x => x.Name).ToArray();
			var tabPageNamesInOrder = reorderTabPageNames?.Invoke(defaultTabPages);

			if (tabPageNamesInOrder != null && tabPageNamesInOrder.Any() && string.Join("|", tabPageNamesInOrder) != string.Join("|", defaultTabPages))
			{
				var allTabPages = tabControl.Controls.Cast<ZTabPage>().ToList();
				tabControl.Controls.Clear();

				foreach (var tabName in tabPageNamesInOrder)
				{
					var tab = allTabPages.Single(x => x.Name == tabName);
					tab.Parent = null;
					tabControl.Controls.Add(tab);
					allTabPages.Remove(tab);
				}

				allTabPages.ForEach(x => x.Dispose());
			}
		}
	}
}

