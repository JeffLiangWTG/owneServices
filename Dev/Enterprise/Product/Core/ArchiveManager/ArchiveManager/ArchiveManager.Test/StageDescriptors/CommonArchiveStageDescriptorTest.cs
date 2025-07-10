using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.StageDescriptors
{
	public abstract class CommonArchiveStageDescriptorTest : TestCaseWithFactory
	{
		protected abstract IArchiveSystemDescriptor SystemDescriptor { get; }

		protected abstract IArchiveStageDescriptor StageDescriptor { get; }

		protected IArchiveConfiguration _config { get; set; }

		protected abstract string ExpectedName { get; }

		protected abstract SchemaColumn ExpectedMainArchivePKColumn { get; }

		protected abstract SchemaColumn ExpectedMainArchiveNKColumn { get; }

		protected abstract SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; }

		protected abstract ZQuery ExpectedMainArchiveFilterWithoutDeclarations { get; }

		protected abstract ZQuery ExpectedMainArchiveFilterWithDeclarations { get; }

		protected abstract Type[] ExpectedPreparationActions { get; }

		protected abstract Type[] ExpectedArchiveActions { get; }

		protected ZDateTime TestDate => new(2023, 01, 01, 12, 0, 0, 0);

		public void TestName()
			=> AssertEquals(ExpectedName, StageDescriptor.Name);

		public void TestMainArchivePKColumn()
			=> AssertEquals(ExpectedMainArchivePKColumn, StageDescriptor.MainArchivePKColumn);

		public void TestMainArchiveNKColumn()
			=> AssertEquals(ExpectedMainArchiveNKColumn, StageDescriptor.MainArchiveNKColumn);

		public virtual void TestMainArchiveDateFilterColumn()
			=> AssertEquals(ExpectedMainArchiveDateFilterColumn, StageDescriptor.MainDateFilterColumn);

		public virtual void TestGetPreparationAction()
		{
			var logger = new TestArchiveLogger();
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			var mainArchiveItem = new ArchiveItem(dummyBizo.PKSchemaColumn, dummyBizo.PK.ToGuid(), null, Guid.Empty, false, dummyBizo.TablePrefix);
			var archiveSet = new TestArchiveSet(SystemDescriptor, "Dummy Stage Name", Guid.Empty, mainArchiveItem);
			var cache = new ArchiveSystemCache();
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, false);

			var preparationActions = StageDescriptor.GetPreparationAction(logger, archiveSet, cache, config);

			AssertEquals(ExpectedPreparationActions.Length, preparationActions.Count());
			AssertContainsExactElementsInExactOrder(ExpectedPreparationActions, preparationActions.Select(a => a.GetType()));
		}

		public virtual void TestGetArchiveAction()
		{
			var logger = new TestArchiveLogger();
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			var mainArchiveItem = new ArchiveItem(dummyBizo.PKSchemaColumn, dummyBizo.PK.ToGuid(), null, Guid.Empty, false, dummyBizo.TablePrefix);
			var archiveSet = new TestArchiveSet(SystemDescriptor, "Dummy Stage Name", Guid.Empty, mainArchiveItem);

			var preparationActions = StageDescriptor.GetArchiveAction(logger, archiveSet);

			AssertEquals(ExpectedArchiveActions.Length, preparationActions.Count());
			AssertContainsExactElementsInExactOrder(ExpectedArchiveActions, preparationActions.Select(a => a.GetType()));
		}

		[TestDate(2023, 01, 01, 12, 0, 0, 0)]
		public virtual void TestMainArchiveableFilter()
		{
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, false);
			var mainArchiveableFilter = StageDescriptor.GetMainArchiveableFilter(config).LiteralTextSqlFormatted;
			AssertEquals(ExpectedMainArchiveFilterWithoutDeclarations.LiteralTextSqlFormatted, mainArchiveableFilter);

			config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);
			mainArchiveableFilter = StageDescriptor.GetMainArchiveableFilter(config).LiteralTextSqlFormatted;
			AssertEquals(ExpectedMainArchiveFilterWithDeclarations.LiteralTextSqlFormatted, mainArchiveableFilter);
		}
	}
}
