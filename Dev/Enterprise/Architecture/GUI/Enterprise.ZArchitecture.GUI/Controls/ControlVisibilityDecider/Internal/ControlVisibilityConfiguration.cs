using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using CargoWise.Windows.UI.Layout;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static CargoWise.Windows.UI.Layout.RowLayoutDependentControlManager;

namespace Enterprise.ZArchitecture.GUI
{
	internal sealed class ControlVisibilityConfiguration : IDisposable
	{
		public ControlVisibilityConfiguration(Control containerControl)
		{
			ContainerControl = containerControl;
		}

		#region Customisation Settings

		internal IFormCustomisationSettings FormCustomisationSettings
		{
			get
			{
#if DEBUG
				if (FormCustomisationSettingsForTest != null)
				{
					return FormCustomisationSettingsForTest;
				}
#endif

				try
				{
					var template = GetProcessTaskTemplate();
					return template != null ? template.FormCustomisationSettings : null;
				}
				catch (ZBlobReadException)
				{
					var template = GetProcessTaskTemplate(true);
					return template != null ? template.FormCustomisationSettings : null;
				}
			}
#if DEBUG
			set
			{
				FormCustomisationSettingsForTest = value;
			}
#endif
		}

#if DEBUG
		internal IFormCustomisationSettings FormCustomisationSettingsForTest;
#endif

		internal IProcessTaskTemplate ProcessTaskTemplateCached
		{
			get
			{
				if (ParentBusinessObject != null)
				{
					var wasCached = ParentBusinessObject.Factory.TryGetValueFromCacheOnly("CVCProcessTaskTemplate" + ParentBusinessObject.PK.ToString(), out IProcessTaskTemplate result);
					if (wasCached)
					{
						return result;
					}
				}
				return null;
			}
			set
			{
				if (ParentBusinessObject != null)
				{
					ParentBusinessObject.Factory.ClearCachedValue<IProcessTaskTemplate>("CVCProcessTaskTemplate" + ParentBusinessObject.PK.ToString());
					ParentBusinessObject.Factory.GetCachedValue("CVCProcessTaskTemplate" + ParentBusinessObject.PK.ToString(), delegate { return value; });
				}
			}
		}

		IProcessTaskTemplate GetProcessTaskTemplate(bool ignoreCache = false)
		{
			var processTaskTemplateCached = ProcessTaskTemplateCached;
			if (processTaskTemplateCached != null)
			{
				return processTaskTemplateCached;
			}
			if (ParentBusinessObject != null)
			{
				var loader = (IProcessTaskTemplateLoader)Activator.CreateInstance(ObjectFactory.GetType<IProcessTaskTemplateLoader>(), ParentBusinessObject.Factory);
				processTaskTemplateCached = loader.FindTemplateForScreenLayout((IWorkflowProviderCore)ParentBusinessObject, ignoreCache);
				ProcessTaskTemplateCached = processTaskTemplateCached;
				return processTaskTemplateCached;
			}
			return null;
		}

		string[] PropertiesThatAffectWorkflow
		{
			get
			{
				var result = Array.Empty<string>();
				if (ParentBusinessObject != null)
				{
					var provider = (IPropertiesThatAffectWorkflowProvider)Activator.CreateInstance(ObjectFactory.GetType<IPropertiesThatAffectWorkflowProvider>());
					result = provider.GetPropertiesThatAffectWorkflow(((IWorkflowProviderCore)ParentBusinessObject).WorkflowType);
				}
				return result;
			}
		}

		bool WorkflowTabsInitialised
		{
			get
			{
				if (ParentBusinessObject != null)
				{
					var wasCached = ParentBusinessObject.Factory.TryGetValueFromCacheOnly("WorkflowTabsInitialised" + ParentBusinessObject.PK.ToString(), out bool result);
					return wasCached && result;
				}
				else
				{
					return false;
				}
			}
			set
			{
				if (!value)
				{
					throw new ArgumentException("WorkflowTabsInitialised can't be set back to false");
				}
				if (ParentBusinessObject != null)
				{
					ParentBusinessObject.Factory.GetCachedValue("WorkflowTabsInitialised" + ParentBusinessObject.PK.ToString(), delegate { return true; });
				}
			}
		}

