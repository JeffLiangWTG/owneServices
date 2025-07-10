using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManSlotOrgValidation : Customs.Business.CusSeaManSlotOrgValidation
	{
		public CusSeaManSlotOrgValidation(CusSeaManSlotOrg parent)
			: base(parent)
		{
			this.slotOrg = parent;
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateABNOrCCID();
		}

		#endregion

		#region ValidateABNOrCCID

		public void ValidateABNOrCCID()
		{
			ValidateCalculatedProperty(slotOrg.ABNOrCCIDInfo);
		}

		protected virtual void CheckABNOrCCID()
		{
			MandatoryValidation.MessageErrorIfNotEntered(slotOrg.ABNOrCCIDInfo, "ABN Or CCID");
		}

		#endregion

		#region CheckBS_OH_SlotCharterer

		protected override void CheckBS_OH_SlotCharterer()
		{
			base.CheckBS_OH_SlotCharterer();
			ValidateABNOrCCID();
		}

		#endregion

		#region Implementation

		readonly CusSeaManSlotOrg slotOrg;

		#endregion
	}
}
