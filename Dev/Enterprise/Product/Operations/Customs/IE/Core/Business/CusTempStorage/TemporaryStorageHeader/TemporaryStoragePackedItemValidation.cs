using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IE.Business.CusTempStorage;

public class TemporaryStoragePackedItemValidation : AsycudaPackedItemValidation
{
	public TemporaryStoragePackedItemValidation(AutoAsycudaPackedItem parent) : base(parent)
	{
	}
	public new TemporaryStoragePackedItem Parent => (TemporaryStoragePackedItem)base.Parent;

	protected override void CheckAPI_Tariff()
	{
		base.CheckAPI_Tariff();
		var parent = Parent;
		if (parent.API_Tariff.IsEmpty)
		{
			if (parent.Bill is TemporaryStorageBill bill
				&& bill.Header is TemporaryStorageHeader header
				&& !((header.AMA_MessageType == PNTSMessageTypeList.Codes.CombinedTemporaryStorage || header.AMA_MessageType == PNTSMessageTypeList.Codes.PreLodgedTempStorage)
				&& header.IsUCC5))
			{
				var info = parent.API_TariffInfo;
				info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(info.Description));
			}
		}
	}
}
