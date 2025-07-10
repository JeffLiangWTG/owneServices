using System;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Netting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.Netting;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	NettingTransactionMatchingServiceTask.Code,
	"Netting Transaction Automatic Matching Service Task",
	"ACC",
	typeof(NettingTransactionMatchingServiceTask),
	IsMandatory = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "15minutes",
	DefaultScheduleRunEvery = "1hour")
]

namespace Enterprise.Accounting.ServiceTasks.Netting
{
	public class NettingTransactionMatchingServiceTask : ServiceProviderImpl
	{
		public const string Code = "NTM";

		[HostedServiceRequirement]
		public static string IsNettingSystem() =>
			HostedServiceRequirementAttribute.CheckValueIsNotEqualTo(AccountingConfigurationRegistry.Instance.IsNettingSystem, false);

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Netting matching process starting."));
			try
			{
				var factory = new BusinessObjectFactory();

				var companies = AccountingUtils.GetAllActiveCompanies(factory);

				foreach (var company in companies)
				{
					token.ThrowIfCancellationRequested();
					if (AccountingConfigurationRegistry.Instance.IsNettingSystem.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty))
					{
						var nettingPeriod = NettingPeriodHelper.GetFirstOpenNettingPeriod(company.PK, factory);
						if (nettingPeriod != null)
						{
							NettingMatchTransactions(nettingPeriod, company);

							ZDateTime lastTimeMoveUnmatchedWasPerformed = AccountingConfigurationRegistry.Instance.NettingLastMoveUnmatchedTransactionTimeStamp.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
							if (ZDateTime.UtcNow > nettingPeriod.NSP_LatestApprovalDateUtc
								&& (lastTimeMoveUnmatchedWasPerformed == DateTime.MinValue || lastTimeMoveUnmatchedWasPerformed < nettingPeriod.NSP_LatestApprovalDateUtc))
							{
								NettingHelper.MoveUnmatchedTransactionsToTheNextPeriod(nettingPeriod, factory, Db.Connection);

								AccountingConfigurationRegistry.Instance.NettingLastMoveUnmatchedTransactionTimeStamp.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime());

								ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, "Transactions that could not be matched in current Netting Cycle moved to next Cycle as Latest Approval Date has elapsed for cycle '{0}'.", nettingPeriod.NSP_Period));
							}

							var nextNettingPeriod = NettingPeriodHelper.GetNextOpenPeriod(nettingPeriod, factory);
							if (nextNettingPeriod != null)
							{
								NettingMatchTransactions(nextNettingPeriod, company);
							}
							else
							{
								ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, "No Netting Cycle found after current open Cycle: '{0}'.", nettingPeriod.NSP_Period));
							}
						}
						else
						{
							ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "No open Netting Period found."));
						}
					}
				}
			}
			catch (IncorrectDataSetupException ex)
			{
				ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Netting matching process finished with the following error:{0}{1}", System.Environment.NewLine, ex.Message));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Netting matching process finished abnormally.{0}{1}", System.Environment.NewLine, ex.Message));
			}

			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Netting matching process completed."));
		}

		void NettingMatchTransactions(NettingSystemPeriod nettingPeriod, GlbCompany company)
		{
			foreach (var issuer in NettingHelper.GetIssuerParticipantEHubIds(nettingPeriod))
			{
				foreach (var recipient in NettingHelper.GetRecipientParticipantEHubIds(nettingPeriod))
				{
					if (!issuer.Equals(recipient))
					{
						ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, "Netting matching process starting for company: '{0}', Netting Cycle: '{1}', Issuer: '{2}', Recipient: '{3}'."
											, company.GC_Code, nettingPeriod.NSP_Period, issuer, recipient));

						NettingHelper.NettingMatchTransactions(Db.Connection, nettingPeriod.PK.ToGuid(), issuer, recipient, company.PK.ToGuid(), GlbStaff.CurrentUser.GS_Code.ToString());

						ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, "Netting matching process ran successfully for company: '{0}', Netting Cycle: '{1}', Issuer: '{2}', Recipient: '{3}'."
											, company.GC_Code, nettingPeriod.NSP_Period, issuer, recipient));
					}
				}
			}
		}
	}
}
