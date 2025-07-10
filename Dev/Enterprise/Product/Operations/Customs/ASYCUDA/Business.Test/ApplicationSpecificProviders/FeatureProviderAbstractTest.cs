using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestsSubclassesOf(typeof(FeatureProvider))]
	public abstract class FeatureProviderAbstractTest<T> : TestCaseWithFactory where T : FeatureProvider, new()
	{
	}
}
