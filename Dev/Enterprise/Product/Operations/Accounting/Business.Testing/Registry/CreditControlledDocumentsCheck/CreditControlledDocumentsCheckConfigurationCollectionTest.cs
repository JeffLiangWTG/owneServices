using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CreditControlledDocumentsCheckConfigurationCollection))]
	public class CreditControlledDocumentsCheckConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CreditControlledDocumentsCheckConfigurationCollection>
	{
		#region Implementation

		protected override CreditControlledDocumentsCheckConfigurationCollection GetCollectionToTest()
		{
			return new CreditControlledDocumentsCheckConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CreditControlledDocumentsCheckConfiguration();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new CreditControlledDocumentsCheckConfigurationCollection Collection
		{
			get { return base.Collection; }
		}

		#endregion
	}
}
