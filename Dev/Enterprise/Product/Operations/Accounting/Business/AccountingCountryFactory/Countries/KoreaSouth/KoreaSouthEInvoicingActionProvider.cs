using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class KoreaSouthEInvoicingActionProvider : IEInvoicingActionProvider
	{
		public void OnEvaluateEligibilityAndQueue(AccTransactionHeader transaction)
		{
			Argument.NotNull(transaction, nameof(transaction));
			if (transaction.Company.Country.Code == CountryCodes.KoreaSouth && IsIssueIdEmpty(transaction))
			{
				var reference = transaction.Factory.New<AccTransactionHeaderReference>();
				reference.AH1_AH = transaction.PK;
				reference.AH1_Type = AccTransactionHeaderReferenceTypes.KRI;
				reference.AH1_Reference = CreateIssueId(transaction);
			}
		}

		public bool SupportPendingInvoiceAction => AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;

		string CreateIssueId(AccTransactionHeader transaction)
		{
			var part1 = transaction.AH_InvoiceDate.ToString("yyyyMMdd");
			var part2 = GetKoreaSouthRegistryNumber(transaction.Company.PK.ToGuid());
			var part3 = Env.NumberFountains.GetKoreaSouthIssueId().GetNextFormatted(transaction.Factory);
			return part1 + part2 + part3;
		}

		string GetKoreaSouthRegistryNumber(Guid companyPK)
		{
			return IsProductionSystem(companyPK)
				? KoreaSouthRegistryNumber.Production
				: KoreaSouthRegistryNumber.Testing;
		}

		bool IsProductionSystem(Guid companyPK)
		{
			switch (AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty))
			{
				case EReportingGEIMessageSystemTypeCodes.AlwaysProductionSystem:
					return true;
				case EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem:
					return false;
				case EReportingGEIMessageSystemTypeCodes.Default:
				default:
					return Env.Instance.IsProductionSystem;
			}
		}

		bool IsIssueIdEmpty(AccTransactionHeader transaction)
		{
			var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_AH, transaction.PK)
				.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccTransactionHeaderReferenceTypes.KRI);
			return !transaction.Factory.Exists(typeof(AccTransactionHeaderReference), query);
		}
	}
}
