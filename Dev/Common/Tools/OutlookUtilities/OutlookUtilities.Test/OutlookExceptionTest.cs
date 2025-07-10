using System.Runtime.InteropServices;
using NUnit.Framework;

namespace Enterprise.Interop.OutlookIntegration.Testing
{
	sealed class OutlookExceptionTest : TestCase
	{
		[ExpectExceptionMessage(typeof(OutlookException), "Sending an email probably\r\n\r\nPlease check that Outlook is installed, configured and working properly on this machine.")]
		public void TestHandleCOMException()
		{
			OutlookException.HandleCOMException(new COMException("server failed, unspecified error, insert some other unhelpful COMException message here"), "Sending an email probably");
		}

		[ExpectException(typeof(OutlookDialogBoxOpenException))]
		public void TestHandleCOMException_WhenDialogBoxOpen()
		{
			OutlookException.HandleCOMException(new COMException("a dialog box is open", CargoWise.Common.Interop.HResult.E_FAIL), "");
		}

		[ExpectException(typeof(OutlookOperationAbortedException))]
		public void TestHandleCOMException_WhenUserAborted()
		{
			OutlookException.HandleCOMException(new COMException("changed my mind go away", CargoWise.Common.Interop.HResult.E_ABORT), "");
		}
	}
}
