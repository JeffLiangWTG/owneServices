using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Registry.Testing
{
	[TestedType(typeof(SendAcknowledgementsRegistryCollection))]
	class SendAcknowledgementsRegistryCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<SendAcknowledgementsRegistryCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override SendAcknowledgementsRegistryCollection GetCollectionToTest() => new SendAcknowledgementsRegistryCollection(CurrentFallbackLevel, Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new SendAcknowledgementsRegistry();

		FallbackLevel CurrentFallbackLevel
		{
			get { return currentFallbackLevel ?? (currentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); }
		}
		FallbackLevel currentFallbackLevel;
	}

	[TestedType(typeof(SendAcknowledgementsRegistryCollection))]
	class SendAcknowledgementsNonPersistentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SendAcknowledgementsRegistryCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(true, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(true, collection.AllowRemove);
		}

		protected override SendAcknowledgementsRegistryCollection GetCollectionToTest() => new SendAcknowledgementsRegistryCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new SendAcknowledgementsRegistry();

		protected override void SetUp()
		{
			base.SetUp();
			collection = new SendAcknowledgementsRegistryCollection();
		}
		SendAcknowledgementsRegistryCollection collection;
	}
}
