namespace Enterprise.Customs.DE.NCTS.Business
{
	sealed class DESREMMessageHeaderProvider : NCTSMessageHeaderProvider<DESREMHeaderProvider>
	{
		public DESREMMessageHeaderProvider(NctsHeader header) : base(header)
		{
		}
	}
}
