using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;
using Argument = CargoWise.Common.Argument;
using CusEntryLine = Enterprise.Customs.IT.Business.Declaration.CusEntryLine;
using CusEntryLineFee = Enterprise.Customs.IT.Business.Declaration.CusEntryLineFee;

using CustomsBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

sealed class CusEntryLineCustomsMessageWrapper : ICusEntryLineCustomsMessageWrapper
{
	public CusEntryLineCustomsMessageWrapper(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		declaration = Argument.NotNull(entryLine.Declaration, nameof(entryLine.Declaration));
		InitializeLazy();
	}

	#region ICusEntryLineCustomsMessageWrapper

	int ICusEntryLineCustomsMessageWrapper.ItemNumber => entryLine.CL_LineNumber;

	ICustomsProcedure ICusEntryLineCustomsMessageWrapper.Procedure => lazyProcedure.Value;
	Lazy<ICustomsProcedure> lazyProcedure;

	IReadOnlyCollection<IPreviousDocument> ICusEntryLineCustomsMessageWrapper.PreviousDocuments => lazyPreviousDocuments.Value;
	Lazy<IReadOnlyCollection<IPreviousDocument>> lazyPreviousDocuments;

	IReadOnlyCollection<IAdditionalInformation> ICusEntryLineCustomsMessageWrapper.AdditionalInformation => lazyAdditionalInformation.Value;
	Lazy<IReadOnlyCollection<IAdditionalInformation>> lazyAdditionalInformation;

	IReadOnlyCollection<CustomsBuilder.ISupportingDocument> ICusEntryLineCustomsMessageWrapper.SupportingDocuments => lazySupportingDocuments.Value;
	Lazy<IReadOnlyCollection<CustomsBuilder.ISupportingDocument>> lazySupportingDocuments;

	IEoriTrader ICusEntryLineCustomsMessageWrapper.Exporter => lazyExporter.Value;
	Lazy<IEoriTrader> lazyExporter;

	IEoriTrader ICusEntryLineCustomsMessageWrapper.Seller => lazySeller.Value;
	Lazy<IEoriTrader> lazySeller;

	IEoriTrader ICusEntryLineCustomsMessageWrapper.Buyer => lazyBuyer.Value;
	Lazy<IEoriTrader> lazyBuyer;

	IReadOnlyCollection<IAdditionalSupplyChainActor> ICusEntryLineCustomsMessageWrapper.AdditionalSupplyChainActors => lazyAdditionalSupplyChainActors.Value;
	Lazy<IReadOnlyCollection<IAdditionalSupplyChainActor>> lazyAdditionalSupplyChainActors;

	IReadOnlyCollection<IFiscalReference> ICusEntryLineCustomsMessageWrapper.FiscalReferences => lazyFiscalReferences.Value;
	Lazy<IReadOnlyCollection<IFiscalReference>> lazyFiscalReferences;

	IReadOnlyCollection<CustomsBuilder.IFee> ICusEntryLineCustomsMessageWrapper.Fees => lazyFees.Value;
	Lazy<IReadOnlyCollection<CustomsBuilder.IFee>> lazyFees;

	decimal ICusEntryLineCustomsMessageWrapper.TotalFeeAmount => lazyTotalFeeAmount.Value;
	Lazy<decimal> lazyTotalFeeAmount;

	IReadOnlyCollection<IAdditionOrDeduction> ICusEntryLineCustomsMessageWrapper.AdditionOrDeductions => lazyAdditionOrDeductions.Value;
	Lazy<IReadOnlyCollection<IAdditionOrDeduction>> lazyAdditionOrDeductions;

	string ICusEntryLineCustomsMessageWrapper.RelatedIndicator => lazyRelatedIndicator.Value;
	Lazy<string> lazyRelatedIndicator;

	decimal ICusEntryLineCustomsMessageWrapper.ItemPrice => lazyItemPrice.Value;
	Lazy<decimal> lazyItemPrice;

	int ICusEntryLineCustomsMessageWrapper.ValuationMethod => lazyValuationMethod.Value;
	Lazy<int> lazyValuationMethod;

