using System;
using System.IO;
using System.Runtime.InteropServices;
using Outlook;

#if DEBUG
using Enterprise.ZArchitecture.Environment;
#endif

namespace Enterprise.Interop.OutlookIntegration
{
	public class OutlookApplication
	{
		public virtual OutlookMailItem CreateMailItem(object customData)
		{
			OutlookMailItem result = null;
			try
			{
				var outlookApp = GetComOutlookApplication();
				var mailItem = (_DMailItem)outlookApp.CreateItem(OlItems.olMailItem);
				result = new OutlookMailItem(mailItem, customData);
			}
			catch (COMException ex)
			{
				OutlookException.HandleCOMException(ex, "");
			}
			catch (InvalidCastException)
			{
				throw new OutlookException(ErrorMessage);
			}
			catch (FileNotFoundException ex)
			{
				throw new OutlookException(ErrorMessage + " (" + ex.Message + ")", ex);
			}
			catch (DirectoryNotFoundException ex)
			{
				throw new OutlookException(ex.Message, ex);
			}
			return result;
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Methods")]
		TimeSpan TimeoutForGettingApplication
		{
			get
			{
				return
#if DEBUG
 Globals.IsTest ? TimeSpan.FromMilliseconds(5000) :
#endif
 TimeSpan.FromMilliseconds(30000);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		const string ErrorMessage = "There was a problem loading Microsoft Outlook. It is likely that Outlook has not been installed properly. Try re-installing Outlook.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Application name")]
		protected virtual _DApplication GetComOutlookApplication()
		{
			if (fCachedOutlookApp == null)
			{
				fCachedOutlookApp = new Application();
				var nameSpace = fCachedOutlookApp.GetNamespace("MAPI");
				nameSpace.Logon("Outlook", "", false, false);
			}
			return fCachedOutlookApp;
		}

		Application fCachedOutlookApp;

		#endregion
	}
}
