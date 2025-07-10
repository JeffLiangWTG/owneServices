namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders.Testing
{
	static class NCTSXMLTestFileConstants
	{
		internal const string TestFilePath = "Enterprise.Customs.ES.NCTS.Messaging.Test.MessageBuilders.XML.TestFiles";

		internal static string NCTSTestFilePath =>
#if NETFRAMEWORK
			$"{TestFilePath}.NCTS";
#else
			$"{TestFilePath}.NCTS.net8";
#endif

		internal const string XmlElementNamespace =
#if NETFRAMEWORK
			"q1:";
#else
			"";
#endif

	}
}
