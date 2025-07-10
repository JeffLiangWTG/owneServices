using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(ShipmentXQueryPaths))]

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class ModuleGuidsFilterTest : ModuleFilterTestCase<ModuleGuidsFilter>
	{
		#region TestProperty1Validation

		public void TestProperty1Validation()
		{
			var errorText = "You cant hug your children with atomic arms!";
			Filter.Property1Validation = null;
			Filter.Property1 = ZGuid.Empty;

			Filter.Validation.ValidateProperty1();
			AssertNoError(Filter.Property1Info, errorText);

			Filter.Property1Validation = delegate(ZPropertyInfo info)
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(errorText);
				}
			};

			Filter.Validation.ValidateProperty1();
			AssertHasError(Filter.Property1Info, errorText);
		}

		#endregion

		#region TestProperty2Validation

		public void TestProperty2Validation()
		{
			var errorText = "You cant hug your children with atomic arms!";
			Filter.Property2Validation = null;
			Filter.Property2 = ZGuid.Empty;

			Filter.Validation.ValidateProperty2();
			AssertNoError(Filter.Property1Info, errorText);

			Filter.Property2Validation = delegate(ZPropertyInfo info)
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(errorText);
				}
			};

			Filter.Validation.ValidateProperty2();
			AssertHasError(Filter.Property2Info, errorText);
		}

		#endregion

		#region TestClearCore_SuspendValidationDuringClear

		public void TestClearCore_SuspendValidationDuringClear()
		{
			var errorText = "You cant hug your children with atomic arms!";
			Filter.Property2Validation = null;
			Filter.Property2 = ZGuid.Empty;

			Filter.Validation.ValidateProperty2();
			AssertNoError(Filter.Property1Info, errorText);

			Filter.Property2Validation = delegate(ZPropertyInfo info)
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(errorText);
				}
			};

			Filter.Validation.ValidateProperty2();
			AssertHasError(Filter.Property2Info, errorText);
		}

		#endregion

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestClearResetsToDefaults

		public void TestClearResetsToDefaults()
		{
			var defaultValue1 = new ZGuid();
			var defaultValue2 = new ZGuid();

			AssertEquals("Precondition", ZGuid.Empty, Filter.Property1);
			AssertEquals("Precondition", ZGuid.Empty, Filter.Property2);
			AssertEquals("Precondition", ZGuid.Empty, Filter.DefaultProperty1);
			AssertEquals("Precondition", ZGuid.Empty, Filter.DefaultProperty2);

			Filter.DefaultProperty1 = defaultValue1;
			Filter.DefaultProperty2 = defaultValue2;
			AssertEquals(defaultValue1, Filter.Property1);
			AssertEquals(defaultValue2, Filter.Property2);

			Filter.Property1 = ZGuid.Empty;
			Filter.Property2 = ZGuid.Empty;
			AssertEquals("Precondition", ZGuid.Empty, Filter.Property1);
			AssertEquals("Precondition", ZGuid.Empty, Filter.Property2);

			Filter.Clear();
			AssertEquals(defaultValue1, Filter.Property1);
			AssertEquals(defaultValue2, Filter.Property2);
			AssertEquals(defaultValue1, Filter.DefaultProperty1);
			AssertEquals(defaultValue2, Filter.DefaultProperty2);

			// test Clear when ReadOnly
			Filter.Property1 = ZGuid.Empty;
			Filter.Property2 = ZGuid.Empty;
			Filter.ReadOnly = true;
			AssertEquals("Precondition", ZGuid.Empty, Filter.Property1);
			AssertEquals("Precondition", ZGuid.Empty, Filter.Property2);

			Filter.Clear();
			AssertEquals("Filter was ReadOnly thus Clear() should have no effect.", ZGuid.Empty, Filter.Property1);
			AssertEquals("Filter was ReadOnly thus Clear() should have no effect.", ZGuid.Empty, Filter.Property2);
		}

		#endregion

		#region TestDeserializePropertiesFromXml

		public void TestDeserializePropertiesFromXml()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new DummyModuleGuidsFilter(Factory);

			var guid1 = new ZGuid();
			filter.Property1 = guid1;

			var guid2 = new ZGuid();
			filter.Property2 = guid2;

			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			strip.FilterDescription = "";
			strip.Delete();

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleGuidsFilter)filterStripBizO[filter.Description];

			AssertEquals(ZGuid.Empty, loadedFilter.Property1);
			AssertEquals(guid2, loadedFilter.Property2);
		}

		#endregion

		#region TestDeserializeInvalidGuidsFromXml

		public void TestDeserializeInvalidGuidsFromXml()
		{
			var filter = new ModuleGuidsFilter(
				"Filter",
				ModuleIDs.AccBankAccount,
				DummyBizoSchema.Z0_Guid,
				new DummyBusinessObjectCollection(Factory),
				DummyBizoSchema.Z0_Guid,
				new DummyBusinessObjectCollection(Factory));

			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();

			using (var stringReader = new StringReader("<Xml><Property1>" + guid1.ToGuid() + "</Property1><Property2>" + guid2.ToGuid() + "</Property2></Xml>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.ReadToFollowing("Property1");
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Property1", guid1, filter.Property1);
				AssertEquals("Property2", guid2, filter.Property2);
			}

			filter.Property1 = ZGuid.Empty;
			filter.Property2 = ZGuid.Empty;
			using (var stringReader = new StringReader("<Xml><Property1>moo</Property1><Property2>oink</Property2></Xml>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.ReadToFollowing("Property1");
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Property1", ZGuid.Empty, filter.Property1);
				AssertEquals("Property2", ZGuid.Empty, filter.Property2);
			}
		}

		#endregion

		#region TestTemplateFilterQuery

		public void TestTemplateFilterQuery()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			var filter = new ModuleGuidsFilter("moo", ModuleIDs.JobShipment, OrgAddressSchema.OA_OH, list, OrgAddressSchema.OA_OH, list);

			AssertEquals("", filter.XQuery.LiteralTextADO);

			filter.PropertyCode1 = "aaa";
			filter.PropertyCode2 = "bbb";
			filter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ConsignorDocumentaryAddress, ShipmentXQueryPaths.ConsigneeDocumentaryAddress, OrgHeaderSchema.OH_Code.MaxLength, OrgHeaderSchema.OH_Code.MaxLength);

			var expectedQuery = @"STR_Data.value('declare default element namespace ""http://www.cargowise.com/Schemas/Universal/2011/11""; (UniversalShipment/Shipment/OrganizationAddressCollection/OrganizationAddress[./AddressType=""ConsignorDocumentaryAddress""]/OrganizationCode)[1]', 'varchar(12)') = 'aaa' and STR_Data.value('declare default element namespace ""http://www.cargowise.com/Schemas/Universal/2011/11""; (UniversalShipment/Shipment/OrganizationAddressCollection/OrganizationAddress[./AddressType=""ConsigneeDocumentaryAddress""]/OrganizationCode)[1]', 'varchar(12)') = 'bbb'";

			AssertEquals("Query when both properties have values", expectedQuery, filter.XQuery.LiteralTextADO);

			filter.PropertyCode2 = "";
			expectedQuery = @"STR_Data.value('declare default element namespace ""http://www.cargowise.com/Schemas/Universal/2011/11""; (UniversalShipment/Shipment/OrganizationAddressCollection/OrganizationAddress[./AddressType=""ConsignorDocumentaryAddress""]/OrganizationCode)[1]', 'varchar(12)') = 'aaa'";

			AssertEquals("Query when one of the properties is empty", expectedQuery, filter.XQuery.LiteralTextADO);
		}

		#endregion

		#region TestIsNotBlankShouldBeValidOfModuleGuidFilter

		public void TestIsNotBlankShouldBeValidOfModuleGuidFilter()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var business1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var business2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dependentBusiness = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			business1.Z0_Guid = dependentBusiness.PK;
			business2.Z0_Guid = ZGuid.Empty;
			collection.Add(business1);
			collection.Add(business2);

			Factory.Save();

			var filterBizO = new DummyFilterBusinessObjectWithModuleGuidFilter();
			var filter = (ModuleGuidFilter)filterBizO["RelatedBizo"];
			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filter.IsActive = true;
			filter.SubGroup = new DummyModuleFilterSubGroupForModuleGuidFilterTest();

			AssertEquals(true, filter.ForceProcessingGroup);

			collection.Load(filterBizO.Filter);

			AssertEquals(1, collection.Count);
			AssertCollectionContains(business1, collection);
			AssertCollectionNotContains(business2, collection);
		}

		public void TestIsNotBlankFilterShouldNotHaveValidationWarningInSomeSituations()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var business1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var business2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dependentBusiness = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			business1.Z0_Guid = dependentBusiness.PK;
			business2.Z0_Guid = ZGuid.Empty;
			collection.Add(business1);
			collection.Add(business2);

			Factory.Save();

			var filterBizO = new DummyFilterBusinessObjectWithModuleGuidFilter();
			var filter = (ModuleGuidFilter)filterBizO["RelatedBizo"];
			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filter.IsActive = true;

			Assert(!filter.ForceProcessingGroup);
			AssertEquals(null, filter.QueryDelegate);
			filter.Validation.ValidateComparisonOperator();
			Assert(filter.ComparisonOperatorInfo.HasWarnings());

			filter.SubGroup = new DummyModuleFilterSubGroupForModuleGuidFilterTest();
			Assert(filter.ForceProcessingGroup);
			AssertEquals(null, filter.QueryDelegate);
			filter.Validation.ValidateComparisonOperator();
			Assert(!filter.ComparisonOperatorInfo.HasWarnings());

			filterBizO = new DummyFilterBusinessObjectWithModuleGuidFilter(new GetGuidQueryWithOperator((a, b) => new ZQuery()));
			filter = (ModuleGuidFilter)filterBizO["RelatedBizo"];
			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filter.IsActive = true;

			Assert(!filter.ForceProcessingGroup);
			AssertNotEquals(null, filter.QueryDelegate);
			filter.Validation.ValidateComparisonOperator();
			Assert(!filter.ComparisonOperatorInfo.HasWarnings());
		}

		#endregion

		#region Implementation

		protected override ModuleGuidsFilter GetNewModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			return new ModuleGuidsFilter("moo", ModuleIDs.JobShipment, DummyBizoSchema.Z0_Guid, list, DummyBizoSchema.Z0_Guid, list);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleGuidsFilter filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { nameof(filter.PropertyCode1), nameof(filter.PropertyCode2) }).ToArray();
		}

		#endregion

		#region DummyModuleFilterSubGroupForModuleGuidFilterTest

		class DummyModuleFilterSubGroupForModuleGuidFilterTest : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
				var subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyBizoSchema.Z0_Guid);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		#region DummyFilterBusinessObjectWithModuleGuidFilter

		class DummyFilterBusinessObjectWithModuleGuidFilter : DummyFilterBusinessObject
		{
			public GetGuidQueryWithOperator queryDelegate { get; set; }
			public DummyFilterBusinessObjectWithModuleGuidFilter()
			{
			}
			public DummyFilterBusinessObjectWithModuleGuidFilter(GetGuidQueryWithOperator queryDelegate)
			{
				this.queryDelegate = queryDelegate;
			}
			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				var filters = base.GetModuleFiltersCore();
				if (queryDelegate == null)
				{
					filters.AddGuidFilter("RelatedBizo", DummyModuleIDs.Dummy, DummyDependentBizoSchema.PK, new DummyDependentBusinessObjectCollection(Factory));
				}
				else
				{
					filters.AddGuidFilter("RelatedBizo", DummyModuleIDs.Dummy, queryDelegate, new DummyDependentBusinessObjectCollection(Factory), DummyDependentBizoSchema.PK);
				}

				return filters;
			}
		}

		#endregion

		#region DummyModuleGuidsFilter

		public class DummyModuleGuidsFilter : ModuleGuidsFilter
		{
			public DummyModuleGuidsFilter(BusinessObjectFactory factory)
				: base("DummyGuidsFilter", ModuleIDs.Organisation, DummyBizoSchema.Z0_Guid, new StmNoteNonDependentCollection(factory), DummyBizoSchema.Z0_Guid, new StmNoteNonDependentCollection(factory))
			{
			}

			protected override void SerializePropertiesToXml(XmlWriter writer)
			{
				writer.WriteElementString("Property2", Property2.ToString());
			}
		}

		#endregion
	}
}
