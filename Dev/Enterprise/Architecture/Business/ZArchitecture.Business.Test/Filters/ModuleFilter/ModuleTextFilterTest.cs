using System.Collections.Generic;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.ZArchitecture.Business.ModuleFilterWithListAndComparisonOperators<CargoWise.Types.ZString>;

[assembly: UsesConstants(typeof(ShipmentXQueryPaths))]

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class ModuleTextFilterTest : ModuleFilterTestCase<ModuleTextFilter>
	{
		#region TestCopyPersistantValuesFromFilter

		public void TestCopyPersistantValuesFromFilter()
		{
			var filterToCopy = new ModuleTextFilter("Z0_Description", DummyBizoSchema.Z0_Description);
			filterToCopy.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filterToCopy.Property = "100";
			var filter = new ModuleFountainFilter("Z0_Description", DummyBizoSchema.Z0_Description, "S");
			filter.CopyPersistantValuesFromFilterForTest(filterToCopy);
			AssertEquals("No expand value", filterToCopy.Property, filter.Property);
		}

		#endregion

		#region TestTemplateFilterQuery

		public void TestTemplateFilterQuery()
		{
			var filter = new ModuleTextFilter("moo", DummyBizoSchema.Z0_Description);
			AssertEquals("", filter.XQuery.LiteralTextADO);

			filter.Property = "aa";
			filter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.TransportMode, JobShipmentSchema.JS_TransportMode.MaxLength);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			AssertEquals("STR_Data.value('declare default element namespace \"http://www.cargowise.com/Schemas/Universal/2011/11\"; (UniversalShipment/Shipment/TransportMode/Code)[1]', 'varchar(3)') = 'aa'",
				filter.XQuery.LiteralTextADO);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			AssertEquals("STR_Data.value('declare default element namespace \"http://www.cargowise.com/Schemas/Universal/2011/11\"; (UniversalShipment/Shipment/TransportMode/Code)[1]', 'varchar(3)') like 'aa%'",
				filter.XQuery.LiteralTextADO);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			AssertEquals("STR_Data.value('declare default element namespace \"http://www.cargowise.com/Schemas/Universal/2011/11\"; (UniversalShipment/Shipment/TransportMode/Code)[1]', 'varchar(3)') like '%aa%'",
				filter.XQuery.LiteralTextADO);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			AssertEquals("STR_Data.value('declare default element namespace \"http://www.cargowise.com/Schemas/Universal/2011/11\"; (UniversalShipment/Shipment/TransportMode/Code)[1]', 'varchar(3)') <> 'aa'",
				filter.XQuery.LiteralTextADO);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			AssertEquals("STR_Data.value('declare default element namespace \"http://www.cargowise.com/Schemas/Universal/2011/11\"; (UniversalShipment/Shipment/TransportMode/Code)[1]', 'varchar(3)') not like 'aa%'",
				filter.XQuery.LiteralTextADO);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			AssertEquals("STR_Data.value('declare default element namespace \"http://www.cargowise.com/Schemas/Universal/2011/11\"; (UniversalShipment/Shipment/TransportMode/Code)[1]', 'varchar(3)') not like '%aa%'",
				filter.XQuery.LiteralTextADO);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertEquals("STR_Data.value('declare default element namespace \"http://www.cargowise.com/Schemas/Universal/2011/11\"; (UniversalShipment/Shipment/TransportMode/Code)[1]', 'varchar(3)') = ''",
				filter.XQuery.LiteralTextADO);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertEquals("STR_Data.value('declare default element namespace \"http://www.cargowise.com/Schemas/Universal/2011/11\"; (UniversalShipment/Shipment/TransportMode/Code)[1]', 'varchar(3)') <> ''",
				filter.XQuery.LiteralTextADO);
		}

		#endregion

		#region TestSqlComparisonOperatorWithDelegateAndList

		public void TestSqlComparisonOperatorWithDelegateAndList()
		{
			var filter = new ModuleTextFilter("Hello",
				(comparisonOperator, value) => new ZQuery(DummyBizoSchema.Z0_Code, comparisonOperator, value), new CodeDescriptionPairList());
			filter.Property = "Y";
			filter.ComparisonOperator = Enterprise.ZArchitecture.Business.ModuleTextBaseFilter.ComparisonConstants.NotEqual;

			AssertEquals(DummyBizoSchema.Z0_Code.Name + " <> 'Y'", filter.Query.LiteralTextADO);
		}

		#endregion

		#region Property trimming

		public void TestProperty()
		{
			Filter.Property = "HELLO ";
			Assert(Filter.Property.EndsWith("HELLO"));
		}

		#endregion

		#region TestPropertyValidation

		public void TestPropertyValidation()
		{
			var errorText = "You cant hug your children with atomic arms!";
			Filter.PropertyValidation = null;
			Filter.Property = ZString.Empty;

			Filter.Validation.ValidateProperty();
			AssertNoError(Filter.PropertyInfo, errorText);

			Filter.PropertyValidation = info =>
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(errorText);
				}
			};

			Filter.Validation.ValidateProperty();
			AssertHasError(Filter.PropertyInfo, errorText);
		}

		public void TestListValidationForCollectionCodeDescList()
		{
			var dummyList = new DummyBizoCollectionThatImplementsICodeDescPairList(Factory);
			var textFilterWithList = new ModuleTextFilter("moo2", DummyBizoSchema.Z0_Description, dummyList);

			textFilterWithList.Property = "Hello";
			AssertNoErrors(textFilterWithList.PropertyInfo);

			textFilterWithList.Property = "Crap";
			AssertHasWarnings(textFilterWithList.PropertyInfo);

			textFilterWithList.ErrorOnCodeNotPresent = true;
			textFilterWithList.Property = "Crapper";
			AssertHasErrors(textFilterWithList.PropertyInfo);

			textFilterWithList.Property = "Goodbye";
			AssertNoErrors(textFilterWithList.PropertyInfo);
		}

		class DummyBizoCollectionThatImplementsICodeDescPairList : NonPersistentBusinessObjectCollection<DummyObjectForTest>, ICodeDescriptionPairList
		{
			public DummyBizoCollectionThatImplementsICodeDescPairList(BusinessObjectFactory factory)
				: base(factory)
			{
				Add(new DummyObjectForTest("Hello"));
				Add(new DummyObjectForTest("Goodbye"));
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new DummyObjectForTest("");
			}

			#region ICodeDescriptionPairList Members

			bool ICodeDescriptionPairList.ContainsCode(object code)
			{
				foreach (DummyObjectForTest dummy in this)
				{
					if (dummy.MyCode == (ZString)code)
					{
						return true;
					}
				}
				return false;
			}

			string ICodeDescriptionPairList.GetDescriptionFromCode(string code)
			{
				return "";
			}

			#endregion
		}

		[CodeProperty("MyCode")]
		class DummyObjectForTest : NonPersistentBusinessObject
		{
			public DummyObjectForTest(ZString code)
			{
				MyCode = code;
			}

			public ZString MyCode
			{
				get { return fMyCode; }
				set { fMyCode = value; }
			}

			ZString fMyCode;
		}

		#endregion

		#region TestComparisonFilterValidation

		public void TestComparisonFilterValidation()
		{
			if (Filter.ComparisonOperator != ComparisonConstants.StartsWith)
			{
				//some subclasses have a fixed or different list of ComparisonOperators
				Assert(true);
				return;
			}
			Filter.ContainsBanned = true;
			Filter.ComparisonOperator = ComparisonConstants.Contains;
			AssertHasErrors(Filter.ComparisonOperatorInfo);
			Filter.ComparisonOperator = ComparisonConstants.StartsWith;
			AssertNoErrors(Filter.ComparisonOperatorInfo);
			Filter.ComparisonOperator = ComparisonConstants.NotContain;
			AssertHasErrors(Filter.ComparisonOperatorInfo);
			Filter.ContainsBanned = false;
			Filter.ComparisonOperator = ComparisonConstants.Contains;
			AssertNoErrors(Filter.ComparisonOperatorInfo);
		}

		#endregion

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			// without filter data
			Filter.Property = "";

			Filter.ComparisonOperator = "starts with";
			AssertEquals(false, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "contains";
			AssertEquals(false, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "exact";
			AssertEquals(false, Filter.IsExpensiveQuery);

			// with filter data
			Filter.Property = "cell";

			Filter.ComparisonOperator = "starts with";
			AssertEquals(false, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "contains";
			AssertEquals(true, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "exact";
			AssertEquals(false, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "not starting";
			AssertEquals(false, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "not contain";
			AssertEquals(true, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "not equal";
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestDefaultComparisonOperatorIsStartsWith

		public void TestDefaultComparisonOperator()
		{
			AssertEquals(Filter.ComparisonOperator, DefaultComparisonOperator);
		}

		protected virtual string DefaultComparisonOperator
		{
			get
			{
				if (Filter.ComparisonOperator_List.ContainsCode(ModuleTextBaseFilter.ComparisonConstants.StartsWith))
				{
					return ModuleTextBaseFilter.ComparisonConstants.StartsWith;
				}
				else
				{
					return Filter.ComparisonOperator_List[0].GetMultilingualCode();
				}
			}
		}

		#endregion

		#region TestSettingInvalidComparisonOperatorRevertsToStartsWith

		public void TestSettingInvalidComparisonOperatorRevertsToStartsWith()
		{
			Filter.ComparisonOperator = "crap";
			AssertEquals(Filter.SqlComparisonOperator, SQLComparisonOperator.StartsWith);
		}

		#endregion

		#region TestClearResetsToDefaults

		public void TestClearResetsToDefaults()
		{
			ZString defaultValue = "moo";

			AssertEquals("Precondition", "", Filter.Property);
			AssertEquals("Precondition", "", Filter.DefaultProperty);

			Filter.DefaultProperty = defaultValue;
			AssertEquals(defaultValue, Filter.Property);

			Filter.Property = "";
			AssertEquals("Precondition", "", Filter.Property);

			Filter.Clear();
			AssertEquals(defaultValue, Filter.Property);
			AssertEquals(defaultValue, Filter.DefaultProperty);

			// test Clear when ReadOnly
			Filter.Property = "";
			Filter.ReadOnly = true;
			Filter.Clear();
			AssertEquals("Filter was ReadOnly thus Clear() should have no effect.", "", Filter.Property);
		}

		public virtual void TestClearSetsCorrectComparisonOperator()
		{
			Filter.Clear();

			AssertEquals("Precondition", "", Filter.Property);
			AssertEquals("Precondition", "", Filter.DefaultProperty);
			AssertEquals("Precondition", ModuleTextFilter.ComparisonConstants.StartsWith, Filter.ComparisonOperator);

			Filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			Filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			Filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			Filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			Filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			Filter.Clear();
			AssertEquals(ModuleTextFilter.ComparisonConstants.StartsWith, Filter.ComparisonOperator);

			Filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Exact);
			Filter.Clear();
			AssertEquals(ModuleTextFilter.ComparisonConstants.StartsWith, Filter.ComparisonOperator);
		}

		#endregion

		#region TestDeserializePropertiesFromXml

		public void TestDeserializePropertiesFromXml()
		{
			var filter = new DummyModuleTextFilterWithNoSerialization();
			filter.ComparisonOperator = SQLComparisonOperator.Contains.ToString();
			filter.Property = "someValue";

			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			strip.FilterDescription = "";
			strip.Delete();
			filterStripBizO.LoadLayout(savedFilter);
			var loadedFilter = (ModuleTextFilter)filterStripBizO[filter.Description];

			AssertEquals("Contains", loadedFilter.ComparisonOperator);
			AssertEquals("", loadedFilter.Property);
		}

		#endregion

		#region TestDeserializeComparerWhenComparerIsNotSupported

		public void TestDeserializeComparerWhenComparerIsNotSupported()
		{
			var moduleFilter = new DummyModuleTextFilter();
			moduleFilter.ComparisonOperator = SQLComparisonOperator.Contains.ToString();
			moduleFilter.Property = "hello";

			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.AddModuleFilterForTest(moduleFilter);

			var strip = filterStripBizO.FilterStrips.AddNew(moduleFilter.Description);
			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			strip.FilterDescription = "";
			strip.Delete();
			moduleFilter.SupportsComparisonOperatorSetForTest = false; // ensure comparisonOp is not deserialized
			filterStripBizO.LoadLayout(savedFilter);
			var loadedFilter = (ModuleTextFilter)filterStripBizO[moduleFilter.Description];

			AssertEquals("Setting the ComparisonOperator is not supported and should *not* have been deserialized.", "Contains", loadedFilter.ComparisonOperator);
		}

		#endregion

		#region SqlComparisonOperator

		public virtual void TestSqlComparisonOperator()
		{
			Filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertEquals(ModuleTextBaseFilter.ComparisonConstants.IsBlank, Filter.ComparisonOperator);

			Filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertEquals(ModuleTextBaseFilter.ComparisonConstants.IsNotBlank, Filter.ComparisonOperator);
		}

		public void TestGetComparisonOperatorDefault_ShouldReturnDefaultInEnglish()
		{
			Filter.ComparisonOperator_List.Clear();
			var contains = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair("contains");
			Filter.ComparisonOperator_List.Add(contains);

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			{
				var message = "\n" +
					"Given a list comparison operators that does not contain the default 'starts with' \n" +
					"And the language is set to another language aside from English (Chinese in this test) \n" +
					"When trying to get the Comparison Operator to display by calling its property \n" +
					"Then the first item in the list IN ENGLISH should be returned \n";
				AssertEquals(message, contains.Code, Filter.ComparisonOperator);
			}
		}

		#endregion

		#region TestRemoveComparisonOperatorLeavingOne

		public void TestRemoveComparisonOperatorLeavingOne()
		{
			Filter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith);
			Filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			AssertEquals("ComparisonOperator should only be readonly when one is available", false, Filter.ComparisonOperatorInfo.ReadOnly);
			Filter.RemoveComparisonOperatorsLeavingOne(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith);
			AssertEquals("RemoveComparisonOperatorsLeavingOne should only leave one operator", 1, Filter.ComparisonOperator_List.Count);
			AssertEquals("ComparisonOperator should only be readonly when only one is available", true, Filter.ComparisonOperatorInfo.ReadOnly);

			var comparisionOperator = Filter.ComparisonOperator_List[0];
			var code = comparisionOperator.GetMultilingualCode();
			var description = comparisionOperator.GetMultilingualDescription();
			AssertNotNull(code);
			Assert("Code should be a Multilingual String", !(code is NoResString));
			AssertNotNull(description);
			Assert("Description should be a Multilingual String", !(description is NoResString));
			AssertEquals(code.GetUnresolvedString(), ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith);

			Filter.ComparisonOperator_List.Clear();
		}

		#endregion

		#region TestAllowedComparisonOperators

		public void TestAllowedComparisonOperators()
		{
			var filter = new DummyModuleTextFilter();
			Assert(filter.AllowedComparisonOperators.Count > 0);
			var contains = false;
			foreach (var entry in filter.AllowedComparisonOperators)
			{
				if (entry == filter.ComparisonOperator)
				{
					contains = true;
					break;
				}
			}
			Assert("default value should be allowed", contains);

			filter = new DummyModuleTextFilter();
			filter.OverrideAllowedComparisonOperators = true;
			AssertEquals(0, filter.AllowedComparisonOperators.Count);
			AssertEquals(0, filter.ComparisonOperator_List.Count);

			filter = new DummyModuleTextFilter();
			filter.OverrideAllowedComparisonOperators = true;
			filter.OverriddenAllowedComparisonOperators.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
			AssertEquals(1, filter.AllowedComparisonOperators.Count);
			AssertEquals(1, filter.ComparisonOperator_List.Count);
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains;
			AssertEquals("Not included in allowed, should return to 'exact'.", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, filter.ComparisonOperator);
		}

		public void TestAllowedComparisonOperators_ForIsCommonModuleFilter()
		{
			var filter = new DummyModuleTextFilter(new ModuleFilterCollection());
			Assert(filter.AllowedComparisonOperators.Count > 0);
			var contains = false;
			foreach (var entry in filter.AllowedComparisonOperators)
			{
				if (entry == ModuleTextBaseFilter.ComparisonConstants.IsBlank || entry == ModuleTextBaseFilter.ComparisonConstants.IsNotBlank)
				{
					contains = true;
					break;
				}
			}
			Assert("should not contain is blank / not blank", !contains);
		}

		public void TestIsBlankOrNotBlankMakesPropertyReadOnly()
		{
			var filter = new DummyModuleTextFilter();
			filter.OverrideAllowedComparisonOperators = true;
			filter.OverriddenAllowedComparisonOperators.Add(ModuleTextFilter.ComparisonConstants.IsBlank);
			filter.OverriddenAllowedComparisonOperators.Add(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			filter.OverriddenAllowedComparisonOperators.Add(ModuleTextFilter.ComparisonConstants.Exact);
			filter.OverriddenAllowedComparisonOperators.Add(ModuleTextFilter.ComparisonConstants.Contains);
			filter.Property = "Anton";

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			AssertComparisonOperator(filter, ModuleTextFilter.ComparisonConstants.Exact, SQLComparisonOperator.Equal, "Anton", false);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertComparisonOperator(filter, ModuleTextFilter.ComparisonConstants.IsBlank, SpecialComparisonOperator.IsBlank, "", true);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			AssertComparisonOperator(filter, ModuleTextFilter.ComparisonConstants.Contains, SQLComparisonOperator.Contains, "Anton", false);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			AssertComparisonOperator(filter, ModuleTextFilter.ComparisonConstants.IsNotBlank, SpecialComparisonOperator.IsNotBlank, "", true);
		}

		void AssertComparisonOperator(ModuleTextFilter filter, string comparisonOperator, SQLComparisonOperator sqlOperator, string property, bool readOnly)
		{
			AssertEquals(comparisonOperator, filter.ComparisonOperator);
			AssertEquals(sqlOperator.GetType(), filter.SqlComparisonOperator.GetType());
			AssertEquals(property, filter.Property);
			AssertEquals(readOnly, filter.PropertyInfo.ReadOnly);
		}

		#endregion

		#region TestComparisonOperatorDefault

		public void TestComparisonOperatorDefault()
		{
			var filter = new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code);
			filter.ComparisonOperator_List.DefaultCode = null;
			AssertEquals(ModuleTextFilter.ComparisonConstants.Default, filter.ComparisonOperator);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			AssertEquals(ModuleTextFilter.ComparisonConstants.Exact, filter.ComparisonOperator);
			filter.Clear();
			AssertEquals(ModuleTextFilter.ComparisonConstants.Exact, filter.ComparisonOperator);
			filter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Clear();
			AssertEquals(ModuleTextFilter.ComparisonConstants.Exact, filter.ComparisonOperator);
			filter.ComparisonOperator = ZString.Empty;
			AssertEquals(ModuleTextFilter.ComparisonConstants.NotEqual, filter.ComparisonOperator);
			filter.ComparisonOperator_List.DefaultCode = "ZZZ";
			filter.Clear();
			AssertEquals(ModuleTextFilter.ComparisonConstants.NotEqual, filter.ComparisonOperator);
			filter.ComparisonOperator = ZString.Empty;
			AssertEquals(ModuleTextFilter.ComparisonConstants.StartsWith, filter.ComparisonOperator);
		}

		#endregion

		#region TestSetValueFromInitialCode

		public void TestSetValueFromInitialCode_FilterColumnName()
		{
			var filter = new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code);

			filter.Property = "ABC";
			AssertEquals("Initial code should not be set", false, filter.SetValueFromInitialCode("Z0_VarCharMax", "XYZ"));
			AssertEquals("ABC", filter.Property);

			AssertEquals("Initial code should be set", true, filter.SetValueFromInitialCode("Z0_Code", "XYZ"));
			AssertEquals("XYZ", filter.Property);
		}

		public void TestSetValueFromInitialCode_Prefix()
		{
			var filter = new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code);

			filter.Property = "ABC";
			AssertEquals("Initial code should be set", true, filter.SetValueFromInitialCode("Z0_Code", "A:XYZ"));
			AssertEquals("Initial code set from property", "A:XYZ", filter.Property);

			filter.Prefix = "A";
			filter.Property = "ABC";
			AssertEquals("Initial code should be set", true, filter.SetValueFromInitialCode("Z0_Code", "A:XYZ"));
			AssertEquals("Initial code set from initial code suffix", "XYZ", filter.Property);

			filter.Property = "ABC";
			AssertEquals("Initial code should be set", true, filter.SetValueFromInitialCode("Whatever", "A:XYZ"));
			AssertEquals("Initial code set from initial code suffix regardless of filter column name", "XYZ", filter.Property);

			filter.Property = "ABC";
			AssertEquals("Initial code should not be set", false, filter.SetValueFromInitialCode("Whatever", "B:XYZ"));
			AssertEquals("ABC", filter.Property);
		}

		public void TestShouldSetValueFromInitialCode_FilterColumnName()
		{
			var filter = new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code);

			AssertEquals("Initial code should not be set", false, filter.ShouldSetValueFromInitialCode("Z0_VarCharMax", "XYZ"));

			AssertEquals("Initial code should be set", true, filter.ShouldSetValueFromInitialCode("Z0_Code", "XYZ"));
		}

		public void TestShouldSetValueFromInitialCode_Prefix()
		{
			var filter = new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code);

			AssertEquals("Initial code should be set", true, filter.ShouldSetValueFromInitialCode("Z0_Code", "A:XYZ"));

			filter.Prefix = "A";
			AssertEquals("Initial code should be set", true, filter.ShouldSetValueFromInitialCode("Z0_Code", "A:XYZ"));

			AssertEquals("Initial code should be set", true, filter.ShouldSetValueFromInitialCode("Whatever", "A:XYZ"));

			AssertEquals("Initial code should not be set", false, filter.ShouldSetValueFromInitialCode("Whatever", "B:XYZ"));
		}

		public void TestGetFormattedInitialCode_FilterColumnName()
		{
			var filter = new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code);
			var propertyValue = "Property Value";
			filter.Property = propertyValue;

			AssertEquals("Initial code is the property value", propertyValue, filter.GetFormattedInitialCode_FilterColumnName());
		}

		public void TestGetFormattedInitialCode_Prefix()
		{
			var filter = new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code);
			var propertyValue = "Property Value";
			filter.Property = propertyValue;
			filter.Prefix = "A";
			var expectedInitialCode = filter.Prefix + ":" + filter.Property;

			AssertEquals("Initial code should be the prefix and the property value", expectedInitialCode, filter.GetFormattedInitialCode_Prefix());
		}
		#endregion

		#region Filters Match

		public void TestFiltersMatch_ShouldNotBeAListedOption()
		{
			var filter = new DummyModuleTextFilter();
			Assert(filter.AllowedComparisonOperators.Count > 0);
			AssertCollectionNotContains("ModuleTextFilters don't support 'filters match' by default (though some of their child classes like ModuleNkFilter do), and yet...", ModuleTextFilter.ComparisonConstants.FiltersMatch, filter.AllowedComparisonOperators);
		}

		#endregion

		#region Sparse Column

		public void TestSparseColumnQuery()
		{
			var filter = new ModuleTextFilter("Filter", DummyBizoSchema.Z0_SparseNVarChar);
			filter.Property = "Property Value";

			var operatorsToTest = new[] { SQLComparisonOperator.NotEqual, SQLComparisonOperator.NotContains, SQLComparisonOperator.DoesNotStartWith };
			foreach (var comparisonOperator in operatorsToTest)
			{
				filter.SqlComparisonOperator = comparisonOperator;
				AssertContains("Query should contain \" or Z0_SparseNVarChar is NULL\"", " or Z0_SparseNVarChar is NULL", filter.Query.FilterString, true);
			}

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			AssertEquals("Query should equal to \"Z0_SparseNVarChar is NULL\"", "Z0_SparseNVarChar is NULL", filter.Query.FilterString, true);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			AssertEquals("Query should equal to \"Z0_SparseNVarChar is not NULL\"", "Z0_SparseNVarChar is not NULL", filter.Query.FilterString, true);
		}

		#endregion

		#region Implementation

		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new ModuleTextFilter("moo", DummyBizoSchema.Z0_Description);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		#endregion

		#region class DummyModuleTextFilter, DummyModuleTextFilterWithNoSerialization

		public class DummyModuleTextFilter : ModuleTextFilter
		{
			public DummyModuleTextFilter()
				: base("DummyTextFilter", DummyBizoSchema.Z0_Description)
			{
			}

			public DummyModuleTextFilter(ModuleFilterCollection parentCollection)
				: base(FilterCategories.NumbersAndReferences, parentCollection)
			{
			}

			protected override bool SupportsComparisonOperatorSet
			{
				get { return SupportsComparisonOperatorSetForTest; }
			}

			public bool SupportsComparisonOperatorSetForTest = true;

			public bool OverrideAllowedComparisonOperators { get; set; }

			public List<string> OverriddenAllowedComparisonOperators = new List<string>();

			public override IReadOnlyList<string> AllowedComparisonOperators
			{
				get
				{
					return OverrideAllowedComparisonOperators ? OverriddenAllowedComparisonOperators : base.AllowedComparisonOperators;
				}
			}
		}

		sealed class DummyModuleTextFilterWithNoSerialization : ModuleTextFilter
		{
			public DummyModuleTextFilterWithNoSerialization()
				: base("DummyModuleTextFilterWithNoSerialization", DummyBizoSchema.Z0_Description)
			{
			}

			protected override void SerializePropertiesToXml(XmlWriter writer)
			{
				// don't call base
			}
		}

		#endregion

	}
}
