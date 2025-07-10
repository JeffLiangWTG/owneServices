using CargoWise.EntityFramework.Testing;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class TemporaryStorageMessageSendingObjectLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypes()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;

			var sendingObject = new TemporaryStorageMessageSendingObject(header);
			var lookups = sendingObject.Lookups;

			header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			AssertEquals("115", lookups.MessageTypes.CodesAsString);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			AssertEquals("015", lookups.MessageTypes.CodesAsString);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			AssertEquals("007", lookups.MessageTypes.CodesAsString);

			header.CustomsStatus = PNTS.CustomsStatus.TemporaryStoragePreLodged;
			AssertEquals("007", lookups.MessageTypes.CodesAsString);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			AssertEquals("413, 414", lookups.MessageTypes.CodesAsString);

			header.CustomsStatus = PNTS.CustomsStatus.TemporaryStorageActivated;
			AssertEquals("413", lookups.MessageTypes.CodesAsString);
		}
	}
}
