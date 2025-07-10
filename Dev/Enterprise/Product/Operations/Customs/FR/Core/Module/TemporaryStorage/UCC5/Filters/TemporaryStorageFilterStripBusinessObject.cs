using System.Collections.Generic;
using Enterprise.Customs.EU.TemporaryStorage.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Module;

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
		new CustomerFilterInflator(this),
		new CustomerReferenceFilterInflator(this),
		new CustomsOfficeFilterInflator(this, CusTempStorageJobHeaderSchema.SJH_CustomsOffice, FilterCategories.Other),
		new DdtNumberFilterInflator(this),
		new JobNumberFilterInflator(this, CusTempStorageJobHeaderSchema.SJH_JobReference),
		new PresenterFilterInflator(this, CusTempStorageJobHeaderSchema.SJH_OA_Presenter),
		new TransportRegNoFilterInflator(this)
	];
}
