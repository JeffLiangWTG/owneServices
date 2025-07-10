using System;

namespace CargoWise.Common.Testing
{
	/// <summary>
	/// Suppress the target class from the 'class names are unique across assemblies' reflection test.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
	public sealed class SuppressClassNamesAreUniqueAcrossAssembliesMessageAttribute : Attribute
	{
	}
}
