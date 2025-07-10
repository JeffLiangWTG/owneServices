
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class CalloutChargeCollection : DependentBusinessObjectCollection<CalloutCharge, CalloutJobHeader>
	{
		public CalloutChargeCollection(CalloutJobHeader calloutJobHeader)
			: base(calloutJobHeader)
		{
		}

		public CalloutCharge FindByChargeDescription(ZString description)
		{
			CalloutCharge[] result = (CalloutCharge[])Find(new ZQuery(JobChargeSchema.JR_Desc, description));
			return result.Length >= 1 ? result[0] : null;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			PopulateFieldsRequiredForSaving(child as CalloutCharge);
		}

		public CalloutCharge AddNewFromPWSChargeDetails(PWSChargeDetails pWSCharge)
		{
			CalloutCharge result = AddNew();
			result.JR_Desc = pWSCharge.ChargeDescription;
			result.TaxableAmount = pWSCharge.TaxableAmount;
			result.NonTaxableAmount = pWSCharge.NonTaxableAmount;
			result.Discount = pWSCharge.Discount;
			result.NettAmount = pWSCharge.NettAmount;
			return result;
		}

		#region Implementation

		void PopulateFieldsRequiredForSaving(CalloutCharge charge)
		{
			charge.JR_JH = CalloutJobHeader.PK;
			charge.JR_GB = CalloutJobHeader.JH_GB;
			charge.JR_GC = CalloutJobHeader.JH_GC;
			charge.JR_GE = CalloutJobHeader.JH_GE;
			charge.JR_AC = Env.Registry.FreightChargeCode;
		}

		CalloutJobHeader CalloutJobHeader
		{
			get { return Master; }
		}

		#endregion
	}
}
