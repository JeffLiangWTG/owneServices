using System;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Specifies the member or constant value for meta-data type MetaDataTypes.DecimalPrecision.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class DecimalPrecisionAttribute : SingleMetaDataAttribute
	{
		public DecimalPrecisionAttribute(int decimalPrecision)
			: base(MetaDataTypes.DecimalPrecision)
		{
			this.decimalPrecision = decimalPrecision;
		}

		public DecimalPrecisionAttribute(string decimalPrecisionMember)
			: base(MetaDataTypes.DecimalPrecision)
		{
			DecimalPrecisionMember = decimalPrecisionMember;
		}

		public override bool ProvidesMetaDataValue(string metaDataTypeId)
		{
			return metaDataTypeId == MetaDataTypes.DecimalPrecision && DecimalPrecision != -1;
		}

		public override object GetMetaDataValue(string metaDataTypeId)
		{
			return DecimalPrecision;
		}

		public override bool ProvidesMetaDataMember(string metaDataTypeId)
		{
			return metaDataTypeId == MetaDataTypes.DecimalPrecision && DecimalPrecisionMember != null;
		}

		public override string GetMetaDataMember(string metaDataTypeId)
		{
			return DecimalPrecisionMember;
		}

		public int DecimalPrecision
		{
			get { return decimalPrecision; }
		}
		readonly int decimalPrecision = -1;

		public string DecimalPrecisionMember { get; private set; }
	}
}
