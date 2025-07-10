using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing.InvoiceLineToChargeMatching
{
	public class GroupingKey
	{
		public GroupingKey(ZString chargeCode, ZString jobNumber, ZString consolNumber, ZGuid consolCostPK, ZString currency, ZString orgCode, OrgType orgType)
		{
			ChargeCode = chargeCode;
			JobNumber = jobNumber;
			ConsolNumber = consolNumber;
			ConsolCostPK = consolCostPK;
			Currency = currency;

			OrgCode = orgCode;
			OrgType = orgType;
		}

		public GroupingKey(ZString jobNumber, ZString consolNumber, ZGuid consolCostPK, ZString currency, ZString orgCode, OrgType orgType)
			: this(ZString.Empty, jobNumber, consolNumber, consolCostPK, currency, orgCode, orgType)
		{
		}

		public GroupingKey(ZString chargeCode, ZString jobNumber, ZString consolNumber, ZString currency)
			: this(chargeCode, jobNumber, consolNumber, ZGuid.Empty, currency, ZString.Empty, OrgType.OriginalOrg)
		{
		}

		public GroupingKey(ZString jobNumber, ZString consolNumber, ZString currency)
			: this(ZString.Empty, jobNumber, consolNumber, currency)
		{
		}

		public ZString ChargeCode { get; }
		public ZString JobNumber { get; }
		public ZString ConsolNumber { get; }
		public ZGuid ConsolCostPK { get; }
		public ZString Currency { get; }
		public ZString OrgCode { get; }
		public OrgType OrgType { get; }

		public override bool Equals(object obj)
		{
			var key = obj as GroupingKey;
			return key != null && key.ChargeCode == ChargeCode && key.JobNumber == JobNumber && key.ConsolNumber == ConsolNumber && key.ConsolCostPK == ConsolCostPK && key.Currency == Currency && key.OrgCode == OrgCode && key.OrgType == OrgType;
		}

		public override int GetHashCode() => ChargeCode.GetHashCode() ^ JobNumber.GetHashCode() ^ ConsolNumber.GetHashCode() ^ ConsolCostPK.GetHashCode() ^ Currency.GetHashCode() ^ OrgCode.GetHashCode() ^ OrgType.GetHashCode();
	}
}
