using System.Linq;
using System.Xml.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class AmendmentSumASenderTest : TemporaryStorageSenderAbstractTest<AmendmentSumASender>
	{
		public void TestCannotSend()
		{
			var line1 = TempStorageDec.CusTempStorageLines[0];
			line1.TSL_GoodsDescription = "LINE 1 GOODS";
			line1.TSL_IsModified = true;
			line1.TSL_CustomsStatus = CustomsStatusList.Codes.TST;
			var line2 = TempStorageDec.CusTempStorageLines.AddNew();
			line2.TSL_GoodsDescription = "LINE 3 GOODS";
			line2.TSL_IsModified = false;
			line2.TSL_CustomsStatus = CustomsStatusList.Codes.PAC;

			var (can, whyCannotSend) = GetTempStorageSender().CanSend;
			CombineAssertions(() =>
			{
				AssertEquals("CanSend?", false, can);
				AssertContains("WhyCannotSend", "All Lines are already finalized or there are no changes to be sent.", whyCannotSend);
			});
		}

		public void TestStorageLinesFiltered()
		{
			var line1 = TempStorageDec.CusTempStorageLines[0];
			line1.TSL_GoodsDescription = "LINE 1 GOODS";
			line1.TSL_IsModified = true;
			line1.TSL_CustomsStatus = CustomsStatusList.Codes.TST;
			var line2 = TempStorageDec.CusTempStorageLines.AddNew();
			line2.TSL_GoodsDescription = "LINE 2 GOODS";
			line2.TSL_IsModified = true;
			line2.TSL_CustomsStatus = CustomsStatusList.Codes.PAC;
			var line3 = TempStorageDec.CusTempStorageLines.AddNew();
			line3.TSL_GoodsDescription = "LINE 3 GOODS";
			line3.TSL_IsModified = false;
			line3.TSL_CustomsStatus = CustomsStatusList.Codes.PAC;

			TempStorageSender.Send();
			var message = TempStorageDec.Messages[0];
			var document = XDocument.Parse(message.EM_MessageText);
			var goodsItem = document.Descendants().Elements("GoodsItem").Single();
			AssertEquals("The line sent was line 2", "LINE 2 GOODS", goodsItem.Elements("GoodsDescription").Single().Value);
		}

		protected override CusTempStorageDec GetTempStorageDecToTest()
		{
			var storageDec = CUSPRLCusTempStorageDec.LoadOrCreate(storageJobHeader);
			storageDec.CusEntryNumber.CE_EntryNum = LogbookRegNum;
			var line = storageDec.CusTempStorageLines.AddNew();
			line.TSL_IsModified = true;
			line.TSL_CustomsStatus = CustomsStatusList.Codes.PRE;
			return storageDec;
		}

		protected override TemporaryStorageSender GetTempStorageSender() => new AmendmentSumASender(TempStorageDec);

		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SCPRLK);

		protected override ZString ExpectedMessageSubType => Messaging.TemporaryStorageMessageSubTypeList.Codes.PreliminarySummaryDeclaration;

		protected override ZString ExpectedLogbookRegNumForIndicatorREG => LogbookRegNum;

		const string LogbookRegNum = "ATB150002110520195875";
	}
}
