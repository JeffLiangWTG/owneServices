using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public interface IUSCTariff
			{
				ZGuid PK { get; }
				object this[string propertyName] { get; set; }
				ZBool UE_AdditionalTariffNumberIndicator { get; set; }
				ZBool UE_AntiDumping { get; set; }
				ZDecimal UE_Column1RateAdValorem { get; set; }
				ZDecimal UE_Column1RateOther { get; set; }
				ZDecimal UE_Column1RateSpecific { get; set; }
				ZDecimal UE_Column2RateAdValorem { get; set; }
				ZDecimal UE_Column2RateOther { get; set; }
				ZDecimal UE_Column2RateSpecific { get; set; }
				ZBool UE_CountervailingDutyFlag { get; set; }
				ZDateTime UE_DateFrom { get; set; }
				ZDateTime UE_DateTo { get; set; }
				ZString UE_DutyComputationCode { get; set; }
				ZString UE_GSPExcludedCountries { get; set; }
				ZBool UE_IsBaseRate { get; set; }
				ZString UE_ISOCountryofOriginEditCode { get; set; }
				ZByte UE_NumberOfReportingUnits { get; set; }
				ZString UE_OGACodes { get; set; }
				ZString UE_PermitLicenseIndicator { get; set; }
				ZString UE_PGACodes { get; set; }
				ZBool UE_QuotaIndicator { get; set; }
				ZString UE_ShortDescription { get; set; }
				ZString UE_SPICode { get; set; }
				ZString UE_Tariff { get; set; }
				ZString UE_TextileCategoryNumber { get; set; }
				ZString UE_Unit1 { get; set; }
				ZString UE_Unit2 { get; set; }
				ZString UE_Unit3 { get; set; }
			}
		}
	}
}
