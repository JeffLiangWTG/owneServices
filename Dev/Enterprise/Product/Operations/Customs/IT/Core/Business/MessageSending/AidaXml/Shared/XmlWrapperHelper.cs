using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml;

public static class XmlWrapperHelper
{
	public static ZString RemoveIsoCode(ZString customsOffice) => customsOffice.SubstringSafe(2);
}
