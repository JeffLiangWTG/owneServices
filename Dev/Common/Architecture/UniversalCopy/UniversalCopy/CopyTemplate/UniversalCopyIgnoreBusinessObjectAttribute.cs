using System;

namespace CargoWise.UniversalCopy
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class UniversalCopyIgnoreBusinessObjectAttribute : Attribute
	{
	}
}
