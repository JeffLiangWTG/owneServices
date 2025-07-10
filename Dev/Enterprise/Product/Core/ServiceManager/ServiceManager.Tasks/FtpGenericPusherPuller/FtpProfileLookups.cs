using Enterprise.ZArchitecture.Core;

namespace Enterprise.ServiceManager.Tasks.FTP
{
	public class FtpProfileLookups
	{
		public CodeDescriptionPairList UniqueOptions
		{
			get { return new FtpClobberingOptions(); }
		}

		public CodeDescriptionPairList PushOrPull
		{
			get { return new FtpDirection(); }
		}

		public CodeDescriptionPairList SuccessActions
		{
			get { return new FtpArchiveOptions(); }
		}
	}
}
