using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface IDutyAndTax
			{
				ZBool C1_Override { get; set; }
				ZString C1_TaxType { get; set; }
				ZString C1_ExemptCode { get; set; }
				ZDecimal C1_Amount { get; set; }
				ZGuid B7_ParentID { get; set; }
				ZString B7_ParentTableCode { get; set; }
			}
		}
	}
}
