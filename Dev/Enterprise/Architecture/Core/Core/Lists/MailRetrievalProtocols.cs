namespace Enterprise.ZArchitecture.Core.Lists
{
	public class MailRetrievalProtocols : CodeDescriptionPairList
	{
		public const string IMAP = "IMAP";
		public const string POP3 = "POP3";

		public MailRetrievalProtocols()
		{
			AddPair(IMAP, SourceGenerated.ResString.GetMultilingualString("280757DF-4256-470C-9E7D-86A776E28D40", "Internet Message Access Protocol"));
			AddPair(POP3, SourceGenerated.ResString.GetMultilingualString("38766FC7-FF56-49F1-8FA0-5C164DC0DE8E", "Post Office Protocol Version 3"));

			DefaultCode = POP3;
		}
	}
}
