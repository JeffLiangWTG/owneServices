using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Declaration;
using Enterprise.Customs.GB.CDS.Messaging.Calculators;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;
using JobDeclaration = Enterprise.Customs.GB.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers
{
	public class GbCDSImportDeclarationWrapper : IImportDeclaration
		, IConsignment
	{
		public GbCDSImportDeclarationWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
			declaration = entryHeader.Declaration;
			entryInstruction = entryHeader.EntryInstruction;
		}

		protected GbCDSImportEntryHeaderWrapper CDSEntryHeaderWrapper => cdsImportEntryHeaderWrapper ?? (cdsImportEntryHeaderWrapper = GetHeaderWrapper(entryHeader));
		GbCDSImportEntryHeaderWrapper cdsImportEntryHeaderWrapper;

		protected readonly CusEntryHeader entryHeader;
		protected readonly JobDeclaration declaration;
		readonly CusEntryInstruction entryInstruction;

		IAmountAndCurrency IDeclaration.FreightChargeAmount => new H7DeclarationFreightChargeCalculator(entryHeader).CalculateFreightChargeAmount();

		protected virtual GbCDSImportEntryHeaderWrapper GetHeaderWrapper(CusEntryHeader header)
		{
			return new GbCDSImportEntryHeaderWrapper(header);
		}

		ZString IDeclaration.ExitOfficeID => GetExitOfficeID();
		protected virtual ZString GetExitOfficeID() => ZString.Empty;

		ZString IDeclaration.SpecificCircumstancesCodeCode => GetSpecificCircumstancesCodeCode();
		protected virtual ZString GetSpecificCircumstancesCodeCode() => ZString.Empty;

		ZString IDeclaration.DeclarationTypeCode => entryHeader.GetCdsDeclarationTypeCode();
		ZInt IDeclaration.GoodsItemQuantity => entryHeader.MergedLines.Count;
		ZString IDeclaration.FunctionalReferenceID => CusEntryHeader.LRNReferencePlaceHolderXmlFriendly;
		ZDateTime IDeclaration.AcceptanceDateTime => declaration.JE_EntryAuthorisationDate;
		ZString IDeclaration.PresentationOffice => declaration.JE_CustomsOffice;
		ZString IDeclaration.SupervisingOffice => declaration.SupervisingOfficeDocAddress.Organisation?.LocalCustomsClientCode ?? ZString.Empty;
		ZDecimal IDeclaration.TotalPackageQuantity => ((ZDecimal)entryHeader.PackagesCount).Normalize();

		IEnumerable<IStatement> IDeclaration.AdditionalInformations
		{
			get
			{
				foreach (var ai in entryHeader.AdditionalInfos.OfType<Business.Declaration.MultiLineAddInfos.AdditionalInfo>().Where(ai => ai.IsHeaderOnly).Distinct())
				{
					yield return new Business.Messaging.Statement(ai);
				}
			}
		}

		//3/1
		//3/2
		IOrganisation IDeclaration.Exporter
		{
			get
			{
				var exporter = declaration.SupplierDocumentaryAddress.Organisation;
				var exporterId = exporter?.GetEuIdentificationNumber() ?? ZString.Empty;

				IOrganisation organisation = entryHeader.HasMultipleExportersViaLines()
					? OrganisationWrapper.New("00200", ZString.Empty, AddressWrapper.New("00200", "00200", ExporterCountryCode, "00200"))
					: (exporter != null ?
							(exporterId.IsEmpty ? (!CDSExtensions.IsUnmatchedOrgHeader(exporter) ? OrganisationWrapper.New(declaration.SupplierDocumentaryAddress) : null) : OrganisationWrapper.New(ZString.Empty, exporterId))
							: null);

				if (organisation != null && organisation.IsForeignEori && !declaration.IsSendForeignEoriToCds)
				{
					organisation = OrganisationWrapper.New(declaration.SupplierDocumentaryAddress, true);
				}
				return organisation;
			}
		}

		ZString ExporterCountryCode => declaration.JE_GoodsOrigin;

		IOrganisation IDeclaration.Declarant => !CDSExtensions.IsUnmatchedOrgAddress(declaration.DeclarantAddress) ? OrganisationWrapper.New(declaration.DeclarantAddress, declaration.DeclarantTraderId) : null;

		IAgent IDeclaration.Agent => declaration.Representative == null || CDSExtensions.IsUnmatchedOrgAddress(declaration.Representative)
										? AgentWrapper.New(null, CDSEntryHeaderWrapper.ImportHeaderWrapper.DECLT_REP)
										: AgentWrapper.New(OrganisationWrapper.New(declaration.Representative), CDSEntryHeaderWrapper.ImportHeaderWrapper.DECLT_REP);

		IEnumerable<IDecAdditionalDocument> IDeclaration.DecAdditionalDocuments
		{
			get
			{
				var approvalDeferNumber = declaration.JE_DefermentAccountNumber;
				if (!approvalDeferNumber.IsEmpty)
				{
					yield return DecAdditionalDocumentWrapper.New("1", "DAN", approvalDeferNumber);
				}

				var approvalVATNumber = declaration.ZG_VATDeferNumber;
				if (!approvalVATNumber.IsEmpty)
				{
					yield return DecAdditionalDocumentWrapper.New("2", "DAN", approvalVATNumber);
				}
			}
		}

		IGoodsShipment IDeclaration.GoodsShipment => CDSEntryHeaderWrapper;

		IEnumerable<IAuthorisationHolder> IDeclaration.AuthorisationHolders => entryInstruction.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner.Cast<CusAuthorizationUsage>().Select(x => AuthorisationHolderWrapper.New(x.AGC_Number, x.AGC_Code));

		IEnumerable<ICurrencyExchange> IDeclaration.CurrencyExchanges
		{
			get
			{
				var factory = entryHeader.Factory;
				var currentCurrencyCode = ZString.Empty;
				foreach (var invoiceHeader in entryHeader.InvoiceHeaders)
				{
					var currencyCode = invoiceHeader.JZ_RX_NKInvoice_Currency;
					if (currentCurrencyCode != currencyCode)
					{
						currentCurrencyCode = currencyCode;
						var refCurrency = RefCurrency.LoadFromCurrencyCode(factory, currentCurrencyCode);
						if (refCurrency != null)
						{
							yield return CurrencyExchangeWrapper.New(refCurrency.CurrentCustomsRate);
						}
					}
				}
			}
		}

		ITransportMeans IDeclaration.BorderTransportMeans => TransportMeansWrapper.New(
			BorderTransportMeansID
			, CDSEntryHeaderWrapper.ModeOfTransportAtTheBorder74
			, BorderTransportMeansIdType
			, CDSEntryHeaderWrapper.NationalityOfActiveMeansOfTransportCrossingTheBorder715);

		protected virtual ZString BorderTransportMeansID => ZString.Empty;
		protected virtual ZString BorderTransportMeansIdType => CDSEntryHeaderWrapper.IdentityOfMeansOfTransportOnArrivalTypeOfIdentification79;

		IEnumerable<IObligationGuarantee> IDeclaration.ObligationGuarantees
		{
			get => declaration.Guarantees.Cast<GBGuarantee>()
				.Where(x => x.IsGuarantee && x.IsRelatedToEntryInstruction(entryInstruction))
				.Select(ObligationGuaranteeWrapper.New);
		}

		ZDecimal IDeclaration.TotalGrossMassMeasure => entryHeader.GrossWeight.Amount.Normalize();

		IConsignment IDeclaration.Consignment => this;

		IAmountAndCurrency IDeclaration.InvoiceAmount => GetInvoiceAmount();

		protected virtual IAmountAndCurrency GetInvoiceAmount() => null;

		#region IConsignment

		ZString IConsignment.ContainerCode => throw new System.NotSupportedException();

		ITransportMeans IConsignment.ArrivalTransportMeans => throw new System.NotSupportedException();

		ITransportMeans IConsignment.DepartureTransportMeans => throw new System.NotSupportedException();

		IGoodsLocation IConsignment.GoodsLocation => throw new System.NotSupportedException();

		ZString IConsignment.LoadingLocationID => throw new System.NotSupportedException();

		IEnumerable<ITransportEquipment> IConsignment.TransportEquipments => throw new System.NotSupportedException();

		IOrganisation IConsignment.Carrier => GetCarrier();

		protected virtual IOrganisation GetCarrier() => null;

		ZString IConsignment.FreightPaymentMethodCode => GetFreightPaymentMethodCode();

		protected virtual ZString GetFreightPaymentMethodCode() => ZString.Empty;

		IEnumerable<ZString> IConsignment.ItineraryRoutingCountryCodes => GetItineraryRoutingCountryCodes();

		protected virtual IEnumerable<ZString> GetItineraryRoutingCountryCodes() => Enumerable.Empty<ZString>();

		IConsignmentItem IConsignment.ConsignmentItem => throw new System.NotSupportedException();

		IOrganisation IConsignment.Consignor => GetConsignor();

		protected virtual IOrganisation GetConsignor() => null;

		IOrganisation IDeclaration.ExporterNameAndAddress => !CDSExtensions.IsUnmatchedJobDocAddress(declaration.SupplierDocumentaryAddress) ? OrganisationWrapper.New(declaration.SupplierDocumentaryAddress, true) : null;

		#endregion
	}
}
