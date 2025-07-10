using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class DEPDATPackageProviderTest : Customs.Business.Testing.DataProviderTestCase<IDEPDATPackage>
	{
		public void TestKind()
		{
			AssertEquals("BX", Provider.Kind);
		}

		public void TestQuantity_Main()
		{
			AssertEquals(3L, Provider.Quantity);
		}

		public void TestQuantity_Bulk()
		{
			package.B5_UnitType = bulkCode;
			AssertNull(Provider.Quantity);
		}

		public void TestQuantity_Unpack()
		{
			package.B5_UnitType = unpackCode;
			AssertNotNull(Provider.Quantity);
		}

		public void TestQuantity_ByPack()
		{
			var provider = GetProviderByPack("BX");
			AssertEquals(0L, provider.Quantity);
		}

		public void TestMarksNumber()
		{
			AssertEquals("MarksAndNumbers", Provider.MarksNumber);
		}

		public void TestGoodsItemNumber_Main()
		{
			AssertNull(Provider.GoodsItemNumber);
		}

		public void TestGoodsItemNumber_ByPack_Bulk()
		{
			var provider = GetProviderByPack(bulkCode);
			AssertNull(provider.GoodsItemNumber);
		}

		public void TestGoodsItemNumber_ByPack_Unpacked()
		{
			var provider = GetProviderByPack(unpackCode);
			AssertNull(provider.GoodsItemNumber);
		}

		public void TestGoodsItemNumber_ByPack()
		{
			var provider = GetProviderByPack("BX");
			AssertEquals(2, provider.GoodsItemNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			bulkCode = Factory.SetupBulkCusCode();
			unpackCode = Factory.SetupUnpackCusCode();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			bill = header.Bills.AddNew();
			cargoDesc = bill.GoodsItems.AddNew();
			cargoDesc.BY_LineNo = 1;
			package = cargoDesc.Packages.AddNew();
			package.B5_UnitType = "BX";
			package.B5_UnitCount = 3;
			package.B5_MarksAndNumbers = "MarksAndNumbers";
		}

		protected override IDEPDATPackage GetProvider() => DEPDATPackageProvider.NewOrNull(package);

		IDEPDATPackage GetProviderByPack(string packType)
		{
			cargoDesc.BY_IsMainPack = true;
			var additionalCargoDesc = bill.GoodsItems.AddNew();
			additionalCargoDesc.BY_LineNo = 2;
			additionalCargoDesc.BY_IsMainPack = true;
			var additionalPackage = additionalCargoDesc.Packages.AddNew();
			additionalPackage.B5_UnitType = "BX";
			additionalPackage.B5_UnitCount = 5;
			additionalPackage.B5_MarksAndNumbers = "MarksAndNumbers2";

			var cargoDescByPack = bill.GoodsItems.AddNew();
			cargoDescByPack.BY_LineNo = 3;
			packageByPack = cargoDescByPack.Packages.AddNew();
			packageByPack.B5_UnitType = packType;
			packageByPack.B5_UnitCount = 0;
			packageByPack.B5_MarksAndNumbers = "MarksAndNumbers2";
			return DEPDATPackageProvider.NewOrNull(packageByPack);
		}

		NctsBill bill;
		NctsDepartureCargoDesc cargoDesc;
		NctsPackage package;
		NctsPackage packageByPack;
		ZString bulkCode;
		ZString unpackCode;
	}
}
