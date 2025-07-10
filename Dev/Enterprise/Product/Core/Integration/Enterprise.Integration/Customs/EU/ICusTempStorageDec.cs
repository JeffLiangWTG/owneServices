using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public interface ICusTempStorageDec : IBusiness
			{
				ZGuid PK { get; }
				object this[string propertyName] { get; set; }
				ZString STH_AdditionalInformation { get; set; }
				ZString STH_DeclarationSubType { get; set; }
				ZString STH_DeclarationType { get; set; }
				ZString STH_IdentificationIndicator { get; set; }
				ZString STH_MessageStatus { get; set; }
				ZGuid STH_SJH { get; set; }
				ZDateTime STH_SystemCreateTimeUtc { get; set; }
				ZString STH_SystemCreateUser { get; set; }
				ZDateTime STH_SystemLastEditTimeUtc { get; set; }
				ZString STH_SystemLastEditUser { get; set; }
			}
		}
	}
}
