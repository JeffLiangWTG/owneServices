using CargoWise.Types;

namespace Enterprise.DocumentEngine.Public.Testing
{
	public class DocumentPrinterForTest : DocumentPrinter
	{
		public DocumentPrinterForTest(bool showNotification = true)
			: base(showNotification)
		{
		}

		public bool ShowNotificationForTesting
		{
			get { return ShowNotification; }
		}

		protected override void PrintCore(DocumentCommand documentCommand, ZGuid printer, int numberOfCopies)
		{
			HasRun = true;
		}

		public bool HasRun;
	}
}
