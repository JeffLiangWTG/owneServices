using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IRefSysConfig
			{
				ZGuid PK { get; }
				object this[string propertyName] { get; set; }
				ZBlob ZRC_BinaryValue { get; set; }
				ZBool ZRC_BitValue { get; set; }
				ZDecimal ZRC_DecimalValue { get; set; }
				ZDateTime ZRC_EndDate { get; set; }
				ZDateTime ZRC_StartDate { get; set; }
				ZString ZRC_StringValue { get; set; }
				ZString ZRC_ZRT_NKConfigCode { get; set; }
			}
		}
	}
}
