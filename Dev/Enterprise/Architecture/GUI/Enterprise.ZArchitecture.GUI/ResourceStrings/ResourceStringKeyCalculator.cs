using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public class ResourceStringKeyCalculator
	{
		#region Constructors

		public ResourceStringKeyCalculator(Control control)
		{
			Argument.NotNull(control, "control");
			this.control = control;
		}

		public ResourceStringKeyCalculator(IListEditableControl grid, string mappingName)
			: this((Control)grid)
		{
			Argument.NotNull(grid, "grid");
			Argument.NotNull(mappingName, "mappingName");
			gridColumnMappingName = mappingName;
		}

		ResourceStringKeyCalculator()
		{
		}

		#endregion

		#region ControlPathKey

		public string ControlPathKey
		{
			get
			{
				if (controlPathKey == null)
				{
					var result = new ZStringBuilder();
					if (gridColumnMappingName != null)
					{
						result.Append(gridColumnMappingName);
						result.Append(KeyGroup.KeyDelimiter);
					}
					result.Append(string.Join(KeyGroup.KeyDelimiter, ControlPathForKey));
					result.Append(KeyGroup.KeyDelimiter + GetFormOrUserControlTypeName(IdentifyingControl));
					controlPathKey = result.ToString();
				}
				return controlPathKey;
			}
		}
		string controlPathKey;

		string[] ControlPathForKey
		{
			get
			{
				if (controlPathForKey == null)
				{
					controlPathForKey = GetControlPathForKey();
				}
				return controlPathForKey;
			}
		}
		string[] controlPathForKey;

		string[] GetControlPathForKey()
		{
			var result = new List<string>();

			Control previous = null;
			var current = control;
			while (current != null && (!(current is ITopLevelDataSourceType) || IsCompositeFieldControl(current)))
			{
				if (IsCompositeFieldControl(current))
				{
					result.Clear();
				}
				if (previous == null || previous.Name != current.Name)
				{
					result.Add(current.Name);
				}

				previous = current;
				current = current.Parent;
			}
			return result.ToArray();
		}

		static string GetFormOrUserControlTypeName(Control control)
		{
			var topLevelControl = GetTopLevelDataSourceTypeInterface<ITopLevelDataSourceType>(control.Parent);
			return ((Control)topLevelControl ?? control.GetTopLevelNonParentedControl()).GetType().Name;
		}

		static bool IsCompositeFieldControl(Control control)
		{
			return TypeDescriptor.GetAttributes(control)[typeof(CompositeFieldControlAttribute)] != null;
		}

		#endregion

		#region DataKey

		IReadOnlyList<string> MultipleResourceKeys
		{
			get
			{
				if (!hasMultipleResourceKeyBeenInitialized)
				{
					hasMultipleResourceKeyBeenInitialized = true;
					if (GetISupportMultipleResourceStringData() is ISupportMultipleResourceStringData multipleResourceStringDataSupporter)
					{
						multipleResourceKeys = multipleResourceStringDataSupporter.MultipleKeysToUse;
					}
				}

				return multipleResourceKeys;
			}
		}
		IReadOnlyList<string> multipleResourceKeys;
		bool hasMultipleResourceKeyBeenInitialized;

		public ResourceStringData DataString
		{
			get
			{
				if (dataString == null)
				{
					var boundBusinessObject = GetDataBoundBusinessObject();
					dataString = DataBoundResourceStrings.GetDataForProperty(FinalPropertyComponentType, PropertyName, boundBusinessObject, MultipleResourceKeys) ?? ResourceStringData.Empty;
				}
				return dataString;
			}
		}

		ResourceStringData dataString;

		IDataBoundBusinessObject GetDataBoundBusinessObject()
		{
			if (string.IsNullOrEmpty(PropertyName))
			{
				return null;
			}

			var boundItem = IdentifyingControl is ZGrid grid ? GetDataBoundObjectFromGrid(grid) : GetDataBoundObject(IdentifyingControl);
			var boundBusinessObject = boundItem is BusinessObject bo ? new DataBoundBusinessObject(bo) : null;
			return boundBusinessObject;
		}

		object GetDataBoundObjectFromGrid(ZGrid grid) => grid.ListManager?.GetCurrent() is BusinessObject businessObject ? businessObject : null;

		object GetDataBoundObject(Control targetControl)
		{
			var bindingSource = KBindingSource.GetBindingSource(targetControl);
			return bindingSource is KBindingSource kBindingSource ? kBindingSource.Current : null;
		}

		#endregion

		#region IsControlPathKeyValid

		public bool IsControlPathKeyValid
		{
			get
			{
				if (isControlPathKeyValid == null)
				{
					isControlPathKeyValid = ControlPathForKey.Length <= 8;
					if ((bool)isControlPathKeyValid)
					{
						var current = control;
						while (current != null)
						{
							if (current.Name.StartsWith(current.GetType().Name, StringComparison.OrdinalIgnoreCase) &&
								IsNumber(current.Name.Substring(current.GetType().Name.Length)) &&
								ControlPathKey.Contains(current.Name))
							{
								isControlPathKeyValid = false;
								break;
							}
							current = current.Parent;
						}
					}
				}
				return (bool)isControlPathKeyValid;
			}
		}
		bool? isControlPathKeyValid;

		static bool IsNumber(string number)
		{
			var result = number.Length > 0;
			foreach (var c in number)
			{
				if (!char.IsNumber(c))
				{
					result = false;
					break;
				}
			}
			return result;
		}

		#endregion

		#region DataSourceType

		Type DataSourceType
		{
			get
			{
				if (dataSourceType == null)
				{
					if (control is ZGrid)
					{
						dataSourceType = ((ZGrid)control).ElementTypeFromCollection;
					}
					else
					{
						dataSourceType = GetTopLevelDataSourceType(IdentifyingControl);
					}
				}
				return dataSourceType;
			}
		}
		Type dataSourceType;

		static T GetTopLevelDataSourceTypeInterface<T>(Control control) where T : class
		{
			var current = control;
			while (current != null)
			{
				var dataSourceType = current as T;
				if (dataSourceType != null && !IsCompositeFieldControl(current))
				{
					return dataSourceType;
				}
				current = current.Parent;
			}
			return null;
		}

		static Type GetTopLevelDataSourceType(Control control)
		{
			Type result = null;
			var dataSourceType = GetTopLevelDataSourceTypeInterface<ITopLevelDataSourceType>(control);
			if (dataSourceType != null)
			{
				result = dataSourceType.DataSourceType;
			}
			if ((result == null || result == typeof(object)) &&
				control.Parent != null)
			{
				result = GetTopLevelDataSourceType(control.Parent);
			}
			if (result == null || result == typeof(object))
			{
				result = GetTopLevelDataSourceTypeFromBoundForm(control);
			}
			return result;
		}

		static Type GetTopLevelDataSourceTypeFromBoundForm(Control control)
		{
			Type result = null;
			var form = control.FindForm() as ZForm;
			if (form != null && form.BusinessEntity != null)
			{
				result = form.BusinessEntity.GetType();
			}
			return result;
		}

		#endregion

		#region FinalPropertyComponentType / PropertyPath / PropertyName

		internal string FinalPropertyTableName
		{
			get { return FinalPropertyComponentType != null ? BusinessObjectFactory.GetTableNameFromType(FinalPropertyComponentType, false) : null; }
		}

		internal Type FinalPropertyComponentType
		{
			get
			{
				if (finalPropertyComponentType == null && DataSourceType != null && PropertyPath != null)
				{
					finalPropertyComponentType = DataMemberTypeProvider.GetFinalComponentTypeFromDataMember(DataSourceType, PropertyPath);
				}
				return finalPropertyComponentType;
			}
		}
		Type finalPropertyComponentType;

		string PropertyPath
		{
			get
			{
				if (propertyPath == null)
				{
					propertyPath = "";
					if (gridColumnMappingName != null)
					{
						propertyPath = gridColumnMappingName;
					}
					else if (IdentifyingControl != null)
					{
						var bindingMember = IdentifyingControl as IResourceStringBindingMember;
						propertyPath = bindingMember != null ? bindingMember.ResourceStringBindingMember : (IdentifyingControl == null ? "" : IdentifyingControl.GetBindingMember());
					}
				}
				return propertyPath;
			}
		}
		string propertyPath;

		internal string PropertyName
		{
			get
			{
				if (propertyName == null)
				{
					propertyName = PropertyUtilities.GetPropertyName(PropertyPath);
				}
				return propertyName;
			}
		}
		string propertyName;

		internal SchemaColumn ShemaColumnObject
		{
			get
			{
				if (FinalPropertyComponentType != null &&
					typeof(BusinessObject).IsAssignableFrom(FinalPropertyComponentType) &&
					BusinessObjectFactory.HasTableName(FinalPropertyComponentType))
				{
					return ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(PropertyName, BusinessObjectFactory.GetTableNameFromType(FinalPropertyComponentType));
				}
				return null;
			}
		}

		#endregion

		#region Implementation

		readonly Control control;
		readonly string gridColumnMappingName;

		Control IdentifyingControl
		{
			get
			{
				if (identifyingControl == null)
				{
					var current = control;
					while (current != null)
					{
						if (IsCompositeFieldControl(current))
						{
							identifyingControl = current;
						}
						current = current.Parent;
					}
					if (identifyingControl == null)
					{
						identifyingControl = control;
					}

					var next = identifyingControl;
					while (next != null)
					{
						next = next.GetResourceStringIdentifyingControl();
						if (next != null)
						{
							identifyingControl = next;
						}
					}
				}
				return identifyingControl;
			}
		}
		Control identifyingControl;

		ISupportMultipleResourceStringData GetISupportMultipleResourceStringData()
		{
			ISupportMultipleResourceStringData result = null;
			var checkControl = control;
			while (checkControl != null)
			{
				if (checkControl is ISupportMultipleResourceStringDataSupporter supporter)
				{
					result = supporter.SupportMultipleResourceStringData;
					break;
				}
				checkControl = checkControl.Parent;
			}

			return result ?? GetISupportMultipleResourceStringDataFromForm();
		}

		ISupportMultipleResourceStringData GetISupportMultipleResourceStringDataFromForm() => control.FindForm() is ZForm form ? form.DataSource as ISupportMultipleResourceStringData : null;

		#endregion
	}
}
