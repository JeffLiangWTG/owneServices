using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects
{
	public class ATRCertificateDocDataObject : DocDataObject
	{
		public ATRCertificateDocDataObject(IATRCertificateOfOrigin certificateOfOrigin, BusinessObjectFactory factory) : base(factory)
		{
			CertificateOfOrigin = Argument.NotNull(certificateOfOrigin, nameof(certificateOfOrigin));
			Argument.NotNull(factory, nameof(factory));
			Declaration = Argument.NotNull(certificateOfOrigin.Declaration, nameof(certificateOfOrigin.Declaration));
			SetATRCertificateDefaultData();
			BuildCaptions();
		}

		public IATRCertificateDeclaration Declaration { get; }

		#region Exporter

		[MaxLength(NotificationTypes.Warning, (ATRCertificateOfOriginConstants.Length.MaxLinesExporterInDocument * ATRCertificateOfOriginConstants.Length.MaxLengthLineExporterInDocument))]
		[BusinessObjectMaxLengthTestExclude]
		public ZString Exporter
		{
			get
			{
				var getLimits = new TextLimitCalculator();
				return getLimits.CalculateLimits(exporter.ToUpperInvariant(), ATRCertificateOfOriginConstants.Length.MaxLinesExporterInDocument, ATRCertificateOfOriginConstants.Length.MaxLengthLineExporterInDocument);
			}
			set => SetNonPersistentPropertyValue(ExporterInfo, ref exporter, value);
		}

		ZString exporter;

		public ZPropertyInfo ExporterInfo => GetZPropertyInfo(nameof(Exporter));

		#endregion

		#region Importer

		public ZString Importer
		{
			get => importer.ToUpperInvariant();
			set => SetNonPersistentPropertyValue(ImporterInfo, ref importer, value);
		}

		ZString importer;

		public ZPropertyInfo ImporterInfo => GetZPropertyInfo(nameof(Importer));

		#endregion

		#region ExportationCountry

		[MaxLength(NotificationTypes.Warning, ATRCertificateOfOriginConstants.Length.MaxLengthCountryOfExportationInDocument)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString ExportationCountry
		{
			get => exportationCountry.Substring(0, ATRCertificateOfOriginConstants.Length.MaxLengthCountryOfExportationInDocument).ToUpperInvariant();
			set => SetNonPersistentPropertyValue(ExportationCountryInfo, ref exportationCountry, value);
		}

		ZString exportationCountry;

		public ZPropertyInfo ExportationCountryInfo => GetZPropertyInfo(nameof(ExportationCountry));

		#endregion

		#region DestinationCountry

		[MaxLength(NotificationTypes.Warning, ATRCertificateOfOriginConstants.Length.MaxLengthCountryOfDestinationInDocument)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString DestinationCountry
		{
			get => destinationCountry.Substring(0, ATRCertificateOfOriginConstants.Length.MaxLengthCountryOfDestinationInDocument).ToUpperInvariant();
			set => SetNonPersistentPropertyValue(ExportationCountryInfo, ref destinationCountry, value);
		}

		ZString destinationCountry;

		public ZPropertyInfo DeclaratiionCountryInfo => GetZPropertyInfo(nameof(DestinationCountry));

		#endregion

		#region Box12CustomsEndorsement

		public CustomsEndorsement Box12CustomsEndorsement => box12CustomsEndorsement ?? (box12CustomsEndorsement = GetNewBox12CustomsEndorsement());
		CustomsEndorsement box12CustomsEndorsement;

		#endregion

		#region Box13DeclarationExporter

		public DeclarationExporter Box13DeclarationExporter => box13DeclarationExporter ?? (box13DeclarationExporter = GetNewBox13DeclarationExporter());
		DeclarationExporter box13DeclarationExporter;

		#endregion

		#region ReferenceDateFormat

		public string ReferenceDateFormat => CertificateOfOrigin.ReferenceDateFormat;

		#endregion

		#region TransportDetails

		[MaxLength(NotificationTypes.Warning, ATRCertificateOfOriginConstants.Length.MaxLengthTransportDetailsInDocument)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString TransportDetails => (transportDetails ?? (transportDetails = CertificateOfOrigin.TransportDetail?.Voyage ?? ZString.Empty)).Value;

		ZString? transportDetails;

		#endregion

		#region Remarks

		public ZString Remarks
		{
			get => remarks;
			set => SetNonPersistentPropertyValue(RemarksInfo, ref remarks, value);
		}

		ZString remarks;
		public ZPropertyInfo RemarksInfo => GetZPropertyInfo(nameof(Remarks));

		#endregion

		#region Items

		[MaxLength(NotificationTypes.Warning, (ATRCertificateOfOriginConstants.Length.MaxLinesItemsInDocument * ATRCertificateOfOriginConstants.Length.MaxLengthLineItemsInDocument))]
		[BusinessObjectMaxLengthTestExclude]
		public virtual ZString Items
		{
			get
			{
				if (!items.HasValue)
				{
					CreateItemsList();
				}
				var getLimits = new TextLimitCalculator();
				return getLimits.CalculateLimits(items.Value, ATRCertificateOfOriginConstants.Length.MaxLinesItemsInDocument, ATRCertificateOfOriginConstants.Length.MaxLengthLineItemsInDocument);
			}
			set => SetNonPersistentPropertyValue(ItemsInfo, ref items, value);
		}

		ZString? items;

		public ZPropertyInfo ItemsInfo => GetZPropertyInfo(nameof(Items));

		#endregion

		#region MarksNumbers

		[MaxLength(NotificationTypes.Warning, (ATRCertificateOfOriginConstants.Length.MaxLinesMarksInDocument * ATRCertificateOfOriginConstants.Length.MaxLengthLineMarksInDocument))]
		[BusinessObjectMaxLengthTestExclude]
		public virtual ZString MarksNumbers
		{
			get
			{
				if (!marksNumbers.HasValue)
				{
					CreateItemsList();
				}
				if ((CertificateOfOrigin.ShouldAddTotalCertificateItem) && (!totalsMarksNumbers.HasValue))
				{
					totalsMarksNumbers = CertificateOfOrigin.TotalATRCertificateItem?.GoodsDescription ?? ZString.Empty;
					marksNumbers += totalsMarksNumbers;
				}
				var getLimits = new TextLimitCalculator();
				return getLimits.CalculateLimits(marksNumbers.Value, ATRCertificateOfOriginConstants.Length.MaxLinesMarksInDocument, ATRCertificateOfOriginConstants.Length.MaxLengthLineMarksInDocument);
			}
			set => SetNonPersistentPropertyValue(MarksNumbersInfo, ref marksNumbers, value);
		}

		ZString? marksNumbers;

		public ZPropertyInfo MarksNumbersInfo => GetZPropertyInfo(nameof(MarksNumbers));

		public virtual ZString TotalsMarksNumbers
		{
			get => totalsMarksNumbers ?? ZString.Empty;
			set => SetNonPersistentPropertyValue(TotalsMarksNumbersInfo, ref totalsMarksNumbers, value);
		}

		ZString? totalsMarksNumbers;

		public ZPropertyInfo TotalsMarksNumbersInfo => GetZPropertyInfo(nameof(TotalsMarksNumbers));

		#endregion

		#region GrossWeight

		[MaxLength(NotificationTypes.Warning, (ATRCertificateOfOriginConstants.Length.MaxLinesGrossWeightInDocument * ATRCertificateOfOriginConstants.Length.MaxLengthLineGrossWeightInDocument))]
		[BusinessObjectMaxLengthTestExclude]
		public virtual ZString GrossWeight
		{
			get
			{
				if (!grossWeight.HasValue)
				{
					CreateItemsList();
				}
				var getLimits = new TextLimitCalculator();
				return getLimits.CalculateLimits(grossWeight.Value, ATRCertificateOfOriginConstants.Length.MaxLinesGrossWeightInDocument, ATRCertificateOfOriginConstants.Length.MaxLengthLineGrossWeightInDocument);
			}
			set => SetNonPersistentPropertyValue(GrossWeightInfo, ref grossWeight, value);
		}

		ZString? grossWeight;

		public ZPropertyInfo GrossWeightInfo => GetZPropertyInfo(nameof(GrossWeight));

		#endregion

		#region Url

		public ZString Url
		{
			get => url;
			set => SetNonPersistentPropertyValue(UrlInfo, ref url, value);
		}

		ZString url;

		public ZPropertyInfo UrlInfo => GetZPropertyInfo(nameof(Url));

		#endregion

		void CreateItemsList()
		{
			var boxItemsBuilder = CertificateOfOrigin.ATRBoxItemBuilder;
			boxItemsBuilder.Build();

			items = boxItemsBuilder.ItemsInfoBox9;
			marksNumbers = boxItemsBuilder.MarksNumberBox10;
			grossWeight = boxItemsBuilder.GrossWeightBox11;
		}

		void BuildCaptions()
		{
			ARTMovementCertificateCaption = Res.GetString("E819ACB7-9779-49B4-9FF1-A8D7B61A8932", "Movement Certificate");
			ARTNumberCaption = CertificateOfOrigin.ARTNumberCaption;
			ARTExporterCaption = Res.GetString("F452F00E-5D9D-4AC7-AFCC-3A65D297D68A", "Exporter (Name, full address, country)");
			ARTTransportDocumentCaption = Res.GetString("86C3F597-3984-4A62-89FD-5A284D770923", "Transport Document (Optional)");
			ARTTransportDocumentNumberCaption = Res.GetString("DE176E23-85A8-49F6-8FAC-BACA33DCBE9A", "No.");
			ARTDateCaption = Res.GetString("AC73EAFC-4750-4430-9827-C3A661C2E475", "Date");
			ARTConsigneeCaption = Res.GetString("5659E956-FAEC-417D-A6C1-8F05EA320D58", "Consignee (Name, full address, country) (Optional)");
			ARTCountryOfExportationCaption = Res.GetString("400C66F0-4EDD-4364-9D70-BAC1DD02543E", "Country of exportation");
			ARTCountryOfDestinationCaption = Res.GetString("B7711774-C895-4A82-B04F-8E8B16E021FD", "Country of destination");
			ARTTransportDetailsCaption = Res.GetString("C798A379-66F7-4B0B-9DA2-11D6F79669CB", "Transport details (Optional)");
			ARTRemarksCaption = Res.GetString("8D1AD748-F459-49DF-931A-7474C8CFF120", "Remarks");
			ARTItemNoCaption = Res.GetString("6FA8CED4-4196-4E10-9C00-FA0DEAA4FAFA", "Item No.");
			ARTMarksOrNumbersCaption = Res.GetString("9F07041E-3F91-4D22-993A-9CA9E5AF9192", "Marks and numbers; Number and kind  of packages (for goods in bulk, indicate the name of the ship or the number of the railway wagon or road vehicle); Description of goods");
			ARTGrossWeightOrOtherMeasureCaption = Res.GetString("3934B1EA-B2C2-4B98-A570-66C0ECE016AA", "Gross weight (kg) or other measure (hl, m3, etc.)");
			ARTCustomsEndoresementCaption = Res.GetString("EC99D292-F6EE-4B62-9A91-3F6CA21A8AD0", "CUSTOMS ENDORSEMENT");
			ARTDeclarationCertifiedCaption = Res.GetString("3BA3E8D5-9C25-4660-AEE0-FC2085CF5405", "Declaration certified");
			ARTExportDocumentCaption = Res.GetString("9CF51010-5AC2-4BCA-989A-5B4CBE59EE40", "Movement Certificate");
			ARTFormCaption = Res.GetString("5CAED5A4-5D44-449E-9899-0D3F181A7D78", "Form");
			ARTOfCaption = Res.GetString("7D23D794-E003-4F81-A830-89A864381964", "Of");
			ARTCustomsOfficeCaption = Res.GetString("998A6441-9F2C-466D-9A0E-4C2F596CD98D", "Customs office");
			ARTIssuingCountryCaption = Res.GetString("5577E261-FCD6-463F-93CA-3E29EB93106C", "Issuing country");
			ARTPlaceCaption = Res.GetString("AA5FD8A1-997B-418E-BAE3-96ED7A589E43", "Place");
			ARTDeclarationByExporterCaption = Res.GetString("C1F02B96-28FB-450B-AC3D-E03AF53DCCA9", "DECLARATION BY EXPORTER");
			ARTSignatureCaption = Res.GetString("0077264E-8335-4D57-93B5-98F79D533838", "(Signature)");
			ARTStampCaption = Res.GetString("8936C146-7F38-4645-AE29-FD3F646D4848", "Stamp");
			ARTStatementCaption = Res.GetString("4D2F3A86-A7A7-4249-BADA-0841B0448462", "I, the undersigned, declare that the goods described above meet the conditions required for the issue of this certificate");
			ARTAssociationCaption = Res.GetString("C4DC8AB4-9B53-492D-A0D7-A58F757A8923", "ASSOCIATION");
			ARTBetweenTheCaption = Res.GetString("B3F1C221-32BD-4BFC-9B71-D6E672369687", "between the");
			ARTEuropeanUnionCaption = CertificateOfOrigin.ARTEuropeanUnionCaption;
			ARTAndCaption = Res.GetString("ECE1CB91-2744-4DB5-929E-A7F45A11A744", "and");
			ARTTurkeyCaption = Res.GetString("D0A55071-737D-4465-AB04-9857FEE65698", "TURKEY");
		}

		public ZString ARTMovementCertificateCaption { get; private set; }
		public ZString ARTNumberCaption { get; private set; }
		public ZString ARTExporterCaption { get; private set; }
		public ZString ARTTransportDocumentCaption { get; private set; }
		public ZString ARTTransportDocumentNumberCaption { get; private set; }
		public ZString ARTDateCaption { get; private set; }
		public ZString ARTConsigneeCaption { get; private set; }
		public ZString ARTCountryOfExportationCaption { get; private set; }
		public ZString ARTCountryOfDestinationCaption { get; private set; }
		public ZString ARTTransportDetailsCaption { get; private set; }
		public ZString ARTRemarksCaption { get; private set; }
		public ZString ARTItemNoCaption { get; private set; }
		public ZString ARTMarksOrNumbersCaption { get; private set; }
		public ZString ARTGrossWeightOrOtherMeasureCaption { get; private set; }
		public ZString ARTCustomsEndoresementCaption { get; private set; }
		public ZString ARTDeclarationCertifiedCaption { get; private set; }
		public ZString ARTExportDocumentCaption { get; private set; }
		public ZString ARTFormCaption { get; private set; }
		public ZString ARTOfCaption { get; private set; }
		public ZString ARTCustomsOfficeCaption { get; private set; }
		public ZString ARTIssuingCountryCaption { get; private set; }
		public ZString ARTPlaceCaption { get; private set; }
		public ZString ARTDeclarationByExporterCaption { get; private set; }
		public ZString ARTSignatureCaption { get; private set; }
		public ZString ARTStampCaption { get; private set; }
		public ZString ARTStatementCaption { get; private set; }
		public ZString ARTAssociationCaption { get; private set; }
		public ZString ARTBetweenTheCaption { get; private set; }
		public ZString ARTEuropeanUnionCaption { get; private set; }
		public ZString ARTAndCaption { get; private set; }
		public ZString ARTTurkeyCaption { get; private set; }

		protected IATRCertificateOfOrigin CertificateOfOrigin { get; }

		CustomsEndorsement GetNewBox12CustomsEndorsement()
		{
			var customsEndorsement = CertificateOfOrigin.CustomsEndorsement;
			var maxLengthInfo = new CustomsEndorsementMaxLength(ATRCertificateOfOriginConstants.Length.MaxLength12FormInDocument, ATRCertificateOfOriginConstants.Length.MaxLength12AATR1InDocument, ATRCertificateOfOriginConstants.Length.MaxLength12CustomsInDocument, ATRCertificateOfOriginConstants.Length.MaxLength12IssuingInDocument, ATRCertificateOfOriginConstants.Length.MaxLength12PlaceInDocument);
			return new CustomsEndorsement(customsEndorsement, Factory, maxLengthInfo);
		}

		DeclarationExporter GetNewBox13DeclarationExporter()
		{
			var exporterDeclaration = CertificateOfOrigin.ExporterDeclaration;
			var maxLengthInfo = new DeclarationExporterMaxLength(ATRCertificateOfOriginConstants.Length.MaxLength13PlaceInDocument, ATRCertificateOfOriginConstants.Length.MaxLength13SupplierDetailsInDocument);
			return new DeclarationExporter(exporterDeclaration, Factory, maxLengthInfo);
		}

		#region Implementation

		void SetATRCertificateDefaultData()
		{
			Exporter = Declaration.FullFormattedExporterAddress;
			Importer = Declaration.FullFormattedImporterDocumentaryAddress;
			ExportationCountry = Declaration.GoodsOrigin;
			DestinationCountry = Declaration.GoodsDestination;
			Url = CertificateOfOrigin.Url;
			Remarks = CertificateOfOrigin.Remarks;
		}

		#endregion
	}
}
