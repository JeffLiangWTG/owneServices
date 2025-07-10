using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	class TS313MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<TS313MessageProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Should throw ArgumentException with null header.", () => new TS313MessageProvider(null));
		}

		public void TestDeclaration()
		{
			AssertType<TS313MessageProvider>("TS313MessageProvider.Declaration should return object of TS313MessageProvider", GetProvider().Declaration);
		}

		public void TestFallbackProcedure()
		{
			AssertType<FallbackProcedureProvider>("TS313MessageProvider.FallbackProcedure should return object of FallbackProcedure", Provider.FallbackProcedure);
		}

		public void TestLRN()
		{
			header.LRN = "123";
			AssertEquals("LRN", "123", Provider.LRN);
		}

		public void TestMRN()
		{
			header.MRN = "456";
			AssertEquals("MRN", "456", Provider.MRN);
		}

		public void TestMsgType()
		{
			header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			AssertEquals("MsgType G4G3", TemporaryStorageDeclarationTypeList.Codes.DeclarationAndPresentationNotification, Provider.MsgType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			AssertEquals("MsgType G4", TemporaryStorageDeclarationTypeList.Codes.Declaration, Provider.MsgType);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			AssertNull("LRN returns null.", Provider.SpecificCircumstanceIndicator);
		}

		public void TestPreviousDocument()
		{
			Assert("Should be IReadOnlyCollection<DateTime>", Provider.PreviousDocument is IReadOnlyCollection<DateTime>);
			AssertEquals("PreviousDocument count", 1, Provider.PreviousDocument.Count);
		}

		protected override TS313MessageProvider GetProvider() => new TS313MessageProvider(new TemporaryStorageMessageSendingObject(header));

		TemporaryStorageHeader header;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		}
	}
}
