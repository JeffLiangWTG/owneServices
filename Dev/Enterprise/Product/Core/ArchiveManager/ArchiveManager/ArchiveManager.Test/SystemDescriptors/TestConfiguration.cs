using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors
{
	public class TestConfiguration
	{
		public string ArchiveSystemCodeToTest { get; set; }
		public List<string> ListOfStageNames { get; set; }

		public ZGuid Consol1PK { get; set; }
		public ZGuid[] ShipmentPKs { get; set; }

		public bool NeedWorkingStatusJobHeader { get; set; }
		public bool AllJobHeadersNeedToBeClosed { get; set; }
		public bool NeedJobHeaderConsol { get; set; }
		public bool NeedJobHeaderCartage { get; set; }
		public bool NeedShipmentDeclaration { get; set; }
		public bool IsVerboseLog { get; set; }
		public bool IsPeriodClosed { get; set; }
		public bool IsHotChequeCancelled { get; set; }
		public bool IsForwardingDataCancelled { get; set; }
		public bool IsHotChequeLinkedToAH { get; set; }

		public List<string> TablesToIgnore { get; set; } = [];

		public bool NeedTableRecordCounts { get; set; }
		public Dictionary<string, TableInfo> TableRecordCountsBaseline { get; set; }
		public Dictionary<string, TableInfo> TableRecordCountsWithTestRecords { get; set; }
		public Dictionary<string, TableInfo> TableRecordCountsAfterRun { get; set; }
	}
}
