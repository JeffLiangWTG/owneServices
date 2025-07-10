using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class NorwayComplianceReportGUIActionProvider : IComplianceReportGUIActionProvider
	{
		bool IComplianceReportGUIActionProvider.IsCountrySupportGenerateSAFT => true;

		(bool IsNeedCompression, int AllowedMaxSize) IComplianceReportGUIActionProvider.GetSAFTFileCompressionInfo()
		{
			return (true, 10 * 1024 * 1024);
		}

		bool IComplianceReportGUIActionProvider.IsNeedAggregateBeforeGenerate => true;

		string IComplianceReportGUIActionProvider.ValidateBeforeGenerateSAFT(AccComplianceReport[] reports)
		{
			var firstReport = reports.FirstOrDefault();
			result = new StringBuilder();
			var taxIdAndTaxMessageCombinationRules = AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.GetFallBackValueAtAllLevels(firstReport.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty).TaxIdAndTaxMessageCombinationRulesCollection
				.OfType<TaxIdAndTaxMessageCombinationRules>()
				.Where(x => !string.IsNullOrEmpty(x.GovernmentCode));

			if (!taxIdAndTaxMessageCombinationRules.Any())
			{
				AppendResultMessage(Res.GetString("56C671C6-E818-416F-9891-25A6A8F21A2C", @"Please configure the 'Tax ID and Tax Message Combination Rules' registry before exporting the SAF-T file.
This registry cannot be empty."));
			}

			var glOpeningBalanceDetails = reports.SelectMany(x => x.GLOpeningBalanceDetails);
			var glMovementDetails = reports.SelectMany(x => x.GLMovementDetails);
			var glAccounts = firstReport.Factory.Load<AccGLHeader>(new ZQuery());
			var activeGLAccounts = from openingBalance in glOpeningBalanceDetails.Cast<GeneralLedgerBalanceLine>()
								   join balanceMovement in glMovementDetails.Cast<GeneralLedgerBalanceLine>()
								   on openingBalance.AG_AccountNum equals balanceMovement.AG_AccountNum
								   where !(openingBalance.GeneralLedgerAmountDR.IsEmpty && openingBalance.GeneralLedgerAmountCR.IsEmpty && balanceMovement.GeneralLedgerAmountDR.IsEmpty && balanceMovement.GeneralLedgerAmountCR.IsEmpty)
								   select glAccounts.FirstOrDefault(x => x.AG_AccountNum == openingBalance.AG_AccountNum);

			var isSAFTv130FeatureEnabled = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingSAFT13Report) != null;

			if (isSAFTv130FeatureEnabled)
			{
				ValidateAlternateGLAccounts(firstReport, activeGLAccounts);
			}
			else
			{
				var missGLLanguageMappingAccounts = activeGLAccounts.Where(accGlHeader => accGlHeader.AG_AccountType != Core.Constants.AccountType.Note &&
					(AccGLAccountDescriptor.GetLocalAccountDescriptor(accGlHeader.Factory, accGlHeader.PK, SharedConstants.Languages.Norwegian)?.AJ_LocalAccountNumber ?? ZString.Empty).IsEmpty).Distinct();
				if (missGLLanguageMappingAccounts.Any())
				{
					var accountNumbers = string.Join(", ", missGLLanguageMappingAccounts.Select(x => x.AG_AccountNum));

					AppendResultMessage(Res.GetString("D30827D2-135C-4DF1-BABA-3A3668FCD7C9", @"Please create GL Mappings with Language = NB-NO and Country = NO for all active GL Accounts before exporting the SAF-T file.

The Parent GL Accounts are:
{0}.", accountNumbers));
				}
			}

			return result.ToString();
		}

		void ValidateAlternateGLAccounts(AccComplianceReport firstReport, IEnumerable<AccGLHeader> activeGLAccounts)
		{
			var alternateChartId = AccountingConfigurationRegistry.Instance.AlternateChartOfAccountsForSAFT.GetFallBackValueAtAllLevels(firstReport.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty);

			if (alternateChartId == Guid.Empty)
			{
				var emptyChartMessage = Res.GetString("7FF227FB-DCDA-423D-9467-673C291CB809", "To create a SAF-T report for a Norwegian company, it is mandatory to specify an Alternate Chart of Account in the registry under: {0}.",
					AccountingConfigurationRegistry.Instance.AlternateChartOfAccountsForSAFT.HumanReadableRegistryPath());

				AppendResultMessage(emptyChartMessage);

				var missingGLAccounts = activeGLAccounts.Select(acc => acc.AccountNum).Distinct().ToList();

				AppendResultMessage(Res.GetString("E3B0646E-FAF5-4C21-B9F2-0BF327435D2A", @"All accounts must be mapped and the alternate GL account numbers must be in accordance with the Norwegian tax authorities specifications.
The Parent GL Accounts are: {0}.", string.Join(", ", missingGLAccounts)));
			}
			else
			{
				var alternateGLAccounts = firstReport.Factory.Load<AccAlternateGLAccountAttribute>(
						new ZQuery(AccAlternateGLAccountAttributeSchema.AAA_AAC_AlternateChart, alternateChartId).AddToFilter(
						new ZQuery(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, activeGLAccounts.Select(acc => acc.PK))))
					.Select(atr => atr.GLHeader.AccountNum).ToList();
				var missingGLAccounts = activeGLAccounts.Select(acc => acc.AccountNum).Except(alternateGLAccounts).ToList();
				var duplicateGLAccounts = alternateGLAccounts.GroupBy(acc => acc).Where(group => group.Count() > 1).Select(group => group.Key).ToList();

				string alternateChartCode = null;
				string getAlternateChartCode() => alternateChartCode ??= firstReport.Factory.Load<AccAlternateChart>(alternateChartId).AAC_Code;

				if (missingGLAccounts.Count > 0)
				{
					AppendResultMessage(Res.GetString("6643A9B5-B627-46DB-95A7-2E05FB212905", @"One or more GL accounts have not been mapped to the Alternate Chart of Accounts: {0}.", getAlternateChartCode()));
					AppendResultMessage(Res.GetString("BB6E38DB-D487-4483-AEBF-AD3DF4DAD847", @"All accounts must be mapped and the alternate GL account numbers must be in accordance with the Norwegian tax authorities specifications.
The Parent GL Accounts are: {0}.", string.Join(", ", missingGLAccounts)));
				}

				if (duplicateGLAccounts.Count > 0)
				{
					AppendResultMessage(Res.GetString("C245465C-D39D-49DB-86CE-36E7261B0C34", @"All Parent GL accounts must be mapped exactly once in the Alternate Chart of Accounts: {0}.
The following parent GL Accounts are mapped more than once: {1}.", getAlternateChartCode(), string.Join(", ", duplicateGLAccounts)));
				}
			}
		}

		void AppendResultMessage(string message)
		{
			if (result.Length > 0)
			{
				result.AppendLine().AppendLine();
			}
			result.Append(message);
		}

		StringBuilder result;
	}
}
