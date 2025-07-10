using System;
using CargoWise.Schema;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.StageDescriptors.PDO
{
	public class PDOWithoutJobsJobShipmentDescriptor : PDOWithoutJobsBaseDescriptor
	{
		public override string Name
			=> Res.GetString("1D36C5AD-533A-4CE7-8832-68D241D1D55F", "Purge Documents of Operational Records without Jobs - Job Shipment");

		public override SchemaColumn MainArchivePKColumn
			=> JobShipmentSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> JobShipmentSchema.JS_UniqueConsignRef;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> JobShipmentSchema.JS_SystemCreateTimeUtc;

		public override Type TypeToArchive
			=> typeof(ICommonShipment);
	}
}
