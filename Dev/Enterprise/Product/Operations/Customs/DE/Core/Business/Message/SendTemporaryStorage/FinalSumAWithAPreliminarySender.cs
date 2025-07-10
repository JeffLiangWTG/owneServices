using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class FinalSumAWithAPreliminarySender : TemporaryStorageSender
	{
		public FinalSumAWithAPreliminarySender(CusTempStorageDec storageDec, HashSet<ZGuid> linesToBeSentPKs)
			: base(storageDec, TemporaryStorageMessageBuilderLoader.FinalSumAWithAPreliminary, new CUSPRLCusTempStorageDecProvider(storageDec, (line) => linesToBeSentPKs.Contains(line.PK)))
		{
		}

		protected override string RegistrationNumber => IdentificationIdicatorIsREG ? ((ICUSPRLTempStorageDec)dataProvider).ATBNumber : string.Empty;

		protected override string MessageSubType => TemporaryStorageMessageSubTypeList.Codes.PreliminarySummaryDeclaration;
	}
}
