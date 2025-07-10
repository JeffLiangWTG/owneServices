using System;
using System.Collections.Generic;
using CargoWise.Data.Testing;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ArchiveManager.Test.SystemDescriptors;
using Enterprise.Client.EDI;
using ZClientEDI.Business.ArchiveManager;
using ZClientEDI.Business.ArchiveManager.StageDescriptors;

namespace ZClientEDI.Business.Test.ArchiveManager
{
	public class ESTPurgeSystemTest : BaseArchiveSystemTest
	{
		protected override IArchiveSystemDescriptor SystemDescriptor
			=> new ESTPurgeSystemDescriptor();

		protected override string ExpectedCode
			=> "EST";

		protected override string ExpectedName
			=> "Purge ediProd Specific Tables";

		protected override string ExpectedNoun
			=> "Purge";

		protected override string ExpectedPresentTenseVerb
			=> "Purging";

		protected override string ExpectedPastTenseVerb
			=> "Purged";

		protected override IReportGenerator ExpectedReportGeneratorType
			=> new PurgeReportGenerator();

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptors
			=> [new ESTApplicationActiveLoggerStageDescriptor()];

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptorsWithCustoms
			=> ExpectedArchiveStageDescriptors;

		protected override List<IArchiveStage> ExpectedArchiveStages
			=> [new ESTPurgeStage(new ESTApplicationActiveLoggerStageDescriptor(), (ISelfContainedArchiveSystemDescriptor)SystemDescriptor)];

		public override void TestAllowShouldArchiveDeclaration()
			=> Assert(!SystemDescriptor.AllowShouldArchiveDeclaration);

		[UseSnapshotProtection]
		public override void TestGetRegistryLogs()
		{
			using (EDIDataRegistry.Instance.PurgeEdiProdSpecificTablesBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 27))
			using (EDIDataRegistry.Instance.PurgeEdiProdSpecificTablesOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 65))
			{
				AssertSequencesEqual(SystemDescriptor.GetRegistryLogs(), ["Set Batch Size: 27", "On or Before Minimum: 65"]);
			}
		}
	}
}
