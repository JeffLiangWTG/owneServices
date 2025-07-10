using System;

namespace CargoWise.EntityFramework.Testing
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class TestExcludeDetectStaticBusinessObjectsCollectionsAndFactoriesAttribute : Attribute
	{
	}
}
