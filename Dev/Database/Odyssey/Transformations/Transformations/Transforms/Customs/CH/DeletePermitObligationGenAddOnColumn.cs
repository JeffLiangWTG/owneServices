using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CH
{
	sealed class DeletePermitObligationGenAddOnColumn : DataTransformation
	{
		public override string UserDescription => "Delete PermitObligation from GenAddOnColumn";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (Db.Connection.Exists($"FROM [dbo].[GlbCompany] WHERE [GC_RN_NKCountryCode] = '{CountryCode}'"))
			{
				Db.Connection.ExecuteNonQuery(DeletePermitObligationFromGenAddOnColumnTable);
			}
		}

		const string CountryCode = "CH";

		const string DeletePermitObligationFromGenAddOnColumnTable = $@"
DELETE dbo.GenAddOnColumn
	FROM dbo.GenAddOnColumn
	JOIN dbo.CusInBondCargoDesc ON BY_PK=XA_ParentID
	JOIN dbo.CusInBondBill ON B0_PK=BY_ParentID
	JOIN dbo.CusInBondHeader ON BH_PK=B0_BH
	JOIN dbo.GlbBranch ON GB_PK = BH_GB
	JOIN dbo.GlbCompany ON GC_PK = GB_GC
	WHERE XA_Name = 'PermitObligation'
	AND GC_RN_NKCountryCode = '{CountryCode}'";
	}
}

