using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithMandatoryDescriptionCollection))]
	sealed class CodeDescriptionWithMandatoryDescriptionCollectionTest : RegistryBusinessObjectCollectionTestCase<CodeDescriptionWithMandatoryDescriptionCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		#region Implementation

		protected override CodeDescriptionWithMandatoryDescriptionCollection GetCollectionToTest()
		{
			return new CodeDescriptionWithMandatoryDescriptionCollection();
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CodeDescriptionWithMandatoryDescription();
		}

		#endregion
	}
}
