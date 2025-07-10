using System.Collections;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	public abstract class DynamicMetaData
	{
		public static DynamicMetaData ReadOnly(bool readOnly) { return new ReadOnlyImpl(readOnly); }
		public static DynamicMetaData DecimalPlaces(int decimalPlaces) { return new DecimalPlacesImpl(decimalPlaces); }
		public static DynamicMetaData MaxLength(int maxLength) { return new MaxLengthImpl(maxLength); }
		public static DynamicMetaData Name(string name) { return new NameImpl(name); }
		public static DynamicMetaData Description(IDescription description) { return new DescriptionImpl(description); }
		public static DynamicMetaData DateTimeFormat(KDateTimeFormat format) { return new DateTimeFormatImpl(format); }
		public static DynamicMetaData DateTimeCustomFormat(string customFormat) { return new DateTimeCustomFormatImpl(customFormat); }
		public static DynamicMetaData ListDataSource(IList list) { return new ListDataSourceImpl(list); }
		public static DynamicMetaData Position(int? position) { return new PositionImpl(position); }
		public static DynamicMetaData ParentCustomFieldType(string type) { return new ParentCustomFieldTypeImpl(type); }
		public static DynamicMetaData CustomFieldPosition(int? position) { return new CustomFieldPositionImpl(position); }
		public static DynamicMetaData Visible(bool visible) { return new VisibleImpl(visible); }
		public static DynamicMetaData DisableModifiable(bool disableModifiable) { return new DisableModifiableImpl(disableModifiable); }

		protected DynamicMetaData(string id, object value)
		{
			Id = id;
			Value = value;
		}

		public string Id { get; private set; }
		public object Value { get; private set; }

		class ReadOnlyImpl : DynamicMetaData
		{
			public ReadOnlyImpl(bool readOnly) : base(MetaDataTypes.ReadOnly, readOnly) { }
		}

		class DecimalPlacesImpl : DynamicMetaData
		{
			public DecimalPlacesImpl(int decimalPlaces) : base(MetaDataTypes.DecimalPlaces, decimalPlaces) { }
		}

		class MaxLengthImpl : DynamicMetaData
		{
			public MaxLengthImpl(int maxLength) : base(MetaDataTypes.MaxLength, maxLength) { }
		}

		class NameImpl : DynamicMetaData
		{
			public NameImpl(string name) : base(MetaDataTypes.Name, name) { }
		}

		class DescriptionImpl : DynamicMetaData
		{
			public DescriptionImpl(IDescription description) : base(MetaDataTypes.Description, description) { }
		}

		class DateTimeFormatImpl : DynamicMetaData
		{
			public DateTimeFormatImpl(KDateTimeFormat format) : base(MetaDataTypes.DateTimeFormat, format) { }
		}

		class DateTimeCustomFormatImpl : DynamicMetaData
		{
			public DateTimeCustomFormatImpl(string customFormat) : base(MetaDataTypes.DateTimeCustomFormat, customFormat) { }
		}

		class ListDataSourceImpl : DynamicMetaData
		{
			public ListDataSourceImpl(IList list) : base(MetaDataTypes.ListDataSource, list) { }
		}

		class PositionImpl : DynamicMetaData
		{
			public PositionImpl(int? position) : base(MetaDataTypes.Position, position) { }
		}

		class ParentCustomFieldTypeImpl : DynamicMetaData
		{
			public ParentCustomFieldTypeImpl(string type) : base(MetaDataTypes.ParentCustomFieldType, type) { }
		}

		class CustomFieldPositionImpl : DynamicMetaData
		{
			public CustomFieldPositionImpl(int? position) : base(MetaDataTypes.CustomFieldPosition, position) { }
		}

		class VisibleImpl : DynamicMetaData
		{
			public VisibleImpl(bool visible) : base(MetaDataTypes.Visible, visible) { }
		}

		class DisableModifiableImpl : DynamicMetaData
		{
			public DisableModifiableImpl(bool disableModifiable) : base(MetaDataTypes.DisableModifiable, disableModifiable) { }
		}
	}
}
