using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.AirCargo.Testing
{
	[TestedType(typeof(AirCargoDepotStandAloneFilterStripBusinessObject))]
	sealed class AirCargoDepotStandAloneFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestGetStatusList()
		{
			AssertEquals("List should contain everything", 80, filterBO.GetStatusList(null).Count);
			AssertEquals(29, filterBO.GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRUnderbond).Count);
			AssertEquals(13, filterBO.GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRCustoms).Count);
			AssertEquals(10, filterBO.GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRMessage).Count);
			AssertEquals(29, filterBO.GetStatusList(AirCargoFilterConstants.StatusFilterType.Outturn).Count);
		}

		public void TestOriginAddressFilter()
		{
			underbond1.C4_ParentID = mAWB1.PK;
			underbond2.C4_ParentID = mAWB2.PK;
			underbond1.C4_OA_OriginAddress = address1.PK;
			underbond2.C4_OA_OriginAddress = address2.PK;
			address1.OA_Address1 = "BBBBBBBBBB";
			address1.OA_Address2 = "CCCCAAACCC";
			address2.OA_Address1 = "DDAAADDDDD";
			address2.OA_Address2 = "EEEEEEEEEE";
			Factory.Save();
			ModuleTextFilter establishmentFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.EstablishmentTypes.OriginAddress];
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			establishmentFilter.Property = "BBB";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain mAWB2", !filterCollection.Contains(mAWB2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			establishmentFilter.Property = "DDAAADDDDD";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain mAWB2", filterCollection.Contains(mAWB2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "AAA";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain mAWB2", filterCollection.Contains(mAWB2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "ZZZ";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain mAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestOriginCodeFilter()
		{
			underbond1.C4_ParentID = mAWB1.PK;
			underbond2.C4_ParentID = mAWB2.PK;
			underbond1.C4_OriginPremiseID = "XAAAX";
			underbond2.C4_OriginPremiseID = "YAAAY";
			Factory.Save();
			ModuleTextFilter establishmentFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.EstablishmentTypes.OriginCode];
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			establishmentFilter.Property = "XAA";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain mAWB2", !filterCollection.Contains(mAWB2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			establishmentFilter.Property = "YAAAY";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain mAWB2", filterCollection.Contains(mAWB2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "AAA";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain mAWB2", filterCollection.Contains(mAWB2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "ZZZ";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain mAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestDestinationAddressFilter()
		{
			underbond1.C4_ParentID = mAWB1.PK;
			underbond2.C4_ParentID = mAWB2.PK;
			underbond1.C4_OA_DestinationAddress = address1.PK;
			underbond2.C4_OA_DestinationAddress = address2.PK;
			address1.OA_Address1 = "BBBBBBBBBB";
			address1.OA_Address2 = "CCCCAAACCC";
			address2.OA_Address1 = "DDAAADDDDD";
			address2.OA_Address2 = "EEEEEEEEEE";
			Factory.Save();
			ModuleTextFilter establishmentFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.EstablishmentTypes.DestinationAddress];
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			establishmentFilter.Property = "BBB";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain mAWB2", !filterCollection.Contains(mAWB2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			establishmentFilter.Property = "DDAAADDDDD";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain mAWB2", filterCollection.Contains(mAWB2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "AAA";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain mAWB2", filterCollection.Contains(mAWB2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "ZZZ";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain mAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestDestinationCodeFilter()
		{
			underbond1.C4_ParentID = mAWB1.PK;
			underbond2.C4_ParentID = mAWB2.PK;
			underbond1.C4_DestinationPremiseID = "XAAAX";
			underbond2.C4_DestinationPremiseID = "YAAAY";
			Factory.Save();
			ModuleTextFilter establishmentFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.EstablishmentTypes.DestinationCode];
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			establishmentFilter.Property = "XAA";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain mAWB2", !filterCollection.Contains(mAWB2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			establishmentFilter.Property = "YAAAY";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain mAWB2", filterCollection.Contains(mAWB2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "AAA";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain mAWB2", filterCollection.Contains(mAWB2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "ZZZ";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain mAWB2", !filterCollection.Contains(mAWB2));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new AirCargoDepotStandAloneFilterStripBusinessObject();

		CusMAWB mAWB1;
		CusMAWB mAWB2;
		CusUnderbond underbond1;
		CusUnderbond underbond2;
		ModuleMAWBCollection filterCollection;
		OrgAddress address1;
		OrgAddress address2;
		AirCargoDepotStandAloneFilterStripBusinessObject filterBO;
		protected override void SetUp()
		{
			base.SetUp();
			mAWB1 = Factory.NewWithValidTestData<CusMAWB>();
			mAWB2 = Factory.NewWithValidTestData<CusMAWB>();
			underbond1 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond2 = Factory.NewWithValidTestData<CusUnderbond>();
			filterCollection = new ModuleMAWBCollection(Factory);

			address1 = Factory.NewWithValidTestData<OrgAddress>();
			address2 = Factory.NewWithValidTestData<OrgAddress>();
			filterBO = (AirCargoDepotStandAloneFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
