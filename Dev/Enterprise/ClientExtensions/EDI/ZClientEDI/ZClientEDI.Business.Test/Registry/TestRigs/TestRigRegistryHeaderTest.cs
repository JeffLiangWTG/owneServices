using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(TestRigRegistryHeader))]
	public class TestRigRegistryHeaderTest : RegistryBusinessObjectTemplateTestCase<TestRigRegistryHeader>
	{
		public void TestSave_ShouldValidateCollection()
		{
			var header = NewPopulatedBusinessObject();
			var options = header.OptionsCollection.AddNew();

			AssertExceptionThrown<RegistryValidationException>(() => header.RunPreSaveValidation());
			AssertHasError(options.ProductInfo, "Please enter a Product.");
		}

		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override TestRigRegistryHeader GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override TestRigRegistryHeader GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		TestRigRegistryHeader NewPopulatedBusinessObject()
		{
			return new TestRigRegistryHeader(NewFallbackLevel(), Factory);
		}

		#endregion
	}
}
