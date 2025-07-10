using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class MovementReferenceNumberFilterInflator : EU.TemporaryStorage.Module.MovementReferenceNumberFilterInflator
{
	public MovementReferenceNumberFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	protected override ModuleFilterSubGroup GetFilterSubGroup() => new BillCusEntryNumModuleFilterSubGroup(CusEntryNumberTypes.Standard.MovementReferenceNumber);
}
