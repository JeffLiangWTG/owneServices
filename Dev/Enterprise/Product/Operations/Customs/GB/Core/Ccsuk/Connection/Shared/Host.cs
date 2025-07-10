namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class Host
	{
		public string HostNameFormatted
		{
			get
			{
				char space = ' ';
				return GB.Registry.GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.Value.PadRight(10, space).ToUpper();
			}
		}
	}
}
