using System;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.PDO;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.PDO
{
	class PDOWithoutJobsJobDeclarationDescriptorTest : PDOWithoutJobsBaseDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new PDOWithoutJobsJobDeclarationDescriptor();

		protected override string ExpectedName
			=> "Purge Documents of Operational Records without Jobs - Job Declaration";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> JobDeclarationSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> JobDeclarationSchema.JE_DeclarationReference;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn
			=> JobDeclarationSchema.JE_SystemCreateTimeUtc;

		public override Type ExpectedTypeToArchive
			=> typeof(IBaseJobDeclaration);
	}
}
