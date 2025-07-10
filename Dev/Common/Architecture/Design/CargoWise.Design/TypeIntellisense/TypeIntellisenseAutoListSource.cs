using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Design;
using CargoWise.ComponentModel.Design;
using CargoWise.Design.DTE;
using CargoWise.Windows.UI;

namespace CargoWise.Design.TypeIntellisense
{
	public class TypeIntellisenseAutoListSource : ListBoxAutoListSource
	{
		public TypeIntellisenseAutoListSource(ITypeDescriptorContext context)
			: base(context)
		{
			UISyncContext = SynchronizationContext.Current;
		}

		#region GetTypesToAutoList

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal error message")]
		protected internal virtual IEnumerable<Type> GetTypesToAutoList()
		{
			ITypeResolutionService typeResolutionService = TypeResolutionService;
			if (typeResolutionService == null)
			{
				yield return new TypeNameHolder("Intellisense is not available at this time.");
			}
			else
			{
				IEnumerable<Type> types = GetUnfilteredTypesToAutoList();
				types = FilterTypes(types);
				IEnumerator<Type> enumerator = types.GetEnumerator();

				bool move = true;
				do
				{
					try
					{
						move = enumerator.MoveNext();
					}
					catch (TypeLoadException) { }
					catch (MissingMemberException) { }
					catch (COMException) { }
					if (move)
					{
						yield return enumerator.Current;
					}
				}
				while (move);
			}
		}

		protected virtual IEnumerable<Type> GetUnfilteredTypesToAutoList()
		{
			ServiceContainer services = new ServiceContainer(Context);
			services.AddService(typeof(SynchronizationContext), UISyncContext);
			return DesignTimeTypeEnumerable.GetCachedEnumerableFromCurrentProject(services);
		}

		#endregion

		#region FilterTypes

		IEnumerable<Type> FilterTypes(IEnumerable<Type> types)
		{
			if (SubtypeFilters != null && SubtypeFilters.Length > 0)
			{
				foreach (Type type in GetSubtypesFromFilterAttributes(types))
				{
					if (IsTypeEditorBrowsable(type))
					{
						yield return type;
					}
				}
			}
			else
			{
				foreach (Type type in types)
				{
					yield return type;
				}
			}
		}

		IEnumerable<Type> GetSubtypesFromFilterAttributes(IEnumerable<Type> types)
		{
			ITypeResolutionService typeResolutionService = TypeResolutionService;
			foreach (TypeValueIntellisenseEditorSubtypeFilterAttribute attr in SubtypeFilters)
			{
				IEnumerable<Type> enumerable = (attr.BaseTypes == null) ? types : new SubtypeEnumerable(types, attr.BaseTypes);
				enumerable = FilterByTypeFilterMethod(attr, enumerable);

				foreach (Type type in enumerable)
				{
					yield return type;
				}
			}
		}

