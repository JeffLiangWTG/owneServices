using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ServiceLevelRegistryBusinessObjectCollection))]
	public class ServiceLevelRegistryBusinessObjectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ServiceLevelRegistryBusinessObjectCollection>
	{
		public void TestToIListZString()
		{
			ServiceLevelRegistryBusinessObjectCollection actualCollection = new ServiceLevelRegistryBusinessObjectCollection();
			UniqueList<ZString> expectedList = new UniqueList<ZString>(2);

			ServiceLevelRegistryBusinessObject obj1 = (ServiceLevelRegistryBusinessObject)GetNewElementToAddToTheCollection();
			AddToExpectedAndActualCollections(obj1, expectedList, actualCollection);

			ServiceLevelRegistryBusinessObject obj2 = (ServiceLevelRegistryBusinessObject)GetNewElementToAddToTheCollection();
			AddToExpectedAndActualCollections(obj1, expectedList, actualCollection);

			CombineAssertions(delegate
			{ AssertCollectionsEqual("Collections {0} equals", expectedList, actualCollection); });
		}

		void AddToExpectedAndActualCollections(ServiceLevelRegistryBusinessObject obj1, UniqueList<ZString> expectedList, ServiceLevelRegistryBusinessObjectCollection actualCollection)
		{
			expectedList.Add(obj1.ServiceLevel);
			actualCollection.Add(obj1);
		}

		void AssertCollectionsEqual(ZString messageFormat, UniqueList<ZString> expectedList, ServiceLevelRegistryBusinessObjectCollection actualCollection)
		{
			AssertEquals(ZString.Format(messageFormat, "count"), expectedList.Count, actualCollection.Count);

			for (int idx = 0; idx < expectedList.Count; idx++)
			{
				AssertEquals(ZString.Format(messageFormat, "element"), expectedList[idx], actualCollection[idx].ServiceLevel);
			}
		}

		protected override bool SupportsAddNew => false;

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ServiceLevelRegistryBusinessObjectCollection GetCollectionToTest()
		{
			return new ServiceLevelRegistryBusinessObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ServiceLevelRegistryBusinessObject result = new ServiceLevelRegistryBusinessObject();
			result.ServiceLevel = ((ZString)ZGuid.NewZGuid().ToString()).Left(3);
			return result;
		}

		public void TestIsDuplicateRegistryItem()
		{
			ZGuid sendingAgentPK = ZGuid.NewZGuid();

			ServiceLevelRegistryBusinessObjectCollection setupCollection = new ServiceLevelRegistryBusinessObjectCollection();
			ServiceLevelRegistryBusinessObject registrySetup = new ServiceLevelRegistryBusinessObject();
			registrySetup.ServiceLevel = "OBC";
			AssertEquals("Duplicate item not expected", false, setupCollection.IsDuplicateItem(registrySetup));

			setupCollection.Add(registrySetup);
			ServiceLevelRegistryBusinessObject duplicateRegistry = new ServiceLevelRegistryBusinessObject();
			duplicateRegistry.ServiceLevel = "OBC";
			AssertEquals("Duplicate item error expected", true, setupCollection.IsDuplicateItem(duplicateRegistry));

			ServiceLevelRegistryBusinessObject newRegistry = new ServiceLevelRegistryBusinessObject();
			newRegistry.ServiceLevel = "SMP";
			AssertEquals("Duplicate item not expected", false, setupCollection.IsDuplicateItem(newRegistry));

			setupCollection.Add(newRegistry);
			ServiceLevelRegistryBusinessObject simmilarRegistry = new ServiceLevelRegistryBusinessObject();
			simmilarRegistry.ServiceLevel = "SMP";
			AssertEquals("Duplicate item error expected", true, setupCollection.IsDuplicateItem(simmilarRegistry));
		}
	}
}
