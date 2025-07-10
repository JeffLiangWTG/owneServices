using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	public abstract class BusinessObjectCollectionBaseTestCase<T> : TestCaseWithFactory where T : IBusinessObjectCollection
	{
		protected abstract T GetCollectionToTest();

		protected abstract BusinessObject GetNewElementToAddToTheCollection();

		public virtual void TestAddNew()
		{
			int initialCount = Collection.Count;
			if (Collection.AllowNew)
			{
				try
				{
					BusinessObject bizO1 = Collection.AddNew();
					BusinessObject bizO2 = Collection.AddNew();

					AssertEquals("Collection count", initialCount + 2, Collection.Count);
					Assert("Contains new elements", Collection.Contains(bizO1));
					Assert("Contains new elements", Collection.Contains(bizO2));
					var expectedType = bizO1.GetType();
					var type = Collection.TypeOfElements;
					AssertEquals(Collection.GetType().FullName + ".TypeOfElements (" + type.FullName + ") should be assignable from (" + expectedType.FullName + ")", true, type.IsAssignableFrom(expectedType));
				}
				catch (NotSupportedException)
				{
					AssertEquals("NotSupportedException thrown on CreateNonPersistentBusinessObject(). Override AllowNew and return false.", false, Collection.AllowNew);
				}
			}
			Assert(true);
		}

		[ExpectNoExceptions]
		public virtual void TestTypedAddNew()
		{
			if (Collection.AllowNew)
			{
				try
				{
					MethodInfo typedAddNewMethod = GetTypedAddNewMethod();
					if (typedAddNewMethod != null && typedAddNewMethod.ReturnType != typeof(BusinessObject))
					{
						BusinessObject bizO2 = (BusinessObject)typedAddNewMethod.Invoke(Collection, Array.Empty<object>());
						AssertNotNull("AddNew of Type " + typedAddNewMethod.ReturnType.FullName + " not null", bizO2);
					}
				}
				catch (TargetInvocationException e)
				{
					if (!(e.InnerException is NotSupportedException))
					{
						throw;
					}
				}
			}
		}

		public virtual void TestTypedget_Item()
		{
			try
			{
				if (Collection.Count == 0)
				{
					BusinessObject businessObject = GetNewElementToAddToTheCollection();
					if (Collection.Count == 0 &&
						(ActiveCollection == null || ActiveCollection.Relationship.SupportsAddToRelationship()))
					{
						Collection.Add(businessObject);
					}
				}
				MethodInfo typedget_ItemMethod = Collection.GetType().GetMethod("get_Item", new Type[] { typeof(int) });
				if (typedget_ItemMethod != null)
				{
					BusinessObject result = (BusinessObject)typedget_ItemMethod.Invoke(Collection, new object[] { 0 });
					AssertNotNull("Return Type of get_Item", result);
				}
				else
				{
					Assert(true);
				}
			}
			catch (TargetInvocationException e)
			{
				if (e.InnerException is NotSupportedException)
				{
					Assert(true);
				}
				else
				{
					throw;
				}
			}
		}

		public virtual void TestAdd()
		{
			int initialCount = Collection.Count;

			BusinessObject bizO1 = GetNewElementToAddToTheCollection();
			BusinessObject bizO2 = GetNewElementToAddToTheCollection();

			if (ActiveCollection == null || ActiveCollection.Relationship.SupportsAddToRelationship())
			{
				Collection.Add(bizO1);
				Collection.Add(bizO2);
			}

			AssertEquals("Collection count", initialCount + 2, Collection.Count);
			Assert("Contains element 1", Collection.Contains(bizO1));
			Assert("Contains element 2", Collection.Contains(bizO2));
		}

		public virtual void TestRemoveFromRelationship()
		{
			int initialCount = Collection.Count;

			BusinessObject bizO1 = GetNewElementToAddToTheCollection();
			BusinessObject bizO2 = GetNewElementToAddToTheCollection();

			if (ActiveCollection == null || ActiveCollection.Relationship.SupportsAddToRelationship())
			{
				Collection.Add(bizO1);
				Collection.Add(bizO2);
				AssertEquals("Precondition : Collection count", initialCount + 2, Collection.Count);

				Collection.RemoveFromRelationship(bizO1); //remove bizO1 from the collection

				AssertEquals("Collection count", initialCount + 1, Collection.Count);
				Assert("Contains element 2", Collection.Contains(bizO2));
				Assert("Doesn't contain element 1", !Collection.Contains(bizO1));
				Assert("Element 1 was only removed, but not deleted", !bizO1.IsDeleted);

				ErrorReporter.Clear();

				Collection.RemoveFromRelationship(bizO1); //remove again bizO1 from the collection (twice)

				AssertEquals(ZString.Empty, ErrorReporter.LastMessageReported);
				AssertEquals("Collection count", initialCount + 1, Collection.Count);
				Assert("Contains element 2", Collection.Contains(bizO2));
				Assert("Doesn't contain element 1", !Collection.Contains(bizO1));
				Assert("Element 1 was only removed, but not deleted", !bizO1.IsDeleted);

				ErrorReporter.Clear();
			}
			else if (ActiveCollection != null)
			{
				AssertExceptionThrown(typeof(InvalidOperationException), () => Collection.RemoveFromRelationship(bizO1));
			}
		}

		public virtual void TestDelete()
		{
			int initialCount = Collection.Count;
			try
			{
				BusinessObject bizO1 = GetNewElementToAddToTheCollection();
				BusinessObject bizO2 = GetNewElementToAddToTheCollection();
				if (ActiveCollection == null || ActiveCollection.Relationship.SupportsAddToRelationship())
				{
					Collection.Add(bizO1);
					Collection.Add(bizO2);
				}
				AssertEquals("Precondition : Collection count", initialCount + 2, Collection.Count);

				Collection.Delete(bizO1);

				AssertEquals("Collection count", initialCount + 1, Collection.Count);
				Assert("Contains element 2", Collection.Contains(bizO2));
				Assert("Doesn't contain element 1", !Collection.Contains(bizO1));
				Assert("Element 1 was removed, and deleted", bizO1.IsDeleted);
			}
			catch (NotSupportedException) // deleting not supported
			{
				Assert(true);
			}
			catch (CannotDeleteException) // deleting not supported in a zgrid handleable way
			{
				Assert(true);
			}
		}

		public virtual void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("Precondition: Collection does not have changes", !Collection.HasChanges);
			int initialCount = Collection.Count;
			IBindingList bindingList = Collection;

			if (bindingList.AllowNew)
			{
				BusinessObject @new = (BusinessObject)bindingList.AddNew();

				AssertEquals("New element added to collection", initialCount + 1, Collection.Count);
				Assert("New element added to collection", bindingList.Contains(@new));

				((ICancelAddNew)bindingList).CancelNew(bindingList.Count - 1);

				AssertEquals("New element removed from the collection", initialCount, Collection.Count);
				AssertEquals("Has changes should be set to false.", false, Collection.HasChanges);
			}
		}

		[ExpectNoExceptions]
		public virtual void TestAddAndDeleteOfElementAsThoughBinding()
		{
			if (!Collection.TypeOfElements.IsAbstract)
			{
				var list = (IBindingList)Collection;
				if (list.AllowNew)
				{
					var element = (BusinessObject)list.AddNew();
					var iCollection = (IBusinessObjectCollection)Collection;
					if (iCollection.AllowRemove && element.CanDelete)
					{
						iCollection.Delete(element);
					}
				}
			}
		}

		[ExpectNoExceptions]
		public virtual void TestCancelNew_DoesNotReport()
		{
			var icancel = (ICancelAddNew)Collection;
			if (icancel != null)
			{
				icancel.CancelNew(-2);
				icancel.CancelNew(0);
				icancel.CancelNew(999);

				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		#region Generics

		public void TestReintroducedAddNewRemovedForGenericCollection()
		{
			Type collectionType = Collection.GetType();
			Type collectionTypeBeforeGeneric = collectionType;
			Type genericType = GetFirstGenericBaseClass(ref collectionTypeBeforeGeneric);
			if (genericType != null && genericType.GenericTypeArguments[0] != typeof(BusinessObject))
			{
				foreach (MethodInfo method in collectionType.GetMethods())
				{
					if (typeof(BusinessObject).IsAssignableFrom(method.ReturnType)
						&& method.Name == "AddNew"
						&& method.GetParameters().Length == 0
						&& method.DeclaringType != genericType
						&& method.DeclaringType == collectionTypeBeforeGeneric)
					{
						Fail("You must remove the public new " + method.ReturnType.Name + " AddNew() from " + collectionTypeBeforeGeneric.Name + " as it should be already defined in the generic " + genericType.Name + "<T>.");
					}
				}
				Assert("Passed", true);
			}
			else
			{
				Assert("Only relevant for generic collections", true);
			}
		}

		public void TestReintroducedIndexerRemovedForGenericCollection()
		{
			Type collectionType = Collection.GetType();
			Type collectionTypeBeforeGeneric = collectionType;
			Type genericType = GetFirstGenericBaseClass(ref collectionTypeBeforeGeneric);
			if (genericType != null && genericType.GenericTypeArguments[0] != typeof(BusinessObject))
			{
				foreach (PropertyInfo property in collectionType.GetProperties())
				{
					if (typeof(BusinessObject).IsAssignableFrom(property.PropertyType)
						&& property.GetIndexParameters().Length == 1
						&& property.GetIndexParameters()[0].ParameterType == typeof(int)
						&& property.DeclaringType != genericType
						&& property.DeclaringType == collectionTypeBeforeGeneric)
					{
						Fail("You must remove the public new " + property.PropertyType.Name + " this[int i] from " + collectionTypeBeforeGeneric.Name + " as it should be already defined in the generic " + genericType.Name + "<T>.");
					}
				}
				Assert("Passed", true);
			}
			else
			{
				Assert("Only relevant for generic collections", true);
			}
		}
		#endregion

		#region Sparse columns should not be set default value in base

		[ExpectNoExceptions]
		public virtual void TestSparseColumnsAreEmptyAfterSetDefaultValues()
		{
			var message = ZString.Empty;

			if (Collection is BusinessObjectCollection collection && !collection.TypeOfElements.IsAbstract && !collection.IsNonPersistent && collection.AllowNew)
			{
				var bizo = GetNewElementToAddToTheCollection();
				if (bizo != null)
				{
					message = GenericTestHelper.AssertSparseColumnsAreEmptyAfterSetDefaultValues(TestConnection, bizo, GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues());
				}
			}

			if (message.IsEmpty)
			{
				Assert(true);
			}
			else
			{
				Assert(message, false);
			}
		}

		public virtual List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>();
		}

		#endregion

		#region Implementation

		protected Type GetFirstGenericBaseClass(ref Type collectionType)
		{
			Type baseType = null;
			while ((baseType = collectionType.BaseType) != null)
			{
				if (baseType.IsGenericType)
				{
					return baseType;
				}
				collectionType = baseType;
			}
			return null;
		}

		protected T Collection
		{
			get
			{
				if (fCollection == null)
				{
					fCollection = GetCollectionToTest();
				}

				return fCollection;
			}
		}
		T fCollection;

		IActiveBusinessObjectCollection ActiveCollection
		{
			get { return Collection as IActiveBusinessObjectCollection; }
		}

		MethodInfo GetTypedAddNewMethod()
		{
			Type collectionType = Collection.GetType();
			return collectionType.GetMethod("AddNew", Array.Empty<Type>());
		}

		protected virtual Type GetExpectedCollectionType()
		{
			Type result;
			MethodInfo getCollectionToTestMethod = GetCollectionToTestMethod();
			if (getCollectionToTestMethod.GetGenericArguments().Length == 1)
			{
				// avoid instantiating the object which can create forms etc
				result = getCollectionToTestMethod.DeclaringType.GetGenericArguments()[0];
			}
			else
			{
				result = Collection.GetType();
			}
			return result;
		}

		MethodInfo GetCollectionToTestMethod()
		{
			return GetCollectionToTestMethod(GetType());
		}

		MethodInfo GetCollectionToTestMethod(Type testType)
		{
			MethodInfo result = testType.GetMethod("GetCollectionToTest", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Array.Empty<Type>(), null);
			if (result == null && testType != typeof(object))
			{
				result = GetCollectionToTestMethod(testType.BaseType);
			}
			return result;
		}

		#endregion
	}
}
