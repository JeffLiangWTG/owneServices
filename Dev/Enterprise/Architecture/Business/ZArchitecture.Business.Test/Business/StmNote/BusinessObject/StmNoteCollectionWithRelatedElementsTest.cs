using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmNoteCollectionWithRelatedElements))]
	sealed class StmNoteCollectionWithRelatedElementsTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmNoteCollectionWithRelatedElements(Factory.New<DummyEnterpriseBusinessObject>());
		}

		public void TestRemoveAllRelatedElements()
		{
			var collection = new StmNoteCollectionWithRelatedElements(Dummy);
			AssertEquals("StmNoteCollectionWithRelatedElements.Count", 0, collection.Count);

			var note = Factory.New<StmNote>();
			note.ST_ParentID = Dummy.PK;
			note.ST_Table = Dummy.TableName;
			var relatedNote = Factory.New<StmNote>();
			relatedNote.ST_Table = Dummy.TableName;

			collection.AddRange(new BusinessObject[] { note, relatedNote });
			AssertEquals("StmNoteCollectionWithRelatedElements.Count", 2, collection.Count);

			collection.RemoveAllRelatedElements();
			AssertEquals("StmNoteCollectionWithRelatedElements.Count", 1, collection.Count);
			AssertEquals("StmNoteCollectionWithRelatedElements should contain the non-related note", note, collection[0]);
		}

		DummyEnterpriseBusinessObject Dummy;
		protected override void SetUp()
		{
			Dummy = Factory.New<DummyEnterpriseBusinessObject>();
			base.SetUp();
		}

		[ExpectNoExceptions()]
		public override void TestLoad()
		{
			try
			{
				ZQuery top1Filter = new ZQuery(StmNoteSchema.ST_Table, Dummy.TableName);
				top1Filter.MaximumRows = 1;
				Collection.Load(top1Filter);
			}
			catch (NotSupportedException) // Load not supported for this collection
			{
				Assert(true);
			}
		}
	}
}
