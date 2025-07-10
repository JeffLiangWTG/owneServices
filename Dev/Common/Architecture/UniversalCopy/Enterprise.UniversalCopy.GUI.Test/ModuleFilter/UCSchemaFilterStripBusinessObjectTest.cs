using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.GUI.Testing
{
	[TestedType(typeof(UCSchemaFilterStripBusinessObject))]
	public class UCSchemaFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestGetModuleFiltersCoreWithList()
		{
			var schemaFilterStripBusinessObject = new UCSchemaFilterStripBusinessObjectForTest(ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(StmNoteSchema.Constants.TableName));
			var dummyList = new CodeDescriptionPairList();
			dummyList.AddPair(StmNoteDescription.Pub, StmNoteDescription.PubDescriptive);
			dummyList.AddPair(StmNoteDescription.Int, StmNoteDescription.IntDescriptive);
			dummyList.AddPair(StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive);
			dummyList.AddPair(StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive);

			schemaFilterStripBusinessObject.ColumnNamesToInclude = new Dictionary<string, IList>();
			schemaFilterStripBusinessObject.ColumnNamesToInclude.Add(StmNoteSchema.ST_NoteText.Name, null);
			schemaFilterStripBusinessObject.ColumnNamesToInclude.Add(StmNoteSchema.ST_NoteType.Name, dummyList);

			var filters = schemaFilterStripBusinessObject.GetModuleFiltersCoreForTest();
			var stNoteText = filters[StmNoteSchema.ST_NoteText.Name] as ModuleTextFilter;
			AssertNotNull("Should contains ST_NoteText filter", stNoteText);
			AssertNull("Shouldn't have a list", stNoteText.List);

			var stNoteType = filters[StmNoteSchema.ST_NoteType.Name] as UCModuleTextFilter;
			AssertNotNull("Should contains ST_NoteType filter as UCModuleTextFilter", stNoteType);
			Assert("Should have a list", stNoteType.List is CodeDescriptionPairList);
			Assert("Validation should be UCModuleTextFilterValidation", stNoteType.Validation is UCModuleTextFilterValidation);
		}

		[TestedType(typeof(UCSchemaFilterStripBusinessObject))]
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

		#region Implementation

		protected override bool ShouldBeLocalizable => false;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new UCSchemaFilterStripBusinessObject(StmNoteSchema.Instance);
		}

		#endregion
	}
}
