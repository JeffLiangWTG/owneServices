using System;
using System.Reflection;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	/// <summary>
	/// Used by the operational actions system to identify how a property should be presented to the user when
	/// running an action that updates the field.
	/// </summary>
	/// <remarks>
	/// See: https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/CorrectingTheFieldDetection.aspx
	/// </remarks>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class ActionFieldAttribute : Attribute
	{
		public const OLookUpEditType NoDropEdit = (OLookUpEditType)(-1);

		public ActionFieldAttribute()
		{
			LookUpEditType = NoDropEdit;
			DateTimeFormat = ZDateTimePickerFormat.Long;
			DefaultAddressType = AddressType.NoDefault;
			FieldType = ActionFieldType.Auto;
			MinValue = float.MinValue;
			MaxValue = float.MaxValue;
			Scale = -1;
		}

		/// <summary>
		/// Specifies the maximum length for a ZString property or the Precision of a numeric property.
		/// </summary>
		public int MaxLength { get; set; }

		/// <summary>
		/// Specifies the CodeDescriptionPairList to use for a ZString property.
		/// </summary>
		public OLookUpEditType LookUpEditType { get; set; }

		/// <summary>
		/// Specifies the ZDateTimePickerFormat to use for a ZDateTime property.
		/// </summary>
		public ZDateTimePickerFormat DateTimeFormat { get; set; }

		/// <summary>
		/// Specifies the BusinessObjectCollection to use for a ZString or ZGuid property.
		/// </summary>
		public Type CollectionType { get; set; }

		/// <summary>
		/// Specifies the default AddressType to use for a ZGuid property.
		/// </summary>
		public AddressType DefaultAddressType { get; set; }

		/// <summary>
		/// Specifies the type of control to present to the user. Defaults to Auto.
		/// </summary>
		public ActionFieldType FieldType { get; set; }

		/// <summary>
		/// The minimum acceptable value for numeric properties.
		/// </summary>
		public float MinValue { get; set; }

		/// <summary>
		/// The maximum acceptable value for numeric properties.
		/// </summary>
		public float MaxValue { get; set; }

		/// <summary>
		/// The scale for numeric properties.
		/// </summary>
		public sbyte Scale { get; set; }

		/// <summary>
		/// Read-only properties cannot be set but may still be used in filters.
		/// </summary>
		public bool ReadOnly { get; set; }

		public static ActionFieldAttribute Get(PropertyInfo info)
		{
			return (ActionFieldAttribute)Attribute.GetCustomAttribute(info, typeof(ActionFieldAttribute));
		}
	}
}
