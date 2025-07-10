using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class ConstraintCSI_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column CSI_ParentTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
DELETE FROM dbo.CusSupportingInfo
WHERE CSI_ParentTableCode NOT IN (
	'ABL', -- AsycudaBill
	'AMA', -- AsycudaManifestHeader
	'APA', -- AsycudaPack
	'API', -- AsycudaPackedItem
	'ASR', -- AsycudaBillScreening
	'B0', -- CusInBondBill
	'B3', -- CusStatementLine
	'B9', -- CusInBondMoveDetail
	'BH', -- CusInBondHeader
	'BM', -- CusInBondMoveHeader
	'BY', -- CusInBondCargoDesc
	'CCI', -- CusExitConsignmentItem
	'CED', -- CusExitDetail
	'CEI', -- CusEntryInstruction
	'CER', -- CusExitReport
	'CI', -- CusClassPartPivot
	'CL', -- CusEntryLine
	'CSI', -- CusSupportingInfo
	'CXC', -- CusExitConsignment
	'CY', -- CusCodeData
	'ERI', -- CusExitReportItem
	'JE', -- JobDeclaration
	'JI', -- JobComInvoiceLine
	'JZ', -- JobComInvoiceHeader
	'QH', -- QuarantineExDocHeader
	'STH', -- CusTempStorageDec
	'TSL', -- CusTempStorageLine
	'TW1' -- CusTWControllingMessageHeader
)");

			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusSupportingInfoSchema.Instance)
				.Key(CusSupportingInfoSchema.Constants.CSI_ParentID)
				.Include(CusSupportingInfoSchema.Constants.CSI_Type)
				.Include(CusSupportingInfoSchema.Constants.CSI_CSI_SupportingInfo)
				.Include(CusSupportingInfoSchema.Constants.CSI_SystemCreateTimeUtc)
				.Include(CusSupportingInfoSchema.Constants.CSI_SystemLastEditTimeUtc)
				.Where(@"[CSI_ParentTableCode]<>'ABL' AND [CSI_ParentTableCode]<>'AMA' AND [CSI_ParentTableCode]<>'APA' AND [CSI_ParentTableCode]<>'API' AND [CSI_ParentTableCode]<>'ASR' AND [CSI_ParentTableCode]<>'B0' AND [CSI_ParentTableCode]<>'B3' AND [CSI_ParentTableCode]<>'B9' AND [CSI_ParentTableCode]<>'BH' AND [CSI_ParentTableCode]<>'BM' AND [CSI_ParentTableCode]<>'BY' AND [CSI_ParentTableCode]<>'CCI' AND [CSI_ParentTableCode]<>'CED' AND [CSI_ParentTableCode]<>'CEI' AND [CSI_ParentTableCode]<>'CER' AND [CSI_ParentTableCode]<>'CI' AND [CSI_ParentTableCode]<>'CL' AND [CSI_ParentTableCode]<>'CSI' AND [CSI_ParentTableCode]<>'CXC' AND [CSI_ParentTableCode]<>'CY' AND [CSI_ParentTableCode]<>'ERI' AND [CSI_ParentTableCode]<>'JE' AND [CSI_ParentTableCode]<>'JI' AND [CSI_ParentTableCode]<>'JZ' AND [CSI_ParentTableCode]<>'QH' AND [CSI_ParentTableCode]<>'STH' AND [CSI_ParentTableCode]<>'TSL' AND [CSI_ParentTableCode]<>'TW1'")
				.GetInfo();

				return indexProvider;
			}
		}
	}
}
