
namespace Enterprise.DataTransfer.Native.Integration
{
	/// <summary>
	/// Native Data Service via EHub
	/// Please refer Enterprise.DataTransfer.Native.WCFClient.NativeDataServiceClient
	/// </summary>
	public interface INativeDataService
	{
		IResponseMessage Update(IRequestMessage request);
		INativeServiceClient Client { get; set; }
	}
}
