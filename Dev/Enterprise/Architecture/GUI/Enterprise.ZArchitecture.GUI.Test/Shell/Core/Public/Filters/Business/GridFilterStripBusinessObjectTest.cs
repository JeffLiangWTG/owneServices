using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(GridFilterStripBusinessObject))]
	public class GridFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestSimpleFilters

		public void TestSimpleFilters()
		{
			using (var control = new ZUserControl())
			using (var grid = new ZGrid())
			{
				control.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Date", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Number", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Bool", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_DateTimeOffset", 80));

				grid.SetDataBinding(new DummyBusinessObjectCollection(Factory), "");

				var filters = new GridFilterStripBusinessObject(grid).GetModuleFilters();
				AssertEquals(5, filters.Count());

				AssertModuleFilter("Description", filters["Z0_Description"], typeof(ModuleTextFilter));
				AssertModuleFilter("Date", filters["Z0_Date"], typeof(ModuleDateFilter));
				AssertModuleFilter("Number", filters["Z0_Number"], typeof(ModuleNumberRangeFilter));
				AssertModuleFilter("Flags", filters["Flags"], typeof(ModuleFlagsFilter));
				AssertModuleFilter("Date (With Offset)", filters["Z0_DateTimeOffset"], typeof(ModuleDateTimeOffsetFilter));

				AssertEquals(1, ((ModuleFlagsFilter)filters["Flags"]).FlagNames.Length);
				AssertEquals("Flag", ((ModuleFlagsFilter)filters["Flags"]).FlagNames[0]);
			}
		}

		public void TestShouldNotIncludeSensitiveValue()
		{
			using (var control = new ZUserControl())
			using (var grid = new ZGrid())
			{
				control.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Date", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Number", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Bool", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_DateTimeOffset", 80));
				var sensitiveColInfo = new ZTextBoxColumnStyleInfo { ColumnName = "Z0_NVarChar", Caption = "SensitiveValue", PasswordChar = '*' };
				grid.ColumnStyles.Add(sensitiveColInfo);

				grid.SetDataBinding(new DummyBusinessObjectCollection(Factory), "");

				var filters = new GridFilterStripBusinessObject(grid).GetModuleFilters();
				AssertEquals(5, filters.Count());

				AssertModuleFilter("Description", filters["Z0_Description"], typeof(ModuleTextFilter));
				AssertModuleFilter("Date", filters["Z0_Date"], typeof(ModuleDateFilter));
				AssertModuleFilter("Number", filters["Z0_Number"], typeof(ModuleNumberRangeFilter));
				AssertModuleFilter("Flags", filters["Flags"], typeof(ModuleFlagsFilter));
				AssertModuleFilter("Date (With Offset)", filters["Z0_DateTimeOffset"], typeof(ModuleDateTimeOffsetFilter));
				AssertNull("SensitiveValue should not be included", filters["Z0_NVarChar"]);

				AssertEquals(1, ((ModuleFlagsFilter)filters["Flags"]).FlagNames.Length);
				AssertEquals("Flag", ((ModuleFlagsFilter)filters["Flags"]).FlagNames[0]);
			}
		}

		#endregion

		#region TestFiltersOnNonPersistentCollection

		public void TestFiltersOnNonPersistentCollection()
		{
			using (var control = new ZUserControl())
			using (var grid = new ZGrid())
			{
				control.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Text", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Date", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Number", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("DateTimeOffset", 80));

				grid.SetDataBinding(new NonPersistentBizoWithPropertiesCollection(), "");

				var filters = new GridFilterStripBusinessObject(grid).GetModuleFilters();
				AssertEquals(5, filters.Count());

				AssertModuleFilter("Text", filters["Text"], typeof(ModuleTextFilter));
				AssertModuleFilter("Date", filters["Date"], typeof(ModuleDateFilter));
				AssertModuleFilter("Number", filters["Number"], typeof(ModuleNumberRangeFilter));
				AssertModuleFilter("Flags", filters["Flags"], typeof(ModuleFlagsFilter));
				AssertModuleFilter("DateTimeOffset", filters["DateTimeOffset"], typeof(ModuleDateTimeOffsetFilter));

				AssertEquals(1, ((ModuleFlagsFilter)filters["Flags"]).FlagNames.Length);
				AssertEquals("Bool", ((ModuleFlagsFilter)filters["Flags"]).FlagNames[0]);

				((ModuleTextFilter)filters["Text"]).Property = "A";
				((ModuleNumberRangeFilter)filters["Number"]).Property1 = 10;
				((ModuleNumberRangeFilter)filters["Number"]).Property1 = 20;
			}
		}

		public void TestFiltersOnNonPersistentCollectionIntegration()
		{
			using (var control = new ZUserControl())
			using (var grid = new ZGrid())
			{
				var collection = new NonPersistentBizoWithPropertiesCollection();

				control.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Text", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Number", 80));
				grid.SetDataBinding(collection, "");

				var filters = new GridFilterStripBusinessObject(grid).GetModuleFilters();
				AssertEquals(2, filters.Count());
				AssertModuleFilter("Text", filters["Text"], typeof(ModuleTextFilter));
				AssertModuleFilter("Number", filters["Number"], typeof(ModuleNumberRangeFilter));

				((ModuleTextFilter)filters["Text"]).Property = "A";
				((ModuleNumberRangeFilter)filters["Number"]).Property1 = 10;
				((ModuleNumberRangeFilter)filters["Number"]).Property2 = 20;

				var query = filters.GetFilterQuery(filters);

				var bizo1 = collection.AddNew();
				bizo1.Text = "ABC";
				bizo1.Number = 15;
				AssertMatchesFilter(bizo1, query, true);

				var bizo2 = collection.AddNew();
				bizo2.Text = "ABC";
				bizo2.Number = 25;
				AssertMatchesFilter(bizo2, query, false);

				var bizo3 = collection.AddNew();
				bizo3.Text = "XYZ";
				bizo3.Number = 15;
				AssertMatchesFilter(bizo3, query, false);
			}
		}

		void AssertMatchesFilter(BusinessObject bizo, ZQuery query, bool matches)
		{
			((ISupportMainElement)query).SetMainElement(bizo);
			AssertEquals(matches, bizo.MatchesFilter(query));
		}

		class NonPersistentBizoWithProperties : NonPersistentBusinessObject
		{
			public ZString Text { get; set; }
			public ZPropertyInfo TextInfo { get { return GetZPropertyInfo(nameof(Text)); } }

			public ZDateTime Date { get; set; }
			public ZPropertyInfo DateInfo { get { return GetZPropertyInfo(nameof(Date)); } }

			public ZDateTimeOffset DateTimeOffset { get; set; }
			public ZPropertyInfo DateTimeOffsetInfo { get { return GetZPropertyInfo(nameof(DateTimeOffset)); } }

			public ZInt Number { get; set; }
			public ZPropertyInfo NumberInfo { get { return GetZPropertyInfo(nameof(Number)); } }

			public ZBool Bool { get; set; }
			public ZPropertyInfo BoolInfo { get { return GetZPropertyInfo(nameof(Bool)); } }
		}

		class NonPersistentBizoWithPropertiesCollection : NonPersistentBusinessObjectCollection<NonPersistentBizoWithProperties>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new NonPersistentBizoWithProperties();
			}
		}

		#endregion

		#region TestFilterOnPersistentBizoWithNonPersistentProperty

		public void TestFilterOnPersistentBizoWithNonPersistentProperty()
		{
			using (var control = new ZUserControl())
			using (var grid = new ZGrid())
			{
				var collection = new CalculatedDummyCollection(Factory);

				control.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CalculatedProperty", 80));
				grid.SetDataBinding(collection, "");

				var filters = new GridFilterStripBusinessObject(grid).GetModuleFilters();
				AssertEquals(2, filters.Count());
				AssertModuleFilter("Code", filters["Z0_Code"], typeof(ModuleTextFilter));
				AssertModuleFilter("CalculatedProperty", filters["CalculatedProperty"], typeof(ModuleNumberRangeFilter));

				((ModuleTextFilter)filters["Z0_Code"]).Property = "A";
				((ModuleNumberRangeFilter)filters["CalculatedProperty"]).Property1 = 10;

				var query = filters.GetFilterQuery(filters);

				var dummy = collection.AddNew();
				dummy.Z0_Code = "A";
				dummy.CalculatedProperty = 0;
				AssertMatchesFilter(dummy, query, false);

				dummy = collection.AddNew();
				dummy.Z0_Code = "B";
				dummy.CalculatedProperty = 10;
				AssertMatchesFilter(dummy, query, false);

				dummy = collection.AddNew();
				dummy.Z0_Code = "A";
				dummy.CalculatedProperty = 10;
				AssertMatchesFilter(dummy, query, true);

				dummy = collection.AddNew();
				dummy.Z0_Code = "B";
				dummy.CalculatedProperty = 100;
				AssertMatchesFilter(dummy, query, false);
			}
		}

		class CalculatedDummy : DummyBusinessObject
		{
			public CalculatedDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public ZInt CalculatedProperty { get; set; }
			public ZPropertyInfo CalculatedPropertyInfo { get { return GetZPropertyInfo(nameof(CalculatedProperty)); } }
		}

		class CalculatedDummyCollection : BusinessObjectCollection<CalculatedDummy>
		{
			public CalculatedDummyCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		#endregion

		#region TestGuidFilter

		public void TestGuidFilterMetaData()
		{
			using (var control = new ZUserControl())
			using (var grid = new ZGrid())
			{
				control.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo { ColumnName = "Z0_Code" });
				grid.ColumnStyles.Add(new ZGuidFindBoxColumnStyleInfo { ColumnName = "Z0_Guid" });

				grid.SetDataBinding(new DummyBusinessObjectWithDummiesCollection(Factory), "");

				var filters = new GridFilterStripBusinessObject(grid).GetModuleFilters();
				AssertEquals(2, filters.Count());

				AssertModuleFilter("Code", filters["Z0_Code"], typeof(ModuleNkFilter));
				AssertModuleFilter("Id", filters["Z0_Guid"], typeof(ModuleGuidFilter));

				var codeFilter = (ModuleNkFilter)filters["Z0_Code"];
				AssertEquals("Z0_Code filter should have ModuleID initialized", DummyModuleIDs.Dummy, codeFilter.ModuleId);
				AssertNotNull("Z0_Code filter should have list initialized", codeFilter.List);
				AssertEquals("Z0_Code filter should have list initialized", typeof(NewDummyBusinessObjectCollection), codeFilter.List.GetType());

				var guidFilter = (ModuleGuidFilter)filters["Z0_Guid"];
				AssertEquals("Z0_Guid filter should have ModuleID initialized", DummyModuleIDs.Dummy, guidFilter.ModuleId);
				AssertNotNull("Z0_Guid filter should have list initialized", guidFilter.List);
				AssertEquals("Z0_Guid filter should have list initialized", typeof(NewDummyBusinessObjectCollection), guidFilter.List.GetType());
			}
		}

		public void TestGuidFilterBindToList()
		{
			using (var control = new ZUserControl())
			using (var grid = new ZGrid())
			{
				control.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo { ColumnName = "Z0_Code", BindToList = "OtherDummies", ModuleID = DummyModuleIDs.Dummy2 });
				grid.ColumnStyles.Add(new ZGuidFindBoxColumnStyleInfo { ColumnName = "Z0_Guid", BindToList = "OtherDummies", ModuleID = DummyModuleIDs.Dummy2 });

				grid.SetDataBinding(new DummyBusinessObjectWithDummiesCollection(Factory), "");

				var filters = new GridFilterStripBusinessObject(grid).GetModuleFilters();
				AssertEquals(2, filters.Count());

				AssertModuleFilter("Code", filters["Z0_Code"], typeof(ModuleNkFilter));
				AssertModuleFilter("Id", filters["Z0_Guid"], typeof(ModuleGuidFilter));

				var codeFilter = (ModuleNkFilter)filters["Z0_Code"];
				AssertEquals("Z0_Code filter should have ModuleID initialized", DummyModuleIDs.Dummy2, codeFilter.ModuleId);
				AssertNotNull("Z0_Code filter should have list initialized", codeFilter.List);
				AssertEquals("Z0_Code filter should have list initialized", typeof(DummyBusinessObjectCollection), codeFilter.List.GetType());

				var guidFilter = (ModuleGuidFilter)filters["Z0_Guid"];
				AssertEquals("Z0_Guid filter should have ModuleID initialized", DummyModuleIDs.Dummy2, guidFilter.ModuleId);
				AssertNotNull("Z0_Guid filter should have list initialized", guidFilter.List);
				AssertEquals("Z0_Guid filter should have list initialized", typeof(DummyBusinessObjectCollection), guidFilter.List.GetType());
			}
		}

		class DummyBusinessObjectWithDummies : DummyBusinessObject
		{
			public DummyBusinessObjectWithDummies(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[List("Dummies")]
			public override ZString Z0_Code
			{
				get { return base.Z0_Code; }
				set { base.Z0_Code = value; }
			}

			[List("Dummies")]
			public override ZGuid Z0_Guid
			{
				get { return base.Z0_Guid; }
				set { base.Z0_Guid = value; }
			}

			public NewDummyBusinessObjectCollection Dummies
			{
				get { return new NewDummyBusinessObjectCollection(Factory); }
			}

			public DummyBusinessObjectCollection OtherDummies
			{
				get { return new DummyBusinessObjectCollection(Factory); }
			}
		}

		[ModuleID(ModuleId.Dummy)]
		class NewDummyBusinessObjectCollection : DummyBusinessObjectCollection
		{
			public NewDummyBusinessObjectCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		class DummyBusinessObjectWithDummiesCollection : BusinessObjectCollection<DummyBusinessObjectWithDummies>
		{
			public DummyBusinessObjectWithDummiesCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		#endregion

		#region TestDropDownFilter

		public void TestDropDownFilterMetaData()
		{
			using (var control = new ZUserControl())
			using (var grid = new ZGrid())
			{
				control.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZDropEditColumnStyleInfo("Z0_Code", 80));

				grid.SetDataBinding(new DummyBusinessObjectWithListCollection(Factory), "");

				var filters = new GridFilterStripBusinessObject(grid).GetModuleFilters();
				AssertEquals(1, filters.Count());

				AssertModuleFilter("Code", filters["Z0_Code"], typeof(ModuleTextFilter));

				var codeFilter = (ModuleTextFilter)filters["Z0_Code"];
				AssertNotNull("Z0_Code filter should have list initialized", codeFilter.List);
				AssertEquals("Z0_Code filter should have list initialized", 3, codeFilter.List.Count);

				var list = codeFilter.List as CodeDescriptionPairList;
				AssertNotNull("Z0_Code filter list should have type CodeDescriptionPairList", list);
				AssertEquals("Code 01", list.GetDescriptionFromCode("C01"));
				AssertEquals("Code 02", list.GetDescriptionFromCode("C02"));
				AssertEquals("Code 03", list.GetDescriptionFromCode("C03"));
			}
		}

		public void TestDropDownFilterBindToList()
		{
			using (var control = new ZUserControl())
			using (var grid = new ZGrid())
			{
				control.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZDropEditColumnStyleInfo("Z0_Code", 80) { BindToList = "OtherCodes" });

				grid.SetDataBinding(new DummyBusinessObjectWithListCollection(Factory), "");

				var filters = new GridFilterStripBusinessObject(grid).GetModuleFilters();
				AssertEquals(1, filters.Count());

				AssertModuleFilter("Code", filters["Z0_Code"], typeof(ModuleTextFilter));

				var codeFilter = (ModuleTextFilter)filters["Z0_Code"];
				AssertNotNull("Z0_Code filter should have list initialized", codeFilter.List);
				AssertEquals("Z0_Code filter should have list initialized", 2, codeFilter.List.Count);
				var list = codeFilter.List as CodeDescriptionPairList;
				AssertNotNull("Z0_Code filter list should have type CodeDescriptionPairList", list);
				AssertEquals("Dude 01", list.GetDescriptionFromCode("D01"));
				AssertEquals("Dude 02", list.GetDescriptionFromCode("D02"));
			}
		}

		public void TestNoFilterMadeWhenCaptionIsEmpty()
		{
			using (var control = new ZUserControl())
			using (var grid = new ZGrid())
			{
				control.Controls.Add(grid);
				var info = new ZDropEditColumnStyleInfo("Z0_Code", 80) { BindToList = "OtherCodes" };
				info.Caption = " ";
				info.CaptionResourceString = new ResourceStringData("bcc50f03-0dff-461b-8840-47dc14cfa588", " ");
				grid.ColumnStyles.Add(info);

				grid.SetDataBinding(new DummyBusinessObjectWithListCollection(Factory), "");

				var filters = new GridFilterStripBusinessObject(grid).GetModuleFilters();
				AssertEquals(0, filters.Count());
			}
		}

		class DummyBusinessObjectWithList : DummyBusinessObject
		{
			public DummyBusinessObjectWithList(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[List("Codes")]
			public override ZString Z0_Code
			{
				get { return base.Z0_Code; }
				set { base.Z0_Code = value; }
			}

			public CodeDescriptionPairList Codes
			{
				get
				{
					return new CodeDescriptionPairList
					{
						new CodeDescriptionPair("C01", "Code 01"),
						new CodeDescriptionPair("C02", "Code 02"),
						new CodeDescriptionPair("C03", "Code 03")
					};
				}
			}

			public CodeDescriptionPairList OtherCodes
			{
				get
				{
					return new CodeDescriptionPairList
					{
						new CodeDescriptionPair("D01", "Dude 01"),
						new CodeDescriptionPair("D02", "Dude 02"),
					};
				}
			}
		}

		class DummyBusinessObjectWithListCollection : BusinessObjectCollection<DummyBusinessObjectWithList>
		{
			public DummyBusinessObjectWithListCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		#endregion

		#region TestHeaderText

		public void TestHeaderText()
		{
			using (var cache = Res.UseMockData())
			using (var control = new ZUserControl())
			using (var grid = new ZGrid())
			{
				cache.SetResourceGetter((key) => null);
				cache.Put("DummyBizo|Z0_Number", new ResourceStringData("", "DL: Number Column"));
				cache.Put("DummyBizo|Z0_AnotherNumber", new ResourceStringData("", "DL: Another Number Column"));
				cache.Put("f36ccfff-26bb-477b-bcd5-7885a0179eed", new ResourceStringData("", "Checkboxes"));

				control.CaptionRenderingEnabled = true;
				control.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Number", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Bool", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_AnotherNumber", 80) { CaptionResourceString = Res.GetData("C", "Caption Resource String") });

				grid.SetDataBinding(new DummyBusinessObjectCollection(Factory), "");

				var filters = new GridFilterStripBusinessObject(grid).GetModuleFilters();
				AssertEquals(3, filters.Count());

				AssertModuleFilter("DL: Number Column", filters["Z0_Number"], typeof(ModuleNumberRangeFilter));
				AssertModuleFilter("Checkboxes", filters["Flags"], typeof(ModuleFlagsFilter));
				AssertModuleFilter("Caption Resource String", filters["Z0_AnotherNumber"], typeof(ModuleNumberRangeFilter));
			}
		}

		#endregion

		#region TestMoreThanTenFlagsAreSplitToMultipleGroups

		public void TestMoreThanTenFlagsAreSplitToMultipleGroups()
		{
			using (var control = new ZUserControl())
			using (var grid = new ZGrid())
			{
				control.CaptionRenderingEnabled = true;
				control.Controls.Add(grid);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool01", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool02", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool03", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool04", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool05", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool06", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool07", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool08", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool09", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool10", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool11", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool12", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool13", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool14", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool15", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool16", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool17", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool18", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool19", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool20", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool21", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool22", 80));

				grid.SetDataBinding(new ActiveBusinessObjectCollection<DummyBusinessObjectWithManyBoolProperties>(Factory), "");
				var filters = new GridFilterStripBusinessObject(grid).GetModuleFilters();

				Assert(!filters.Filter_List.ContainsCode("Flags"));
				Assert(filters.Filter_List.ContainsCode("Flags 1"));
				Assert(filters.Filter_List.ContainsCode("Flags 2"));
				Assert(filters.Filter_List.ContainsCode("Flags 3"));

				var filter1 = (ModuleFlagsFilter)filters["Flags 1"];
				AssertNotNull(filter1);
				AssertEquals(10, filter1.FlagNames.Length);
				AssertArrayEqualsByElements(new[] { "Bool01", "Bool02", "Bool03", "Bool04", "Bool05", "Bool06", "Bool07", "Bool08", "Bool09", "Bool10" }, filter1.FlagNames);

				var filter2 = (ModuleFlagsFilter)filters["Flags 2"];
				AssertNotNull(filter2);
				AssertEquals(10, filter2.FlagNames.Length);
				AssertArrayEqualsByElements(new[] { "Bool11", "Bool12", "Bool13", "Bool14", "Bool15", "Bool16", "Bool17", "Bool18", "Bool19", "Bool20" }, filter2.FlagNames);

				var filter3 = (ModuleFlagsFilter)filters["Flags 3"];
				AssertNotNull(filter3);
				AssertEquals(2, filter3.FlagNames.Length);
				AssertArrayEqualsByElements(new[] { "Bool21", "Bool22" }, filter3.FlagNames);
			}
		}

		#endregion

		#region TestFlagNamesAndFlagFilterColumnsCorrespondOneByOneAfterSorting

		public void TestFlagNamesAndFlagFilterColumnsCorrespondOneByOneAfterSorting()
		{
			using (var control = new ZUserControl())
			using (var grid = new ZGrid())
			{
				control.CaptionRenderingEnabled = true;
				control.Controls.Add(grid);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool02", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool01", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool03", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool04", 80));

				grid.SetDataBinding(new ActiveBusinessObjectCollection<DummyBusinessObjectWithManyBoolProperties>(Factory), "");
				var filters = new GridFilterStripBusinessObject(grid).GetModuleFilters();

				Assert(filters.Filter_List.ContainsCode("Flags"));

				var filter = (ModuleFlagsFilter)filters["Flags"];
				AssertNotNull(filter);
				AssertEquals(4, filter.FlagNames.Length);
				AssertEquals(4, filter.FlagFilterColumns.Select(e => e.Name).ToArray().Length);
				AssertArrayEqualsByElements(new[] { "Bool01", "Bool02", "Bool03", "Bool04" }, filter.FlagNames);
				AssertArrayEqualsByElements(new[] { "Bool01", "Bool02", "Bool03", "Bool04" }, filter.FlagFilterColumns.Select(e => e.Name).ToArray());
			}
		}

		#endregion

		#region TestFlagNamesAndFlagFilterColumnsWhenDuplicate

		public void TestFlagNamesAndFlagFilterColumnsWhenDuplicate()
		{
			using (var control = new ZUserControl())
			using (var grid = new ZGrid())
			{
				control.CaptionRenderingEnabled = true;
				control.Controls.Add(grid);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool02", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool01", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool01", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Bool02", 80));

				grid.SetDataBinding(new ActiveBusinessObjectCollection<DummyBusinessObjectWithManyBoolProperties>(Factory), "");
				var filters = new GridFilterStripBusinessObject(grid).GetModuleFilters();

				Assert(filters.Filter_List.ContainsCode("Flags"));

				var filter = (ModuleFlagsFilter)filters["Flags"];
				AssertNotNull(filter);
				AssertEquals(2, filter.FlagNames.Length);
				AssertEquals(2, filter.FlagFilterColumns.Select(e => e.Name).ToArray().Length);
				AssertArrayEqualsByElements(new[] { "Bool01", "Bool02" }, filter.FlagNames);
				AssertArrayEqualsByElements(new[] { "Bool01", "Bool02" }, filter.FlagFilterColumns.Select(e => e.Name).ToArray());
			}
		}

		#endregion

		#region TestFilters_DoNotThrowWhenListElementTypeIsAbstract

		public void TestFilters_DoNotThrowWhenListElementTypeIsAbstract()
		{
			using (var control = new ZUserControl())
			using (var grid = new ZGrid())
			{
				control.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZDropEditColumnStyleInfo("Z0_Code", 80));

				grid.SetDataBinding(new DummyAbstractBusinessObjectCollection(Factory), "");

				AssertNoExceptionThrown(() => new GridFilterStripBusinessObject(grid).GetModuleFilters());
			}
		}

		class DummyAbstractBusinessObjectCollection : BusinessObjectCollection<DummyAbstractBusinessObject>, IHaveAbstractElementType
		{
			public DummyAbstractBusinessObjectCollection(BusinessObjectFactory factory) : base(factory) { }

			Type IHaveAbstractElementType.NonAbstractTypeOfElements
			{
				get
				{
					return typeof(DummyBusinessObject);
				}
			}
		}

		abstract class DummyAbstractBusinessObject : DummyBusinessObject
		{
			public DummyAbstractBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		#endregion

		#region Implementation

		static void AssertModuleFilter(string localizedDescription, ModuleFilter filter, Type expectedType)
		{
			var description = filter != null ? filter.Description.ToString() : localizedDescription;
			AssertNotNull("There should be filter " + description, filter);
			AssertEquals("There should be correct localized description for filter " + description, localizedDescription, filter.LocalizedDescription);
			AssertEquals("Wrong type of filter " + description, expectedType, filter.GetType());
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GridFilterStripBusinessObject(TestGrid);
		}

		ZGrid TestGrid
		{
			get
			{
				if (testGrid == null)
				{
					testGrid = new ZGrid();
					testGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 80));
				}
				return testGrid;
			}
		}
		ZGrid testGrid;

		protected override void TearDown()
		{
			if (testGrid != null)
			{
				testGrid.Dispose();
				testGrid = null;
			}

			base.TearDown();
		}

		#endregion
	}
}
