using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;
using CusEntryLineFee = Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee;

namespace Enterprise.Customs.DE.Business.DocumentWrappers
{
	public class DocCusEntryHeader : DocBaseCusEntryHeader
	{
		DocCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
			: base(cusEntryHeader, factoryToWrap)
		{
		}

		public static DocCusEntryHeader New(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap) => cusEntryHeader == null ? null : new DocCusEntryHeader(cusEntryHeader, factoryToWrap);

		public DocCusEntryLineCollection MergedLines
		{
			get
			{
				if (mergedLines == null)
				{
					mergedLines = new DocCusEntryLineCollection(EntryHeader.MergedLines, Factory);
					var isFinal = IsFinalTaxReport == YesNoList.Codes.Yes;
					if (isFinal)
					{
						foreach (var line in mergedLines.Cast<DocCusEntryLine>())
						{
							line.UseConfirmedFees = true;
						}
					}
				}
				return mergedLines;
			}
		}
		DocCusEntryLineCollection mergedLines;

		public DocPreviousDocumentCollection PreviousDocuments => previousDocuments ??= EntryInstruction != null
				? new DocPreviousDocumentCollection(EntryInstruction.PreviousDocuments.Cast<PreviousDocument>().Where(d => !d.CSI_Code.IsEmpty), Factory)
				: new DocPreviousDocumentCollection(Factory);

		DocPreviousDocumentCollection previousDocuments;
		public ZString PreviousDocumentsExist => PreviousDocuments.Count > 0 ? "Y" : "N";

		public DocSupportingDocumentCollection SupportingDocuments
			=> supportingDocuments ??= new DocSupportingDocumentCollection(EntryInstruction.Invoices.Cast<JobComInvoiceHeader>().SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>()), Factory);
		DocSupportingDocumentCollection supportingDocuments;

		public DocEntryFeeCollection Fees => fees ??= GetFees();
		DocEntryFeeCollection fees;

		public ZString LocalReferenceNumber => EntryHeader.LocalReferenceNumber;

		public ZString MovementReferenceNumber => EntryHeader.MovementReferenceNumber;

