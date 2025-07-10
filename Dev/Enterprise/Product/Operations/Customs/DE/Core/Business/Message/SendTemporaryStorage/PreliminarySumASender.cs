namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class PreliminarySumASender : TemporaryStorageSender
	{
		public PreliminarySumASender(CusTempStorageDec storageDec)
			: base(storageDec, TemporaryStorageMessageBuilderLoader.PreliminarySumA, new CUSPRLCusTempStorageDecProvider(storageDec))
		{
		}

		protected override string MessageSubType => Messaging.TemporaryStorageMessageSubTypeList.Codes.PreliminarySummaryDeclaration;
	}
}
