using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using CargoWiseSchema = CargoWise.Schema.Schema;

namespace Enterprise.ZArchitecture.GUI
{
	public static class TabPageNotificationsExposer
	{
		#region ExposeTabPageNotifications

		public static void ExposeTabPageNotifications(Control control, IBusiness topLevelDataSource)
		{
			ExposeTabPageNotifications(control, topLevelDataSource, false);
		}

		public static void ExposeTabPageNotificationsOnIdle(Control control, IBusiness topLevelDataSource)
		{
			ExposeTabPageNotifications(control, topLevelDataSource, true);
		}

		static void ExposeTabPageNotifications(Control control, IBusiness topLevelDataSource, bool onIdle)
		{
			var propertyNames = GetPropertyNamesWithNotifications(topLevelDataSource.Notifications);
			if (propertyNames.Length > 0)
			{
				ExposeTabPageNotifications(control, propertyNames, onIdle);
			}
		}

		internal static void ExposeTabPageNotifications(Control control, string[] propertyNames, bool onIdle)
		{
			if (control.IsDisposed || (!(control is Form) && control.Parent == null))
			{
				return;
			}

			if (control is ZTabPage tabPage && tabPage.Parent is ZTabControl tabControl)
			{
				using (new CellNotificationSuspender(control))
				{
					tabPage.NotifyBindingOrShowing();
					ExposeTabPageNotificationsForTabPage(tabControl, tabPage, propertyNames);
				}
			}

			if (control is IDynamicControlCreationUserControl dynamicControlCreationUserControl)
			{
				dynamicControlCreationUserControl.ForceCreateHostedControl();
			}

			foreach (var child in GetControls(control))
			{
				if (onIdle)
				{
					UserIdleWorker.QueueWorkItem(child, GetIdleDescription(child), 10, new Action(() => ExposeTabPageNotifications(child, propertyNames, onIdle)));
				}
				else
				{
					ExposeTabPageNotifications(child, propertyNames, onIdle);
				}
			}
		}

		static void ExposeTabPageNotificationsForTabPage(ZTabControl tabControl, ZTabPage tab, string[] propertyNames)
		{
			if (!tab.ExcludeFromBindingOnSave)
			{
				using (PerformanceStatisticsCollector.StartMonitoring("TabPageNotificationsExposer.ExposeTabPageNotificationsForTabPage()", tab.GetType().FullName)) // end users cannot see this message
				{
					var plugIn = ZTabPagePlugIn.FindParentPlugIn(tab);
					if (plugIn != null)
					{
						ExposeTabPageNotificationsForPlugIn(tabControl, tab, propertyNames, plugIn);
					}
					else
					{
						BindTabAndControlsBindingTo(tabControl, tab, propertyNames);
					}
				}
			}
		}

