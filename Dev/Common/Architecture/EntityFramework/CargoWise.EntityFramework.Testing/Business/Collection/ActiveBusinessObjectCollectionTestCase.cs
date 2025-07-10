using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestsSubclassesOf(typeof(IActiveBusinessObjectCollection), typeof(TestExcludeBusinessObjectsAllHaveTestCasesAttribute))]
	public abstract class ActiveBusinessObjectCollectionTestCase<T> : BusinessObjectCollectionBaseTestCase<T> where T : IActiveBusinessObjectCollection
	{
		protected override T GetCollectionToTest()
		{
			ConstructorInfo constructor = typeof(T).GetConstructor(new Type[] { typeof(BusinessObjectFactory) })
				?? throw new InvalidOperationException("No constructor was found that takes single argument " + nameof(BusinessObjectFactory) + ". Override GetCollectionToTest() yourself.");
			T result = (T)constructor.Invoke(new object[] { Factory });

			ZQuery fetchFromLocalCacheFilter = new ZQuery();
			fetchFromLocalCacheFilter.FetchOnlyFromLocalCache = true;
			result.AdditionalFilter = fetchFromLocalCacheFilter;
			return result;
		}

		#region TestCollectionStateIsDeclared

		public void TestCollectionStateIsDeclared()
		{
			Type collectionType = Collection.GetType();
			var fieldNames = new List<string>();
			GetNonReadOnlyFieldsIncludingBaseClasses(collectionType, fieldNames);
			if (collectionType.GetMember("GetCollectionState", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod).Length > 0)
			{
				var collectionState = (object[])collectionType.InvokeMember("GetCollectionState", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Collection, null);

				if (fieldNames.Count > (collectionState == null ? 0 : collectionState.Length))
				{
					Fail(string.Format(
@"All {0} state fields must be marked as readonly, returned in GetCollectionState(), or suppressed with SuppressCollectionStateTestAttribute if collection's state does not depend on them.
Check that following fields are change appropriately:
{1}",
						collectionType.FullName, string.Join(Environment.NewLine, fieldNames)));
				}
			}
			Assert(true);
		}

		void GetNonReadOnlyFieldsIncludingBaseClasses(Type type, List<string> fieldNames)
		{
			if (TypeIsActiveBusinessObjectCollectionOfT(type) && !type.FullName.Contains("Test"))
			{
				foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
				{
					if (!field.IsInitOnly && !fieldNames.Contains(field.Name) && !IsGoodField(field))
					{
						fieldNames.Add(field.Name);
					}
				}
			}

			if (TypeIsActiveBusinessObjectCollectionOfT(type.BaseType))
			{
				GetNonReadOnlyFieldsIncludingBaseClasses(type.BaseType, fieldNames);
			}
		}

		bool TypeIsActiveBusinessObjectCollectionOfT(Type type)
		{
			return type != null &&
				(
					type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ActiveBusinessObjectCollection<>) ||
					type.BaseType != type && TypeIsActiveBusinessObjectCollectionOfT(type.BaseType)
				);
		}

		bool IsGoodField(FieldInfo field)
		{
			if (field.GetCustomAttributes(typeof(SuppressCollectionStateTestAttribute), false).Length > 0)
			{
				return true;
			}

			var goodTypes = new[]
			{
				typeof(Delegate),
				typeof(BusinessObjectFactory),
				typeof(IActiveBusinessObjectCollectionIndex),
				typeof(IActiveBusinessObjectCollectionIndexCache),
				typeof(ICollectionRelationship),
				typeof(IComparer),
				typeof(IFindBoxListProvider),
				typeof(Type),
				typeof(CachedProperty<>)
			};

			if (goodTypes.Any(goodType => goodType.IsAssignableFrom(field.FieldType) || field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == goodType))
			{
				return true;
			}

			return false;
		}

		#endregion

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BusinessObject @object = Factory.New(Collection.TypeOfElements);
			if (Collection.Relationship.RelationshipFilter != null)
			{
				ZSqlParameter[] filterParts = Collection.Relationship.RelationshipFilter.FilterParts.Params();
				foreach (ZSqlParameter filterPart in filterParts)
				{
					if (filterPart.ComparisonOperator != SQLComparisonOperator.NotEqual)
					{
						var collection = filterPart.Value as ICollection;

						if (collection != null)
						{
							foreach (var value in collection)
							{
								@object[filterPart.SchemaColumn] = value;
							}
						}
						else
						{
							@object[filterPart.SchemaColumn] = filterPart.Value;
						}
					}
				}
			}
			var objectFromCollection = Collection.ToArray().FirstOrDefault(x => x.PK == @object.PK);
			if (objectFromCollection != null)
			{ return objectFromCollection; }
			return @object;
		}
	}
}
