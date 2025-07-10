using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class Import5ULCreator
	{
		public Import5ULHeader Create(CusEntryHeader importEntry, GOVCBR5ULDetails refundDetails)
		{
			var import5ULData = new Import5ULHeader();
			var entryNumberIMP = importEntry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == SharedJobMessageTypeList.Codes.Import);
			import5ULData.RefundDeclarationNumber = refundDetails.RefundRequestNumber;
			import5ULData.RefundType = refundDetails.RefundType;
			import5ULData.RefundCauseCode = refundDetails.RefundCause;
			import5ULData.RefundReasonCode = refundDetails.RefundReason;

			var declaration = importEntry.Declaration;
			import5ULData.DeclarationCustomsOffice = declaration.JE_CustomsOffice;
			import5ULData.DeclarationCustomsDivision = declaration.CustomsOfficeRefundDepartment;
			import5ULData.TaxOfficeCode = declaration.JE_TaxOffice;
			PopulateImport5ULPayer(declaration.PayerAddress, import5ULData);

			var import5ULLineData = CreateEntryLineDataA(importEntry, entryNumberIMP, refundDetails);
			import5ULData.EntryLines = new Import5ULEntryLine[] { import5ULLineData };
			import5ULData.Are5FE_5ULToBeSentTogether = refundDetails.Are5FE_5ULToBeSentTogether;
			import5ULData.TotalRefundAmount = import5ULLineData.TotalOtherTaxItemAmount;

			return import5ULData;
		}

		Import5ULEntryLine CreateEntryLineDataA(CusEntryHeader entry, CusEntryNumber entryNumberIMP, GOVCBR5ULDetails refundDetails)
		{
			var result = new Import5ULEntryLine();

			result.ImportDeclarationNumber = entryNumberIMP?.CE_EntryNum ?? ZString.Empty;
			result.RefundLineNo = 1;
			result.VersionNumber5WN = (ZInt)refundDetails.Amendment5WNVersionNumber;
			result.SoABillNumber = refundDetails.DisbursementBillNumber;

			Populate5ULTaxLineA(entry, result, refundDetails);
			return result;
		}

		readonly string[] otherTaxItems = { EntryTaxTypeList.Codes._5CQ, EntryTaxTypeList.Codes._5CR, EntryTaxTypeList.Codes._5AC, EntryTaxTypeList.Codes._5AY, EntryTaxTypeList.Codes._5CT, EntryTaxTypeList.Codes._5CS };
		readonly string[] taxItems = { EntryTaxTypeList.Codes.CUD, EntryTaxTypeList.Codes._5AB, EntryTaxTypeList.Codes.CAP, EntryTaxTypeList.Codes.VAT, EntryTaxTypeList.Codes.ACT, EntryTaxTypeList.Codes.IND, EntryTaxTypeList.Codes.ENV };
		void Populate5ULTaxLineA(CusEntryHeader entry, Import5ULEntryLine result, GOVCBR5ULDetails refundDetails)
		{
			var taxItemDataList = new List<Import5ULTaxItem>();
			var otherTaxItemDataList = new List<Import5ULTaxItem>();

			foreach (string taxItem in taxItems.Concat(otherTaxItems))
			{
				var tax = refundDetails.RefundAmounts.FirstOrDefault(x => x.Key == taxItem).Value;
				var penaltyAmount = refundDetails.PenaltiesToRefund.FirstOrDefault(x => x.Key == taxItem).Value;

				if (taxItems.Contains(taxItem))
				{
					//관세:CUD / 교육세:5AB / 농특세:CAP / 부가세:VAT / 주세:ACT / 개별소비세:IND / 교통에너지환경세의 세액 + 가산세:ENV
					taxItemDataList.Add(Create5ULTaxItem(taxItem, tax, penaltyAmount));
				}
				else
				{
					//부가세 과세표준:5CQ / 부가세 면세표준:5CR제외 / 신고지연가산세:5AC / 미신고가산세:5AY / 납기경과가산금:5CT / 세외수입금액:5CS의 기타세목금액
					otherTaxItemDataList.Add(Create5ULTaxItem(taxItem, tax));
				}
			}

			result.TaxItems = taxItemDataList.Count > 0 ? taxItemDataList.ToArray() : null;
			result.OtherTaxItems = otherTaxItemDataList.Count > 0 ? otherTaxItemDataList.ToArray() : null;
		}

		Import5ULTaxItem Create5ULTaxItem(string code, ZDecimal refundAmount, decimal penaltyToRefund = decimal.Zero)
		{
			var taxItemData = new Import5ULTaxItem();
			taxItemData.TaxItem = code;
			taxItemData.Tax = refundAmount;
			taxItemData.PenaltyAmount = penaltyToRefund;

			return taxItemData;
		}

		public Import5ULHeader Create(CusReconDeclaration reconDeclaration)
		{
			var import5ULHeaderData = new Import5ULHeader();
			import5ULHeaderData.RefundDeclarationNumber = reconDeclaration.RefundDeclarationNumber;
			import5ULHeaderData.RefundType = reconDeclaration.CRD_DeclarantType;
			import5ULHeaderData.RefundCauseCode = reconDeclaration.CRD_RefundCauseCode;
			import5ULHeaderData.RefundReasonCode = reconDeclaration.CRD_RefundReasonCode;
			import5ULHeaderData.DeclarationCustomsOffice = reconDeclaration.CRD_CustomsOffice;
			import5ULHeaderData.DeclarationCustomsDivision = reconDeclaration.CRD_CustomsDivision;
			import5ULHeaderData.TaxOfficeCode = reconDeclaration.CRD_TaxOffice;
			import5ULHeaderData.Are5FE_5ULToBeSentTogether = false;
			PopulateImport5ULPayer(reconDeclaration.DeclarantAddress, import5ULHeaderData);

			decimal totalRefundAmount = 0;
			var import5ULLineDataList = new List<Import5ULEntryLine>();
			foreach (var entryLine in reconDeclaration.CusReconEntryLines)
			{
				var import5ULLineData = new Import5ULEntryLine();
				var reconEntry = entryLine.Header;
				import5ULLineData.ImportDeclarationNumber = reconEntry.CRE_OriginalEntryNumber;
				import5ULLineData.RefundLineNo = entryLine.CRL_LineNumber;
				import5ULLineData.ImportEntryLineNo = entryLine.CRL_OriginalEntryLineNumber;
				import5ULLineData.VersionNumber5WN = reconEntry.CRE_Amendment5WNVersionNumber;
				import5ULLineData.VATDecisionDate = ZDateTime.Today.ToDateTime();
				import5ULLineData.SoABillNumber = reconEntry.CRE_CustomsBillNumber;
				import5ULLineData.TotalOtherTaxItemAmount = entryLine.TotalRefundAmount;

				var contractRevocation = entryLine.ContractRevocation;
				if (contractRevocation != null)
				{
					import5ULLineData.CancelReasonCode = contractRevocation.CSI_Code;
					if (import5ULLineData.CancelReasonCode == CancelReasonCodeList.Codes.A)
					{
						import5ULLineData.ExportDeclarationNumber = contractRevocation.CSI_ReferenceNumber;
						import5ULLineData.ExportEntryLineNo = contractRevocation.CSI_LineNo;
					}
					else
					{
						if (contractRevocation.CSI_DateOfExpiry.IsValid)
						{
							import5ULLineData.DisposalDate = contractRevocation.CSI_DateOfExpiry.ToDateTime();
						}
						import5ULLineData.GoodsLocationDescription = contractRevocation.CSI_AdditionalDescription;
						import5ULLineData.ResidualSubstanceDescription = contractRevocation.CSI_ReferenceNumber2;
						import5ULLineData.DisposalNumber = contractRevocation.CSI_ReferenceNumber;
						import5ULLineData.DamageSituation = contractRevocation.CSI_Description;
					}
				}
				totalRefundAmount += import5ULLineData.TotalOtherTaxItemAmount;
				CreateTaxItems(import5ULLineData, entryLine);

				var import5ULInvoiceLineDataList = new List<Import5ULInvoiceLine>();
				foreach (var invoiceLines in entryLine.RefundInvoiceLines)
				{
					var import5ULInvoiceLineData = new Import5ULInvoiceLine();
					import5ULInvoiceLineData.InvoiceLineNo = invoiceLines.CSI_LineNo;
					import5ULInvoiceLineData.HSDescription = invoiceLines.CSI_AdditionalDescription;
					import5ULInvoiceLineData.ItemDescription = invoiceLines.CSI_Description;
					import5ULInvoiceLineData.RefundQuantity = invoiceLines.CSI_Quantity;
					import5ULInvoiceLineData.InvoiceQuantity = invoiceLines.CSI_Quantity2;
					import5ULInvoiceLineData.UnitPrice = invoiceLines.CSI_Value;
					import5ULInvoiceLineDataList.Add(import5ULInvoiceLineData);
				}
				import5ULLineData.InvoiceLines = import5ULInvoiceLineDataList.ToArray();

				import5ULLineDataList.Add(import5ULLineData);
			}
			import5ULHeaderData.TotalRefundAmount = totalRefundAmount;
			import5ULHeaderData.EntryLines = import5ULLineDataList.ToArray();

			return import5ULHeaderData;
		}

		void CreateTaxItems(Import5ULEntryLine import5ULLineData, CusReconEntryLine entryLine)
		{
			var taxItemsList = new List<Import5ULTaxItem>();
			var otherTaxItemsList = new List<Import5ULTaxItem>();

			foreach (var charge in entryLine.CusReconCharges)
			{
				var chargeType = EntryTaxTypeList.Is5ULPenaltyChargeType(charge.CRC_ChargeType)
					? EntryTaxTypeList.GetCustomsChargeTypeByPenaltyType(charge.CRC_ChargeType)
					: EntryTaxTypeList.GetCorrespondingChargeTypeCW1ToKRCFor5UL(charge.CRC_ChargeType);

				Import5ULTaxItem taxItem = null;
				if (taxItems.Contains(chargeType))
				{
					taxItem = GetOrCreateTaxItem(taxItemsList, chargeType);
					if (EntryTaxTypeList.Is5ULPenaltyChargeType(charge.CRC_ChargeType))
					{
						taxItem.PenaltyAmount = charge.CRC_Amount;
					}
					else
					{
						taxItem.Tax = charge.CRC_Amount;
					}
				}

				Import5ULTaxItem otherTaxItem = null;
				if (otherTaxItems.Contains(chargeType))
				{
					otherTaxItem = GetOrCreateTaxItem(otherTaxItemsList, chargeType);
					otherTaxItem.Tax = charge.CRC_Amount;
				}
			}

			import5ULLineData.TaxItems = taxItemsList.ToArray();
			import5ULLineData.OtherTaxItems = otherTaxItemsList.ToArray();

			Import5ULTaxItem GetOrCreateTaxItem(List<Import5ULTaxItem> taxItemsList, string chargeType)
			{
				var taxItem = taxItemsList.SingleOrDefault(x => x.TaxItem == chargeType);
				if (taxItem == null)
				{
					taxItem = new Import5ULTaxItem { TaxItem = chargeType };
					taxItemsList.Add(taxItem);
				}
				return taxItem;
			}
		}
		void PopulateImport5ULPayer(OrgAddress address, Import5ULHeader import5ULData)
		{
			if (address != null)
			{
				var dutyPayer = address.Header;
				import5ULData.Payer = new Organisation(RoleType.Payer)
				{
					CompanyName = address.CompanyName,
					RepresentativeName = dutyPayer.GetRepresentativeName(),
					AddressLine1 = address.Address1,
					AddressLine2 = address.Address2,
					Postcode = address.Postcode,
					BuildingNumber = address.GetBuildingNumber(),
					RoadNameCode = address.GetRoadNameCode(),
					IsIndividual = dutyPayer.GetIsIndividual(),
				};

				var idNumbers = address.GetRegistrationIDNumbers(dutyPayer.GetIsIndividual() ? new string[] { IdentificationType.KoreanRegNoForResident, IdentificationType.UnipassIDForIndividual } : new string[] { IdentificationType.BusinessRegNo, IdentificationType.KoreanRegNoForResident, IdentificationType.UnipassIDForOrganization });
				import5ULData.Payer.SetRegistrationIDNumbers(idNumbers);

				var payer = OrgHeaderWrapper.New(dutyPayer);
				import5ULData.BankCode = payer.ZO_BankCode;
				import5ULData.BankAccountNumber = payer.ZO_BankAccNo;
			}
		}
	}
}
