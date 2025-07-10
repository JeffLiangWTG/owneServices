using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public class UCC6TemporaryStorageFilterInflatorFactory
{
	#region Factory Methods

	public IFilterInflator CreateApplicationCodeFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new ApplicationCodeFilterInflator(bizObj);
	public IFilterInflator CreateBillNumberFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new BillNumberFilterInflator(bizObj);
	public IFilterInflator CreateBranchFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new BranchFilterInflator(bizObj);
	public IFilterInflator CreateConsigneeFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new ConsigneeFilterInflator(bizObj);
	public IFilterInflator CreateConsignorFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new ConsignorFilterInflator(bizObj);
	public IFilterInflator CreateCountryFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new CountryFilterInflator(bizObj);
	public IFilterInflator CreateCustomsOfficeFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> CreateCustomsOfficeFilterInflatorCore(bizObj);
	public IFilterInflator CreateCustomsStatusFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new CustomsStatusFilterInflator(bizObj);
	public IFilterInflator CreateDeclarantFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> CreateDeclarantFilterInflatorCore(bizObj);
	public IFilterInflator CreateJobNumberFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new JobNumberFilterInflator(bizObj);
	public IFilterInflator CreateLocalReferenceNumberFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> CreateLocalReferenceNumberFilterInflatorCore(bizObj);
	public IFilterInflator CreateMessageTypeFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new MessageTypeFilterInflator(bizObj);
	public IFilterInflator CreateMovementReferenceNumberFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> CreateMovementReferenceNumberFilterInflatorCore(bizObj);
	public IFilterInflator CreateNotifyFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new NotifyFilterInflator(bizObj);
	public IFilterInflator CreatePackingMarksFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new PackingMarksFilterInflator(bizObj);
	public IFilterInflator CreatePackingTypeFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new PackingTypeFilterInflator(bizObj);
	public IFilterInflator CreatePresentationCustomsOfficeFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> CreatePresentationCustomsOfficeFilterInflatorCore(bizObj);
	public IFilterInflator CreatePresentationDateFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new PresentationDateFilterInflator(bizObj);
	public IFilterInflator CreatePresenterFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new PresenterFilterInflator(bizObj);
	public IFilterInflator CreatePreviousDocumentNumberFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new PreviousDocumentNumberFilterInflator(bizObj);
	public IFilterInflator CreatePreviousDocumentTypeFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new UCC6TemporaryStoragePreviousDocumentTypeFilterInflator(bizObj);
	public IFilterInflator CreateRepresentativeFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> CreateRepresentativeFilterInflatorCore(bizObj);
	public IFilterInflator CreateSupportingDocumentNumberFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new SupportingDocumentNumberFilterInflator(bizObj);
	public IFilterInflator CreateSupportingDocumentTypeFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new UCC6TemporaryStorageSupportingDocumentTypeFilterInflator(bizObj);
	public IFilterInflator CreateTariffFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new TariffFilterInflator(bizObj);
	public IFilterInflator CreateTransportationModeFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new TransportationModeFilterInflator(bizObj);
	public IFilterInflator CreateTransportIdFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new TransportIdFilterInflator(bizObj);
	public IFilterInflator CreateTransportTypeFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new TransportTypeFilterInflator(bizObj);

	#endregion

	#region Virtual Core Methods

	protected virtual IFilterInflator CreateCustomsOfficeFilterInflatorCore(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new CustomsOfficeFilterInflator(bizObj);
	protected virtual IFilterInflator CreateDeclarantFilterInflatorCore(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new DeclarantFilterInflator(bizObj);
	protected virtual IFilterInflator CreateLocalReferenceNumberFilterInflatorCore(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new LocalReferenceNumberFilterInflator(bizObj);
	protected virtual IFilterInflator CreateMovementReferenceNumberFilterInflatorCore(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new MovementReferenceNumberFilterInflator(bizObj);
	protected virtual IFilterInflator CreatePresentationCustomsOfficeFilterInflatorCore(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new PresentationCustomsOfficeFilterInflator(bizObj);
	protected virtual IFilterInflator CreateRepresentativeFilterInflatorCore(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new RepresentativeFilterInflator(bizObj);

	#endregion
}
