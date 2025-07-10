using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class LookupFilterFieldBaseTest : TestCaseWithFactory
	{
		public void TestBindToFindBoxList()
		{
			var lookupFilter = new MockLookupFilterFieldBase(Factory);
			lookupFilter.SetCollectionProvider(new RefUNLOCOCollectionProvider(new BusinessObjectFactory()));
			IActiveBusinessObjectCollection activeCollection = lookupFilter.BindToFindBoxList as IActiveBusinessObjectCollection;
			Assert(activeCollection != null && !(activeCollection.Relationship is AdhocCollectionRelationship));
		}

		public void TestMasterAndDetailRelationsAreNotNull()
		{
			MockLookupFilterFieldBase lookupFilter = new MockLookupFilterFieldBase(Factory);
			lookupFilter.SetCollectionProvider(new RefUNLOCOCollectionProvider(new BusinessObjectFactory()));
			AssertNotNull("MasterRelations", lookupFilter.MasterRelations);
			AssertNotNull("DetailRelations", lookupFilter.DetailRelations);

			MockLookupFilterFieldBase deserializedLookupFilter;
			var settings = new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				Converters =
				{
					new MockLookupFilterFieldBaseJsonConverter()
				}
			};

			var serializedLookupFilter = JsonSerializer.Serialize(lookupFilter, settings);
			deserializedLookupFilter = JsonSerializer.Deserialize<MockLookupFilterFieldBase>(serializedLookupFilter, settings);

			AssertNotNull("MasterRelations", deserializedLookupFilter.MasterRelations);
			AssertNotNull("DetailRelations", deserializedLookupFilter.DetailRelations);
		}

		#region class MockLookupFilterFieldBase

		class MockLookupFilterFieldBaseJsonConverter : ZJsonConverter<MockLookupFilterFieldBase, MockLookupFilterFieldBaseJsonData>
		{ }

		public class MockLookupFilterFieldBaseJsonData : LookupFieldJsonData
		{ }

		class MockLookupFilterFieldBase : LookupFilterFieldBase, IJsonSerializable
		{
			public MockLookupFilterFieldBase(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			#region Constructor For IJsonSerializable

			internal MockLookupFilterFieldBase(MockLookupFilterFieldBaseJsonData data)
				: base(data)
			{
				if (!string.IsNullOrEmpty(data.CollectionProviderName))
				{
					fCollectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, data.CollectionProviderName);
				}
			}

			#endregion

			public override void ClearValueForUnitTest()
			{
				throw new NotImplementedException();
			}

			public override FilterFieldSuggestedUserControlType SuggestedUserControlType
			{
				get { return FilterFieldSuggestedUserControlType.None; }
			}

			public override bool IsEmpty
			{
				get { throw new NotImplementedException(); }
			}

			public override object ValueAsObject
			{
				get { throw new NotImplementedException(); }
			}

			protected internal new List<MasterDetailRelation> MasterRelations => base.MasterRelations;

			protected internal new List<MasterDetailRelation> DetailRelations => base.DetailRelations;

			protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
			{
				return true;
			}

			public override void FillFilterData(ReportFilterData reportFilterData)
			{
				//Do nothing
			}

			public override void SetFilterValue(ReportFilterData reportFilterData)
			{
				//Do nothing
			}

			protected override string NonEmptyWhereClause()
			{
				throw new NotImplementedException();
			}

			protected override string ValueAsStringForSerialisationInternal
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			#region IFilter Members
			public override void SafeCopyValuesFrom(IFilter source)
			{
			}

			public override void ClearValues()
			{
			}

			public object GetJsonData()
			{
				var filterData = new MockLookupFilterFieldBaseJsonData();
				SetJsonData(filterData);
				filterData.CollectionProviderName = CollectionAndModuleIDBuilder.GetCollectionProviderNameFromType(CollectionProvider.GetType());
				return filterData;
			}
			#endregion
		}

		#endregion
	}
}
