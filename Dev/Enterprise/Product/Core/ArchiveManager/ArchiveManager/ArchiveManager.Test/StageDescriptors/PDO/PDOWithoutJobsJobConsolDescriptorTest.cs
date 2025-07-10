using System;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.PDO;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.PDO
{
	class PDOWithoutJobsJobConsolDescriptorTest : PDOWithoutJobsBaseDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new PDOWithoutJobsJobConsolDescriptor();

		protected override string ExpectedName
			=> "Purge Documents of Operational Records without Jobs - Job Consol";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> JobConsolSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> JobConsolSchema.JK_UniqueConsignRef;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn
			=> JobConsolSchema.JK_SystemCreateTimeUtc;

		public override Type ExpectedTypeToArchive
			=> typeof(ICommonConsol);
	}
}
