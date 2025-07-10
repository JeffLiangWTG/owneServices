using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SubscriptionRuleCollection))]
	sealed class SubscriptionRuleCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<SubscriptionRuleCollection>
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

		protected override SubscriptionRuleCollection GetCollectionToTest()
		{
			return new SubscriptionRuleCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SubscriptionRule();
		}

		#endregion
	}
}
