using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing.KoreaSouth
{
	public static class InvoiceeIDProvider
	{
		public static (ZString InvoiceePartyIdTypeCode, ZString InvoiceeId) GetInvoiceeID(TransactionInfo transactionInfo, Func<OrganizationAddress, ZString, ZString, string> getRegistrationCode)
		{
			var invoiceeIdTypeCode = OrgCusCode.CodeTypes.VATCode;
			var invoiceeId = getRegistrationCode(transactionInfo.OrganizationAddress, CountryCodes.KoreaSouth, invoiceeIdTypeCode);

			if (string.IsNullOrEmpty(invoiceeId))
			{
				invoiceeIdTypeCode = KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForResident;
				invoiceeId = getRegistrationCode(transactionInfo.OrganizationAddress, CountryCodes.KoreaSouth, invoiceeIdTypeCode);
			}

			if (string.IsNullOrEmpty(invoiceeId))
			{
				invoiceeIdTypeCode = KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForForeigner;
				invoiceeId = !string.IsNullOrEmpty(getRegistrationCode(transactionInfo.OrganizationAddress, CountryCodes.KoreaSouth, invoiceeIdTypeCode))
					? KoreaSouthComplianceInfo.ForeignerID
					: default;
			}

			if (string.IsNullOrEmpty(invoiceeId))
			{
				invoiceeIdTypeCode = default;
			}

			return (invoiceeIdTypeCode, invoiceeId);
		}

		public static ZString GetInvoiceeID(OrgHeader orgHeader)
		{
			return orgHeader?.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes
			(
				CountryCodes.KoreaSouth,
				OrgCusCode.CodeTypes.VATCode,
				KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForResident,
				KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForForeigner
			)?.OK_CustomsRegNo ?? ZString.Empty;
		}
	}
}