	int? ICusEntryLineCustomsMessageWrapper.Preferences => lazyPreferences.Value;
	Lazy<int?> lazyPreferences;

	string ICusEntryLineCustomsMessageWrapper.DestinationStateCode => lazyDestinationStateCode.Value;
	Lazy<string> lazyDestinationStateCode;

	string ICusEntryLineCustomsMessageWrapper.OriginCountryCode => lazyOriginCountryCode.Value;
	Lazy<string> lazyOriginCountryCode;

	string ICusEntryLineCustomsMessageWrapper.PreferredOriginCountryCode => lazyPreferredOriginCountryCode.Value;
	Lazy<string> lazyPreferredOriginCountryCode;

	decimal ICusEntryLineCustomsMessageWrapper.NetMass => entryLine.CustomsQuantity;

	decimal? ICusEntryLineCustomsMessageWrapper.SupplementaryUnit => entryLine.SupplementaryQuantity.GetValueOrNullIfZero();

	decimal ICusEntryLineCustomsMessageWrapper.GrossMass => lazyGrossMass.Value;
	Lazy<decimal> lazyGrossMass;

	string ICusEntryLineCustomsMessageWrapper.GoodsDescription => entryLine.GoodsDescription;

	IReadOnlyCollection<IPackage> ICusEntryLineCustomsMessageWrapper.Packages => lazyPackages.Value;
	Lazy<IReadOnlyCollection<IPackage>> lazyPackages;

	string ICusEntryLineCustomsMessageWrapper.CusCode => lazyCusCode.Value;
	Lazy<string> lazyCusCode;

	string ICusEntryLineCustomsMessageWrapper.NcCode => lazyNcCode.Value;
	Lazy<string> lazyNcCode;

	string ICusEntryLineCustomsMessageWrapper.TaricCode => lazyTaricCode.Value;
	Lazy<string> lazyTaricCode;

	IReadOnlyCollection<string> ICusEntryLineCustomsMessageWrapper.AdditionalCodes => lazyAdditionalCodes.Value;
	Lazy<IReadOnlyCollection<string>> lazyAdditionalCodes;

	IReadOnlyCollection<string> ICusEntryLineCustomsMessageWrapper.NationalAdditionalCodes => lazyNationalAdditionalCodes.Value;
	Lazy<IReadOnlyCollection<string>> lazyNationalAdditionalCodes;

	IReadOnlyCollection<string> ICusEntryLineCustomsMessageWrapper.Containers => lazyContainers.Value;
	Lazy<IReadOnlyCollection<string>> lazyContainers;

	string ICusEntryLineCustomsMessageWrapper.ConcessionOrder => RandomLine.JI_ConcessionOrder;

	int ICusEntryLineCustomsMessageWrapper.TransactionNature => lazyTransactionNature.Value;
	Lazy<int> lazyTransactionNature;

	decimal ICusEntryLineCustomsMessageWrapper.StatisticalValue => entryLine.CL_StatisticalValue;

	string ICusEntryLineCustomsMessageWrapper.DestinationCountryCode => declaration.JE_GoodsDestination;

	string ICusEntryLineCustomsMessageWrapper.DispatchCountryCode => declaration.JE_GoodsOrigin;
	DateTime? ICusEntryLineCustomsMessageWrapper.AcceptanceDate { get; }

	IReadOnlyCollection<IBaseAmount> ICusEntryLineCustomsMessageWrapper.BaseAmounts => lazyBaseAmounts.Value;
	Lazy<IReadOnlyCollection<IBaseAmount>> lazyBaseAmounts;

	IReadOnlyCollection<IPreviousDocument> ICusEntryLineCustomsMessageWrapper.ExportPreviousDocuments => lazyExportPreviousDocuments.Value;
	Lazy<IReadOnlyCollection<IPreviousDocument>> lazyExportPreviousDocuments;

	IReadOnlyCollection<IAdditionalInformation> ICusEntryLineCustomsMessageWrapper.ExportAdditionalInformation => lazyExportAdditionalInformation.Value;
	Lazy<IReadOnlyCollection<IAdditionalInformation>> lazyExportAdditionalInformation;

