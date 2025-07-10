using System.Linq;
using System.Reflection;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public static class OrganisationDataItemIDProvider
	{
		public static ZString GetItemID(ZString messageType, RoleType role, ZString propertyName)
		{
			return typeof(IOrganization).GetProperty(propertyName)?.GetCustomAttributes<OrganisationDataItemIDAttribute>().FirstOrDefault(x => x.Role == role && x.MessageType == messageType)?.ItemID ?? ZString.Empty;
		}

		public static ZBool IsOrganisationDataItemID(ZString messageType, ZString code)
		{
			return typeof(IOrganization).GetProperties().Any(x => x.GetCustomAttributes<OrganisationDataItemIDAttribute>().Any(x => x.MessageType == messageType && x.ItemID == code));
		}
	}
}
