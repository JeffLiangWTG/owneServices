namespace Enterprise.DocumentEngine
{
	public enum PageStyles
	{
		Portrait = 0,
		Landscape = 1,
		Continuous = 2,
		LetterPortrait = 3,
		LetterLandscape = 4,
		LetterPortraitMatrix = 5,
		CustomsLetterPortrait = 6,
		Label = 7,
		Custom = 8
	}

	public enum DataSourceTypes
	{
		NormalData,
		EdwData
	}

	#region SuppressResourceStringsCheckRegion
	public class Constants
	{
		public static class CustomPaperSizesInInch
		{
			public static int Label4x6Width = 4;
			public static int Label4x6Height = 6;
		}

		public class AreaIdentifierTags
		{
			public const string Config = "#CONFIG";
			public const string ConfigurableSection = "#CONFIGURABLESECTION:";
			public const string EndOfReport = "#ENDOFREPORT";
		}

		public static class ConfigAreaParameters
		{
			public const string TemplateNameSignature = "NAME=";
			public const string HideColumnIfSignature = "HIDECOLUMNIF";
			public const string ColumnHeadingsSignature = "COLUMNHEADINGS";
			public const string VersionSignature = "VERSION=";
			public const string PageStyleSignature = "PAGESTYLE=";
			public const string ForcedLanguageSignature = "FORCEDLANGUAGE=";
			public const string PageWidthSignature = "PAGEWIDTH=";
			public const string PageHeightSignature = "PAGEHEIGHT=";
			public const string AutoPageHeightSignature = "AUTOPAGEHEIGHT";
			public const string AutoHeightModeSignature = "AUTOHEIGHTMODE=";
			public const string EmailSubjectSignature = "EMAILSUBJECT=";
			public const string DataContextSignature = "DATACONTEXT=";
			public const string TrailingFormFeedSignature = "TRAILINGFORMFEED=";
			public const string DataSourceSignature = "DATA:";
			public const string EDWDataSourceSignature = "EDWDATA:";
			public const string EDWOnlyDataSourceSignature = "EDWONLYDATA:";
			public const string SlowServers = "SLOWSERVERS=";
			public const string Title = "REPORTTITLE=";
			public const string DocumentCurrency = "DOCUMENTCURRENCY=";
			public const string DocumentCurrencyApplyExpression = "ApplyDocumentCurrency";
			public const string SheetNameOverride = "SHEETNAMEOVERRIDE=";
			public const string HideSheetIfSignature = "HIDESHEETIF=";
			public const string SuppressDraftWatermark = "SUPPRESSDRAFTWATERMARK";
			public const string ForceWebPublishSignature = "FORCEWEBPUBLISH";
			public const string SqlTimeoutSignature = "SQLTIMEOUT=";
			public const string SqlQueryHints = "SQLQUERYHINTS=";
			public const string SqlQueryHintsIgnoreRecompile = "SQLQUERYHINTSIGNORERECOMPILE";
			public const string DisableXLSXExport = "DISABLEXLSXEXPORT";
			public const string DisableCSVExport = "DISABLECSVEXPORT";
			public const string DisableFixedValueCacheSignature = "DISABLEFIXEDVALUECACHE=";
			public const string DisableTranslate = "DISABLETRANSLATE";
			public const string ContainsCustomisedSections = "ContainsCustomisedSections=";
			public const string RemoveFirstPageIfNoData = "REMOVEFIRSTPAGEIFNODATA";
			public const string TranslateLegacyDocument = "TRANSLATELEGACYDOCUMENT";
			public const string HideRowsInsteadOfRemove = "HIDEROWSINSTEADOFREMOVE";
		}

		public static class SectionBodyAreaParameters
		{
			public const string StartingRowToShow = "STARTINGROWTOSHOW";
			public const string NumberOfRowsToShow = "NUMBEROFROWSTOSHOW";
			public const string RowCountAMultipleof = "ROWCOUNTAMULTIPLEOF";
			public const string MaximumNumberOfRowsToShow = "MAXIMUMNUMBEROFROWSTOSHOW";
			public const string FilterDataSource = "FILTERBY";
			public const string SortDataSource = "ORDERBY";
			public const string CustomSorting = "CUSTOMSORTING";
		}

		public static class SectionBodySortByDirections
		{
			public const string Ascending = "ASC";
			public const string Descending = "DESC";
		}

		public static class DocumentFooterParameters
		{
			public const string DontSplitSignature = "Don't Split";
		}

		public static class CommonAreaParameters
		{
			/// <summary>
			/// Used in SectionBodyArea, SectionHeaderArea, SectionPageHeaderArea, and GroupByArea.
			/// </summary>
			public const string Sticky = "STICKY";

			/// <summary>
			/// Used in SectionHeaderArea and SectionFooterArea.
			/// </summary>
			public const string ShowEvenWithNoDataSignature = "ShowEvenWithNoData";

			/// <summary>
			/// Used in SectionHeaderArea and GroupByArea.
			/// </summary>
			public const string PageBreakSignature = "PageBreak";
		}

		public static class ConditionalTags
		{
			public const string If = "#IF";
			public const string EndIf = "#ENDIF";
			public const string Else = "#ELSE";
		}

		public static class AddressTags
		{
			public const string LeftHandAddress = "#LeftHandAddress";
			public const string RightHandAddress = "#RightHandAddress";
			public const string EndAddress = "#EndAddress";
		}

		public static class SectionForeachTags
		{
			public const string BeginForeach = "#BeginLoop";
			public const string EndForeach = "#EndLoop";
		}

		/// <summary>
		/// Constants used to map transport mode/document names to Opening and Closing texts in the registry.
		/// MUST be in upper case.
		/// </summary>
		public static class DocumentNames
		{
			public const string CarrierBookingRequest = "CARRIER BOOKING REQUEST";
			public const string CollectionDeliveryNote = "COLLECTION - DELIVERY NOTE";
			public const string TransportDeliveryNote = "TRANSPORT DELIVERY NOTE";
			public const string AirPreAlert = "AIR PRE-ALERT";
			public const string AirArrivalNotice = "AIR ARRIVAL NOTICE";
			public const string AirDeliveryOrder = "AIR DELIVERY ORDER";
			public const string AirDeliveryDocket = "AIR DELIVERY DOCKET";
			public const string AirUltimateConsigneePreAlert = "AIR ULTIMATE CONSIGNEE PRE-ALERT";
			public const string AirUltimateConsigneeArrivalNotice = "AIR ULTIMATE CONSIGNEE ARRIVAL NOTICE";
			public const string AirAgentsInstruction = "AIR AGENTS INSTRUCTION";
			public const string AirShippingAdvice = "AIR SHIPPING ADVICE";
			public const string AirOutturnReport = "AIR OUTTURN REPORT";
			public const string AirAgentDepartureNotice = "AIR AGENT DEPARTURE NOTICE";
			public const string AirShipperDepartureNotice = "AIR SHIPPER DEPARTURE NOTICE";
			public const string AirLetterToOverseasAgent = "AIR LETTER TO OVERSEAS AGENT";
			public const string AirBookingConfirmation = "AIR BOOKING CONFIRMATION";
			public const string ExportOrderAdvice = "EXPORT ORDER ADVICE";
			public const string ExportOrderNotification = "EXPORT ORDER NOTIFICATION";
			public const string ExportOrderStatus = "EXPORT ORDER STATUS";
			public const string ImportOrderAdvice = "IMPORT ORDER ADVICE";
			public const string OrderShippedOnBoardAdvice = "SHIPPED ON BOARD ADVICE";
			public const string OrderAmendmentToBooking = "AMENDMENT TO BOOKING";
			public const string ImportOrderNotification = "IMPORT ORDER NOTIFICATION";
			public const string ImportOrderStatus = "IMPORT ORDER STATUS";
			public const string SeaPreAlert = "SEA PRE-ALERT";
			public const string SeaArrivalNotice = "SEA ARRIVAL NOTICE";
			public const string SeaShippingAdvice = "SEA SHIPPING ADVICE";
			public const string SeaDeliveryOrder = "SEA DELIVERY ORDER";
			public const string SeaDeliveryDocket = "SEA DELIVERY DOCKET";
			public const string SeaUltimateConsigneePreAlert = "SEA ULTIMATE CONSIGNEE PRE-ALERT";
			public const string SeaUltimateConsigneeArrivalNotice = "SEA ULTIMATE CONSIGNEE ARRIVAL NOTICE";
			public const string SeaAgentsInstruction = "SEA AGENTS INSTRUCTION";
			public const string SeaOutturnReport = "SEA OUTTURN REPORT";
			public const string SeaAgentDepartureNotice = "SEA AGENT DEPARTURE NOTICE";
			public const string SeaShipperDepartureNotice = "SEA SHIPPER DEPARTURE NOTICE";
			public const string SeaLetterToOverseasAgent = "SEA LETTER TO OVERSEAS AGENT";
			public const string SeaBookingConfirmation = "SEA BOOKING CONFIRMATION";
			public const string ShipmentCartageAdvice = "SHIPMENT CARTAGE ADVICE";
			public const string ShipmentCartageAdviceWithReceipt = "SHIPMENT CARTAGE ADVICE WITH RECEIPT";
			public const string CFSCartageAdvice = "CFS CARTAGE ADVICE";
			public const string CFSCartageAdviceWithReceipt = "CFS CARTAGE ADVICE WITH RECEIPT";
			public const string CFSShipmentCartageAdvice = "CFSSHIPMENT CARTAGE ADVICE";
			public const string CFSShipmentCartageAdviceWithReceipt = "CFSSHIPMENT CARTAGE ADVICE WITH RECEIPT";
			public const string ShipmentCartageAdviceWithRouting = "SHIPMENT CARTAGE ADVICE WITH ROUTING";
			public const string ShipmentCartageAdviceWithRoutingWithReceipt = "SHIPMENT CARTAGE ADVICE WITH ROUTING WITH RECEIPT";
			public const string ContainerLegCartageAdvice = "CONTAINERLEG CARTAGE ADVICE";
			public const string ContainerLegCartageAdviceWithReceipt = "CONTAINERLEG CARTAGE ADVICE WITH RECEIPT";
			public const string BookingCartageAdvice = "BOOKING CARTAGE ADVICE";
			public const string BookingCartageAdviceWithReceipt = "BOOKING CARTAGE ADVICE WITH RECEIPT";
			public const string ConsolCartageAdvice = "CONSOL CARTAGE ADVICE";
			public const string ConsolCartageAdviceWithReceipt = "CONSOL CARTAGE ADVICE WITH RECEIPT";
			public const string CustomsDeclarationCartageAdvice = "DECLARATION CARTAGE ADVICE";
			public const string CustomsDeclarationCartageAdviceWithReceipt = "DECLARATION CARTAGE ADVICE WITH RECEIPT";
			public const string CustomsDeclarationMultiContainerCartageAdvice = "DECLARATION MULTI-CONTAINER CARTAGE ADVICE";
			public const string CustomsDeclarationMultiContainerCartageAdviceWithReceipt = "DECLARATION MULTI-CONTAINER CARTAGE ADVICE WITH RECEIPT";
			public const string AgencyCartageAdvice = "AGENCY CARTAGE ADVICE";
			public const string AgencyCartageAdviceWithReceipt = "AGENCY CARTAGE ADVICE WITH RECEIPT";
			public const string LocalCartageAdvice = "LOCAL CARTAGE ADVICE";
			public const string LocalCartageAdviceWithReceipt = "LOCAL CARTAGE ADVICE WITH RECEIPT";
			public const string LocalMultiContainerCartageAdvice = "LOCAL MULTI-CONTAINER CARTAGE ADVICE";
			public const string ConfirmationCartageAdvice = "CONFIRMATION CARTAGE ADVICE";
			public const string ConfirmationCartageAdviceWithReceipt = "CONFIRMATION CARTAGE ADVICE WITH RECEIPT";
			public const string CartageLegCartageAdvice = "CARTAGE LEG CARTAGE ADVICE";
			public const string RequestForMissingDocuments = "REQUEST FOR MISSING DOCUMENTS";
			public const string EFTRequest = "EFT REQUEST";
			public const string DeliveryInformation = "DELIVERY INFORMATION";
			public const string SeaCargoWeightsAndMeasurementsReport = "SEA CARGO WEIGHT AND MEASUREMENT REPORT";
			public const string AirCargoWeightsAndMeasurementsReport = "AIR CARGO WEIGHT AND MEASUREMENT REPORT";
			public const string RequestForCollectCharges = "REQUEST FOR COLLECT CHARGES";
			public const string ShippingOrder = "SHIPPING ORDER";
			public const string CartageAdviceTimeSlotRequest = "TIME SLOT REQUEST";
			public const string CartageAdviceTimeSlotConfirmation = "TIME SLOT CONFIRMATION";
			public const string ContainerDetentionReminder = "CONTAINER DETENTION REMINDER";
			public const string DelayAlert = "DELAY ALERT";
			public const string LetterOfIndemnity = "LETTER OF INDEMNITY";
			public const string ContainerRelease = "CONTAINER RELEASE";
			public const string ContainerReleaseAuthorization = "CONTAINER RELEASE AUTHORIZATION";
			public const string DetentionAdvice = "DETENTION ADVICE";
			public const string AgencyBookingConfirmation = "AGENCY BOOKING CONFIRMATION";
			public const string IMODangerousGoodsDeclaration = "IMO DANGEROUS GOODS DECLARATION";
			public const string DangerousPackingCertificate = "DANGEROUS PACKING CERTIFICATE";
			public const string AgencyShipmentArrivalNotice = "AGENCYSHIPMENT ARRIVAL NOTICE";
			public const string ExportCertification = "AUSFUHRBESCHEINIGUNG (EXPORT CERT.)";
			public const string FOBAndienung = "FOB - ANDIENUNG";
			public const string Versicherungsanmeldung = "VERSICHERUNGSANMELDUNG";
			public const string RequestForService = "REQUEST FOR SERVICE";
			public const string AuthorisationForService = "AUTHORIZATION FOR SERVICE";
			public const string QuotationAcceptance = "ACCEPTANCE PAGE";
			public const string OneOffPricingPage = "ONE OFF PRICING PAGE";
			public const string OneOffPricingPageMultipleCarriers = "ONE OFF PRICING PAGE (MULTIPLE CARRIERS)";
			public const string CartageAdvice = "CARTAGE ADVICE";
			public const string CartageAdviceWithReceipt = "CARTAGE ADVICE WITH RECEIPT";
			public const string ConsignmentRequestForService = "CONSIGNMENT REQUEST FOR SERVICE";
			public const string ConsignmentAuthorizationForService = "CONSIGNMENT AUTHORIZATION FOR SERVICE";
			public const string RoutingOrder = "ROUTING ORDER";
		}
	}
	#endregion
}
