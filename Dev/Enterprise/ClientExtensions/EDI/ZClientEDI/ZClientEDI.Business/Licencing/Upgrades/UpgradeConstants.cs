
namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public abstract class UpgradeConstants
	{
		public static string WebServerClientSpecificPath
		{
			get { return EDIDataRegistry.Instance.WebServerClientSpecificPath; }
		}

		public static string WebServerGenericPath
		{
			get { return EDIDataRegistry.Instance.WebServerGenericPath; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public static string HttpClientSpecificBaseUrl
		{
			get { return EDIDataRegistry.Instance.HttpClientSpecificBaseUrl; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public static string HttpGenericBaseUrl
		{
			get { return EDIDataRegistry.Instance.HttpGenericBaseUrl; }
		}

		public static string HttpDownloadUserName
		{
			get { return EDIDataRegistry.Instance.HttpDownloadUserName; }
		}

		public static string HttpDownloadPassword
		{
			get { return EDIDataRegistry.Instance.HttpDownloadPassword; }
		}
	}
}

