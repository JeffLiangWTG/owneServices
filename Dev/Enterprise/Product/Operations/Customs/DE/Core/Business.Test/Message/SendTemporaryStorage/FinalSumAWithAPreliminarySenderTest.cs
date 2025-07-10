using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class FinalSumAWithAPreliminarySenderTest : TemporaryStorageSenderAbstractTest<FinalSumAWithAPreliminarySender>
	{
		protected override CusTempStorageDec GetTempStorageDecToTest()
		{
			var storageDec = CUSPRLCusTempStorageDec.LoadOrCreate(storageJobHeader);
			storageDec.CusEntryNumber.CE_EntryNum = LogbookRegNum;
			storageDec.CusTempStorageLines.AddNew();
			return storageDec;
		}

		protected override TemporaryStorageSender GetTempStorageSender() => new FinalSumAWithAPreliminarySender(TempStorageDec, new HashSet<ZGuid>(TempStorageDec.Lines.Cast<CusTempStorageLine>().Select(x => x.PK)));

		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SCPRLK);

		protected override ZString ExpectedMessageSubType => Messaging.TemporaryStorageMessageSubTypeList.Codes.PreliminarySummaryDeclaration;

		protected override ZString ExpectedLogbookRegNumForIndicatorREG => LogbookRegNum;

		const string LogbookRegNum = "ATB150002110520195879";
	}
}
