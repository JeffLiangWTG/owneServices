namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public class ClientUpdate
	{
		string version;
		string link;

		public ClientUpdate() { }

		public string Version
		{
			get { return version; }
			set { version = value; }
		}

		public string Link
		{
			get { return link; }
			set { link = value; }
		}
	}
}