	IReadOnlyCollection<IAdditionalReference> ICusEntryLineCustomsMessageWrapper.ExportAdditionalReferences => lazyExportAdditionalReferences.Value;
	Lazy<IReadOnlyCollection<IAdditionalReference>> lazyExportAdditionalReferences;

	IReadOnlyCollection<IAuthorization> ICusEntryLineCustomsMessageWrapper.ExportAuthorizations => lazyExportAuthorizations.Value;
	Lazy<IReadOnlyCollection<IAuthorization>> lazyExportAuthorizations;

	IReadOnlyCollection<ITransportDocument> ICusEntryLineCustomsMessageWrapper.ExportTransportDocuments => lazyExportTransportDocuments.Value;
	Lazy<IReadOnlyCollection<ITransportDocument>> lazyExportTransportDocuments;

	IEoriTrader ICusEntryLineCustomsMessageWrapper.ExportConsignor => lazyExportConsignor.Value;
	Lazy<IEoriTrader> lazyExportConsignor;

	IEoriTrader ICusEntryLineCustomsMessageWrapper.ExportConsignee => lazyExportConsignee.Value;
	Lazy<IEoriTrader> lazyExportConsignee;

	string ICusEntryLineCustomsMessageWrapper.ExportTransportChargesMethodOfPayment => lazyExportTransportChargesMethodOfPayment.Value;
	Lazy<string> lazyExportTransportChargesMethodOfPayment;

	string ICusEntryLineCustomsMessageWrapper.CountryOfDestination => lazyCountryOfDestination.Value;
	Lazy<string> lazyCountryOfDestination;

	string ICusEntryLineCustomsMessageWrapper.CountryOfExport => lazyCountryOfExport.Value;
	Lazy<string> lazyCountryOfExport;

	string ICusEntryLineCustomsMessageWrapper.ExportCountryOfOrigin => lazyExportCountryOfOrigin.Value;
	Lazy<string> lazyExportCountryOfOrigin;

	string ICusEntryLineCustomsMessageWrapper.ExportRegionOfDispatch => lazyExportRegionOfDispatch.Value;
	Lazy<string> lazyExportRegionOfDispatch;

	string ICusEntryLineCustomsMessageWrapper.ExportHsTariffCode => lazyExportHsTariffCode.Value;
	Lazy<string> lazyExportHsTariffCode;

	string ICusEntryLineCustomsMessageWrapper.ExportNcTariffCode => lazyExportNcTariffCode.Value;
	Lazy<string> lazyExportNcTariffCode;

	IReadOnlyCollection<string> ICusEntryLineCustomsMessageWrapper.DangerousGoodsCodes => lazyDangerousGoodsCodes.Value;
	Lazy<IReadOnlyCollection<string>> lazyDangerousGoodsCodes;

	int? ICusEntryLineCustomsMessageWrapper.ExportNatureOfTransaction => lazyExportNatureOfTransaction.Value;
	Lazy<int?> lazyExportNatureOfTransaction;

	#endregion

	#region Implementation

