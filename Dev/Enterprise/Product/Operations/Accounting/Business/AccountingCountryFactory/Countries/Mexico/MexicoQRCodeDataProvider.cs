using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class MexicoQRCodeDataProvider : IQRCodeDataProvider
	{
		string IQRCodeDataProvider.GetTransactionQRCodeString(InvoicingBase invoice)
		{
			var result = string.Empty;

			var governmentAllocatedNumber = invoice.EInvoicingGovernmentAllocatedNumber;
			if (governmentAllocatedNumber.IsEmpty)
			{
				return result;
			}

			var transactionHeaderAuthorisationRecord = AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(invoice.Factory, invoice.PK, invoice.Company.Country.RN_Code);
			var authorisationRecordIDNumber = transactionHeaderAuthorisationRecord?.AHF_IDNumber ?? ZString.Empty;
			var debtorOrganizationCode = GetDebtorOrganizationCode(invoice);
			var oSInvoiceTotal = invoice.AH_OSTotal;
			var issuerAuthorizationData = GetLastCharacters(Convert.ToBase64String(transactionHeaderAuthorisationRecord?.AHF_IssuerAuthorizationData ?? ZBlob.Empty), 8);

			#region SuppressResourceStringsCheckRegion

			result = $"https://verificacfdi.facturaelectronica.sat.gob.mx/default.aspx?id={governmentAllocatedNumber}&re={authorisationRecordIDNumber}&rr={debtorOrganizationCode}&tt={oSInvoiceTotal}&fe={issuerAuthorizationData}";

			#endregion

			return result;
		}

		ZString GetDebtorOrganizationCode(InvoicingBase invoice)
		{
			var debtorOrganizationCode = ZString.Empty;
			var orgProxyCustomsCodes = invoice.Header?.CustomsCodes;

			if (orgProxyCustomsCodes != null)
			{
				debtorOrganizationCode = orgProxyCustomsCodes
					.GetOrgCusCodeObjectMatchingCountryAndCodes(Constants.CountryCodes.Mexico, MexicoOrgCusCodeInfo.OrgCusCodes.RFC, MexicoOrgCusCodeInfo.OrgCusCodes.RFG)?.OK_CustomsRegNo ?? ZString.Empty;
			}

			return debtorOrganizationCode;
		}

		string GetLastCharacters(string source, int subStringLenght)
		{
			return subStringLenght < source.Length ? source.Substring(source.Length - subStringLenght) : source;
		}
	}
}
