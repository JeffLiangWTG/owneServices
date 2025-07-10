using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(SystemProduct))]
	internal sealed class SystemProductTest : RegistryBusinessObjectTemplateTestCase<SystemProduct>
	{
		public void TestCodeExisting()
		{
			var collection = new SystemProductCollection();
			collection.AddNew("P1", "Desc 1", true);
			collection.AddNew("P2", "Desc 2", true);

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var newProduct = NewPopulatedBusinessObject();
			AssertEquals("", newProduct.Code);
			AssertEquals("", newProduct.Description);
			AssertNoErrors(newProduct.CodeExistingInfo);

			newProduct.CodeExisting = "P1";
			AssertEquals("P1", newProduct.Code);
			AssertEquals("Desc 1", newProduct.Description);
			AssertNoErrors(newProduct.CodeExistingInfo);

			newProduct.CodeExisting = "P2";
			AssertEquals("P2", newProduct.Code);
			AssertEquals("Desc 2", newProduct.Description);
			AssertNoErrors(newProduct.CodeExistingInfo);

			newProduct.CodeExisting = "P3";
			AssertEquals("P2", newProduct.Code);
			AssertEquals("Desc 2", newProduct.Description);
			AssertHasErrors(newProduct.CodeExistingInfo);
		}

		public void TestGetClone()
		{
			var product = NewPopulatedBusinessObject();
			product.IsProductReadOnly = true;

			var clone = (SystemProduct)product.Clone(product.CurrentFallbackLevel, product.Factory);
			AssertEquals(true, clone.IsProductReadOnly);
		}

		public void TestCollectionProductCode()
		{
			var collection = new SystemProductCollection();
			var product = collection.AddNew();
			var productCode = product.ServiceTypeModuleMappings.ProductCode;
			AssertEquals(string.Empty, productCode);

			product.Code = "NAN";
			productCode = product.ServiceTypeModuleMappings.ProductCode;
			AssertEquals("NAN", productCode);
		}

		#region Validation

		public void TestValidateModuleCode()
		{
			var collection = new SystemProductCollection();

			var mapping1 = collection.AddNew();
			mapping1.Code = "";
			mapping1.ValidateCode();
			AssertHasErrorContaining(mapping1.CodeInfo, MandatoryValidation.MustBeEntered);

			var mapping2 = collection.AddNew();
			mapping2.Code = "XXX";
			mapping2.ValidateCode();
			AssertNoErrors(mapping2.CodeInfo);

			var mapping3 = collection.AddNew();
			mapping3.Code = "XXX";
			mapping3.ValidateCode();
			AssertHasErrorContaining(mapping3.CodeInfo, "unique");
		}

		public void TestValidateModuleDescription()
		{
			var mapping = NewPopulatedBusinessObject();
			mapping.ValidateDescription();
			AssertHasErrors(mapping.DescriptionInfo);

			mapping.Description = "Test Module";
			mapping.ValidateDescription();
			AssertNoErrors(mapping.DescriptionInfo);
		}

		#endregion

		#region ICanDelete

		public void TestCanDelete()
		{
			var mapping = NewPopulatedBusinessObject();

			mapping.IsProductReadOnly = false;
			AssertEquals(true, mapping.CanDelete);

			mapping.IsProductReadOnly = true;
			AssertEquals(false, mapping.CanDelete);
			AssertEquals("Product is hard-coded and can not be removed via registry.", mapping.ReasonForNotAbleToDelete);
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

		protected override SystemProduct GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override SystemProduct GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		SystemProduct NewPopulatedBusinessObject()
		{
			return new SystemProduct(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var collection = new SystemProductCollection();
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		#endregion
	}
}
