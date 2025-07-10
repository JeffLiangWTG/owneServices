using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class ExportEntryHeaderWrapper : NonPersistentBusinessObject,
		IVisualizerNoteSupporter
	{
		public ExportEntryHeaderWrapper(ZGuid pk, ExportEntryHeader header, BusinessObjectFactory factory)
			: base(factory)
		{
			Header = header;
			EntryPK = pk;
		}
		public IExportEntryHeader Header { get; }

		public ExportEntryLineCollectionWrapper EntryLineItems
		{
			get
			{
				if (entryLineItems == null)
				{
					entryLineItems = new ExportEntryLineCollectionWrapper(Header.EntryLines, Header.ExchangeRate, base.Factory);
				}
				return entryLineItems;
			}
		}
		ExportEntryLineCollectionWrapper entryLineItems;

		public ZString EntryLinesCount => EntryLineItems.Count.ToString("D3");
		ZGuid EntryPK { get; }

		#region IVisualizerNoteSupporter members
		ZGuid IVisualizerNoteSupporter.PK => EntryPK;
		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;
		string IVisualizerNoteSupporter.TableCode => CusEntryHeaderSchema.Constants.Prefix;
		#endregion

		#region SuppressResourceStringsCheckRegion
		public const string FrontPage = "갑지";
		public const string ContinuationPage = "을지";
		public const string DeclarationProcedureTypeMTitleOnRelease = "반송신고수리내역서";
		public const string ExportTypeCodeDTitleOnRelease = "자유무역지역 반출(국외반출)내역서";
		public const string GenericTitleOnRelease = "수출신고수리내역서";
		public const string BeforeLoadingTitle = "적재전";
		public const string ExportedTitle = "수출이행";
		public const string DeclarationProcedureTypeMTitleBeforeRelease = "반  송  신  고  서";
		public const string ExportTypeCodeDTitleBeforeRelease = "자유무역지역 반출(국외반출)신고서";
		public const string GenericTitleBeforeRelease = "수  출  신  고  서";

		public const string EnglishFrontPage = "A";
		public const string EnglishContinuationPage = "B";
		public const string EnglishTitle = "EXPORT DECLARATION CERTIFICATE";
		public const string EnglishBeforeLoadingTitle = "BEFORE LOADING";
		public const string EnglishExportedTitle = "AFTER EXPORTING";
		#endregion

		public ZString DocumentTitle => GetDocumentTitle(true);
		public ZString SubDocumentTitle => GetDocumentTitle(false);

		public ZString EnglishDocumentTitle => GetEnglishDocumentTitle(true);
		public ZString EnglishSubDocumentTitle => GetEnglishDocumentTitle(false);

		ZString GetDocumentTitle(ZBool isFrontPage)
		{
			var theFirstTitle = ZString.Empty;
			var wordingOnLoaded = ZString.Empty;
			var frontOrContinuationPage = ZString.Empty;

			if (Header.DeclarationProcedureType == ExportDeclarationTypeCodeList.Codes.M)
			{
				theFirstTitle = EntryReleaseDateTime.IsValid ? DeclarationProcedureTypeMTitleOnRelease : DeclarationProcedureTypeMTitleBeforeRelease;
			}
			else if (Header.ExportTypeCode == ExportTypeCodeList.Codes.D)
			{
				theFirstTitle = EntryReleaseDateTime.IsValid ? ExportTypeCodeDTitleOnRelease : ExportTypeCodeDTitleBeforeRelease;
			}
			else
			{
				theFirstTitle = EntryReleaseDateTime.IsValid ? GenericTitleOnRelease : GenericTitleBeforeRelease;
			}

			if (EntryReleaseDateTime.IsValid)
			{
				wordingOnLoaded = ActualLoadingDate.IsValid ? ExportedTitle : BeforeLoadingTitle;
			}

			frontOrContinuationPage = isFrontPage ? FrontPage : ContinuationPage;

			return GetTitle(theFirstTitle, wordingOnLoaded, frontOrContinuationPage);
		}

		ZString GetEnglishDocumentTitle(ZBool isFrontPage)
		{
			var theFirstTitle = EnglishTitle;
			var wordingOnLoaded = ZString.Empty;
			var frontOrContinuationPage = ZString.Empty;

			if (EntryReleaseDateTime.IsValid)
			{
				wordingOnLoaded = ActualLoadingDate.IsValid ? EnglishExportedTitle : EnglishBeforeLoadingTitle;
			}

			frontOrContinuationPage = isFrontPage ? EnglishFrontPage : EnglishContinuationPage;

			return GetTitle(theFirstTitle, wordingOnLoaded, frontOrContinuationPage);
		}

		static ZString GetTitle(ZString theFirstTitle, ZString wordingOnLoaded, ZString frontOrContinuationPage)
		{
			var result = new ZStringBuilder();
			result.Append(theFirstTitle);
			result.Append("(");
			if (!wordingOnLoaded.IsEmpty)
			{
				result.Append(wordingOnLoaded);
				result.Append(", ");
			}
			result.Append(frontOrContinuationPage);
			result.Append(")");
			return result.ToString();
		}

		public ZString MessageStatus { get; set; }

		public ZString FormattedEntryNumber => MessageFunctions.DeclarationNumberFormat(Header.ExportDeclarationNumber);

		public OrganizationDocWrapper Declarant => declarant ?? (declarant = new OrganizationDocWrapper(Header.Declarant));
		OrganizationDocWrapper declarant;

		public OrganizationDocWrapper Exporter => exporter ?? (exporter = new OrganizationDocWrapper(Header.Exporter));
		OrganizationDocWrapper exporter;

		public OrganizationDocWrapper Supplier => supplier ?? (supplier = new OrganizationDocWrapper(Header.Supplier));
		OrganizationDocWrapper supplier;

		public OrganizationDocWrapper Manufacturer => manufacturer ?? (manufacturer = new OrganizationDocWrapper(Header.Manufacturer));
		OrganizationDocWrapper manufacturer;

		public OrganizationDocWrapper Importer => importer ?? (importer = new OrganizationDocWrapper(Header.Importer));
		OrganizationDocWrapper importer;

		public Guid RegistryCompanyPK { get; set; }
		public ZString EnglishCompanyName => KRCustomsRegistry.Instance.CompanyName.GetValueWithFallbackDefault(RegistryCompanyPK, Guid.Empty, Guid.Empty).ToString();
		public ZString EnglishRepresentativeName => KRCustomsRegistry.Instance.RepresentativeName.GetValueWithFallbackDefault(RegistryCompanyPK, Guid.Empty, Guid.Empty).ToString();

		public ZDecimal TotalCustomsValueUSD
		{
			get
			{
				ZDecimal result = 0;
				if (CurrencyConverter != null)
				{
					var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
					var krwCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.KoreaRepublicOf);
					result = CurrencyConverter.ConvertExact(new Money(Header.TotalCustomsValue, krwCurrency), usdCurrency).Amount.Truncate(0);
				}
				return result;
			}
		}
		public ZString CountryOfDestinationShort => MessageFunctions.GetCountryKRCCode(Factory, Header.CountryOfDestination);
		public ZString DeclarationProcedureTypeShort => Factory.GetCachedValue<ExportDeclarationTypeCodeListShort>().GetDescriptionFromCode(Header.DeclarationProcedureType);
		public ZString TransactionTypeShort => Factory.GetCachedValue<ExportDealingTypeCodeListShort>().GetDescriptionFromCode(Header.TransactionType);
		public ZString ExportTypeCodeShort => Factory.GetCachedValue<ExportTypeCodeList>().GetDescriptionFromCode(Header.ExportTypeCode);
		public ZString InvoicePaymentTermShort => Factory.GetCachedValue<SettlementMethodCodeListShort>().GetDescriptionFromCode(Header.InvoicePaymentTerm);
		public ZString PortOfLoadingDescription => ExtensionMethods.GetLoadPortNameInKorean(Factory, Header.PortOfLoading);
		public ZString FormattedCargoManagementNo
		{
			get
			{
				ZString result = ZString.Empty;
				if (Header.CargoManagement.ImportCargoManagementNumber.Length == 19)
				{
					result = MessageFunctions.GetFormattedNumber(Header.CargoManagement.ImportCargoManagementNumber, new int[] { 0, 11, 15 });
				}
				else
				{
					result = MessageFunctions.GetFormattedNumber(Header.CargoManagement.ImportCargoManagementNumber, new int[] { 0, 11 });
				}
				return result;
			}
		}
		public ZString FirstContainerNumber
		{
			get
			{
				return Header.Containers?.OrderBy(x => x.SequenceNo).FirstOrDefault()?.ContainerNo ?? ZString.Empty;
			}
		}

		public ExportVehicleNoWrapperCollection VehicleItems
		{
			get
			{
				if (vehicleItems == null)
				{
					vehicleItems = new ExportVehicleNoWrapperCollection(Header.EntryLines);
				}
				return vehicleItems;
			}
		}
		ExportVehicleNoWrapperCollection vehicleItems;

		public ZDateTime DeclarationDate { get; set; }
		public ZDateTime EntryReleaseDateTime { get; set; }
		public ZString CustomsMessageRemarks { get; set; }
		public ZString ResponsibleCustomsOfficer { get; set; }
		public ZDateTime ExpectedLoadingDate { get; set; }
		public ZDateTime ActualLoadingDate { get; set; }
		public ZDecimal InvoiceCurrencyExchangeRate { get; set; }
		public CurrencyConverter CurrencyConverter { get; set; }
		public ZString CustomsOfficeName { get; set; }
	}
}
