using System;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE801PackageProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE801PackageProvider(null));
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
			package = new PackageType
			{
				KindOfPackages = "KG",
				NumberOfPackages = "20",
				CommercialSealIdentification = "SealNo 001",
				SealInformation = new LsdSealInformationType
				{
					Value = "Seal Info 1",
					Language = "en",
				},
				ShippingMarks = "Shipping Marks 1",
			};
			dataProvider = new IE801PackageProvider(package);
		}
		IEMCSPackageInComing dataProvider;
		PackageType package;
	}
}
