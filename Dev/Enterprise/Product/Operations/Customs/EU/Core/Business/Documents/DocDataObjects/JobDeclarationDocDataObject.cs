using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects
{
	public class JobDeclarationDocDataObject : DocDataObject
	{
		public JobDeclarationDocDataObject(IEURCertificateOfOrigin certificateOfOrigin)
		{
			this.certificateOfOrigin = Argument.NotNull(certificateOfOrigin, nameof(certificateOfOrigin));
			BuildCaptions();
			SetCumulationAppliedDefault();
		}

		readonly IEURCertificateOfOrigin certificateOfOrigin;

		public ZString DeclarationReference => certificateOfOrigin.DeclarationReference;

		public ZString Url => certificateOfOrigin.Url;

		public ZString Remarks => certificateOfOrigin.Remarks;

		public string ReferenceDateFormat => certificateOfOrigin.ReferenceDateFormat;

		public string EntryNumber => EUR1Pg1Box11CustomsEndorsement.EntryNumber;

		public string EUR1Pg1Box11TextEntryNumber => EUR1Pg1Box11CustomsEndorsement.EUR1Pg1Box11TextEntryNumber;

		public bool ShowEntryNumber => EUR1Pg1Box11CustomsEndorsement.ShowEntryNumber;

		#region CumulationApplied

		public ZBool CumulationApplied
		{
			get => cumulationApplied;
			set => SetNonPersistentPropertyValue(CumulationAppliedInfo, ref cumulationApplied, value);
		}

		ZBool cumulationApplied;

		public ZPropertyInfo CumulationAppliedInfo => GetZPropertyInfo(nameof(CumulationApplied));

		void SetCumulationAppliedDefault()
		{
			CumulationApplied = !certificateOfOrigin.Remarks.IsEmpty;
		}

		#endregion

		#region Exporter

		[MaxLength(NotificationTypes.Warning, (Constants.MaxLinesExporterInDocument * Constants.MaxLengthLineExporterInDocument))]
		[BusinessObjectMaxLengthTestExclude]
		public ZString Exporter
		{
			get
			{
				var getLimits = new TextLimitCalculator();
				return (exporter ?? (exporter = getLimits.CalculateLimits(certificateOfOrigin.SupplierAddress, Constants.MaxLinesExporterInDocument, Constants.MaxLengthLineExporterInDocument))).Value;
			}
			set => SetNonPersistentPropertyValue(ExporterInfo, ref exporter, value);
		}

		ZString? exporter;

		public ZPropertyInfo ExporterInfo => GetZPropertyInfo(nameof(Exporter));

		#endregion

		#region Importer

		[MaxLength(NotificationTypes.Warning, (Constants.MaxLinesImporterInDocument * Constants.MaxLengthLineImporterInDocument))]
		[BusinessObjectMaxLengthTestExclude]
		public ZString Importer
		{
			get
			{
				var getLimits = new TextLimitCalculator();
				return (importer ?? (importer = getLimits.CalculateLimits(certificateOfOrigin.ImporterAddress, Constants.MaxLinesImporterInDocument, Constants.MaxLengthLineImporterInDocument))).Value;
			}
			set => SetNonPersistentPropertyValue(ImporterInfo, ref importer, value);
		}

		ZString? importer;

		public ZPropertyInfo ImporterInfo => GetZPropertyInfo(nameof(Importer));

		#endregion

		#region OriginCountry

		[MaxLength(NotificationTypes.Warning, Constants.MaxLengthOriginCountryInDocument)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString OriginCountry
		{
			get => (originCountry ?? (originCountry = certificateOfOrigin.OriginCountry)).Value.Substring(0, Constants.MaxLengthOriginCountryInDocument);
			set => SetNonPersistentPropertyValue(OriginCountryInfo, ref originCountry, value);
		}

		ZString? originCountry;

		public ZPropertyInfo OriginCountryInfo => GetZPropertyInfo(nameof(OriginCountry));

		#endregion

		#region OriginGroup

		[MaxLength(NotificationTypes.Warning, Constants.MaxLengthOriginGroupInDocument)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString OriginGroup
		{
			get => (originGroup ?? (originGroup = certificateOfOrigin.OriginGroup)).Value.Substring(0, Constants.MaxLengthOriginGroupInDocument);
			set => SetNonPersistentPropertyValue(OriginGroupInfo, ref originGroup, value);
		}

		ZString? originGroup;

		public ZPropertyInfo OriginGroupInfo => GetZPropertyInfo(nameof(OriginGroup));

		#endregion

		#region DestinationCountry

		[MaxLength(NotificationTypes.Warning, Constants.MaxLengthDestinationCountryInDocument)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString DestinationCountry
		{
			get => (destinationCountry ?? (destinationCountry = certificateOfOrigin.DestinationCountry)).Value.Substring(0, Constants.MaxLengthDestinationCountryInDocument);
			set => SetNonPersistentPropertyValue(DestinationCountryInfo, ref destinationCountry, value);
		}

		ZString? destinationCountry;

		public ZPropertyInfo DestinationCountryInfo => GetZPropertyInfo(nameof(DestinationCountry));

		#endregion

		#region DestinationGroup

		[MaxLength(NotificationTypes.Warning, Constants.MaxLengthDestinationGroupInDocument)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString DestinationGroup
		{
			get => (destinationGroup ?? (destinationGroup = certificateOfOrigin.DestinationGroup)).Value.Substring(0, Constants.MaxLengthDestinationGroupInDocument);
			set => SetNonPersistentPropertyValue(DestinationGroupInfo, ref destinationGroup, value);
		}

		ZString? destinationGroup;

		public ZPropertyInfo DestinationGroupInfo => GetZPropertyInfo(nameof(DestinationGroup));

		#endregion

		#region VoyageFlight

		[MaxLength(NotificationTypes.Warning, Constants.MaxLengthVoyageFlightInDocument)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString VoyageFlight
		{
			get => (voyageFlight ?? (voyageFlight = certificateOfOrigin.TransportDetail.Voyage).Value).Substring(0, Constants.MaxLengthVoyageFlightInDocument);
			set => SetNonPersistentPropertyValue(VoyageFlightInfo, ref voyageFlight, value);
		}

		ZString? voyageFlight;

		public ZPropertyInfo VoyageFlightInfo => GetZPropertyInfo(nameof(VoyageFlight));

		#endregion

		#region Items

		[MaxLength(NotificationTypes.Warning, (Constants.MaxLinesItemsInDocument * Constants.MaxLengthLineItemsInDocument))]
		[BusinessObjectMaxLengthTestExclude]
		public ZString Items
		{
			get
			{
				if (!items.HasValue)
				{
					items = certificateOfOrigin.GoodsSummary.Description;
				}
				var getLimits = new TextLimitCalculator();
				return getLimits.CalculateLimits(items.Value, Constants.MaxLinesItemsInDocument, Constants.MaxLengthLineItemsInDocument);
			}
			set => SetNonPersistentPropertyValue(ItemsInfo, ref items, value);
		}

		ZString? items;

		public ZPropertyInfo ItemsInfo => GetZPropertyInfo(nameof(Items));

		#endregion

		#region ItemMassVolume

		[MaxLength(NotificationTypes.Warning, (Constants.MaxLinesItemsInDocument * Constants.MaxLengthLineGrossMassInDocument))]
		[BusinessObjectMaxLengthTestExclude]
		public ZString ItemMassVolume
		{
			get
			{
				if (!itemMassVolume.HasValue)
				{
					itemMassVolume = certificateOfOrigin.GoodsSummary.WeightAndVolume;
				}
				var getLimits = new TextLimitCalculator();
				return getLimits.CalculateLimits(itemMassVolume.Value, Constants.MaxLinesItemsInDocument, Constants.MaxLengthLineGrossMassInDocument);
			}
			set => SetNonPersistentPropertyValue(ItemMassVolumeInfo, ref itemMassVolume, value);
		}

		ZString? itemMassVolume;

		public ZPropertyInfo ItemMassVolumeInfo => GetZPropertyInfo(nameof(ItemMassVolume));

		#endregion

		#region InvoiceNumbers

		[MaxLength(NotificationTypes.Warning, Constants.MaxLinesItemsInDocument * Constants.MaxLengthLineInvoiceInDocument)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString InvoiceNumbers
		{
			get
			{
				if (!invoiceNumbers.HasValue)
				{
					invoiceNumbers = certificateOfOrigin.GoodsSummary.InvoiceNumbers;
				}
				var getLimits = new TextLimitCalculator();
				return getLimits.CalculateLimits(invoiceNumbers.Value, Constants.MaxLinesItemsInDocument, Constants.MaxLengthLineInvoiceInDocument);
			}
			set => SetNonPersistentPropertyValue(InvoiceNumbersInfo, ref invoiceNumbers, value);
		}

		ZString? invoiceNumbers;

		public ZPropertyInfo InvoiceNumbersInfo => GetZPropertyInfo(nameof(InvoiceNumbers));

		#endregion

		#region Customs Endorsement

		public CustomsEndorsement EUR1Pg1Box11CustomsEndorsement => eur1Pg1Box11CustomsEndorsement ?? (eur1Pg1Box11CustomsEndorsement = GetEUR1Pg1Box11CustomsEndorsement());
		CustomsEndorsement eur1Pg1Box11CustomsEndorsement;

		CustomsEndorsement GetEUR1Pg1Box11CustomsEndorsement()
		{
			var customsEndorsementDataProvider = certificateOfOrigin.CustomsEndorsement;
			var maxLengthInfo = new CustomsEndorsementMaxLength(Constants.MaxLength11FormInDocument, Constants.MaxLength11NumberInDocument, Constants.MaxLength11CustomsInDocument, Constants.MaxLength11IssuingInDocument, Constants.MaxLength11PlaceInDocument);
			return new CustomsEndorsement(customsEndorsementDataProvider, Factory, maxLengthInfo);
		}

		#endregion

		#region Declaration Exporter

		public DeclarationExporter EUR1Pg1Box12DeclarationExporter => eur1Pg1Box12DeclarationExporter ?? (eur1Pg1Box12DeclarationExporter = GetEUR1Pg1Box12DeclarationExporter());
		DeclarationExporter eur1Pg1Box12DeclarationExporter;

		DeclarationExporter GetEUR1Pg1Box12DeclarationExporter()
		{
			var exporterDeclaration = certificateOfOrigin.ExporterDeclaration;
			var maxLengthInfo = new DeclarationExporterMaxLength(Constants.MaxLength12PlaceInDocument, Constants.MaxLength12SupplierDetailsInDocument);
			return new DeclarationExporter(exporterDeclaration, Factory, maxLengthInfo);
		}

		#endregion

		#region EUR1 Captions & logo

		void BuildCaptions()
		{
			EUR1Pg1DocumentTitle = Res.GetString("B376EB9E-A2E1-457F-BD2D-C00F700959DF", "MOVEMENT CERTIFICATE");
			EUR1Pg3DocumentTitle = Res.GetString("D6DDFD33-192C-4ACD-88A5-CEE6E1C4EFEF", "APPLICATION FOR A MOVEMENT CERTIFICATE");
			EUR1Pg1NotesCaption = Res.GetString("BF81C66A-1F0E-4ED4-B912-65890CBFD0A2", "See notes overleaf before completing this form");
			EUR1Pg1Box1Caption = Res.GetString("6217E7E0-485A-4F9D-A70F-6C70444AB71E", "Exporter");
			EUR1Pg1Box1CaptionHint = Res.GetString("584840A1-F17B-40F5-AB37-446A3D9D2FC5", "(Name, full address, country)");
			EUR1Pg1Box2CaptionA = Res.GetString("7E0D5923-0D3E-4F40-972D-36A5A9830427", "Certificate used in preferential trade between");
			EUR1Pg1Box2CaptionB = Res.GetString("47487833-9122-4C1B-AE51-683E11B04D32", "and");
			EUR1Pg1Box2CaptionHint = Res.GetString("D37BCB62-EDA0-4775-BD40-F8E1152BECA5", "(insert appropriate countries, groups of countries or territories)");
			EUR1Pg1Box3Caption = Res.GetString("54D14288-F6EF-4900-80F0-8F4C723955AF", "Consignee");
			EUR1Pg1Box3CaptionHint = Res.GetString("25351D99-6898-4EEC-8762-731F864D7636", "(Name, full address country) (Optional)");
			EUR1Pg1Box4Caption = Res.GetString("0B5ECFAD-4485-4D0E-90AF-585594FE15C2", "4. Country, group of countries or territory in which the products are considered as originating");
			EUR1Pg1Box5Caption = Res.GetString("0604C09B-0085-4368-BB4D-9E05AD8ABF4C", "5. Country, group of countries or territory of destination");
			EUR1Pg1Box6Caption = Res.GetString("CEF6851D-E346-464C-897E-0F289DF35A7F", "Transport details");
			EUR1Pg1Box6CaptionHint = Res.GetString("D015A840-C935-49F7-889D-66B6BB23C74B", "(Optional)");
			EUR1Pg1Box7Caption = Res.GetString("DCCD2494-5570-4312-9E40-F0A4538AE363", "Remarks");
			EUR1Pg1Box8Caption = Res.GetString("B28BE41A-21B0-46F1-A83C-BFE25D763E16", "Item number; Marks and numbers; Number and kind of packages; Description of goods");
			EUR1Pg1Box9Caption = Res.GetString("Form|EUR1Certificate|88B9DB00-1B93-46F0-AEED-E40E39938EF0", "Gross mass (kg) or other measure (litres, m3, etc.)");
			EUR1Pg1Box10Caption = Res.GetString("6A2DB538-AC28-4FF9-B5D3-CC9D0711CCA5", "Invoices");
			EUR1Pg1Box11Caption = Res.GetString("95381B24-ECE7-4CA7-8505-6D3ACD3E6EBE", "CUSTOMS ENDORSEMENT");
			EUR1Pg1Box11Text = Res.GetString("DF37233E-EC73-491F-8628-E4055EE5A370", "Declaration certified.\nExport document(2)");
			EUR1Pg1Box11TextCustomsOffice = Res.GetString("4720DCF8-BE59-454A-974D-69C98459E84B", "Customs Office");
			EUR1Pg1Box11TextIssuingCountry = Res.GetString("A9F9D7F2-7B30-4A55-BFC3-B67D23488090", "Issuing country or territory");
			EUR1Pg1Box12Caption = Res.GetString("1AC68BCC-0ADB-4051-88B4-0C3AB1229506", "DECLARATION BY THE EXPORTER");
			EUR1Pg1Box12Text = Res.GetString("FDC9F993-94A8-41B9-9C9C-565B2A3404B9", "I, the undersigned, declare that the goods described above meet the conditions required for the issue of this certificate");
			EUR1Pg4Caption = Res.GetString("F7D4F8AE-D26A-419B-88DF-CD65B11802A1", "DECLARATION BY THE EXPORTER");
			EUR1Pg4TextDeclarationCaption = Res.GetString("4A7A82E4-886C-49B2-A388-3D3196C66B69", "I, the undersigned, exporter of the goods described overleaf,");
			EUR1Pg4TextDeclare = Res.GetString("86213FF7-9A54-4343-9225-564F4EB9EE3B", "DECLARE  that the goods meet the conditions required for the issue of the attached certificate;");
			EUR1Pg4TextSpecify = Res.GetString("CD936CE4-F904-4D93-99B9-27C1901FB9F0", "SPECIFY  as follows the circumstances which have enabled these goods to meet the above conditions:");
			EUR1Pg4TextSubmit = Res.GetString("0E3BD309-FED7-47C4-B8F6-336250C62422", "SPECIFY  the following supporting documents (1):");
			EUR1Pg4TextUndertake = Res.GetString("BF28830F-DE3F-4A07-B20E-2DF4DED996CF", "UNDERTAKE  to submit, at the request of the appropriate authorities, any supporting evidence which these authorities may require for the purpose of issuing the attached certificate, and undertake, if required, to agree to any inspection of my accounts and to any check on the processes of manufacture of the above goods, carried out by the said authorities;");
			EUR1Pg4TextRequest = Res.GetString("C2F703D1-EDA0-43D8-B831-AC3AABDE0553", "REQUEST  the issue of the attached certificate for these goods.");
			EUR1TextForm = Res.GetString("13EDE3C0-423C-4225-804F-A4A1F6B87924", "Form");
			EUR1TextNumberAbrev = Res.GetString("8F46E36C-3E60-43A6-8798-6D9503510147", "No");
			EUR1TextOf = Res.GetString("D4E1B979-199D-4537-AF5C-48C85A1F0C25", "Of");
			EUR1TextPlaceAbrev = Res.GetString("3CAC4F31-8D9F-4D41-B026-B56B691FC03C", "P.");
			EUR1TextDateAbrev = Res.GetString("1C93CDB7-8707-4550-8D69-90A0BB576AEC", "D.");
			EUR1TextSignature = Res.GetString("FB978530-B2C3-48E2-9927-BD8D8E494EE8", "(Signature)");
			EUR1TextStamp = Res.GetString("8A9AB0E5-95D2-46FA-B1BF-6B16CC941657", "Stamp");
		}

		public ZString EUR1Pg1DocumentTitle { get; private set; }
		public ZString EUR1Pg3DocumentTitle { get; private set; }
		public ZString EUR1Pg1NotesCaption { get; private set; }
		public ZString EUR1Pg1Box1Caption { get; private set; }
		public ZString EUR1Pg1Box1CaptionHint { get; private set; }
		public ZString EUR1Pg1Box2CaptionA { get; private set; }
		public ZString EUR1Pg1Box2CaptionB { get; private set; }
		public ZString EUR1Pg1Box2CaptionHint { get; private set; }
		public ZString EUR1Pg1Box3Caption { get; private set; }
		public ZString EUR1Pg1Box3CaptionHint { get; private set; }
		public ZString EUR1Pg1Box4Caption { get; private set; }
		public ZString EUR1Pg1Box5Caption { get; private set; }
		public ZString EUR1Pg1Box6Caption { get; private set; }
		public ZString EUR1Pg1Box6CaptionHint { get; private set; }
		public ZString EUR1Pg1Box7Caption { get; private set; }
		public ZString EUR1Pg1Box8Caption { get; private set; }
		public ZString EUR1Pg1Box9Caption { get; private set; }
		public ZString EUR1Pg1Box10Caption { get; private set; }
		public ZString EUR1Pg1Box11Caption { get; private set; }
		public ZString EUR1Pg1Box11Text { get; private set; }
		public ZString EUR1Pg1Box11TextCustomsOffice { get; private set; }
		public ZString EUR1Pg1Box11TextIssuingCountry { get; private set; }
		public ZString EUR1Pg1Box12Caption { get; private set; }
		public ZString EUR1Pg1Box12Text { get; private set; }

		public ZString EUR1Pg4Caption { get; private set; }
		public ZString EUR1Pg4TextDeclarationCaption { get; private set; }
		public ZString EUR1Pg4TextDeclare { get; private set; }
		public ZString EUR1Pg4TextSpecify { get; private set; }
		public ZString EUR1Pg4TextSubmit { get; private set; }
		public ZString EUR1Pg4TextUndertake { get; private set; }
		public ZString EUR1Pg4TextRequest { get; private set; }

		public ZString EUR1TextForm { get; private set; }
		public ZString EUR1TextNumberAbrev { get; private set; }
		public ZString EUR1TextOf { get; private set; }
		public ZString EUR1TextPlaceAbrev { get; private set; }
		public ZString EUR1TextDateAbrev { get; private set; }
		public ZString EUR1TextSignature { get; private set; }
		public ZString EUR1TextStamp { get; private set; }

		#endregion

		#region EUR1 Lenght

		public int MaxLengthFormInDocument = Constants.MaxLength11FormInDocument;

		public static class Constants
		{
			public const int MaxLinesExporterInDocument = 4;
			public const int MaxLengthLineExporterInDocument = 27;
			public const int MaxLengthOriginCountryInDocument = 34;
			public const int MaxLengthDestinationCountryInDocument = 34;
			public const int MaxLinesImporterInDocument = 6;
			public const int MaxLengthLineImporterInDocument = 30;
			public const int MaxLengthOriginGroupInDocument = 19;
			public const int MaxLengthDestinationGroupInDocument = 19;
			public const int MaxLengthVoyageFlightInDocument = 30;
			public const int MaxLinesItemsInDocument = 31;
			public const int MaxLengthLineItemsInDocument = 80;
			public const int MaxLengthLineGrossMassInDocument = 8;
			public const int MaxLengthLineInvoiceInDocument = 20;
			public const int MaxLength11FormInDocument = 11;
			public const int MaxLength11NumberInDocument = 38;
			public const int MaxLength11CustomsInDocument = 49;
			public const int MaxLength11IssuingInDocument = 32;
			public const int MaxLength11PlaceInDocument = 20;
			public const int MaxLength12PlaceInDocument = 12;
			public const int MaxLength12SupplierDetailsInDocument = 66;
			public const string FieldForMaxLengthTest100 = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			public const string FieldForMaxLengthTest50 = "1234567890123456789012345678901234567890123456789";
		}

		#endregion
	}
}