		public ZString ResponsiblePerson => Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, EntryInstruction.CEI_SystemLastEditUser)).SingleOrDefault()?.GS_FullName ?? "n/a";

		public ZString EntryStyle => Declaration.JE_EntryStyle;

		public DocDocAddress Sender => DocDocAddress.New(Declaration.SupplierDocumentaryAddress, Factory);

		public DocDocAddress Recipient => DocDocAddress.New(recipientAddress, Factory);

		public ZString RecipientEORIDetails => GetEoriDetailsFromAddressFormatted(recipientAddress.Address);

		public DocDocAddress Declarant => DocDocAddress.New(declarantAddress, Factory);

		public ZString DeclarantEORIDetails => GetEoriDetailsFromAddressFormatted(declarantAddress);

		public DocDocAddress Representative
		{
			get
			{
				DocDocAddress result = null;
				if (representativeIsRepresentative)
				{
					result = DocDocAddress.New(representativeAddress, Factory);
				}
				else if (representativeIsRepresentedParty)
				{
					result = DocDocAddress.New(representedPartyAddress, Factory);
				}
				return result;
			}
		}

		public ZString RepresentativeEORIDetails
		{
			get
			{
				var result = ZString.Empty;
				if (representativeIsRepresentative)
				{
					result = GetEoriDetailsFromAddressFormatted(representativeAddress);
				}
				else if (representativeIsRepresentedParty)
				{
					result = GetEoriDetailsFromAddressFormatted(representedPartyAddress);
				}
				return result;
			}
		}
		public ZString IsRepresented => representativeIsRepresentedParty.ToString();

		public ZString DepartureCountry
		{
			get
			{
				var departureCountryCode = Declaration.JE_GoodsOrigin;
				var countryName = RefCountry.LoadFromCountryCode(Factory, departureCountryCode)?.RN_Desc ?? ZString.Empty;
				return string.Join(" ", departureCountryCode, countryName);
			}
		}

		public ZString BorderMOT
		{
			get
			{
				var borderMOT = Declaration.ZG_BorderTransportMeans;
				return string.Join(" ", borderMOT, Declaration.Lookups.BorderTransportMeansList.GetDescriptionFromCode(borderMOT));
			}
		}

		public ZString BorderTransportMeansNationality => Declaration.JE_RN_NKTransportNationality;

		public ZString TransportIDInland => Declaration.ZG_Box18TransportID;

		public ZString Incoterm
		{
			get
			{
				if (RandomInvoiceHeader is JobComInvoiceHeader invoiceHeader)
				{
					var incoterm = invoiceHeader.JZ_IncoTerm;
					return string.Join(" - ", string.Join(" ", incoterm, invoiceHeader.Lookups.JZ_IncoTerm_List.GetDescriptionFromCode(incoterm)), invoiceHeader.JZ_IncoTermPlace);
				}
				return ZString.Empty;
			}
		}

		public ZString TransactionNature
		{
			get
			{
				var transactionNature = RandomInvoiceHeader?.JZ_ValuationCode ?? ZString.Empty;
				return string.Join(" ", transactionNature, RandomInvoiceHeader?.Lookups.ValuationCodeList.GetDescriptionFromCode(transactionNature));
			}
		}

		public ZString TotalInvoiceAmount => totalInvoiceAmount.ToString(2, useCommas: true);

		ZDecimal totalInvoiceAmount => EntryInstruction.Invoices?.Sum(x => x.JZ_InvoiceAmount.Round(2)) ?? ZDecimal.Zero;

		public ZString TotalInvoiceAmountCurrency => RandomInvoiceHeader?.JZ_RX_NKInvoice_Currency ?? ZString.Empty;

		public ZString TotalInvoiceAmountEURValue
		{
			get
			{
				var result = ZDecimal.Zero;
				if (exchangeRate > ZDecimal.Zero)
				{
					result = totalInvoiceAmount / exchangeRate;
				}
				return result.ToStringRounded(2);
			}
		}

		public ZString ExchangeRate => exchangeRate.ToStringRounded(6);

		ZDecimal exchangeRate => RandomInvoiceHeader?.JZ_InvoiceCurrExRate ?? ZDecimal.Zero;

		public ZString PaymentNumber => RandomInvoiceHeader?.JZ_PaymentNo ?? ZString.Empty;

		public ZString TotalChargesAmount => ((ZDecimal)AllFees.Sum(y => y.CF_ChargeAmount.Round(2))).ToString(2, useCommas: true);

		public ZString IsFinalTaxReport => !isFinalTaxReport.IsEmpty ? isFinalTaxReport
			: (isFinalTaxReport = ((ZBool)EntryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.ConfirmedFees.Count > 0)).ToString());
		ZString isFinalTaxReport;

		public ZDateTime ValuationDateOverride => RandomInvoiceHeader?.JZ_ValuationDateOverride ?? ZDateTime.Empty;

		public ZString CustomsOfficeIdentification => CustomsOffice != null ? new ZString($"{CustomsOffice.ZZD_Code} - {CustomsOffice.ZZD_Description}") : ZString.Empty;

		public ZString CustomsOfficeStreet => CustomsOffice != null ? CustomsOffice.GetAttribute(CustomsOfficeAttributes.Street) : ZString.Empty;

		public ZString CustomsOfficePostCodeAndCity => CustomsOffice != null ? new ZString($"{CustomsOffice.GetAttribute(CustomsOfficeAttributes.PostCode)} {CustomsOffice.GetAttribute(CustomsOfficeAttributes.City)}".Trim()) : ZString.Empty;

		ZZRefCusCodeListCombined CustomsOffice => CachedValueHelper.GetValue(ref customsOffice, () => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, Declaration.JE_CustomsOffice, "DE", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today));
		CachedValue<ZZRefCusCodeListCombined> customsOffice;

		public ZString ContainerNumbers => string.Join("; ", Declaration.IsContainerised
			? EntryInstruction.InvoiceLines.SelectMany(il => il.ContainersPivot.Select(c => c.ContainerNumber.ToString())).Distinct().ToArray()
			: Array.Empty<string>());

		public DocDefermentAccountCollection DefermentAccounts => defermentAccounts ??= new (EntryHeader.GetDutyDefermentAccounts(), Factory);
		DocDefermentAccountCollection defermentAccounts;

		CusEntryHeader EntryHeader => entryHeader ??= (CusEntryHeader)WrappedObject;
		CusEntryHeader entryHeader;

		JobDeclaration Declaration => declaration ??= EntryHeader.Declaration;
		JobDeclaration declaration;

		CusEntryInstruction EntryInstruction => entryInstruction ??= EntryHeader.EntryInstruction;
		CusEntryInstruction entryInstruction;

		JobComInvoiceHeader RandomInvoiceHeader => randomInvoiceHeader ??= (JobComInvoiceHeader)EntryInstruction.InvoiceLines.FirstOrDefault()?.InvoiceHeader;
		JobComInvoiceHeader randomInvoiceHeader;

		IEnumerable<CusEntryLineFee> AllFees
		{
			get
			{
				if (allFees == null)
				{
					if (IsFinalTaxReport == YesNoList.Codes.Yes)
					{
						allFees = EntryHeader.AllEntryLines.Cast<CusEntryLine>().SelectMany(x => x.ConfirmedFees.Cast<CusEntryLineFee>()).ToArray();
					}
					else
					{
						allFees = EntryHeader.AllEntryLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).ToArray();
					}
				}
				return allFees;
			}
		}
		CusEntryLineFee[] allFees;

		ZBool representativeIsRepresentative => Declaration.JE_DeclarantType == Enterprise.Customs.EU.Business.RepresentationTypeList.Codes._2Direct;

		ZBool representativeIsRepresentedParty => Declaration.JE_DeclarantType == Enterprise.Customs.EU.Business.RepresentationTypeList.Codes._3Indirect;

		JobDocAddress recipientAddress => Declaration.ImporterDocumentaryAddress;

		OrgAddress declarantAddress => Declaration.Declarant;

		OrgAddress representativeAddress => Declaration.Representative;

		OrgAddress representedPartyAddress => Declaration.BuyingAgentAddress;

		string GetEoriDetailsFromAddressFormatted(OrgAddress address)
		{
			var result = string.Join(" ", address?.Header.GetEUEoriDetails(), address.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix)).Trim();
			if (!string.IsNullOrEmpty(result))
			{
				result = $"({result})";
			}
			return result;
		}

		DocEntryFeeCollection GetFees()
		{
			var cumulatedFees = AllFees
				.GroupBy(y => new { ChargeTypeDescription = y.Factory.GetGermanRateCodeDescription(y.CF_ChargeType, y.CountryCode) ?? y.ChargeTypeDescription, y.CF_ChargeType },
					y => y.CF_ChargeAmount.Round(2))
				.Select(feeGroup => new EntryFee(feeGroup.Key.ChargeTypeDescription, feeGroup.Key.CF_ChargeType, feeGroup.Sum(x => x)))
				.ToArray();
			return new DocEntryFeeCollection(cumulatedFees, Factory);
		}

		internal class CustomsOfficeAttributes
		{
			internal const string PostCode = "PostCode";
			internal const string City = "CITY";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description String")]
			internal const string Street = "Street";
		}
	}
}