	void InitializeLazy()
	{
		lazyProcedure = new Lazy<ICustomsProcedure>(GetCustomsProcedure);
		lazyPreviousDocuments = new Lazy<IReadOnlyCollection<IPreviousDocument>>(GetPreviousDocuments);
		lazyAdditionalInformation = new Lazy<IReadOnlyCollection<IAdditionalInformation>>(GetAdditionalInformation);
		lazySupportingDocuments = new Lazy<IReadOnlyCollection<CustomsBuilder.ISupportingDocument>>(GetSupportingDocuments);
		lazyExporter = new Lazy<IEoriTrader>(GetExporter);
		lazySeller = new Lazy<IEoriTrader>(GetSeller);
		lazyBuyer = new Lazy<IEoriTrader>(GetBuyer);
		lazyAdditionalSupplyChainActors = new Lazy<IReadOnlyCollection<IAdditionalSupplyChainActor>>(GetAdditionalSupplyChainActors);
		lazyFiscalReferences = new Lazy<IReadOnlyCollection<IFiscalReference>>(GetFiscalReferences);
		lazyFees = new Lazy<IReadOnlyCollection<CustomsBuilder.IFee>>(GetFees);
		lazyTotalFeeAmount = new Lazy<decimal>(GetTotalFeeAmount);
		lazyAdditionOrDeductions = new Lazy<IReadOnlyCollection<IAdditionOrDeduction>>(GetAdditionsDeductions);
		lazyRelatedIndicator = new Lazy<string>(GetRelatedIndicator);
		lazyValuationMethod = new Lazy<int>(GetValuationMethod);
		lazyPreferences = new Lazy<int?>(GetPreferences);
		lazyDestinationStateCode = new Lazy<string>(GetDestinationStateCode);
		lazyOriginCountryCode = new Lazy<string>(GetOriginCountryCode);
		lazyPreferredOriginCountryCode = new Lazy<string>(GetPreferredOriginCountryCode);
		lazyPackages = new Lazy<IReadOnlyCollection<IPackage>>(GetPackages);
		lazyCusCode = new Lazy<string>(GetCusCode);
		lazyNcCode = new Lazy<string>(GetNcCode);
		lazyTaricCode = new Lazy<string>(GetTaricCode);
		lazyAdditionalCodes = new Lazy<IReadOnlyCollection<string>>(GetAdditionalCodes);
		lazyNationalAdditionalCodes = new Lazy<IReadOnlyCollection<string>>(GetNationalAdditionalCodes);
		lazyContainers = new Lazy<IReadOnlyCollection<string>>(GetContainers);
		lazyTransactionNature = new Lazy<int>(() => SharedWrapperDataProvider.GetNatureOfTransaction(entryLine.Header));
		lazyItemPrice = new Lazy<decimal>(() => entryLine.LinesValueInInvoiceCurrency);
		lazyGrossMass = new Lazy<decimal>(() => entryLine.GrossWeight.InKilograms);
		lazyBaseAmounts = new Lazy<IReadOnlyCollection<IBaseAmount>>(GetBaseAmounts);
		lazyExportPreviousDocuments = new Lazy<IReadOnlyCollection<IPreviousDocument>>(GetExportPreviousDocuments);
		lazyExportAdditionalInformation = new Lazy<IReadOnlyCollection<IAdditionalInformation>>(GetExportAdditionalInformation);
		lazyExportAdditionalReferences = new Lazy<IReadOnlyCollection<IAdditionalReference>>(GetExportAdditionalReferences);
		lazyExportAuthorizations = new Lazy<IReadOnlyCollection<IAuthorization>>(GetExportAuthorizations);
		lazyExportTransportDocuments = new Lazy<IReadOnlyCollection<ITransportDocument>>(GetExportTransportDocuments);
		lazyExportConsignor = new Lazy<IEoriTrader>(GetExportConsignor);
		lazyExportConsignee = new Lazy<IEoriTrader>(GetExportConsignee);
		lazyExportTransportChargesMethodOfPayment = new Lazy<string>(GetExportTransportChargesMethodOfPayment);
		lazyCountryOfDestination = new Lazy<string>(GetCountryOfDestination);
		lazyCountryOfExport = new Lazy<string>(GetCountryOfExport);
		lazyExportCountryOfOrigin = new Lazy<string>(() => RandomLine.JI_CountryOfOrigin);
		lazyExportRegionOfDispatch = new Lazy<string>(() => RandomLine.JI_StateOrRegionOfOrigin);
		lazyExportHsTariffCode = new Lazy<string>(GetExportHsTariffCode);
		lazyExportNcTariffCode = new Lazy<string>(GetExportNcTariffCode);
		lazyDangerousGoodsCodes = new Lazy<IReadOnlyCollection<string>>(GetDangerousGoodsCodes);
		lazyExportNatureOfTransaction = new Lazy<int?>(GetExportNatureOfTransaction);
	}

	IReadOnlyCollection<IAdditionOrDeduction> GetAdditionsDeductions()
	{
		var allCharges = GetAllCharges();

		var additionsAndDeductions = new ChargesToAdditionDeductionConverter()
			.GetAdditionsAndDeductions(allCharges);

		return additionsAndDeductions
			.Select(codeBucket =>
			{
				decimal amount = codeBucket.CalculateAmount();
				return new AdditionOrDeduction(codeBucket.Code, amount);
			})
			.ToArray();
	}

