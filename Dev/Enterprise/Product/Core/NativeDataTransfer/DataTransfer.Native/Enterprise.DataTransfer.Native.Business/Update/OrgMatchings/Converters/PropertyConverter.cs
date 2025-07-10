using System;
using System.Reflection;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters
{
	/// <summary>
	/// It would assign value of property to Object 
	/// When Object have a property with same column name
	/// </summary>
	public class PropertyConverter
	{
		public PropertyConverter(Property property)
		{
			this.property = property;
		}
		readonly Property property;

		public void AssignProperty(object result)
		{
			var propertyInfo = FindPropertyInfo(result);

			if (propertyInfo == null)
			{
				return;
			}

			var zValue = ObjectToZType(propertyInfo.PropertyType, property.Value);
			propertyInfo.SetValue(result, zValue, null);
		}

		PropertyInfo FindPropertyInfo(object result)
		{
			var propertyDefinition = property.Definition;
			var columnName = propertyDefinition.ColumnDef.Name;

			var type = result.GetType();

			return type.GetProperty(columnName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Checking against well-known string literal.")]
		public static IZType ObjectToZType(Type property, object propertyValue)
		{
			if (property == typeof(ZBool) && propertyValue is string str)
			{
				switch (str)
				{
					case "true":
					case "1":
						return ZBool.True;
					case "false":
					case "0":
					default:
						return ZBool.False;
				}
			}

			return ZDataType.ObjectToZType(property, propertyValue);
		}
	}
}
