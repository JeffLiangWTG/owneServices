using System;
using System.Runtime.InteropServices;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Interop.OutlookIntegration
{
	[Serializable]
	public class OutlookException : ApplicationException
	{
		public OutlookException(string message)
			: base(message)
		{
		}

		public OutlookException(string message, Exception nested)
			: base(message, nested)
		{
		}

#if NETFRAMEWORK
		protected OutlookException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public static void HandleCOMException(COMException ex, string description)
		{
			if (ex.ErrorCode == CargoWise.Common.Interop.HResult.E_FAIL)
			{
				if (ex.Message.ToLower().IndexOf("a dialog box is open") != -1) // There is no other way of knowing the real cause - no HResult even
				{
					throw new OutlookDialogBoxOpenException(ex.Message, ex);
				}
				else
				{
					string message = description + System.Environment.NewLine + System.Environment.NewLine + (NoResString)"Please check that Outlook is installed, configured and working properly on this machine.";
					throw new OutlookException(message, ex);
				}
			}

			if (ex.ErrorCode == CargoWise.Common.Interop.HResult.E_ABORT)
			{
				throw new OutlookOperationAbortedException(description + " (operation aborted)", ex);
			}
			else
			{
				throw new OutlookException(description + " (reason: " + ex.Message + ")", ex);
			}
		}
	}
}
