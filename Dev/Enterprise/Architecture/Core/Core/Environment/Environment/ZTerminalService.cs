using CargoWise.Application;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public class ZTerminalService : TerminalService
	{
		public override bool IsRemoteAppSession
		{
			get
			{
#if DEBUG
				if (Globals.IsTest)
				{
					return false;
				}
#endif
				return base.IsRemoteAppSession;
			}
		}

		public override bool IsCitrixICA
		{
			get
			{
#if DEBUG
				if (Globals.IsTest)
				{
					return false;
				}
#endif
				return base.IsCitrixICA;
			}
		}

		public override bool IsWTSSession
		{
			get
			{
#if DEBUG
				if (Globals.IsTest)
				{
					return false;
				}
#endif
				return base.IsWTSSession;
			}
		}

		public static MultilingualString ClientPluginApplicationNotInstalledWarning
		{
			get { return ObjectFactory.Get<TerminalService>().IsCitrixICA ? CitrixApplicationNotInstalledWarning : RDApplicationNotInstalledWarning; }
		}

		public static MultilingualString ClientPluginApplicationNotInstalledError
		{
			get { return ObjectFactory.Get<TerminalService>().IsCitrixICA ? CitrixApplicationNotInstalledError : RDApplicationNotInstalledError; }
		}

		public static MultilingualString RDApplicationNotInstalledWarning
		{
			get
			{
				return SourceGenerated.ResString.GetMultilingualString("2454325A-CA20-4040-B21B-A59BF55A2D59", @"Some functions will not be available because {0} is not installed or was not found.
Please download and run the {0} installer to use this function. If you have already installed this, please restart current application.

You can download the latest version of the {0} installer via the following link:
https://myaccount-portal.cargowise.com/myaccount/downloads/{1}", ClientVersion.ProductName, ClientVersion.InstallerExeName);
			}
		}

		public static MultilingualString RDApplicationNotInstalledError
		{
			get
			{
				return SourceGenerated.ResString.GetMultilingualString("2741BED5-932D-49FA-92B1-D52B12F21FCF", @"This function is not available because {0} is not installed or was not found.
Please download and run the {0} installer to use this function. If you have already installed this, please restart current application.

You can download the latest version of the {0} installer via the following link:
https://myaccount-portal.cargowise.com/myaccount/downloads/{1}", ClientVersion.ProductName, ClientVersion.InstallerExeName);
			}
		}

		public static MultilingualString CitrixApplicationNotInstalledWarning
		{
			get
			{
				return SourceGenerated.ResString.GetMultilingualString("8A5A3D45-815C-4152-B8D8-F9F79269026F", @"Some functions will not be available because {0} is not installed or was not found.
Please download and run the {0} installer to use this function. If you have already installed this, please restart current application.

You can download the latest version of the {0} installer via the following link:
https://myaccount-portal.cargowise.com/myaccount/downloads/{1}", ClientCitrixVersion.ProductName, ClientCitrixVersion.InstallerExeName);
			}
		}

		public static MultilingualString CitrixApplicationNotInstalledError
		{
			get
			{
				return SourceGenerated.ResString.GetMultilingualString("0874C686-11BF-4033-BAA1-9DEC79737326", @"This function is not available because {0} is not installed or was not found.
Please download and run the {0} installer to use this function. If you have already installed this, please restart current application.

You can download the latest version of the {0} installer via the following link:
https://myaccount-portal.cargowise.com/myaccount/downloads/{1}", ClientCitrixVersion.ProductName, ClientCitrixVersion.InstallerExeName);
			}
		}
	}
}
