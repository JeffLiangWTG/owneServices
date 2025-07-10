using System.Linq;
using CargoWise.Common;

namespace Enterprise.Customs.AE.Manifest.Business;

sealed class ConsignmentInfoProvider : IConsignmentInfoProvider
{
	public ConsignmentInfoProvider(AsycudaBill bill)
	{
		Bill = Argument.NotNull(bill, nameof(bill));
	}
	AsycudaBill Bill { get; }

	public int TotalHouseBills => totalHouseBills ??= GetHouseBillsTotal();
	int? totalHouseBills;

	public IConsignmentDetailsProvider BillDetails => billDetails ??= new ConsignmentDetailsProvider(Bill);
	IConsignmentDetailsProvider billDetails;

	int GetHouseBillsTotal() => Bill.Header.Bills.Cast<AsycudaBill>().Count(x => x.ABL_BolType == Bill.ABL_BolType);
}
