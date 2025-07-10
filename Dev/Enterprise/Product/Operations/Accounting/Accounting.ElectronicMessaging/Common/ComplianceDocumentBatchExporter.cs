using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class ComplianceDocumentBatchExporter
	{
		public ComplianceDocumentBatch CreateComplianceDocumentBatch(AccEInvoicingBatch eInvoicingBatch)
		{
			var complianceDocumentBatch = new ComplianceDocumentBatch();
			complianceDocumentBatch.BatchNumber = eInvoicingBatch.AIB_BatchNumber;
			complianceDocumentBatch.CompanyPK = eInvoicingBatch.AIB_GC;
			complianceDocumentBatch.ComplianceDocumentHeaderDetails = GetComplianceDocumentHeaderDetails(eInvoicingBatch);

			return complianceDocumentBatch;
		}

		ComplianceDocumentHeaderDetail[] GetComplianceDocumentHeaderDetails(AccEInvoicingBatch eInvoicingBatch)
		{
			var complianceDocuments = eInvoicingBatch.LoadComplianceDocumentsReadyToBeSent();
			var complianceDocumentHeaderDetails = new List<ComplianceDocumentHeaderDetail>();
			foreach (var complianceDocument in complianceDocuments)
			{
				var complianceDocumentHeaderDetail = new ComplianceDocumentHeaderDetail();
				complianceDocumentHeaderDetail.HeaderPK = complianceDocument.PK;
				complianceDocumentHeaderDetail.TransactionType = complianceDocument.ADH_TransactionType;
				complianceDocumentHeaderDetail.DocumentNumber = complianceDocument.ADH_DocumentNumber;
				complianceDocumentHeaderDetail.DocumentDate = complianceDocument.ADH_DocumentDate;

				if (complianceDocument.ADH_TransactionType == TransactionTypes.CreditNote)
				{
					complianceDocumentHeaderDetail.OriginalDocumentDate = complianceDocument.INVComplianceDocumentHeaderForCRD?.ADH_DocumentDate;
				}
				if (complianceDocument.IsVoided)
				{
					var voidLog = complianceDocument.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.EditedARecord.Code && x.SL_Reference == AccComplianceDocumentHeader.VoidLogReference);
					if (voidLog != null)
					{
						complianceDocumentHeaderDetail.VoidDate = voidLog.First().SL_EventTime;
					}
				}
				complianceDocumentHeaderDetail.SystemVATRegistrationNum =
					complianceDocument.Company.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, complianceDocument.Company.GC_RN_NKCountryCode)?.OK_CustomsRegNo
					?? string.Empty;
				complianceDocumentHeaderDetail.BarCode = complianceDocument.ADH_BarCode;
				complianceDocumentHeaderDetail.Description = complianceDocument.ADH_Description;
				complianceDocumentHeaderDetail.InternalReference = complianceDocument.ADH_InternalReference;
				complianceDocumentHeaderDetail.ComplianceDocumentLineDetails = GetComplianceDocumentLineDetails(complianceDocument);

				complianceDocumentHeaderDetail.OrgHeaderDetail = GetOrgHeaderDetail(complianceDocument);
				complianceDocumentHeaderDetail.VoidingReason = complianceDocument.ADH_VoidingReason;
				complianceDocumentHeaderDetail.ApprovalNumber = complianceDocument.ADH_ApprovalNumber;
				complianceDocumentHeaderDetail.IsSpecialVoiding = !complianceDocument.ADH_VoidingReason.IsEmpty;

				complianceDocumentHeaderDetails.Add(complianceDocumentHeaderDetail);
			}

			return complianceDocumentHeaderDetails.ToArray();
		}

		OrgHeaderDetail GetOrgHeaderDetail(AccComplianceDocumentHeader complianceDocument)
		{
			var orgHeaderDetail = new OrgHeaderDetail();
			orgHeaderDetail.Category = complianceDocument.OrgHeaderCategory;
			orgHeaderDetail.CompanyName = complianceDocument.AddressOverride?.CompanyName ?? ZString.Empty;
			orgHeaderDetail.VATRegistrationNum = complianceDocument.VATRegistrationNum;
			orgHeaderDetail.CountryCode = complianceDocument.Organisation?.CountryCode ?? ZString.Empty;

			var customsCodes = complianceDocument.Organisation?.CustomsCodes.Cast<OrgCusCode>().Where(x => x.OK_RN_NKCodeCountry == complianceDocument.Company.GC_RN_NKCountryCode);
			orgHeaderDetail.MCIRegistrationNum = customsCodes?.FirstOrDefault(x => x.OK_CodeType == OrgCusCode.TaiwanCodeTypes.MCI)?.OK_CustomsRegNo ?? ZString.Empty;
			orgHeaderDetail.PIGRegistrationNum = customsCodes?.FirstOrDefault(x => x.OK_CodeType == OrgCusCode.TaiwanCodeTypes.PIG)?.OK_CustomsRegNo ?? ZString.Empty;

			return orgHeaderDetail;
		}

		ComplianceDocumentLineDetail[] GetComplianceDocumentLineDetails(AccComplianceDocumentHeader complianceDocument)
		{
			var complianceDocumentLineDetails = new List<ComplianceDocumentLineDetail>();
			foreach (AccComplianceDocumentLine complianceDocumentLine in complianceDocument.OriginalComplianceDocumentLines)
			{
				var complianceDocumentLineDetail = new ComplianceDocumentLineDetail();
				complianceDocumentLineDetail.LinePK = complianceDocumentLine.PK;
				complianceDocumentLineDetail.LineDescription = complianceDocumentLine.ADL_Description;
				complianceDocumentLineDetail.TaxCode = complianceDocumentLine.TaxRate?.AT_Code ?? ZString.Empty;
				complianceDocumentLineDetail.Rate = complianceDocumentLine.TaxRate?.RateForTodayForUIBinding ?? 0M;
				complianceDocumentLineDetail.Amount = complianceDocumentLine.LocalAmount;
				complianceDocumentLineDetail.TaxAmount = complianceDocumentLine.LocalTaxAmount;

				complianceDocumentLineDetails.Add(complianceDocumentLineDetail);
			}

			return complianceDocumentLineDetails.ToArray();
		}
	}
}
