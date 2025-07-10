using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using EURepresentationTypeList = Enterprise.Customs.EU.Business.RepresentationTypeList;
using GW = Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
public class DocSADH : Enterprise.DocumentWrappers.Customs.EU.DocSADH
{
	DocSADH(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
		: base(entryHeader, factoryToWrap)
	{ }

	public static DocSADH New(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap) => new DocSADH(entryHeader, factoryToWrap);

	public new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

	public new JobDeclaration Declaration => EntryHeader.Declaration;

	protected override Enterprise.DocumentWrappers.Customs.EU.DocSADHLineCollection GetLinesCore() => new DocSADHLineCollection(EntryHeader.MergedLines, Factory);

	protected override Enterprise.DocumentWrappers.Customs.EU.DocSADHPageCollection GetPagesCore() => new DocSADHPageCollection(EntryHeader.MergedLines, Factory);

	public DocSADHPage FirstPage => Pages.Cast<DocSADHPage>().FirstOrDefault();

	protected override ZBool ShowDeclarantRepresentativeAddressCore => true;

	protected override ZString BoxBLabelCaptionCore => (NoResString)"DONNEES COMPTABLE";

	protected override ZString BoxBAccountingDetailsCore
	{
		get
		{
			var builder = new ZStringBuilder();
			var feesMapNameList = FeesSumsandTotal.Keys;

			foreach (var key in feesMapNameList)
			{
				builder.Append(key);
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}
	}

	protected override ZString TimeLimitDateCore => EntryHeader.CusEntryNumber is CusEntryNumber entrynum && !entrynum.CE_IssueDate.IsEmpty ? (ZString)entrynum.CE_IssueDate.AddDays(90).ToString(TimeLimitDateFormat) : ZString.Empty;

	protected override string IssuingDateFormat => "dd/MM/yyyy";
	protected override string TimeLimitDateFormat => "dd/MM/yyyy";

	public ZString BoxBAccountingDetailsValues
	{
		get
		{
			var builder = new ZStringBuilder();
			var feesMapNameList = FeesSumsandTotal.Keys;

			foreach (var key in feesMapNameList)
			{
				builder.Append(FeesSumsandTotal[key]);
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}
	}

	ZDecimal GetTotalFeesAmount(IEnumerable<ZString> paymentMethods)
	{
		return EntryHeader.MergedLines.OfType<CusEntryLine>().SelectMany(x => x.ConfirmedFees).OfType<CusEntryLineFee>()
			.Where(x => paymentMethods.Contains(x.CF_MethodOfPayment)).Sum(x => x.CF_ChargeAmount) +
			EntryHeader.ConfirmedCharges.Cast<CusEntryHeaderCharges>().Where(x => paymentMethods.Contains(x.C1_MethodOfPayment)).Sum(x => x.C1_ChargeAmount);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Description Zstrings")]
	Dictionary<ZString, ZString> FeesSumsandTotal
	{
		get
		{
			if (feesSumsandTotal == null)
			{
				var declaration = EntryHeader.Declaration;
				var guarantee = declaration.CustomsGuarantee;

				ZString totalFeesCautionAmountText = (NoResString)"Liquidation CE (1) " + declaration.JE_DefermentAccountNumber;
				ZString totalFeesNonCautionAmountText = "Liquidation NC (2)";
				ZString totalFeeAmountText = "Total à payer";
				ZString totalFeesAi2AmountText = "Total AI2 (3)";
				ZString totalFeesCodAmountText = "COD garantie (4) ";
				ZString totalFeesGuaranteedAmountText = "Taxes non perçues (5)";
				ZString totalFeesAtvaiAmountText = "Autoliquidation (6) ";

				var totalFeesCautionAmount = FormatLineFees(GetTotalFeesAmount(new ZString[] { "1" }).ToString(0));
				var totalFeesNonCautionAmount = FormatLineFees(GetTotalFeesAmount(new ZString[] { "2" }).ToString(0));
				var totalFeeAmount = FormatLineFees(((ZDecimal)(GetTotalFeesAmount(new ZString[] { "1" }) + GetTotalFeesAmount(new ZString[] { "2" }))).ToString(0));
				var totalFeesAi2Amount = FormatLineFees(GetTotalFeesAmount(new ZString[] { "3" }).ToString(0));
				var totalFeesCodAmount = EntryHeader.CH_ConfirmedGuaranteeAmount.IsEmpty ? FormatLineFees(GetTotalFeesAmount(new ZString[] { "4" }).ToString(0)) : (ZString)EntryHeader.CH_ConfirmedGuaranteeAmount.ToString();
				var totalFeesGuaranteedAmount = FormatLineFees(GetTotalFeesAmount(new ZString[] { "5", "" }).ToString(0));
				var totalFeesAtvaiAmount = FormatLineFees(GetTotalFeesAmount(new ZString[] { "6" }).ToString(0));

				feesSumsandTotal = new Dictionary<ZString, ZString>();
				if (guarantee != null)
				{
					var applicationSpecificReference = guarantee.GetApplicationSpecificReferenceWithoutFallbackToPermitNumber(declaration.JE_DeltaMode);
					totalFeesCodAmountText += applicationSpecificReference;
				}

				var supportingDoc = declaration.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber)?.CSI_ReferenceNumber ?? ZString.Empty;
				if (!supportingDoc.IsEmpty)
				{
					totalFeesAtvaiAmountText += supportingDoc;
				}

				feesSumsandTotal.Add(totalFeesCautionAmountText, totalFeesCautionAmount);
				feesSumsandTotal.Add(totalFeesNonCautionAmountText, totalFeesNonCautionAmount);
				feesSumsandTotal.Add(totalFeeAmountText, totalFeeAmount);
				feesSumsandTotal.Add(totalFeesAi2AmountText, totalFeesAi2Amount);
				feesSumsandTotal.Add(totalFeesCodAmountText, totalFeesCodAmount);
				feesSumsandTotal.Add(totalFeesGuaranteedAmountText, totalFeesGuaranteedAmount);
				feesSumsandTotal.Add(totalFeesAtvaiAmountText, totalFeesAtvaiAmount);
			}
			return feesSumsandTotal;
		}
	}
	Dictionary<ZString, ZString> feesSumsandTotal;

	ZString FormatLineFees(ZString sumLineFees)
	{
		for (int i = sumLineFees.Length - 3; i > 0; i -= 3)
		{
			sumLineFees = sumLineFees.Insert(i, " ");
		}
		return sumLineFees;
	}

	protected override ZString Box14DetailsFooterCore
	{
		get
		{
			var representationDesc = ZString.Empty;

			switch (Declaration.JE_DeclarantType)
			{
				case RepresentationTypeList.Codes.SEL:
					representationDesc = (NoResString)"1 Compte propre";
					break;
				case RepresentationTypeList.Codes.DIR:
					representationDesc = (NoResString)"2 Direct";
					break;
				case RepresentationTypeList.Codes.IND:
					representationDesc = (NoResString)"3 Indirect";
					break;
			}

			return $"N° agrément : {Declaration.JE_CustomsProfile} - Représentation : {representationDesc}";
		}
	}

	protected override ZString Box54DateCore => ZDateTime.Now.ToLongTimeString();

	public ZString Box54ValidationDate
	{
		get
		{
			var log = EntryHeader.Logs.Find(l => l.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && (l.SL_Reference == EntryStatusDescriptionCodeList.Codes.ES060 || l.SL_Reference == EntryStatusDescriptionCodeList.Codes.ES061)).OrderByDescending(l => l.SL_EventTime).FirstOrDefault();
			var eventTime = log?.SL_EventTime ?? ZDateTime.Empty;
			return eventTime.ToLongTimeString();
		}
	}

	public ZString Box54FirstReleaseDate => EntryHeader.CH_EntryReleaseDate.ToLongTimeString();

	public ZString Box54LastAmendmentDate
	{
		get
		{
			var log = EntryHeader.Logs.Find(l => l.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && l.SL_Reference == EntryStatusDescriptionCodeList.Codes.ES118).OrderByDescending(l => l.SL_EventTime).FirstOrDefault();
			var eventTime = log?.SL_EventTime ?? ZDateTime.Empty;
			return eventTime.ToLongTimeString();
		}
	}

	public ZString Box54InvalidationDate
	{
		get
		{
			var log = EntryHeader.Logs.Find(l => l.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && l.SL_Reference == EntryStatusDescriptionCodeList.Codes.ES150).OrderByDescending(l => l.SL_EventTime).FirstOrDefault();
			var eventTime = log?.SL_EventTime ?? ZDateTime.Empty;
			return eventTime.ToLongTimeString();
		}
	}

	public ZString Box54EditionDateLabel => Box54Date.IsEmpty ? ZString.Empty : Res.GetString("31596315-8860-4C4E-B4BA-98180281E560", "Date of Edition : ");

	public ZString Box54ValidationDateLabel => Box54ValidationDate.IsEmpty ? ZString.Empty : Res.GetString("0B8994B0-6415-4DFA-AFC6-36DEE48D8315", "Date of Validation : ");

	public ZString Box54FirstReleaseDateLabel => Box54FirstReleaseDate.IsEmpty ? ZString.Empty : Res.GetString("B6279E30-0F1E-49E5-97B5-3C4E309BFBA6", "Date of first Release : ");

	public ZString Box54LastAmendmentDateLabel => Box54LastAmendmentDate.IsEmpty ? ZString.Empty : Res.GetString("8F33D17A-FA9F-4698-BAA2-DF4CCFDDB3AB", "Date of last Amendment : ");

	public ZString Box54InvalidationDateLabel => Box54InvalidationDate.IsEmpty ? ZString.Empty : Res.GetString("B9A6AC2B-4B6B-4720-A65C-754015E60FE6", "Date of Invalidation : ");

	protected override ZString Box10LabelPart1CaptionCore => Res.GetString("B028A572-3A61-44B6-888B-8EAEDBC39373", "Cty.1st.dest");

	protected override bool ShowExportAccompanyingDocumentCore => false;

	public override ZString Box24TransactionNature
	{
		get { return EntryHeader?.EntryInstruction?.ZG_TransNature ?? ZString.Empty; }
	}

	protected override ZString TitleEUCommunityLabelCaptionCore => Res.GetString("4C21FCD4-983B-484A-B3E6-E08C4460BBB8", "EUROPEAN COMMUNITY - DAU");

	protected override ZString Box29ExitOfficeCore
	{
		get
		{
			ZString officeCode = EntryHeader.IsExport ? EntryHeader.OfficeOfExit : EntryHeader.OfficeOfEntry;
			ZString officeDesc = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, officeCode, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today)?.ZZD_Description ?? ZString.Empty;
			return officeDesc.IsEmpty ? officeCode : new ZString(officeCode + " - " + officeDesc);
		}
	}

	protected override ZString Box35GrossWeightInKGCore
	{
		get
		{
			ZDecimal result = EntryHeader.TotalInvoiceLinesGrossWeightInKG.Round(3);
			return result.IsEmpty ? (ZString)EadBlankBoxDashes : FormatDecimalDefault(result);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "France only")]
	public override ZString Box47aCaption => "T. Nat/T.Com";

	public override ZString Box47eMethodPayment => "Stat./MP";

	public override ZString Box17CountryOfDestination => Declaration.IsExport ? ((CodeDescriptionPairList)Declaration.Lookups.GoodsDestination).GetMultilingualDescriptionFromCode(Box17CountryOfDestinationCode) : ZString.Empty;

	public override ZString Box15CountryOfOrigin
	{
		get
		{
			var goodsOrigin = Box15GoodsOrigin;
			return goodsOrigin.IsEmpty ? ZString.Empty : ((CodeDescriptionPairList)Declaration.Lookups.GoodsOrigin).GetMultilingualDescriptionFromCode(goodsOrigin);
		}
	}

	public ZString Box15GoodsOrigin
	{
		get
		{
			var goodsOrigin = Declaration.JE_GoodsOrigin;
			return Declaration.IsExport && goodsOrigin == Core.Constants.CountryCodes.France ? ZString.Empty : goodsOrigin;
		}
	}

	#region Captions

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "France only")]
	public ZString Box33ECSupplementCaption => "1st CACO";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "France only")]
	public ZString Box33ECSupplement2Caption => "2nd CACO";

	public override ZString Box15SupplierState
	{
		get
		{
			var origin = Declaration.Origin;
			var state = ZString.Empty;

			if (origin != null && origin.RL_RN_NKCountryCode == Core.Constants.CountryCodes.France)
			{
				state = origin.CountryStates?.RW_Code ?? ZString.Empty;
			}
			else
			{
				var postCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Declaration.Factory, Declaration.JE_CustomsOffice, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today)?.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.PostCode) ?? ZString.Empty;
				if (postCode.Length > 2)
				{
					state = postCode.Left(2);
				}
			}

			return state;
		}
	}

	protected override ZString Box17ImporterStateCore
	{
		get
		{
			if (Declaration.IsImport)
			{
				if (Declaration.ImporterDeliveryAddress != null && Declaration.ImporterDeliveryAddress.E2_RN_NKCountryCode == Core.Constants.CountryCodes.France)
				{
					return Declaration.ImporterDeliveryAddress.E2_State;
				}

				if (Declaration.ImporterDocumentaryAddress != null)
				{
					return Declaration.ImporterDocumentaryAddress.E2_State;
				}
			}

			return ZString.Empty;
		}
	}

	public ZBool ShowBoxADetails
	{
		get
		{
			return Declaration.JE_MessageType == EUJobMessageTypeList.Codes.Export && !Declaration.JE_LocationOfGoods.IsEmpty && FRCustomsDataRegistry.DeltaGFallbackIsActive;
		}
	}

	public ZString BoxADetails
	{
		get
		{
			if (ShowBoxADetails)
			{
				var auth = Declaration.JE_LocationOfGoodsRelatedCusAuthorisation;
				string officeCode = auth.GetAuthorisationRuleValueWithCode(CusAuthorisationRuleTypeList.Codes.OFC).FirstOrDefault();
				string officeDesc = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, officeCode, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today)?.ZZD_Description ?? ZString.Empty;
				ZStringBuilder boxADetails = new ZStringBuilder();
				boxADetails.Append($"| {auth?.AppliesTo?.Country?.Code}  |  {officeCode} {officeDesc}\n");
				boxADetails.Append($"| {EntryHeader.FRCustomsFallbackNumber}  |  {ZDateTime.Today.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)}\n");
				boxADetails.Append($"| {auth?.PermitHolder?.OH_FullName}  |  {auth?.CPH_Number}");
				return boxADetails.ToString();
			}
			return ZString.Empty;
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "France only")]
	protected override ZString Box31PackagesAndDescriptionOfGoodsCaptionCore => new ZString("Marques et numéros - No(s) conteneur(s) - Nombre et nature");

	public override ZString FormatDecimalDefault(ZDecimal number) => (ZString)number.ToString("F3");

	#endregion

	public ZBool IsDeltaG1ExportFallBack => Declaration.JE_DeltaMode.Equals(OrgCusAccountDeltaGTypeList.Codes.G1) && Declaration.JE_MessageType.Equals(EUJobMessageTypeList.Codes.Export);

	protected override ZString LayoutStyle2Core => IsDeltaG1ExportFallBack ? new ZString("3") : base.LayoutStyle2Core;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "France only")]
	protected override ZString LayoutStyle2DescriptionCore => IsDeltaG1ExportFallBack ? new ZString("EXEMPLAIRE POUR L’EXPIDITEUR/L’EXPORTATEUR") : base.LayoutStyle2DescriptionCore;

	public ZString BoxDBarcode
	{
		get
		{
			var origin = Declaration.JE_GoodsOrigin;
			var result = ZString.Empty;
			if (origin == Core.Constants.CountryCodes.UnitedKingdom
				|| origin == Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes
				|| origin == Core.Constants.CountryCodes.Ireland)
			{
				result = ENO;
			}
			return result;
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "french text + translation;")]
	public ZString BoxDBarcodeCaption => new ZString("Code Barre à scanner avant embarquement\r\nBarcode to be scanned before boarding");

	#region Fallback

	protected override ZString FallbackStampTitleCore => EntryHeader.FallbackStampTitle;

	protected override ZString FallbackStampTextCore => EntryHeader.FallbackStampText;

	protected override ZString Box54NameOfDeclarantAndRepresentativeCore
	{
		get
		{
			switch (Declaration.JE_DeclarantType)
			{
				case EURepresentationTypeList.Codes._2Direct:
					return Declaration.Declarant?.Header.OH_FullName ?? ZString.Empty;
				case EURepresentationTypeList.Codes._1Self:
				case EURepresentationTypeList.Codes._3Indirect:
				default:
					return Declaration.Branch.Company.GC_Name;
			}
		}
	}

	protected override ZString Box54PlaceCore => Declaration.Branch.City;

	protected override GlbStaff GetStaffForBox54Details()
	{
		return Declaration.CusAgent;
	}

	protected override ZString Box54SignatoryNameAndPositionTitle => ZString.Empty;

	#endregion
	Money Box22TotalMoneyInvoiced => Factory.GetValue(ref box22TotalMoneyInvoicedCached, () =>
				{
					var currencyConverter = EntryHeader.CurrencyConverter;
					return EntryHeader.MergedLines.Aggregate(Money.Empty, (s, m) => currencyConverter.Add(s, m.CL_InvoiceMoney));
				});

	CachedProperty<Money> box22TotalMoneyInvoicedCached;

	protected override ZDecimal Box22TotalPriceInvoicedCore()
	{
		return Box22TotalMoneyInvoiced.Amount;
	}

	protected override ZString Box22TotalPriceCurrencyCore()
	{
		return Box22TotalMoneyInvoiced.Currency?.Code ?? ZString.Empty;
	}

	protected override ZDateTime DOECore
	{
		get
		{
			var log = EntryHeader.Logs.Find(l => l.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && l.SL_Reference == EntryStatusDescriptionCodeList.Codes.ES060).OrderByDescending(l => l.SL_EventTime).FirstOrDefault();

			if (log != null)
			{
				return log.SL_EventTime;
			}
			else
			{
				return EntryHeader.Messages.LastOutgoingMessage?.EM_SystemCreateTimeUtc ?? ZDateTime.Now;
			}
		}
	}

	protected override ZString Box19HasContainerCore => Declaration.IsContainerizedAndHasContainer ? "1" : "0";

	protected override GW.OrganisationWrapper GetOrganisationWrapperForSupplier()
	{
		if (Declaration.HasNonStandardCountryOfOrigin)
		{
			var addressToSend = Declaration.SupplierDocumentaryAddress;
			return new GW.OrganisationWrapper(GW.OrganisationUsageType.Consignor, addressToSend, Declaration.JE_GoodsOrigin, ((CodeDescriptionPairList)Declaration.Lookups.GoodsOrigin).GetMultilingualDescriptionFromCode(Declaration.JE_GoodsOrigin), Core.Constants.CountryCodes.France, Factory);
		}
		return base.GetOrganisationWrapperForSupplier();
	}

	protected override GW.OrganisationWrapper GetOrganisationWrapperForImporter()
	{
		if (Declaration.HasNonStandardCountryOfDestination)
		{
			var addressToSend = Declaration.ImporterDocumentaryAddress;
			return new GW.OrganisationWrapper(GW.OrganisationUsageType.Consignee, addressToSend, Declaration.JE_GoodsDestination, ((CodeDescriptionPairList)Declaration.Lookups.GoodsDestination).GetMultilingualDescriptionFromCode(Declaration.JE_GoodsDestination), Core.Constants.CountryCodes.France, Factory);
		}
		return base.GetOrganisationWrapperForImporter();
	}

	public ZString Box44LabelCaptionSuite => Res.GetString("7F9F18B7-4D2D-4BC0-A03D-0181A54C2AFD", "Additional information/ Documents produced/ Certificates and authorizations.\r\nFollowing");

	public ZString BoxDResultLabel => Res.GetString("5386C054-4F98-46C5-AA88-3B5AE9E73BF3", "Result:");
	public ZString BoxDSealsAffixedLabel => Res.GetString("5A86028F-6F13-457D-B628-5847A5BE4EE1", "Seals affixed: Number");
	public ZString BoxDTimeLimitLabel => Res.GetString("03FF7D15-A928-4294-B47F-DF37A1CE62A9", "Time limit (date):");
	public ZString BoxDIdentityLabel => Res.GetString("797D862A-6696-4FAB-9CC3-6A7CCA9FEE85", "Identity:");
	public ZString BoxDStampLabel => Res.GetString("D9783F28-389E-4F4A-AEAB-030CFAFEEA79", "Stamp:");
}
