using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ProductAreaModuleMapping))]
	internal sealed class ProductAreaModuleMappingTest : RegistryBusinessObjectTemplateTestCase<ProductAreaModuleMapping>
	{
		public void TestGetClone()
		{
			ProductAreaModuleMapping mapping = NewPopulatedBusinessObject();
			mapping.IsModuleReadOnly = true;

			ProductAreaModuleMapping clone = (ProductAreaModuleMapping)mapping.Clone(mapping.CurrentFallbackLevel, mapping.Factory);
			AssertEquals(true, clone.IsModuleReadOnly);
		}

		#region Properties

		public void TestGetProductAreaWithSourceModule()
		{
			ProductAreaModuleMappingCollection collection = new ProductAreaModuleMappingCollection();
			ProductAreaModuleMapping mapping1 = collection.AddNew("AAA", "Module AAA", "ARC", true);
			mapping1.SourceModuleMappings.AddNew("SuperModule", "GEO");
			ProductAreaModuleMapping mapping2 = collection.AddNew("BBB", "Module BBB", "CUS", false);

			AssertEquals("ARC", mapping1.GetProductAreaWithSourceModule(""));
			AssertEquals("GEO", mapping1.GetProductAreaWithSourceModule("SuperModule"));
			AssertEquals("ARC", mapping1.GetProductAreaWithSourceModule("AnotherModule"));
			AssertEquals("CUS", mapping2.GetProductAreaWithSourceModule(""));
			AssertEquals("CUS", mapping2.GetProductAreaWithSourceModule("SuperModule"));
		}

		#endregion

		#region Validation

		public void TestValidateModuleCode()
		{
			ProductAreaModuleMappingCollection collection = new ProductAreaModuleMappingCollection();

			var mapping1 = collection.AddNew();
			mapping1.ModuleCode = "";
			mapping1.ValidateModuleCode();
			AssertHasErrorContaining(mapping1.ModuleCodeInfo, MandatoryValidation.MustBeEntered);

			var mapping2 = collection.AddNew();
			mapping2.ModuleCode = "XXX";
			mapping2.ValidateModuleCode();
			AssertNoErrors(mapping2.ModuleCodeInfo);

			var mapping3 = collection.AddNew();
			mapping3.ModuleCode = "XXX";
			mapping3.ValidateModuleCode();
			AssertHasErrorContaining(mapping3.ModuleCodeInfo, "unique");
		}

		public void TestValidateModuleDescription()
		{
			ProductAreaModuleMapping mapping = NewPopulatedBusinessObject();
			mapping.ValidateModuleDescription();
			AssertHasErrors(mapping.ModuleDescriptionInfo);

			mapping.ModuleDescription = "Test Module";
			mapping.ValidateModuleDescription();
			AssertNoErrors(mapping.ModuleDescriptionInfo);
		}

		public void TestValidateProductArea()
		{
			ProductAreaModuleMapping mapping = NewPopulatedBusinessObject();
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
			var mapping = new ProductAreaModuleMapping();

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

		protected override ProductAreaModuleMapping GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override ProductAreaModuleMapping GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		ProductAreaModuleMapping NewPopulatedBusinessObject()
		{
			ProductAreaModuleMapping result = new ProductAreaModuleMapping(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			return result;
		}

		#endregion
	}
}