	IReadOnlyCollection<IInvoiceLineChargeWrapper> GetAllCharges()
	{
		var jobComInvoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().ToArray();
		var allCharges = new List<IInvoiceLineChargeWrapper>();

		var charges = jobComInvoiceLines
			.SelectMany(l => l.Charges)
			.Cast<BaseInvoiceLineCharge>()
			.Select(c => new InvoiceLineChargeWrapper(c));
		allCharges.AddRange(charges);

		var apportionedCharges = jobComInvoiceLines
			.SelectMany(c => c.ApportionedCharges)
			.Cast<BaseInvoiceLineApportionedCharge>()
			.Select(c => new InvoiceLineApportionedChargeWrapper(c));
		allCharges.AddRange(apportionedCharges);

		return allCharges;
	}

	IEoriTrader GetExporter()
	{
		var traderWrapper = GetBaseExporterWrapper();
		return traderWrapper != null
			? new TraderCustomsMessageWrapper(traderWrapper)
			: null;
	}

	IEoriTrader GetBaseExporterWrapper()
	{
		var lineExporter = RandomLine.ExporterAddress;
		if (lineExporter != null)
		{
			return new TraderWrapper(lineExporter);
		}

		var declarationExporter = declaration.SupplierDocumentaryAddress;
		if (declarationExporter != null && !declarationExporter.IsEmpty)
		{
			return new TraderWrapper(declarationExporter);
		}

		return null;
	}

	IEoriTrader GetSeller() => RandomLine.SellerDocAddress.ToTraderOrEmpty();

	IEoriTrader GetBuyer() => RandomLine.BuyerDocAddress.ToTraderOrEmpty();

	IReadOnlyCollection<IFiscalReference> GetFiscalReferences()
	{
		return InvoiceLines.SelectMany(x => x.FiscalReferences)
			.Cast<EU.Business.Declaration.CusFiscalReference>()
			.Select(x => new FiscalReferenceWrapper(x))
			.ToArray();
	}

	IReadOnlyCollection<CustomsBuilder.IFee> GetFees()
	{
		return SortedFeesIncludedInMessage
			.Select(x => new FeeWrapper(x))
			.ToArray();
	}

	decimal GetTotalFeeAmount() => SortedFeesIncludedInMessage.Sum(x => x.CF_ChargeAmount);

	string GetRelatedIndicator()
	{
		var randomLine = RandomLine;
		var randomHeader = entryLine.Header.RandomHeader;
		var relatedIndicatorBuilder = new StringBuilder();
		relatedIndicatorBuilder.Append(GetFromLineIfSetOtherwiseFromHeader(randomHeader.RelatedIndicator, randomLine.JI_RelatedIndicator));
		relatedIndicatorBuilder.Append(GetFromLineIfSetOtherwiseFromHeader(randomHeader.RelatedIndicator2, randomLine.ZG_RelatedIndicator2));
		relatedIndicatorBuilder.Append(GetFromLineIfSetOtherwiseFromHeader(randomHeader.RelatedIndicator3, randomLine.ZG_RelatedIndicator3));
		relatedIndicatorBuilder.Append(GetFromLineIfSetOtherwiseFromHeader(randomHeader.RelatedIndicator4, randomLine.ZG_RelatedIndicator4));

		int GetFromLineIfSetOtherwiseFromHeader(ZBool headerIndicator, ZString lineIndicator)
		{
			switch (lineIndicator)
			{
				case ValuationIndicatorCodeList.Codes.Yes:
					return 1;
				case ValuationIndicatorCodeList.Codes.No:
					return 0;
				default:
					return headerIndicator.ToInt();
			}
		}

		return relatedIndicatorBuilder.ToString();
	}

	int GetValuationMethod()
	{
		int.TryParse(RandomLine.JI_ValuationCode, out var valuationMethod);
		return valuationMethod;
	}

	IReadOnlyCollection<IBaseAmount> GetBaseAmounts()
	{
		return SortedFeesIncludedInMessage
			.Select(x => new BaseAmountWrapper(x))
			.ToArray();
	}

