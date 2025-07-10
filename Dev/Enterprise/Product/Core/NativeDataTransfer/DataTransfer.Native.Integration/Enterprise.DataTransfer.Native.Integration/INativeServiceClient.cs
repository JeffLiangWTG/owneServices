using System;
using System.ServiceModel;
using CargoWise.Common;

namespace Enterprise.DataTransfer.Native.Integration
{
	public interface INativeServiceClient : ICommunicationObject
	{
		IResponseMessage Update(IRequestMessage input);
	}

	/// <summary>
	/// Helper class that would only use it by NativeDataServiceClient
	/// </summary>
	public static class ClientExtension
	{
		public static void CloseOrAbort(this ICommunicationObject client)
		{
			try
			{
				client.Close();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				client.Abort();
			}
		}
	}
}
