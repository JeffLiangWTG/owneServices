using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.ExitControl.GUI
{
	public static class CommonPromptMessages
	{
		public static ResourceString SelectARowMessage => ResString.GetMultilingualString("70CAA8E2-A688-4599-A387-BAA4EC5788E1", "Please select a row first");

		public static ResourceString NoReportErrorMessage => ResString.GetMultilingualString("4DD1A793-AB85-4EB2-84CF-735F7062DE4E", "No reports exist – Please add exit reports before attempting to send a message.");

		public static ResourceString CredentialsErrorMessage => ResString.GetMultilingualString("C025302B-F842-47D1-8416-A13C2D8E10C3", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.");
	}
}
