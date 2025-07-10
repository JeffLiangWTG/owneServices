using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(UPEConsolReportingFilterBusinessObject))]
	public class UPEConsolReportingFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		[TestDate(2006, 8, 14)]
		public void TestAddLoadedDateFilter()
		{
			UPECusMAWB mAWB1 = Factory.NewWithValidTestData<UPECusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2006, 8, 15);
			UPECusMAWB mAWB2 = Factory.NewWithValidTestData<UPECusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2006, 8, 16);
			UPECusMAWB mAWB3 = Factory.NewWithValidTestData<UPECusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();
			UPEConsolReportingFilterBusinessObject filterBizO = new UPEConsolReportingFilterBusinessObject();
			ModuleDateFilter dateFilter = (ModuleDateFilter)filterBizO["Loaded Date"];
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property2 = new DateTime(2006, 8, 14);
			AssertCorrectUPECusMAWBIsLoaded(mAWB1.PK, filterBizO.Filter);
			dateFilter.Property1 = new ZDateTime(2006, 8, 15);
			dateFilter.Property2 = new ZDateTime(2006, 8, 15);
			AssertCorrectUPECusMAWBIsLoaded(mAWB2.PK, filterBizO.Filter);
			dateFilter.Property1 = new ZDateTime(2006, 8, 16);
			dateFilter.Property2 = ZDateTime.Empty;
			AssertCorrectUPECusMAWBIsLoaded(mAWB3.PK, filterBizO.Filter);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new UPEConsolReportingFilterBusinessObject();
		}

		void AssertCorrectUPECusMAWBIsLoaded(ZGuid pK, ZQuery filter)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			UPECusMAWB[] result = newFactory.Load<UPECusMAWB>(filter);
			AssertEquals("Should find 1 UPECusMAWB", 1, result.Length);
			AssertEquals("Incorrect UPECusMAWB was loaded", pK, result[0].PK);
		}
		#endregion
	}
}
