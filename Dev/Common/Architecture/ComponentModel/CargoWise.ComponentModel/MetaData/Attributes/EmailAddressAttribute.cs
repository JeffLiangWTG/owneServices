using System;

namespace CargoWise.ComponentModel
{
	//I dont think this is the best spot for this. Open to suggestions...
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class EmailAddressAttribute : Attribute
	{
	}
}
