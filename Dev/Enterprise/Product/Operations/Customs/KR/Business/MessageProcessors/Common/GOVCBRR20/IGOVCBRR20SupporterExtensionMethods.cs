using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public static class IGOVCBRR20SupporterExtensionMethods
	{
		public static EDIMessage GetLastMessageWithMatchingVersionNumber(this IGOVCBRR20Supporter support, BusinessObject header, ZString typeCode, ZString versionNumber)
		{
			return ((CusEntryHeader)header).Messages.GetLastMessageWithMatchingVersionNumber(typeCode, versionNumber);
		}
	}
}
