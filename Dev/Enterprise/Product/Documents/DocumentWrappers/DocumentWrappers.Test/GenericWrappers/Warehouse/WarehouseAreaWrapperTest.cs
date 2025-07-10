using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Barcode.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseAreaWrapper))]
	sealed class WarehouseAreaWrapperTest : FreightWrapperTest
	{
		#region Properties

		#region TestWarehoueName_Translatable

		public void TestWarehoueName_Translatable()
		{
			var warehouse = Helper.CreateWarehouse("Whs1");
			var area = Helper.CreateArea(warehouse, "Area");

			var wrapper = new WarehouseAreaWrapper(area, Factory);
			AssertEquals("WarehouseName in English.", "Whs1", wrapper.WarehouseName);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "Whs1").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "仓库1"));
				AssertEquals("WarehouseName in Chinese.", "仓库1", wrapper.WarehouseName);
			}
		}

		#endregion

		#region TestAreaName_Translatable

		public void TestAreaName_Translatable()
		{
			var warehouse = Helper.CreateWarehouse("Whs1");
			var area = Helper.CreateArea(warehouse, "Area");

			var wrapper = new WarehouseAreaWrapper(area, Factory);
			AssertEquals("AreaName in English.", "Area", wrapper.AreaName);

			var resKey = area.WA_NameInfo.CustomizableDataResourceStrings.GetMultilingualString(area, "Area").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "仓库1"));
				AssertEquals("AreaName in Chinese.", "仓库1", wrapper.AreaName);
			}
		}

		#endregion

		protected override ZString ExpectedWarehouseName
		{
			get
			{
				return Area.Warehouse.WW_WarehouseNameMultilingual;
			}
		}

		protected override ZString ExpectedAreaName
		{
			get
			{
				return Area.WA_NameMultilingual;
			}
		}

		protected override ZString ExpectedAreaBarcode
		{
			get
			{
				return new TextBarcode(Area.WA_Name).TextAs128sFontString;
			}
		}

		#endregion

		#region Implementation

		#region Overrides

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "AreaBarcode", "ÈDEFAULT.Ê" },
					{ "AreaName", "DEFAULT" },
					{ "WarehouseName", "WHS" },
				};
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Area;
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new WarehouseAreaWrapper(Area, Factory);
		}

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region Area

		WhsArea Area
		{
			get { return area ?? (area = (WhsArea)GetNewWhsBusinessObject()); }
		}
		WhsArea area;

		#endregion

		#region GetNewWhsBusinessObject

		BusinessObject GetNewWhsBusinessObject()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS", "A", 2, 2);
			var area = warehouse.Areas[0];

			return area;
		}

		#endregion

		#endregion

		#region Helper

		WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}
		WhsTestHelperFunctionsEnv helper;

		#endregion
	}
}
