namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class SplitSender : TemporaryStorageSender
	{
		public SplitSender(CusTempStorageDec storageDec)
			: base(storageDec, TemporaryStorageMessageBuilderLoader.Split, new CUSPCSCusTempStorageDecProvider(storageDec))
		{
		}

		protected override string MessageSubType => Messaging.TemporaryStorageMessageSubTypeList.Codes.Split;
	}
}