	IReadOnlyCollection<IPreviousDocument> GetExportPreviousDocuments()
	{
		return InvoiceLines
			.SelectMany(invoiceLine => invoiceLine.PreviousDocuments.Cast<PreviousDocument>())
			.Select(previousDocument => new Export.PreviousDocumentWrapper(previousDocument))
			.ToArray();
	}

	IReadOnlyCollection<IAdditionalInformation> GetExportAdditionalInformation()
	{
		return SharedWrapperDataProvider.GetAdditionalInfosFromInvoiceLinesAndHeaders<IAdditionalInformation>(InvoiceLines, AdditionalInfoSubTypeList.Codes.AdditionalInformation, x => new AdditionalInformationWrapper(x))
			.GroupBy(x => new { x.Code, x.Description })
			.Select(x => x.First())
			.ToArray();
	}

	IReadOnlyCollection<IAdditionalReference> GetExportAdditionalReferences()
	{
		return SharedWrapperDataProvider.GetAdditionalInfosFromInvoiceLinesAndHeaders<IAdditionalReference>(InvoiceLines, AdditionalInfoSubTypeList.Codes.AdditionalReference, x => new AdditionalReferenceWrapper(x))
			.GroupBy(x => new { x.ReferenceType, x.ReferenceNumber })
			.Select(x => x.First())
			.ToArray();
	}

	IReadOnlyCollection<IAuthorization> GetExportAuthorizations()
	{
		return InvoiceLines
			.SelectMany(x => x.CusAuthorizationUsages)
			.Select(x => new CustomsCodeAuthorizationWrapper(x))
			.ToArray();
	}

	IReadOnlyCollection<ITransportDocument> GetExportTransportDocuments()
	{
		if (!declaration.IsTransitionPeriodAES30)
		{
			return Array.Empty<ITransportDocument>();
		}

		return SharedWrapperDataProvider.GetAdditionalInfosFromInvoiceLinesAndHeaders<ITransportDocument>(InvoiceLines, AdditionalInfoSubTypeList.Codes.TransportDocument, x => new TransportDocumentWrapper(x))
			.GroupBy(x => new { x.DocumentType, x.ReferenceNumber })
			.Select(x => x.First())
			.ToArray();
	}

	IEoriTrader GetExportConsignor() => SharedWrapperDataProvider.GetNewConsignorHeaderOrLineValueResolver().GetValueForLine(entryLine);

	IEoriTrader GetExportConsignee() => SharedWrapperDataProvider.GetNewConsigneeHeaderOrLineValueResolver().GetValueForLine(entryLine);

	string GetExportTransportChargesMethodOfPayment()
	{
		var resolver = SharedWrapperDataProvider.GetNewTransportChargesMoPHeaderOrLineValueResolver();
		return resolver.GetValueForLine(entryLine);
	}

	string GetCountryOfDestination()
	{
		var resolver = SharedWrapperDataProvider.GetNewCountryOfDestinationHeaderOrLineValueResolver();
		return resolver.GetValueForLine(entryLine);
	}

	string GetCountryOfExport()
	{
		var resolver = SharedWrapperDataProvider.GetNewCountryOfExportHeaderOrLineValueResolver();
		return resolver.GetValueForLine(entryLine);
	}

	string GetExportHsTariffCode() => entryLine.Tariff.Left(ExportHsTariffCodeLength);

	string GetExportNcTariffCode() => entryLine.Tariff.SubstringSafe(ExportHsTariffCodeLength, ExportNcTariffCodeLength);

	const int ExportHsTariffCodeLength = 6;
	const int ExportNcTariffCodeLength = 2;
	const int UndgCodeLength = 4;

	IReadOnlyCollection<string> GetDangerousGoodsCodes()
	{
		return InvoiceLines
			.SelectMany(x => x.UNDGs)
			.Where(x => x.Substance != null)
			.Select(x => (string)x.Substance.DG_Code.ToUpper().Left(UndgCodeLength))
			.ToArray();
	}

