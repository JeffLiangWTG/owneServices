using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	class DataWritingManagerTest : TestCaseWithFactory
	{
		public void TestNamespace_UseRegistry_2011_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var dummyBO = Factory.New<DummyBusinessObject>();
				var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dummyBO));

				AssertEquals("writeManager.Namespace", writeManager.Schema.Namespace, UniversalXmlInfo.Namespace_2011_11);
				AssertEquals("writeManager.Version", writeManager.Schema.Version, UniversalXmlInfo.Version_2011_11);
			}
		}

		public void TestNamespace_UseRegistry_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var dummyBO = Factory.New<DummyBusinessObject>();
				var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dummyBO));

				AssertEquals("writeManager.Namespace", writeManager.Schema.Namespace, UniversalXmlInfo.Namespace_2012_11);
				AssertEquals("writeManager.Version", writeManager.Schema.Version, UniversalXmlInfo.Version_2012_11_DO_NOT_USE);
			}
		}

		public void TestNamespace_SpecifyNamespace()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var dummyBO = Factory.New<DummyBusinessObject>();
				var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dummyBO), null, UniversalXmlSchema.Version_2012_11_DO_NOT_USE);

				AssertEquals("writeManager.Namespace", writeManager.Schema.Namespace, UniversalXmlInfo.Namespace_2012_11);
				AssertEquals("writeManager.Version", writeManager.Schema.Version, UniversalXmlInfo.Version_2012_11_DO_NOT_USE);
			}
		}

		public void TestDuplicatePKChecking()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dummyBO));

			var testHelper = new TestHelper(writeManager);
			testHelper.AssertResult("Precondition", false, false, false);

			writeManager.AddPK(testHelper.pk2);
			testHelper.AssertResult("After adding PK2", false, true, false);

			writeManager.AddPK(testHelper.pk1);
			testHelper.AssertResult("Added PK1 as well", true, true, false);

			using (writeManager.UseNewListForDuplicatePKCheck())
			{
				testHelper.AssertResult("UseNewDuplicatePKCheck called", false, false, false);

				writeManager.AddPK(testHelper.pk1);
				testHelper.AssertResult("Added PK1", true, false, false);

				writeManager.AddPK(testHelper.pk3);
				testHelper.AssertResult("Added PK3", true, false, true);
			}

			testHelper.AssertResult("UseNewDuplicatePKCheck disposed", true, true, false);
		}

		public void TestPublishingInternally()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dummyBO));

			Assert(!writeManager.IsPublishingInternally);

			using (writeManager.SetIsPublishingInternally())
			{
				Assert(writeManager.IsPublishingInternally);
			}

			Assert(!writeManager.IsPublishingInternally);
		}

		class TestHelper
		{
			internal TestHelper(IDataWritingManager writeManager)
			{
				this.writeManager = writeManager;
			}
			readonly IDataWritingManager writeManager;

			internal ZGuid pk1 = ZGuid.NewZGuid();
			internal ZGuid pk2 = ZGuid.NewZGuid();
			internal ZGuid pk3 = ZGuid.NewZGuid();

			internal void AssertResult(string message, bool pk1Exported, bool pk2Exported, bool pk3Exported)
			{
				CombineAssertions(message, delegate
				{
					AssertEquals("writeManager.PKAlreadyExported(pk1)", pk1Exported, writeManager.PKAlreadyExported(pk1));
					AssertEquals("writeManager.PKAlreadyExported(pk2)", pk2Exported, writeManager.PKAlreadyExported(pk2));
					AssertEquals("writeManager.PKAlreadyExported(pk3)", pk3Exported, writeManager.PKAlreadyExported(pk3));
				});
			}
		}
	}
}
