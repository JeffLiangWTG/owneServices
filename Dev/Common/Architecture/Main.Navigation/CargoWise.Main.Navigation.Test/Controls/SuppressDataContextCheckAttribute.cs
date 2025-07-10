using System;

namespace CargoWise.Main.Navigation.WPF.Test
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class SuppressDataContextCheckAttribute : Attribute
	{
	}
}
