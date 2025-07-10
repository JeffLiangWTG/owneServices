using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.GB.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers
{
	public class GbCDSImportEntryHeaderWrapper : IGoodsShipment
		, IConsignment
	{
		public GbCDSImportEntryHeaderWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
			randomHeader = Argument.NotNull(entryHeader.RandomHeader, nameof(randomHeader));
			ImportHeaderWrapper = new GbCDSImportHeader(entryHeader);
			declaration = entryHeader.Declaration;
		}

		public GbCDSImportHeader ImportHeaderWrapper { get; private set; }

		#region IGoodsShipment

		IEnumerable<IGovernmentAgencyGoodsItem> IGoodsShipment.GovernmentAgencyGoodsItems => entryHeader.MergedLines.Cast<CusEntryLine>().Select(GetLineWrapper);

		protected virtual GbCDSImportEntryLineWrapper GetLineWrapper(CusEntryLine entryLine) => new GbCDSImportEntryLineWrapper(entryLine);

		ZString IGoodsShipment.UCRTraderAssignedReferenceID => CusEntryHeader.BGMReferencePlaceHolderXmlFriendly;

		ZString WarehouseId => entryHeader.EntryInstruction.WarehouseIDFor27;

		IWareHouse IGoodsShipment.Warehouse => WarehouseWrapper.New(WarehouseId.SubstringSafe(0, 1), WarehouseId.SubstringSafe(1));

		//3/15 not needed if 3/16 sent
		//3/16
		IOrganisation IGoodsShipment.Importer
		{
			get
			{
				var importer = declaration.ImporterDocumentaryAddress.Organisation;
				var importerId = importer != null && importer.OH_Category != OrgConstants.Category.NaturalPersonIndividual ? importer.GetEuIdentificationNumber() : ZString.Empty;
				IOrganisation result = !CDSExtensions.IsUnmatchedOrgHeader(importer)
					? (importerId.IsEmpty ? OrganisationWrapper.New(declaration.ImporterDocumentaryAddress) : OrganisationWrapper.New(ZString.Empty, importerId))
					: null;

				if (result != null && result.IsForeignEori && !declaration.IsSendForeignEoriToCds)
				{
					result = OrganisationWrapper.New(declaration.ImporterDocumentaryAddress, ignoreID: true);
				}
				return result;
			}
		}

		//3/24
		//3/25
		IOrganisation IGoodsShipment.Seller
		{
			get
			{
				OrganisationWrapper result = null;
				var countSellersViaInvoiceHeaders = entryHeader?.CountSellersViaInvoiceHeaders() ?? ZInt.Zero;

				if (countSellersViaInvoiceHeaders == 0)
				{
					var exporter = declaration.SupplierDocumentaryAddress.Organisation;
					var seller = declaration.SellerAddress?.Header;

					if (seller != null && (exporter == null || seller != exporter))
					{
						result = !CDSExtensions.IsUnmatchedOrgAddress(declaration.SellerAddress) ? OrganisationWrapper.New(declaration.SellerAddress, seller.GetEuIdentificationNumber(), outputPreference: EoriNameAndAddressOrBoth.EORI) : null;
					}
				}
				else if (countSellersViaInvoiceHeaders == 1)
				{
					result = !CDSExtensions.IsUnmatchedOrgAddress(randomHeader.SellerAddress) ? OrganisationWrapper.New(randomHeader.SellerAddress, randomHeader.SellerAddress?.Header.GetEuIdentificationNumber(), EoriNameAndAddressOrBoth.EORI) : null;
				}

				return result;
			}
		}

		//3/26
		//3/27
		IOrganisation IGoodsShipment.Buyer
		{
			get
			{
				OrganisationWrapper result = null;
				var countBuyersViaInvoiceHeaders = entryHeader?.CountBuyersViaInvoiceHeaders() ?? ZInt.Zero;

				if (countBuyersViaInvoiceHeaders == 0)
				{
					var importer = declaration.ImporterDocumentaryAddress.Organisation;
					var buyerDocAddress = declaration.BuyerDocAddress;
					if (buyerDocAddress.IsEmpty)
					{
						var shipmentBuyer = declaration.Shipment?.BuyerDocAddress;
						if (shipmentBuyer != null)
						{
							buyerDocAddress = shipmentBuyer;
						}
					}
					var buyer = buyerDocAddress.Organisation;

					if (buyer != null && (importer == null || importer != buyer))
					{
						result = !CDSExtensions.IsUnmatchedJobDocAddress(buyerDocAddress) ? OrganisationWrapper.New(buyerDocAddress, outputPreference: EoriNameAndAddressOrBoth.EORI) : null;
					}
				}
				else if (countBuyersViaInvoiceHeaders == 1)
				{
					result = !CDSExtensions.IsUnmatchedOrgAddress(randomHeader.BuyerAddress) ? OrganisationWrapper.New(randomHeader.BuyerAddress, randomHeader.BuyerAddress?.Header.GetEuIdentificationNumber(), EoriNameAndAddressOrBoth.EORI) : null;
				}

				return result;
			}
		}

		IEnumerable<IParty> IGoodsShipment.AEOMutualRecognitionParties => GetAEOMutualRecognitionParties();

		protected virtual IEnumerable<IParty> GetAEOMutualRecognitionParties() => null;

		IEnumerable<IParty> IGoodsShipment.DomesticDutyTaxParties
		{
			get
			{
				foreach (var fr in entryHeader.FiscalReferences)
				{
					yield return PartyWrapper.New(fr.CFR_Reference, fr.CFR_Code);
				}
			}
		}

		ITradeTerms IGoodsShipment.TradeTerms
		{
			get
			{
				return TradeTermsWrapper.New(
					!randomHeader.JZ_IncoTerm.IsEmpty ? randomHeader.JZ_IncoTerm : declaration.JE_ShipmentIncoTerm,
					!randomHeader.JZ_IncoTermPlace.IsEmpty ? randomHeader.JZ_IncoTermPlace : declaration.JE_ShipmentIncoTermPlace,
					ZString.Empty);
			}
		}

		ZString IGoodsShipment.ExportCountryID => ImportHeaderWrapper.CountryOfExport;

		ZString IGoodsShipment.DestinationCountryCode => ImportHeaderWrapper.CountryOfDestination;

		ZString IGoodsShipment.TransactionNatureCode => randomHeader.JZ_ValuationCode;

		IConsignment IGoodsShipment.Consignment => this;

		ICustomsValuation IGoodsShipment.CustomsValuation
		{
			get
			{
				var chargeDeductions = entryHeader.CDSChargeDeductions.Select(x =>
					ChargeDeductionWrapper.New(
						AmountAndCurrencyWrapper.New(x.Value.Amount, x.Value.Currency?.Code ?? ZString.Empty),
						x.Key.Left(2)));

				return CustomsValuationWrapper.New(
					chargeDeductions,
					randomHeader.ZG_TransportChargesMethodOfPayment
				);
			}
		}

		public ZString CDSDUCRAutomationSetting => GBCustomsDataRegistry.Instance.CDSDUCRAutomation.Value.CDSDUCRAutomation;

		IEnumerable<IPreviousDocument> IGoodsShipment.PreviousDocuments
		{
			get
			{
				var previousDocuments = entryHeader.PreviousDocuments.Select(PreviousDocumentWrapper.New).OfType<IPreviousDocument>().ToList();

				if (ShouldAddPreviousDocumentForDucrAndPart)
				{
					var ucrRef = (CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields == CDSDUCRAutomationSetting) ?
							CusEntryHeader.UCRReferencePlaceHolderXmlFriendly : CusEntryHeader.BGMReferencePlaceHolderXmlFriendly;

					if (CDSUCRAutomationSettingsList.Codes.NotForImports != CDSDUCRAutomationSetting)
					{
						previousDocuments.Add(DCRPreviousDocument.New(previousDocuments.Count, ucrRef));
					}

					if (!entryHeader.DeclarationUCRPartSuffix.IsEmpty)
					{
						if (CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields == CDSDUCRAutomationSetting)
						{
							previousDocuments.Add(DCSPreviousDocument.New(previousDocuments.Count, entryHeader.DeclarationUCRPartSuffix));
						}
					}
				}
				if (!entryHeader.CH_MasterUCR.IsEmpty)
				{
					previousDocuments.Add(MUCRPreviousDocument.New(previousDocuments.Count, entryHeader.CH_MasterUCR));
				}

				return previousDocuments;
			}
		}
		bool ShouldAddPreviousDocumentForDucrAndPart => (entryHeader?.EntryInstruction?.CEI_Style ?? ZString.Empty) != (ZString)ImportDeclarationTypeList.Codes.FinalSupplementaryDeclaration;

		// 7/4 - Declaration/BorderTransportMeans/ModeCode
		public ZString ModeOfTransportAtTheBorder74 => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportMode);

		// 7/15 - Declaration/BorderTransportMeans/RegistrationNationalityCode
		public ZString NationalityOfActiveMeansOfTransportCrossingTheBorder715 => declaration.JE_RN_NKTransportNationality;

		// 7/5 - Declaration/GoodsShipment/Consignment/ArrivalTransportMeans/ModeCode
		public ZString InlandModeOfTransport75 => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland, true);

		// 7/7 7/9 - Declaration/GoodsShipment/Consignment/ArrivalTransportMeans/IdentificationTypeCode
		public ZString IdentityOfMeansOfTransportOnArrivalTypeOfIdentification79 => GetArrivalTransportMeansIdentificationTypeCode(declaration.JE_TransportMode);

		// 7/7 7/9 - Declaration/GoodsShipment/Consignment/ArrivalTransportMeans/ID
		public ZString IdentityOfMeansOfTransportOnArrivalIdentificationNumber79 => GetArrivalTransportMeansId(declaration.JE_TransportMode);

		public ZString GetArrivalTransportMeansId(ZString transportMode)
		{
			switch (transportMode)
			{
				case Enterprise.Customs.Business.TransportTypeList.Codes.Air:
					return declaration.JE_VoyageFlightNo;
				case Enterprise.Customs.Business.TransportTypeList.Codes.Sea:
					return declaration.JE_VesselName;
				default:
					return declaration.ZG_Box18TransportID;
			}
		}

		public ZString GetArrivalTransportMeansIdentificationTypeCode(ZString transportMode)
		{
			var transportId = GetArrivalTransportMeansId(transportMode);

			if (!transportId.IsEmpty)
			{
				//https://www.gov.uk/guidance/import-declaration-completion-guide#group-7-transport-information-modes-means-and-equipment
				//10  IMO ship identification number
				//11  Name of the sea-going vessel
				//20  Wagon number
				//30  Registration number of the road vehicle
				//40  IATA flight number
				//41  Registration number of the aircraft
				//80  European Vessel Identification Number(ENI code)
				//81  Name of the inland waterways vessel
				switch (transportMode)
				{
					case Enterprise.Customs.Business.TransportTypeList.Codes.Road:
					case GBTransportTypeList.Codes.ROR:
						return "30";
					case Enterprise.Customs.Business.TransportTypeList.Codes.Air:
						return "40";
					case Enterprise.Customs.Business.TransportTypeList.Codes.Sea:
						return "11";
					case Enterprise.Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport:
						return "81";
					case Enterprise.Customs.Business.TransportTypeList.Codes.Rail:
						return "20";
					case Enterprise.Customs.Business.TransportTypeList.Codes.OwnPropulsion:
						return "41";    //Note: Not the real type of Identification - just a dummy value for now
					default:
						return ZString.Empty;
				}
			}
			return ZString.Empty;
		}

		#endregion

		#region IConsignment

		ZString IConsignment.ContainerCode => declaration.JE_ContainerMode;

		ITransportMeans IConsignment.ArrivalTransportMeans => TransportMeansWrapper.New(
			IdentityOfMeansOfTransportOnArrivalIdentificationNumber79
			, InlandModeOfTransport75
			, IdentityOfMeansOfTransportOnArrivalTypeOfIdentification79
			, ZString.Empty //N/A
		);

		ITransportMeans IConsignment.DepartureTransportMeans => GetDepartureTransportMeans();

		protected virtual ITransportMeans GetDepartureTransportMeans() => null;

		IGoodsLocation IConsignment.GoodsLocation
		{
			get
			{
				var fullLocation = entryHeader.EntryInstruction.CurrentLocationFor523;  // e.g. GBAUXXXYYYZZZ or GBBYCW1234567
				var country = fullLocation.SubstringSafe(0, 2);
				var aOrB_TypeCode = fullLocation.SubstringSafe(2, 1);
				var uOrY_AddressTypeCode = fullLocation.SubstringSafe(3, 1);
				var name = fullLocation.SubstringSafe(4);
				return GoodsLocationWrapper.New(name, aOrB_TypeCode, uOrY_AddressTypeCode, country);
			}
		}

		ZString IConsignment.LoadingLocationID => ImportHeaderWrapper.OSAirTransportLoad;

		IEnumerable<ITransportEquipment> IConsignment.TransportEquipments
		{
			get => entryHeader.Containers.Cast<EU.Business.Declaration.CusContainer>()
				.Where(x => !x.CO_ContainerNumber.IsEmpty)
				.Distinct()
				.Select(x => TransportEquipmentWrapper.New(x.CO_ContainerNumber, x.CO_Seal, x.CO_SecondSeal));
		}

		IOrganisation IConsignment.Carrier => null; //not supported on header level

		IOrganisation IConsignment.Consignor => null; //not supported on header level

		ZString IConsignment.FreightPaymentMethodCode => ImportHeaderWrapper.TransportChargesMethodOfPayment;

		IEnumerable<ZString> IConsignment.ItineraryRoutingCountryCodes => Enumerable.Empty<ZString>(); //not supported on header level

		IConsignmentItem IConsignment.ConsignmentItem => null; //not supported on header level

		#endregion

		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly JobComInvoiceHeader randomHeader;
	}
}
