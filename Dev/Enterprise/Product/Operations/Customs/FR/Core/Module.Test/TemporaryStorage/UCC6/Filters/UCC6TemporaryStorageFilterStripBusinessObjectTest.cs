using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using TemporaryStorageHeader = Enterprise.Customs.FR.Business.CusTempStorage.TemporaryStorageHeader;

namespace Enterprise.Customs.FR.Module.TempStorage.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageFilterStripBusinessObject))]
	class UCC6TemporaryStorageFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new UCC6TemporaryStorageFilterStripBusinessObject();

		public void TestCompanyFilter() => AssertModuleTextFilter(AsycudaManifestHeaderSchema.Constants.AMA_ManifestType, DeclarationTypeFilterInflator.FilterDescription);

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

		void AssertModuleTextFilter(string propertyName, string filterName, bool saving = false)
		{
			var header1 = GetNewTemporaryStorageHeader();
			header1[propertyName] = "AB";

			var header2 = GetNewTemporaryStorageHeader();
			header2[propertyName] = "CD";

			if (saving)
			{
				Factory.Save();
			}

			var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBizObj[filterName];
			textFilter.IsActive = true;

			textFilter.Property = ZString.Empty;

			var collection = new TemporaryStorageHeaderCollection(Factory);
			collection.AdditionalFilter = filterStripBizObj.Filter;
			AssertEquals(2, collection.Count);
			AssertEquals(header1.PK, collection[0].PK);
			AssertEquals(header2.PK, collection[1].PK);

			textFilter.Property = "AB";

			collection.AdditionalFilter = filterStripBizObj.Filter;
			AssertEquals(1, collection.Count);
			AssertEquals(header1.PK, collection[0].PK);
		}

		TemporaryStorageHeader GetNewTemporaryStorageHeader()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TemporaryStorage;
			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();

			if (GlbCompany.CurrentCompany.Country.Code != Core.Constants.CountryCodes.France)
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.France);
			}
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
			typeof(DeclarationTypeFilterInflator),
			typeof(EU.TemporaryStorage.Module.JobNumberFilterInflator),
			typeof(EU.TemporaryStorage.Module.LocalReferenceNumberFilterInflator),
			typeof(EU.TemporaryStorage.Module.MessageTypeFilterInflator),
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
