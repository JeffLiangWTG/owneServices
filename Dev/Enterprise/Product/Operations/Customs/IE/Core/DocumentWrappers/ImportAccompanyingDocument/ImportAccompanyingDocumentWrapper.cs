using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using CusEntryHeader = Enterprise.Customs.IE.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.IE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.IE.DocumentWrappers
{
	public class ImportAccompanyingDocumentWrapper : DocBaseWrapper
	{
		public static ImportAccompanyingDocumentWrapper New(CusEntryHeader entryHeader, BusinessObjectFactory factory) => new ImportAccompanyingDocumentWrapper(entryHeader, factory);

		public ImportAccompanyingDocumentWrapper(CusEntryHeader entryHeader, BusinessObjectFactory factory) : base(entryHeader, factory)
		{
			Argument.NotNull(entryHeader, nameof(entryHeader));
			this.entryHeader = entryHeader;
			declaration = entryHeader.Declaration;
			entryInstruction = entryHeader.EntryInstruction;
		}
		public CusEntryHeader EntryHeader => entryHeader;
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly CusEntryInstruction entryInstruction;

		#region Properties

		public ZString FormattedEntryNumber => entryHeader.EntryNumber;

		public Image ClientLogo => SystemDataRegistry.Instance.CompanyLogo.GetFallBackValueAtAllLevels(entryHeader.RegistryCompanyPK, entryHeader.RegistryBranchPK, Guid.Empty);

		public ZString EntryStyle
		{
			get
			{
				var stringBuilder = new ZStringBuilder();

				if (entryInstruction != null)
				{
					stringBuilder.AppendIfNotEmpty(entryInstruction.CEI_Style);
				}

				return stringBuilder.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		public ZString DeclarationUCR =>
			!declaration?.JE_UCR.IsEmpty ?? false
			? declaration.JE_UCR
			: entryHeader.RandomHeader?.JZ_UCR ?? ZString.Empty;
		public ZString Box44OfficeOfPresentation => declaration.JE_CustomsOffice;
		public ZString ExporterFullName => declaration?.SupplierDocumentaryAddress?.CompanyName ?? ZString.Empty;
		public ZString ExporterStreetAndNumber => declaration?.SupplierDocumentaryAddress?.Address1 ?? ZString.Empty;
		public ZString ExporterEORI => entryHeader.SupplierEoriOfMainOffice;

		public ZString ImporterFullName => declaration?.ImporterDocumentaryAddress?.CompanyName ?? ZString.Empty;
		public ZString ImporterStreetAndNumber => declaration?.ImporterDocumentaryAddress?.Address1 ?? ZString.Empty;
		public ZString ImporterEORI => entryHeader.ImporterEoriOfMainOffice;

		public ZString DeclarantFullName => declaration?.Declarant?.Header?.OH_FullName ?? ZString.Empty;
		public ZString DeclarantStreetAndNumber => declaration?.Declarant?.Address1 ?? ZString.Empty;
		public ZString DeclarantEORI => entryHeader.RepresentativeOrDeclarantEoriOfMainOffice;

		public ZString SellerFullName => declaration?.Seller?.OH_FullName ?? ZString.Empty;
		public ZString SellerStreetAndNumber => declaration?.SellerAddress?.Address1 ?? ZString.Empty;
		public ZString SellerEORI => entryHeader.SellerEoriOfMainOffice;

		public ZString BuyerFullName => declaration?.Buyer?.OH_FullName ?? ZString.Empty;
		public ZString BuyerStreetAndNumber => declaration?.Buyer?.MainAddress?.Address1 ?? ZString.Empty;
		public ZString BuyerEORI => declaration?.Buyer?.GetEORI() ?? ZString.Empty;

		public ZString RepresentativeFullName => declaration?.Representative?.CompanyName ?? ZString.Empty;
		public ZString RepresentativeStreetAndNumber => declaration?.Representative?.Address1 ?? ZString.Empty;
		public ZString RepresentativeEORI => entryHeader.RepresentativeEoriOfMainOffice;
		public ZString Box14RepresenativeStatus
		{
			get
			{
				switch (entryHeader.Declaration.JE_DeclarantType.ToUpper())
				{
					case RepresentationTypeList.Codes._1Self:
						return Enterprise.Customs.IE.Business.Constants.RepresentationTypeList.Self;
					case RepresentationTypeList.Codes._2Direct:
						return Enterprise.Customs.IE.Business.Constants.RepresentationTypeList.Direct;
					case RepresentationTypeList.Codes._3Indirect:
						return Enterprise.Customs.IE.Business.Constants.RepresentationTypeList.Indirect;
					default:
						return ZString.Empty;
				}
			}
		}

		public ZDecimal Box22InvoiceTotal => entryHeader.TotalPrice.Amount;
		public ZString Box22InvoiceCurrency => entryHeader.TotalPrice?.Currency?.Code ?? ZString.Empty;
		public ZString Box25BorderTransportMode => declaration?.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportMode) ?? ZString.Empty;
		public ZString Box44SupervisingOffice => declaration.SupervisingOfficeDocAddress.Organisation?.LocalCustomsClientCode ?? ZString.Empty;
		public ZString NatureOfTransaction => entryHeader.RandomHeader?.JZ_ValuationCode ?? ZString.Empty;
		public ZString Box35GrossMass
		{
			get
			{
				var result = entryHeader.GrossWeight.InKilogramsSafe.Round(3);
				return result.IsEmpty ? (ZString)eadBlankBoxDashes : (ZString)result.ToString();
			}
		}
		public ZString Box22InvoiceTotalCurrency
		{
			get { return entryHeader.TotalPrice.Currency == null ? "" : entryHeader.TotalPrice.Currency.Code; }
		}

		public ZString Box22ExchangeRate
		{
			get
			{
				var exchangeRate = ZDecimal.Zero;
				if (declaration?.IsUCC5AndIsImport ?? false)
				{
					exchangeRate = entryHeader?.RandomHeader?.JZ_InvoiceCurrExRate.Round(5) ?? ZDecimal.Zero;
				}
				return exchangeRate == ZDecimal.Zero ? ZString.Empty : (ZString)exchangeRate.ToString();
			}
		}

		public ZString EntryStatus => EntryHeader.EntryHeaderStatusDescription;
		public ZString DateOfAcceptance => EntryHeader.MovementReferenceNumberIssueDate.ToBestReadableDateString();

		public ZString Box31Containers => Factory.GetValue(ref box31ContainersCache, delegate
			{
				var result = new ZStringBuilder();
				foreach (var invoiceLine in declaration.FilteredInvoiceLines
					.OrderBy(x => x.JI_LineNo))
				{
					foreach (var container in invoiceLine.ContainersForInvoiceLinesForBindingOnly
						.OrderBy(x => x.ContainerNumber))
					{
						result.AppendIfNotEmpty(container.ContainerNumber);
					}
				}

				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			});
		CachedProperty<ZString> box31ContainersCache;

		public ZString Box18TransportReference => declaration.JE_TransportMeans + " " + declaration.JE_TransportIDInland;

		public ZString Box40SummaryDeclarationAndPreviousDocsCombined => Factory.GetValue(ref box40SummaryDeclarationAndPreviousDocsCombinedCached, delegate
			{
				var result = new ZStringBuilder();

				if (entryInstruction != null)
				{
					foreach (var previousDocument in entryInstruction.PreviousDocuments
						.Select(x => (x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_LineNo))
						.OrderBy(x => x.CSI_Code)
						.ThenBy(x => x.CSI_ReferenceNumber)
						.ThenBy(x => x.CSI_LineNo))
					{
						result.Append(previousDocument.CSI_Code + fieldSeparator + previousDocument.CSI_ReferenceNumber + fieldSeparator + previousDocument.CSI_LineNo);
					}
				}

				var invoice = entryHeader.RandomHeader;
				if ((object)invoice != null)
				{
					foreach (var previousDocument in invoice.PreviousDocuments
						.Select(x => (x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_LineNo))
						.OrderBy(x => x.CSI_Code)
						.ThenBy(x => x.CSI_ReferenceNumber)
						.ThenBy(x => x.CSI_LineNo))
					{
						result.Append(previousDocument.CSI_Code + fieldSeparator + previousDocument.CSI_ReferenceNumber + fieldSeparator + previousDocument.CSI_LineNo);
					}
				}

				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			});
		CachedProperty<ZString> box40SummaryDeclarationAndPreviousDocsCombinedCached;

		(ZString TaxType, ZDecimal TaxBase, ZDecimal TaxRate, ZDecimal PayableAmount, ZString PaymentMethod)[] SummarisedTaxes => Factory.GetValue(ref summarisedTaxesCached, () =>
			(entryHeader.HasAnyConfirmedFeesOnAnyMergedLine
				? entryHeader.MergedLines.SelectMany(entryLine => entryLine.ConfirmedFees.Cast<CusEntryLineFee>())
				: entryHeader.MergedLines.SelectMany(entryLine => entryLine.Fees.Cast<CusEntryLineFee>())
			)
			.GroupBy(fee => fee.CF_ChargeType)
			.Select(group => (
				TaxType: group.Key,
				TaxBase: (ZDecimal)group.Sum(fee => fee.CF_BaseValue),
				TaxRate: group.All(fee => !fee.CF_Rate.IsEmpty) && group.AllSame(fee => fee.CF_Rate) ? group.First().CF_Rate : ZDecimal.Zero,
				PayableAmount: (ZDecimal)group.Sum(fee => fee.CF_ChargeAmount),
				PaymentMethod: ZString.Join(comma, group.Select(fee => fee.CF_MethodOfPayment).Distinct().Where(x => !x.IsEmpty).OrderBy(p => p).ToArray())
			))
			.OrderBy(x => x.TaxType)
			.ToArray()
		);
		CachedProperty<(ZString, ZDecimal,ZDecimal, ZDecimal, ZString)[]> summarisedTaxesCached;

		public ZString SummarisedTaxType => ZString.Join(lineSeparator, SummarisedTaxes.Select(x => x.TaxType).ToArray());

		public ZString SummarisedTaxBase => string.Join(lineSeparator, SummarisedTaxes.Select(x => x.TaxBase.ToString(Decimal2DigitFormat)).ToArray());

		public ZString SummarisedTaxRate => string.Join(lineSeparator, SummarisedTaxes.Select(x => x.TaxRate.IsEmpty ? NotAvailable : x.TaxRate.ToString(Decimal2DigitFormat)).ToArray());

		public ZString SummarisedPayableAmount => string.Join(lineSeparator, SummarisedTaxes.Select(x => x.PayableAmount.ToString(Decimal2DigitFormat)).ToArray());

		public ZString SummarisedPaymentMethod => string.Join(lineSeparator, SummarisedTaxes.Select(x => x.PaymentMethod).ToArray());

		public ZString TotalTaxAmount => SummarisedTaxes.Sum(tax => tax.PayableAmount).ToString(Decimal2DigitFormat);

		public ZString Box45SupportingDocuments => Factory.GetValue(ref box45SupportingDocumentsCached, delegate
			{
				var result = new ZStringBuilder();

				if (entryInstruction != null)
				{
					foreach (var supportingDocument in entryInstruction.SupportingDocuments
						.Select(x => (x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_LineNo))
						.OrderBy(x => x.CSI_Code)
						.ThenBy(x => x.CSI_ReferenceNumber)
						.ThenBy(x => x.CSI_LineNo))
					{
						result.Append(supportingDocument.CSI_Code + fieldSeparator + supportingDocument.CSI_ReferenceNumber);
					}
				}

				var invoice = entryHeader.RandomHeader;
				if ((object)invoice != null)
				{
					foreach (var supportingDocument in invoice.SupportingDocuments
						.Select(x => (x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_LineNo))
						.OrderBy(x => x.CSI_Code)
						.ThenBy(x => x.CSI_ReferenceNumber)
						.ThenBy(x => x.CSI_LineNo))
					{
						result.Append(supportingDocument.CSI_Code + fieldSeparator + supportingDocument.CSI_ReferenceNumber);
					}
				}

				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			});
		CachedProperty<ZString> box45SupportingDocumentsCached;

		public ZString Box44AuthorisationHolders => Factory.GetValue(ref box44AuthorisationHoldersCached, delegate
			{
				var result = new ZStringBuilder();
				var authorisations = entryInstruction?.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner.Cast<CusAuthorizationUsage>() ?? new List<CusAuthorizationUsage>();

				foreach (var authorisation in authorisations
					.OrderBy(x => x.AGC_Code)
					.ThenBy(x => x.AGC_Number))
				{
					result.Append(authorisation.AGC_Code + fieldSeparator + authorisation.AGC_Number);
				}

				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			});
		CachedProperty<ZString> box44AuthorisationHoldersCached;

		public ZString Box52Guarantee => Factory.GetValue(ref box52GuaranteeCached, delegate
			{
				var result = new ZStringBuilder();
				var guarantees = declaration.Guarantees.Cast<EU.Business.Declaration.GuaranteeForDeclaration>().Where(x => x.PW_BondType == "G" && (x.EntryInstruction == null || x.EntryInstruction == entryInstruction));
				foreach (var guarantee in guarantees
					.OrderBy(x => x.PW_Password)
					.ThenBy(x => x.PW_HolderIdentification))
				{
					result.Append(guarantee.PW_Password + fieldSeparator + guarantee.PW_HolderIdentification);
				}

				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			});
		CachedProperty<ZString> box52GuaranteeCached;

		public ZString Box49WarehouseID => entryInstruction?.WarehouseIDFor27 ?? ZString.Empty;

		public ZString TransportModeInlandConverted => declaration?.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland) ?? ZString.Empty;

		public ZString Box30LocationOfGoods
		{
			get
			{
				if (declaration?.IsUCC5AndIsImport ?? false)
				{
					if (entryInstruction?.GoodsLocation is Business.CusGoodsLocation goodsLocation)
					{
						return $"{goodsLocation.CGL_Qualifier} {goodsLocation.CGL_Type} {goodsLocation.AdditionalIdentifier}";
					}
					else
					{
						return ZString.Empty;
					}
				}
				else
				{
					return Unlocode;
				}
			}
		}

		ZString Unlocode
		{
			get
			{
				var goodsLocation = entryInstruction?.GoodsLocation;
				var returnValue = goodsLocation?.CGL_Qualifier;

				if (returnValue.ToString().IsNullOrEmpty())
				{
					returnValue = goodsLocation?.CGL_AdditionalIdentifier;
				}

				if (returnValue.ToString().IsNullOrEmpty())
				{
					returnValue = goodsLocation?.CGL_Type;
				}

				if (returnValue.ToString().IsNullOrEmpty())
				{
					returnValue = GlbCompany.CurrentCompany.Country.Code;
				}

				return returnValue ?? ZString.Empty;
			}
		}

		public ZString PresentationOffice => declaration.CustomsOffices.GetOffice(EuOfficeCodesTypes.Codes.OfficeOfPresentation)?.CY_Data ?? ZString.Empty;

		public ZString AdditionalFiscalReferences => Factory.GetValue(ref additionalFiscalReferencesCached, delegate
			{
				var result = new ZStringBuilder();

				if (entryInstruction != null)
				{
					foreach (var additionalInfos in entryInstruction.AdditionalInfos)
					{
						foreach (var info in additionalInfos.Cast<AdditionalInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation)
							.OrderBy(x => x.CSI_Code)
							.OrderBy(x => x.CSI_Description))
						{
							result.AppendLine(info.CSI_Code + " " + info.CSI_Description);
						}
					}
				}

				foreach (JobComInvoiceLine invoiceLine in entryHeader.InvoiceLines)
				{
					var invoice = invoiceLine.InvoiceHeader;
					if ((object)invoice != null)
					{
						foreach (var additionalInfos in invoice.AdditionalInfos)
						{
							foreach (var info in additionalInfos.Cast<AdditionalInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation)
								.OrderBy(x => x.CSI_Code)
								.OrderBy(x => x.CSI_Description))
							{
								result.AppendLine(info.CSI_Code + " " + info.CSI_Description);
							}
						}
					}
				}

				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			});
		CachedProperty<ZString> additionalFiscalReferencesCached;

		public ZString AdditionalSupplyChainActors => Factory.GetValue(ref additionalSupplyChainActorsCached, delegate
			{
				var result = new ZStringBuilder();

				if (entryInstruction != null)
				{
					foreach (var supplyChainActors in entryInstruction.CusSupplyChainActorReferences
						.OrderBy(x => x.CFR_Code)
						.ThenBy(x => x.CFR_Reference))
					{
						result.Append(supplyChainActors.CFR_Code + " " + supplyChainActors.CFR_Reference);
					}
				}

				return result.ToStringWithDelimiterBetweenAppends(lineSeparator);
			});
		CachedProperty<ZString> additionalSupplyChainActorsCached;

		public ZString DeliveryTerms
		{
			get
			{
				var place = entryHeader.RandomHeader.JZ_IncoTermPlace.IsEmpty ? entryHeader.RandomHeader.ZG_AgreedPlaceCode : entryHeader.RandomHeader.JZ_IncoTermPlace;

				return entryHeader.RandomHeader.JZ_IncoTerm + " " + place;
			}
		}

		#endregion

		#region Routing

		public ZString Routing => Factory.GetCached(ref routing, () => IADRoutingHelper.GetRouting(entryHeader));
		CachedProperty<ZString> routing;

		#endregion

		public ImportAccompanyingDocumentEntryLineWrapperCollection Items
		{
			get { return items ?? (items = new ImportAccompanyingDocumentEntryLineWrapperCollection(entryHeader)); }
		}
		ImportAccompanyingDocumentEntryLineWrapperCollection items;

		const string lineSeparator = "\r\n";
		const string fieldSeparator = " | ";
		const string eadBlankBoxDashes = "---";
		const string comma = ",";
		const string Decimal2DigitFormat = "F2";
		const string NotAvailable = "N/A";
	}
}
