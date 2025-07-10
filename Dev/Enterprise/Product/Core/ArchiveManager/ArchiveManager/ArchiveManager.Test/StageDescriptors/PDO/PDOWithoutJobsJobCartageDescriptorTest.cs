using System;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.PDO;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.PDO
{
	class PDOWithoutJobsJobCartageDescriptorTest : PDOWithoutJobsBaseDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new PDOWithoutJobsJobCartageDescriptor();

		protected override string ExpectedName
			=> "Purge Documents of Operational Records without Jobs - Job Cartage";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> JobCartageSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> JobCartageSchema.JJ_ConsignmentID;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn
			=> JobCartageSchema.JJ_SystemCreateTimeUtc;

		public override Type ExpectedTypeToArchive
			=> typeof(ICommonCartage);
	}
}
