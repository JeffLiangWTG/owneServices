using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public class UCC6TemporaryStorageFilterStripBusinessObject : InflatableFilterStripBusinessObject
{
	public UCC6TemporaryStorageFilterLookups Lookups => GetNewLookups();

	protected virtual UCC6TemporaryStorageFilterLookups GetNewLookups() => new(this);

	protected virtual UCC6TemporaryStorageFilterInflatorFactory GetFilterInflatorFactory() => new();

	protected override List<IFilterInflator> GetFilterInflators() => GetFilterInflators(GetFilterInflatorFactory());

	List<IFilterInflator> GetFilterInflators(UCC6TemporaryStorageFilterInflatorFactory f) =>
	[
		f.CreateApplicationCodeFilterInflator(this),
		f.CreateBillNumberFilterInflator(this),
		f.CreateBranchFilterInflator(this),
		f.CreateConsigneeFilterInflator(this),
		f.CreateConsignorFilterInflator(this),
		f.CreateCountryFilterInflator(this),
		f.CreateCustomsOfficeFilterInflator(this),
		f.CreateCustomsStatusFilterInflator(this),
		f.CreateDeclarantFilterInflator(this),
		f.CreateJobNumberFilterInflator(this),
		f.CreateLocalReferenceNumberFilterInflator(this),
		f.CreateMessageTypeFilterInflator(this),
		f.CreateMovementReferenceNumberFilterInflator(this),
		f.CreateNotifyFilterInflator(this),
		f.CreatePackingMarksFilterInflator(this),
		f.CreatePackingTypeFilterInflator(this),
		f.CreatePresentationCustomsOfficeFilterInflator(this),
		f.CreatePresentationDateFilterInflator(this),
		f.CreatePresenterFilterInflator(this),
		f.CreatePreviousDocumentNumberFilterInflator(this),
		f.CreatePreviousDocumentTypeFilterInflator(this),
		f.CreateRepresentativeFilterInflator(this),
		f.CreateSupportingDocumentNumberFilterInflator(this),
		f.CreateSupportingDocumentTypeFilterInflator(this),
		f.CreateTariffFilterInflator(this),
		f.CreateTransportationModeFilterInflator(this),
		f.CreateTransportIdFilterInflator(this),
		f.CreateTransportTypeFilterInflator(this)
	];
}
