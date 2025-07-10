using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing
{
	public class RemoveCommisionAgreementItemsWhereCodeIsEmpty : DataTransformation
	{
		public override string UserDescription => "Remove Records with Empty Codes from database";

		protected override void OfflinePostUpgradeTransform()
			=> Db.Connection.ExecuteNonQuery(@"
		DECLARE @agreementitems TABLE (itemPk UNIQUEIDENTIFIER);
		WITH AgreementItems AS (
			SELECT CAI_PK FROM dbo.OrgCommissionAgreementItem
			WHERE CAI_Code = ''
			UNION ALL
			SELECT child.CAI_PK FROM dbo.OrgCommissionAgreementItem as child
			INNER JOIN AgreementItems AS parent
				ON child.CAI_ParentTableCode = 'CAI' AND child.CAI_ParentID = parent.CAI_PK
		)
		INSERT INTO @agreementitems (itemPk)
		SELECT CAI_PK FROM AgreementItems;

		Delete dbo.OrgCommissionAgreementItemCondition from dbo.OrgCommissionAgreementItemCondition
		INNER JOIN @agreementitems ON itemPK = CIC_CAI;
		Delete dbo.OrgCommissionAgreementItem from dbo.OrgCommissionAgreementItem
		INNER JOIN @agreementitems ON itemPK = CAI_PK;
		");
	}
}
