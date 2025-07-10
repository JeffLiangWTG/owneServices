using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZBindingSource : KBindingSource, IDefaultBindingSettings
	{
		public ZBindingSource()
		{
		}

		public ZBindingSource(Control containerControl)
			: base(containerControl)
		{
		}

		public ZBindingSource(IContainer container)
			: base(container)
		{
		}

		public ZBindingSource(Control containerControl, Type dataSourceType)
			: base(containerControl, dataSourceType)
		{
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool DefaultBindingMembersEnabled
		{
			get { return defaultBindingMembersEnabled; }
			set { defaultBindingMembersEnabled = value; }
		}
		bool defaultBindingMembersEnabled = true;

		public override string GetBindingMember(Control control)
		{
			var result = base.GetBindingMember(control);
			if (this.IsDesignMode() &&
				string.IsNullOrEmpty(result) &&
				CanExtend(control) &&
				!ExcludedFromDefaultBindingMember.Contains(control))
			{
				var defaultBindingMember = DefaultDataSourceBindingMemberAttribute.GetDefaultBindingMember(control.GetType());
				if (!string.IsNullOrEmpty(defaultBindingMember))
				{
					result = defaultBindingMember;
				}
			}
			return result;
		}

		public override void SetBindingMember(Control control, string value)
		{
			var fetchHintGenerator = control as IFetchHintGenerator;
			if (fetchHintGenerator != null &&
				DataSource != null &&
				!string.IsNullOrEmpty(value))
			{
				fetchHintGenerator.AddFetchHint(DataSource, new KBindingMemberInfo(DataMember ?? "", value).BindingMember);
			}

			if (string.IsNullOrEmpty(value))
			{
				ExcludedFromDefaultBindingMember.Add(control);
			}
			else if (excludedFromDefaultBindingMember != null)
			{
				excludedFromDefaultBindingMember.Remove(control);
			}

			base.SetBindingMember(control, value);
			CheckCustomTypeDescriptionProviderPresent(control);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null && ContainerControl != null && DefaultBindingMembersEnabled)
			{
				SetDefaultBindingMembersOnChildren(ContainerControl);
			}
			if (dataSource != null)
			{
				AddFetchHints(dataSource, dataMember);
			}
			base.SetDataBinding(dataSource, dataMember);
		}

		internal void AddFetchHints(object dataSource, string dataMember)
		{
			foreach (var control in FullBindingMembers)
			{
				var fetchHintGenerator = control.Key as IFetchHintGenerator;
				if (fetchHintGenerator != null)
				{
					fetchHintGenerator.AddFetchHint(dataSource, new KBindingMemberInfo(dataMember ?? "", GetBindingMember(control.Key)).BindingMember);
				}
			}
		}

		#region CheckCustomTypeDescriptionProviderPresent

#if DEBUG
		[SuppressThreadStaticFieldMessage]
		internal static bool TypeDescriptionProviderCheckEnabled = true;
#endif

		void CheckCustomTypeDescriptionProviderPresent(Control control)
		{
#if DEBUG
			if (TypeDescriptionProviderCheckEnabled && RequiresTypeDescriptionProvider(control))
			{
				CheckHasNoBindings(control.DataBindings);
				control.DataBindings.CollectionChanged += new CollectionChangeEventHandler(DataBindings_CollectionChanged);
			}
#endif
		}

#if DEBUG
		bool RequiresTypeDescriptionProvider(Control control)
		{
			var result = false;
			if (!CheckedTypeDescriptionProviderControlTypes.TryGetValue(control.GetType(), out result))
			{
				var isApplied = TypeDescriptionProviderAttributeChecker.IsAppliedTo(control.GetType());
				var isSuppressed = TypeDescriptor.GetAttributes(control.GetType())[typeof(SuppressTypeDescriptionProviderAttributeChecker)] != null;
				CheckedTypeDescriptionProviderControlTypes[control.GetType()] = !isApplied && !isSuppressed;
			}
			return result;
		}
#endif

		static Dictionary<Type, bool> CheckedTypeDescriptionProviderControlTypes
		{
			get { return checkedTypeDescriptionProviderControlTypes ?? (checkedTypeDescriptionProviderControlTypes = new Dictionary<Type, bool>()); }
		}
		[ThreadStatic]
		static Dictionary<Type, bool> checkedTypeDescriptionProviderControlTypes;

		void DataBindings_CollectionChanged(object sender, CollectionChangeEventArgs e)
		{
			var collection = (ControlBindingsCollection)sender;
			CheckHasNoBindings(collection);
		}

		void CheckHasNoBindings(ControlBindingsCollection collection)
		{
			if (collection.Count > 0)
			{
				collection.CollectionChanged -= new CollectionChangeEventHandler(DataBindings_CollectionChanged);
				ErrorReporter.ReportOnce(
					"ZControlTypeDescriptionProviderShouldBeUsed" + collection.Control.GetType().FullName,
					"ZControlTypeDescriptionProvider should be used in TypeDescriptionProvider attribute for type " +
					collection.Control.GetType().FullName);
			}
		}

		#endregion

		#region SetDefaultBindingMembers

		void IDefaultBindingSettings.ExcludeFromDefaultBinding(Control control)
		{
			ExcludedFromDefaultBindingMember.Add(control);
		}

		List<Control> ExcludedFromDefaultBindingMember
		{
			get { return excludedFromDefaultBindingMember ?? (excludedFromDefaultBindingMember = new List<Control>()); }
		}
		List<Control> excludedFromDefaultBindingMember;

		void SetDefaultBindingMembersOnChildren(object sender, EventArgs e)
		{
			SetDefaultBindingMembersOnChildren((Control)sender);
		}

		void SetDefaultBindingMembersOnChildren(Control control)
		{
			if (!(control is ZBindingTabPage))
			{
				control.ControlAdded -= new ControlEventHandler(Control_ControlAdded);
				control.ControlAdded += new ControlEventHandler(Control_ControlAdded);
				foreach (Control child in control.Controls)
				{
					SetDefaultBindingMembers(child);
				}
			}
		}

		void ResetDefaultBindingMembersOnChildren(Control control)
		{
			if (!(control is ZBindingTabPage))
			{
				control.ControlAdded -= new ControlEventHandler(Control_ControlAdded);
				foreach (Control child in control.Controls)
				{
					ResetDefaultBindingMembersOnChildren(child);
				}
			}
		}

		void SetDefaultBindingMembers(Control control)
		{
			if (!(control is ZBindingTabPage))
			{
				var tabPage = control as ZTabPage;
				if (tabPage != null)
				{
					tabPage.RunWhenBindingOrFirstShown(new EventHandler(SetDefaultBindingMembersOnChildren));
				}

				var zcontrol = control as ZUserControl;
				if (zcontrol != null)
				{
					if (string.IsNullOrEmpty(GetBindingMember(control)) && ((IDataBoundControl)zcontrol).DataSource == null)
					{
						var defaultBindingMember = DefaultDataSourceBindingMemberAttribute.GetDefaultBindingMember(control.GetType());
						if (!string.IsNullOrEmpty(defaultBindingMember) &&
							(excludedFromDefaultBindingMember == null || !excludedFromDefaultBindingMember.Contains(control)))
						{
							SetBindingMember(control, defaultBindingMember);
						}
					}
				}
				else
				{
					SetDefaultBindingMembersOnChildren(control);
				}
			}
		}

		void Control_ControlAdded(object sender, ControlEventArgs e)
		{
			if (DataSource != null)
			{
				SetDefaultBindingMembers(e.Control);
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (ContainerControl != null)
				{
					ResetDefaultBindingMembersOnChildren(ContainerControl);
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Implementation

		protected override CompileTimeCheckBindingMember NewCompileTimeCheckBindingMember(Type dataSourceType, Type controlPropertyType, string bindingMember)
		{
			return new ZCompileTimeCheckBindingMember(dataSourceType, controlPropertyType, bindingMember);
		}

		#endregion
	}
}
