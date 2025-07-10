using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class LandedCosting
	{
		public interface ILandedLineCostItem
		{
			ZGuid PK { get; }
			object this[string propertyName] { get; set; }

			ZDecimal LZ_CostAmount { get; set; }
			ZString LZ_CostType { get; set; }
			ZGuid LZ_LH { get; set; }
		}
	}
}