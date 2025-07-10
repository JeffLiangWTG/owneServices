using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICusClassPartPivot
			{
				ZGuid PK { get; }
				ZString CI_ZZF_NKTaxType { get; set; }
				ZDecimal CI_ValuationMarkup { get; set; }
				ZString CI_ValuationCode { get; set; }
				ZString CI_UsageComment { get; set; }
				ZString CI_TariffNum { get; set; }
				ZBool CI_TariffChangePending { get; set; }
				ZString CI_SupplementalTariff { get; set; }
				ZString CI_SecondaryPreference { get; set; }
				ZString CI_RW_NKOriginState { get; set; }
				ZString CI_RN_NKCountryOfOrigin { get; set; }
				ZString CI_RN_NKCountryOfExport { get; set; }
				ZString CI_RN_NKCountry { get; set; }
				ZString CI_RelatedIndicator { get; set; }
				ZString CI_PrimaryPreference { get; set; }
				ZGuid CI_CI_Parent { get; set; }
				ZGuid CI_OP { get; set; }
				ZString CI_LastAuditedUser { get; set; }
				ZDateTime CI_LastAuditedDate { get; set; }
				ZDateTime CI_DateStart { get; set; }
				ZDateTime CI_DateEnd { get; set; }
				ZString CI_ConcessionOrder { get; set; }
				ZGuid CI_OH { get; set; }
				ZString CI_ChildType { get; set; }
				ZString CI_ChildQtyType { get; set; }
				ZDecimal CI_ChildQty { get; set; }
				ZByte CI_ChildListOrder { get; set; }
				ZGuid CI_CC { get; set; }
				ZString CI_AddInfo { get; set; }
				ZString CI_NAddInfo { get; set; }
			}
		}
	}
}
