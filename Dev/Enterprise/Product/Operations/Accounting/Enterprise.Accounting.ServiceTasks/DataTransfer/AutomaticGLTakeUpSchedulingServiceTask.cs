using System;
using System.Globalization;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.DataTransfer;
using Enterprise.Environment;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	AutomaticGLTakeUpServiceTask.Code,
	"Accounting Automatic Sub Ledger Takeup Service Task",
	"ACC",
	typeof(AutomaticGLTakeUpServiceTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "15minutes",
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true
	)]

namespace Enterprise.Accounting.ServiceTasks.DataTransfer
{
	public class AutomaticGLTakeUpServiceTask : ServiceProviderImpl
	{
		public const string Code = "ATU";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Baseline")]
		public override void RunTask(CancellationToken token)
		{
			var initialUserContext = Env.CurrentUserContext;

			try
			{
				ServiceLogger.Log(LogType.Information, "Automatic Sub Ledger Takeup Service Task started."); // Service Task Logging

				var companies = AccountingUtils.GetAllActiveCompanies(Factory);

				foreach (var company in companies)
				{
					token.ThrowIfCancellationRequested();
					var registrySetting = AccountingConfigurationRegistry.Instance.AllowAutomaticSubLedgerTakeup.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
					if (registrySetting.NextRunDateTime.IsValid)
					{
						var branches = AccountingUtils.GetBranchesOfCompanySortedByTimeZone(company.PK, Factory);
						if (branches.Count > 0)
						{
							using (DisposableEnvironment.ForBranch(branches[0].PK.ToGuid()))
							{
								if (ZDateTime.Now >= registrySetting.NextRunDateTime)
								{
									ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Automatic Sub Ledger Takeup running for Company: '{0}'.", company.GC_Code)); // Service Task Logging

									IAggregateRunner aggregateRunner = Activator.CreateInstance(ObjectFactory.GetType<IAggregateRunner>()) as IAggregateRunner;
									if (aggregateRunner.Aggregate())
									{
										ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Automatic Sub Ledger Takeup successfully ran for Company: '{0}'.", company.GC_Code)); // Service Task Logging
									}
									else
									{
										ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Automatic Sub Ledger Takeup for '{0}' failed with the following error.{1}{2}"
											, company.GC_Code
											, System.Environment.NewLine
											, aggregateRunner.AggregateResult)); // Service Task Logging
									}

									ZDateTime nextRunTime = ZDateTime.Empty;
									switch (registrySetting.IntervalType.ToUpper())
									{
										case "MONTHS":
											nextRunTime = registrySetting.NextRunDateTime.AddMonths(registrySetting.Interval);
											break;
										case "DAYS":
											nextRunTime = registrySetting.NextRunDateTime.AddDays(registrySetting.Interval);
											break;
										case "HOURS":
											nextRunTime = registrySetting.NextRunDateTime.AddHours(registrySetting.Interval);
											break;
										case "MINUTES":
											nextRunTime = registrySetting.NextRunDateTime.AddMinutes(registrySetting.Interval);
											break;
									}

									registrySetting.NextRunDateTime = nextRunTime;
									AccountingConfigurationRegistry.Instance.AllowAutomaticSubLedgerTakeup.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, registrySetting);

									Factory.Save();
								}
							}
						}
						else
						{
							ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "No active branch found for Company: '{0}'.", company.GC_Code)); // Service Task Logging
						}
					}
				}

				ServiceLogger.Log(LogType.Information, "Automatic Sub Ledger Takeup Service Task completed."); // Service Task Logging
			}
			finally
			{
				Env.SetUserContext(initialUserContext); // Putting back the initial context
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
