using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.CountryCompliance.ArgentinaComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	public class ArgentinaEInvoiceAPICommandList : CodeDescriptionPairList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateLocalInvoiceRequest = EInvoiceAPICommandList.Codes.GenerateLocalInvoiceRequest;
			public const string GenerateExportInvoiceRequest = EInvoiceAPICommandList.Codes.GenerateExportInvoiceRequest;
			public const string GenerateDetailItemsInvoiceRequest = EInvoiceAPICommandList.Codes.GenerateDetailItemsInvoiceRequest;
		}

		public static HashSet<string> GetExportEInvoiceComplianceSubTypeList()
		{
			return new HashSet<string>
			{
				ComplianceSubTypeCodes.TXE,
				ComplianceSubTypeCodes.TDE,
				ComplianceSubTypeCodes.TCE
			};
		}

		public static ZString GetMessageType(TransactionInfo transactionInfo)
		{
			if (GetExportEInvoiceComplianceSubTypeList().Contains(transactionInfo.ComplianceSubType.GetValueOrDefault()))
			{
				return Codes.GenerateExportInvoiceRequest;
			}

			if (GetEInvoicingEligibleComplianceSubTypeList().Contains(transactionInfo.ComplianceSubType.GetValueOrDefault()))
			{
				if (!isReportDomesticTransactionsWithWSMTXCAWebService)
				{
					return Codes.GenerateLocalInvoiceRequest;
				}
				else
				{
					return Codes.GenerateDetailItemsInvoiceRequest;
				}
			}

			throw new ArgumentException("Invalid Compliance Sub Type.");
		}

		static bool isReportDomesticTransactionsWithWSMTXCAWebService => AccountingElectronicMessagingRegistry.Instance.ReportDomesticTransactionsWithWSMTXCAWebService.Value;
	}
}
