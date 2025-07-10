using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	class AISPackagingProviderTest : DataProviderTestCase<AISPackagingProvider>
	{
		public void TestPackageType()
		{
			SetUpTestDataIfNeeded();
			AssertEquals("PackageType=>CW_PackType", "T1", Provider.PackageType);
		}

		public void TestPackageQuantity()
		{
			SetUpTestDataIfNeeded();
			AssertEquals("PackageQuantity=>CW_PackQty for non-BULKs", 10, Provider.PackageQuantity);
		}

		public void TestShippingMarks()
		{
			SetUpTestDataIfNeeded();
			AssertEquals("ShippingMarks=>CW_MarksAndNos", "M1", Provider.ShippingMarks);
		}

		protected override AISPackagingProvider GetProvider()
		{
			SetUpTestDataIfNeeded();
			return new AISPackagingProvider(buckPack.CW_PackType, buckPack.CW_PackQty, buckPack.CW_MarksAndNos);
		}

		void SetUpTestDataIfNeeded()
		{
			if (buckPack == null)
			{
				declaration = Factory.New<JobDeclaration>();

				buckPack = (Package)declaration.Packages.AddNew();
				buckPack.CW_PackType = "T1";
				buckPack.CW_PackQty = 10;
				buckPack.CW_MarksAndNos = "M1";
			}
		}

		JobDeclaration declaration;
		Package buckPack;
	}
}
