using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	public class RemoveCA_AccountingAgeFromGenAddOnColumn : DataTransformation
	{
		public override string UserDescription => "Remove CA_AccountingAge from GenAddOnColumn";

		internal bool IsCACustoms => Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'");

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (IsCACustoms)
			{
				while (Db.Connection.ExecuteNonQuery("DELETE TOP(10000) FROM dbo.GenAddOnColumn WHERE XA_Name = 'CA_AccountingAge' AND XA_ParentTableCode = 'JE'") > 0)
				{
					token.ThrowIfCancellationRequested();
				}
			}
		}
	}
}
