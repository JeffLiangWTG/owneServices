using System;

namespace CargoWise.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class IncludeOnlyNamedPropertiesForPropertyDescriptorReflectionAttribute : Attribute
	{
		public IncludeOnlyNamedPropertiesForPropertyDescriptorReflectionAttribute()
		{
			IncludedPropertyNames = Array.Empty<string>();
		}

		public string[] IncludedPropertyNames { get; set; }
	}
}
