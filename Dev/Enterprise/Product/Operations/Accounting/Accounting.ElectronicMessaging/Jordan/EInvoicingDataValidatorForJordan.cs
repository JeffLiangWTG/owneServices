using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Jordan
{
	public class EInvoicingDataValidatorForJordan(GlbCompany company, bool shouldSendErrorNotificationEmail = false)
			: BaseEInvoicingDataValidator(company, shouldSendErrorNotificationEmail)
	{
		protected override void RunCore(ILogger logger) => ValidateBatchedTransactions(logger);

		public override IReadOnlyCollection<ZString> ValidateTransaction(InvoicingBase transaction, AccEInvoicingTransactionPivot pivot)
		{
			var errorMessages = new List<ZString>();

			CheckClientIDAndSecret(transaction, errorMessages);
			CheckRegistryNumberForProxy(transaction, errorMessages);

			return errorMessages;
		}

		void CheckClientIDAndSecret(InvoicingBase transaction, List<ZString> errorMessages)
		{
			var eInvoicingCredentials = AccountingElectronicMessagingRegistry.Instance.JordanEInvoicingCredentials.Value;
			if (eInvoicingCredentials.ClientId.IsEmpty || eInvoicingCredentials.ClientSecret.IsEmpty)
			{
				errorMessages.Add(Res.GetString("7FDA1886-79B3-42B2-B406-65EC942CF3C6", "Jordan E-Invoicing Client ID and Secret Key must be set against the registry 'E-Invoicing Credentials'."));
			}
		}

		void CheckRegistryNumberForProxy(InvoicingBase transaction, List<ZString> errorMessages)
		{
			var branchOrgProxy = transaction.Branch.OrgProxy;
			if (branchOrgProxy != null)
			{
				CheckProxyRegistryNumber(branchOrgProxy, (NoResString)"Branch", errorMessages);
			}
			else
			{
				CheckProxyRegistryNumber(transaction.Company.OrgProxy, (NoResString)"Company", errorMessages);
			}
		}

		void CheckProxyRegistryNumber(OrgHeader proxy, string proxyLevel, List<ZString> errorMessages)
		{
			if (GetRegistrationNumberByType(proxy, OrgCusCode.CodeTypes.GSTCode).IsEmpty)
			{
				errorMessages.Add(Res.GetString("E69F47F9-4A54-4601-988F-ECC42C871049", "Please record JO GST - Government GST Code against the Login {0} Organization Proxy.", proxyLevel));
			}

			if (GetRegistrationNumberByType(proxy, JordanOrgCusCodeInfo.OrgCusCodes.BusinessActivityNumber).IsEmpty)
			{
				errorMessages.Add(Res.GetString("AB867E3D-B9E8-4D34-AC3D-26845772CDFF", "Please record JO BAN - Business Activity Number against the Login {0} Organization Proxy.", proxyLevel));
			}
		}

		ZString GetRegistrationNumberByType(OrgHeader orgHeader, string codeType) => orgHeader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(codeType, Constants.CountryCodes.Jordan)?.OK_CustomsRegNo ?? ZString.Empty;
	}
}
