using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public partial class CusEntryHeaderCharges : AutoCusEntryHeaderCharges, Integration.Customs.IL.ICusEntryHeaderCharges
	{
		public CusEntryHeaderCharges(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ReadOnlyMember(nameof(C1_ChargeAmountReadOnly))]
		public override ZDecimal C1_ChargeAmount { get => base.C1_ChargeAmount; set => base.C1_ChargeAmount = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderChargesLookups.RateOverrideReasonCodeList))]
		public override ZString C1_RateOverrideReasonCode { get => base.C1_RateOverrideReasonCode; set => base.C1_RateOverrideReasonCode = value; }

		protected ZBool C1_ChargeAmountReadOnly => C1_RateOverrideReasonCode.IsEmpty;
	}
}
