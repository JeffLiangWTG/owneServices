using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.IPS
{
	abstract class IPSArchiveStageDescriptorTest : CommonArchiveStageDescriptorTest
	{
		protected abstract SchemaColumn ExpectedIsCancelledSchemaColumn { get; }

		protected override IArchiveSystemDescriptor SystemDescriptor
			=> new IPSArchiveSystemDescriptor();

		public void TestExpectedIsCancelledSchemaColumn()
		{
			var ipsArchiveStageDescriptor = StageDescriptor as IPSArchiveDescriptor;
			AssertNotNull("Stage descriptor should be an IPS descriptor", ipsArchiveStageDescriptor);
			AssertEquals(ExpectedIsCancelledSchemaColumn, ipsArchiveStageDescriptor.IsCancelledSchemaColumn);
		}

		protected override ZQuery ExpectedMainArchiveFilterWithDeclarations
		{
			get
			{
				var query = new ZQuery(ExpectedIsCancelledSchemaColumn, true);
				_ = query.AddToFilter(ExpectedMainArchiveDateFilterColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, TestDate);
				query.OrderBy = ExpectedMainArchiveDateFilterColumn.Name;

				return query;
			}
		}

		protected override Type[] ExpectedPreparationActions
			=> new Type[] { typeof(ArchiveImageGenerationAction) };

		protected override Type[] ExpectedArchiveActions
			=> new Type[] { typeof(PeriodArchiveCommencedAction), typeof(NullifyFKAction) };
	}
}
