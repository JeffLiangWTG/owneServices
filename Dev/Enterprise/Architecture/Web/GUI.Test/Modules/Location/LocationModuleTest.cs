using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(LocationModule))]
	sealed class LocationModuleTest : ZFilterGridModuleTest
	{
		#region Setup

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(typeof(RefUNLOCO), isCancelled);
			var location = result as RefUNLOCO;
			if (location != null)
			{
				location.RL_PortName = string.Format("TST{0}", DateTime.Now.Ticks);
			}
			return result;
		}

		protected override void AddAdditionalFilterForActiveStatusTest(ZQuery filter)
		{
			base.AddAdditionalFilterForActiveStatusTest(filter);
			filter.AddToFilter(RefUNLOCOSchema.RL_PortName, SQLComparisonOperator.StartsWith, "TST");
		}

		protected override bool GetShoudTestLoadDBHitsWithDBOnlyQuery(IBusinessObjectCollection collection)
		{
			return false;
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.Location; }
		}

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			return Array.Empty<FilterBusinessObjectDefault>();
		}

		#endregion

		#region TestDefaultOrdering

		public override void TestGetSortInfos()
		{
			var filterBizO = FilterGridModule.CreateNewFilterBusinessObject() as WebLocationFilterBusinessObject;
			AssertNotNull(filterBizO);
			filterBizO.IsPort = true;
			AssertSortInfos(new[] { new ColumnAndSortOrder(RefUNLOCOSchema.RL_Code.Name, ListSortDirection.Ascending) }, FilterGridModule.GetSortInfos(filterBizO));
			filterBizO.IsCountry = true;
			AssertSortInfos(new[] { new ColumnAndSortOrder(RefCountrySchema.RN_Code.Name, ListSortDirection.Ascending) }, FilterGridModule.GetSortInfos(filterBizO));
			filterBizO.IsRegion = true;
			AssertSortInfos(new[] { new ColumnAndSortOrder(RefZoneHeaderSchema.FZ_Code.Name, ListSortDirection.Ascending) }, FilterGridModule.GetSortInfos(filterBizO));
		}

		#endregion

		#region TestLoadCollectionReturnsRowCount

		public override void TestLoadCollectionReturnsRowCount()
		{
			WebLocationFilterBusinessObject filterBizO = (WebLocationFilterBusinessObject)FilterGridModule.CreateNewFilterBusinessObject();
			ZQuery filter = filterBizO.Filter;

			AssertEquals("LocationModule should limit collection to 1000 rows by default", 1000, FilterGridModule.MaxRows);
			FilterGridModule.MaxRows = 250;
			filterBizO.IsPort = ZBool.True;
			int expectedCount = Factory.GetDatabaseCount(typeof(RefUNLOCO), filter);
			expectedCount = expectedCount > FilterGridModule.MaxRows ? FilterGridModule.MaxRows : expectedCount;

			int actualCount = FilterGridModule.LoadCollection(filterBizO);
			AssertEquals("LoadCollection should have returned expected number of rows", expectedCount, FilterGridModule.GridCollection.Count);
			AssertEquals("LoadCollection should return row count", expectedCount, actualCount);

			filterBizO.IsRegion = ZBool.True;
			filter = filterBizO.Filter;
			FilterGridModule.MaxRows = 150;
			((BusinessObjectCollection)FilterGridModule.GridCollection).RemoveAll();
			expectedCount = Factory.GetDatabaseCount(typeof(RefZoneHeader), filter);
			expectedCount = expectedCount > FilterGridModule.MaxRows ? FilterGridModule.MaxRows : expectedCount;
			actualCount = FilterGridModule.LoadCollection(filterBizO);
			AssertEquals("LoadCollection should not have returned more than 150 rows", expectedCount, FilterGridModule.GridCollection.Count);
			AssertEquals("LoadCollection should return row count", expectedCount, actualCount);

			filterBizO.IsCountry = ZBool.True;
			filter = filterBizO.Filter;
			FilterGridModule.MaxRows = 150;
			((BusinessObjectCollection)FilterGridModule.GridCollection).RemoveAll();
			expectedCount = Factory.GetDatabaseCount(typeof(RefCountry), filter);
			expectedCount = expectedCount > FilterGridModule.MaxRows ? FilterGridModule.MaxRows : expectedCount;
			actualCount = FilterGridModule.LoadCollection(filterBizO);
			AssertEquals("LoadCollection should not have returned more than 150 rows", expectedCount, FilterGridModule.GridCollection.Count);
			AssertEquals("LoadCollection should return row count", expectedCount, actualCount);

			FilterGridModule.MaxRows = 0;
			((BusinessObjectCollection)FilterGridModule.GridCollection).RemoveAll();
			actualCount = FilterGridModule.LoadCollection(filterBizO);
			expectedCount = (FilterGridModule.MaxRows < actualCount) ? FilterGridModule.MaxRows : actualCount;
			expectedCount = expectedCount > FilterGridModule.MaxRows ? FilterGridModule.MaxRows : expectedCount;
			AssertEquals("LoadCollection should have returned all rows", expectedCount, FilterGridModule.GridCollection.Count);
		}
		#endregion
	}
}
