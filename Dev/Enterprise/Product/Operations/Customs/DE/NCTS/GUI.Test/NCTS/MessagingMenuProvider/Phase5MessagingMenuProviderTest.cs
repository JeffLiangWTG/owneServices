using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	sealed class Phase5MessagingMenuProviderTest : TestCaseWithFactory
	{
		public void TestGetNctsHeaderGenerator()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var testProvider = new Phase5MessagingMenuProviderForTest(nctsHeader);
			AssertType<NctsHeaderGenerator>(testProvider.GetNctsHeaderGeneratorForTest());
		}

		public void TestGetInventorySelectionHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var bill = Factory.New<NctsBill>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var testProvider = new Phase5MessagingMenuProviderForTest(nctsHeader);
			AssertType<NctsInventorySelectionHeader>(testProvider.GetInventorySelectionHeaderForTest(bill));
		}
	}

	sealed class Phase5MessagingMenuProviderForTest : Phase5MessagingMenuProvider
	{
		public Phase5MessagingMenuProviderForTest(NctsHeader header)
			: base(header)
		{
		}

		public EU.NCTS.Business.NctsHeaderGenerator GetNctsHeaderGeneratorForTest() => GetNctsHeaderGenerator();

		public EU.NCTS.Business.NctsInventorySelectionHeader GetInventorySelectionHeaderForTest(NctsBill bill) => GetInventorySelectionHeader(bill);
	}
}