		IEnumerable<Type> FilterByTypeFilterMethod(TypeValueIntellisenseEditorSubtypeFilterAttribute filter, IEnumerable<Type> types)
		{
			IEnumerable<Type> result = types;
			if (!string.IsNullOrEmpty(filter.TypeFilterMember))
			{
				UIThreadInvoke(delegate
				{
					MethodInfo method = Context.Instance.GetType().GetMethod(filter.TypeFilterMember, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ITypeResolutionService), typeof(IEnumerable<Type>) }, null);
					if (method != null)
					{
						result = (IEnumerable<Type>)method.Invoke(Context.Instance, new object[] { TypeResolutionService, types });
					}
				});
			}
			return result;
		}

		#endregion

		#region SubtypeFilters

		TypeValueIntellisenseEditorSubtypeFilterAttribute[] SubtypeFilters
		{
			get
			{
				if (subtypeFilters == null && Context != null && Context.Instance != null)
				{
					IEnumerable<Attribute> all_attributes = AttributeUtil.FindAttributes(
						Context.Instance.GetType(),
						Context.PropertyDescriptor.Name,
						Context.PropertyDescriptor.PropertyType,
						typeof(Attribute),
						BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					List<TypeValueIntellisenseEditorSubtypeFilterAttribute> list = new List<TypeValueIntellisenseEditorSubtypeFilterAttribute>();
					foreach (Attribute attr in all_attributes)
					{
						if (attr.GetType().Name == nameof(TypeValueIntellisenseEditorSubtypeFilterAttribute))
						{
							// change the assembly of the attribute and it's BaseTypes property
							list.Add(FixUpTypeFilterAttribute(attr));
						}
					}
					subtypeFilters = list.ToArray();
				}
				return subtypeFilters;
			}
		}
		TypeValueIntellisenseEditorSubtypeFilterAttribute[] subtypeFilters;

		TypeValueIntellisenseEditorSubtypeFilterAttribute FixUpTypeFilterAttribute(Attribute attr)
		{
			Type[] baseTypes = (Type[])TypeDescriptor.GetProperties(attr)["BaseTypes"].GetValue(attr);
			string typeFilterMember = (string)TypeDescriptor.GetProperties(attr)["TypeFilterMember"].GetValue(attr);
			string importNamespacesMember = (string)TypeDescriptor.GetProperties(attr)["ImportNamespacesMember"].GetValue(attr);

			TypeValueIntellisenseEditorSubtypeFilterAttribute attrWithBaseTypes =
				new TypeValueIntellisenseEditorSubtypeFilterAttribute(typeFilterMember, baseTypes);
			UIThreadInvoke(delegate
			{ baseTypes = attrWithBaseTypes.GetBaseTypes(TypeResolutionService); });
			TypeValueIntellisenseEditorSubtypeFilterAttribute result =
				new TypeValueIntellisenseEditorSubtypeFilterAttribute(typeFilterMember, baseTypes);
			result.ImportNamespacesMember = importNamespacesMember;
			return result;
		}

		#endregion

		#region IsTypeEditorBrowsable

		static bool IsTypeEditorBrowsable(Type type)
		{
			bool result = true;
			VSCodeType codeType = type as VSCodeType;
			if (codeType != null)
			{
				foreach (EnvDTE.CodeAttribute attribute in codeType.CodeType.Attributes)
				{
					if (attribute.Value.IndexOf("EditorBrowsableState.Never", StringComparison.OrdinalIgnoreCase) != -1)
					{
						result = false;
						break;
					}
				}
			}
			else
			{
				try
				{
					EditorBrowsableAttribute attr = (EditorBrowsableAttribute)TypeDescriptor.GetAttributes(type)[typeof(EditorBrowsableAttribute)];
					if (attr != null && attr.State == EditorBrowsableState.Never)
					{
						result = false;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// GetAttributes may cause references to be loaded, which won't always work
				}
			}
			return result;
		}

		#endregion

		#region IAutoListSource

		protected override ListBox NewListControl()
		{
			IntellisenseListBox result = new IntellisenseListBox();
			result.HandleCreated += new EventHandler(OnListBox_HandleCreated);
			return result;
		}

		protected override bool UpdateListControl(ListBox listControl, TextBoxBase textBox)
		{
			IntellisenseListBox intellisenseListBox = listControl as IntellisenseListBox;
			if (intellisenseListBox.DataSource != null)
			{
				intellisenseListBox.DataSource.PartialNameAsync = GetPartialNameFromTextBox(textBox);
			}
			return true;
		}

		protected override bool HasSelection(ListBox listControl)
		{ return !((IntellisenseListBox)listControl).OutlineSelectedItem && base.HasSelection(listControl); }

		protected override void OnTextBoxKeyDown(ListBox listControl, KeyEventArgs e)
		{
			IntellisenseListBox intellisenseListControl = listControl as IntellisenseListBox;
			if (e.KeyCode == Keys.OemPeriod)
			{
				OnValueCommitRequired(EventArgs.Empty);
			}
			else if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
			{
				if (intellisenseListControl.OutlineSelectedItem)
				{
					intellisenseListControl.OutlineSelectedItem = false;
				}
				else
				{
					base.OnTextBoxKeyDown(listControl, e);
				}
			}
			else
			{
				base.OnTextBoxKeyDown(listControl, e);
			}
		}

		protected override void ReplaceText(ListBox listControl, TextBoxBase textBox)
		{
			IntellisenseDataSource dataSource = ((IntellisenseListBox)listControl).DataSource;
			IntellisenseDataSourceItem item = (IntellisenseDataSourceItem)listControl.SelectedItem;

			int nameStart = CalculateNameStart(textBox);
			string newName = dataSource.NamePath + "." + item.DisplayName;
			if (string.IsNullOrEmpty(dataSource.NamePath))
			{
				newName = item.NameToReplaceWith;
			}

			int oldSelectionStart = textBox.SelectionStart;
			textBox.SelectionStart = nameStart;
			textBox.SelectionLength = oldSelectionStart + nameStart;
			textBox.SelectedText = newName;

			textBox.SelectionStart = nameStart;
			textBox.SelectionStart = newName.Length;
		}

		#endregion

		#region Implementation

		readonly SynchronizationContext UISyncContext;

		void UIThreadInvoke(SendOrPostCallback callback)
		{
			if (UISyncContext != null)
			{
				UISyncContext.Send(callback, null);
			}
			else
			{
				callback(null);
			}
		}

		ITypeResolutionService TypeResolutionService
		{
			get
			{
				if (typeResolutionService == null && Context != null)
				{
					typeResolutionService = TypeResolutionServiceLocator.Get(Context);
					if (typeResolutionService == null && !IsOnUIThread)
					{
						UIThreadInvoke(delegate
						{
							typeResolutionService = TypeResolutionServiceLocator.Get(Context);
						});
					}
				}
				return typeResolutionService;
			}
		}
		ITypeResolutionService typeResolutionService;

		bool IsOnUIThread
		{ get { return UISyncContext == SynchronizationContext.Current; } }

		static int CalculateNameStart(TextBoxBase textBox)
		{
			string text = textBox.Text;
			int index1 = text.LastIndexOf("(", textBox.SelectionStart, StringComparison.Ordinal);
			int index2 = text.LastIndexOf(" ", textBox.SelectionStart, StringComparison.Ordinal);
			int index3 = text.LastIndexOf("\t", textBox.SelectionStart, StringComparison.Ordinal);
			int index4 = text.LastIndexOf("\r", textBox.SelectionStart, StringComparison.Ordinal);
			int index5 = text.LastIndexOf("\n", textBox.SelectionStart, StringComparison.Ordinal);

			int index = 0;
			if (index1 != -1 && index1 < index)
			{
				index = index1;
			}

			if (index2 != -1 && index2 < index)
			{
				index = index2;
			}

			if (index3 != -1 && index3 < index)
			{
				index = index3;
			}

			if (index4 != -1 && index4 < index)
			{
				index = index4;
			}

			if (index5 != -1 && index5 < index)
			{
				index = index5;
			}

			return index;
		}

		static string GetPartialNameFromTextBox(TextBoxBase textBox)
		{
			int index = CalculateNameStart(textBox);
			string result = textBox.Text.Substring(index, textBox.SelectionStart - index);
			return result;
		}

		void OnListBox_HandleCreated(object sender, EventArgs e)
		{
			IntellisenseListBox ctrl = (IntellisenseListBox)sender;
			if (ctrl.DataSource == null)
			{
				IEnumerable<Type> types = GetTypesToAutoList();
				CachedEnumerableWrapper<Type> cachedEnumerableTypes = new CachedEnumerableWrapper<Type>(types);
				ctrl.DataSource = new IntellisenseDataSource(cachedEnumerableTypes, UISyncContext, GetUsingNamespaces());
			}
		}

		protected virtual string[] GetUsingNamespaces()
		{
			List<string> result = new List<string>(GetUsingNamespacesFromFilterAttributes());
			if (result.Count == 0 && Context != null)
			{
				EnvDTE.ProjectItem item = (EnvDTE.ProjectItem)Context.GetService(typeof(EnvDTE.ProjectItem));
				if (item != null)
				{
					result.AddRange(EnvDTEUtil.GetUsingNamespacesFromProjectItem(item));
				}
			}
			return result.ToArray();
		}

		IEnumerable<string> GetUsingNamespacesFromFilterAttributes()
		{
			if (SubtypeFilters != null)
			{
				foreach (TypeValueIntellisenseEditorSubtypeFilterAttribute attr in SubtypeFilters)
				{
					MethodInfo method = attr.ImportNamespacesMember == null ? null : Context.Instance.GetType().GetMethod(attr.ImportNamespacesMember, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Array.Empty<Type>(), null);
					if (method != null)
					{
						IEnumerable<string> items = (IEnumerable<string>)method.Invoke(Context.Instance, null);
						foreach (string item in items)
						{
							yield return item;
						}
					}
				}
			}
		}

		#endregion
	}
}
