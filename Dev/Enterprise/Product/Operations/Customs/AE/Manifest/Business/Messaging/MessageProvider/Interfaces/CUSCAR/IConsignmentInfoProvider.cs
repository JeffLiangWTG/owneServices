namespace Enterprise.Customs.AE.Manifest.Business;

public interface IConsignmentInfoProvider
{
	int TotalHouseBills { get; }

	IConsignmentDetailsProvider BillDetails { get; }
}
