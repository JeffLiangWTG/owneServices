using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceRollupAndGroupDescriptionCollection))]
	public class InvoiceRollupAndGroupDescriptionCollectionTest : Enterprise.Registry.Business.Testing.RegistryBusinessObjectCollectionTemplateTestCase<InvoiceRollupAndGroupDescriptionCollection>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override InvoiceRollupAndGroupDescriptionCollection GetCollectionToTest()
		{
			return new InvoiceRollupAndGroupDescriptionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new InvoiceRollupAndGroupDescription();
		}
	}
}
