using System.Globalization;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.ServiceTasks
{
	static class OnlineUpgradeHelper
	{
		internal delegate void RunOnlineUpgradeDelegate(BusinessObjectFactory factory, DynamicBusinessObjectCollection dynamicBOs);

		internal static void RunOnlineUpgradeWithTempTable(string tempTableName, string fieldName, int batchSize, RunOnlineUpgradeDelegate onlineUpgradeDelegate, CancellationToken token)
		{
			var branch = GlbBranch.GetFirstActiveBranch(Core.Constants.CountryCodes.Canada);
			if (branch != null)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					if (DbObjectCreator.TableExists(Db.Connection, tempTableName))
					{
						var factory = new BusinessObjectFactory();
						var dynamicBOs = new DynamicBusinessObjectCollection(factory);
						dynamicBOs.Load(string.Format(CultureInfo.InvariantCulture, "SELECT TOP {0} {1} FROM {2}", batchSize, fieldName, tempTableName));
						if (dynamicBOs.Count > 0)
						{
							onlineUpgradeDelegate(factory, dynamicBOs);

							if (!token.IsCancellationRequested)
							{
								factory.Save();
								StringBuilder deleteCommand = new StringBuilder(55);
								deleteCommand.Append(string.Format(CultureInfo.InvariantCulture, "DELETE {0} WHERE {1} IN ('{2}')", tempTableName, fieldName,
									string.Join("','", dynamicBOs.Select(bo => bo[fieldName].ToString()))));
								Db.Connection.ExecuteNonQuery(deleteCommand.ToString());
							}
							else
							{
								Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "DROP TABLE {0}", tempTableName));
							}
						}
					}
				}
			}
		}
	}
}
