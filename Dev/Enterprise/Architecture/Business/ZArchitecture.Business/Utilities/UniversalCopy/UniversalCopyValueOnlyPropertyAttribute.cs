using System;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class UniversalCopyValueOnlyPropertyAttribute : Attribute
	{
	}
}
