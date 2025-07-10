using System;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Specifies the member or constant value for meta-data type MetaDataTypes.MaxLength.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class MaxLengthAttribute : SingleMetaDataAttribute
	{
		public MaxLengthAttribute(int maxLength)
			: base(MetaDataTypes.MaxLength)
		{
			this.maxLength = maxLength;
		}

		public MaxLengthAttribute(string maxLengthMember)
			: base(MetaDataTypes.MaxLength)
		{
			this.maxLengthMember = maxLengthMember;
		}

		public override bool ProvidesMetaDataValue(string metaDataTypeId)
		{
			return metaDataTypeId == MetaDataTypes.MaxLength && MaxLength != -1;
		}

		public override object GetMetaDataValue(string metaDataTypeId)
		{
			return MaxLength;
		}

		public override bool ProvidesMetaDataMember(string metaDataTypeId)
		{
			return metaDataTypeId == MetaDataTypes.MaxLength && MaxLengthMember != null;
		}

		public override string GetMetaDataMember(string metaDataTypeId)
		{
			return MaxLengthMember;
		}

		public int MaxLength
		{
			get { return maxLength; }
		}
		readonly int maxLength = -1;

		public string MaxLengthMember
		{
			get { return maxLengthMember; }
		}
		readonly string maxLengthMember;
	}
}
