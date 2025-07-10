using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCustomizationModifiedLogsManager : CommissionAgreementModifiedLogsManager<EdiCommissionAgreementCustomization>
	{
		public EdiCommissionAgreementCustomizationModifiedLogsManager(EdiCommissionAgreementCustomization draft)
			: base(draft)
		{
		}

		protected override ZBool HasChangesThatCanCreateLogs
		{
			get { return base.HasChangesThatCanCreateLogs || NewSource.HasChanges; }
		}

		protected override void AddLogsOnFactorySavingCore()
		{
			var commissionAgreement = NewSource.CommissionAgreement;
			if (MainVersion.IsInDatabase)
			{
				AddLogIfChanged(x => x.EZN_IsAllCompaniesInfo, AutoApprovalHelper.OnlyIfNotEffectiveInThePast(commissionAgreement));
				AddLogIfChanged(x => x.EZN_IsAllDatabasesInfo, AutoApprovalHelper.OnlyIfNotEffectiveInThePast(commissionAgreement));

				Func<ZGuid, string> clientCompanyDescriptionGetter = (clientCompanyPk) =>
				{
					var clientCompany = Factory.Load<ClientCompany>(clientCompanyPk);
					if (clientCompany == null)
					{
						return "Unknown";
					}
					else
					{
						return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", clientCompany.LCC_Name, clientCompany.LCC_Code);
					}
				};

				AddAttachDetachLogs(NewSource.GetCurrentCompanies(), NewSource.OriginalCompanies,
					x => string.Format(CultureInfo.CurrentCulture, "Company Added: {0}", clientCompanyDescriptionGetter(x)),
					x => string.Format(CultureInfo.CurrentCulture, "Company Removed: {0}", clientCompanyDescriptionGetter(x)),
					AutoApprovalHelper.OnlyIfNotEffectiveInThePast(commissionAgreement));

				Func<ZGuid, string> licenceDatabaseDescriptionGetter = (databasePk) =>
				{
					var database = Factory.Load<LicenceDatabase>(databasePk);
					if (database == null)
					{
						return "New Databases";
					}
					else
					{
						return string.Format(CultureInfo.CurrentCulture, "{0} Database", database.LD_ServerCode);
					}
				};

				AddAttachDetachLogs(NewSource.GetCurrentDatabases(), NewSource.OriginalDatabases,
					x => string.Format(CultureInfo.CurrentCulture, "Included {0} Usages", licenceDatabaseDescriptionGetter(x)),
					x => string.Format(CultureInfo.CurrentCulture, "Excluded {0} Usages", licenceDatabaseDescriptionGetter(x)),
					AutoApprovalHelper.OnlyIfNotEffectiveInThePast(commissionAgreement));

				AddAttachDetachLogs(NewSource.GetCurrentCompanyAutoAddCountries(), NewSource.OriginalCompanyAutoAddCountries,
					x => string.Format(CultureInfo.CurrentCulture, "Enabled Auto-add New Companies in {0} Country ({1})", x.Item2, licenceDatabaseDescriptionGetter(x.Item1)),
					x => string.Format(CultureInfo.CurrentCulture, "Disabled Auto-add New Companies in {0} Country ({1})", x.Item2, licenceDatabaseDescriptionGetter(x.Item1)),
					AutoApprovalHelper.OnlyIfNotEffectiveInThePast(commissionAgreement));

				AddAttachDetachLogs(NewSource.GetCurrentCompanyAutoAddDatabases(), NewSource.OriginalCompanyAutoAddDatabases,
					x => string.Format(CultureInfo.CurrentCulture, "Enabled Auto-add All New Companies in {0}", licenceDatabaseDescriptionGetter(x)),
					x => string.Format(CultureInfo.CurrentCulture, "Disabled Auto-add All New Companies in {0}", licenceDatabaseDescriptionGetter(x)),
					AutoApprovalHelper.OnlyIfNotEffectiveInThePast(commissionAgreement));
			}
			else if (MainVersion.CommissionAgreement != null && MainVersion.CommissionAgreement.MainVersion.IsInDatabase)
			{
				MainVersion.CommissionAgreement.ModifiedLogs.AddChangeLog("Added Customer Filters", AutoApprovalHelper.OnlyIfNotEffectiveInThePast(commissionAgreement));
			}
		}

		void AddAttachDetachLogs<T>(IEnumerable<T> currentItems, HashSet<T> originalItems, Func<T, string> itemAtacchedLogReferenceDelegate, Func<T, string> itemDetachedLogReferenceDelegate, bool changeCanBeAutoApproved)
		{
			if (currentItems != null && originalItems != null)
			{
				var currentItemsSet = new HashSet<T>(currentItems);

				var newItems = currentItemsSet.Where(x => !originalItems.Contains(x));
				foreach (var newItem in newItems)
				{
					var message = itemAtacchedLogReferenceDelegate(newItem);
					MainVersion.ModifiedLogs.AddChangeLog(message, changeCanBeAutoApproved);
				}

				var deletedItems = originalItems.Where(x => !currentItemsSet.Contains(x));
				foreach (var deletedItem in deletedItems)
				{
					var message = itemDetachedLogReferenceDelegate(deletedItem);
					MainVersion.ModifiedLogs.AddChangeLog(message, changeCanBeAutoApproved);
				}
			}
		}

		protected override void AddDetachedLogOnDeleteCore()
		{
			MainVersion.CommissionAgreement.ModifiedLogs.AddChangeLog("Deleted Customer Filters", AutoApprovalHelper.OnlyIfNotEffectiveInThePast(NewSource.CommissionAgreement));
		}
	}
}

