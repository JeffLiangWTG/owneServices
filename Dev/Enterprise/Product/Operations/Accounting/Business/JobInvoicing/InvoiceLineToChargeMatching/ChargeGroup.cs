using System.Linq;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing.InvoiceLineToChargeMatching
{
	public class ChargeGroup
	{
		public ChargeGroup(IGrouping<GroupingKey, RecordedCharges> group)
		{
			Key = group.Key;
			charges = group.ToArray();
			TotalCostAmount = group.Sum(x => x.TotalCostAmount);
			ConsolCostAmount = group.First().ConsolCostAmount;
			IsConsolRelated = group.Key.ConsolCostPK != ZGuid.Empty;
		}

		public GroupingKey Key { get; }
		public RecordedCharges[] GetCharges()
		{
			return charges;
		}
		public ZDecimal TotalCostAmount { get; }
		public ZDecimal ConsolCostAmount { get; }
		public ZBool IsConsolRelated { get; }
		readonly RecordedCharges[] charges;
	}
}
