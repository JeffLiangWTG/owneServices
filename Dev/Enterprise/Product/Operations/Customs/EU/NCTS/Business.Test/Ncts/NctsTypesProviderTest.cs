using System;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(INctsTypesProvider))]
	public abstract class NctsTypesProviderAbstractTest<T> : TestCase
		where T : INctsTypesProvider, new()
	{
		public void TestNctsAdditionalInfoType() => AssertEquals(ExpectedNctsAdditionalInfoType, new T().NctsAdditionalInfoType);
		public void TestNctsArrivalCargoDescType() => AssertEquals(ExpectedNctsArrivalCargoDescType, new T().NctsArrivalCargoDescType);
		public void TestNctsDepartureCargoDescType() => AssertEquals(ExpectedNctsDepartureCargoDescType, new T().NctsDepartureCargoDescType);
		protected abstract Type ExpectedNctsAdditionalInfoType { get; }
		protected abstract Type ExpectedNctsArrivalCargoDescType { get; }
		protected abstract Type ExpectedNctsDepartureCargoDescType { get;  }
	}

	[TestedType(typeof(NctsTypesProvider))]
	sealed class NctsTypesProviderTest : NctsTypesProviderAbstractTest<NctsTypesProvider>
	{
		protected override Type ExpectedNctsAdditionalInfoType => typeof(NctsAdditionalInfo);
		protected override Type ExpectedNctsArrivalCargoDescType => typeof(NctsArrivalCargoDesc);
		protected override Type ExpectedNctsDepartureCargoDescType => typeof(NctsDepartureCargoDesc);
	}
}
