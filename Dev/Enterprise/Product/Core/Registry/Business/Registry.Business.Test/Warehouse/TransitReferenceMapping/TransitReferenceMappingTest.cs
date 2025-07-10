using CargoWise.Definitions;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TransitReferenceMapping))]
	class TransitReferenceMappingTest : RegistryBusinessObjectTemplateTestCase<TransitReferenceMapping>
	{
		public void TestProperties()
		{
			var mapping = new TransitReferenceMapping();
			mapping.SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;
			mapping.SourceType = WarehouseAdditionalReferenceTypes.Codes.Other;
			mapping.TargetCategory = TransitWarehouseReferenceCategories.Codes.CustomsReference;
			mapping.TargetType = TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber;

			AssertEquals(TransitWarehouseReferenceCategories.Codes.AdditionalReference, mapping.SourceCategory);
			AssertEquals(WarehouseAdditionalReferenceTypes.Codes.Other, mapping.SourceType);
			AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, mapping.TargetCategory);
			AssertEquals(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, mapping.TargetType);
		}

		#region Implementation

		protected override TransitReferenceMapping GetBusinessObjectToClone()
		{
			var result = new TransitReferenceMapping();
			return result;
		}

		public override void TestBizObjectFields()
		{
			Assert(true);
		}

		protected override TransitReferenceMapping GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
