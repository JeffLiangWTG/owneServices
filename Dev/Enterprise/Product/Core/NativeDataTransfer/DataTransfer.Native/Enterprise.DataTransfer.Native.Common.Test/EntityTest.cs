using System;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.Exceptions;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common
{
	public class EntityTest : TransactionedTestCase
	{
		public void TestInternalPKSync()
		{
			var definition = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");
			var pk1 = Guid.NewGuid();
			var pk2 = Guid.NewGuid();
			var pk3 = Guid.NewGuid();
			var pk4 = Guid.NewGuid();

			var entity1 = new Entity(definition, sessionServices);
			var entity2 = new Entity(definition, sessionServices);
			var entity3 = new Entity(definition, sessionServices);
			var entity4 = new Entity(definition, sessionServices);

			entity1.InternalPK = pk1;
			AssertEquals("Assignment works", pk1, entity1.InternalPK);
			AssertEquals("Empty guids do not change", Guid.Empty, entity2.InternalPK);

			entity2.InternalPK = pk2;
			entity3.InternalPK = pk3;
			entity4.InternalPK = pk4;

			entity1.InternalPK = pk2;
			entity1.InternalPK = pk3;
			AssertEquals("entity2.InternalPK changed automatically", pk3, entity2.InternalPK);
			AssertEquals("entity4.InternalPK didn't change", pk4, entity4.InternalPK);

			entity3.InternalPK = pk4;
			CombineAssertions("All internal PKs are the same now", () =>
			{
				AssertEquals(pk4, entity1.InternalPK);
				AssertEquals(pk4, entity2.InternalPK);
				AssertEquals(pk4, entity3.InternalPK);
				AssertEquals(pk4, entity4.InternalPK);
			});

			entity2.InternalPK = Guid.Empty;
			CombineAssertions("Clearing one internal PK didn't affect others", () =>
			{
				AssertEquals(pk4, entity1.InternalPK);
				AssertEquals(Guid.Empty, entity2.InternalPK);
				AssertEquals(pk4, entity3.InternalPK);
				AssertEquals(pk4, entity4.InternalPK);
			});

			entity2.InternalPK = pk1;
			CombineAssertions("After clearing internal PK connection to other entities is lost", () =>
			{
				AssertEquals(pk4, entity1.InternalPK);
				AssertEquals(pk1, entity2.InternalPK);
				AssertEquals(pk4, entity3.InternalPK);
				AssertEquals(pk4, entity4.InternalPK);
			});

			entity4.InternalPK = pk3;
			CombineAssertions("Other entities are still synchronized", () =>
			{
				AssertEquals(pk3, entity1.InternalPK);
				AssertEquals(pk1, entity2.InternalPK);
				AssertEquals(pk3, entity3.InternalPK);
				AssertEquals(pk3, entity4.InternalPK);
			});
		}

		public void TestIndexing_PropertyNameExisted()
		{
			AssertEquals("No Property existed before first set to entity index", false, entity.HasProperty("Code"));
			entity["Code"] = null;
			AssertEquals("Property was created", true, entity.HasProperty("Code"));
		}

		public void TestIndexing_PropertyNameNotExisted()
		{
			AssertEquals("No Property existed before first set to entity index", false, entity.HasProperty("Dummy"));
			AssertExceptionThrown("Property Not Define Exception should be thrown",
				typeof(PropertyNotDefinedException),
				delegate
				{
					entity["Dummy"] = null;
				});
		}

		public void TestEquality()
		{
			var definition = TestUtil.FindEntityDefinition("Organization", "OrgHeader");
			var first = new Entity(definition, sessionServices)
			{
				InternalPK = Guid.NewGuid()
			};
			var second = new Entity(definition, sessionServices)
			{
				InternalPK = Guid.NewGuid()
			};
			AssertNotEquals("Entities should be different if internalPKs are not empty and are different", first, second);

			var id = Guid.NewGuid();
			first.InternalPK = id;
			second.InternalPK = id;
			AssertEquals("Entities should be the same if internalPKs are the same", first, second);

			first.InternalPK = Guid.Empty;
			second.InternalPK = Guid.Empty;
			first["Code"] = "Test";
			second["Code"] = "Test";
			AssertEquals("Entities should be the same if internalPKs are empty but all properties are the same", first, second);
		}

		#region Implementation

		IEntityDefinition definition;
		Entity entity;

		protected override void SetUp()
		{
			base.SetUp();
			TestUtil.AlterDummyTable();
			sessionServices = new AncillaryImportServices();
			definition = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");
			entity = new Entity(definition, sessionServices);
		}
		AncillaryImportServices sessionServices;

		#endregion
	}
}
