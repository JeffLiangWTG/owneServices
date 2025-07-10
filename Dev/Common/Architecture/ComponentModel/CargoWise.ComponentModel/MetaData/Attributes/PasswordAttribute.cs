using System;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Specifies the member or constant value for meta-data type MetaDataTypes.Password.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class PasswordAttribute : SingleMetaDataAttribute
	{
		public PasswordAttribute(string valueMember)
			: base(MetaDataTypes.Password)
		{
			this.valueMember = valueMember;
		}

		public PasswordAttribute(bool value)
			: base(MetaDataTypes.Password)
		{
			this.value = value;
		}

		public PasswordAttribute()
			: base(MetaDataTypes.Password)
		{
			this.value = true;
		}

		public override bool ProvidesMetaDataValue(string metaDataTypeId)
		{
			return metaDataTypeId == MetaDataTypes.Password && value;
		}

		public override object GetMetaDataValue(string metaDataTypeId)
		{
			return Value;
		}

		public override bool ProvidesMetaDataMember(string metaDataTypeId)
		{
			return metaDataTypeId == MetaDataTypes.Password && ValueMember != null;
		}

		public override string GetMetaDataMember(string metaDataTypeId)
		{
			return ValueMember;
		}

		public bool Value
		{
			get { return value; }
		}
		readonly bool value;

		public string ValueMember
		{
			get { return valueMember; }
		}
		readonly string valueMember;
	}
}
