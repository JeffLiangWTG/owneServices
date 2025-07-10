using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	public class EDIDelinkInactiveSystemUserAccountsFromContactsTransform : DataTransformation
	{
		public override string UserDescription => "Delink inactive system user accounts from contacts";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (!token.IsCancellationRequested && DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "EdiCustomerUserAccount"))
			{
				var sql = $@"UPDATE dbo.EdiCustomerUserAccount SET EUA_OC_WebAccessContact = NULL, EUA_SystemLastEditTimeUtc = GETUTCDATE(), EUA_SystemLastEditUser = '~BP' WHERE EUA_IsActive = 0 and EUA_OC_WebAccessContact is not null;";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
