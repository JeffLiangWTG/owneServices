using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class FtpUriRegistryDataType : UriRegistryDataType
	{
		public FtpUriRegistryDataType()
			: base(Uri.UriSchemeFtp)
		{
		}

		public FtpUriRegistryDataType(string uriScheme, int minLength, int maxLength)
			: base(Uri.UriSchemeFtp, minLength, maxLength)
		{
		}
	}
}
