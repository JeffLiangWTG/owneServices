namespace Enterprise.Customs.DE.NCTS.Business
{
	sealed class DEPDATMessageHeaderProvider : NCTSMessageHeaderProvider<DEPDATHeaderProvider>
	{
		public DEPDATMessageHeaderProvider(NctsHeader header) : base(header)
		{
		}
	}
}
