using System;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Apply this attribute to an interface if it is bound directly to a ZGrid where grid colour is enabled.
	/// Without applying this attribute to the interface, grid colour will be disabled if the filter uses any property from the interface that is implemented explicitly (eg. ZString IFoo.Name)
	/// Once this attribute is applied, the business object class must implement all its IZtype properties publicly (eg. public ZString Name).
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface)]
	public sealed class SupportGridColourAttribute : Attribute
	{
	}
}
