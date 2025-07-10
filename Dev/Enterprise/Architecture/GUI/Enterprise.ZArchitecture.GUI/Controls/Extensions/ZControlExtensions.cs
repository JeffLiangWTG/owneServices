using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.ZArchitecture.GUI
{
	public static class ZControlExtensions
	{
		#region SetReadOnlyIncludingChildren

		public static void SetReadOnlyIncludingChildren(this Control control)
		{
			SetReadOnlyIncludingChildren(control, false, null);
		}

		public static void SetReadOnlyIncludingChildren(this Control control, bool removeReadonlyBinding)
		{
			SetReadOnlyIncludingChildren(control, removeReadonlyBinding, null);
		}

		public static void SetReadOnlyIncludingChildren(this Control control, List<string> namesOfControlsToIgnore)
		{
			SetReadOnlyIncludingChildren(control, false, namesOfControlsToIgnore);
		}

		public static void SetReadOnlyIncludingChildren(this Control control, bool removeReadonlyBinding, List<string> namesOfControlsToIgnore)
		{
			var tabPage = control as ZTabPage;
			if (tabPage != null)
			{
				if (!tabPage.ShouldBeReadOnlyInViewMode)
				{
					return;
				}
			}

			control.ControlAdded -= Control_ControlAddedWithRemoveBinding;
			control.ControlAdded -= Control_ControlAdded;
			control.DataBindings.CollectionChanging -= DataBindings_CollectionChanging;

			if (removeReadonlyBinding)
			{
				control.ControlAdded += Control_ControlAddedWithRemoveBinding;
				control.DataBindings.CollectionChanging += DataBindings_CollectionChanging;
			}
			else
			{
				control.ControlAdded += Control_ControlAdded;
			}

			if (tabPage != null)
			{
				tabPage.RunWhenTabInitialized((_, __) => SetReadOnlyIncludingChildrenCore(tabPage, removeReadonlyBinding, namesOfControlsToIgnore));
			}
			else
			{
				SetReadOnlyIncludingChildrenCore(control, removeReadonlyBinding, namesOfControlsToIgnore);
			}
		}

		static void DataBindings_CollectionChanging(object sender, CollectionChangeEventArgs e)
		{
			var bindings = ((ControlBindingsCollection)sender);
			if (e.Action == CollectionChangeAction.Add &&
				e.Element is Binding binding &&
				bindings.BindableComponent is Control control)
			{
				var readOnlyProperty = BindableComponentMetaDataPropertyLocator.GetDefaultMetaDataProperty(control.GetType(), MetaDataTypes.ReadOnly);
				if (readOnlyProperty?.Name == binding.PropertyName)
				{
					binding.ControlUpdateMode = ControlUpdateMode.Never;
					readOnlyProperty.SetValue(control, true);
				}
			}
		}

		static void Control_ControlAdded(object sender, ControlEventArgs e)
		{
			SetReadOnlyIncludingChildren(e.Control);
		}

		static void Control_ControlAddedWithRemoveBinding(object sender, ControlEventArgs e)
		{
			SetReadOnlyIncludingChildren(e.Control, true);
		}

		static void SetReadOnlyIncludingChildrenCore(Control control, bool removeReadonlyBinding, List<string> namesOfControlsToIgnore)
		{
			if (namesOfControlsToIgnore == null || !namesOfControlsToIgnore.Contains(control.Name))
			{
				var skipSettingChildControlControl = control.Parent as ISkipSettingChildControlReadOnly;
				if (skipSettingChildControlControl != null)
				{
					if (skipSettingChildControlControl.SkipSettingChildControlReadOnly)
					{
						return;
					}
				}
				var readOnlyProperty = BindableComponentMetaDataPropertyLocator.GetDefaultMetaDataProperty(control.GetType(), MetaDataTypes.ReadOnly);
				if (readOnlyProperty != null)
				{
					if (removeReadonlyBinding)
					{
						control.DataBindings.RemoveBinding(readOnlyProperty.Name);
					}
					readOnlyProperty.SetValue(control, true);
				}

				if (control is ToolStrip)
				{
					var toolStrip = control as ToolStrip;
					var readOnlyAttributeType = typeof(CanBeReadOnlyUIAttribute);
					foreach (ToolStripItem toolStripItem in toolStrip.Items)
					{
						var canBeReadOnly = TypeDescriptor.GetAttributes(toolStripItem)[readOnlyAttributeType] != null;
						if (canBeReadOnly)
						{
							toolStripItem.Enabled = false;
						}
					}
				}
				else if (control is ZLinkLabel)
				{
					var linkLabel = control as ZLinkLabel;
					linkLabel.Enabled = false;
				}
				else if (control is ZButton button)
				{
					SetReadOnlyIncludingChildrenIsButton(control, button);
				}
				else if (control is DataGridView gridView)
				{
					gridView.Enabled = false;
				}
				else
				{
					var readOnlyControl = control as IReadOnlyToggleControl;

					if (readOnlyControl != null)
					{
						var optionalControl = control as IReadOnlyAutomationOptional;

						if (optionalControl == null || optionalControl.ShouldSetReadOnlyWhenSettingIncludingChildren)
						{
							readOnlyControl.ReadOnly = true;
						}
					}
					foreach (Control child in control.Controls)
					{
						child.SetReadOnlyIncludingChildren(removeReadonlyBinding, namesOfControlsToIgnore);
					}
				}
			}
		}

		static void SetReadOnlyIncludingChildrenIsButton(Control control, ZButton button)
		{
			var parentForm = control.TopLevelControl as ZForm;
			var moduleIsReadOnly = false;

			var module = parentForm?.GetModule();
			if (module != null)
			{
				if (button.DoNoOverrideMyEditableMode)
				{
					return;
				}
				if (!module.AllowEdit)
				{
					button.Enabled = true;
					moduleIsReadOnly = true;
				}
			}

			if (!moduleIsReadOnly)
			{
				if (!button.EditableInViewMode || button.ShouldSetReadOnlyWhenSettingIncludingChildren)
				{
					button.Enabled = false;
				}
				else
				{
					button.Enabled = true;
				}
			}
		}

		#endregion

		#region UpdateEditableIncludingChildren

		public static void UpdateEditableIncludingChildren(this Control control, bool isEditable, string[] namesOfControlsToIgnore = null)
		{
			var tabPage = control as ZTabPage;
			if (tabPage != null)
			{
				if (!tabPage.ShouldBeReadOnlyInViewMode)
				{
					return;
				}

				tabPage.RunWhenTabInitialized((_, __) => UpdateEditableIncludingChildrenCore(tabPage, isEditable, namesOfControlsToIgnore));
			}
			else
			{
				UpdateEditableIncludingChildrenCore(control, isEditable, namesOfControlsToIgnore);
			}
		}

		static void UpdateEditableIncludingChildrenCore(Control control, bool isEditable, string[] namesOfControlsToIgnore)
		{
			if (namesOfControlsToIgnore == null || !namesOfControlsToIgnore.Contains(control.Name))
			{
				var toolStrip = control as ToolStrip;
				if (toolStrip != null)
				{
					foreach (ToolStripItem toolStripItem in toolStrip.Items)
					{
						toolStripItem.Enabled = isEditable;
					}

					return;
				}

				var optionalControl = control as IReadOnlyAutomationOptional;
				if (optionalControl == null || optionalControl.ShouldSetReadOnlyWhenSettingIncludingChildren)
				{
					var grid = control as ZGrid;

					if (grid != null)
					{
						grid.ReadOnlyChanged -= SetReadOnlyOnControl;
					}

					control.ControlAdded -= SetReadOnlyOnControl;
					control.VisibleChanged -= SetReadOnlyOnControl;

					if (!isEditable)
					{
						if (grid != null)
						{
							grid.ReadOnlyChanged += SetReadOnlyOnControl;
						}

						control.ControlAdded += SetReadOnlyOnControl;
						control.VisibleChanged += SetReadOnlyOnControl;
					}

					var extendedControl = control as IExtendedControl;
					var extensions = extendedControl?.Extensions;
					var extension = extensions?.OfType<ReadOnlyCacheExtension>().FirstOrDefault();

					if (extensions != null && extension == null)
					{
						extension = new ReadOnlyCacheExtension(namesOfControlsToIgnore);
						extensions.Add(extension);
					}

					var controlsToIgnore = namesOfControlsToIgnore ?? extension?.GetNamesOfControlsToIgnore();

					var readOnlyProperty = BindableComponentMetaDataPropertyLocator.GetDefaultMetaDataProperty(control.GetType(), MetaDataTypes.ReadOnly);

					if (extension != null && readOnlyProperty != null)
					{
						if (isEditable)
						{
							extension.Restore();
						}
						else
						{
							extension.Backup();

							var currentReadOnly = control.GetReadOnly();
							if (!currentReadOnly)
							{
								control.SetReadOnly(true);
							}
						}
					}
					else
					{
						foreach (Control child in control.Controls)
						{
							UpdateEditableIncludingChildren(child, isEditable, controlsToIgnore);
						}
					}
				}
			}
		}

		static void SetReadOnlyOnControl(object sender, EventArgs eventArgs)
		{
			var control = sender as Control;

			if (control != null && !control.IsDisposed && control.Visible)
			{
				var extension = (control as IExtendedControl)?.Extensions?.OfType<ReadOnlyCacheExtension>().FirstOrDefault();
				UpdateEditableIncludingChildren(control, false, extension?.GetNamesOfControlsToIgnore());
			}
		}

		#endregion

		#region LabelCaption

		static readonly int CachedLabelCaptionsUserDataKey = ControlExtensions.CreateUserDataKey();
		static readonly int CachedLabelCaptionsValidCacheKeyUserDataKey = ControlExtensions.CreateUserDataKey();

		public static string[] GetCachedLabelCaptions(this Control control, long validCacheKey)
		{
			var cacheKey = control.GetUserData(CachedLabelCaptionsValidCacheKeyUserDataKey) ?? (long)0;
			return (long)cacheKey == validCacheKey ?
				(string[])control.GetUserData(CachedLabelCaptionsUserDataKey) : null;
		}

		public static void SetCachedLabelCaption(this Control control, string[] value, long validCacheKey)
		{
			control.SetUserData(CachedLabelCaptionsUserDataKey, value);
			control.SetUserData(CachedLabelCaptionsValidCacheKeyUserDataKey, validCacheKey);
		}

		public static void ManuallySetCaptionToolTip(this Control control, string tooltip)
		{
			if (control is IExtendedControl extendedControl)
			{
				var labelCaptionRenderer = extendedControl.Extensions.Get<ILabelCaptionRenderer>();
				if (labelCaptionRenderer != null)
				{
					labelCaptionRenderer.ManuallySetCaptionToolTip(tooltip);
				}
			}
		}

		#endregion

		#region DockInParent

		public static void DockInside(this UserControl userControl, Control newParent)
		{
			userControl.Parent = newParent;
			userControl.Dock = DockStyle.Fill;
			userControl.Visible = true;
		}

		#endregion

		#region ContainerControl

		public static Control GetRootContainer(this Control control)
		{
			var current = control is Form ? control : control.Parent;
			while (current != null)
			{
				if (TypeDescriptor.GetAttributes(current)[typeof(ContainerControlBaseClassAttribute)] != null)
				{
					return current;
				}
				current = current.Parent;
			}
			return null;
		}

		#endregion

		#region ForceBindingIncludingParents

		public static void ForceBindingIncludingParents(this Control control)
		{
			if (control.IsDisposed)
			{
				return;
			}

			if (control.Parent != null)
			{
				ForceBindingIncludingParents(control.Parent);
			}

			try
			{
				var bindingSource = (KBindingSource)KBindingSource.GetBindingSource(control);
				if (bindingSource != null && !string.IsNullOrEmpty(bindingSource.GetBindingMember(control)))
				{
					if (control.Visible || control is TabPage)
					{
						control.Visible = true;
					}
					bindingSource.ForceBinding(control);
				}
			}
			catch (ObjectDisposedException)
			{
				//might happen if control is asynchronously disposed after earlier check. Ignore - if it's disposed then it's too late to bind it.
			}
		}

		#endregion

		#region SetDoubleBuffered

		public static void SetDoubleBuffered(this Control control, bool isDoubleBuffered)
		{
			var property = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			property.SetValue(control, isDoubleBuffered, null);
		}

		#endregion

		#region TemporarilyDrawAsBitmap

		public static IDisposable TemporarilyDrawAsBitmap(this Control control)
		{
			var image = !control.IsDisposed ? ZScreenShotGrabber.Capture(control) : null;
			if (image == null)
			{
				return DisposableAction.NoAction;
			}

			var pictureBox = new PictureBox { Size = control.Size, Image = image };
			control.Controls.Add(pictureBox);
			pictureBox.BringToFront();

			return new DisposableAction(() =>
			{
				pictureBox.Image.Dispose();
				pictureBox.Dispose();
				control.Controls.Remove(pictureBox);
			});
		}

		#endregion

		#region IsDisposedOrHasDisposedParent

		public static bool IsDisposedOrHasDisposedParent(this Control control)
		{
			if (control == null)
			{
				return true;
			}
			if (control.IsDisposed)
			{
				return true;
			}

			var parent = control.Parent;
			if (parent != null && parent != control && parent.IsDisposedOrHasDisposedParent())
			{
				return true;
			}

			return false;
		}

		#endregion

		#region Control Finding

		public static IEnumerable<Control> Find(this Control control, Func<Control, bool> predicate)
		{
			return control.FindAll(predicate);
		}

		public static IEnumerable<T> FindAll<T>(this Control control, Func<T, bool> predicate = null, int maxLevelsDeep = -1)
		{
			var controls = control.Controls.OfType<T>();

			if (maxLevelsDeep-- != 0)
			{
				controls = controls.Union(control.Controls.Cast<Control>().SelectMany(c => c.FindAll(predicate, maxLevelsDeep)));
			}

			return predicate == null ? controls : controls.Where(predicate);
		}

		public static T FindSingle<T>(this Control control, Func<T, bool> predicate = null, int maxLevelsDeep = -1)
		{
			return control.FindAll(predicate, maxLevelsDeep).Single();
		}

		public static T FindSingle<T>(this Control control, string nameToFind, int maxLevelsDeep = -1)
		{
			return control.FindSingle<T>(x => string.Equals(((Control)(object)x).Name, nameToFind, StringComparison.Ordinal), maxLevelsDeep);
		}

		public static T FindSingleOrDefault<T>(this Control control, Func<T, bool> predicate = null, int maxLevelsDeep = -1)
		{
			return control.FindAll(predicate, maxLevelsDeep).SingleOrDefault();
		}

		public static T FindSingleOrDefault<T>(this Control control, string nameToFind, int maxLevelsDeep = -1)
		{
			return control.FindSingleOrDefault<T>(x => string.Equals(((Control)(object)x).Name, nameToFind, StringComparison.Ordinal), maxLevelsDeep);
		}

#if DEBUG
		public static MenuItem FindMenuItem_ForTest(this Form form, string menuItemText)
		{
			return form.Menu.MenuItems.FindByText(menuItemText, findSubitems: true);
		}
#endif

		#endregion
	}
}
