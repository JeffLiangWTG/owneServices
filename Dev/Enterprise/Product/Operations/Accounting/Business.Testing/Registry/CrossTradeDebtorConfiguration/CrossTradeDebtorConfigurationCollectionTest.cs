using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CrossTradeDebtorConfigurationCollection))]
	public class CrossTradeDebtorConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CrossTradeDebtorConfigurationCollection>
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

		protected override CrossTradeDebtorConfigurationCollection GetCollectionToTest()
		{
			return new CrossTradeDebtorConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CrossTradeDebtorConfiguration();
		}

		#endregion
	}
}

