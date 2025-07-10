using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Module.Testing;

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
		// New factory methods
		(typeof(CustomsStatusDateFilterInflator), f.CreateCustomsStatusDateFilterInflator),
		(typeof(LocationOfGoodsAuthorizationNumberFilterInflator), f.CreateLocationOfGoodsAuthorizationNumberFilterInflator),
		(typeof(LocationOfGoodsOrganizationFilterInflator), f.CreateLocationOfGoodsOrganizationFilterInflator),
		(typeof(LocationOfGoodsPlaceIdFilterInflator), f.CreateLocationOfGoodsPlaceIdFilterInflator),
		(typeof(MessageStatusFilterInflator), f.CreateMessageStatusFilterInflator),
		(typeof(RegistrationNumberFilterInflator), f.CreateRegistrationNumberFilterInflator),
		(typeof(ReleaseDateFilterInflator), f.CreateReleaseDateFilterInflator),
		// Overridden methods
		(typeof(SupervisingCustomsOfficeFilterInflator), f.CreateCustomsOfficeFilterInflator),
		(typeof(DeclarantFilterInflator), f.CreateDeclarantFilterInflator),
		(typeof(LocalReferenceNumberFilterInflator), f.CreateLocalReferenceNumberFilterInflator),
		(typeof(MovementReferenceNumberFilterInflator), f.CreateMovementReferenceNumberFilterInflator),
		(typeof(PresentationCustomsOfficeFilterInflator), f.CreatePresentationCustomsOfficeFilterInflator),
		(typeof(RepresentativeFilterInflator), f.CreateRepresentativeFilterInflator)
	];
}
