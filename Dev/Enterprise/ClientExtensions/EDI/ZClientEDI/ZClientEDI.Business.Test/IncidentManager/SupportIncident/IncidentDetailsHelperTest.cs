using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentDetailsHelperTest : TestCaseWithFactory
	{
		public void TestGetRecalculatedProductArea()
		{
			#region Test Data

			CodeDescriptionPairList eDIAreaList = new CodeDescriptionPairList();
			eDIAreaList.AddPair("CFA", "Category Fruits Area A");
			eDIAreaList.AddPair("CFB", "Category Fruits Area B");
			eDIAreaList.AddPair("8FA", "CR8 Fruits Area A");
			eDIAreaList.AddPair("8FB", "CR8 Fruits Area B");
			eDIAreaList.AddPair("9FA", "CR9 Fruits Area A");
			eDIAreaList.AddPair("9FB", "CR9 Fruits Area B");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, eDIAreaList);

			SystemProductCollection collection = new SystemProductCollection();
			var prod = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", true);
			prod.ModuleMappings.AddNew("APP", "Apples", "CFA", false);
			prod.ModuleMappings.AddNew("TOM", "Tomatoes", "CFB", false);

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			prod = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", true);
			prod.ModuleMappings.AddNew("APP", "Apples", "8FA", false);
			prod.ModuleMappings.AddNew("BAN", "Bannanas", "8FB", false);

			var nonEDIproduct = collection.AddNew();
			nonEDIproduct.Code = "VEG";
			nonEDIproduct.Description = (NoResString)"Vegetables";
			var nonEDImodule = nonEDIproduct.ModuleMappings.AddNew();
			nonEDImodule.ModuleCode = "TOM";
			nonEDImodule.ModuleDescription = (NoResString)"Tomatoes";
			nonEDImodule.ProductArea = "CFA";

			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			prod = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", true);
			prod.ModuleMappings.AddNew("APP", "Apples", "9FA", false);
			prod.ModuleMappings.AddNew("PEA", "Pears", "9FB", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Factory.Save();

			LegacyModuleMappingCollection legacyMenuSectionMappings = new LegacyModuleMappingCollection(ModuleListType.MenuSection);
			legacyMenuSectionMappings.AddNew("LAP", "Legacy Apples", "", "APP");
			EDIDataRegistry.Instance.LegacyMenuSectionMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, legacyMenuSectionMappings);

			LegacyModuleMappingCollection legacycr8ModuleMappings = new LegacyModuleMappingCollection(ModuleListType.Cr8);
			legacycr8ModuleMappings.AddNew("LBA", "Legacy Bannanas", "", "BAN");
			EDIDataRegistry.Instance.LegacyCr8ModuleMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, legacycr8ModuleMappings);

			LegacyModuleMappingCollection legacycr9ModuleMappings = new LegacyModuleMappingCollection(ModuleListType.Cr9);
			legacycr9ModuleMappings.AddNew("LPE", "Legacy Pears", "", "PEA");
			EDIDataRegistry.Instance.LegacyCr9ModuleMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, legacycr9ModuleMappings);

			#endregion

			var workTask = Factory.New<SupportIncident>();
			workTask.IM_Product = "ENT";
			workTask.IM_Priority = "";
			workTask.IM_Module = "APP";
			AssertEquals("CFA", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "";
			workTask.IM_Module = "BAN";
			AssertEquals("8FB", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "";
			workTask.IM_Module = "PEA";
			AssertEquals("9FB", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "CR4";
			workTask.IM_Module = "APP";
			AssertEquals("CFA", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "CR8";
			workTask.IM_Module = "APP";
			AssertEquals("8FA", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "CR9";
			workTask.IM_Module = "APP";
			AssertEquals("9FA", workTask.GetRecalculatedProductArea());

			#region Test Product Area for Legacy

			workTask.IM_Priority = "CR3";
			workTask.IM_Module = "LAP";
			AssertEquals("CFA", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "CR8";
			workTask.IM_Module = "LBA";
			AssertEquals("8FB", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "CR9";
			workTask.IM_Module = "LPE";
			AssertEquals("9FB", workTask.GetRecalculatedProductArea());

			#endregion

			workTask.IM_Product = "VEG";
			workTask.IM_Module = "TOM";
			AssertEquals("", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "CR8";
			AssertEquals("CFA", workTask.GetRecalculatedProductArea());
		}
	}
}
