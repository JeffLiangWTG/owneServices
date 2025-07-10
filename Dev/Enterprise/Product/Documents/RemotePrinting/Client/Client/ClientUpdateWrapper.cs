using Enterprise.RemotePrinting.Client.RemotePrintServer;

namespace Enterprise.RemotePrinting.Client
{
	#region IClientUpdate

	public interface IClientUpdate
	{
		string Version { get; }
		string Link { get; }
	}

	public class ClientUpdateWrapper : IClientUpdate
	{
		public ClientUpdateWrapper(ClientUpdate instance)
		{
			this.Instance = instance;
		}

		public readonly ClientUpdate Instance;

		#region IClientUpdate Members

		public string Version
		{
			get { return Instance.Version; }
		}

		public string Link
		{
			get { return Instance.Link; }
		}

		#endregion
	}

	#endregion
}
