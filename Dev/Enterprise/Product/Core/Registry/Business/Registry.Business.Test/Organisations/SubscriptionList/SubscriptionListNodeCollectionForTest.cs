using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SubscriptionListNodeCollection))]
	sealed class SubscriptionListNodeCollectionForTest : SubscriptionListNodeCollection
	{
		public SubscriptionListNodeCollectionForTest(bool isDescriptionRequired) : base(isDescriptionRequired)
		{
		}
		public SubscriptionProperties CreateNonPersistentBusinessObjectExposed()
		{
			return (SubscriptionProperties)CreateNonPersistentBusinessObject();
		}
	}
}
