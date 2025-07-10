using System.Collections.Generic;
using Enterprise.Customs.EU.TemporaryStorage.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module;

public class SumAFilterStripBusinessObject : TemporaryStorageFilterStripBusinessObject
{
	public SumAFilterStripBusinessObjectLookups Lookups => lookups ??= new SumAFilterStripBusinessObjectLookups(this);
	SumAFilterStripBusinessObjectLookups lookups;

	protected override List<IFilterInflator> GetFilterInflators()
	{
		var filterInflators = base.GetFilterInflators();
		filterInflators.AddRange(FilterInflators);
		return filterInflators;
	}

	IFilterInflator[] FilterInflators =>
	[
		new ApplicationCodeFilterInflator(this),
		new ArrivalDateFilterInflator(this),
		new BranchFilterInflator(this),
		new CurrentDeclarationTypeFilterInflator(this),
		new CurrentMessageStatusFilterInflator(this),
		new CurrentRegistrationNumberFilterInflator(this),
		new CustomerFilterInflator(this),
		new CustomsOfficeFilterInflator(this),
		new JobNumberFilterInflator(this, CusTempStorageJobHeaderSchema.SJH_JobReference),
		new LoadingFilterInflator(this),
		new PresentationDateFilterInflator(this, CusTempStorageJobHeaderSchema.SJH_PresentationDate),
		new PreviousReferenceNumberFilterInflator(this),
		new ReferenceNumberFilterInflator(this),
		new TransportModeFilterInflator(this),
	];
}
