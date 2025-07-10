using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.eHub.Testing
{
	[TestedType(typeof(BillingTransactionsTransformationSettings))]
	sealed class BillingTransactionsTransformationSettingsTest : RegistryBusinessObjectTemplateTestCase<BillingTransactionsTransformationSettings>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BillingTransactionsTransformationSettings GetBusinessObjectToClone()
		{
			return (BillingTransactionsTransformationSettings)GetNewBusinessObject();
		}

		protected override BillingTransactionsTransformationSettings GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BillingTransactionsTransformationSettings();
		}
	}
}
