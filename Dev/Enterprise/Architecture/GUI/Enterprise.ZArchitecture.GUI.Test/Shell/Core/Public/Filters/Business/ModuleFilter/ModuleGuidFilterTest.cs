using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleGuidFilter))]
	public class ModuleGuidFilterTest : ModuleFilterWithSelectedFiltersTestCase<ModuleGuidFilter, ZGuid>
	{
		#region TestUsingFilterColumnsWithNotEqualQueryContainsDBNull

		public void TestUsingFilterColumnsWithNotEqualQueryContainsDBNull()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			var filter = new ModuleGuidFilter("Test GUID", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, list);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = ZGuid.NewZGuid();
			filter.Visibility = FilterVisibility.Visible;
			var query = filter.Query;

			var isBlankQuery = new ZQuery();
			isBlankQuery.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Guid, SpecialComparisonOperator.IsBlank, DBNull.Value);

			AssertContains("ModuleGuidFilter should have is blank query.", isBlankQuery.FilterString, query.FilterString);
		}

		#endregion

		#region TestPropertyValidation

		public void TestPropertyValidation()
		{
			var errorText = "You cant hug your children with atomic arms!";
			Filter.PropertyValidation = null;
			Filter.Property = ZGuid.Empty;

			Filter.Validation.ValidateProperty();
			AssertNoError(Filter.PropertyInfo, errorText);

			Filter.PropertyValidation = delegate(ZPropertyInfo info)
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(errorText);
				}
			};

			Filter.Validation.ValidateProperty();
			AssertHasError(Filter.PropertyInfo, errorText);
		}

		#endregion

		#region TestValidation

		public void TestValidation_MissingValue()
		{
			Filter.MultilingualDescription = (NoResString)"moo";
			Filter.Property = ZGuid.Missing;
			AssertHasError(Filter.PropertyInfo, string.Format("The selected {0} is no longer valid. Please choose a new {0} from the list.", Filter.MultilingualDescription));
		}

		public void TestValidation_ShouldValidateSelectedFilters()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessTasks))
			{
				var parentFilter = (ModuleGuidModuleSpecifiedFilter)module.FilterBusinessObject["Parent Job"];
				parentFilter.SelectedModule = ModuleIDs.SalesEnquiry.Name;
				var subModuleFilter = parentFilter.SelectedFilters.AddNkFilterStrip("Assigned Staff", "XYI");
				AssertNoErrors(parentFilter);

				parentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				AssertHasError(parentFilter.SelectedFiltersDescriptionInfo, "The selected filters have one or more errors.");

				subModuleFilter.Property = Env.CurrentUser.Initials;
				parentFilter.Validation.ValidateSelectedFiltersDescription();
				AssertNoErrors(parentFilter);
			}
		}

		public void TestValidation_CheckPropertyShouldUseMultilingualDescriptionInErrorMessageWhenShouldBeLocalizable()
		{
			Filter.Property = ZGuid.Invalid;
			Filter.MultilingualDescription = (NoResString)"Test validation";

			using (var mockChs = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("8ABCBBA5-C380-4F47-8B5A-03B53B63282C", new ResourceStringData("8ABCBBA5-C380-4F47-8B5A-03B53B63282C", "测试"));

				using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
				{
					Filter.Validation.ValidateProperty();
					AssertHasErrorContaining(Filter.PropertyInfo, Filter.MultilingualDescription.ToString());
				}
			}
		}

		public void TestValidation_CheckComparisonShouldUseMultilingualDescriptionInErrorMessageWhenShouldBeLocalizable()
		{
			var originalValue = typeof(SchemaColumn).GetField("IsNullable").GetValue(Filter.FilterColumn);
			try
			{
				typeof(SchemaColumn).GetField("IsNullable").SetValue(Filter.FilterColumn, false);
				Filter.MultilingualDescription = (NoResString)"Test validation";

				using (var mockChs = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
				{
					mockChs.Put("8ABCBBA5-C380-4F47-8B5A-03B53B63282C", new ResourceStringData("8ABCBBA5-C380-4F47-8B5A-03B53B63282C", "测试"));

					using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
					{
						Filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
						Filter.Validation.ValidateComparisonOperator();
						AssertHasErrorContaining(Filter.ComparisonOperatorInfo, Filter.MultilingualDescription.ToString());

						Filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
						Filter.Validation.ValidateComparisonOperator();
						AssertHasWarningContaining(Filter.ComparisonOperatorInfo, Filter.MultilingualDescription.ToString());
					}
				}
			}
			finally
			{
				typeof(SchemaColumn).GetField("IsNullable").SetValue(Filter.FilterColumn, originalValue);
			}
		}

		public void TestValidation_CheckComparisonOperatorUsesValidationPropertyForDeterminingIfColumnIsNullable_NoColumn()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			var filter1 = new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperator((a, b) => new ZQuery()), list);
			var filter2 = new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperator((a, b) => new ZQuery()), () => list);
			var filter3 = new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperatorAndOption((a, b, c) => new ZQuery()), list);
			AssertValidation_CheckComparisonOperatorUsesValidationPropertyForDeterminingIfColumnIsNullable(false, filter1, filter2, filter3);
		}

		public void TestValidation_CheckComparisonOperatorUsesValidationPropertyForDeterminingIfColumnIsNullable_NullableColumn()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			var z0 = DummyBizoSchema.Z0_Guid;
			Assert(z0.IsNullable);
			var filter1 = new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperator((a, b) => new ZQuery()), list, z0);
			var filter2 = new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperator((a, b) => new ZQuery()), () => list, z0);
			var filter3 = new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperatorSupportsFiltersMatch((a, b, c) => new ZQuery()), list, z0);
			var filter4 = new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperatorSupportsFiltersMatch((a, b, c) => new ZQuery()), () => list, z0);
			var filter5 = new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperatorAndOption((a, b, c) => new ZQuery()), list, z0);
			AssertValidation_CheckComparisonOperatorUsesValidationPropertyForDeterminingIfColumnIsNullable(false, filter1, filter2, filter3, filter4, filter5);
		}

		public void TestValidation_CheckComparisonOperatorUsesValidationPropertyForDeterminingIfColumnIsNullable_NonNullableColumn()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			var z0_notNullable = new SchemaGuidColumn(DummyBizoSchema.Instance, "Z0_GuidNullable", 50, DBNull.Value, false);
			Assert(!z0_notNullable.IsNullable);
			var filter1 = new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperator((a, b) => new ZQuery()), list, z0_notNullable);
			var filter2 = new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperator((a, b) => new ZQuery()), () => list, z0_notNullable);
			var filter3 = new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperatorSupportsFiltersMatch((a, b, c) => new ZQuery()), list, z0_notNullable);
			var filter4 = new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperatorSupportsFiltersMatch((a, b, c) => new ZQuery()), () => list, z0_notNullable);
			var filter5 = new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperatorAndOption((a, b, c) => new ZQuery()), list, z0_notNullable);
			AssertValidation_CheckComparisonOperatorUsesValidationPropertyForDeterminingIfColumnIsNullable(true, filter1, filter2, filter3, filter4, filter5);
		}

		void AssertValidation_CheckComparisonOperatorUsesValidationPropertyForDeterminingIfColumnIsNullable(bool shouldFailValidation, params ModuleGuidFilter[] filtersToTest)
		{
			foreach (var filter in filtersToTest)
			{
				filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
				filter.Validation.ValidateComparisonOperator();
				if (shouldFailValidation)
				{
					Assert(filter.ComparisonOperatorInfo.HasError("DESC field cannot be blank. Please choose another filter option."));
				}
				else
				{
					Assert(!filter.ComparisonOperatorInfo.HasErrors());
				}

				filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
				filter.Validation.ValidateComparisonOperator();
				Assert(!filter.ComparisonOperatorInfo.HasErrors());
				var forceProcessingGroup = filter.SqlComparisonOperator == SpecialComparisonOperator.IsNotBlank && filter.SubGroup != null;
				if (shouldFailValidation && !forceProcessingGroup && filter.QueryDelegate == null)
				{
					Assert(filter.ComparisonOperatorInfo.HasWarning("DESC field cannot be blank. This filter will return all records."));
				}
				else
				{
					Assert(!filter.ComparisonOperatorInfo.HasWarnings());
				}
			}
		}

		public void TestValidation_CheckComparisonOperator_ShouldNotBlockIsBlankFilterIfHasBlankQueryDelegate()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			var z0_notNullable = new SchemaGuidColumn(DummyBizoSchema.Instance, "Z0_GuidNullable", 50, DBNull.Value, false);
			Assert(!z0_notNullable.IsNullable);
			var filter = new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperator((a, b) => new ZQuery()), list, z0_notNullable);
			filter.IsBlankFilterQueryDelegate = () => ZQuery.NoResultQuery;
			AssertValidation_CheckComparisonOperatorUsesValidationPropertyForDeterminingIfColumnIsNullable(false, filter);
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
			var defaultValue = ZGuid.NewZGuid();

			AssertEquals("Precondition", ZGuid.Empty, Filter.Property);
			AssertEquals("Precondition", ZGuid.Empty, Filter.DefaultProperty);

			Filter.DefaultProperty = defaultValue;
			AssertEquals(defaultValue, Filter.Property);

			Filter.Property = ZGuid.Empty;
			AssertEquals("Precondition", ZGuid.Empty, Filter.Property);

			Filter.Clear();
			AssertEquals(defaultValue, Filter.Property);
			AssertEquals(defaultValue, Filter.DefaultProperty);

			// test Clear when ReadOnly
			Filter.Property = ZGuid.Empty;
			Filter.ReadOnly = true;
			Filter.Clear();
			AssertEquals("Filter was ReadOnly thus Clear() should have no effect.", ZGuid.Empty, Filter.Property);
		}

		public void TestClear_ShouldResetSelectedFilters()
		{
			var filterBizo = GetSelectedFiltersBusinessObjectForTest();
			var filter = (ModuleGuidFilter)filterBizo["Dummy"];
			filter.SelectedFilters.AddTextFilterStrip("Z0_Description");

			AssertEquals(2, filter.SelectedFilterCount);
			AssertEquals(false, filter.Query.IsEmpty);

			filter.ReadOnly = true;
			filter.Clear();

			AssertEquals("Filter was ReadOnly thus Clear() should have no effect, and yet...", 2, filter.SelectedFilterCount);

			filter.ReadOnly = false;
			filter.Clear();

			AssertEquals("Filter was cleared, so the selected filters should be reset, and yet...", 0, filter.SelectedFilterCount);
		}

		#endregion

		#region TestDeserializePropertiesFromXml

		public void TestDeserializePropertiesFromXml()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new DummyModuleGuidFilter(Factory);

			var guid = new ZGuid();
			filter.Property = guid;

			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			strip.FilterDescription = "";
			strip.Delete();

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleGuidFilter)filterStripBizO[filter.Description];

			AssertEquals(ZGuid.Empty, loadedFilter.Property);
		}

		public void TestDeserializeSelectedFiltersFromXml()
		{
			var filterBizo = GetSelectedFiltersBusinessObjectForTest();
			var filter = (ModuleGuidFilter)filterBizo["Dummy"];

			AssertEquals(false, filterBizo.Filter.IsEmpty);

			var savedFilter = filterBizo.SaveLayout("Saved Filter");
			var newFilterBizo = new DummyDependentFilterBusinessObject();
			newFilterBizo.LoadLayout(savedFilter);

			AssertEquals(filterBizo.Filter.LiteralTextSqlFormatted, newFilterBizo.Filter.LiteralTextSqlFormatted);

			var loadedFilter = newFilterBizo["Dummy"];

			AssertEquals(filter.Query.LiteralTextSqlFormatted, loadedFilter.Query.LiteralTextSqlFormatted);
		}

		#endregion

		#region TestDeserializeInvalidGuidFromXml

		public void TestDeserializeComparisonOperator()
		{
			var filter = new ModuleGuidFilter("Filter", ModuleIDs.AccBankAccount, DummyBizoSchema.Z0_Guid, new DummyBusinessObjectCollection(Factory));

			using (var stringReader = new StringReader("<Comparer>" + ModuleTextBaseFilter.ComparisonConstants.NotEqual + "</Comparer>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("ComparisonOperator should be 'Not Equal'", ModuleTextBaseFilter.ComparisonConstants.NotEqual, filter.ComparisonOperator);
			}

			using (var stringReader = new StringReader("<Comparer>moo</Comparer>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("ComparisonOperator should remain the same if invalid option entered", ModuleTextBaseFilter.ComparisonConstants.NotEqual, filter.ComparisonOperator);
			}
		}

		#endregion

		#region TestDeserializeInvalidGuidFromXml

		public void TestDeserializeInvalidGuidFromXml()
		{
			var filter = new ModuleGuidFilter("Filter", ModuleIDs.AccBankAccount, DummyBizoSchema.Z0_Guid, new DummyBusinessObjectCollection(Factory));
			var guid = ZGuid.NewZGuid();

			using (var stringReader = new StringReader("<Property>" + guid.ToGuid() + "</Property>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Property", guid, filter.Property);
			}

			filter.Property = ZGuid.Empty;
			using (var stringReader = new StringReader("<Property>moo</Property>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Property", ZGuid.Empty, filter.Property);
			}
		}

		#endregion

		#region Comparison Operators

		public void TestAllowedComparisonOperators()
		{
			AssertEquals(6, Filter.AllowedComparisonOperators.Count);
			Assert(StringArrayContains(Filter.AllowedComparisonOperators, ModuleTextBaseFilter.ComparisonConstants.Exact));
			Assert(StringArrayContains(Filter.AllowedComparisonOperators, ModuleTextBaseFilter.ComparisonConstants.NotEqual));
			Assert(StringArrayContains(Filter.AllowedComparisonOperators, ModuleTextBaseFilter.ComparisonConstants.IsBlank));
			Assert(StringArrayContains(Filter.AllowedComparisonOperators, ModuleTextBaseFilter.ComparisonConstants.IsNotBlank));
			Assert(StringArrayContains(Filter.AllowedComparisonOperators, ModuleTextBaseFilter.ComparisonConstants.FiltersMatch));
		}

		public void TestAllowedComparisonOperators_NotSupportBlankComparisonOperators()
		{
			Filter.SupportsBlankComparisonOperators = false;

			AssertEquals(4, Filter.AllowedComparisonOperators.Count);
			Assert(StringArrayContains(Filter.AllowedComparisonOperators, ModuleTextBaseFilter.ComparisonConstants.Exact));
			Assert(StringArrayContains(Filter.AllowedComparisonOperators, ModuleTextBaseFilter.ComparisonConstants.NotEqual));
			Assert(StringArrayContains(Filter.AllowedComparisonOperators, ModuleTextBaseFilter.ComparisonConstants.FiltersMatch));
			Assert(!StringArrayContains(Filter.AllowedComparisonOperators, ModuleTextBaseFilter.ComparisonConstants.IsBlank));
			Assert(!StringArrayContains(Filter.AllowedComparisonOperators, ModuleTextBaseFilter.ComparisonConstants.IsNotBlank));
		}

		static bool StringArrayContains(IEnumerable<string> array, string value)
		{
			return array.Any(entry => entry == value);
		}

		public void TestHasComparisonOperator()
		{
			var g1 = new ModuleGuidFilter("aaa", ModuleIDs.AccBankAccount, DoNothingWithOperator, new DummyBusinessObjectCollection(Factory));
			var g2 = new ModuleGuidFilter("bbb", ModuleIDs.AccBankAccount, DoNothing, new DummyBusinessObjectCollection(Factory));
			var g3 = new ModuleGuidFilter("ccc", ModuleIDs.AccBankAccount, DoNothingWithOperatorSupportsFilterMatching, new DummyBusinessObjectCollection(Factory), DummyBizoSchema.Z0_Guid);
			var g4 = new ModuleGuidFilter("ccc", ModuleIDs.AccBankAccount, DoNothingWithOperatorAndOption, new DummyBusinessObjectCollection(Factory));
			var g5 = new ModuleGuidFilter("ddd", ModuleIDs.AccBankAccount, AccBankAccountSchema.AB_AG, new DummyBusinessObjectCollection(Factory));

			Assert(g1.HasComparisonOperator);
			Assert(!g2.HasComparisonOperator);
			Assert(g3.HasComparisonOperator);
			Assert(g4.HasComparisonOperator);
			Assert(g5.HasComparisonOperator);
		}

		public void TestMatchesFilterOperator_ShouldOnlyAppearWhenDesired()
		{
			var filterWithSchemaColumn = new ModuleGuidFilter("aaa", ModuleIDs.AccBankAccount, AccBankAccountSchema.AB_AG, new DummyBusinessObjectCollection(Factory));
			var filterWithQuerySupportsFilterMatching = new ModuleGuidFilter("bbb", ModuleIDs.AccBankAccount, DoNothingWithOperatorSupportsFilterMatching, new DummyBusinessObjectCollection(Factory), DummyBizoSchema.Z0_Guid) { SupportsFiltersMatchComparisonOperator = true };
			var filterWithQueryComparisonAllowed = new ModuleGuidFilter("bbb", ModuleIDs.AccBankAccount, DoNothingWithOperatorAndOption, new DummyBusinessObjectCollection(Factory)) { SupportsFiltersMatchComparisonOperator = true };
			var filterWithQueryComparisonNotAllowed = new ModuleGuidFilter("ccc", ModuleIDs.AccBankAccount, DoNothingWithOperatorAndOption, new DummyBusinessObjectCollection(Factory));

			AssertEquals(true, filterWithSchemaColumn.AllowedComparisonOperators.Contains(ModuleTextFilter.ComparisonConstants.FiltersMatch));
			AssertEquals(true, filterWithQuerySupportsFilterMatching.AllowedComparisonOperators.Contains(ModuleTextFilter.ComparisonConstants.FiltersMatch));
			AssertEquals(true, filterWithQueryComparisonAllowed.AllowedComparisonOperators.Contains(ModuleTextFilter.ComparisonConstants.FiltersMatch));
			AssertEquals(false, filterWithQueryComparisonNotAllowed.AllowedComparisonOperators.Contains(ModuleTextFilter.ComparisonConstants.FiltersMatch));
		}

		public void TestFiltersMatchSubQueryForDelegate()
		{
			ZDBOnlyQuery cachedQuery = null;
			GetGuidQueryWithOperatorSupportsFiltersMatch del =
				(subQuery, comparisonOperator, value) =>
				{
					cachedQuery = subQuery;
					var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
					if (subQuery != null)
					{
						query.AddSubQuery(subQuery, JoinCondition.And);
					}
					return query;
				};

			var filter = new ModuleGuidFilter("Z0_PK", DummyModuleIDs.Dummy, del, new DummyBusinessObjectCollection(Factory), DummyBizoSchema.PK);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

			var textFilter = filter.SelectedFilters.AddTextFilterStrip(nameof(DummyBizoSchema.Z0_Description), "some description");
			AssertContains("Selected Filters should be passed to delegate when operator is Filters Match", TrimByLine(textFilter.Query.LiteralTextSqlFormatted), TrimByLine(filter.Query.LiteralTextSqlFormatted));
			AssertNotNull("Selected Filters subquery not null when operator is Filters Match", cachedQuery);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = ZGuid.NewZGuid();
			AssertNotContains("Selected Filters should not be passed to delegate when operator is not Filters Match", TrimByLine(textFilter.Query.LiteralTextSqlFormatted), TrimByLine(filter.Query.LiteralTextSqlFormatted));
			AssertNull("Selected Filters subquery is null when operator is not Filters Match", cachedQuery);

			string TrimByLine(string text) => string.Join(System.Environment.NewLine, text.SplitByLine().Select(x => x.Trim()));
		}

		ZQuery DoNothing(ZGuid value)
		{
			return new ZQuery();
		}

		ZQuery DoNothingWithOperator(SQLComparisonOperator comparisonOperator, object value)
		{
			return new ZQuery();
		}
		ZQuery DoNothingWithOperatorSupportsFilterMatching(ZDBOnlySubQuery query, SQLComparisonOperator comparisonOperator, object value)
		{
			return new ZQuery();
		}

		ZQuery DoNothingWithOperatorAndOption(SQLComparisonOperator comparisonOperator, object value, ZString option)
		{
			return new ZQuery();
		}

		#endregion

		#region TestTemplateFilterQuery

		public void TestTemplateFilterQuery()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			var filter = new ModuleGuidFilter("moo", ModuleIDs.JobShipment, OrgAddressSchema.OA_OH, list);

			AssertEquals("", filter.XQuery.LiteralTextADO);

			filter.PropertyCode = "aaa";
			filter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.PickupAgent, OrgHeaderSchema.OH_Code.MaxLength);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			AssertEquals("STR_Data.value('declare default element namespace \"http://www.cargowise.com/Schemas/Universal/2011/11\"; (UniversalShipment/Shipment/OrganizationAddressCollection/OrganizationAddress[./AddressType=\"PickupAgent\"]/OrganizationCode)[1]', 'varchar(12)') = 'aaa'",
				filter.XQuery.LiteralTextADO);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			AssertEquals("STR_Data.value('declare default element namespace \"http://www.cargowise.com/Schemas/Universal/2011/11\"; (UniversalShipment/Shipment/OrganizationAddressCollection/OrganizationAddress[./AddressType=\"PickupAgent\"]/OrganizationCode)[1]', 'varchar(12)') <> 'aaa'",
				filter.XQuery.LiteralTextADO);
		}

		#endregion

		#region TestQueryContainsDBNull

		public void TestQueryContainsDBNull()
		{
			Filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			Assert(!Filter.HasComparisonOperator || Filter.Query.LiteralTextADO.Contains("is null"));
			Filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			Assert(!Filter.HasComparisonOperator || Filter.Query.LiteralTextADO.Contains("is not null"));
		}

		#endregion

		#region TestDefaultSqlComparisonOperator

		public void TestDefaultSqlComparisonOperator()
		{
			AssertEquals(ModuleGuidFilter.ComparisonConstants.Exact, Filter.GetComparisonOperatorDefault());
			AssertEquals(SQLComparisonOperator.Equal, Filter.SqlComparisonOperator);

			Filter.ComparisonOperator = "Caecilius";
			AssertEquals("Setting the comparison operator to something invalid should not affect sql comparison operator.", SQLComparisonOperator.Equal, Filter.SqlComparisonOperator);
		}

		#endregion

		#region TestSetValueFromInitialCode
		public void TestSetValueFromInitialCode()
		{
			Filter.Visibility = FilterVisibility.Visible;
			AssertEquals("Initial code should be set", true, Filter.SetValueFromInitialCode("Z0_Guid", "initial code"));
			AssertEquals("Filter vibility set to always visible", FilterVisibility.AlwaysVisible, Filter.Visibility);

			Filter.Visibility = FilterVisibility.Visible;
			AssertEquals("Initial code should not be set", false, Filter.SetValueFromInitialCode("Z0_Code", "some code"));
			AssertEquals("Filter not set", FilterVisibility.Visible, Filter.Visibility);
		}

		public void TestShouldSetValueFromInitialCode()
		{
			AssertEquals("Initial code should be set", true, Filter.ShouldSetValueFromInitialCode("Z0_Guid", "initial code"));

			AssertEquals("Initial code should not be set", false, Filter.ShouldSetValueFromInitialCode("Whatever", "some code"));
		}

		public void TestGetFormattedInitialCode_FilterColumnName()
		{
			var propertyValue = new ZGuid("8C658DF5-7079-4BF1-B673-62A994B84A8D");
			Filter.Property = propertyValue;
			AssertEquals("Initial code is the property value", propertyValue.ToString(), Filter.GetFormattedInitialCode_FilterColumnName());
		}

		#endregion

		#region TestNullFilterColumnForDelegateSupportingFiltersMatch

		public void TestNullFilterForDelegateSupportingFiltersMatch()
		{
			var list = new DummyBusinessObjectCollection(Factory);
			AssertExceptionThrown<ArgumentNullException>(() => new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperatorSupportsFiltersMatch((a, b, c) => new ZQuery()), list, null));
			AssertExceptionThrown<ArgumentNullException>(() => new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperatorSupportsFiltersMatch((a, b, c) => new ZQuery()), () => list, null));
			AssertNoExceptionThrown(() => new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperatorSupportsFiltersMatch((a, b, c) => new ZQuery()), list, DummyBizoSchema.Z0_Guid));
			AssertNoExceptionThrown(() => new ModuleGuidFilter("DESC", DummyModuleIDs.Dummy, new GetGuidQueryWithOperatorSupportsFiltersMatch((a, b, c) => new ZQuery()), () => list, DummyBizoSchema.Z0_Guid));
		}

		#endregion

		#region Implementation

		protected override ModuleGuidFilter GetNewModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			return new ModuleGuidFilter("moo", ModuleIDs.JobShipment, DummyBizoSchema.Z0_Guid, list);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		protected override string FilterDescriptionForFiltersMatchTest => "Dummy";

		public static FilterStripBusinessObject GetSelectedFiltersBusinessObjectForTest()
		{
			var filterBizo = new DummyDependentFilterBusinessObject();
			var filter = filterBizo.AddGuidFilterStrip("Dummy");
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Z0_Description", "Keokuk");

			return filterBizo;
		}

		#region DummyModuleGuidFilter

		public class DummyModuleGuidFilter : ModuleGuidFilter
		{
			public DummyModuleGuidFilter(BusinessObjectFactory factory)
				: base("DummyGuidFilter", ModuleIDs.Organisation, DummyBizoSchema.Z0_Guid, new StmNoteNonDependentCollection(factory))
			{
			}

			protected override void SerializePropertiesToXml(XmlWriter writer)
			{
				//writer.WriteElementString("Property", Property.ToString());
			}
		}

		#endregion

		#endregion
	}
}
