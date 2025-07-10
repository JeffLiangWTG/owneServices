namespace WTG.TestHelpers.Xml.Test
{
	public static class XmlConstants
	{
		public const string TestSchema = "http://schemas.cw1.com/test";
		public const string Test2Schema = "http://schemas.cw1.com/test2";

		public const string InvalidXml = @"<?xml version=""1.0"" encoding=""utf-8""?><A><B></B>";
		public const string ValidSimpleXml = @"<?xml version=""1.0"" encoding=""utf-8""?><A>TEST</A>";
		public const string ValidComplexXml = @"<?xml version=""1.0"" encoding=""utf-8""?><A Z=""z""><B X=""1"">TEST</B><B></B><CS><C Y=""1""></C><C Y=""2""></C><C Y=""3""></C></CS></A>";
		public const string ValidComplexXmlWithNamespaces = @"<?xml version=""1.0"" encoding=""utf-8""?><A xmlns=""" + TestSchema + @""" Z=""z"" xmlns:ex=""" + Test2Schema + @"""><ex:B X=""1"">TEST</ex:B><ex:B></ex:B><CS><C Y=""1""></C><C Y=""2""></C><C Y=""3""></C></CS></A>";
		public const string ValidXmlWithDefaultNamespace = @"<?xml version=""1.0"" encoding=""utf-8""?><A xmlns=""" + TestSchema + @"""></A>";
		public const string ValidXmlWithNonDefaultNamespace = @"<?xml version=""1.0"" encoding=""utf-8""?><ex:A xmlns:ex=""" + Test2Schema + @"""></ex:A>";
	}
}
