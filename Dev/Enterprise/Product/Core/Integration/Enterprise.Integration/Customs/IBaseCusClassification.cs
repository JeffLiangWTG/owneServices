using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IBaseCusClassification
		{
			ZGuid PK { get; }
			object this[string propertyName] { get; set; }
			ZString CC_AddInfo { get; set; }
			ZString CC_ClassificationType { get; set; }
			ZString CC_Description { get; set; }
			ZBool CC_IsActive { get; set; }
			ZBool CC_IsUnpublished { get; set; }
			ZDateTime CC_LastAuditedDate { get; set; }
			ZString CC_LastAuditedUser { get; set; }
			ZString CC_LookupCode { get; set; }
			ZString CC_RN_NKCountryCode { get; set; }
			ZBool CC_TariffChangePending { get; set; }
			ZString CC_TariffNum { get; set; }
		}
	}
}