	int? GetExportNatureOfTransaction()
	{
		var resolver = SharedWrapperDataProvider.GetNewNatureOfTransactionHeaderOrLineValueResolver();
		var fieldForLine = resolver.GetValueForLine(entryLine);
		return int.TryParse(fieldForLine, out var result) ? result : null;
	}

	JobComInvoiceLine RandomLine => entryLine.RandomLine;

	IEnumerable<JobComInvoiceLine> InvoiceLines => invoiceLines ?? (invoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().ToArray());
	IEnumerable<JobComInvoiceLine> invoiceLines;

	CusEntryLineFee[] SortedFeesIncludedInMessage => sortedFeesIncludedInMessage ?? (sortedFeesIncludedInMessage = GetSortedFeesIncludedInMessage());
	CusEntryLineFee[] sortedFeesIncludedInMessage;

	CusEntryLineFee[] GetSortedFeesIncludedInMessage()
	{
		return FeesWithoutActionExclude
			.Where(x => methodOfPaymentsToIncludeInMessage.Contains(x.CF_MethodOfPayment))
			.InCustomsCompliantOrder()
			.ToArray();
	}

	readonly ImmutableArray<string> methodOfPaymentsToIncludeInMessage = new[]
	{
		DutyMethodOfPayment.ImmediatePaymentInCashA,
		DutyMethodOfPayment.DeferredPaymentE,
		DutyMethodOfPayment.DeferredPaymentVatProcedureG,
		DutyMethodOfPayment.AgentGeneralGuaranteeAccountT,
		DutyMethodOfPayment.SecurityDepositDeferredPaymentR
	}.ToImmutableArray();

	ICustomsProcedure GetCustomsProcedure()
	{
		var randomLine = RandomLine;

		var procedure = randomLine.JI_Procedure;
		var additionalProcedureCodes = randomLine.AdditionalProcedureCodes.Select(x => x.CY_Code).ToArray();
		if (!procedure.IsEmpty)
		{
			return new CustomsProcedureWrapper(procedure, additionalProcedureCodes, addDefaultAdditionalProcedureIfNone: randomLine.IsImport);
		}
		return null;
	}

	IReadOnlyCollection<IPreviousDocument> GetPreviousDocuments()
	{
		var previousDocuments = InvoiceLines.SelectMany(invoiceLine => invoiceLine.PreviousDocuments.Cast<PreviousDocument>());

		if (declaration.IsImport)
		{
			return previousDocuments
				.GroupBy(previousDocument => PreviousDocumentHelper.GetImportWrapperGroupKey(previousDocument))
				.Select(group => new Import.PreviousDocumentWrapper(group.First(), group))
				.ToList()
				.AsReadOnly();
		}

		return previousDocuments
			.Select(previousDocument => new Export.PreviousDocumentWrapper(previousDocument))
			.ToList()
			.AsReadOnly();
	}

	IReadOnlyCollection<IAdditionalInformation> GetAdditionalInformation()
	{
		return InvoiceLines.SelectMany(x => x.AdditionalInfos.Cast<AdditionalInfo>())
			.ToAdditionalInformationWrapperCollection();
	}

	IReadOnlyCollection<CustomsBuilder.ISupportingDocument> GetSupportingDocuments()
	{
		return entryLine.SupportingDocuments
			.Cast<SupportingDocument>()
			.Select(x => new SupportingDocumentWrapper(x))
			.ToCollection();
	}

	IReadOnlyCollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActors()
	{
		return InvoiceLines.SelectMany(x => x.CusSupplyChainActorReferences)
			.Cast<CusSupplyChainActorReference>()
			.Select(x => new AdditionalSupplyChainActorWrapper(x))
			.ToCollection();
	}

	int? GetPreferences()
	{
		return int.TryParse(RandomLine.JI_PrimaryPreference, out var primaryPreference) ? primaryPreference : null;
	}

	string GetDestinationStateCode()
	{
		if (declaration.FinalDestination is ILocation finalDestination)
		{
			if (IsSanMarino(finalDestination))
			{
				return Core.Constants.CountryCodes.SanMarino;
			}

			return finalDestination.State?.RW_Code ?? ZString.Empty;
		}

		return ZString.Empty;

		bool IsSanMarino(ILocation location) => (location.Country?.Code ?? ZString.Empty) == Core.Constants.CountryCodes.SanMarino;
	}

