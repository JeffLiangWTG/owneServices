using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LandedCostingPreferencesRegistryItem))]
	sealed class LandedCostingPreferencesRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<LandedCostingGroupCollection>
	{
		public void TestDefaultValue()
		{
			LandedCostingGroupCollection value = RegistryItem.Value;

			ZQuery filter = new ZQuery(AccChargeCodeSchema.AC_GC, Env.CurrentCompany.PK);
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, "PS");
			Guid chargeCodePK = ((BusinessObject)Factory.LoadTop1<Enterprise.MasterFiles.Integration.IAccChargeCode>(filter)).PK.ToGuid();

			ChargeGroupAndChargeCode charge = value[0].Charges[0];
			AssertEquals("Charge.ChargeCodePK", chargeCodePK, charge.ChargeCodePK);
		}

		public void TestCompanyLevelValue()
		{
			LandedCostingGroupCollection collection = new LandedCostingGroupCollection();
			LandedCostingGroup landedCostingGroup = collection.AddNew();
			ChargeGroupAndChargeCode charge = landedCostingGroup.Charges.AddNew();

			landedCostingGroup.GroupID = 2;
			landedCostingGroup.GroupName = "Group 2";
			landedCostingGroup.CostDistributionCode = "AWV";

			ZGuid newGuid = ZGuid.NewZGuid();

			using (charge.GetValidationSuspender())
			{
				charge.ChargeCodePK = newGuid;
			}

			RegistryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			LandedCostingGroupCollection companyValue = RegistryItem.Value;

			AssertEquals("CompanyValue[0].GroupID", 2, companyValue[0].GroupID);
			AssertEquals("CompanyValue[0].GroupName", "Group 2", companyValue[0].GroupName);
			AssertEquals("CompanyValue[0].CostDistributionCode", "AWV", companyValue[0].CostDistributionCode);
			AssertEquals("CompanyValue[0].Charges[0].ChargeCodePK", newGuid, companyValue[0].Charges[0].ChargeCodePK);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			LandedCostingGroupCollection collection = new LandedCostingGroupCollection();
			LandedCostingGroup landedCostingGroup = collection.AddNew();
			ChargeGroupAndChargeCode charge = landedCostingGroup.Charges.AddNew();

			landedCostingGroup.GroupID = 1;
			landedCostingGroup.GroupName = "Group";
			landedCostingGroup.CostDistributionCode = "AWV";

			charge.ChargeCodeCodeForDefaultValue = "PS";

			RegistryItem = new LandedCostingPreferencesRegistryItem("", null, null, null, RegistryStorageFlags.Company, collection);
		}

		protected override StronglyTypedRegistryItem<LandedCostingGroupCollection, LandedCostingGroupCollection> GetNewRegistryItem()
		{
			return new LandedCostingPreferencesRegistryItem("", null, null, null, RegistryStorageFlags.System, new LandedCostingGroupCollection());
		}

		LandedCostingPreferencesRegistryItem RegistryItem;

		#endregion
	}
}
