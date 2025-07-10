using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.Internal
{
	#region SuppressResourceStringsCheckRegion

	public sealed class ControlInformationDigger : NonPersistentBusinessObject
	{
		public ControlInformationDigger(Control controlTag)
		{
			PropertiesValues = CreateCollection(controlTag);
			RegisterEditableChildObject(PropertiesValues);
		}

		static ControlPropertyCollection CreateCollection(Control controlTag)
		{
			var collection = new ControlPropertyCollection();
			collection.AddRange(controlTag.GetType().GetProperties().Where(IsNotIndexProperty).Select(p => new ControlProperty(p, controlTag)));
			return collection;
		}

		static bool IsNotIndexProperty(PropertyInfo p) => p.GetIndexParameters().Length == 0;

		public ControlPropertyCollection PropertiesValues { get; }
	}

	public class ControlPropertyCollection : NonPersistentBusinessObjectCollection<ControlProperty>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException();
		}

		protected override bool AllowNewCore => false;
	}

	public class ControlProperty : NonPersistentBusinessObject
	{
		readonly PropertyInfo currentProperty;
		readonly Control propertyControl;
		const string placeholderForNull = "NULL";
		const string placeholderForUnreadable = "PROPERTY UNREADABLE";

		TypeConverter TypeConverter => TypeDescriptor.GetConverter(currentProperty.PropertyType);

		public ControlProperty(PropertyInfo property, Control control)
		{
			currentProperty = Argument.NotNull(property, nameof(property));
			propertyControl = Argument.NotNull(control, nameof(control));
		}

		public ZString Name => currentProperty.Name;

		[ReadOnlyMember(nameof(Value_ReadOnly))]
		public ZString Value
		{
			get
			{
				try
				{
					return currentProperty.GetValue(propertyControl)?.ToString() ?? placeholderForNull;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return placeholderForUnreadable;
				}
			}
			set
			{
				ConvertAndSetValue(value);
				ValueInfo.RefreshBinding();
			}
		}

		public CodeDescriptionPairList ValueList
		{
			get
			{
				CodeDescriptionPairList valueList = new CodeDescriptionPairList();
				try
				{
					var converter = TypeDescriptor.GetConverter(currentProperty.PropertyType);
					if (converter != null)
					{
						var values = converter.GetStandardValuesSupported() ? converter.GetStandardValues() : null;
						if (values != null && values.Count > 0)
						{
							foreach (var value in converter.GetStandardValues())
							{
								valueList.AddPair(converter.ConvertToString(value), value.ToString());
							}
						}
						else
						{
							var currentValue = currentProperty.GetValue(propertyControl);
							if (currentValue != null)
							{
								valueList.AddPair(converter.ConvertToString(currentValue), currentValue.ToString());
							}
							else
							{
								valueList.AddPair(placeholderForNull);
							}
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					valueList.AddPair(placeholderForUnreadable);
				}
				return valueList;
			}
		}

		bool Value_ReadOnly => !currentProperty.CanWrite || !TypeConverter.CanConvertFrom(typeof(string));

		void ConvertAndSetValue(ZString input)
		{
			var newValue = input == placeholderForNull ? null : TypeConverter.ConvertFrom((string)input);

			currentProperty.SetValue(propertyControl, newValue);
		}

		public ZPropertyInfo ValueInfo
		{
			get { return GetZPropertyInfo(nameof(Value)); }
		}

		public ZString PropertyType => currentProperty.PropertyType.ToString();
	}

	#endregion
}

