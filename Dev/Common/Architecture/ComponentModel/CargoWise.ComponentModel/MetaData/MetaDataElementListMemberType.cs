using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// A specific type of MetaDataType to be used for describing members of element lists.
	/// For example, ListDataSource can have element lists with ListDisplayMember and
	/// ListValueMember.
	/// </summary>
	internal class MetaDataListElementMemberType : ConstantOnlyMetaDataType
	{
		public MetaDataListElementMemberType(string id)
			: this(id, typeof(object))
		{
		}

		public MetaDataListElementMemberType(string id, Type expectedValueMemberType)
			: base(id, typeof(string), "")
		{
			Argument.NotNull(expectedValueMemberType, nameof(expectedValueMemberType));
			ExpectedValueMemberType = expectedValueMemberType;
		}

		/// <summary>
		/// Get the expected type of the list element member. For example, ComboBox's DisplayMember should be
		/// of type string because doing an automatic x.ToString() is smelly.
		/// </summary>
		public readonly Type ExpectedValueMemberType;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error reporting")]
		public override string ValidateMetaDataValue(Type entityType, PropertyDescriptor property, object value)
		{
			string result = null;
			var listMetaDataType = MetaDataType.GetMetaDataType(MetaDataTypes.ListDataSource);
			if (property != null && typeof(IList).IsAssignableFrom(listMetaDataType.DataType))
			{
				if (MetaDataValueMemberLocator.GetInstance(property, listMetaDataType.Id).IsSpecified)
				{
					var elementType = MetaDataValueMemberLocator.GetInstance(property, listMetaDataType.Id).ListElementType;
					if (elementType == null || elementType == typeof(object))
					{
						result = "Could not determine list element type";
					}
					else if (value != null && !string.IsNullOrEmpty(value.ToString()))
					{
						// ensure the list element type has the value member
						var memberProperty = TypeDescriptor.GetProperties(elementType)[value.ToString()];
						if (memberProperty == null)
						{
							result = "Could not find member '" + value + "' on '" + elementType.FullName + "'";
						}
						else if (!ExpectedValueMemberType.IsAssignableFrom(memberProperty.PropertyType))
						{
							result = "Member is not of correct type, expected '" + ExpectedValueMemberType.FullName + "'";
						}
					}
				}
			}
			return result;
		}
	}
}
