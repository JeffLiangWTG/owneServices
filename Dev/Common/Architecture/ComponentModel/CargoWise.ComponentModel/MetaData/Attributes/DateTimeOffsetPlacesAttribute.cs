using System;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Specifies the member or constant value for meta-data type DMetaDataTypes.DateTimeOffsetPlaces.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class DateTimeOffsetPlacesAttribute : SingleMetaDataAttribute
	{
		public DateTimeOffsetPlacesAttribute(int dateTimeOffsetPlaces)
			: base(MetaDataTypes.DateTimeOffsetPlaces)
		{
			this.dateTimeOffsetPlaces = dateTimeOffsetPlaces;
		}

		public DateTimeOffsetPlacesAttribute(string dateTimeOffsetPlacesMember)
			: base(MetaDataTypes.DateTimeOffsetPlaces)
		{
			DateTimeOffsetPlacesMember = dateTimeOffsetPlacesMember;
		}

		public override bool ProvidesMetaDataValue(string metaDataTypeId)
		{
			return metaDataTypeId == MetaDataTypes.DateTimeOffsetPlaces && DateTimeOffsetPlaces != -1;
		}

		public override object GetMetaDataValue(string metaDataTypeId)
		{
			return DateTimeOffsetPlaces;
		}

		public override bool ProvidesMetaDataMember(string metaDataTypeId)
		{
			return metaDataTypeId == MetaDataTypes.DateTimeOffsetPlaces && DateTimeOffsetPlacesMember != null;
		}

		public override string GetMetaDataMember(string metaDataTypeId)
		{
			return DateTimeOffsetPlacesMember;
		}

		public int DateTimeOffsetPlaces
		{
			get { return dateTimeOffsetPlaces; }
		}
		readonly int dateTimeOffsetPlaces = 7;

		public string DateTimeOffsetPlacesMember { get; private set; }
	}
}