		void InitialiseWorkflowTabsIfNecessary()
		{
			if (WorkflowTabsInitialised || FormCustomisationSettings == null || FormCustomisationSettings.CustomisableTabPageNames.Length == 0 || FormCustomisationSettings.AllFieldsAreOnDefaultTabs())
			{
				return;
			}
			WorkflowTabsInitialised = true;

			//We have to do this, rather than directly calling ControlVisibilityConfiguration.Refresh() on all other CVCs, for (at least) two reasons:
			//1) Until the tab is shown, it is not bound, so SyncBindingMember/SetBindingMemberIfNotExist will malfunction, and binding extremely expects the control to be bound and visible before resuming.
			//2) Until the tab is shown, the tab will have no controls in Controls, so GetControlForPlacement will fail.
			//Because a customization might swap a control to and from a not yet visible tab, this seems necessary without a very different idea for how this functionality can be accomplished.
			//But we can otherwise try and cache as much as possible.
			foreach (var tabPageName in FormCustomisationSettings.CustomisableTabPageNames)
			{
				var parentForm = (ZForm)containerControl.FindForm();
				if (parentForm != null && parentForm.TopLevelTabControl != null)
				{
					if (parentForm.TopLevelTabControl.TabPages[tabPageName] is TabPage tab)
					{
						tab.Visible = true;
					}
				}
			}
		}

		#endregion

		#region Dispose

		public void Dispose()
		{
			HookWorkflowAffectedProperties(false);
			ContainerControl = null;
			BindingSource = null;
		}

		void ContainerControl_Disposed(object sender, EventArgs e)
		{
			Dispose();
		}

		#endregion

		#region Controls

		BusinessObject ParentBusinessObject
		{
			get { return parentBusinessObject; }
			set
			{
				if (parentBusinessObject != value)
				{
					if (parentBusinessObject != null)
					{
						HookWorkflowAffectedProperties(false);
					}
					this.parentBusinessObject = value;
					if (parentBusinessObject != null)
					{
						HookWorkflowAffectedProperties(true);
					}
				}
			}
		}
		BusinessObject parentBusinessObject;

		Control ContainerControl
		{
			get { return containerControl; }
			set
			{
				if (containerControl != value)
				{
					if (containerControl != null)
					{
						containerControl.RemoveAncestorChanged(Refresh);
						containerControl.Disposed -= ContainerControl_Disposed;
					}
					this.containerControl = value;
					if (containerControl != null)
					{
						containerControl.AddAncestorChanged(Refresh);
						containerControl.Disposed += ContainerControl_Disposed;
						Refresh();
					}
				}
			}
		}
		Control containerControl;

		KBindingSource BindingSource
		{
			get { return bindingSource; }
			set
			{
				if (bindingSource != value)
				{
					if (bindingSource != null)
					{
						bindingSource.DataSourceChanged -= Refresh;
					}
					this.bindingSource = value;
					if (bindingSource != null)
					{
						bindingSource.DataSourceChanged += Refresh;
					}
				}
			}
		}
		KBindingSource bindingSource;

		void UpdateParentBusinessObject()
		{
			ParentBusinessObject = BindingSource != null ? BindingSource.Current as BusinessObject : null;
		}

		#endregion

		#region Control Visibility

		void Refresh(object sender, EventArgs args)
		{
			Refresh();
		}

		void ClearCacheAndRefresh(object sender, EventArgs args)
		{
			ProcessTaskTemplateCached = null;
			//everyone will want to refresh simultaneously, so we have one wave of 'clear the cache' then one wave of 'ok now refresh and whoever goes first can fill the cache'
			if (ContainerControl.IsHandleCreated)
			{
				ContainerControl.BeginInvoke(new Action(Refresh));
			}
			else
			{
				Refresh();
			}
		}

#if DEBUG
		internal
#endif
 void Refresh()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (ContainerControl != null)
				{
					this.BindingSource = (KBindingSource)KBindingSource.GetBindingSource(ContainerControl);
				}
				UpdateParentBusinessObject();

				var currentTemplate = GetProcessTaskTemplate();

				var forceRefreshInTest = false;
#if DEBUG
				if (Globals.IsTest)
				{
					forceRefreshInTest = ForceRefreshEvenIfTemplateNotChanged;
				}
#endif

				if (lastMode != currentTemplate || forceRefreshInTest)
				{
					if (!forceRefreshInTest)
					{
						lastMode = currentTemplate;
					}

					InitialiseWorkflowTabsIfNecessary();

					if (ParentBusinessObject != null && ContainerControl != null)
					{
						ContainerControl.SuspendLayout();
						try
						{
							foreach (Control child in ContainerControl.Controls)
							{
								var visible = FormCustomisationSettings != null ? FormCustomisationSettings.IsElementVisible(child.Name, ElementType.Field) : true;

								if (visible.HasValue)
								{
									child.Visible = visible.Value;
								}
							}
						}
						finally
						{
							RefreshLayoutWithErrorReporting(); //will ResumeLayout in here
						}
					}
				}
			}
		}

		IProcessTaskTemplate lastMode;

