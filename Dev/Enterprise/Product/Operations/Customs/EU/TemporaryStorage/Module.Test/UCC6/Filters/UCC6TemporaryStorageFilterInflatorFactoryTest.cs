using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing;

[TestedType(typeof(UCC6TemporaryStorageFilterInflatorFactory))]
sealed class UCC6TemporaryStorageFilterInflatorFactoryTest : TestCaseWithFactory
{
	public void TestFactoryMethods()
	{
		var bizObj = FilterStripBizObj;
		CombineAssertions(() =>
		{
			FactoryMethodTestCases(FilterInflatorFactory)
				.ForEach(tc => AssertType(tc.ExpectedType, tc.Create(bizObj)));
		});
	}

	UCC6TemporaryStorageFilterStripBusinessObject FilterStripBizObj => new UCC6TemporaryStorageFilterStripBusinessObject();
	UCC6TemporaryStorageFilterInflatorFactory FilterInflatorFactory => new UCC6TemporaryStorageFilterInflatorFactory();

	List<(Type ExpectedType, Func<UCC6TemporaryStorageFilterStripBusinessObject, IFilterInflator> Create)> FactoryMethodTestCases(UCC6TemporaryStorageFilterInflatorFactory f) =>
	[
		(typeof(ApplicationCodeFilterInflator), f.CreateApplicationCodeFilterInflator),
		(typeof(BillNumberFilterInflator), f.CreateBillNumberFilterInflator),
		(typeof(BranchFilterInflator), f.CreateBranchFilterInflator),
		(typeof(ConsigneeFilterInflator), f.CreateConsigneeFilterInflator),
		(typeof(ConsignorFilterInflator), f.CreateConsignorFilterInflator),
		(typeof(CountryFilterInflator), f.CreateCountryFilterInflator),
		(typeof(CustomsOfficeFilterInflator), f.CreateCustomsOfficeFilterInflator),
		(typeof(CustomsStatusFilterInflator), f.CreateCustomsStatusFilterInflator),
		(typeof(DeclarantFilterInflator), f.CreateDeclarantFilterInflator),
		(typeof(JobNumberFilterInflator), f.CreateJobNumberFilterInflator),
		(typeof(LocalReferenceNumberFilterInflator), f.CreateLocalReferenceNumberFilterInflator),
		(typeof(MessageTypeFilterInflator), f.CreateMessageTypeFilterInflator),
		(typeof(MovementReferenceNumberFilterInflator), f.CreateMovementReferenceNumberFilterInflator),
		(typeof(NotifyFilterInflator), f.CreateNotifyFilterInflator),
		(typeof(PackingMarksFilterInflator), f.CreatePackingMarksFilterInflator),
		(typeof(PackingTypeFilterInflator), f.CreatePackingTypeFilterInflator),
		(typeof(PresentationCustomsOfficeFilterInflator), f.CreatePresentationCustomsOfficeFilterInflator),
		(typeof(PresentationDateFilterInflator), f.CreatePresentationDateFilterInflator),
		(typeof(PresenterFilterInflator), f.CreatePresenterFilterInflator),
		(typeof(PreviousDocumentNumberFilterInflator), f.CreatePreviousDocumentNumberFilterInflator),
		(typeof(UCC6TemporaryStoragePreviousDocumentTypeFilterInflator), f.CreatePreviousDocumentTypeFilterInflator),
		(typeof(RepresentativeFilterInflator), f.CreateRepresentativeFilterInflator),
		(typeof(SupportingDocumentNumberFilterInflator), f.CreateSupportingDocumentNumberFilterInflator),
		(typeof(UCC6TemporaryStorageSupportingDocumentTypeFilterInflator), f.CreateSupportingDocumentTypeFilterInflator),
		(typeof(TariffFilterInflator), f.CreateTariffFilterInflator),
		(typeof(TransportationModeFilterInflator), f.CreateTransportationModeFilterInflator),
		(typeof(TransportIdFilterInflator), f.CreateTransportIdFilterInflator),
		(typeof(TransportTypeFilterInflator), f.CreateTransportTypeFilterInflator)
	];
}
