using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC043CGoodsMeasureProvider))]
	sealed class CC043CGoodsMeasureProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("goodsMeasure missing", () => new CC043CGoodsMeasureProvider(null));
			});
		}

		public void TestGrossMass()
		{
			AssertEquals("Gross mass", 120.65m, provider.GrossMass);
		}
		public void TestNetMassValue()
		{
			AssertEquals("Nett mass", 120m, provider.NetMassValue);
		}

		protected override void SetUp()
		{
			provider = new CC043CGoodsMeasureProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.GoodsMeasureType03()
			{
				GrossMass = 120.65m,
				NetMass = 120m
			});
		}
		CC043CGoodsMeasureProvider provider;
	}
}
