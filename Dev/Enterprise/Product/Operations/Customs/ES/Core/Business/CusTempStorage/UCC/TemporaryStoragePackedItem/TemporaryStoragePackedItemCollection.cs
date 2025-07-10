using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class TemporaryStoragePackedItemCollection : EU.Business.CusTempStorage.TemporaryStoragePackedItemCollection<TemporaryStoragePackedItem, TemporaryStorageBill>, ISequenceNumberHeaderWithFlagToRecalculate
{
	public TemporaryStoragePackedItemCollection(TemporaryStorageBill master) : base(master)
	{
	}

	protected override HugeSequenceNumberGenerator SequenceNumberCalculatorCore => new TemporaryStoragePackedItemSequenceNumberGenerator(this);

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		((AsycudaPackedItem)child).API_GrossWeightUQ = Core.Constants.Weight.Kilograms;
	}

	bool ISequenceNumberHeaderWithFlagToRecalculate.ShouldRecalculateLineNos => !Master?.Header?.IsMessageTypeTSM ?? true;
}
