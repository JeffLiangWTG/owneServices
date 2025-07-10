using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.STA
{
	class STAHVLVBookingHeaderArchiveStageDescriptorTest : STAArchiveDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new STAHVLVBookingHeaderArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Standalone HVLV Booking Header Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> HVLVBookingHeaderSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> HVLVBookingHeaderSchema.HVH_BookingReference;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = HVLVBookingHeaderSchema.HVH_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedParentIDColumn
			=> null;

		protected override List<ArchiveRelationshipForSTATest> ExpectedRelationships
			=> new()
			{
				new ArchiveRelationshipForSTATest(HVLVBookingHeaderSchema.Constants.TableName, HVLVBookingHeaderSchema.PK, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.HVC_HVH_BookingHeader, isReversed: false),
				new ArchiveRelationshipForSTATest(HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.PK, HVLVItemSchema.Constants.TableName, HVLVItemSchema.HVI_HVC_Consignment, isReversed: false),
				new ArchiveRelationshipForSTATest(HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.PK, HVLVReturnPivotSchema.Constants.TableName, HVLVReturnPivotSchema.HVP_HVC_Former, isReversed: false),
				new ArchiveRelationshipForSTATest(HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.PK, HVLVReturnPivotSchema.Constants.TableName, HVLVReturnPivotSchema.HVP_HVC_Return, isReversed: false),
				new ArchiveRelationshipForSTATest(HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.PK, CusEntryNumSchema.Constants.TableName, CusEntryNumSchema.CE_ParentID, isReversed: false)
			};

		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
		{
			get
			{
				var query = base.ExpectedMainArchiveFilterWithoutDeclarations;
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

				return query.AddFilterAndZSQLParameterCollection(sql, null);
			}
		}
	}
}
