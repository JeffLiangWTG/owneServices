using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(BillingDbUsageCodesCollection))]
	internal sealed class BillingDbUsageCodesCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<BillingDbUsageCodesCollection>
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

		protected override BillingDbUsageCodesCollection GetCollectionToTest()
		{
			return new BillingDbUsageCodesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BillingDbUsageCodes();
		}

		#endregion
	}
}