#if DEBUG
		internal bool ForceRefreshEvenIfTemplateNotChanged;
#endif

		void HookWorkflowAffectedProperties(bool hook)
		{
			if (ParentBusinessObject != null)
			{
				var workflowProperties = PropertiesThatAffectWorkflow;
				foreach (var propertyName in workflowProperties)
				{
					var info = ParentBusinessObject.ZPropertyInfoHash[propertyName];
					info.ValueChanged -= ClearCacheAndRefresh;
					if (hook)
					{
						info.ValueChanged += ClearCacheAndRefresh;
					}
				}
			}
		}

		#endregion

		#region Control Placement

		void RefreshLayoutWithErrorReporting()
		{
			try
			{
				RefreshLayout();
			}
			catch (SetRowAndMoveDependentRowsException ex)
			{
				var message =
$@"Control: {ControlDescription.GetControlPathAndLocation(ex.Control)}, Visible: {ex.Control.Visible}
Other Control: {ControlDescription.GetControlPathAndLocation(ex.OtherControl)}, Visible: {ex.OtherControl.Visible}
Visibility Dependent Control: {ControlDescription.GetControlPathAndLocation(ex.VisibilityDependentControl)}
Row Current: {ex.RowCurrent}
Row Target: {ex.RowTarget}
Row Other: {ex.RowOther}

Rows:

{ex.RowDescriptions}

IsHighRiskRowSetTo0CallStack: {ex.IsHighRiskRowSetTo0CallStack}
";
				ErrorReporter.ReportOnce("ArgumentOutOfRange in SetRowAndMoveDependentRows", message, ex.InnerException);
			}
		}

		void RefreshLayout()
		{
			try
			{
				if (containerControl is RowLayoutPanel rowLayoutPanel)
				{
					if (containerControl?.Parent?.Parent?.Parent is ZGroupBox innerGroupBox)
					{
						SetControlPlacement(rowLayoutPanel, innerGroupBox);
					}
					else if (containerControl?.Parent?.Parent?.Parent?.Parent?.Parent is ZGroupBox outerGroupBox)
					{
						SetControlPlacement(rowLayoutPanel, outerGroupBox);
					}
					else
					{
						SetControlPlacement(containerControl?.Parent?.Parent?.Parent); // Entire RowLayoutPanel moves in one block
					}
				}
				else
				{
					var allInitialContainerControls = containerControl.Controls.Cast<Control>().ToArray();
					foreach (var control in allInitialContainerControls)
					{
						SetControlPlacement(control);
					}
				}
			}
			finally
			{
				containerControl.ResumeLayout(containerControl.FindForm() is ZForm zForm && !zForm.IsLayoutSuspended());
			}
		}

		void SetControlPlacement(RowLayoutPanel rowLayout, ZGroupBox groupBox)
		{
			if (rowLayout != null)
			{
				SetRowLayoutControls(rowLayout, groupBox);
			}
		}

		void SetControlPlacement(Control control)
		{
			if (FormCustomisationSettings != null)
			{
				var placement = FormCustomisationSettings.GetTabPlacement(control.Name);
				if (!string.IsNullOrEmpty(placement.TabPageName) && !string.IsNullOrEmpty(placement.Placement))
				{
					var placementTab = GetTabForPlacement(placement.TabPageName);
					if (placementTab != null)
					{
						var placementControl = GetControlForPlacement(placement.Placement, placementTab);
						if (placementControl != null)
						{
							SetNewParentOnControl(control, placementControl, placement);
							var rowLayout = GetRowLayoutPanel(placementControl);
							if (rowLayout != null)
							{
								SetRowLayoutControls(rowLayout, placementControl);
							}
						}
					}
				}
			}
		}

		ZTabPage GetTabForPlacement(string tabPageName)
		{
			var parentForm = FindForm(containerControl);
			if (parentForm != null)
			{
				return (ZTabPage)((ZForm)parentForm).TopLevelTabControl.AllTabPages.FirstOrDefault(tabPage => tabPage.Name == tabPageName)
					?? throw new ArgumentNullException("Form Config specifies tab [" + tabPageName + "] but this tab was not found on the form.");
			}
			return null;
		}

		Form FindForm(Control control)
		{
			while (control != null && !(control is Form))
			{
				ZTabPage tabPage;
				control = control.Parent == null && (tabPage = control as ZTabPage) != null
					? tabPage.ParentTabControl
					: control.Parent;
			}
			return control as Form;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Control name")]
		Control GetControlForPlacement(string placement, ZTabPage tabPage)
		{
			var placementPanelName = placement.Replace(" ", "") + "Panel";
			var parentControls = tabPage.Controls.Find(placementPanelName, true);
			return parentControls.Length > 0 ? parentControls[0] : null;
		}

		void SetBindingMemberIfNotExist(Control control, ICompositeControlBindingSource sourceBindingSource, ICompositeControlBindingSource destinationBindingSource)
		{
			if (string.IsNullOrEmpty(destinationBindingSource.GetBindingMember(control)))
			{
				destinationBindingSource.SetBindingMember(control, sourceBindingSource.GetBindingMember(control));
			}
		}

		void SyncBindingMember(Control parentControl, ICompositeControlBindingSource sourceBindingSource, ICompositeControlBindingSource destinationBindingSource)
		{
			SetBindingMemberIfNotExist(parentControl, sourceBindingSource, destinationBindingSource);
			foreach (Control control in parentControl.Controls)
			{
				SetBindingMemberIfNotExist(control, sourceBindingSource, destinationBindingSource);
				SyncBindingMember(control, sourceBindingSource, destinationBindingSource);
			}
		}

		void SetNewParentOnControl(Control control, Control parentControl, TabPlacement placement)
		{
			if (control.Parent != parentControl)
			{
				var sourceBindingSource = KBindingSource.GetBindingSource(control.Parent);
				var destinationBindingSource = KBindingSource.GetBindingSource(parentControl);
				SyncBindingMember(control, sourceBindingSource, destinationBindingSource);

				control.Parent = parentControl;

				// when changing the control parent, somehow a new BindingContext is created for 'control' rather than 'control'
				// inheriting it's BindingContext from its parent.
				// Here we set it to the parent's BindingContext and this fixes binding issues for the control.
				control.BindingContext = parentControl.BindingContext;
			}
		}

		void SetRowLayoutControls(RowLayoutPanel rowLayout, Control parentControl)
		{
			if (parentControl.Controls.Count > 0 && parentControl.Controls[0].Controls.Count > 0)
			{
				var orderList = GetControlOrderedList();

				RowLayoutDependentControlManager.PrepareSpaceForDependentRows(rowLayout.LayoutEngine as RowLayout, parentControl.Controls[0].Controls[0], orderList);

				foreach (Control childControl in ContainerControl.Controls)
				{
					if (orderList.ContainsKey(childControl) && rowLayout.GetRow(childControl) != orderList[childControl])
					{
						RowLayoutDependentControlManager.SetRowAndMoveDependentRows(rowLayout.LayoutEngine as RowLayout, childControl, orderList[childControl], parentControl.Controls[0].Controls[0]);
					}
				}
			}
		}

		Dictionary<Control, int> GetControlOrderedList()
		{
			var orderList = ContainerControl.Controls.Cast<Control>().Select(
				c => {
					var tp = FormCustomisationSettings.GetTabPlacement(c.Name);
					return new { Control = c, Placement = tp.Placement, Order = tp.RowNumber };
				}
			).ToList();

			var result = new Dictionary<Control, int>();

			if (orderList.Count > 0)
			{
				var usedOrders = new HashSet<(string, int)>();
				var maxValue = orderList.Max(c => c.Order);
				var lastOrder = maxValue >= 0 ? maxValue + 1 : 0;
				foreach (var controlWithOrder in orderList)
				{
					if (controlWithOrder.Order < 0 || usedOrders.Contains((controlWithOrder.Placement, controlWithOrder.Order)))
					{
						result.Add(controlWithOrder.Control, lastOrder++);
					}
					else
					{
						result.Add(controlWithOrder.Control, controlWithOrder.Order);
						if (controlWithOrder.Control.Visible && !controlWithOrder.Placement.IsNullOrEmpty())
						{
							usedOrders.Add((controlWithOrder.Placement, controlWithOrder.Order));
						}
					}
				}
			}

			return result;
		}

		RowLayoutPanel GetRowLayoutPanel(Control control)
		{
			RowLayoutPanel result = null;
			foreach (Control childControl in control.Controls)
			{
				result = (childControl is RowLayoutPanel) ? (RowLayoutPanel)childControl : GetRowLayoutPanel(childControl);
				if (result != null)
				{
					break;
				}
			}
			return result;
		}

		#endregion
	}
}
