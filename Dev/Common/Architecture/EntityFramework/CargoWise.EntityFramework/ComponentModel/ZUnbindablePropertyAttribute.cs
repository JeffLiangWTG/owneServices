using System;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class ZUnbindablePropertyAttribute : Attribute
	{
	}
}
