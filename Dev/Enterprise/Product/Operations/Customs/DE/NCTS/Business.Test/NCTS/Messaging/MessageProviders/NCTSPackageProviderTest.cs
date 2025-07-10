using System;
using CargoWise.Customs.DE.MessageContracts.NCTS;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSPackageProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSPackageProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new NCTSPackageProvider(null));
		}

		public void TestKind()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", null, dataProvider.Kind);
				package.B5_UnitType = "BX";
				AssertEquals("Entered", "BX", dataProvider.Kind);
			});
		}

		public void TestQuantity()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", 1L, dataProvider.Quantity);
				package.B5_UnitCount = 4;
				AssertEquals("Entered", 4L, dataProvider.Quantity);
			});
		}

		public void TestMarksNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", null, dataProvider.MarksNumber);
				package.B5_MarksAndNumbers = "MarksNumber";
				AssertEquals("Entered", "MarksNumber", dataProvider.MarksNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var commonCargoDesc = header.Bills.AddNew().GoodsItems.AddNew();
			package = commonCargoDesc.Packages.AddNew();
			dataProvider = new NCTSPackageProvider(package);
		}
		NctsPackage package;
		INCTSPackage dataProvider;

		protected override NCTSPackageProvider GetProvider() => (NCTSPackageProvider)dataProvider;
	}
}
