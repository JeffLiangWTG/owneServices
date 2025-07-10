using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class LocalReferenceNumberFilterInflator : EU.TemporaryStorage.Module.LocalReferenceNumberFilterInflator
{
	public LocalReferenceNumberFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	protected override ModuleFilterSubGroup GetFilterSubGroup() => new BillCusEntryNumModuleFilterSubGroup(CusEntryNumberTypes.Standard.LocalReferenceNumber);
}
