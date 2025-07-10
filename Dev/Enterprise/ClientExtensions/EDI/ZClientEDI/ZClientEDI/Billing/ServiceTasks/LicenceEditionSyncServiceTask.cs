using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Client.EDI.Billing.ServiceTasks.LicenceEditionSyncServiceTask.Code,
	"License Edition Synchronizer",
	"CSP",
	typeof(Enterprise.Client.EDI.Billing.ServiceTasks.LicenceEditionSyncServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "6Hours",
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true
	)]

namespace Enterprise.Client.EDI.Billing.ServiceTasks
{
	public class LicenceEditionSyncServiceTask : ServiceProviderImpl
	{
		public const string Code = "LES";

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "Begin processing");

			var currentPk = ZGuid.Empty;

			for (;;)
			{
				token.ThrowIfCancellationRequested();
				var query = new ZDBOnlyQuery(typeof(LicenceHeader));
				query.AddToFilter(LicenceHeaderSchema.LA_LicenceAdvStdOth, SQLComparisonOperator.NotEqual, BillingConstants.BillingModel.STL);
				query.AddToFilter(LicenceHeaderSchema.PK, SQLComparisonOperator.GreaterThan, currentPk);
				query.MaximumRows = MaximumLoadRows;
				query.OrderBy = LicenceHeaderSchema.Constants.PK;

				var licenceHeaders = Factory.Load<LicenceHeader>(query);

				if (!licenceHeaders.Any())
				{
					break;
				}
				else
				{
					var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						currentPk = licenceHeaders.Last().PK;
						var headersWithStlEdition = licenceHeaders.Where(x => x.Edition == BillingConstants.BillingModel.STL).ToArray();

						if (headersWithStlEdition.Any())
						{
							var sqlSync = FormattableString.Invariant($@"
UPDATE {LicenceHeaderSchema.Constants.TableName} 
SET {LicenceHeaderSchema.Constants.LA_LicenceAdvStdOth} = '{BillingConstants.BillingModel.STL}' 
WHERE {LicenceHeaderSchema.Constants.PK} IN ({string.Join(",", headersWithStlEdition.Select(x => FormattableString.Invariant($"'{x.PK}'")))});");

							var log = string.Join(System.Environment.NewLine, headersWithStlEdition.Select(x => FormattableString.Invariant($"Organization {x.OrganisationCode}, database {x.DatabaseCode} updated to STL edition")));

							Db.Connection.ExecuteNonQuery(sqlSync);
							ServiceLogger.Log(LogType.Information, log);
						}
					}
				}
			}

			ServiceLogger.Log(LogType.Information, "End processing");
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		protected virtual int MaximumLoadRows => 300;
	}
}
