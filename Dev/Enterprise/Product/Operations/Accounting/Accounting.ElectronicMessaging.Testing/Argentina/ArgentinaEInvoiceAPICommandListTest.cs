using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.CountryCompliance.ArgentinaComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	public class ArgentinaEInvoiceAPICommandListTest : TestCase
	{
		public void TestMessageType_accordingComplianceSubType()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			AssertExceptionThrown<ArgumentException>("Invalid Message Type.", () => ArgentinaEInvoiceAPICommandList.GetMessageType(transactionInfo));

			transactionInfo.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.OCZ;
			AssertExceptionThrown<ArgumentException>("Invalid Message Type.", () => ArgentinaEInvoiceAPICommandList.GetMessageType(transactionInfo));

			foreach (var complianceSubType in GetEInvoicingEligibleComplianceSubTypeList())
			{
				transactionInfo.ComplianceSubType = complianceSubType;

				if (IsExportEInvoiceComplianceSubType(transactionInfo?.ComplianceSubType.Value))
				{
					AssertEquals("Message Type", ArgentinaEInvoiceAPICommandList.Codes.GenerateExportInvoiceRequest, ArgentinaEInvoiceAPICommandList.GetMessageType(transactionInfo));
				}
				else
				{
					AssertEquals("Message Type", ArgentinaEInvoiceAPICommandList.Codes.GenerateLocalInvoiceRequest, ArgentinaEInvoiceAPICommandList.GetMessageType(transactionInfo));
				}
			}
		}

		bool IsExportEInvoiceComplianceSubType(ZString? complianceSubType)
		{
			switch (complianceSubType)
			{
				case ComplianceSubTypeCodes.TXE:
				case ComplianceSubTypeCodes.TDE:
				case ComplianceSubTypeCodes.TCE:
					return true;
				default:
					return false;
			}
		}
	}
}
