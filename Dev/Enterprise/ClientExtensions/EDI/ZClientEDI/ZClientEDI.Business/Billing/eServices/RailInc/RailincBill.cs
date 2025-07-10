using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing
{
	public class RailincBill : TransactionSystemBillEx
	{
		public RailincBill(ZString systemCode, BusinessObjectFactory factory)
			: base(systemCode, factory)
		{
		}

		public override ZString SystemDescription
		{
			get
			{
				var usage = SystemUsages.Cast<RailincUsage>().FirstOrDefault(x => x.PriceItem != null);
				return usage != null ? usage.PriceItem.L7_DescriptionLocalized.Trim() : base.SystemDescription;
			}
		}
	}
}

