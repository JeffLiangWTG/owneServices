using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	sealed class RegistryBusinessObjectTemplateWithChildCollectionTest : TestCaseWithFactory
	{
		public void TestValidationWithInvalidElementOnCollection_ShouldThrowException()
		{
			var header = new DummyRegistryBusinessObjectTemplateWithChildCollection();
			var element = header.Collection.AddNew();

			AssertExceptionThrown<RegistryValidationException>(header.RunPreSaveValidation);
			AssertHasError(element.CodeInfo, "Please enter a Code.");

			element.Code = "ABC";

			header.RunPreSaveValidation();
			AssertNoErrors(element.CodeInfo);
		}
	}
}
