using System.Linq;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class PlaceOfUnloadingWrapperTest : Customs.Business.Testing.DataProviderTestCase<PlaceOfUnloadingWrapper>
	{
		public void TestUnLoCode()
		{
			AssertEquals("UnLoCode should equal ABL_RL_NKPortOfDischarge from the related ABL of type 'BOL'.", "FRBER", Provider.UnLoCode);
		}

		protected override PlaceOfUnloadingWrapper GetProvider()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			var bill = temporaryStorageHeader.Bills.FirstOrDefault() ?? temporaryStorageHeader.Bills.AddNew();
			bill.ABL_BolType = "BOL";
			bill.ABL_RL_NKPortOfDischarge = "FRBER";

			var qq = temporaryStorageHeader.Bills.Cast<TemporaryStorageBill>().FirstOrDefault(b => b.ABL_BolType == "BOL")?.ABL_RL_NKPortOfDischarge;

			return PlaceOfUnloadingWrapper.New(temporaryStorageHeader);
		}
	}
}
