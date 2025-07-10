using System;

namespace CargoWise.EntityFramework.Testing
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class ExcludeDeferrableTriggerTestCaseAttribute : Attribute
	{
	}
}
