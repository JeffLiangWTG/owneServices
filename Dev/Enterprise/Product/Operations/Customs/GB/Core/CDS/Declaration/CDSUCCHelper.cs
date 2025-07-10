using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSUCCHelper : UCCHelper
	{
		protected override void DoIfPaymentMethodAndDefermentAccountNumberBothAreSetCore(JobDeclaration declaration, ZString paymentMethod, ZString defermentAccountNumber, JobComInvoiceLine invoiceLine)
		{
			var invoiceLines = invoiceLine == null ? declaration.InvoiceLines.Cast<JobComInvoiceLine>().ToList() : new List<JobComInvoiceLine>() { invoiceLine };
			invoiceLines = invoiceLines.Where(x => x.ZG_MethodOfPayment == "E").ToList();

			if (invoiceLines.Any())
			{
				AddSupportingDocument(declaration, invoiceLines, paymentMethod, defermentAccountNumber, Constants.DocumentCodes.C506);
				AddSupportingDocument(declaration, invoiceLines, paymentMethod, "guaranteenotrequired", Constants.DocumentCodes.C505, "CC", "C", "C");
			}
		}

		void AddSupportingDocument(JobDeclaration declaration, List<JobComInvoiceLine> invoiceLines, ZString paymentMethod, ZString defermentAccountNumber, ZString documentCode, ZString? documentStatus = null, ZString? action = null, ZString? availability = null)
		{
			var authorisationCode = GetAuthorisationCode(documentCode);
			if (!authorisationCode.IsEmpty)
			{
				invoiceLines.ForEach(invLine =>
					AddUpdateSupportingDocument(invLine, documentCode, Invariant($"{declaration.CountryCode}{authorisationCode}{defermentAccountNumber}"), documentStatus ?? ZString.Empty, action ?? ZString.Empty, availability ?? ZString.Empty));
				AddAuthorisationIfNotExists(declaration, paymentMethod, authorisationCode, invoiceLines);
			}
		}

		void AddAuthorisationIfNotExists(JobDeclaration declaration, ZString paymentMethod, ZString authorisationCode, List<JobComInvoiceLine> invoiceLines)
		{
			var eori = declaration?.GetEoriFor(paymentMethod) ?? ZString.Empty;
			var owner = declaration?.GetOrgFor(paymentMethod) ?? ZGuid.Empty;
			if (!eori.IsEmpty && !owner.IsEmpty)
			{
				AddAuthorisationIfNotExists(invoiceLines, authorisationCode, eori, owner);
			}
		}

		protected override void DoIfTaxLinePaymentMethodIsNOrPCore(JobDeclaration declaration)
		{
			var eori = declaration?.DeclarantTraderId ?? ZString.Empty;
			if (!eori.IsEmpty)
			{
				AddGuaranteeIfNotExists(declaration, GuaranteeTypeList.Codes.Guarantee, Constants.GuaranteeCodes.Y, eori);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected ZString GetAuthorisationCode(ZString documentCode)
		{
			switch (documentCode)
			{
				case Constants.DocumentCodes.C501:
					return CDSAuthorisationHeaderTypeList.Codes.AuthorisedEconomicOperatorCustomsSimplifications;
				case Constants.DocumentCodes.C502:
					return CDSAuthorisationHeaderTypeList.Codes.AuthorisedEconomicOperatorSecurityAndSafety;
				case Constants.DocumentCodes.C503:
					return CDSAuthorisationHeaderTypeList.Codes.AuthorisedEconomicOperatorCustomsSimplificationsSecurityAndSafety;
				case Constants.DocumentCodes.C504:
					return CDSAuthorisationHeaderTypeList.Codes.CustomsValue;
				case Constants.DocumentCodes.C505:
					return CDSAuthorisationHeaderTypeList.Codes.ComprehensiveGuarantee;
				case Constants.DocumentCodes.C506:
					return CDSAuthorisationHeaderTypeList.Codes.DeferredPayment;
				case Constants.DocumentCodes.C507:
					return CDSAuthorisationHeaderTypeList.Codes.RepaymentOfTheAmountsOfImportOrExportDuty;
				case Constants.DocumentCodes.C508:
					return CDSAuthorisationHeaderTypeList.Codes.RemissionOfTheAmountsOfImportOrExportDuty;
				case Constants.DocumentCodes.C509:
					return CDSAuthorisationHeaderTypeList.Codes.TemporaryStorage;
				case Constants.DocumentCodes.C510:
					return CDSAuthorisationHeaderTypeList.Codes.RegularShippingServices;
				case Constants.DocumentCodes.C511:
					return CDSAuthorisationHeaderTypeList.Codes.AuthorizedIssuer;
				case Constants.DocumentCodes.C512:
					return CDSAuthorisationHeaderTypeList.Codes.SimplifiedDeclaration;
				case Constants.DocumentCodes.C513:
					return CDSAuthorisationHeaderTypeList.Codes.CentralizedClearance;
				case Constants.DocumentCodes.C514:
					return CDSAuthorisationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				case Constants.DocumentCodes.C515:
					return CDSAuthorisationHeaderTypeList.Codes.SelfAssessment;
				case Constants.DocumentCodes.C516:
					return CDSAuthorisationHeaderTypeList.Codes.TemporaryAdmission;
				case Constants.DocumentCodes.C517:
					return CDSAuthorisationHeaderTypeList.Codes.CustomsWarehousingCWP;
				case Constants.DocumentCodes.C518:
					return CDSAuthorisationHeaderTypeList.Codes.CustomsWarehousingCW1;
				case Constants.DocumentCodes.C519:
					return CDSAuthorisationHeaderTypeList.Codes.CustomsWarehousingCW2;
				case Constants.DocumentCodes.C520:
					return CDSAuthorisationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				case Constants.DocumentCodes.C521:
					return CDSAuthorisationHeaderTypeList.Codes.AuthorizedConsignorTransit;
				case Constants.DocumentCodes.C522:
					return CDSAuthorisationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				case Constants.DocumentCodes.C523:
					return CDSAuthorisationHeaderTypeList.Codes.SpecialSeals;
				case Constants.DocumentCodes.C524:
					return CDSAuthorisationHeaderTypeList.Codes.TransitReducedDataset;
				case Constants.DocumentCodes.C525:
					return CDSAuthorisationHeaderTypeList.Codes.ElectronicTransportDocument;
				case Constants.DocumentCodes.C526:
					return CDSAuthorisationHeaderTypeList.Codes.AuthorizedWeighersOfBananas;
				case Constants.DocumentCodes.C601:
					return CDSAuthorisationHeaderTypeList.Codes.InwardProcessing;
				case Constants.DocumentCodes._1ATR:
					return CDSAuthorisationHeaderTypeList.Codes.AdvanceTariffRuling;
				case Constants.DocumentCodes._1AOR:
					return CDSAuthorisationHeaderTypeList.Codes.AdvanceOriginRuling;
				case Constants.DocumentCodes._1AVR:
					return CDSAuthorisationHeaderTypeList.Codes.AdvanceValuationRuling;
				default:
					return ZString.Empty;
			}
		}
	}
}

