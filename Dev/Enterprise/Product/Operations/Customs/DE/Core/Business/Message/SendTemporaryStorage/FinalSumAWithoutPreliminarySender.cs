namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class FinalSumAWithoutPreliminarySender : TemporaryStorageSender
	{
		public FinalSumAWithoutPreliminarySender(CusTempStorageDec storageDec)
			: base(storageDec, TemporaryStorageMessageBuilderLoader.FinalSumAWithoutPreliminary, new CUSPRLCusTempStorageDecProvider(storageDec))
		{
		}

		protected override string MessageSubType => Messaging.TemporaryStorageMessageSubTypeList.Codes.SummaryDeclarationAfterPresentation;
	}
}
