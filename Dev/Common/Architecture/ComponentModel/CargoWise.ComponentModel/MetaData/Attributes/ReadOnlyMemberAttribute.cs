using System;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Specifies the propertyName for meta-data type MetaDataTypes.ReadOnly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class ReadOnlyMemberAttribute : MetaDataMemberAttribute
	{
		public ReadOnlyMemberAttribute(string propertyName)
			: base(MetaDataTypes.ReadOnly, propertyName)
		{
			Argument.NotNull(propertyName, nameof(propertyName));
		}
	}
}
