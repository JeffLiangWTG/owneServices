using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface ICACusRulingConfig
			{
				ZGuid ZZY_JI_InvoiceLine { get; set; }
				ZGuid ZZY_ZZX_CusRuling { get; set; }
				ZString ZZY_Value { get; set; }
				ZString ZZY_Category { get; set; }
				ZString ZZY_Type { get; set; }
			}
		}
	}
}