	string GetOriginCountryCode()
	{
		var randomLine = RandomLine;

		var primaryPreference = randomLine.JI_PrimaryPreference;
		var primaryPreferenceFirstChar = primaryPreference.SubstringSafe(0, 1);

		return primaryPreference.IsEmpty || primaryPreferenceFirstChar == PrimaryPreference1
			? randomLine.JI_CountryOfOrigin
			: ZString.Empty;
	}

	string GetPreferredOriginCountryCode()
	{
		var randomLine = RandomLine;

		var primaryPreference = randomLine.JI_PrimaryPreference;
		var primaryPreferenceFirstChar = primaryPreference.SubstringSafe(0, 1);

		return !primaryPreference.IsEmpty && primaryPreferenceFirstChar != PrimaryPreference1
			? randomLine.JI_CountryOfOrigin
			: ZString.Empty;
	}

	IReadOnlyCollection<IPackage> GetPackages()
	{
		return entryLine.PackagingDetails
			.Cast<Declaration.InvoiceLinePackagePivot>()
			.Select(x => new PackageWrapper(x))
		.ToCollection();
	}

	string GetCusCode()
	{
		var cusNumber = entryLine.RandomLine?.ZG_CusNumber ?? ZString.Empty;

		if (declaration.IsImport)
		{
			const string dash = "-";
			return cusNumber.Replace(dash, ZString.Empty);
		}

		return cusNumber;
	}

	string GetNcCode()
	{
		return entryLine.Tariff.SubstringSafe(0, NcCodeLength);
	}

	string GetTaricCode()
	{
		return entryLine.Tariff.SubstringSafe(TaricCodeStartIndex, TaricCodeLength);
	}

	IReadOnlyCollection<string> GetAdditionalCodes()
	{
		return GetAllowedAdditionalCodes(entryLine, allowedAdditionalCodeFamilies);
	}

	IReadOnlyCollection<string> GetNationalAdditionalCodes()
	{
		return GetAllowedAdditionalCodes(entryLine, allowedNationalAdditionalCodeFamilies);
	}

	IReadOnlyCollection<string> GetContainers()
	{
		return entryLine.Containers.Select(x => x.ToString()).ToCollection();
	}

	#endregion

	IEnumerable<CusEntryLineFee> FeesWithoutActionExclude
		=> entryLine
			.Fees
			.Cast<CusEntryLineFee>()
			.Where(x => !x.IsActionExclude);

	IReadOnlyCollection<string> GetAllowedAdditionalCodes(CusEntryLine entryLine, ImmutableArray<string> allowedFamilies)
	{
		return entryLine.AdditionalCodes
			.Where(x => allowedFamilies.Contains(x.SubstringSafe(0, 1)))
			.Select(x => x.ToString())
			.ToCollection();
	}

	static readonly ImmutableArray<string> allowedAdditionalCodeFamilies = new string[]
	{
		AllowedAdditionalCodes._2,
		AllowedAdditionalCodes._3,
		AllowedAdditionalCodes._4,
		AllowedAdditionalCodes._6,
		AllowedAdditionalCodes._8,
		AllowedAdditionalCodes.A,
		AllowedAdditionalCodes.B,
		AllowedAdditionalCodes.C,
		AllowedAdditionalCodes.D,
		AllowedAdditionalCodes.P
	}.ToImmutableArray();

	static readonly ImmutableArray<string> allowedNationalAdditionalCodeFamilies = new string[]
	{
		AllowedNationalAdditionalCodes.Q,
		AllowedNationalAdditionalCodes.R,
		AllowedNationalAdditionalCodes.S,
		AllowedNationalAdditionalCodes.T,
		AllowedNationalAdditionalCodes.U,
		AllowedNationalAdditionalCodes.Z
	}.ToImmutableArray();

	readonly CusEntryLine entryLine;

	readonly JobDeclaration declaration;

	const string PrimaryPreference1 = "1";

	const int NcCodeLength = 8;

	const int TaricCodeStartIndex = 8;

	const int TaricCodeLength = 2;
}
