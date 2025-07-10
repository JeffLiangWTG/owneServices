using System;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.ArchiveManager.Business.StageDescriptors.PDO
{
	public class PDOWithoutJobsJobDeclarationDescriptor : PDOWithoutJobsBaseDescriptor
	{
		public override string Name
			=> Res.GetString("D0A527C8-1390-49DC-8F30-BB3176175133", "Purge Documents of Operational Records without Jobs - Job Declaration");

		public override SchemaColumn MainArchivePKColumn
			=> JobDeclarationSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> JobDeclarationSchema.JE_DeclarationReference;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> JobDeclarationSchema.JE_SystemCreateTimeUtc;

		public override Type TypeToArchive
			=> typeof(IBaseJobDeclaration);
	}
}
