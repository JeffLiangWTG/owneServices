using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class LandedCosting
	{
		public interface ILandedCostHeader
		{
			ZGuid PK { get; }
			object this[string propertyName] { get; set; }

			ZDate LT_DateOfEntry { get; set; }
			ZDateTime LT_DateOfProcessing { get; set; }
			ZDecimal LT_DefaultEstimatedDutyRate { get; set; }
			ZString LT_EstimatedLandedCostComment { get; set; }
			ZGuid LT_GC { get; set; }
			ZString LT_LandedCostType { get; set; }
			ZGuid LT_ParentID { get; set; }
			ZString LT_ParentTableCode { get; set; }
		}
	}
}