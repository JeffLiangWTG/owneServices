using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	public sealed class AllowSpatialTypesAttribute : Attribute
	{
		public AllowSpatialTypesAttribute(params string[] allowedSpatialTypes)
		{
			AllowedSpatialTypes = new ReadOnlyCollection<string>(allowedSpatialTypes);
		}

		public ReadOnlyCollection<string> AllowedSpatialTypes { get; private set; }
	}
}
