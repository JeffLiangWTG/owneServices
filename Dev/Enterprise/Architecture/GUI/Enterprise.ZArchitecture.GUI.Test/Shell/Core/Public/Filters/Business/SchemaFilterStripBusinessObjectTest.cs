using System;
using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(SchemaFilterStripBusinessObject))]
	class SchemaFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestManyFlagsAreSplitToSeveralFilters

		public void TestManyFlagsAreSplitToSeveralFilters()
		{
			var filterBizo = new SchemaFilterStripBusinessObject(new TestSchema());
			var filters = filterBizo.GetModuleFilters();

			Assert(!filters.Filter_List.ContainsCode("Flags"));
			Assert(filters.Filter_List.ContainsCode("Flags 1"));
			Assert(filters.Filter_List.ContainsCode("Flags 2"));

			var filter1 = (ModuleFlagsFilter)filters["Flags 1"];
			AssertNotNull(filter1);
			AssertEquals(10, filter1.FlagNames.Length);
			AssertArrayEqualsByElements(new[] { "Bool01", "Bool02", "Bool03", "Bool04", "Bool05", "Bool06", "Bool07", "Bool08", "Bool09", "Bool10" }, filter1.FlagNames);

			var filter2 = (ModuleFlagsFilter)filters["Flags 2"];
			AssertNotNull(filter2);
			AssertEquals(5, filter2.FlagNames.Length);
			AssertArrayEqualsByElements(new[] { "Bool11", "Bool12", "Bool13", "Bool14", "Bool15" }, filter2.FlagNames);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Testing")]
		class TestSchema : ITableSchema
		{
			public TestSchema()
			{
				Bool01 = new SchemaBoolColumn(this, "Bool01", 0, false, false, false);
				Bool02 = new SchemaBoolColumn(this, "Bool02", 0, false, false, false);
				Bool03 = new SchemaBoolColumn(this, "Bool03", 0, false, false, false);
				Bool04 = new SchemaBoolColumn(this, "Bool04", 0, false, false, false);
				Bool05 = new SchemaBoolColumn(this, "Bool05", 0, false, false, false);
				Bool06 = new SchemaBoolColumn(this, "Bool06", 0, false, false, false);
				Bool07 = new SchemaBoolColumn(this, "Bool07", 0, false, false, false);
				Bool08 = new SchemaBoolColumn(this, "Bool08", 0, false, false, false);
				Bool09 = new SchemaBoolColumn(this, "Bool09", 0, false, false, false);
				Bool10 = new SchemaBoolColumn(this, "Bool10", 0, false, false, false);
				Bool11 = new SchemaBoolColumn(this, "Bool11", 0, false, false, false);
				Bool12 = new SchemaBoolColumn(this, "Bool12", 0, false, false, false);
				Bool13 = new SchemaBoolColumn(this, "Bool13", 0, false, false, false);
				Bool14 = new SchemaBoolColumn(this, "Bool14", 0, false, false, false);
				Bool15 = new SchemaBoolColumn(this, "Bool15", 0, false, false, false);
			}

			public SchemaColumn GetSchemaColumn(string columnName)
			{
				throw new NotImplementedException();
			}

			public string SqlSchemaName { get { return "dbo"; } }

			public string TableName { get { return "TestTable"; } }

			public string DatabaseName { get { return "TestDb"; } }

			public SchemaPKColumn PK { get { return null; } }

			public string PkIndexName => null;

			public bool AddParameterSuffixForPKReference { get { return false; } }

			public SchemaColumnCollection All
			{
				get { return new SchemaColumnCollection(new[] { Bool01, Bool02, Bool03, Bool04, Bool05, Bool06, Bool07, Bool08, Bool09, Bool10, Bool11, Bool12, Bool13, Bool14, Bool15 }); }
			}

			public SchemaBoolColumn Bool01 { get; private set; }
			public SchemaBoolColumn Bool02 { get; private set; }
			public SchemaBoolColumn Bool03 { get; private set; }
			public SchemaBoolColumn Bool04 { get; private set; }
			public SchemaBoolColumn Bool05 { get; private set; }
			public SchemaBoolColumn Bool06 { get; private set; }
			public SchemaBoolColumn Bool07 { get; private set; }
			public SchemaBoolColumn Bool08 { get; private set; }
			public SchemaBoolColumn Bool09 { get; private set; }
			public SchemaBoolColumn Bool10 { get; private set; }
			public SchemaBoolColumn Bool11 { get; private set; }
			public SchemaBoolColumn Bool12 { get; private set; }
			public SchemaBoolColumn Bool13 { get; private set; }
			public SchemaBoolColumn Bool14 { get; private set; }
			public SchemaBoolColumn Bool15 { get; private set; }
		}

		#endregion

		#region TestGetModuleFiltersCoreWithList

		public void TestGetModuleFiltersCoreWithList()
		{
			var schemaFilterStripBusinessObject = new SchemaFilterStripBusinessObject(StmNoteSchema.Instance);
			var dummyList = new CodeDescriptionPairList();
			dummyList.AddPair(StmNoteDescription.Pub, StmNoteDescription.PubDescriptive);
			dummyList.AddPair(StmNoteDescription.Int, StmNoteDescription.IntDescriptive);
			dummyList.AddPair(StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive);
			dummyList.AddPair(StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive);

			schemaFilterStripBusinessObject.ColumnNamesToInclude = new Dictionary<string, System.Collections.IList>();
			schemaFilterStripBusinessObject.ColumnNamesToInclude.Add(StmNoteSchema.ST_NoteText.Name, null);
			schemaFilterStripBusinessObject.ColumnNamesToInclude.Add(StmNoteSchema.ST_NoteType.Name, dummyList);

			var filters = schemaFilterStripBusinessObject.GetModuleFilters();
			var sT_NoteText = filters[StmNoteSchema.ST_NoteText.Name] as ModuleTextFilter;
			AssertNotNull("Should contains ST_NoteText filter", sT_NoteText);
			AssertNull("Shouldn't have a list", sT_NoteText.List);

			var sT_NoteType = filters[StmNoteSchema.ST_NoteType.Name] as ModuleTextFilter;
			AssertNotNull("Should contains ST_NoteType filter", sT_NoteType);
			Assert("Should have a list", sT_NoteType.List is CodeDescriptionPairList);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new SchemaFilterStripBusinessObject(StmDataSchema.Instance);
		}

		protected override bool ShouldBeLocalizable
		{
			get { return false; }
		}

		#endregion
	}
}
