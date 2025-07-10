using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceDateConfigurationCollection))]
	public class InvoiceDateConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<InvoiceDateConfigurationCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override InvoiceDateConfigurationCollection GetCollectionToTest()
		{
			return new InvoiceDateConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new InvoiceDateConfiguration();
		}

		#endregion
	}
}
