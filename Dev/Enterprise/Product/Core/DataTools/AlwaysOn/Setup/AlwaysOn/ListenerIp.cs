namespace Enterprise.AlwaysOn.Setup
{
	public class ListenerIp
	{
		public ListenerIp(string ipAddress, string subnetMask)
		{
			IpAddress = ipAddress;
			SubnetMask = subnetMask;
		}
		public string IpAddress { get; }
		public string SubnetMask { get; }
	}
}