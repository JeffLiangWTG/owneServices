namespace Enterprise.Customs.DE.NCTS.Business
{
	sealed class DESNOTMessageHeaderProvider : NCTSMessageHeaderProvider<DESNOTHeaderProvider>
	{
		public DESNOTMessageHeaderProvider(NctsHeader header) : base(header)
		{
		}
	}
}
