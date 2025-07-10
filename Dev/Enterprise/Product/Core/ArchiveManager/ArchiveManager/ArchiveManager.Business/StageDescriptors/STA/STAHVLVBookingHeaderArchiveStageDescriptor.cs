using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.STA;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class STAHVLVBookingHeaderArchiveStageDescriptor : STAArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("F947661A-996F-4ED5-B968-F3F5F17EA7E3", "Standalone HVLV Booking Header Archive");

		public override SchemaColumn MainArchivePKColumn
			=> HVLVBookingHeaderSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> HVLVBookingHeaderSchema.HVH_BookingReference;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> HVLVBookingHeaderSchema.HVH_SystemCreateTimeUtc;

		public override SchemaColumn ParentIDColumn
			=> null;

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
		{
			ArchiveRelationships.SetUpHVLVBookingHeaderRelationships(systemSetup);
		}

		public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
		{
			var query = base.GetMainArchiveableFilter(config);

			var sql = @"
NOT EXISTS
(
	SELECT 1 FROM dbo.HVLVConsignment
	WHERE HVC_HVH_BookingHeader = HVH_PK
	AND HVC_HCH_Header IS NOT NULL
)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.CusUSLVConsignment
	JOIN HVLVConsignment ON HVC_PK = ULB_HVC_Consignment
	WHERE HVC_HVH_BookingHeader = HVH_PK
)
";

			_ = query.AddFilterAndZSQLParameterCollection(sql, null);

			return query;
		}
	}
}
