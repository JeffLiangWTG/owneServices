using System;

namespace CargoWise.Common.Testing
{
	/// <summary>
	/// Suppress the target member from the NoPublicWeaklyTypedCollections reflection test.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Delegate | AttributeTargets.Constructor)]
	public sealed class SuppressWeaklyTypedCollectionMessageAttribute : Attribute
	{
	}
}
