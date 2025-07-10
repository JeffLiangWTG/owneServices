using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestsSubclassesOf(typeof(RegistryBusinessObjectCollectionTemplate), typeof(TestExcludeBusinessObjectsAllHaveTestCasesAttribute))]
	public abstract class RegistryBusinessObjectCollectionTemplateTestCase<T> : NonPersistentBusinessObjectCollectionTestCase<T> where T : RegistryBusinessObjectCollectionTemplate
	{
		public void TestFactoryIsNull()
		{
			AssertNull("Factory should be null.", Collection.Factory);
		}

		public void TestFactoryForNewElements()
		{
			if (RequiresFactory && SupportsAddNew)
			{
				CurrentFactoryPropertyInfo(typeof(T)).SetValue(Collection, Factory, null);
			}

			RegistryBusinessObjectTemplate element;

			if (SupportsAddNew)
			{
				element = Collection.AddNew();
			}
			else
			{
				element = (RegistryBusinessObjectTemplate)GetNewElementToAddToTheCollection();
				Collection.Add(element);
			}

			AssertNull("BusinessObject created by AddNew() should have a null Factory.", element.Factory);
			AssertEquals("BusinessObject created by AddNew() should have the same CurrentFactory as the Collection.",
				CurrentFactoryPropertyInfo(typeof(T)).GetValue(Collection, null),
				CurrentFactoryPropertyInfo(typeof(RegistryBusinessObjectTemplate)).GetValue(element, null));
		}

		public void TestFallbackLevelForNewElements()
		{
			var expectedFallbackLevel = (RequiresFallbackLevel && SupportsAddNew) ? NewFallbackLevel() : null;
			Collection.CurrentFallbackLevel = expectedFallbackLevel;

			RegistryBusinessObjectTemplate element;

			if (SupportsAddNew)
			{
				element = Collection.AddNew();
			}
			else
			{
				element = (RegistryBusinessObjectTemplate)GetNewElementToAddToTheCollection();
				Collection.Add(element);
			}

			AssertEquals("BusinessObject created by AddNew() should have the same CurrentFallbackLevel as the Collection.", expectedFallbackLevel, element.CurrentFallbackLevel);
		}

		public void TestClone()
		{
			var currentFallbackLevel = NewFallbackLevel();
			RegistryBusinessObjectTemplate element1;
			RegistryBusinessObjectTemplate element2;
			if (SupportsAddNew)
			{
				element1 = Collection.AddNew();
				element2 = Collection.AddNew();
			}
			else
			{
				element1 = (RegistryBusinessObjectTemplate)GetNewElementToAddToTheCollection();
				Collection.Add(element1);

				element2 = (RegistryBusinessObjectTemplate)GetNewElementToAddToTheCollection();
				Collection.Add(element2);
			}

			var clone = Collection.Clone(currentFallbackLevel, Factory);
			var expectedFactory = (RequiresFactory) ? Factory : RegistryFactory.Instance;

			Assert("Clone should be a different instance.", Collection != clone);
			Assert("Clone's elements should be different instances from the original's.", clone[0] != element1);
			Assert("Clone's elements should be different instances from the original's.", clone[1] != element2);
			AssertEquals("Clone.CurrentFactory", expectedFactory, CurrentFactoryPropertyInfo(typeof(T)).GetValue(clone, null));
		}

		#region Implementation

		protected PropertyInfo CurrentFactoryPropertyInfo(Type objectType)
		{
			return objectType.GetProperty("CurrentFactory", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		protected abstract bool RequiresFactory
		{
			get;
		}
		protected abstract bool RequiresFallbackLevel
		{
			get;
		}

		protected virtual bool SupportsAddNew
		{
			get { return true; }
		}

		protected virtual FallbackLevel NewFallbackLevel()
		{
			return new FallbackLevel(Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);
		}

		#endregion
	}
}
