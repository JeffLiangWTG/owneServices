namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class ConsolidationSender : TemporaryStorageSender
	{
		public ConsolidationSender(CusTempStorageDec storageDec)
			: base(storageDec, TemporaryStorageMessageBuilderLoader.Consolidation, new PRLCONCusTempStorageDecProvider(storageDec))
		{
		}

		protected override string MessageSubType => Messaging.TemporaryStorageMessageSubTypeList.Codes.Consolidation;
	}
}
