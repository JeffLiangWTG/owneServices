using System.Collections;

namespace CargoWise.ComponentModel
{
	#region SuppressResourceStringsCheckRegion
	/// <summary>
	/// Contains some 'stock' MetaDataType type identifiers.
	/// </summary>
	public abstract class MetaDataTypes
	{
		/// <summary>
		/// Whether a property is read-only (from the user's perspective). This is of type
		/// boolean.
		/// </summary>
		public const string ReadOnly = "ReadOnly";

		/// <summary>
		/// The name of the field
		/// </summary>
		public const string Name = "Name";

		/// <summary>
		/// An array of descriptions in order of string length.
		/// </summary>
		public const string Description = "Description";

		/// <summary>
		/// The number of decimal places to show after the seconds of a DateTimeOffset
		/// </summary>
		public const string DateTimeOffsetPlaces = "DateTimeOffsetPlaces";

		/// <summary>
		/// The number of decimal places to show of a number (integer).
		/// </summary>
		public const string DecimalPlaces = "DecimalPlaces";

		/// <summary>
		/// The precision of a decimal (integer).
		/// </summary>
		public const string DecimalPrecision = "DecimalPrecision";

		/// <summary>
		/// The maximum length of a text box. 0 is the default value and used to signify no limit.
		/// </summary>
		public const string MaxLength = "MaxLength";

		/// <summary>
		/// The format for a date time picker control.
		/// </summary>
		public const string DateTimeFormat = "DateTimeFormat";

		/// <summary>
		/// The custom format string for a date time picker control.
		/// </summary>
		public const string DateTimeCustomFormat = "DateTimeCustomFormat";

		/// <summary>
		/// The DataSource of a list source (for example the list on a combo box or list box).
		/// </summary>
		public const string ListDataSource = "ListDataSource";

		/// <summary>
		/// The ValueMember of a list source.
		/// </summary>
		public const string ListValueMember = "ListValueMember";

		/// <summary>
		/// The DisplayMember of a list source.
		/// </summary>
		public const string ListDisplayMember = "ListDisplayMember";

		/// <summary>
		/// The Position a control is to displayed (null if order is not relevant).
		/// </summary>
		public const string Position = "Position";

		/// <summary>
		/// Whether a property is visible (by default). This is of type boolean.
		/// </summary>
		public const string Visible = "Visible";

		/// <summary>
		/// Parent custom field type of a multi-part custom field.
		/// </summary>
		public const string ParentCustomFieldType = "ParentCustomFieldType";

		/// <summary>
		/// Position multi-part custom field.
		/// </summary>
		public const string CustomFieldPosition = "CustomFieldPosition";

		/// <summary>
		/// The value to use when no value is expected. For example, when no value is selected in a
		/// combo box.
		/// </summary>
		public const string Null = "EmptyValue";

		/// <summary>
		/// A collection of notifications for a property.
		/// </summary>
		public const string Notifications = "Notifications";

		/// <summary>
		/// This property is a password and shoult not be shown to the end user.
		/// </summary>
		public const string Password = "Password";

		/// <summary>
		/// Whether a property is modifiable. Default modifiable
		/// boolean.
		/// </summary>
		public const string DisableModifiable = "DisableModifiable";

		internal static void RegisterTypes()
		{
			MetaDataType.RegisterMetaDataType(new MetaDataType(ReadOnly, typeof(bool), false));
			MetaDataType.RegisterMetaDataType(new MetaDataType(Name, typeof(string), ""));
			MetaDataType.RegisterMetaDataType(new DescriptionMetaDataType(Description, null));
			MetaDataType.RegisterMetaDataType(new PositiveOrNeg1IntegerMetaDataType(DateTimeOffsetPlaces));
			MetaDataType.RegisterMetaDataType(new PositiveOrNeg1IntegerMetaDataType(DecimalPrecision));
			MetaDataType.RegisterMetaDataType(new PositiveOrNeg1IntegerMetaDataType(DecimalPlaces));
			MetaDataType.RegisterMetaDataType(new PositiveOrNeg1IntegerMetaDataType(MaxLength, -1));
			MetaDataType.RegisterMetaDataType(new MetaDataType(DateTimeFormat, typeof(KDateTimeFormat), KDateTimeFormat.Short));
			MetaDataType.RegisterMetaDataType(new MetaDataType(DateTimeCustomFormat, typeof(string), ""));
			MetaDataType.RegisterMetaDataType(new MetaDataMandatoryType(
				ListDataSource, typeof(IList), null, new string[] { ListDisplayMember, ListValueMember }));
			MetaDataType.RegisterMetaDataType(new MetaDataListElementMemberType(ListValueMember));
			MetaDataType.RegisterMetaDataType(new MetaDataListElementMemberType(ListDisplayMember, typeof(string)));
			MetaDataType.RegisterMetaDataType(new PositiveOrNeg1IntegerMetaDataType(Position));
			MetaDataType.RegisterMetaDataType(new MetaDataType(Visible, typeof(bool), true));
			MetaDataType.RegisterMetaDataType(new MetaDataType(ParentCustomFieldType, typeof(string), ""));
			MetaDataType.RegisterMetaDataType(new PositiveOrNeg1IntegerMetaDataType(CustomFieldPosition));
			MetaDataType.RegisterMetaDataType(new MetaDataType(Null, typeof(object), null));
			MetaDataType.RegisterMetaDataType(new NotificationsMetaDataType());
			MetaDataType.RegisterMetaDataType(new MetaDataType(Password, typeof(bool), false));
			MetaDataType.RegisterMetaDataType(new MetaDataType(DisableModifiable, typeof(bool), false));
		}
	}

	/// <summary>
	/// This enumeration is not to be serialized in a component otherwise you'll a cast exception due to CargoWise.ComponentModel
	/// will be loaded in 2 different assemblies (unless CargoWise.ComponentModel is put into the GAC).
	/// </summary>
	public enum KDateTimeFormat
	{
		Custom,
		Long,
		Short,
		Time,
	}
	#endregion
}
