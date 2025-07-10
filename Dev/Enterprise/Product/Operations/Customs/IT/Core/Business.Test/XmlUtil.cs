namespace Enterprise.Customs.IT.Business.Testing;
public static class XmlUtil
{
	public static string HandleNamespaces(string xmlContent)
	{
#if NETFRAMEWORK
		return xmlContent;
#else
		return xmlContent.Replace("<q1:", "<").Replace("</q1:", "</").Replace(":q1=", "=");
#endif
	}
}
