using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class WarehouseMatcherTest : TestCaseWithFactory
	{
		public void TestMatch()
		{
			AddMatch(mappingOrg, "AA", warehouse.PK);
			AddMatch(mappingOrg, "BB", ZGuid.Empty);
			Factory.Save();

			AssertMatch("AA", warehouse.PK);
			AssertMatch("BB", ZGuid.Empty);
		}

		#region Implementation

		void AssertMatch(string foreignCode, ZGuid matchGuid)
		{
			var columnValuePair = new NKColumnValuePair(ObjectFactory.GetType<Enterprise.Warehouse.Integration.IWhsWarehouse>(), WhsWarehouseSchema.WW_WarehouseCode, foreignCode);
			var matcher = new WarehouseMatcher(Factory, mappingOrg.PK, columnValuePair.Value);

			if (matchGuid.IsEmpty)
			{
				AssertEquals("Warehouse: should not match", false, matcher.Match());
			}
			else
			{
				AssertEquals("Warehouse: should match", matcher.Result, matchGuid);
			}
		}

		OrgHeader mappingOrg;
		BusinessObject warehouse;

		OrgPatternMatchOverride AddMatch(OrgHeader orgProxy, ZString foreignCode, ZGuid warehousePK)
		{
			OrgPatternMatchOverride match = orgProxy.CreatePatternMatchOverrideForTest();
			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Warehouse;
			match.OO_ForeignCode = foreignCode;
			if (!warehousePK.IsEmpty)
			{
				match.OO_LocalGuid = warehousePK;
			}
			return match;
		}

		protected override void SetUp()
		{
			base.SetUp();
			mappingOrg = Factory.New<OrgHeader>();
			mappingOrg.OH_Code = "MAPPORG";
			mappingOrg.OH_FullName = "Mapping Org";

			warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Warehouse.Integration.IWhsWarehouse)));
			PropertyInfo propertyInfo = ObjectFactory.GetType<Enterprise.Warehouse.Integration.IWhsWarehouse>().GetProperty("WW_WarehouseName", BindingFlags.Public | BindingFlags.Instance);
			propertyInfo.SetValue(warehouse, new ZString("Warehouse"), null);
			propertyInfo = ObjectFactory.GetType<Enterprise.Warehouse.Integration.IWhsWarehouse>().GetProperty("WW_WarehouseCode", BindingFlags.Public | BindingFlags.Instance);
			propertyInfo.SetValue(warehouse, new ZString("WHS"), null);

			Factory.Save();
		}

		#endregion
	}
}
