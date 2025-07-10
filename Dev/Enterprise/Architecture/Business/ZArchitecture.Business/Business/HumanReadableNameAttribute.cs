using System;

namespace Enterprise.ZArchitecture.Business
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class HumanReadableNameAttribute : Attribute
	{
		public string PropertyInfoBusinessObjectName { get; set; }
	}
}
