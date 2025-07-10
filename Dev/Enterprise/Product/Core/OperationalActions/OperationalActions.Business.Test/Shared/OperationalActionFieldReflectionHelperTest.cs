using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionFieldReflectionHelperTest : TestCaseWithFactory
	{
		public void TestFieldTextToPathToFieldText()
		{
			const string FieldText = "FollowChildren.Parent+FollowChildren.Z0_AnotherDate";
			AssertPath(FieldText, "FollowChildren", "Parent", "FollowChildren", "Z0_AnotherDate");
		}

		public void TestIsUnsafePropertyName()
		{
			CombineAssertions(delegate
			{
				string[] unsafeNames = new string[] { "Z0_SystemCreateTimeUtc", "Z0_SystemCreateUser", "Z0_SystemLastEditTimeUtc", "Z0_SystemLastEditUser", "Z0_ParentGUID", "Z0_ParentID", "Z0_ParentCode", "Z0_ForeignKey", "Z0_UniqueReference", "Z0_TableCode", };
				string[] safeNames = new string[] { "Z0_Guid", "Z0_ReferenceNumber", };
				foreach (string name in unsafeNames)
				{
					AssertEquals(name, true, ReflectionHelper.IsUnsafePropertyName(name));
				}

				foreach (string name in safeNames)
				{
					AssertEquals(name, false, ReflectionHelper.IsUnsafePropertyName(name));
				}
			});
		}

		public void TestClassify()
		{
			OperationalActionTestFieldSupporterListTest.IDummyChildCollectionForAction collection = new OperationalActionTestFieldSupporterListTest.DummyChildCollectionForAction(Factory);
			OperationalActionTestFieldSupporterListTest.IDummyForAction dummy = Factory.GetNull<OperationalActionTestFieldSupporterListTest.DummyForAction>();
			Type parentType = typeof(OperationalActionTestFieldSupporterListTest.DummyForAction);
			Type childType = typeof(OperationalActionTestFieldSupporterListTest.DummyChildForAction);
			using (ObjectFactory.Substitute("IDummyChildCollectionForAction", collection))
			using (ObjectFactory.Substitute("IDummyForAction", dummy))
			{
				AssertClassification(PropertyClassification.FollowCollection, parentType, "CollectionTypeOverride");
				AssertClassification(PropertyClassification.FollowSingle, parentType, "ReturnTypeOverride");
			}

			AssertClassification(PropertyClassification.FollowCollection, parentType, "FollowChildren");
			AssertClassification(PropertyClassification.FollowSingle, childType, "Parent");
			AssertClassification(PropertyClassification.Updatable, parentType, "Z0_AnotherDate");
			AssertClassification(PropertyClassification.Blocked, parentType, "DontFollowChildren");
		}

		public void TestThatIndexerPropertyShouldBeClassifiedAsBlocked()
		{
			var activeCollectionType = typeof(ActiveBusinessObjectCollection<BusinessObject>);
			AssertClassification(PropertyClassification.Blocked, activeCollectionType, "Item");
			var businessObjectType = typeof(BusinessObject);
			AssertClassification(PropertyClassification.Blocked, businessObjectType, "Item");
		}

		public void TestThatProcessTaskCollectionIsVisibleFromOperationalAction()
		{
			PropertyInfo[] properties = typeof(ClassWithProcessTaskCollection).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			AssertClassification(PropertyClassification.FollowCollection, typeof(ClassWithProcessTaskCollection), "ProcessTasks");
		}

		public void TestThatCountryDataIsVisibleFromOperationalAction()
		{
			PropertyInfo[] properties = typeof(OrgHeader).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			AssertClassification(PropertyClassification.FollowSingle, typeof(OrgHeader), "CountryData");
		}

		#region ClassWithProcessTaskCollection
		class ClassWithProcessTaskCollection
		{
			public ProcessTaskCollection ProcessTasks
			{
				get
				{
					return processTasks ?? (processTasks = new DummyProcessTaskCollection(new BusinessObjectFactory().New<DummyBusinessObject>()));
				}
			}

			ProcessTaskCollection processTasks;
		}

		#endregion
		#region Implementation
		static void AssertPath(string fieldText, params string[] pathText)
		{
			PropertyInfo[] path = ReflectionHelper.FieldTextToPath(typeof(OperationalActionTestFieldSupporterListTest.DummyForAction), fieldText);
			AssertNotNull(fieldText, path);
			AssertEquals(pathText.Length, path.Length);
			for (int i = 0; i < pathText.Length; i++)
			{
				AssertEquals(string.Format("path[{0}]", i), pathText[i], path[i].Name);
			}

			AssertEquals(fieldText, ReflectionHelper.PathToFieldText(path));
		}

		static void AssertClassification(PropertyClassification classification, Type type, string propertyName)
		{
			PropertyInfo info = ReflectionHelper.GetPropertyInfo(type, propertyName);
			AssertEquals(classification, ReflectionHelper.Classify(info));
		}
		#endregion
	}
}
