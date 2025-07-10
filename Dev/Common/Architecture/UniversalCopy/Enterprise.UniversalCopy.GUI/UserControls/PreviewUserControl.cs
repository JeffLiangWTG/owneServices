using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.UniversalCopy;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.UniversalCopy.Business;
using Enterprise.UniversalCopy.GUI.UserControls;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.UniversalCopy.GUI
{
	public partial class PreviewUserControl : ZUserControl
	{
		#region Construction

		public PreviewUserControl(UniversalCopyManager copyManager, UniversalCopyTemplate ucTemplate)
		{
			CopyManager = copyManager;
			UCTemplate = ucTemplate;

			InitializeComponent();
		}

		UniversalCopyManager CopyManager { get; set; }
		UniversalCopyTemplate UCTemplate { get; set; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				mockBizo = null;
				mockCollection = null;

				Application.Idle -= SynchronizeFocusedControlOnApplicationIdle;
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Data bindign

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource == null)
			{
				base.SetDataBinding(null, "");

				Application.Idle -= SynchronizeFocusedControlOnApplicationIdle;
			}
			else
			{
				if (hasGridControls)
				{
					base.SetDataBinding(MockCollection, "");
				}
				else
				{
					base.SetDataBinding(MockBizo, "");
					Application.Idle -= SynchronizeFocusedControlOnApplicationIdle;
					Application.Idle += SynchronizeFocusedControlOnApplicationIdle;

					if (hasFormControls && UCTemplate != null)
					{
						InitializeControlsNotifications();
					}
				}
			}
		}

		BusinessObject MockBizo
		{
			get
			{
				if (mockBizo == null)
				{
					mockBizo = MockFactory.New(CopyManager.ElementType);
					mockBizo.SetReadOnlyIncludingChildren(true);
					mockBizo.SuspendSettingHasChanges();
					mockBizo.SuspendValidation();
				}
				return mockBizo;
			}
		}
		BusinessObject mockBizo;
#if DEBUG
		public
#endif
		class MockBusinessObjectCollection<T> : BusinessObjectCollection<T>
			where T : BusinessObject
		{
			public MockBusinessObjectCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		IBusinessObjectCollection MockCollection
		{
			get
			{
				if (mockCollection == null)
				{
					var genericType = typeof(MockBusinessObjectCollection<>);
					var concreteType = genericType.MakeGenericType(MockBizo.GetType());
					mockCollection = (IBusinessObjectCollection)Activator.CreateInstance(concreteType, MockBizo.Factory);
					mockCollection.Add(MockBizo);
				}
				return mockCollection;
			}
		}
		IBusinessObjectCollection mockCollection;

		BusinessObjectFactory MockFactory
		{
			get
			{
				if (mockFactory == null)
				{
					if (MockController != null)
					{
						mockFactory = MockController.Factory;
					}

					if (mockFactory == null)
					{
						mockFactory = new BusinessObjectFactory { RefreshEnabled = false };
					}

					mockFactory.SuspendValidation();
					//((IBusinessObjectFactoryInternals)mockFactory).ReadOnly = true; // This causes errors in business logic of mock objects
				}
				return mockFactory;
			}
		}
		BusinessObjectFactory mockFactory;

		ZController MockController
		{
			get
			{
				if (mockController == null)
				{
					var filterModule = CopyManager.LocalModule as ZFilterModule;
					if (filterModule != null && filterModule.GridCollection != null && filterModule.GridCollection.TypeOfElements.IsAssignableFrom(CopyManager.ElementType))
					{
						try
						{
							mockController = filterModule.GetNewController(null);
						}
						catch (NullReferenceException) { }
					}
				}
				return mockController;
			}
		}
		ZController mockController;

		#endregion

		#region Prepare preview controls

		public bool UseGridRepresentation { get; set; }

		internal void InitializePreviewControls()
		{
			InitializeEditFormPreviewControls();

			if (Controls.Count == 0)
			{
				InitializeGridPreviewControls();
			}

			if (Controls.Count == 0)
			{
				InitializeNotAvailableLabel();
			}
		}

		void InitializeEditFormPreviewControls()
		{
			if (!UseGridRepresentation && CopyManager != null && CopyManager.LocalModule != null && CopyManager.ElementType != null)
			{
				try
				{
					if (MockController != null)
					{
						var parentForm = FindForm() as UniversalCopyTemplateForm;
						if (parentForm != null)
						{
							// Set ControllerID of copyable business object so preview controls (e.g. Workflow) can correctly configure themselves.
							parentForm.ControllerID = MockController.ID;
						}

						using (var form = ((ZControllerInternals)MockController).GetForm(MockBizo) as ZForm)
						{
							if (form != null)
							{
								form.Show();
								TabPageNotificationsExposer.ExposeTabPageNotifications(form, MockBizo);

								var panel = new ZPanel { AutoScroll = false, Size = form.ClientSize };

								foreach (var control in form.Controls.Cast<Control>().ToArray())
								{
									if (!(control is ZStatusBar) && !(control is ZPanel && control.Name == "BottomPanel"))
									{
										DisableAllControlsFunctionality(control);
										form.Controls.Remove(control);
										panel.Controls.Add(control);
									}
								}

								if (panel.Controls.Count > 0)
								{
									AutoScroll = true;
									Controls.Add(panel);
									hasFormControls = true;
								}
								else
								{
									panel.Dispose();
								}
							}
						}
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("fc784a26-c008-4dc3-9092-4d53d95867a5", "Not enough information to decide which preview form to show."));
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex.IsCriticalException())
					{
						throw;
					}

					ErrorReporter.ReportOnce("PreviewUserControl_Initialize",
						string.Format((NoResString)"Error during initialization of preview control of element of type {0} for universal copy template.", CopyManager.ElementType.FullName) + "\r\n" + ex.Message,
						ex);
				}
			}
		}

		bool hasFormControls;

		void InitializeGridPreviewControls()
		{
			var gridCopyManager = CopyManager as GridUniversalCopyManager;
			ZGrid parentGrid = gridCopyManager != null ? gridCopyManager.Grid : null;
			if (parentGrid != null)
			{
				var grid = new ZGrid { ReadOnly = true, Dock = DockStyle.Fill };
				grid.SetBindingMember(".");

				foreach (ZGridColumnInfo info in parentGrid.ColumnStyles)
				{
					ZGridColumnInfo newInfo = info is ZCheckBoxColumnStyleInfo ? new ZCheckBoxColumnStyleInfo() : new ZTextBoxColumnStyleInfo();
					newInfo.ColumnName = info.ColumnName;
					if (!string.IsNullOrEmpty(info.Caption))
					{
						newInfo.Caption = info.Caption;
					}
					newInfo.CaptionResourceString = info.CaptionResourceString;
					newInfo.Width = info.Width;

					grid.ColumnStyles.Add(newInfo);
				}

				if (grid.ColumnStyles.Count > 0)
				{
					grid.CurrentCellChanged += SynchronizeFocusedCellOnPreviewGrid;
					Controls.Add(grid);
					hasGridControls = true;
				}
				else
				{
					grid.Dispose();
				}
			}
		}

		bool hasGridControls;

		void DisableAllControlsFunctionality(Control control)
		{
			RemoveUnsupportedTabPages(control);
			DisableAllControlsFunctionalityIncludingChildren(control);
		}

		void RemoveUnsupportedTabPages(Control control)
		{
			var mainTabControl = control as ZTabControl;
			if (mainTabControl != null)
			{
				foreach (var page in mainTabControl.TabPages.Cast<TabPage>().ToArray())
				{
					if (page is ZLogsTabPage || page is ZTabPagePlugIn)
					{
						mainTabControl.TabPages.Remove(page);
						page.Dispose();
					}
				}
			}
		}

		void DisableAllControlsFunctionalityIncludingChildren(Control control)
		{
			if (control is Button || control is ZToolStrip)
			{
				control.Enabled = false;
			}

			var dynamicCreatingControl = control as IDynamicControlCreationUserControl;
			if (dynamicCreatingControl != null)
			{
				dynamicCreatingControl.ForceCreateHostedControl();
			}

			var visibilityConfigurationProvider = GetControlVisibilityConfigurationProvider(control);
			if (visibilityConfigurationProvider != null)
			{
				visibilityConfigurationProvider.ClearAllVisibilityConfigurations();
			}

			foreach (Control childControl in control.Controls)
			{
				DisableAllControlsFunctionalityIncludingChildren(childControl);
			}
		}

		ControlVisibilityConfigurationProvider GetControlVisibilityConfigurationProvider(Control control)
		{
			if (control is ZUserControl || control is ZForm)
			{
				var field = control.GetType().GetField("VisibilityConfigurationProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				return field != null ? field.GetValue(control) as ControlVisibilityConfigurationProvider : null;
			}

			return null;
		}

		void InitializeNotAvailableLabel()
		{
			foreach (var control in Controls.Cast<Control>().ToArray())
			{
				control.Dispose();
			}
			var label = new ZLabel
			{
				Text = Res.GetString("0dbf0dee-7d38-4cf4-b496-a06e40f6f83d", "Was not able to find and prepare preview controls for copy object type"),
				IsFontBold = true,
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleCenter
			};
			Controls.Add(label);
		}

		#endregion

		#region Synchronizing focused control with selected element/property

		#region Selected control changed

		bool isSynchronizingControl;

		Control LastFocusedControl
		{
			get { return lastFocusedControl; }
			set
			{
				if (value != lastFocusedControl)
				{
					var previewExtension = GetPreviewControlVisualExtension(lastFocusedControl, false);
					if (previewExtension != null)
					{
						previewExtension.IsSelected = false;
					}

					lastFocusedControl = value;

					previewExtension = GetPreviewControlVisualExtension(lastFocusedControl, true);
					if (previewExtension != null)
					{
						previewExtension.IsSelected = true;
					}
				}
			}
		}
		Control lastFocusedControl;

		PreviewControlVisualExtension GetPreviewControlVisualExtension(Control control, bool canCreate)
		{
			var extendedControl = control as IExtendedControl;
			if (extendedControl != null)
			{
				var previewExtension = extendedControl.Extensions.Get<PreviewControlVisualExtension>();
				if (previewExtension == null && canCreate)
				{
					previewExtension = new PreviewControlVisualExtension();
					previewExtension.Initialize(extendedControl);
					extendedControl.Extensions.Add(previewExtension);
				}
				return previewExtension;
			}
			return null;
		}

		ZTextBoxColumnStyle lastFocusedColumn;

		void SynchronizeFocusedControlOnApplicationIdle(object sender, EventArgs e)
		{
			if (isSynchronizingControl)
			{
				return;
			}
			isSynchronizingControl = true;
			try
			{
				var currentFocusedControl = GetFirstBoundParent(this.GetFrontMostActiveControl());
				if (currentFocusedControl != null && currentFocusedControl != this && currentFocusedControl != LastFocusedControl)
				{
					LastFocusedControl = currentFocusedControl;
					NotifySelectedControlChanged(GetPropertyPathToControl(currentFocusedControl));
				}
			}
			finally
			{
				isSynchronizingControl = false;
			}
		}

		Control GetFirstBoundParent(Control control)
		{
			while (control != null)
			{
				var bindableControl = control as IBindTo;
				if (bindableControl != null && !string.IsNullOrEmpty(bindableControl.BindTo) && bindableControl.BindTo != ".")
				{
					break;
				}
				var bindableContainer = control as IDataBoundControl;
				if (bindableContainer != null && !string.IsNullOrEmpty(bindableContainer.DataMember) && bindableContainer.DataMember != ".")
				{
					break;
				}

				control = control.Parent;
			}
			return control;
		}

		string GetPropertyPathToControl(Control control)
		{
			var pathParts = new List<string>();

			while (control != null)
			{
				if (control is PreviewUserControl)
				{
					pathParts.Insert(0, MockBizo.TableName);

					return string.Join(".", pathParts);
				}

				var bindableControl = control as IBindTo;
				if (bindableControl != null && !string.IsNullOrEmpty(bindableControl.BindTo) && bindableControl.BindTo != ".")
				{
					pathParts.Insert(0, bindableControl.BindTo);
				}
				else
				{
					var bindableContainer = control as IDataBoundControl;
					if (bindableContainer != null && !string.IsNullOrEmpty(bindableContainer.DataMember) && bindableContainer.DataMember != ".")
					{
						pathParts.Insert(0, bindableContainer.DataMember);
					}
				}

				control = control.Parent;
			}

			return null;
		}

		void SynchronizeFocusedCellOnPreviewGrid(object sender, EventArgs e)
		{
			if (isSynchronizingControl)
			{
				return;
			}
			isSynchronizingControl = true;
			try
			{
				var previewGrid = sender as ZGrid;
				if (previewGrid != null && previewGrid.TableStyles.Count > 0 && previewGrid.CurrentCell.ColumnNumber >= 0)
				{
					var currentColumnStyle = previewGrid.TableStyles[0].GridColumnStyles[previewGrid.CurrentCell.ColumnNumber] as ZTextBoxColumnStyle;
					if (currentColumnStyle != null && currentColumnStyle != lastFocusedColumn)
					{
						lastFocusedColumn = currentColumnStyle;
						NotifySelectedControlChanged(MockBizo.TableName + "." + currentColumnStyle.MappingName);
					}
				}
			}
			finally
			{
				isSynchronizingControl = false;
			}
		}

		void NotifySelectedControlChanged(string path)
		{
			if (!string.IsNullOrEmpty(path) && SelectedControlChanged != null)
			{
				SelectedControlChanged(this, new SelectedControlChangedEventArgs(path));
			}
		}

		public class SelectedControlChangedEventArgs : EventArgs
		{
			public SelectedControlChangedEventArgs(string path)
			{
				NewPropertyPath = path;
			}

			public string NewPropertyPath { get; set; }
		}

		public event EventHandler<SelectedControlChangedEventArgs> SelectedControlChanged;

		#endregion

		#region Selected property changed

		public void SelectBoundControl(string propertyPath)
		{
			if (isSynchronizingControl || DataSource == null || (!hasFormControls && !hasGridControls))
			{
				return;
			}

			isSynchronizingControl = true;
			try
			{
				var pathParts = propertyPath.Split('\\', '.', '+');

				var previewGrid = hasGridControls && Controls.Count > 0 ? Controls[0] as ZGrid : null;
				if (previewGrid != null && previewGrid.TableStyles.Count > 0)
				{
					for (int i = 0; i < previewGrid.TableStyles[0].GridColumnStyles.Count; i++)
					{
						var columnStyle = previewGrid.TableStyles[0].GridColumnStyles[i];
						if (columnStyle.MappingName == pathParts[pathParts.Length - 1])
						{
							previewGrid.CurrentCell = new DataGridCell(previewGrid.CurrentCell.RowNumber, i);
						}
					}
				}
				else
				{
					var foundControl = GetBoundControl(this, pathParts, 0);
					var currentFocusedControl = GetFirstBoundParent(foundControl);
					if (foundControl != null && foundControl != this && currentFocusedControl != LastFocusedControl)
					{
						OpenAllParentTabPages(foundControl);
						LastFocusedControl = foundControl;
						NotifySelectedControlChanged(GetPropertyPathToControl(currentFocusedControl));
					}
				}
			}
			finally
			{
				isSynchronizingControl = false;
			}
		}

		public Control GetBoundControl(IList<string> propertyPath)
		{
			return GetBoundControl(this, propertyPath, 0);
		}

		Control GetBoundControl(Control currentControl, IList<string> propertyPath, int startIndex)
		{
			if (currentControl == null || startIndex >= propertyPath.Count)
			{
				return null;
			}

			Control foundControl = null;

			var bindableControl = currentControl as IBindTo;
			var bindableContainer = currentControl as IDataBoundControl;
			if (startIndex == 0 && currentControl == this ||
				bindableControl != null && bindableControl.BindTo == propertyPath[startIndex] ||
				bindableContainer != null && bindableContainer.DataMember == propertyPath[startIndex])
			{
				foundControl = currentControl;
			}

			if (foundControl != null ||
				((bindableControl == null || string.IsNullOrEmpty(bindableControl.BindTo)) && (bindableContainer == null || string.IsNullOrEmpty(bindableContainer.DataMember))))
			{
				foreach (Control control in currentControl.Controls)
				{
					var deeperFoundControl = GetBoundControl(control, propertyPath, startIndex + (foundControl != null ? 1 : 0));
					if (deeperFoundControl != null)
					{
						foundControl = deeperFoundControl;
						break;
					}
				}
			}

			return foundControl;
		}

		void OpenAllParentTabPages(Control control)
		{
			while (control != null && control != this)
			{
				var tabPage = control as TabPage;
				if (tabPage != null)
				{
					var tabControl = tabPage.Parent as TabControl;
					if (tabControl != null && tabControl.SelectedTab != tabPage)
					{
						tabControl.SelectedTab = tabPage;
					}
				}

				control = control.Parent;
			}
		}

		void InitializeControlsNotifications()
		{
			InitializeControlsNotifications(UCTemplate.CopyTemplateTree.CopyTemplateNode.InnerNode as EntityCopyTemplateNode, this);
		}

		bool InitializeControlsNotifications(CopyTemplateNode currentNode, Control currentControl)
		{
			if (currentNode == null || currentControl == null)
			{
				return false;
			}

			var templateNode = currentNode as TemplateCopyTemplateNode;
			if (templateNode != null)
			{
				return InitializeControlsNotifications(templateNode.InnerNode, currentControl);
			}

			var currentElementName = currentNode.Name;
			Control foundControl = null;

			var bindableControl = currentControl as IBindTo;
			var bindableContainer = currentControl as IDataBoundControl;
			if (currentControl == this ||
				bindableControl != null && bindableControl.BindTo == currentElementName ||
				bindableContainer != null && bindableContainer.DataMember == currentElementName)
			{
				if (!currentNode.HasData())
				{
					return true;
				}

				foundControl = currentControl;

				var propertyCopyNode = currentNode as PropertyCopyTemplateNode;
				if (propertyCopyNode != null)
				{
					SetPropertyControlNotification(propertyCopyNode, foundControl);
					return true;
				}
			}

			if (foundControl != null ||
				((bindableControl == null || string.IsNullOrEmpty(bindableControl.BindTo)) && (bindableContainer == null || string.IsNullOrEmpty(bindableContainer.DataMember))))
			{
				WrappedCopyTemplateNode wrappedNode;
				while ((wrappedNode = currentNode as WrappedCopyTemplateNode) != null)
				{
					currentNode = wrappedNode.InnerNode;
				}

				foreach (Control control in currentControl.Controls)
				{
					if (foundControl != null)
					{
						var entityNode = currentNode as EntityCopyTemplateNode;
						if (entityNode != null && entityNode.Nodes.Any(node => InitializeControlsNotifications(node, control)))
						{
							break;
						}
					}
					else
					{
						if (InitializeControlsNotifications(currentNode, control))
						{
							break;
						}
					}
				}
			}

			return foundControl != null;
		}

		public void SetPropertyControlNotification(PropertyCopyTemplateNode propertyCopyNode, Control control)
		{
			var previewExtension = GetPreviewControlVisualExtension(control, true);
			if (previewExtension != null)
			{
				switch (propertyCopyNode.CopyMethod)
				{
					case CopyMethod.Copy:
						previewExtension.SetNotification(PreviewControlVisualExtension.PreviewControlNotification.Copy);
						break;
					case CopyMethod.Empty:
						previewExtension.SetNotification(PreviewControlVisualExtension.PreviewControlNotification.Empty);
						break;
					case CopyMethod.Default:
						previewExtension.SetNotification(PreviewControlVisualExtension.PreviewControlNotification.Default, propertyCopyNode.PropertyType);
						break;
					case CopyMethod.Macro:
						previewExtension.SetNotification(PreviewControlVisualExtension.PreviewControlNotification.Macro, propertyCopyNode.Value);
						break;
					case CopyMethod.Property:
						previewExtension.SetNotification(PreviewControlVisualExtension.PreviewControlNotification.Property, propertyCopyNode.Value);
						break;
					case CopyMethod.Value:
						previewExtension.SetNotification(PreviewControlVisualExtension.PreviewControlNotification.Edit, propertyCopyNode.Value);
						break;
					default:
						previewExtension.ClearNotifications();
						break;
				}
			}
		}

		#endregion

		#endregion
	}
}
