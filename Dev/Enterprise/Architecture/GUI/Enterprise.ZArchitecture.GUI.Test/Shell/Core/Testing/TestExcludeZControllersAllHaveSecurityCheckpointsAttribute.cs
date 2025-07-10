using System;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class TestExcludeZControllersAllHaveSecurityCheckpointsAttribute : Attribute
	{
		public TestExcludeZControllersAllHaveSecurityCheckpointsAttribute()
		{
		}
	}
}
