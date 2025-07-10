using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using EUDocumentWrapperConstants = Enterprise.DocumentWrappers.Customs.EU.DocumentWrapperConstants;
using GW = Enterprise.DocumentWrappers.GenericWrappers;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public class DocSADH : DocBaseWrapper
	{
		// See here for notes:  http://customs.hmrc.gov.uk/channelsPortalWebApp/downloadFile?contentID=HMCE_PROD1_029510
		// http://customs.hmrc.gov.uk/channelsPortalWebApp/channelsPortalWebApp.portal?_nfpb=true&_pageLabel=pageImport_ShowContent&id=HMCE_PROD1_029511&propertyType=document

		public const string EadBlankBoxDashes = "---";

		public static DocSADH New(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
		{
			return new DocSADH(entryHeader, factoryToWrap);
		}

		protected DocSADH(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
			: base(entryHeader, factoryToWrap)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			Argument.NotNull(EntryHeader.Declaration, nameof(EntryHeader.Declaration));
			Argument.NotNull(EntryHeader.MergedLines, nameof(EntryHeader.MergedLines));
		}

		public CusEntryHeader EntryHeader { get; }

		public JobDeclaration Declaration => EntryHeader.Declaration;

		/// <summary>
		/// For SADH copy 3 style docs
		/// </summary>
		public DocSADHPageCollection Pages
		{
			get { return pages ?? (pages = GetPagesCore()); }
		}

		DocSADHPageCollection pages;

		protected virtual DocSADHPageCollection GetPagesCore()
		{
			return new DocSADHPageCollection(EntryHeader.MergedLines, Factory);
		}

		/// <summary>
		/// For new (July 2009) List-of-items type docs (EAD, ESS)
		/// </summary>
		public DocSADHLineCollection Lines
		{
			get { return lines ?? (lines = GetLinesCore()); }
		}

		DocSADHLineCollection lines;

		protected virtual DocSADHLineCollection GetLinesCore()
		{
			return new DocSADHLineCollection(EntryHeader.MergedLines, Factory);
		}

		public ZString SheetName => EntryHeader.MovementReferenceNumber.IsEmpty ? EntryHeader.CH_BGMReference : EntryHeader.MovementReferenceNumber;

		public ZString AgentsReference => AgentsReferenceCore;
		protected virtual ZString AgentsReferenceCore => ZString.Empty;

		public ZString BoxS13CountriesOfRouting => BoxS13CountriesOfRoutingCore;

		protected virtual ZString BoxS13CountriesOfRoutingCore
		{
			get
			{
				return EntryHeader.CountriesOfRouting == null ? ZString.Empty :
						ZString.Join(EUDocumentWrapperConstants.Delimiters.CommaAndSpace, EntryHeader.CountriesOfRouting.ToArray());
			}
		}

		public GW.OrganisationWrapper Box2Supplier
		{
			get
			{
				GW.OrganisationWrapper result = null;
				if (Declaration.SupplierDocumentaryAddress != null)
				{
					result = GetOrganisationWrapperForSupplier();
				}
				return result;
			}
		}

		protected virtual GW.OrganisationWrapper GetOrganisationWrapperForSupplier() => new GW.OrganisationWrapper(GW.OrganisationUsageType.Consignor, Declaration.SupplierDocumentaryAddress, Factory);

		public ZBool ShowBox2SupplierCountryCode => ShowBox2SupplierCountryCodeCore;
		protected virtual ZBool ShowBox2SupplierCountryCodeCore => true;

		public GW.OrganisationWrapper Box8Importer
		{
			get
			{
				GW.OrganisationWrapper result = null;
				if (Declaration.ImporterDocumentaryAddress != null)
				{
					result = GetOrganisationWrapperForImporter();
				}
				return result;
			}
		}

		protected virtual GW.OrganisationWrapper GetOrganisationWrapperForImporter() => new GW.OrganisationWrapper(GW.OrganisationUsageType.Consignee, Declaration.ImporterDocumentaryAddress, Factory);

		public bool ShowExportAccompanyingDocument => ShowExportAccompanyingDocumentCore;
		protected virtual bool ShowExportAccompanyingDocumentCore => true;

		public ZBool ShowBox8ImporterCountryCode => ShowBox8ImporterCountryCodeCore;
		protected virtual ZBool ShowBox8ImporterCountryCodeCore => true;

		public IBisPageTraderBox BisPageTraderBox => GetBisPageTraderBox();

		protected virtual IBisPageTraderBox GetBisPageTraderBox() => new ImporterBisPageTraderBox(this);

		public DocAddress Box14DeclarantRepresentative => Box14DeclarantRepresentativeCore;

		protected virtual DocAddress Box14DeclarantRepresentativeCore
		{
			get
			{
				if (ShowDeclarantRepresentativeAddress)
				{
					return DocAddress.New(Declaration.Declarant, Factory);
				}
				else
				{
					return null;  // Show nothing when using self rep
				}
			}
		}

		public ZString Box14AlternativeText => Box14AlternativeTextCore;

		protected virtual ZString Box14AlternativeTextCore => ZString.Empty;

		public ZBool ShowDeclarantRepresentativeAddress => ShowDeclarantRepresentativeAddressCore;
		protected virtual ZBool ShowDeclarantRepresentativeAddressCore => Declaration.JE_DeclarantType != RepresentationTypeList.Codes._1Self;

		public ZString Box21IdentityOfTransportCrossingTheBorder
		{
			get
			{
				return GetTransportIdentityBox21(Declaration);
			}
		}

		public ZString Box21TransportNationality
		{
			get { return Declaration.JE_RN_NKTransportNationality; }
		}

		public virtual ZString Box24TransactionNature
		{
			get { return EntryHeader.RandomHeader.JZ_ValuationCode; }
		}

		public ZString Box26TransportModeInland
		{
			get { return Declaration.TransportModeTranslator.TranslateToWCOCode(Declaration.JE_TransportModeInland, true); }
		}

		public static ZString GetTransportIdentityBox21(JobDeclaration declaration)
		{
			ZString result;
			switch (declaration.JE_TransportMode)
			{
				case Enterprise.Customs.Business.TransportTypeList.Codes.Air:
					result = declaration.JE_VoyageFlightNo + (declaration.JE_ExportDate.IsValid ? " / " + declaration.JE_ExportDate.ToShortDateString() : "");
					break;

				case Enterprise.Customs.Business.TransportTypeList.Codes.Sea:
					result = declaration.JE_VesselName + (declaration.JE_VoyageFlightNo.IsValid ? " / " + declaration.JE_VoyageFlightNo : "");
					break;

				default:
					result = declaration.JE_VesselName;
					break;
			}
			return result.IsEmpty ? (ZString)EadBlankBoxDashes : result;
		}

		public ZString Box18IdentityOfTransportAtDeparture => Box18IdentityOfTransportAtDepartureCore;

		protected virtual ZString Box18IdentityOfTransportAtDepartureCore => Declaration.Box18IdentityOfTransportAtDepartureForDocumentsAndMessaging;

		public ZString Box18TransportNationalityAtDeparture => Box18TransportNationalityAtDepartureCore;

		protected virtual ZString Box18TransportNationalityAtDepartureCore
		{
			get
			{
				var declaration = Declaration;
				if (declaration.IsExport)
				{
					if (declaration.IsUCC6)
					{
						return GetBox18TransportNationalityForExportUCC6Departure(declaration);
					}
					return declaration.ZG_Box18TransportNationality;
				}
				return ZString.Empty;
			}
		}

		protected virtual ZString GetBox18TransportNationalityForExportUCC6Departure(JobDeclaration declaration)
		{
			if (declaration.JE_TransportModeInland == Core.Constants.TransportModes.Air)
			{
				if (!declaration.JE_TransportIDInland.IsEmpty)
				{
					return declaration.JE_RN_NKTransportNationalityInland;
				}

				if (!declaration.JE_AircraftRegistrationInland.IsEmpty)
				{
					return declaration.JE_RN_NKTrailer1Nationality;
				}

				return ZString.Empty;
			}

			if (declaration.JE_TransportModeInland == Core.Constants.TransportModes.Rail)
			{
				if (!declaration.JE_TransportIDInland.IsEmpty)
				{
					return declaration.JE_RN_NKTransportNationalityInland;
				}

				if (!declaration.JE_Trailer1RegNo.IsEmpty)
				{
					return declaration.JE_RN_NKTrailer1Nationality;
				}

				return ZString.Empty;
			}

			return declaration.JE_RN_NKTransportNationalityInland;
		}

		public ZString Box20ShipmentIncoTerm => Box20ShipmentIncoTermCore;
		protected virtual ZString Box20ShipmentIncoTermCore => IncoTermFields.ShipmentIncoTerm;

		public ZString Box20AgreedPlace => Box20AgreedPlaceCore;
		protected virtual ZString Box20AgreedPlaceCore => IncoTermFields.IncoTermPlace;

		IncoTermFields IncoTermFields => incoTermFields ?? (incoTermFields = new IncotermFieldsBox20Evaluator(EntryHeader).Evaluate());
		IncoTermFields incoTermFields;

		public ZString Box20AgreedPlaceCode => Box20AgreedPlaceCodeCore;

		protected virtual ZString Box20AgreedPlaceCodeCore => IncoTermFields.AgreedPlaceCode;

		public ZString Box7ReferenceNumber => Box7ReferenceNumberCore;

		protected virtual ZString Box7ReferenceNumberCore => Declaration.TradersOwnReferenceFullForBox7;

		public ZDecimal Box22TotalPriceInvoiced
		{
			get { return Box22TotalPriceInvoicedCore(); }
		}

		protected virtual ZDecimal Box22TotalPriceInvoicedCore()
		{
			return EntryHeader.TotalPrice.Amount;
		}

		public ZString Box22TotalPriceCurrency
		{
			get { return Box22TotalPriceCurrencyCore(); }
		}

		protected virtual ZString Box22TotalPriceCurrencyCore()
		{
			return EntryHeader.TotalPrice.Currency == null ? "" : EntryHeader.TotalPrice.Currency.Code;
		}

		public ZString Box27PortOfLoading => Box27PortOfLoadingCore;
		protected virtual ZString Box27PortOfLoadingCore => Declaration.JE_RL_NKPortOfLoading;

		public ZString BoxS28Seals => new DocSADHSealsProvider(this).GetBoxS28Seals();

		public ZString Box29ExitOffice => Box29ExitOfficeCore;
		protected virtual ZString Box29ExitOfficeCore => new Box29OfficeCodeEvaluator(EntryHeader).Evaluate();

		public ZString Box29ExitOfficeLabel => Box29ExitOfficeLabelCore;
		protected virtual ZString Box29ExitOfficeLabelCore => Res.GetString("4DB55363-E6B4-4D59-9373-DEE7C9BAF2E6", "29 Office of exit/entry");

		public ZBool IsIndirectExport => EntryHeader.IsIndirectExport;

		public ZBool ShouldShowNotInEcsCaptionIfNeeded => ShouldShowNotInEcsCaptionIfNeededCore;
		protected virtual ZBool ShouldShowNotInEcsCaptionIfNeededCore => ZBool.True;

		public ZString BoxS29TransportChargesMoP => BoxS29TransportChargesMoPCore;

		protected virtual ZString BoxS29TransportChargesMoPCore
		{
			get
			{
				ZString mop = EntryHeader.TransportChargesMoP;
				return mop.IsEmpty ? (ZString)DocSADH.EadBlankBoxDashes : mop;
			}
		}

		public ZString Box1bSubStyle => Box1bSubStyleCore;
		protected virtual ZString Box1bSubStyleCore => EntryHeader.EntryInstruction?.CEI_SubStyle ?? Declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault()?.CEI_SubStyle ?? ZString.Empty; // e.g. A, D,

		public ZString Box1aEntryStyle => EntryHeader.Declaration.JE_EntryStyle; // e.g. EX, IM, CO

		public ZString Box30LocationOfGoods => Box30LocationOfGoodsCore;
		protected virtual ZString Box30LocationOfGoodsCore => Declaration.Box30LocationOfGoodsForDocumentsAndMessaging;

		// For the entry, not the line
		public ZString Box35GrossWeightInKG => Box35GrossWeightInKGCore;

		protected virtual ZString Box35GrossWeightInKGCore
		{
			get
			{
				ZDecimal result = EntryHeader.GrossWeight.InKilogramsSafe.Round(3);
				return result.IsEmpty ? (ZString)EadBlankBoxDashes : FormatDecimalDefault(result);
			}
		}

		public virtual ZString FormatDecimalDefault(ZDecimal number) => (ZString)number.ToString();

		public virtual ZString Box15CountryOfOrigin => Declaration.Origin?.Country?.Description ?? ZString.Empty;

		public virtual ZString Box17CountryOfDestination => Declaration.FinalDestination?.Country?.Description ?? ZString.Empty;

		public ZString BoxDControlResult => BoxDControlResultCore;
		protected virtual ZString BoxDControlResultCore => ZString.Empty;

		public ZString BoxDSignature => BoxDSignatureCore;
		protected virtual ZString BoxDSignatureCore => ZString.Empty;

		/// <summary>
		/// Field will become mandatory for Indirect Exports via another EU member state when UK introduces ECS.
		/// </summary>
		public ZString BoxDContainerSealsAffixed => BoxDContainerSealsAffixedCore;

		protected virtual ZString BoxDContainerSealsAffixedCore
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				foreach (CusContainer container in EntryHeader.Containers)
				{
					result.AppendIfNotEmpty(container.CO_Seal);
				}
				return result.ToStringWithDelimiterBetweenAppends(EUDocumentWrapperConstants.Delimiters.CommaAndSpace);
			}
		}

		public ZString Box54Place => Box54PlaceCore;

		protected virtual ZString Box54PlaceCore
		{
			get
			{
				RefUNLOCO homeport = Declaration.Branch.HomePort;
				return homeport != null ? homeport.Description : ZString.Empty;
			}
		}

		public ZString Box54Date => Box54DateCore;

		protected virtual ZString Box54DateCore => FormatDateDefault(ZDateTime.Today);

		/// <summary>
		/// No box number on EAD
		/// </summary>
		public ZString IssuingDate
		{
			get
			{
				return EntryHeader.CusEntryNumber != null && !EntryHeader.CusEntryNumber.CE_IssueDate.IsEmpty
						? (ZString)EntryHeader.CusEntryNumber.CE_IssueDate.ToString(IssuingDateFormat)
						: ZString.Empty;
			}
		}

		protected virtual string IssuingDateFormat => "yyyyMMdd";

		public ZString TimeLimitDate => TimeLimitDateCore;

		protected virtual ZString TimeLimitDateCore => ZString.Empty;

		protected virtual string TimeLimitDateFormat => "yyyyMMdd";

		public ZString Box54NameOfDeclarantAndRepresentative => Box54NameOfDeclarantAndRepresentativeCore;

		protected virtual ZString Box54NameOfDeclarantAndRepresentativeCore
		{
			get
			{
				switch (Declaration.JE_DeclarantType)
				{
					case RepresentationTypeList.Codes._2Direct:
						ZStringBuilder result = new ZStringBuilder();
						if (Declaration.Declarant != null)
						{
							result.Append(Declaration.Declarant.Header.OH_FullName);
							result.Append((NoResString)"by");
							result.Append(Declaration.Branch.Company.GC_Name);
						}
						return result.ToStringWithDelimiterBetweenAppends(" ");

					case RepresentationTypeList.Codes._1Self:
					case RepresentationTypeList.Codes._3Indirect:
					default:
						return Declaration.Branch.Company.GC_Name;
				}
			}
		}

		protected GlbStaff staffForBox54Details => _staffForBox54Details ?? (_staffForBox54Details = GetStaffForBox54Details());
		GlbStaff _staffForBox54Details;

		protected virtual GlbStaff GetStaffForBox54Details()
		{
			var lastOutgoingMessage = EntryHeader.Messages.LastOutgoingNonSystemAndNonNullUserMessage;
			if (lastOutgoingMessage != null)
			{
				var result = lastOutgoingMessage.UserWhoQueuedThisRecord;
				if (result != null)
				{
					return result;
				}
			}
			return Declaration.CusAgent;
		}

		public ZString Box54SignatoryNameAndPosition => staffForBox54Details == null ? string.Empty
															: staffForBox54Details.GS_FullName + Box54SignatoryNameAndPositionTitle;

		protected virtual ZString Box54SignatoryNameAndPositionTitle => (staffForBox54Details.GS_Title.IsEmpty ? string.Empty : " (" + staffForBox54Details.GS_Title + ")");

		public ZString Box54SignatoryContactDetails => Box54SignatoryContactDetailsCore;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No Smell")]
		protected virtual ZString Box54SignatoryContactDetailsCore
		{
			get
			{
				var result = new ZStringBuilder();
				result.Append((NoResString)"Tel:" + Declaration.Branch.GB_Phone_Formatted);
				if (staffForBox54Details != null && !staffForBox54Details.GS_WorkPhone_Formatted.IsEmpty)
				{
					result.Append("Direct:" + staffForBox54Details.GS_WorkPhone_Formatted);
				}
				return result.ToStringWithDelimiterBetweenAppends("  ");
			}
		}

		public ZString EPU => EPUCore;

		protected virtual ZString EPUCore
		{
			get
			{
				ZString[] entryNumberParts = EntryHeader.EntryNumber.Split('-');
				if (entryNumberParts.Length == 2)
				{
					return entryNumberParts[0];
				}
				else
				{
					return "";
				}
			}
		}

		public ZString ENO => ENOCore;

		protected virtual ZString ENOCore
		{
			get
			{
				ZString[] entryNumberParts = EntryHeader.EntryNumber.Split('-');
				if (entryNumberParts.Length == 2)
				{
					return entryNumberParts[1];
				}
				else
				{
					return EntryHeader.EntryNumber;
				}
			}
		}

		public ZDateTime DOE => DOECore;

		protected virtual ZDateTime DOECore
		{
			get
			{
				// Date of Entry comes from HMRC but if we ain't got one then the last outbound message is a good bet
				if (EntryHeader.CusEntryNumber != null && !EntryHeader.CusEntryNumber.CE_IssueDate.IsEmpty)
				{
					return EntryHeader.CusEntryNumber.CE_IssueDate;
				}
				else
				{
					EDIMessage lastSentMessage = EntryHeader.Messages.LastOutgoingMessage;
					return lastSentMessage != null ? lastSentMessage.EM_SystemCreateTimeUtc : ZDateTime.Now;
				}
			}
		}

		public ZString BoxA => EntryHeader.SadBoxAText;

		public ZString BoxAType
		{
			get
			{
				var boxAType = BoxATypeCore;
				if (!boxAType.IsEmpty)
				{
					return FormattableString.Invariant($"{boxAType}:");
				}
				return ZString.Empty;
			}
		}

		protected virtual ZString BoxATypeCore => ZString.Empty;

		public ZString Box12ValueDetails => Box12ValueDetailsCore;
		protected virtual ZString Box12ValueDetailsCore => "0.0";

		public ZString Box16CountryOfOrigin => EvaluateBox16CountryOfOrigin();

		ZString EvaluateBox16CountryOfOrigin() => GetBox16CountryOfOriginEvaluator()?.Evaluate() ?? ZString.Empty;

		protected virtual IBox16CountryOfOriginEvaluator GetBox16CountryOfOriginEvaluator() => null;

		public ZString Box23ExchangeRate
		{
			get
			{
				var entryHeader = EntryHeader;
				var exchangeRate = entryHeader?.IsMultiInvoiceCurrency ?? ZBool.False
					? (ZDecimal)1m
					: entryHeader?.RandomHeader.JZ_InvoiceCurrExRate.Round(6) ?? ZDecimal.Zero;

				return exchangeRate == ZDecimal.Zero ? ZString.Empty : (ZString)exchangeRate.ToString(ExchangeRateDecimalFormat);
			}
		}

		public ZString Box28FinancialAndBankingDataLine1 => Box28FinancialAndBankingDataLine1Core;
		protected virtual ZString Box28FinancialAndBankingDataLine1Core => ZString.Empty;

		public ZString Box28FinancialAndBankingDataLine2 => Box28FinancialAndBankingDataLine2Core;
		protected virtual ZString Box28FinancialAndBankingDataLine2Core => ZString.Empty;

		public ZString BoxABarcode => BoxABarcodeCore;
		protected virtual ZString BoxABarcodeCore => ZString.Empty;

		public ZString BoxBAccountingDetails => BoxBAccountingDetailsCore;
		protected virtual ZString BoxBAccountingDetailsCore => ZString.Empty;

		public ZString BoxCOfficeOfDeparture => BoxCOfficeOfDepartureCore;
		protected virtual ZString BoxCOfficeOfDepartureCore => ZString.Empty;

		public ZString BoxC => BoxCCore;
		protected virtual ZString BoxCCore => ZString.Empty;

		public ZString Box14DetailsFooter => Box14DetailsFooterCore;
		protected virtual ZString Box14DetailsFooterCore => ZString.Empty;

		public ZString Box19HasContainer => Box19HasContainerCore;
		protected virtual ZString Box19HasContainerCore => Declaration.IsContainerised && EntryHeader.Containers.Any(x => !x.CO_ContainerNumber.IsEmpty) ? "1" : "0";

		public virtual ZString Box15SupplierState => ZString.Empty;

		public ZString Box17ImporterState => Box17ImporterStateCore;
		protected virtual ZString Box17ImporterStateCore => ZString.Empty;

		public virtual ZString Box47aCaption => Res.GetString("145325AE-D0FB-4B50-A5EC-14E34C10CF9E", "Type");

		public virtual ZString Box47eMethodPayment => Res.GetString("2B865D98-DC08-4D66-A628-4A6AAFBE66DD", "MP");

		protected virtual ZString FormatDateDefault(ZDateTime date) => date.ToShortDateString();

		public ZBool ShowEpuEnoDoeLabelsText => ShowEpuEnoDoeLabelsTextCore;
		protected virtual ZBool ShowEpuEnoDoeLabelsTextCore => true;

		public ZBool ShowEpuEnoDoeLabelsTextOnBIS => ShowEpuEnoDoeLabelsTextOnBISCore;
		protected virtual ZBool ShowEpuEnoDoeLabelsTextOnBISCore => true;

		public ZString Box6TotalNoOfPacks => Box6TotalNoOfPacksCore();

		protected virtual ZString Box6TotalNoOfPacksCore() => EntryHeader.PackagesCount.ToString();

		public ZString Box17CountryOfDestinationCode => Box17CountryOfDestinationCodeCore;
		protected virtual ZString Box17CountryOfDestinationCodeCore => Declaration.JE_GoodsDestination;

		public ZBool ShowBox18LabelText => ShowBox18LabelTextCore;
		protected virtual ZBool ShowBox18LabelTextCore => Declaration.IsExport;

		public ZString Box20AgreedPlaceCode2 => Box20AgreedPlaceCode2Core;
		protected virtual ZString Box20AgreedPlaceCode2Core => IncoTermFields.AgreedPlaceCode2;

		public ZBool ShowBoxDLabelsText => ShowBoxDLabelsTextCore;
		protected virtual ZBool ShowBoxDLabelsTextCore => true;

		#region Captions

		public ZString EadBarcode => EadBarcodeCore;
		protected virtual ZString EadBarcodeCore => Box29ExitOffice.IsEmpty ? ZString.Empty : EntryHeader.MovementReferenceNumber;

		public ZString DispatchOfficeTitleCaption => DispatchOfficeTitleCaptionCore;
		protected virtual ZString DispatchOfficeTitleCaptionCore => Res.GetString("C93F7477-9ED2-4B29-BEF8-D0579181D48F", "A OFFICE OF DISPATCH/EXPORT/DESTINATION");

		public ZString DispatchOfficeTitleBisPageCaption => DispatchOfficeTitleBisPageCaptionCore;
		protected virtual ZString DispatchOfficeTitleBisPageCaptionCore => Res.GetString("0B4FBAA0-8574-4852-98A1-199EC25A3D5E", "OFFICE OF DISPATCH/EXPORT/DESTINATION");

		public ZString Box10LabelPart1Caption => Box10LabelPart1CaptionCore;
		protected virtual ZString Box10LabelPart1CaptionCore => Res.GetString("41CC9084-C987-4AF9-A88B-2D49BD4B7205", "Cty.1st.dest/");
		public ZString Box10LabelPart2Caption => Box10LabelPart2CaptionCore;
		protected virtual ZString Box10LabelPart2CaptionCore => Res.GetString("951C9C84-47D1-45F2-883B-8DA1123680C3", "last consig.");

		public ZString Box11LabelPart1Caption => Box11LabelPart1CaptionCore;
		protected virtual ZString Box11LabelPart1CaptionCore => Res.GetString("494AC05B-6420-4D40-9B0E-6F7EF23B4D43", "Trad./Prod");
		public ZString Box11LabelPart2Caption => Box11LabelPart2CaptionCore;
		protected virtual ZString Box11LabelPart2CaptionCore => Res.GetString("EF2D72DD-63BE-4402-9EC5-5FA6A0DF76CE", "country/region.");

		public ZString Box18LabelCaption => Box18LabelCaptionCore;
		protected virtual ZString Box18LabelCaptionCore => Res.GetString("FE00744F-2918-4767-BF00-B3D465A488AF", "18 Identity and nationality of means of transport at departure/on arrival");

		public ZString Box27LabelCaption => Box27LabelCaptionCore;
		protected virtual ZString Box27LabelCaptionCore => Res.GetString("A5AA64B6-2034-4B3D-A4C2-97FF20045B1C", "27 Place of loading/unloading");

		public ZString BoxDLetterLabelCaption => BoxDLetterLabelCaptionCore;
		protected virtual ZString BoxDLetterLabelCaptionCore => "D/J";

		public ZString BoxDLabelCaption => BoxDLabelCaptionCore;
		protected virtual ZString BoxDLabelCaptionCore => Res.GetString("75F8DD11-428B-4E5B-ADE0-A95D3BFD4F48", "CONTROL BY OFFICE OF DEPARTURE/DESTINATION");

		public ZString Box43LabelCaption => Box43LabelCaptionCore;
		protected virtual ZString Box43LabelCaptionCore => Res.GetString("E44A583A-D9A6-466B-8CAE-9054A63013BF", "VM Code");

		public ZString Box31PackagesAndDescriptionOfGoodsCaption => Box31PackagesAndDescriptionOfGoodsCaptionCore;
		protected virtual ZString Box31PackagesAndDescriptionOfGoodsCaptionCore => Res.GetString("550A451C-4AD3-4161-9A4B-17F5BBF456E9", "Marks and numeration - Container(s) number(s) – Number and class");

		public ZString TitleEUCommunityLabelCaption => TitleEUCommunityLabelCaptionCore;
		protected virtual ZString TitleEUCommunityLabelCaptionCore => Res.GetString("5F504571-5849-40EB-9F33-FD66715F048E", "EUROPEAN COMMUNITY");

		public ZString Box1LabelCaption => Res.GetString("4D423818-D7AB-445B-9797-98D5436B7199", "1 D E C L A R A T I O N");
		public ZString Box2LabelCaption => Res.GetString("AF77817B-E4FB-4955-8EC1-3586B58B66A9", "2 Consignor/Exporter");
		public ZString Box3LabelCaption => Res.GetString("5D3D7EE7-E9A1-4C9E-BB43-8AACC07E3662", "Forms");
		public ZString Box4LabelCaption => Res.GetString("DD9250C8-E66F-4007-A62C-B3E740237110", "Loading lists");
		public ZString Box5LabelCaption => Res.GetString("EC17DF0D-B2E4-4165-ADA7-8FD7269A2243", "Items");
		public ZString Box6LabelCaption => Res.GetString("20F3E1CA-9BDA-4AF7-85D4-5AB5E128CD82", "Total packages");
		public ZString Box7LabelCaption => Res.GetString("0A137B52-6762-4EB2-B6A6-3B3ADD75ED60", "Reference number");
		public ZString Box8LabelCaption => Res.GetString("4538AD9C-9AF1-4737-9B26-082B14696785", "8 Consignee");
		public ZString Box9LabelCaption => Res.GetString("3F46C7D7-AA7F-49BA-B2FB-B93A0AF588A6", "Person responsible for financial settlement");
		public ZString Box12LabelCaption => Res.GetString("70945EF3-6213-45C9-BCB4-D1FC6F31108B", "Value details");
		public ZString Box13LabelCaption => Res.GetString("DE057661-A093-4051-9C1B-CD7BE3A27C20", "CAP");
		public ZString Box14LabelCaption => Res.GetString("5F522532-DE1A-4D6C-8D34-9A1763E8AE84", "Declarant/Representative");
		public ZString Box15LabelCaption => Res.GetString("33E1CEEE-5A1D-4835-AF8A-6F67FBA21482", "Country/Region of dispatch/export");
		public ZString Box15CodeLabelCaption => Res.GetString("8746DF17-2025-4A4A-8651-304C4714F9A5", "C disp./exp. Code");
		public ZString Box16LabelCaption => Res.GetString("9EEBD3C7-92D7-42C9-9E7B-4C5E9633EABC", "Country/Region of origin");
		public ZString Box17LabelCaption => Res.GetString("6F758233-BE29-40C2-B3AA-A4260F134586", "Country/Region of destination");
		public ZString Box17CodeLabelCaption => Res.GetString("8BEC4ACC-9170-4E3D-9D01-9183544AAA23", "Country/Region destin. Code");
		public ZString Box20LabelCaption => Res.GetString("00D043D1-09E0-471B-995D-2922320C3834", "Delivery terms");
		public ZString Box21LabelCaption => Res.GetString("44B61770-2234-4D12-8FD6-238A62643B34", "Identity and nationality of active means of transport crossing the border");
		public ZString Box22LabelCaption => Res.GetString("63128CEC-5DBE-401F-B31E-FC0168C8AE5B", "Currency and total amount invoiced");
		public ZString Box23LabelCaption => Res.GetString("86DEBD67-128D-42C4-8C6A-74DD1630AED8", "Exchange rate");
		public ZString Box24Part1LabelCaption => Res.GetString("1503AF6A-B0A4-48BF-8FB8-59277675D5A8", "Nature of");
		public ZString Box24Part2LabelCaption => Res.GetString("46FF72F6-382E-4C6E-A9FB-978CE0822BB1", "transaction");
		public ZString Box25Part1LabelCaption => Res.GetString("09CE966F-50B2-4E2D-994C-EF3E50607C70", "Mode of Transport");
		public ZString Box25Part2LabelCaption => Res.GetString("E49F7142-0001-4D7A-BB25-445001714A66", "at the border");
		public ZString Box26Part1LabelCaption => Res.GetString("95B6A269-1354-450A-A4C0-C655FA2D7C94", "Inland mode");
		public ZString Box26Part2LabelCaption => Res.GetString("25EE6388-E12B-42B3-96B1-9983DBC7D6F2", "of transport");
		public ZString Box28LabelCaption => Res.GetString("39212426-66BD-45DA-B851-DD1B996395C4", "Financial and banking data");
		public ZString Box30LabelCaption => Res.GetString("450D243D-2146-4806-B4B9-54B69B89807A", "Location of goods");
		public ZString Box31LabelCaption => Res.GetString("0AEA5AF0-6448-44C3-A46E-06D6FCF83DFC", "Packages and description of goods");
		public ZString Box32LabelCaption => Res.GetString("C2F35B35-5F7C-408E-97F2-06BDBC449EC5", "Item");
		public ZString Box33LabelCaption => Res.GetString("27E62CB6-83C3-4FFB-A394-F03D5B97A4E7", "Commodity Code");
		public ZString Box34LabelCaption => Res.GetString("6DB2C71F-A6E3-4B02-B7E4-62EB5E1299BA", "Country/Region origin Code");
		public ZString Box35LabelCaption => Res.GetString("281DD472-1825-44A5-8136-6B972C60E305", "Gross Mass (kg)");
		public ZString Box36LabelCaption => Res.GetString("E31FA5A8-BB88-4E4D-B2A7-B136B5923953", "Preference");
		public ZString Box37LabelCaption => Res.GetString("3EBB8907-9EFA-4C27-AA07-F65C6273B121", "P R O C E D U R E");
		public ZString Box38LabelCaption => Res.GetString("B418E926-A464-4965-A2C8-2043E13B5200", "Net Mass (kg)");
		public ZString Box39LabelCaption => Res.GetString("16FFA82F-FC41-45DB-8309-60891F3E38E2", "Quota");
		public ZString Box40LabelCaption => Res.GetString("BE64A7DC-ED7E-43F7-887E-F2E480E09661", "Summary declaration/Previous document");
		public ZString Box41LabelCaption => Res.GetString("5D82D764-18E1-4DF8-ADE9-2F4F3271BAC6", "Supplementary Units");
		public ZString Box42LabelCaption => Res.GetString("9F6F2B37-D74F-487F-BE15-DC8D55D2D4E2", "Item price");
		public ZString Box44LabelCaption => Res.GetString("A8B8C9B6-99C7-4E1F-AB61-7C7C87610E94", "Additional information/ Documents produced/ Certificates and authorizations");
		public ZString AICodeLabelCaption => Res.GetString("D1562DA2-8B1F-4C67-85E0-984C0BC4813A", "A.I.Code");
		public ZString Box45LabelCaption => Res.GetString("09DE2938-0CB0-458D-A4D8-50B9A497692B", "Adjustment");
		public ZString Box46LabelCaption => Res.GetString("C548E728-A723-41C1-9950-0A3598D6EEBA", "Statistical value");
		public ZString Box47LabelCaption => Res.GetString("D57F8115-3208-449E-84BA-BD0B656204EC", "Calculation of taxes");
		public ZString TaxBaseLabelCaption => Res.GetString("FD326A50-AF47-432F-BA4D-8D1AFBB154D8", "Tax base");
		public ZString TaxRateLabelCaption => Res.GetString("F82809D5-4E9C-420E-AE1F-CD1A1DF75896", "Rate");
		public ZString TaxAmountLabelCaption => Res.GetString("838B43F7-ABB4-495D-80D0-1CE08389FA96", "Amount");
		public ZString Box48LabelCaption => Res.GetString("C5C9D1BF-C27F-47AF-9E27-BB0C8EA98ECA", "Deferred Payment");
		public ZString Box49LabelCaption => Res.GetString("5B581CAC-0798-4F2A-B572-8DA399E89556", "Identification of warehouse");
		public ZString BoxBLabelCaption => BoxBLabelCaptionCore;
		protected virtual ZString BoxBLabelCaptionCore => Res.GetString("50712047-86F7-4BA4-8104-65628BADEFBE", "ACCOUNTING DETAILS");
		public ZString Box50LabelCaption => Res.GetString("1747F749-3278-4BD7-A2E3-145B55B05F54", "Principal");
		public ZString Box50SignatureLabelCaption => Res.GetString("8D21BECF-5E30-4629-A306-59D0C53A0363", "Signature:");
		public ZString Box50RepresentedLabelCaption => Res.GetString("57F48D09-70B5-4412-81DF-103A09081AEE", "represented by");
		public ZString Box50PlaceDateLabelCaption => Res.GetString("732DE87C-5063-42A6-9B6F-50CFC0B43EAD", "Place and date:");
		public ZString BoxCLabelCaption => Res.GetString("DCA654BC-1117-4E46-90D3-97CF70B8644F", "OFFICE OF DEPARTURE");
		public ZString Box51LabelCaption => Res.GetString("200DFE96-6996-4460-837E-118F561EAA52", "Intended offices of transit (and country/region)");
		public ZString Box52Part1LabelCaption => Res.GetString("63556C18-FFFC-4D60-8B72-1F24312EEB5A", "Guarantee");
		public ZString Box52Part2LabelCaption => Res.GetString("C6ECAE69-22AE-4EAF-AC45-811D89A808E4", "not valid for");
		public ZString Box52Part3LabelCaption => Res.GetString("DCFAEA06-41D7-49AD-AB8D-15277BD6DF22", "Code");
		public ZString Box53LabelCaption => Res.GetString("32B09A7B-E23F-4EF2-B709-B938A1FB216E", "Office of destination (and country/region)");
		public ZString Box54Part1LabelCaption => Res.GetString("EBE7686E-35AE-4986-AFD2-B6C3A46AC520", "Place and date:");
		public ZString Box54Part2LabelCaption => Res.GetString("9E055FBB-D85A-4E50-953D-F090396A6876", "Signature and name of declarant/representative");
		public ZString Box47BisTotalFirstItemLabelCaption => Res.GetString("8FFA01CA-60AF-4AAD-9830-BC16174C98F9", "Total first item:");
		public ZString Box47BisTotalSecondItemLabelCaption => Res.GetString("C369DC6F-D3B3-486D-B9A6-57052B6CE727", "Total second item:");
		public ZString Box47BisTotalThirdItemLabelCaption => Res.GetString("7E5D99AC-FB17-40E8-8FC4-9BFC4B2C87FC", "Total third item:");
		public ZString Box47BisSummaryLabelCaption => Res.GetString("CFE57611-5C36-4436-B8DE-DA9F4327551A", "Summary");
		public ZString Box47BisGTLabelCaption => Res.GetString("64FF76E1-3488-45E1-A8A3-7D3FF9793D71", "GT");

		#endregion

		#region Copy Number

		public ZString LayoutStyle2 => LayoutStyle2Core;
		protected virtual ZString LayoutStyle2Core => DefaultLayoutStyle2;

		public ZString LayoutStyle2Description => LayoutStyle2DescriptionCore;
		protected virtual ZString LayoutStyle2DescriptionCore => DefaultLayoutStyle2Description;

		const string DefaultLayoutStyle2 = "6";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No Smell")]
		const string DefaultLayoutStyle2Description = "Copy for the country of destination";

		#endregion

		#region Fallback

		public ZString FallbackStampTitle => FallbackStampTitleCore;
		protected virtual ZString FallbackStampTitleCore => ZString.Empty;

		public ZString FallbackStampText => FallbackStampTextCore;
		protected virtual ZString FallbackStampTextCore => ZString.Empty;

		#endregion

		public bool BoxS00SafeAndSecurity => BoxS00SafeAndSecurityCore;
		protected virtual bool BoxS00SafeAndSecurityCore => ZBool.False;

		public ZString BoxS32SpecificCircumstanceIndicator => BoxS32SpecificCircumstanceIndicatorCore;
		protected virtual ZString BoxS32SpecificCircumstanceIndicatorCore => ZString.Empty;

		const string ExchangeRateDecimalFormat = "F6";
	}
}
