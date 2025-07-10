using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.AutoLicensing.ServiceTasks;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	AutoLicensingServiceTask.Code,
	AutoLicensingServiceTask.Description,
	"CSP",
	typeof(AutoLicensingServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "10minutes",
	DefaultScheduleRunEvery = "15minutes",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(AutoLicensingServiceTask.Code, StmNoteSchema.Constants.TableName,
	new[]
	{
		StmNoteSchema.Constants.ST_Table + "=" + LicenceDatabaseSchema.Constants.TableName
	}, null)]

namespace Enterprise.Client.EDI.Licencing.AutoLicensing.ServiceTasks
{
	public class AutoLicensingServiceTask : ServiceProviderImpl
	{
		public const string Code = "ALC";
		public const string Description = "Auto Licensing Service Task";

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, Description + " started.");

			try
			{
				var enterpriseSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceDatabaseSchema.LD_LE);
				enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseID, EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.Value);

				var filter = new ZDBOnlyQuery(typeof(LicenceDatabase));
				filter.AddSubQuery(enterpriseSubQuery, JoinCondition.And);
				filter.AddToFilter(LicenceDatabaseSchema.LD_OH_WebAccessOrg, SQLComparisonOperator.Equal, null);
				filter.AddToFilter(LicenceDatabaseSchema.LD_MasterOrgSuggestedUTC, SQLComparisonOperator.Equal, null);
				filter.OrderBy = LicenceDatabaseSchema.Constants.LD_DatabaseNumber;
				var reader = new FilteredBusinessObjectReader<LicenceDatabase>(filter, new BusinessObjectFactory());
				reader.BatchSize = 100;
				reader.SaveBeforeLoadNextEnabled = true;

				foreach (LicenceDatabase database in reader)
				{
					token.ThrowIfCancellationRequested();
					FindSuggestions(database);
					database.LD_MasterOrgSuggestedUTC = ZDateTime.UtcNow;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var errorMessage = FormattableString.Invariant($"Error finding suggestions");
				ErrorReporter.ReportOnce(errorMessage, ex);
				ServiceLogger.Error(errorMessage, ex);
			}

			ServiceLogger.Log(LogType.Information, Description + " finished.\r\n");
		}

		void FindSuggestions(LicenceDatabase database)
		{
			try
			{
				database.OrgSuggestionCollections.DeleteAll();
				foreach (var match in new Matcher().GetMatches(database))
				{
					var suggestion = database.Factory.New<EdiLicenceDatabaseOrgSuggestion>();
					suggestion.LDS_LD = database.PK;
					suggestion.LDS_OH = match.Org.PK;
					suggestion.LDS_TotalScore = match.TotalScore;
				}
				database.Factory.Save();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				var message = FormattableString.Invariant($"Error finding suggestions for database {database.PK}");
				ServiceLogger.Log(LogType.Error, message, e);
			}
		}
	}
}
