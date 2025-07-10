using System;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class IsNAddInfoFieldAttribute : Attribute
	{
	}
}
