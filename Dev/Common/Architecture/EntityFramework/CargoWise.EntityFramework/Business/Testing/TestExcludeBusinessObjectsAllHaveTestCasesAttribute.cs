using System;

namespace CargoWise.EntityFramework.Testing
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class TestExcludeBusinessObjectsAllHaveTestCasesAttribute : Attribute
	{
	}
}
