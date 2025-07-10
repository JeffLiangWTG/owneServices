using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ServiceTypeModuleMapping))]
	internal sealed class ServiceTypeModuleMappingTest : RegistryBusinessObjectTemplateTestCase<ServiceTypeModuleMapping>
	{
		public void TestGetClone()
		{
			var mapping = NewPopulatedBusinessObject();
			mapping.Code = "SIM";

			var clone = (ServiceTypeModuleMapping)mapping.Clone(mapping.CurrentFallbackLevel, mapping.Factory);
			AssertEquals("SIM", clone.Code);
		}

		#region Properties

		public void TestDescriptionProperty()
		{
			var mapping = NewPopulatedBusinessObject();
			mapping.Code = "SIM";
			AssertEquals("Service Improvement", mapping.Description);
		}

		#endregion

		#region Validation

		public void TestValidateCode()
		{
			var collection = new ServiceTypeModuleMappingCollection();

			var mapping1 = collection.AddNew();
			mapping1.Code = "";
			mapping1.ValidateCode();
			AssertHasErrorContaining(mapping1.CodeInfo, MandatoryValidation.MustBeEntered);

			var mapping2 = collection.AddNew();
			mapping2.Code = "SIM";
			mapping2.ValidateCode();
			AssertNoErrors(mapping2.CodeInfo);

			var mapping3 = collection.AddNew();
			mapping3.Code = "SIM";
			mapping3.ValidateCode();
			AssertHasErrorContaining(mapping3.CodeInfo, "unique");
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ServiceTypeModuleMapping GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override ServiceTypeModuleMapping GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		ServiceTypeModuleMapping NewPopulatedBusinessObject()
		{
			return new ServiceTypeModuleMapping();
		}

		#endregion
	}
}
