using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public interface IElectronicInvoicingUpdateActionPermissions
	{
		bool CheckIsComplianceNumberResetAllowed(IComplianceNumberResetStatusInputData inputData);
	}

	public class ElectronicInvoicingUpdateActionPermissions : IElectronicInvoicingUpdateActionPermissions
	{
		bool IElectronicInvoicingUpdateActionPermissions.CheckIsComplianceNumberResetAllowed(IComplianceNumberResetStatusInputData inputData)
		{
			return
				!AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(inputData.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty)
				|| ((ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(inputData.CountryCode) as IInstanceProvider<IComplianceNumberResetStatus>)?.Get().CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(inputData) ?? false)
				|| inputData.EInvoicingStatus.IsEmpty;
		}

		public static ZString CheckComplianceSubTypeAndNumberManualUpdateToAnyValue(ZString ledger, GlbCompany company)
		{
			if (ledger == LedgerTypes.AccountsReceivable
				&& AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				var isDisallowed = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeAndNumberUpdateRules(company.GC_RN_NKCountryCode)?
					.IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed ?? false;

				if (isDisallowed)
				{
					return AccountingConstants.DisallowToUpdateComplianceSubTypeAndNumber;
				}
			}

			return ZString.Empty;
		}

		public static ZString GetErrorMessageForARComplianceSubTypeAndNumberUpdate(AccTransactionHeader[] transactionHeaders)
		{
			var company = transactionHeaders.FirstOrDefault()?.Company;

			return company == null
				? ZString.Empty
				: ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeAndNumberUpdateRules(company.GC_RN_NKCountryCode)?.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders) ?? ZString.Empty;
		}

		public static ZString CheckComplianceSubTypeAndNumberEligibilityWarning(GlbCompany company)
		{
			var eInvoicingIsEnabled = AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var eligibilityDecider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(company.GC_RN_NKCountryCode) as IInstanceProvider<IEInvoicingEligibilityDecider>;
			var supportsLiteEligibility = eligibilityDecider != null;

			if (eInvoicingIsEnabled && !supportsLiteEligibility)
			{
				return AccountingConstants.ComplianceSubTypeAndNumberEligibilityWarning;
			}
			else
			{
				return ZString.Empty;
			}
		}
	}
}
