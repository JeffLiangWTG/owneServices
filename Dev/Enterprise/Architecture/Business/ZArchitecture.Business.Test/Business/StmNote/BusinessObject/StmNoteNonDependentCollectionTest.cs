using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmNoteNonDependentCollection))]
	sealed class StmNoteNonDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmNoteNonDependentCollection(Factory);
		}

		[ExpectNoExceptions()]
		public override void TestLoad()
		{
			try
			{
				ZQuery top1Filter = new ZQuery(StmNoteSchema.ST_Table, "DummyBizo");
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
