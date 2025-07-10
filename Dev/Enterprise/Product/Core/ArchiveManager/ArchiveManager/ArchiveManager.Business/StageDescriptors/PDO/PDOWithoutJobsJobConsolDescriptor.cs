using System;
using CargoWise.Schema;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.StageDescriptors.PDO
{
	public class PDOWithoutJobsJobConsolDescriptor : PDOWithoutJobsBaseDescriptor
	{
		public override string Name
			=> Res.GetString("9ADA4366-E929-42D3-AE51-B076ABDA6FED", "Purge Documents of Operational Records without Jobs - Job Consol");

		public override SchemaColumn MainArchivePKColumn
			=> JobConsolSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> JobConsolSchema.JK_UniqueConsignRef;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> JobConsolSchema.JK_SystemCreateTimeUtc;

		public override Type TypeToArchive
			=> typeof(ICommonConsol);
	}
}