		static IEnumerable<Control> GetControls(Control control)
		{
			switch (control)
			{
				case ZTabControl tabControl:
					return BindParentBindingPageIfNotBound(tabControl);
				default:
					return control.Controls.Cast<Control>();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Debug text")]
		static string GetIdleDescription(Control control)
		{
			switch (control)
			{
				case ZTabPage tabPage:
					return "ExposeTabNotificationsForIdleWorker for tab Name = " + tabPage.Text;
				default:
					return "ExposeTabNotificationsForIdleWorker for control = " + control.Name;
			}
		}

		static IEnumerable<ZTabPage> BindParentBindingPageIfNotBound(ZTabControl tabControl)
		{
			var parent = tabControl.Parent;
			while (parent != null)
			{
				var bindingTabPage = parent as ZBindingTabPage;
				if (bindingTabPage != null)
				{
					if (!bindingTabPage.IsBound)
					{
						bindingTabPage.Bind();
					}
					break;
				}
				parent = parent.Parent;
			}
			return tabControl.TabPages.Cast<ZTabPage>();
		}

		static void ExposeTabPageNotificationsForPlugIn(ZTabControl tabControl, ZTabPage tab, string[] propertyNames, ZPlugIn plugIn)
		{
			const string jobDocPrefix = JobRequiredDocumentSchema.Constants.Prefix + "_";

			var bindingTab = tab as ZBindingTabPage;
			if (bindingTab != null &&
				!bindingTab.IsBound &&
				(!plugIn.GetType().Name.Contains("eDocsPlugIn") || propertyNames.Any(p => p.StartsWith(jobDocPrefix, StringComparison.OrdinalIgnoreCase))) &&
				plugIn.ShouldPlugInGUIAndBusinessEntityBeCreated())
			{
				plugIn.Setup();
				plugIn.SetupUserControl();

				if (plugIn.GetType().Name.Contains("eDocsPlugIn"))
				{
					((TabPage)tab).Visible = true;
				}
			}

			// we don't want to bind plugins that were never clicked on
			if ((plugIn.IsActive || ZTabPagePlugIn.FindTopLevelTabWithPlugin(tab).HasBeenMadeVisible)
				&& plugIn.BusinessEntity != null)
			{
				BindTabAndControlsBindingTo(tabControl, tab, propertyNames);
			}
		}

		static void BindTabAndControlsBindingTo(ZTabControl tabControl, ZTabPage tab, string[] propertyNames)
		{
			var bindingTab = tab as ZBindingTabPage;

			if (bindingTab != null && !bindingTab.IsDisposed)
			{
				bindingTab.Bind();
			}
			if (bindingTab == null || !bindingTab.IsDisposed)
			{
				StartBindingForControlsBindingTo(tab, propertyNames, false);
			}
		}

		#endregion

		#region StartBindingForControlsBindingTo

		static void StartBindingForControlsBindingTo(Control control, IList<string> propertyNames, bool hasDynamicParentControl)
		{
			if (control.IsDisposed)
			{
				return;
			}

			if (control is IDynamicControlCreationUserControl dynamicControlCreationUserControl)
			{
				dynamicControlCreationUserControl.ForceCreateHostedControl();
				hasDynamicParentControl = true;
			}

			var bindingMember = control.GetBindingMember();
			if (!string.IsNullOrEmpty(bindingMember))
			{
				var bindingField = GetBindingField(bindingMember);
				if (!string.IsNullOrEmpty(bindingField))
				{
					var bindingSource = (KBindingSource)KBindingSource.GetBindingSource(control);
					if (bindingSource != null)
					{
						if (propertyNames.Contains(bindingField))
						{
							control.ForceBindingIncludingParents();
						}
						else
						{
							if (bindingSource.BindingContext == null
								&& hasDynamicParentControl
								&& GetDynamicControlParent(control) is Control dynamicControlParent)
							{
								if (dynamicControlParent is IDataBoundControl boundControl && boundControl.DataSource == null)
								{
									dynamicControlParent.ForceBindingIncludingParents();
								}
								if (KBindingSource.GetBindingSource(dynamicControlParent) is KBindingSource dynamicControlParentBidingSource)
								{
									bindingSource = dynamicControlParentBidingSource;
								}
							}
							if (!string.IsNullOrEmpty(bindingSource.DataMember))
							{
								bindingMember = bindingSource.DataMember + "." + bindingMember;
							}
							if (bindingSource.BindingContext is BindingContext bindingContext
								&& BindingHelper.GetObject(bindingContext, bindingSource.DataSource, bindingMember, false) is BusinessObject bizo
								&& bizo.FindPropertyInfo(bindingField) is ZWrappedPropertyInfo wrappedProperty
								&& propertyNames.Contains(wrappedProperty.InnerInfo.Name))
							{
								control.ForceBindingIncludingParents();
							}
						}
					}
				}
			}

			if (control is ZGrid grid && grid.ColumnStyles.Cast<ZGridColumnInfo>().Any(
					column =>
						propertyNames.Contains(new KBindingMemberInfo(column.ColumnName.Replace("+", ".")).BindingField)
						|| (
							!string.IsNullOrWhiteSpace(CargoWiseSchema.GetPrefixFromColumnName(column.ColumnName))
							&& propertyNames.Any(propertyName => CargoWiseSchema.GetPrefixFromColumnName(propertyName) == CargoWiseSchema.GetPrefixFromColumnName(column.ColumnName))
						)
				)
				)
			{
				control.ForceBindingIncludingParents();
			}
			else if (control is IDynamicLayoutPanel dynamicLayoutPanel)
			{
				dynamicLayoutPanel.ApplyBinding();
			}

			foreach (Control child in control.Controls)
			{
				StartBindingForControlsBindingTo(child, propertyNames, hasDynamicParentControl);
			}
		}

		static Control GetDynamicControlParent(Control control)
		{
			var parent = control?.Parent;
			while (parent != null)
			{
				if (parent is IDynamicControlCreationUserControl)
				{
					return parent;
				}
				parent = parent.Parent;
			}
			return null;
		}

		static string GetBindingField(string bindingMember)
		{
			return bindingMember == null ? "" : new KBindingMemberInfo(bindingMember.Replace("+", ".")).BindingField;
		}

		#endregion

		#region GetPropertyNamesWithNotifications

		static string[] GetPropertyNamesWithNotifications(IEnumerable<INotification> notifications)
		{
			var result = new List<string>();
			foreach (var notification in notifications)
			{
				var propertyNotification = notification as PropertyNotification;
				if (propertyNotification != null && !result.Contains(propertyNotification.PropertyName))
				{
					result.Add(propertyNotification.PropertyName);
				}
			}
			return result.ToArray();
		}

		#endregion
	}
}
