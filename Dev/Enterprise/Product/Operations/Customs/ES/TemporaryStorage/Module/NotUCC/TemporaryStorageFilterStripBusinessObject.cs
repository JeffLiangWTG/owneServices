using System.Collections.Generic;
using Enterprise.Customs.EU.TemporaryStorage.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.TemporaryStorage.Module;

public class TemporaryStorageFilterStripBusinessObject : EU.TemporaryStorage.Module.TemporaryStorageFilterStripBusinessObject
{
	protected override List<IFilterInflator> GetFilterInflators()
	{
		var filterInflators = base.GetFilterInflators();
		filterInflators.AddRange(FilterInflators);
		return filterInflators;
	}

	IFilterInflator[] FilterInflators =>
	[
		new JobNumberFilterInflator(this, CusTempStorageJobHeaderSchema.SJH_JobReference)
	];
}
