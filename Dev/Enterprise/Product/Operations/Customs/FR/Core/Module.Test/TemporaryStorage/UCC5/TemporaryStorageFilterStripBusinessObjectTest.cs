using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.TemporaryStorage.Module;
using Enterprise.Customs.FR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.TempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageFilterStripBusinessObject))]
	class TemporaryStorageFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new TemporaryStorageFilterStripBusinessObject();

		public void TestCustomerReference() => AssertModuleTextFilter(CusTempStorageJobHeader.Schema.SJH_ReferenceNumber, "Customer Reference");

		public void TestTransportRegNo() => AssertModuleTextFilter(CusTempStorageJobHeader.Schema.SJH_TransportRegNo, "Transport Reg. No.");

		public void TestCustomsOffice() => AssertModuleTextFilter(CusTempStorageJobHeader.Schema.SJH_CustomsOffice, "Customs Office");

		public void TestCustomer() => AssertModuleGuidFilter(CusTempStorageJobHeaderSchema.Constants.SJH_OH_Customer, "Customer");

		public void TestPresenter() => AssertModuleGuidFilter(CusTempStorageJobHeaderSchema.Constants.SJH_OA_Presenter, "Presenter");

		public void TestJobNumber() => AssertModuleTextFilter(CusTempStorageJobHeaderSchema.Constants.SJH_JobReference, "Job #");

		public void TestDDTNumber() => AssertModuleTextFilter(Business.CusTempStorage.CusTempStorageJobHeader.Schema.DDTNumber, "TSD Number");

		public void TestGetFilterInflators()
		{
			var filterStrip = new TemporaryStorageFilterStripBusinessObjectForTest();
			var filterInflators = filterStrip.GetFilterInflators_Exposed();
			AssertContainsExactElementsInAnyOrder(ExpectedFilterInflatorTypes, filterInflators.Select(inf => inf.GetType()));
		}

		void AssertModuleTextFilter(string propertyName, string filterName)
		{
			var header1 = GetNewTempStorageJobHeader();
			header1[propertyName] = "ABC";

			var header2 = GetNewTempStorageJobHeader();
			header2[propertyName] = "DEF";

			Factory.Save();

			var filter = new TemporaryStorageFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filter[filterName];

			textFilter.Property = ZString.Empty;
			textFilter.IsActive = true;

			var coll = new CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>(Factory, GlbBranch.CurrentBranch);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(2, coll.Count);
			AssertEquals(header1.PK, coll[0].PK);
			AssertEquals(header2.PK, coll[1].PK);

			textFilter.Property = "DEF";
			coll.AdditionalFilter = filter.Filter;
			coll.RefreshFromDb();
			AssertEquals(1, coll.Count);
			AssertEquals(header2.PK, coll[0].PK);
		}

		void AssertModuleGuidFilter(string propertyName, string filterName)
		{
			var header1 = GetNewTempStorageJobHeader();
			var header2 = GetNewTempStorageJobHeader();

			Factory.Save();
			header1[propertyName] = ZGuid.NewZGuid();
			header2[propertyName] = ZGuid.NewZGuid();

			var filterStripBizObj = new TemporaryStorageFilterStripBusinessObject();

			var guidFilter = (ModuleGuidFilter)filterStripBizObj[filterName];
			guidFilter.IsActive = true;
			guidFilter.Property = ZGuid.Empty;

			var collection = new CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>(Factory, GlbBranch.CurrentBranch);
			collection.AdditionalFilter = filterStripBizObj.Filter;
			AssertEquals(2, collection.Count);
			AssertEquals(header1.PK, collection[0].PK);
			AssertEquals(header2.PK, collection[1].PK);

			guidFilter.Property = (ZGuid)header1[propertyName];

			collection.AdditionalFilter = filterStripBizObj.Filter;
			AssertEquals(1, collection.Count);
			AssertEquals(header1.PK, collection[0].PK);
		}

		CusTempStorageJobHeader GetNewTempStorageJobHeader()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_GB = GlbBranch.CurrentBranch.PK;
			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			return header;
		}

		Type[] ExpectedFilterInflatorTypes =>
		[
			typeof(CustomerFilterInflator),
			typeof(CustomerReferenceFilterInflator),
			typeof(CustomsOfficeFilterInflator),
			typeof(DdtNumberFilterInflator),
			typeof(JobNumberFilterInflator),
			typeof(PresenterFilterInflator),
			typeof(TransportRegNoFilterInflator)
		];

		sealed class TemporaryStorageFilterStripBusinessObjectForTest : TemporaryStorageFilterStripBusinessObject
		{
			public List<IFilterInflator> GetFilterInflators_Exposed() => GetFilterInflators();
		}
	}
}
