using System;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Specifies the member or constant value for meta-data type DMetaDataTypes.DecimalPlaces.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class DecimalPlacesAttribute : SingleMetaDataAttribute
	{
		public DecimalPlacesAttribute(int decimalPlaces)
			: base(MetaDataTypes.DecimalPlaces)
		{
			this.decimalPlaces = decimalPlaces;
		}

		public DecimalPlacesAttribute(string decimalPlacesMember)
			: base(MetaDataTypes.DecimalPlaces)
		{
			DecimalPlacesMember = decimalPlacesMember;
		}

		public override bool ProvidesMetaDataValue(string metaDataTypeId)
		{
			return metaDataTypeId == MetaDataTypes.DecimalPlaces && DecimalPlaces != -1;
		}

		public override object GetMetaDataValue(string metaDataTypeId)
		{
			return DecimalPlaces;
		}

		public override bool ProvidesMetaDataMember(string metaDataTypeId)
		{
			return metaDataTypeId == MetaDataTypes.DecimalPlaces && DecimalPlacesMember != null;
		}

		public override string GetMetaDataMember(string metaDataTypeId)
		{
			return DecimalPlacesMember;
		}

		public int DecimalPlaces
		{
			get { return decimalPlaces; }
		}
		readonly int decimalPlaces = -1;

		public string DecimalPlacesMember { get; private set; }
	}
}
