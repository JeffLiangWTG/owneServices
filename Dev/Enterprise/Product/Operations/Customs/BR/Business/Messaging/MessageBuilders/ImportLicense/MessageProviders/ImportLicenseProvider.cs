using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts;
using CargoWise.Customs.BR.MessageContracts.ImportLicense.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business.ImportLicense
{
	public class ImportLicenseProvider : IImportLicense
	{
		public ImportLicenseProvider(ImportLicenseMessageSendingObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
			Argument.NotNull(sendingObject.Header, "sendingObject.Header");
			Argument.NotNull(sendingObject.Header.Declaration, "sendingObject.Header.Declaration");
		}

		readonly ImportLicenseMessageSendingObject sendingObject;

		CusEntryHeader EntryHeader => sendingObject.Header;

		JobDeclaration Declaration => sendingObject.Header?.Declaration;

		JobComInvoiceLine InvoiceLine => EntryHeader.InvoiceLines?.FirstOrDefault() as JobComInvoiceLine;

		JobComInvoiceHeader InvoiceHeader => InvoiceLine?.InvoiceHeader;

		CusEntryInstruction EntryInstruction => EntryHeader.EntryInstruction;

		IEnumerable<JobComInvoiceLine> invoiceLines => invoiceLinesCached ?? (invoiceLinesCached = EntryHeader.InvoiceLines?.Cast<JobComInvoiceLine>()?.ToArray() ?? System.Array.Empty<JobComInvoiceLine>());
		JobComInvoiceLine[] invoiceLinesCached;

		public string UniqueBatchNumber => EDIMessage.UniqueBatchNumberPlaceHolder;

		public string ImportLicenseIdentification => EntryHeader.ImportLicenseIdentifier;

		public string CustomsOfficeEntranceCode => Declaration.EntranceOfficeCode;

		public string CustomsOfficeClearanceCode => Declaration.JE_CustomsOffice;

		public string TariffCode => InvoiceLine?.JI_Tariff ?? string.Empty;

		public string SubTariffCode => InvoiceLine?.NaladiHs ?? string.Empty;

		public string InvoiceCurrency => BRRefCusMapper.MapCW1CurrencyCodeToCustomsCode(Declaration.Factory, InvoiceHeader?.JZ_RX_NKInvoice_Currency ?? string.Empty);

		public string Incoterm => InvoiceHeader?.JZ_IncoTerm ?? string.Empty;

		public string ExchangeCover => InvoiceHeader?.ExchangeHedgeType ?? string.Empty;

		public string FinancialInstitution => InvoiceHeader?.ExchangeHedgeFinancialInstitution ?? string.Empty;

		public string Reason => InvoiceHeader?.ExchangeHedgeReason ?? string.Empty;

		public string PaymentMethod => InvoiceHeader?.ExchangeHedgePaymentMethod ?? string.Empty;

		public decimal PaymentDeadLine => InvoiceHeader?.ExchangeHedgePaymentDeadline ?? decimal.Zero;

		public string IsUsedMaterial => MessageBuilderHelper.MapBooleanValue(InvoiceLine != null && !InvoiceLine.JI_UsedMaterialRegime.IsEmpty);

		public string UsedMaterialRegime => InvoiceLine?.JI_UsedMaterialRegime ?? string.Empty;

		public string UsedMaterialOperationType => InvoiceLine?.JI_UsedMaterialOperationType ?? string.Empty;

		public string ManufacturerIndicator => InvoiceLine?.JI_ManufacturerIndicator ?? string.Empty;

		public string TaxRegime => InvoiceLine?.DutyTaxRegime ?? string.Empty;

		public string LegalBase => InvoiceLine?.DutyLegalBase ?? string.Empty;

		public string TariffAgreementType => !AladiAgreementCode.IsNullOrEmpty() ? "2" : string.Empty;

		public string AladiAgreementCode => (!InvoiceLine?.NaladiHs.IsEmpty ?? false) ? TariffAgreementCode?.GetAttribute(Constants.RefCusCodeList.Attributes.AgreementCodeInImportEntry) ?? string.Empty : string.Empty;

		ZZRefCusCodeListCombined TariffAgreementCode => fTariffAgreementCode ?? (fTariffAgreementCode = BRRefCusCodeListTypes.GetTariffAgreementCode(Declaration.Factory, InvoiceLine?.JI_SecondaryPreference ?? ZString.Empty, Declaration.DateOfValuation));
		ZZRefCusCodeListCombined fTariffAgreementCode;

		public string ImporterType
		{
			get
			{
				var primaryRegistration = Declaration.Importer?.PrimaryRegistrationNumber;
				switch (primaryRegistration?.NumberType)
				{
					case BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ:
						return "1";
					case BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration:
						return "2";
					default:
						return string.Empty;
				}
			}
		}

		public string ImporterID
		{
			get
			{
				var primaryRegistration = Declaration.Importer?.PrimaryRegistrationNumber;
				switch (primaryRegistration?.NumberType)
				{
					case BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ:
					case BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration:
						return primaryRegistration.Number.KeepNumericCharacters();
					default:
						return string.Empty;
				}
			}
		}

		public string CommodityOriginCountry => BRRefCusMapper.MapCW1CountryCodeToCustomsCode(Declaration.Factory, InvoiceLine?.JI_CountryOfOrigin ?? string.Empty);

		public string CargoProvenance => BRRefCusMapper.MapCW1CountryCodeToCustomsCode(Declaration.Factory, Declaration.JE_GoodsOrigin);

		public string DrawbackCode => InvoiceLine?.DrawbackModality ?? string.Empty;

		string DrawbackCANumber => InvoiceLine?.DrawbackCANumber ?? string.Empty;

		public string DrawbackCANumberSuspension => DrawbackModalityList.IsSuspension(DrawbackCode) ? DrawbackCANumber : string.Empty;

		public string DrawbackCANumberExemption => DrawbackModalityList.IsExemption(DrawbackCode) ? DrawbackCANumber : string.Empty;

		public string AdditionalInformation => EntryInstruction?.AdditionalInformation.SubstringSafe(0, CusEntryInstruction.Schema.ImportLicenseAdditionalInformationMaxLength) ?? string.Empty;

		public IOrganizationAddress SupplierAddress => fSupplierAddress ?? (fSupplierAddress = ImportLicenseAddressProvider.New(InvoiceHeader?.SupplierDocumentaryAddress));
		IOrganizationAddress fSupplierAddress;

		public IOrganizationAddress ManufacturerAddress => fManufacturerAddress ?? (fManufacturerAddress = ImportLicenseAddressProvider.New(InvoiceLine?.ManufacturerDocAddress));
		IOrganizationAddress fManufacturerAddress;

		public IEnumerable<IDrawbackNcmItemDetail> DrawbackNcmItemsList => drawbackNcmItem ??
			(drawbackNcmItem = invoiceLines.Select(x => DrawbackNcmItemDetailProvider.New(x))?.ToArray());
		IDrawbackNcmItemDetail[] drawbackNcmItem;

		public IEnumerable<IConsentingProcess> ConsentingProcessList => consentingProcess ??
			(consentingProcess = invoiceLines.SelectMany(line => line.ConsentingProcessCollection.Cast<ConsentingProcess>())
				.Where(x => !x.CSI_CustomsOffice.IsEmpty).Select(x => ConsentingProcessProvider.New(x))?.ToArray());
		IConsentingProcess[] consentingProcess;

		public IEnumerable<ITariffDetach> TariffDetachList => tariffDetach ??
			(tariffDetach = invoiceLines.SelectMany(x => x.TariffDetachs.Cast<TariffDetach>())
				.Where(x => !x.CY_Code.IsEmpty).GroupBy(x => x.CY_Code).Select(g => g.First()).Select(x => TariffDetachProvider.New(x))?.ToArray());

		ITariffDetach[] tariffDetach;
	}
}
