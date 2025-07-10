using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Xml.Testing
{
	public class ValueObjectPropertyNavigator
	{
		public ValueObjectPropertyNavigator(Type propertyType) : this(propertyType, Array.Empty<ValueObjectPropertyNavigator>())
		{
		}

		public ValueObjectPropertyNavigator(Type propertyType, ValueObjectPropertyNavigator[] path)
		{
			this.PropertyType = propertyType;
			this.Path = path;
			this.ElementName = "";
		}

		public ValueObjectPropertyNavigator(PropertyDescriptor property, ValueObjectPropertyNavigator[] path)
		{
			if (typeof(IList).IsAssignableFrom(property.PropertyType))
			{
				this.PropertyType = DetermineIndexerType(property.PropertyType);
			}
			else
			{
				this.PropertyType = property.PropertyType;
			}
			this.Path = path;
			this.ElementName = property.Name;
			this.Property = property;
		}

		public readonly Type PropertyType;
		public readonly ValueObjectPropertyNavigator[] Path;
		public readonly string ElementName;
		public readonly PropertyDescriptor Property;

		public override bool Equals(object obj)
		{
			ValueObjectPropertyNavigator rhs = obj as ValueObjectPropertyNavigator;
			return
				rhs != null &&
				this.PropertyType == rhs.PropertyType &&
				this.ElementName == rhs.ElementName &&
				Path.Length == rhs.Path.Length &&
				(Path.Length == 0 || Path[Path.Length - 1].Equals(rhs.Path[Path.Length - 1]));
		}

		public override int GetHashCode()
		{
			return PropertyType.GetHashCode() ^ ElementName.GetHashCode();
		}

		public bool CanValueBeEmpty()
		{
			bool result =
				typeof(ZString).IsAssignableFrom(Property.PropertyType) ||
				typeof(ZDateTime).IsAssignableFrom(Property.PropertyType) ||
				typeof(ZDateTimeOffset).IsAssignableFrom(Property.PropertyType) ||
				TypeDescriptor.GetProperties(Property.ComponentType)[Property.Name + "Specified"] != null;
			return result;
		}

		public bool HasValue(IValueObject valueObject)
		{
			object value = GetValue(valueObject);

			bool result = false;
			if (valueObject.IsSpecified && (value != null))
			{
				result = true;
				PropertyDescriptor isValueSpecifiedField = TypeDescriptor.GetProperties(Property.ComponentType)[Property.Name + "Specified"];
				if (isValueSpecifiedField != null)
				{
					result = (bool)isValueSpecifiedField.GetValue(valueObject);
				}
				else if (value is ZDateTime && !((ZDateTime)value).IsValid)
				{
					result = false;
				}
				else if (value is ZDateTimeOffset && !((ZDateTimeOffset)value).IsValid)
				{
					result = false;
				}
				else if (value is ZString && ((ZString)value).IsEmpty)
				{
					result = false;
				}
				else if (value is Array && ((Array)value).Length == 0)
				{
					result = false;
				}
				else if (Property.PropertyType.IsArray && value == null)
				{
					result = false;
				}
			}
			return result;
		}

		public string PathAsString
		{
			get
			{
				string result = "";
				foreach (ValueObjectPropertyNavigator property in Path)
				{
					if (!string.IsNullOrEmpty(result))
					{
						result += "/";
					}

					result += property.ElementName;
				}
				if (!string.IsNullOrEmpty(result))
				{
					result += "/";
				}

				result += ElementName;
				return result;
			}
		}

		public ValueObjectPropertyNavigator[] GetNextPropertiesInPath()
		{
			ValueObjectPropertyNavigator[] nextPath = new ValueObjectPropertyNavigator[Path.Length + 1];
			Path.CopyTo(nextPath, 0);
			nextPath[nextPath.Length - 1] = this;

			ArrayList result = new ArrayList();
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(PropertyType))
			{
				if (property.Name.IndexOf("__internal") == -1 &&
					!property.Name.EndsWith("Specified"))
				{
					ValueObjectPropertyNavigator next = new ValueObjectPropertyNavigator(property, nextPath);
					result.Add(next);
				}
			}
			return (ValueObjectPropertyNavigator[])result.ToArray(typeof(ValueObjectPropertyNavigator));
		}

		public object GetValue(IValueObject value)
		{
			return Property.GetValue(value);
		}

		public bool IsRecurringInPath()
		{
			bool result = false;
			foreach (ValueObjectPropertyNavigator next in Path)
			{
				if (next.Property == this.Property)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		#region Implementation

		Type DetermineIndexerType(Type listType)
		{
			Type result = null;
			foreach (PropertyInfo property in listType.GetProperties())
			{
				if (property.Name == "Item")
				{
					result = property.PropertyType;
					break;
				}
			}
			return result;
		}

		#endregion
	}
}
