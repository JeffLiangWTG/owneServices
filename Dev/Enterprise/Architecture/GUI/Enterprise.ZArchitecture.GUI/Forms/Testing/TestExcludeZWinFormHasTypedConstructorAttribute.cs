using System;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class TestExcludeZWinFormHasTypedConstructorAttribute : Attribute
	{
		public TestExcludeZWinFormHasTypedConstructorAttribute()
		{
		}
	}
}
