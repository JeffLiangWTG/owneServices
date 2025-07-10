using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class ConstraintCY_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column CY_ParentTableCode";

		public TransformationIndexProvider IndexProvider => CreateIndexProvider();

		TransformationIndexProvider CreateIndexProvider()
		{
			var provider = new TransformationIndexProvider(this);

			provider.New(CusCodeDataSchema.Instance)
				.Key(CusCodeDataSchema.Constants.CY_ParentID)
				.Include(CusCodeDataSchema.Constants.CY_SystemCreateTimeUtc)
				.Include(CusCodeDataSchema.Constants.CY_SystemLastEditTimeUtc)
				.Where(@"[CY_ParentTableCode]<>'ABL' AND [CY_ParentTableCode]<>'AMA' AND [CY_ParentTableCode]<>'B0' AND [CY_ParentTableCode]<>'B7' AND [CY_ParentTableCode]<>'B9' AND [CY_ParentTableCode]<>'BC' AND [CY_ParentTableCode]<>'BH' AND [CY_ParentTableCode]<>'BJ' AND [CY_ParentTableCode]<>'BK' AND [CY_ParentTableCode]<>'BM' AND [CY_ParentTableCode]<>'BY' AND [CY_ParentTableCode]<>'C5' AND [CY_ParentTableCode]<>'CC' AND [CY_ParentTableCode]<>'CEI' AND [CY_ParentTableCode]<>'CER' AND [CY_ParentTableCode]<>'CGC' AND [CY_ParentTableCode]<>'CH' AND [CY_ParentTableCode]<>'CI' AND [CY_ParentTableCode]<>'CL' AND [CY_ParentTableCode]<>'CPH' AND [CY_ParentTableCode]<>'CSI' AND [CY_ParentTableCode]<>'CU' AND [CY_ParentTableCode]<>'CY' AND [CY_ParentTableCode]<>'DL' AND [CY_ParentTableCode]<>'EE' AND [CY_ParentTableCode]<>'EM' AND [CY_ParentTableCode]<>'HVC' AND [CY_ParentTableCode]<>'JE' AND [CY_ParentTableCode]<>'JI' AND [CY_ParentTableCode]<>'JK' AND [CY_ParentTableCode]<>'JPB' AND [CY_ParentTableCode]<>'JPH' AND [CY_ParentTableCode]<>'JS' AND [CY_ParentTableCode]<>'JZ' AND [CY_ParentTableCode]<>'OH' AND [CY_ParentTableCode]<>'OV' AND [CY_ParentTableCode]<>'PF' AND [CY_ParentTableCode]<>'QH' AND [CY_ParentTableCode]<>'QL' AND [CY_ParentTableCode]<>'STH' AND [CY_ParentTableCode]<>'XX'")
				.GetInfo();

			return provider;
		}

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
				DELETE FROM dbo.CusCodeData
				WHERE CY_ParentTableCode NOT IN (
					'ABL', -- AsycudaBill
					'AMA', -- AsycudaManifestHeader
					'B0', -- CusInBondBill
					'B7', -- CusAddInfo
					'B9', -- CusInBondMoveDetail
					'BC', -- CusInBondContainer
					'BH', -- CusInBondHeader
					'BJ', -- CusInBondEquipment
					'BK', -- CusSeal
					'BM', -- CusInBondMoveHeader
					'BY', -- CusInBondCargoDesc
					'C5', -- CusOutturn
					'CC', -- CusClassification
					'CEI', -- CusEntryInstruction
					'CER', -- CusExitReport
					'CGC', -- CusGoodsCatalog
					'CH', -- CusEntryHeader
					'CI', -- CusClassPartPivot
					'CL', -- CusEntryLine
					'CPH', -- CusPermitHeader
					'CSI', -- CusSupportingInfo
					'CU', -- CusDecHouseBill
					'CY', -- CusCodeData
					'DL', -- SupplierBookingLine
					'EE', -- QuarantineExDocEstablishmentAndTime
					'EM', -- EDIMessage
					'HVC', -- HVLVConsignment
					'JE', -- JobDeclaration
					'JI', -- JobComInvoiceLine
					'JK', -- JobConsol
					'JPB', -- JPAFRBills
					'JPH', -- JPAFRHeader
					'JS', -- JobShipment
					'JZ', -- JobComInvoiceHeader
					'OH', -- OrgHeader
					'OV', -- OrgCountryData
					'PF', -- OrgSupBuyLinkTrnMode
					'QH', -- QuarantineExDocHeader
					'QL', -- QuarantineExDocLine
					'STH', -- CusTempStorageDec
					'XX' -- GenPivot
				)");

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
