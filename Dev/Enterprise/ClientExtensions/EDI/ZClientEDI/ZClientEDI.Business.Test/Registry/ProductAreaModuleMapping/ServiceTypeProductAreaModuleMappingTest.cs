using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ServiceTypeProductAreaModuleMapping))]
	internal sealed class ServiceTypeProductAreaModuleMappingTest : RegistryBusinessObjectTemplateTestCase<ServiceTypeProductAreaModuleMapping>
	{
		public void TestGetClone()
		{
			var mapping = NewPopulatedBusinessObject();
			mapping.IsModuleReadOnly = true;

			var clone = (ServiceTypeProductAreaModuleMapping)mapping.Clone(mapping.CurrentFallbackLevel, mapping.Factory);
			AssertEquals(true, clone.IsModuleReadOnly);
		}

		public void TestModuleCodeUpdateInternalEnabled()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("XRM", "XRM");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			// setup for ServiceTypeProductAreaModuleMappingLookups.Modules
			var collection1 = new SystemProductCollection();
			var product1 = collection1.AddNew(ProductTypes.Codes.Enterprise, "Enterprise", true);
			product1.ModuleMappings.AddNew("SAA", "SAA Description", "XRM", true);
			product1.ModuleMappings.AddNew("TST", "TST Description", "XRM", true, true, false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);

			var collection = new ServiceTypeProductAreaModuleMappingCollection();

			var mapping = collection.AddNew();
			mapping.ProductCode = ProductTypes.Codes.Enterprise;
			mapping.ProductArea = "XRM";

			mapping.ModuleCode = "SAA";
			AssertEquals(mapping.IsInternal, false);
			AssertEquals(mapping.IsEnabled, true);

			mapping.ModuleCode = "TST";
			AssertEquals(mapping.IsInternal, true);
			AssertEquals(mapping.IsEnabled, false);
		}

		public void TestModuleListByCriticality_Cr8()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("XRM", "XRM");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			// setup for ServiceTypeProductAreaModuleMappingLookups.Modules
			var collection1 = new SystemProductCollection();
			var product1 = collection1.AddNew(ProductTypes.Codes.Enterprise, "Enterprise", true);
			product1.ModuleMappings.AddNew("SAA", "SAA Description", "XRM", true);
			product1.ModuleMappings.AddNew("TST", "TST Description", "XRM", true, true, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);

			var collection = new ServiceTypeProductAreaModuleMappingCollection();

			var mapping = collection.AddNew();
			mapping.ProductCode = ProductTypes.Codes.Enterprise;
			mapping.ProductCriticality = ModuleListType.Cr8;
			mapping.ProductArea = "XRM";

			mapping.ModuleCode = "SAA";
			AssertEquals(mapping.IsInternal, false);
			AssertEquals(mapping.IsEnabled, true);

			mapping.ModuleCode = "TST";
			AssertEquals(mapping.IsInternal, true);
			AssertEquals(mapping.IsEnabled, false);
		}

		public void TestModuleListByCriticality_Cr9()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("XRM", "XRM");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			// setup for ServiceTypeProductAreaModuleMappingLookups.Modules
			var collection1 = new SystemProductCollection();
			var product1 = collection1.AddNew(ProductTypes.Codes.Enterprise, "Enterprise", true);
			product1.ModuleMappings.AddNew("SAA", "SAA Description", "XRM", true);
			product1.ModuleMappings.AddNew("TST", "TST Description", "XRM", true, true, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);

			var collection = new ServiceTypeProductAreaModuleMappingCollection();

			var mapping = collection.AddNew();
			mapping.ProductCode = ProductTypes.Codes.Enterprise;
			mapping.ProductCriticality = ModuleListType.Cr9;
			mapping.ProductArea = "XRM";

			mapping.ModuleCode = "SAA";
			AssertEquals(mapping.IsInternal, false);
			AssertEquals(mapping.IsEnabled, true);

			mapping.ModuleCode = "TST";
			AssertEquals(mapping.IsInternal, true);
			AssertEquals(mapping.IsEnabled, false);
		}

		#region Validation

		public void TestValidateModuleCode()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("XRM", "XRM");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			// setup for ServiceTypeProductAreaModuleMappingLookups.Modules
			var collection1 = new SystemProductCollection();
			var product1 = collection1.AddNew(ProductTypes.Codes.Enterprise, "Enterprise", true);
			product1.ModuleMappings.AddNew("SAA", "XXX Description", "XRM", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);

			var collection = new ServiceTypeProductAreaModuleMappingCollection();

			var mapping1 = collection.AddNew();
			mapping1.ModuleCode = "";
			mapping1.ValidateModuleCode();
			AssertHasErrorContaining(mapping1.ModuleCodeInfo, MandatoryValidation.MustBeEntered);

			var mapping2 = collection.AddNew();
			mapping2.ProductArea = "XRM";
			mapping2.ModuleCode = "XXX";
			mapping2.ValidateModuleCode();
			AssertHasError(mapping2.ModuleCodeInfo, "Enter a valid selection.");

			var mapping3 = collection.AddNew();
			mapping3.ProductCode = ProductTypes.Codes.Enterprise;
			mapping3.ProductArea = "XRM";
			mapping3.ModuleCode = "SAA";
			mapping3.ValidateModuleCode();
			AssertNoErrors(mapping3.ModuleCodeInfo);
		}

		public void TestValidateProductArea()
		{
			var mapping = NewPopulatedBusinessObject();
			mapping.ValidateProductArea();
			AssertNoErrors(mapping.ProductAreaInfo);

			mapping.ProductArea = "AA";
			mapping.ValidateProductArea();
			AssertHasErrors(mapping.ProductAreaInfo);

			mapping.ProductArea = "ARC";
			mapping.ValidateProductArea();
			AssertNoErrors(mapping.ProductAreaInfo);
		}

		#endregion

		#region ICanDelete

		public void TestCanDelete()
		{
			var mapping = new ServiceTypeProductAreaModuleMapping();

			mapping.IsModuleReadOnly = false;
			AssertEquals(true, mapping.CanDelete);

			mapping.IsModuleReadOnly = true;
			AssertEquals(false, mapping.CanDelete);
			AssertEquals("Module is hard-coded and can not be removed via registry.", mapping.ReasonForNotAbleToDelete);
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ServiceTypeProductAreaModuleMapping GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override ServiceTypeProductAreaModuleMapping GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		ServiceTypeProductAreaModuleMapping NewPopulatedBusinessObject()
		{
			ServiceTypeProductAreaModuleMapping result = new ServiceTypeProductAreaModuleMapping(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			return result;
		}

		#endregion
	}
}
