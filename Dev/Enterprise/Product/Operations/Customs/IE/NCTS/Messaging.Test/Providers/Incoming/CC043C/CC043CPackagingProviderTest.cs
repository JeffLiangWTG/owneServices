using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC043CPackagingProvider))]
	sealed class CC043CPackagingProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("packagingType missing", () => new CC043CPackagingProvider(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals("Sequence Number", "1", provider.SequenceNumber);
		}

		public void TestTypeOfPackages()
		{
			AssertEquals("Package type", "BOX", provider.TypeOfPackages);
		}

		public void TestNumberOfPackages()
		{
			AssertEquals("Number of packages", "4", provider.NumberOfPackages);
		}
		public void TestShippingMarks()
		{
			AssertEquals("Shipping marks", "MARKS", provider.ShippingMarks);
		}

		protected override void SetUp()
		{
			provider = new CC043CPackagingProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.PackagingType02()
			{
				SequenceNumber = "1",
				TypeOfPackages = "BOX",
				NumberOfPackages = "4",
				ShippingMarks = "MARKS"
			});
		}
		CC043CPackagingProvider provider;
	}
}
