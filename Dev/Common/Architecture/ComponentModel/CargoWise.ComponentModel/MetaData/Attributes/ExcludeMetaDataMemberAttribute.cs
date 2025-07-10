using System;

namespace CargoWise.ComponentModel
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class ExcludeMetaDataMemberAttribute : Attribute
	{
		public string MetaDataTypeId { get; set; }
	}
}
