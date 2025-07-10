using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class LandedCosting
	{
		public interface ILandCostInput
		{
			ZGuid PK { get; }
			object this[string propertyName] { get; set; }

			ZGuid LI_AC_ChargeCode { get; set; }
			ZString LI_ChargeDescription { get; set; }
			ZDecimal LI_CostAmount { get; set; }
			ZString LI_DistributeCostBy { get; set; }
			ZBool LI_IsParentGroupInvoice { get; set; }
			ZBool LI_IsUserEntered { get; set; }
			ZByte LI_LandedCostGroup { get; set; }
			ZGuid LI_LT { get; set; }
			ZGuid LI_ParentID { get; set; }
			ZString LI_ParentTableCode { get; set; }
			ZString LI_RX_NKCostCurrency { get; set; }
			ZDecimal LI_ServiceExRate { get; set; }
		}
	}
}