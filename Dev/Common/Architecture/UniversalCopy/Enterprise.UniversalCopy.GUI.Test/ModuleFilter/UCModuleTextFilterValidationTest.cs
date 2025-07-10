using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalCopy.GUI.Testing
{
	public class UCModuleTextFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidationMessage()
		{
			// Default filter
			var schemaFilterStripBusinessObject = new SchemaFilterStripBusinessObjectForTest(ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(StmNoteSchema.Constants.TableName));
			var dummyList = new CodeDescriptionPairList();
			dummyList.AddPair(StmNoteDescription.Pub, StmNoteDescription.PubDescriptive);
			dummyList.AddPair(StmNoteDescription.Int, StmNoteDescription.IntDescriptive);
			dummyList.AddPair(StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive);
			dummyList.AddPair(StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive);

			schemaFilterStripBusinessObject.ColumnNamesToInclude = new Dictionary<string, IList>();
			schemaFilterStripBusinessObject.ColumnNamesToInclude.Add(StmNoteSchema.ST_NoteType.Name, dummyList);

			var filters = schemaFilterStripBusinessObject.GetModuleFiltersCoreForTest();
			var stNoteType = filters[StmNoteSchema.ST_NoteType.Name] as ModuleTextFilter;
			AssertNotNull("Should contains ST_NoteType filter as ModuleTextFilter", stNoteType);
			Assert("Should have a list", stNoteType.List is CodeDescriptionPairList);

			stNoteType.IsActive = true;
			stNoteType.Property = "AAA";
			stNoteType.Validation.ValidateAll();
			AssertHasWarning(stNoteType.PropertyInfo, "You have not entered a valid code.");

			// UniversalCopy filter
			var ucSchemaFilterStripBusinessObject = new UCSchemaFilterStripBusinessObjectForTest(ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(StmNoteSchema.Constants.TableName));
			var ucDummyList = new CodeDescriptionPairList();
			ucDummyList.AddPair(StmNoteDescription.Pub, StmNoteDescription.PubDescriptive);
			ucDummyList.AddPair(StmNoteDescription.Int, StmNoteDescription.IntDescriptive);
			ucDummyList.AddPair(StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive);
			ucDummyList.AddPair(StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive);

			ucSchemaFilterStripBusinessObject.ColumnNamesToInclude = new Dictionary<string, IList>();
			ucSchemaFilterStripBusinessObject.ColumnNamesToInclude.Add(StmNoteSchema.ST_NoteType.Name, ucDummyList);

			var ucFilters = ucSchemaFilterStripBusinessObject.GetModuleFiltersCoreForTest();
			var ucStNoteType = ucFilters[StmNoteSchema.ST_NoteType.Name] as UCModuleTextFilter;
			AssertNotNull("Should contains ST_NoteType filter as UCModuleTextFilter", ucStNoteType);
			Assert("Should have a list", ucStNoteType.List is CodeDescriptionPairList);

			ucStNoteType.IsActive = true;
			ucStNoteType.Property = "AAA";
			ucStNoteType.Validation.ValidateAll();
			AssertNoWarning(ucStNoteType.PropertyInfo, "You have not entered a valid code.");
		}

		class UCSchemaFilterStripBusinessObjectForTest : UCSchemaFilterStripBusinessObject
		{
			public UCSchemaFilterStripBusinessObjectForTest(ITableSchema tableSchema)
			   : base(tableSchema)
			{
			}

			public ModuleFilterCollection GetModuleFiltersCoreForTest()
			{
				return base.GetModuleFiltersCore();
			}
		}

		class SchemaFilterStripBusinessObjectForTest : SchemaFilterStripBusinessObject
		{
			public SchemaFilterStripBusinessObjectForTest(ITableSchema tableSchema)
			: base(tableSchema)
			{
			}

			public ModuleFilterCollection GetModuleFiltersCoreForTest()
			{
				return base.GetModuleFiltersCore();
			}
		}
	}
}
