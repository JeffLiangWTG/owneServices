using System;

namespace Enterprise.Server.Setup
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	sealed class CommandLineArgumentAttribute : Attribute
	{
		public CommandLineArgumentAttribute()
			: this(false)
		{
		}

		public CommandLineArgumentAttribute(bool hasUseDefaultProperty)
		{
			HasUseDefaultProperty = hasUseDefaultProperty;
		}

		public bool HasUseDefaultProperty { get; set; }
	}
}
