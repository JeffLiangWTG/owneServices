using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Module.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageFilterStripBusinessObject))]
	sealed class UCC6TemporaryStorageFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new UCC6TemporaryStorageFilterStripBusinessObject();

		public void TestMessageVersion()
		{
			var header1 = Factory.New<TemporaryStorageHeader>();
			header1.AMA_ManifestType = "V1";

			var header2 = Factory.New<TemporaryStorageHeader>();
			header2.AMA_ManifestType = "V2";

			var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBizObj["Message Version"];
			textFilter.IsActive = true;

			textFilter.Property = ZString.Empty;

			var collection = new EU.Business.CusTempStorage.TemporaryStorageHeaderCollection(Factory);
			collection.AdditionalFilter = filterStripBizObj.Filter;
			AssertEquals(2, collection.Count);
			Assert(collection.Contains(header1));
			Assert(collection.Contains(header2));

			textFilter.Property = "V1";

			collection.AdditionalFilter = filterStripBizObj.Filter;
			AssertEquals(1, collection.Count);
			AssertEquals(header1.PK, collection[0].PK);
		}

		public void TestGetFilterInflatorFactory() => AssertType<UCC6TemporaryStorageFilterInflatorFactory>(new UCC6TemporaryStorageFilterStripBusinessObjectForTest().GetFilterInflatorFactory_Exposed());

		public void TestGetFilterInflators()
		{
			var filterStrip = new UCC6TemporaryStorageFilterStripBusinessObjectForTest();
			var filterInflators = filterStrip.GetFilterInflators_Exposed().WhereNotNull().ToList();
			AssertEquals(ExpectedFilterInflatorTypes.Count, filterInflators.Count);

			var inflatorTypes = filterInflators.ToDictionary(inf => inf.GetType());
			CombineAssertions(() => ExpectedFilterInflatorTypes.ForEach(t =>
				Assert($"Filter inflator '{t.FullName}' should be present", inflatorTypes.ContainsKey(t))));
		}

		List<Type> ExpectedFilterInflatorTypes =>
		[
			typeof(EU.TemporaryStorage.Module.ApplicationCodeFilterInflator),
			typeof(EU.TemporaryStorage.Module.BillNumberFilterInflator),
			typeof(EU.TemporaryStorage.Module.BranchFilterInflator),
			typeof(EU.TemporaryStorage.Module.ConsigneeFilterInflator),
			typeof(EU.TemporaryStorage.Module.ConsignorFilterInflator),
			typeof(EU.TemporaryStorage.Module.CountryFilterInflator),
			typeof(EU.TemporaryStorage.Module.CustomsOfficeFilterInflator),
			typeof(EU.TemporaryStorage.Module.CustomsStatusFilterInflator),
			typeof(EU.TemporaryStorage.Module.DeclarantFilterInflator),
			typeof(EU.TemporaryStorage.Module.JobNumberFilterInflator),
			typeof(EU.TemporaryStorage.Module.LocalReferenceNumberFilterInflator),
			typeof(EU.TemporaryStorage.Module.MessageTypeFilterInflator),
			typeof(MessageVersionFilterInflator),
			typeof(EU.TemporaryStorage.Module.MovementReferenceNumberFilterInflator),
			typeof(EU.TemporaryStorage.Module.NotifyFilterInflator),
			typeof(EU.TemporaryStorage.Module.PackingMarksFilterInflator),
			typeof(EU.TemporaryStorage.Module.PackingTypeFilterInflator),
			typeof(EU.TemporaryStorage.Module.PresentationCustomsOfficeFilterInflator),
			typeof(EU.TemporaryStorage.Module.PresentationDateFilterInflator),
			typeof(EU.TemporaryStorage.Module.PresenterFilterInflator),
			typeof(EU.TemporaryStorage.Module.PreviousDocumentNumberFilterInflator),
			typeof(EU.TemporaryStorage.Module.UCC6TemporaryStoragePreviousDocumentTypeFilterInflator),
			typeof(EU.TemporaryStorage.Module.RepresentativeFilterInflator),
			typeof(EU.TemporaryStorage.Module.SupportingDocumentNumberFilterInflator),
			typeof(EU.TemporaryStorage.Module.UCC6TemporaryStorageSupportingDocumentTypeFilterInflator),
			typeof(EU.TemporaryStorage.Module.TariffFilterInflator),
			typeof(EU.TemporaryStorage.Module.TransportationModeFilterInflator),
			typeof(EU.TemporaryStorage.Module.TransportIdFilterInflator),
			typeof(EU.TemporaryStorage.Module.TransportTypeFilterInflator)
		];

		sealed class UCC6TemporaryStorageFilterStripBusinessObjectForTest : UCC6TemporaryStorageFilterStripBusinessObject
		{
			public EU.TemporaryStorage.Module.UCC6TemporaryStorageFilterInflatorFactory GetFilterInflatorFactory_Exposed() => GetFilterInflatorFactory();
			public List<IFilterInflator> GetFilterInflators_Exposed() => GetFilterInflators();
		}
	}
}
