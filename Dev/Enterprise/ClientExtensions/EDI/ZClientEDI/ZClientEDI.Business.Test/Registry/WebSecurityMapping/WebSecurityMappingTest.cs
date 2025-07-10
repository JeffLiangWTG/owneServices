using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(WebSecurityMapping))]
	internal sealed class WebSecurityMappingTest : RegistryBusinessObjectTemplateTestCase<WebSecurityMapping>
	{
		#region Validation

		public void TestValidate()
		{
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "ASD", true);
			product.ModuleMappings.AddNew("AAA", "AAA Module", "", false);
			product.ModuleMappings.AddNew("BBB", "BBB Module", "", false);
			product.ModuleMappings.AddNew("CCC", "CCC Module", "", false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			Factory.Save();

			var mappingCollection = new WebSecurityMappingCollection();
			var emptyMapping = mappingCollection.AddNew("", "", "");
			AssertHasErrorContaining(emptyMapping.WebSecurityInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(emptyMapping.ModuleMappingInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(emptyMapping.ProductMappingInfo, MandatoryValidation.MustBeEntered);

			var mappingProd = mappingCollection.AddNew("", "ENT", "");
			AssertHasErrorContaining(mappingProd.WebSecurityInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(mappingProd.ModuleMappingInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrors(mappingProd.ProductMappingInfo);

			var mappingAAA = mappingCollection.AddNew(EDIWebSecurityRightsList.WiseBusinessPartner.Code, "ENT", "AAA");
			AssertNoErrors(mappingAAA.WebSecurityInfo);
			AssertNoErrors(mappingAAA.ModuleMappingInfo);
			AssertNoErrors(mappingAAA.ProductMappingInfo);

			var mappingBBB = mappingCollection.AddNew(EDIWebSecurityRightsList.WiseBusinessPartner.Code, "ENT", "BBB");
			AssertNoErrors(mappingBBB.WebSecurityInfo);
			AssertNoErrors(mappingBBB.ModuleMappingInfo);
			AssertNoErrors(mappingBBB.ProductMappingInfo);

			var mappingAAA2 = mappingCollection.AddNew(EDIWebSecurityRightsList.WiseBusinessPartner.Code, "ENT", "AAA");
			mappingCollection.RunPreSaveValidation();
			AssertHasRowErrorContaining(mappingAAA2, "The combination of Web Security, Product and Module must be unique");
			AssertHasRowErrorContaining(mappingAAA, "The combination of Web Security, Product and Module must be unique");
			AssertNoErrors(mappingAAA2.ModuleMappingInfo);
			AssertNoErrors(mappingAAA2.ProductMappingInfo);
		}

		#endregion

		public void TestModuleMappingDescription()
		{
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "ASD", true);
			product.ModuleMappings.AddNew("AAA", "AAA Module", "", false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.Enterprise, "ASD", true);
			product.ModuleMappings.AddNew("BBB", "BBB Module", "", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.Enterprise, "ASD", true);
			product.ModuleMappings.AddNew("CCC", "CCC Module", "", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Factory.Save();

			var obj = NewPopulatedBusinessObject(false);
			obj.ModuleMapping = "AAA";
			AssertEquals("AAA Module", obj.ModuleMappingDescription);
		}

		public void TestProductMappingDescription()
		{
			var collection = new SystemProductCollection();
			collection.AddNew(ProductTypes.Codes.Enterprise, "ASD", true);
			collection.AddNew(ProductTypes.Codes.EHub, "qqq", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			collection.AddNew(ProductTypes.Codes.CargoSphere, "www", true);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			collection.AddNew(ProductTypes.Codes.Telematics, "eee", true);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Factory.Save();

			var obj = NewPopulatedBusinessObject(false);
			obj.ProductMapping = ProductTypes.Codes.CargoSphere;
			AssertEquals("www", obj.ProductMappingDescription);
		}

		public new void TestThreadSafetyOfProperties()
		{
			try
			{
				LegacyModuleMapping.DummyLookupsForTests = (t) => new LegacyModuleMappingLookupsForTest(t);
				base.TestThreadSafetyOfProperties();
				Assert("Because it complains about an empty test", true);
			}
			finally
			{
				LegacyModuleMapping.DummyLookupsForTests = null;
			}
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override WebSecurityMapping GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override WebSecurityMapping GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		WebSecurityMapping NewPopulatedBusinessObject(bool useDummyLookups = true)
		{
			var result = new WebSecurityMapping();
			result.WebSecurity = EDIWebSecurityRightsList.WiseBusinessPartner.Code;
			result.ModuleMapping = MandatoryCustomerServiceMenuSectionList.Codes.Other;
			result.ProductMapping = ProductTypes.Codes.Enterprise;
			return result;
		}

		#endregion
	}
}
