//using CargoWise.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing
{
	public class RemoveCommissionsForInvalidGroupingSourceTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Remove Commissions For Invalid Grouping Source Table Code";
		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(@"DELETE dbo.AccCommissionApprovalRequestItem
FROM dbo.AccCommissionApprovalRequestItem
INNER JOIN dbo.AccCommissionLine ON CRI_CL0 = CL0_PK
INNER JOIN dbo.AccCommissionHeader ON CL0_ParentID = CH0_PK
WHERE CH0_GroupingSourceTableCode NOT IN ('JH', 'AH')");

			Db.Connection.ExecuteNonQuery(@"DELETE dbo.AccCommissionApprovalRequestItem
FROM dbo.AccCommissionApprovalRequestItem
INNER JOIN dbo.AccCommissionLine ON CRI_CL0 = CL0_PK
INNER JOIN dbo.AccCommissionLineGroup ON CL0_ParentID = CLG_PK
INNER JOIN dbo.AccCommissionHeader ON CLG_CH0 = CH0_PK
WHERE CH0_GroupingSourceTableCode NOT IN ('JH', 'AH')");

			Db.Connection.ExecuteNonQuery(@"DELETE dbo.AccCommissionLine
FROM dbo.AccCommissionLine
INNER JOIN dbo.AccCommissionHeader ON CL0_ParentID = CH0_PK
WHERE CH0_GroupingSourceTableCode NOT IN ('JH', 'AH')");

			Db.Connection.ExecuteNonQuery(@"DELETE dbo.AccCommissionLine
FROM dbo.AccCommissionLine
INNER JOIN dbo.AccCommissionLineGroup ON CL0_ParentID = CLG_PK
INNER JOIN dbo.AccCommissionHeader ON CLG_CH0 = CH0_PK
WHERE CH0_GroupingSourceTableCode NOT IN ('JH', 'AH')");

			Db.Connection.ExecuteNonQuery(@"DELETE dbo.AccCommissionHeader
FROM dbo.AccCommissionHeader
WHERE CH0_GroupingSourceTableCode NOT IN ('JH', 'AH')");
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(AccCommissionHeaderSchema.Instance)
					.Key(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode)
					.Where($"[{AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode}]<>'JH' AND [{AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode}]<>'AH'")
					.Include(AccCommissionHeaderSchema.Constants.PK)
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
