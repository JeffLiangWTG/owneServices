using System;

namespace CargoWise.Main.Navigation.WPF.Test
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class TestExcludeWPFControlFromBasherAttribute : Attribute
	{
		public TestExcludeWPFControlFromBasherAttribute()
		{
		}
	}
}
