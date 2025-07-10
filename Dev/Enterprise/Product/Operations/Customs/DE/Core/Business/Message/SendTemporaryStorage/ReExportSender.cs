namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class ReExportSender : TemporaryStorageSender
	{
		public ReExportSender(CusTempStorageDec storageDec)
			: base(storageDec, TemporaryStorageMessageBuilderLoader.ReExport, new REXDISCusTempStorageDecProvider(storageDec))
		{
		}

		protected override string MessageSubType => Messaging.TemporaryStorageMessageSubTypeList.Codes.ReExport;
	}
}
