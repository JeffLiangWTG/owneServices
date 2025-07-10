using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	public class ConstraintB7_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column B7_ParentTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
DELETE FROM dbo.CusAddInfo
WHERE B7_ParentTableCode NOT IN (
	'B0', -- CusInBondBill
	'B7', -- CusAddInfo
	'B9', -- CusInBondMoveDetail
	'BC', -- CusInBondContainer
	'BH', -- CusInBondHeader
	'BY', -- CusInBondCargoDesc
	'CC', -- CusClassification
	'CEI', -- CusEntryInstruction
	'CH', -- CusEntryHeader
	'CI', -- CusClassPartPivot
	'CL', -- CusEntryLine
	'CO', -- CusContainer
	'CS', -- CusHAWB
	'CU', -- CusDecHouseBill
	'CY', -- CusCodeData
	'JE', -- JobDeclaration
	'JI', -- JobComInvoiceLine
	'JK', -- JobConsol
	'JPB', -- JPAFRBills
	'JPH', -- JPAFRHeader
	'JZ', -- JobComInvoiceHeader
	'OH', -- OrgHeader
	'QH', -- QuarantineExDocHeader
	'ULB', -- CusUSLVConsignment
	'WB', -- WhsBondedWarehouseAttribute
	'WE' -- WhsDocketLine
)");
			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusAddInfoSchema.Instance)
					.Key(CusAddInfoSchema.Constants.B7_ParentID)
					.Include(CusAddInfoSchema.Constants.B7_SystemCreateTimeUtc)
					.Include(CusAddInfoSchema.Constants.B7_SystemLastEditTimeUtc)
					.Where(@"[B7_ParentTableCode]<>'B0' AND [B7_ParentTableCode]<>'B7' AND [B7_ParentTableCode]<>'B9' AND [B7_ParentTableCode]<>'BC' AND [B7_ParentTableCode]<>'BH' AND [B7_ParentTableCode]<>'BY' AND [B7_ParentTableCode]<>'CC' AND [B7_ParentTableCode]<>'CEI' AND [B7_ParentTableCode]<>'CH' AND [B7_ParentTableCode]<>'CI' AND [B7_ParentTableCode]<>'CL' AND [B7_ParentTableCode]<>'CO' AND [B7_ParentTableCode]<>'CS' AND [B7_ParentTableCode]<>'CU' AND [B7_ParentTableCode]<>'CY' AND [B7_ParentTableCode]<>'JE' AND [B7_ParentTableCode]<>'JI' AND [B7_ParentTableCode]<>'JK' AND [B7_ParentTableCode]<>'JPB' AND [B7_ParentTableCode]<>'JPH' AND [B7_ParentTableCode]<>'JZ' AND [B7_ParentTableCode]<>'OH' AND [B7_ParentTableCode]<>'QH' AND [B7_ParentTableCode]<>'ULB' AND [B7_ParentTableCode]<>'WB' AND [B7_ParentTableCode]<>'WE'")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
