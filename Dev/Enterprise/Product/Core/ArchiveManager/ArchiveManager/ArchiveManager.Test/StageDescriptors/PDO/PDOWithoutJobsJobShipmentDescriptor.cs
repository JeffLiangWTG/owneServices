using System;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.PDO;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.PDO
{
	class PDOWithoutJobsJobShipmentDescriptorTest : PDOWithoutJobsBaseDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new PDOWithoutJobsJobShipmentDescriptor();

		protected override string ExpectedName
			=> "Purge Documents of Operational Records without Jobs - Job Shipment";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> JobShipmentSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> JobShipmentSchema.JS_UniqueConsignRef;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn
			=> JobShipmentSchema.JS_SystemCreateTimeUtc;

		public override Type ExpectedTypeToArchive
			=> typeof(ICommonShipment);
	}
}
