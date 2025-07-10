using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	[TestedType(typeof(ED801PackageProvider))]
	class ED801PackageProviderTest : InboundDataProviderTestCase<IEMCSPackageInComing, ED801PackageProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED801PackageProvider(null));
		}

		public void TestKindOfPackages()
		{
			AssertEquals("KG", dataProvider.KindOfPackages);
		}

		public void TestNumberOfPackages()
		{
			AssertEquals(20, dataProvider.NumberOfPackages);
		}

		public void TestNumberOfPackages_Null()
		{
			package.NumberOfPackages = null;
			AssertEquals(0, dataProvider.NumberOfPackages);
		}

		public void TestSealNumber()
		{
			AssertEquals("SealNo 001", dataProvider.SealNumber);
		}

		public void TestSealInformation()
		{
			AssertEquals("Seal Info 1", dataProvider.SealInformation);
		}

		public void TestShippingMarks()
		{
			AssertEquals("Shipping Marks 1", dataProvider.ShippingMarks);
		}

		public void TestIsNumberOfPackagesProvided()
		{
			CombineAssertions(() =>
			{
				AssertEquals(true, dataProvider.IsNumberOfPackagesProvided);

				package.NumberOfPackages = null;
				AssertEquals(false, dataProvider.IsNumberOfPackagesProvided);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			package = new ED801DBodyEadContainerBodyEadPackage
			{
				KindOfPackages = "KG",
				NumberOfPackages = "20",
				CommercialSealIdentification = "SealNo 001",
				SealInformation = "Seal Info 1",
				ShippingMarks = "Shipping Marks 1",
			};
			dataProvider = new ED801PackageProvider(package);
		}
		IEMCSPackageInComing dataProvider;
		ED801DBodyEadContainerBodyEadPackage package;

		protected override ED801PackageProvider GetProvider() => (ED801PackageProvider)dataProvider;
	}
}
