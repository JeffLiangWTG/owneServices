
namespace Enterprise.Customs.HK.Business.Testing
{
	public static class XmlAttrHelper
	{
		public static string GetXmlns()
		{
#if NET
			return @"xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""";
#else
			return @"xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""";
#endif
		}
	}
}
