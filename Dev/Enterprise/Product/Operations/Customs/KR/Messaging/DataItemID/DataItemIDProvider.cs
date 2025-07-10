using System;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using static Enterprise.Customs.KR.Messaging.DataItemIDAttribute;

namespace Enterprise.Customs.KR.Messaging
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Will be used by later implementation.")]
	public static class DataItemIDProvider
	{
		public static ZString GetItemID(Type dataProviderType, ZString propertyName)
		{
			return GetInterfaceMembersPropertyInfo(dataProviderType, propertyName)?.GetCustomAttribute<DataItemIDAttribute>()?.ItemID ?? ZString.Empty;
		}
		public static ChangeType GetChangeType(Type dataProviderType, ZString propertyName)
		{
			return dataProviderType.GetProperty(propertyName)?.GetCustomAttribute<DataItemIDAttribute>()?.DutyTaxChangeType ?? ChangeType.Normal;
		}

		public static ZString GetCustomsFeeID(Type dataProviderType, ZString itemID)
		{
			var propertyinfo = dataProviderType.GetProperties().FirstOrDefault(x => x.GetCustomAttribute<DataItemIDAttribute>()?.ItemID == itemID);
			return propertyinfo?.GetCustomAttribute<DataItemIDAttribute>()?.CustomsFeeID ?? ZString.Empty;
		}

		static PropertyInfo GetInterfaceMembersPropertyInfo(Type type, string name)
		{
			return type.GetProperty(name) ?? type.GetInterfaces().Select(x => x.GetProperty(name)).FirstOrDefault();
		}
	}
}
