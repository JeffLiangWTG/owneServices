using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public class UserControlGenerateHelper
	{
		public UserControlGenerateHelper(AsycudaManifestHeader header, IEnumerable<IAdditionalTabPage> additionalUserControls)
		{
			this.additionalUserControls = additionalUserControls?.ToArray();
			this.header = header;
		}

		public void AddAdditionalTabPages(ZTabControl parentControl, KBindingSource bindingSource, ZString bindingMember, Func<AdditionalTabPageUserControlBase> createAdditionalTabPageUserControl, int tabPageIndexOffset = 1)
		{
			if (additionalUserControls != null && additionalUserControls.Any() && createAdditionalTabPageUserControl != null)
			{
				additionalTabPageWrappers.Clear();
				propertyInfoWrappers.Clear();

				var billAdditionalTabPages = additionalUserControls.OrderBy(x => x.TabPageSequence);
				foreach (var additionalTabPage in billAdditionalTabPages)
				{
					var newTab = CreateAdditionalTabPage(parentControl.Name, additionalTabPage, bindingSource, bindingMember, createAdditionalTabPageUserControl);
					parentControl.TabPages.InsertPage(newTab, parentControl.TabPages.Count - tabPageIndexOffset);
					SetUpTabVisible(newTab, additionalTabPage.AdditionalControlVisibility, header, additionalTabPageWrappers, propertyInfoWrappers);
				}
			}
		}

		ZTabPage CreateAdditionalTabPage(string parentTabName, IAdditionalTabPage tabPage, KBindingSource bindingSource, ZString bindingMember, Func<AdditionalTabPageUserControlBase> createAdditionalTabPageUserControl)
		{
			var additionalTabPageUserControl = createAdditionalTabPageUserControl.Invoke();
			additionalTabPageUserControl.CurrentControl = tabPage.AdditionalTabPageUserControl;
			additionalTabPageUserControl.AllowDrop = true;
			additionalTabPageUserControl.AutoScroll = true;
			additionalTabPageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			additionalTabPageUserControl.Location = ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			additionalTabPageUserControl.Name = parentTabName + "_UserControl_" + tabPage.GetType().Name;
			additionalTabPageUserControl.Padding = ControlDpiScalingHelper.NewScaledPadding(3, true);
			additionalTabPageUserControl.Size = ControlDpiScalingHelper.NewScaledSize(400, 209, true);
			additionalTabPageUserControl.TabIndex = 0;
			if (bindingSource != null && !string.IsNullOrEmpty(bindingMember))
			{
				bindingSource.SetBindingMember(additionalTabPageUserControl, bindingMember);
			}

			var additionalTabPage = new ZTabPage();
			additionalTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			additionalTabPage.Name = parentTabName + "_TabPage_" + tabPage.GetType().Name;
			additionalTabPage.Padding = ControlDpiScalingHelper.NewScaledPadding(3, true);
			additionalTabPage.Size = ControlDpiScalingHelper.NewScaledSize(500, 226, true);
			additionalTabPage.Text = tabPage.AdditionalTabPageCaption.Caption;
			additionalTabPage.CaptionResourceString = tabPage.AdditionalTabPageCaption;
			additionalTabPage.UseVisualStyleBackColor = true;
			additionalTabPage.Controls.Add(additionalTabPageUserControl);

			return additionalTabPage;
		}

		static void SetUpTabVisible(ZTabPage newTab, AdditionalTabPageVisibility additionalControlVisibility, AsycudaManifestHeader header, Dictionary<ZTabPage, AdditionalTabPageWrapper> additionalTabPageWrappers, Dictionary<ZPropertyInfo, AdditionalTabPageWrapper> propertyInfoWrappers)
		{
			if (additionalControlVisibility != null && header != null)
			{
				newTab.TabVisible = additionalControlVisibility.isVisible?.Invoke(header) ?? false;
				var dependencies = additionalControlVisibility.dependencies;
				if (dependencies != null)
				{
					foreach (var dependency in dependencies)
					{
						var propertyInfo = dependency?.Invoke(header);
						if (propertyInfo != null)
						{
							var tabpageWrapper = GetControlWrapper(newTab, additionalControlVisibility, header, additionalTabPageWrappers);
							propertyInfo.ValueChanged -= tabpageWrapper.UpdateVisibility;
							propertyInfo.ValueChanged += tabpageWrapper.UpdateVisibility;
							propertyInfoWrappers[propertyInfo] = tabpageWrapper;
						}
					}
				}
			}
			else
			{
				newTab.TabVisible = false;
			}
		}

		static AdditionalTabPageWrapper GetControlWrapper(ZTabPage newTab, AdditionalTabPageVisibility visibility, AsycudaManifestHeader header, Dictionary<ZTabPage, AdditionalTabPageWrapper> tabpageWrappers)
		{
			if (!tabpageWrappers.TryGetValue(newTab, out var tabPageWrapper))
			{
				tabPageWrapper = new AdditionalTabPageWrapper(newTab, visibility, header);
				tabpageWrappers.Add(newTab, tabPageWrapper);
			}

			return tabPageWrapper;
		}

		public void DisposeTabPages()
		{
			if (additionalUserControls != null)
			{
				foreach (var propertyInfo in propertyInfoWrappers)
				{
					propertyInfo.Key.ValueChanged -= propertyInfo.Value.UpdateVisibility;
				}

				foreach (var userControl in additionalUserControls)
				{
					userControl.AdditionalTabPageUserControl?.Dispose();
					userControl.Dispose();
				}
			}
		}

		readonly Dictionary<ZTabPage, AdditionalTabPageWrapper> additionalTabPageWrappers = new Dictionary<ZTabPage, AdditionalTabPageWrapper>();
		readonly Dictionary<ZPropertyInfo, AdditionalTabPageWrapper> propertyInfoWrappers = new Dictionary<ZPropertyInfo, AdditionalTabPageWrapper>();
		readonly IEnumerable<IAdditionalTabPage> additionalUserControls;
		readonly AsycudaManifestHeader header;
	}
}
