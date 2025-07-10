using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class LandedCosting
	{
		public interface ILandedCostHistory
		{
			ZGuid PK { get; }
			object this[string propertyName] { get; set; }

			ZDecimal LH_DutyPercent { get; set; }
			ZString LH_LandedCostHistoryLineType { get; set; }
			ZDecimal LH_LandedCostMarginPercent1 { get; set; }
			ZDecimal LH_LandedCostMarginPercent2 { get; set; }
			ZDecimal LH_LandedCostMarginPercent3 { get; set; }
			ZGuid LH_LT { get; set; }
			ZGuid LH_OP { get; set; }
			ZGuid LH_ParentID { get; set; }
			ZString LH_ParentTableCode { get; set; }
			ZString LH_RN_NKCountryOfEntry { get; set; }

			ZDecimal GetRoundedLineValue(string code);
		}
	}
}