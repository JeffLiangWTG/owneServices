using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	public class ConstraintCSD_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column CSD_ParentTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			const string sql = """
				DELETE FROM dbo.CusStorageDocPivot
				WHERE
					CSD_ParentTableCode NOT IN (
						'ABL', -- AsycudaBill
						'AMA', -- AsycudaManifestHeader
						'BH', -- CusInBondHeader
						'C4', -- CusUnderbond
						'CA', -- CusSCAHouse
						'CB', -- CusSCAOceanBill
						'CEI', -- CusEntryInstruction
						'CH', -- CusEntryHeader
						'CM', -- CusMAWB
						'CS', -- CusHAWB
						'CSI', -- CusSupportingInfo
						'EUS', -- EUMemberStateCommunication
						'JI', -- JobComInvoiceLine
						'JZ', -- JobComInvoiceHeader
						'QCH' -- QuarantineColsHeader
					)
				""";
			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusStorageDocPivotSchema.Instance)
					.Key(CusStorageDocPivotSchema.Constants.CSD_ParentID)
					.Include(CusStorageDocPivotSchema.Constants.CSD_SystemCreateTimeUtc)
					.Include(CusStorageDocPivotSchema.Constants.CSD_SystemLastEditTimeUtc)
					.Where("[CSD_ParentTableCode]<>'ABL' AND [CSD_ParentTableCode]<>'AMA' AND [CSD_ParentTableCode]<>'BH' AND [CSD_ParentTableCode]<>'C4' AND [CSD_ParentTableCode]<>'CA' AND [CSD_ParentTableCode]<>'CB' AND [CSD_ParentTableCode]<>'CEI' AND [CSD_ParentTableCode]<>'CH' AND [CSD_ParentTableCode]<>'CM' AND [CSD_ParentTableCode]<>'CS' AND [CSD_ParentTableCode]<>'CSI' AND [CSD_ParentTableCode]<>'EUS' AND [CSD_ParentTableCode]<>'JI' AND [CSD_ParentTableCode]<>'JZ' AND [CSD_ParentTableCode]<>'QCH'")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
