using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestsSubclassesOf(typeof(RegistryBusinessObjectTemplate))]
	public abstract class RegistryBusinessObjectTemplateTestCase<T> : NonPersistentBusinessObjectTestCase where T : RegistryBusinessObjectTemplate
	{
		public void TestHasChangesIsFalseOnInitialisation()
		{
			T bizObj = (T)Activator.CreateInstance(ExpectedBusinessObjectType);
			AssertEquals("HasChanges should be false.", false, bizObj.HasChanges);
		}

		public void TestFactoryIsNull()
		{
			T bizObj = (T)Activator.CreateInstance(ExpectedBusinessObjectType);
			AssertNull("Factory should be null.", bizObj.Factory);
		}

		public void TestCurrentFactory()
		{
			var bizObj = (T)Activator.CreateInstance(ExpectedBusinessObjectType);
			AssertEquals("CurrentFactory should be RegistryFactory.Instance.", RegistryFactory.Instance, CurrentFactoryPropertyInfo.GetValue(bizObj, null));

			if (ExpectedBusinessObjectType.GetConstructor(new Type[1] { Factory.GetType() }) != null)
			{
				bizObj = (T)Activator.CreateInstance(ExpectedBusinessObjectType, Factory);
				AssertEquals("CurrentFactory should be newly set Factory.", Factory, CurrentFactoryPropertyInfo.GetValue(bizObj, null));
			}

			if (ExpectedBusinessObjectType.GetConstructor(new Type[2] { typeof(FallbackLevel), Factory.GetType() }) != null)
			{
				bizObj = (T)Activator.CreateInstance(ExpectedBusinessObjectType, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
				AssertEquals("CurrentFactory should be newly set Factory.", Factory, CurrentFactoryPropertyInfo.GetValue(bizObj, null));
			}
		}

		[ExpectNoExceptions]
		public void TestThreadSafetyOfProperties()
		{
			T bizObj = (T)Activator.CreateInstance(ExpectedBusinessObjectType);
			var properties = ExpectedBusinessObjectType.GetProperties();
			AssertGetPropertyDoesNotTriggerThreadAccessException(properties, bizObj);
			AssertGetPropertyDoesNotTriggerThreadAccessException(properties, bizObj);
		}

		void AssertGetPropertyDoesNotTriggerThreadAccessException(PropertyInfo[] properties, T instance)
		{
			var thread = new Thread(new ThreadStart(() =>
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						foreach (var property in properties)
						{
							property.GetValue(instance);
						}
					}
				}
				catch (Exception ex) when (!IsThreadAccessException(ex)) { }
			}));
			thread.Start();
			thread.Join();
		}

		bool IsThreadAccessException(Exception ex)
		{
			if (ex == null)
			{
				return false;
			}

			if (ex is CrossThreadAccessException)
			{
				return true;
			}

			if (ex is AggregateException aggregateException)
			{
				return aggregateException.Flatten().InnerExceptions.Any(i => IsThreadAccessException(i));
			}

			return IsThreadAccessException(ex.InnerException);
		}

		[ExpectNoExceptions]
		public void TestHasParameterlessConstructorForXmlSerializer()
		{
			Activator.CreateInstance(ExpectedBusinessObjectType);
		}

		public void TestHasNoNotificationsOnCreation()
		{
			AssertNoNotifications("HasNotifications", BizObj);
		}

		public void TestClone()
		{
			FallbackLevel currentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			T businessObjectToClone = GetBusinessObjectToClone();
			T clone = (T)businessObjectToClone.Clone(currentFallbackLevel, Factory);

			FallbackLevel expectedFallbackLevel = (RequiresFallbackLevel) ? currentFallbackLevel : null;
			BusinessObjectFactory expectedFactory = (RequiresFactory) ? Factory : RegistryFactory.Instance;

			Assert("Clone should be a different instance.", businessObjectToClone != clone);
			AssertEquals("CurrentFallbackLevel", expectedFallbackLevel, clone.CurrentFallbackLevel);
			AssertEquals("CurrentFactory", expectedFactory, CurrentFactoryPropertyInfo.GetValue(clone, null));

			CheckAllPropertiesAreEqual(businessObjectToClone, clone, true);
		}

		[ExpectNoExceptions]
		public void TestSerialiseAndDeserialise()
		{
			IRegistryDataType dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(ExpectedBusinessObjectType);

			T businessObjectToSerialise = GetBusinessObjectToSerialise();
			byte[] serialisedValue = dummyDataType.Serialise(businessObjectToSerialise);

			T deserialisedBusinessObject = (T)dummyDataType.Deserialise(serialisedValue);
			CheckAllPropertiesAreEqual(businessObjectToSerialise, deserialisedBusinessObject, false);
		}

		#region Implementation

		PropertyInfo CurrentFactoryPropertyInfo
		{
			get { return typeof(RegistryBusinessObjectTemplate).GetProperty("CurrentFactory", BindingFlags.NonPublic | BindingFlags.Instance); }
		}

		protected virtual void CheckAllPropertiesAreEqual(T originalBusinessObject, T newBusinessObject, bool isClone)
		{
			CheckAllPropertiesInZPropertyInfoHashAreEqual(originalBusinessObject, newBusinessObject);
		}

		protected void CheckAllPropertiesInZPropertyInfoHashAreEqual(T originalBusinessObject, T newBusinessObject)
		{
			Type businessObjectType = originalBusinessObject.GetType();
			foreach (ZPropertyInfo propertyInfo in originalBusinessObject.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.NonWrapping))
			{
				PropertyInfo info = businessObjectType.GetProperty(propertyInfo.Name);
				if (info.GetCustomAttributes(typeof(BusinessObjectTestExclude), false).Length == 0)
				{
					AssertEquals("NewBusinessObject." + propertyInfo.Name, propertyInfo.Value, newBusinessObject.ZPropertyInfoHash[propertyInfo.Name].Value);
				}
			}
		}

		protected virtual FallbackLevel NewFallbackLevel()
		{
			return new FallbackLevel(Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);
		}

		protected abstract bool RequiresFactory { get; }
		protected abstract bool RequiresFallbackLevel { get; }

		#region Get New Business Object

		protected override void SetUp()
		{
			base.SetUp();
			BizObj = (T)GetNewBusinessObject();
		}

		protected abstract T GetBusinessObjectToClone();
		protected abstract T GetBusinessObjectToSerialise();
		protected T BizObj;

		#endregion

		#endregion
	}
}
