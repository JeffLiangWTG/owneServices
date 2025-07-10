using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StatusSentimentCollection))]
	sealed class StatusSentimentCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<StatusSentimentCollection>
	{
		public void TestAllowNew()
		{
			var collection = new StatusSentimentCollection();
			Assert("Users should be able to add new items to the collection.", collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = new StatusSentimentCollection();
			Assert("Users should be able to remove items from the collection.", collection.AllowRemove);
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override StatusSentimentCollection GetCollectionToTest()
		{
			return new StatusSentimentCollection(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StatusSentiment();
		}

		#endregion
	}
}
