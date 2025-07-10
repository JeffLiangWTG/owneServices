using System;
using System.Collections.Generic;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class Declaration07ProviderTest : DataProviderTestCase<Declaration07Provider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("TemporaryStorageHeader missing", () => GenerateProvider(null));
				AssertNoExceptionThrown(() => GenerateProvider(header));
			});
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
			AssertNull(Provider.SpecificCircumstanceIndicator);
		}

		public void TestPreviousDocument()
		{
			Assert("Should be IReadOnlyCollection<IPreviousDocument08>", Provider.PreviousDocument is IReadOnlyCollection<DateTime>);
		}

		public void TestLRN()
		{
			header.LRN = "123";
			AssertEquals("LRN", "123", Provider.LRN);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<TemporaryStorageHeader>();
		}
		TemporaryStorageHeader header;

		Declaration07Provider GenerateProvider(TemporaryStorageHeader header) => new Declaration07Provider(header);

		protected sealed override Declaration07Provider GetProvider()
		{
			return GenerateProvider(header);
		}
	}
}
