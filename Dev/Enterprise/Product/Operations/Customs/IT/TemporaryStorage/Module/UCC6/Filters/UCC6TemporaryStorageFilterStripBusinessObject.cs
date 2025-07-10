using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

public class UCC6TemporaryStorageFilterStripBusinessObject : EU.TemporaryStorage.Module.UCC6TemporaryStorageFilterStripBusinessObject
{
	protected override EU.TemporaryStorage.Module.UCC6TemporaryStorageFilterLookups GetNewLookups() => new UCC6TemporaryStorageFilterLookups(this);
	protected override EU.TemporaryStorage.Module.UCC6TemporaryStorageFilterInflatorFactory GetFilterInflatorFactory() => new UCC6TemporaryStorageFilterInflatorFactory();

	protected override List<IFilterInflator> GetFilterInflators()
	{
		var filterInflators = base.GetFilterInflators();
		var filterInflatorFactory = GetFilterInflatorFactory() as UCC6TemporaryStorageFilterInflatorFactory;
		var newFilterInflators = GetFilterInflators(filterInflatorFactory);
		filterInflators.AddRange(newFilterInflators);
		return filterInflators;
	}

	List<IFilterInflator> GetFilterInflators(UCC6TemporaryStorageFilterInflatorFactory f) =>
	[
		f.CreateCustomsStatusDateFilterInflator(this),
		f.CreateLocationOfGoodsAuthorizationNumberFilterInflator(this),
		f.CreateLocationOfGoodsOrganizationFilterInflator(this),
		f.CreateLocationOfGoodsPlaceIdFilterInflator(this),
		f.CreateMessageStatusFilterInflator(this),
		f.CreateRegistrationNumberFilterInflator(this),
		f.CreateReleaseDateFilterInflator(this)
	];
}
