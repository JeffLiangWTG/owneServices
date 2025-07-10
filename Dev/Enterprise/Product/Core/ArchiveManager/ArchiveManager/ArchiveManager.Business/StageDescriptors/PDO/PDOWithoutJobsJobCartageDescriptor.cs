using System;
using CargoWise.Schema;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.StageDescriptors.PDO
{
	public class PDOWithoutJobsJobCartageDescriptor : PDOWithoutJobsBaseDescriptor
	{
		public override string Name
			=> Res.GetString("22445499-7A83-4F83-A64B-9CECCDE2E2E9", "Purge Documents of Operational Records without Jobs - Job Cartage");

		public override SchemaColumn MainArchivePKColumn
			=> JobCartageSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> JobCartageSchema.JJ_ConsignmentID;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> JobCartageSchema.JJ_SystemCreateTimeUtc;

		public override Type TypeToArchive
			=> typeof(ICommonCartage);
	}
}
