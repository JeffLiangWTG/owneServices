namespace Enterprise.Accounting.Business.ConsolCosting.Testing
{
	using System;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ConsolRevenue;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Registry.Business;

	public class ConsolRevenueLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSupplyTypes()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.AddNew();
			Factory.Save();

			var consolRevenueMaster = new ConsolRevenueMaster(consol, Factory);
			var rev = new ConsolRevenue(consolRevenueMaster);
			var jobConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			AssertEquals("Default value", "LOC, LOX, LOA, INT, INX, INA, DSB", rev.Lookups.SupplyTypes.CodesAsString);

			var values = new CodeDescriptionBoolDisallowNewCollection(AccountingMasterFilesConstants.SupplyTypeClassificationList);
			values.Cast<CodeDescriptionBool>().ForEach(x => x.Bool = true);
			values[0].Bool = false;
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, values))
			{
				AssertEquals("LOC was removed from list", "LOX, LOA, INT, INX, INA, DSB", rev.Lookups.SupplyTypes.CodesAsString);
			}

			values.Cast<CodeDescriptionBool>().ForEach(x => x.Bool = false);
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, values))
			{
				AssertNullOrEmpty("All removed", rev.Lookups.SupplyTypes.CodesAsString);
			}
		}

		public void TestChargeCodeCollectionFilters()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			GlbDepartment yYYDept = Factory.NewWithValidTestData<GlbDepartment>();
			yYYDept.GE_Code = "YYY";
			GlbDepartment zZZDept = Factory.NewWithValidTestData<GlbDepartment>();
			zZZDept.GE_Code = "ZZZ";
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			var consolRevenueMaster = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue rev = new ConsolRevenue(consolRevenueMaster);
			rev.SplitCharges[0].JR_GE = yYYDept.PK;
			rev.SplitCharges[1].JR_GE = zZZDept.PK;
			FilterBusinessObjectDefault @default = new ConsolRevenueLookups(rev).ChargeCodes.FilterBusinessObjectDefaults["Dept Filter" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"];
			AssertEquals("YYY, ZZZ, ALL", (ZString)@default.Value);
			consolRevenueMaster.ReleaseMutexes();
		}

		public void TestChargeCodeCollectionFiltersOnChargeType()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			var consolRevenueMaster = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue rev = new ConsolRevenue(consolRevenueMaster);
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode5 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode3.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			chargeCode4.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode5.AC_ChargeType = "ABC";
			rev.Lookups.ChargeCodes.Load();
			AssertCollectionContains("Collection should contain MRG Type Charge Code", chargeCode1, rev.Lookups.ChargeCodes);
			AssertCollectionContains("Collection should contain DSB Type Charge Code", chargeCode2, rev.Lookups.ChargeCodes);
			AssertCollectionContains("Collection should contain MJA Type Charge Code", chargeCode3, rev.Lookups.ChargeCodes);
			AssertCollectionContains("Collection should contain REV Type Charge Code", chargeCode4, rev.Lookups.ChargeCodes);
			AssertCollectionNotContains("Collection should not contain ABC Type Charge Code", chargeCode5, rev.Lookups.ChargeCodes);
			consolRevenueMaster.ReleaseMutexes();
		}
	}
}
