using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	class TemporaryStorageMessageSendingObjectLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypes()
		{
			header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			AssertContainsExactElementsInAnyOrder("When AMA_MessageType is TC", new string[]
				{
					IETemporaryStorageMessageTypeList.Codes.Amendment,
					IETemporaryStorageMessageTypeList.Codes.Invalidation,
					IETemporaryStorageMessageTypeList.Codes.Declaration,
				}, lookups.MessageTypes.GetAllCodes());

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			AssertContainsExactElementsInAnyOrder("When AMA_MessageType is TS", new string[]
				{
					IETemporaryStorageMessageTypeList.Codes.Amendment,
					IETemporaryStorageMessageTypeList.Codes.Invalidation,
					IETemporaryStorageMessageTypeList.Codes.Declaration,
				}, lookups.MessageTypes.GetAllCodes());

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			AssertContainsExactElementsInAnyOrder("When AMA_MessageType is PN", new string[]
				{
					IETemporaryStorageMessageTypeList.Codes.Amendment,
					IETemporaryStorageMessageTypeList.Codes.Invalidation,
					IETemporaryStorageMessageTypeList.Codes.PresentationNotification,
				}, lookups.MessageTypes.GetAllCodes());

			header.AMA_MessageType = "ABC";
			AssertContainsExactElementsInAnyOrder("Default values for AMA_MessageType", new string[]
				{
					IETemporaryStorageMessageTypeList.Codes.Amendment,
					IETemporaryStorageMessageTypeList.Codes.Invalidation,
					IETemporaryStorageMessageTypeList.Codes.Declaration,
					IETemporaryStorageMessageTypeList.Codes.PresentationNotification,
					IETemporaryStorageMessageTypeList.Codes.GoodsStatusReportDeclaration,
				}, lookups.MessageTypes.GetAllCodes());
		}

		public void TestMessageTypesIsCached()
		{
			var anotherHeader = Factory.New<TemporaryStorageHeader>();
			var anotherLookups = new TemporaryStorageMessageSendingObject(anotherHeader).Lookups;

			header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			anotherHeader.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			AssertSame(anotherLookups.MessageTypes, lookups.MessageTypes);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			anotherHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			AssertSame(anotherLookups.MessageTypes, lookups.MessageTypes);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			anotherHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			AssertSame(anotherLookups.MessageTypes, lookups.MessageTypes);

			header.AMA_MessageType = "";
			anotherHeader.AMA_MessageType = "";
			AssertSame(anotherLookups.MessageTypes, lookups.MessageTypes);
		}

		public void TestDeclarationTypes()
		{
			AssertEquals("G4G3, G4, G3", lookups.DeclarationTypes.CodesAsString);
		}

		protected override void SetUp()
		{
			header = Factory.New<TemporaryStorageHeader>();
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			lookups = sendingObject.Lookups;
		}
		TemporaryStorageHeader header;
		TemporaryStorageMessageSendingObject sendingObject;
		TemporaryStorageMessageSendingObjectLookups lookups;
	}
}
