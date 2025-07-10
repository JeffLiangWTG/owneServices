using CargoWise.Types;

namespace Enterprise.Customs.AE.Manifest.Business;

public sealed class MessagingProvider : ASYCUDA.Business.MessagingProvider
{
	public override ASYCUDA.Business.MessageStatusProvider MessageStatusProvider => new MessageStatusProvider();

	protected override ZString GetMostSevereValueMessageStatusCore(ASYCUDA.Business.AsycudaManifestHeader manifestHeader)
	{
		return manifestHeader.CombineBillStatuses(bill => bill.ABL_MessageStatus, ZString.Empty);
	}

	protected override ZString GetMostSevereValueCustomsStatusCore(ASYCUDA.Business.AsycudaManifestHeader manifestHeader)
	{
		return manifestHeader.CombineBillStatuses(bill => bill.ABL_BillStatus, ZString.Empty);
	}
}
