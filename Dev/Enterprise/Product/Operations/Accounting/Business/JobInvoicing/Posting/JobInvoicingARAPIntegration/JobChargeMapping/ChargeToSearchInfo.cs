using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public sealed class ChargeToSearchInfo
	{
		public ChargeToSearchInfo(ZGuid chargePK, ZGuid jobHeaderPK, ZGuid chargeCodePK, ZGuid branchPK, ZGuid deparmentPK)
		{
			this.JR_PK = chargePK;
			this.JR_JH = jobHeaderPK;
			this.JR_AC = chargeCodePK;
			this.JR_GB = branchPK;
			this.JR_GE = deparmentPK;
		}

		public ZGuid JR_PK { get; private set; }

		public ZGuid JR_JH { get; private set; }

		public ZGuid JR_AC { get; private set; }

		public ZGuid JR_GB { get; private set; }

		public ZGuid JR_GE { get; private set; }
	}
}
