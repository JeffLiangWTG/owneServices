namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

public static class XMLTestFileConstants
{
	internal const string TestFilePath = "Enterprise.Customs.ES.Messaging.Test.MessageBuilders.XML.TestFiles";
	internal static string AESTestFilePath => $"{TestFilePath}.AES";
	internal static string CGMTestFilePath => $"{TestFilePath}.CGM";
	internal static string DVDTestFilePath => $"{TestFilePath}.DVD";
	internal static string ENSTestFilePath => $"{TestFilePath}.ENS";
	internal static string EXSTestFilePath => $"{TestFilePath}.EXS";
	internal static string G3TestFilePath => $"{TestFilePath}.G3";
	internal static string G5TestFilePath => $"{TestFilePath}.G5";
	internal static string H1TestFilePath => $"{TestFilePath}.H1";
	internal static string H7TestFilePath => $"{TestFilePath}.H7";
	internal static string ImportTestFilePath => $"{TestFilePath}.Import";
	internal static string InboxNotificationTestFilePath => $"{TestFilePath}.InboxNotification";
	internal static string T2LTestFilePath => $"{TestFilePath}.T2L";
	internal static string T2LPOUSTestFilePath => $"{TestFilePath}.T2LPOUS";
#if NETFRAMEWORK
	public const string XmlElementNamespace = "q1:";
	public const string XmlElementNamespaceSuffix = ":q1";
	public const string XmlnsLinkAttributes = @"xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""";

#else
	public const string XmlElementNamespace = "";
	public const string XmlElementNamespaceSuffix = "";
	public const string XmlnsLinkAttributes = @"xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""";
#endif
}
