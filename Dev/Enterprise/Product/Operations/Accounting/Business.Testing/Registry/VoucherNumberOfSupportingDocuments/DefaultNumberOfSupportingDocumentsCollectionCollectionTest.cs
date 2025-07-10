using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(DefaultNumberOfSupportingDocumentsCollection))]
	public class DefaultNumberOfSupportingDocumentsCollectionCollectionTest : Enterprise.Registry.Business.Testing.RegistryBusinessObjectCollectionTemplateTestCase<DefaultNumberOfSupportingDocumentsCollection>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override DefaultNumberOfSupportingDocumentsCollection GetCollectionToTest()
		{
			return new DefaultNumberOfSupportingDocumentsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DefaultNumberOfSupportingDocuments();
		}
	}
}
