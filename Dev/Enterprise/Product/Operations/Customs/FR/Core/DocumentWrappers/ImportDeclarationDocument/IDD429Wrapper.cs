using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE429;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.ImportDeclarationDocument;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument;

public class IDD429Wrapper : DocBaseWrapper, IIDD<IDD429ItemWrapper, IDD429LiquidationWrapper, IDD429DutiesAndTaxWrapper>
{
	public IDD429Wrapper(FREDIMessage message, CusEntryHeader entryHeader, BusinessObjectFactory factory) : base(message, factory)
	{
		this.message = Argument.NotNull(message, nameof(message));
		this.messageObject = (message.MessageDataObject as IE429MessageDataObject)?.ResponseMessage;
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}

	public IDD429Wrapper(FREDIMessage message, BusinessObjectFactory factory) : base(message, factory)
	{
		this.message = Argument.NotNull(message, nameof(message));
		this.messageObject = (message.MessageDataObject as IE429MessageDataObject)?.ResponseMessage;
	}

	public static IDD429Wrapper New(DeltaIEFREDIMessage message)
	{
		if (message.MessageDataObject is IE429MessageDataObject messageDataObject)
		{
			var messageObject = messageDataObject.ResponseMessage;
			if (messageObject != null && message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				return new IDD429Wrapper(message, entryHeader, message.Factory);
			}
		}

		return null;
	}

	readonly CC429BType messageObject;
	readonly FREDIMessage message;
	readonly CusEntryHeader entryHeader;

	public CusEntryHeader EntryHeader => message.EM_LinkedObject as CusEntryHeader;

	public ZString SheetName => EntryHeader.MovementReferenceNumber.IsEmpty ? EntryHeader.CH_BGMReference : EntryHeader.MovementReferenceNumber;

	public JobDeclaration Declaration => EntryHeader.Declaration;

	public ZString DeclarationAcceptanceDateTime => ParseUtcDateTimeString(messageObject.DeclarationStatus.StateDateTime).ToString("dd/MM/yyyy HH:mm:ss");

	public ZString ReleaseDateTime
	{
		get
		{
			var result = ZString.Empty;
			var entryInstruction = entryHeader.EntryInstruction;
			if (entryInstruction != null)
			{
				var subStyle = entryInstruction.CEI_Style;
				if (subStyle == DeltaIEImportDeclarationTypeList.Codes.H1 ||
					subStyle == DeltaIEImportDeclarationTypeList.Codes.H2 ||
					subStyle == DeltaIEImportDeclarationTypeList.Codes.H3 ||
					subStyle == DeltaIEImportDeclarationTypeList.Codes.H4 ||
					subStyle == DeltaIEImportDeclarationTypeList.Codes.H5 ||
					subStyle == DeltaIEImportDeclarationTypeList.Codes.H6 ||
					subStyle == DeltaIEImportDeclarationTypeList.Codes.H7)
				{
					result = ParseUtcDateTimeString(messageObject.DeclarationStatus.StateDateTime).ToString("dd/MM/yyyy HH:mm:ss");
				}
			}

			return result;
		}
	}

	public ZString RelatedRelaseDateTime
	{
		get
		{
			var result = ZString.Empty;
			var entryInstruction = entryHeader.EntryInstruction;
			if (entryInstruction != null && entryInstruction.CEI_Style == DeltaIEImportDeclarationTypeList.Codes.I1)
			{
				var previousMessage = entryHeader.Messages
					.Where(x => x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit && x.EM_MessageDateTime <= message.EM_MessageDateTime)
					.OrderByDescending(x => x.EM_MessageDateTime)
					.FirstOrDefault();
				if (previousMessage != null && previousMessage.EM_MessageSubType == "433")
				{
					result = ParseUtcDateTimeString(messageObject.ImportOperation.ReleaseDate).ToString("dd/MM/yyyy HH:mm:ss");
				}
			}

			return result;
		}
	}

	ZDateTime ParseUtcDateTimeString(string utcDateStr)
	{
		var result = ZDateTime.Empty;

		if (!string.IsNullOrEmpty(utcDateStr))
		{
			if (utcDateStr.Contains("T"))
			{
				ZDateTime.TryParseExact(utcDateStr, out result, (NoResString)"yyyy-MM-ddTHH:mm:ss");
			}
			else
			{
				ZDateTime.TryParseExact(utcDateStr, out result, "yyyy-MM-dd");
			}
		}
		return result;
	}

	public ZString PrintDateTime => ZDateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

	public ZString DeclarationStatus => messageObject.DeclarationStatus.State == "BONAENLEVER" ? (NoResString)"BON A ENLEVER" : messageObject.DeclarationStatus.State;

	public ZString DeclarationType => string.Join(" ", messageObject.ImportOperation.DeclarationType, messageObject.ImportOperation.AdditionalDeclarationType);

	public ZString AdditionalDeclarationType => ZString.Empty;

	public ZString SupervisingCustomsOffice => messageObject.SupervisingCustomsOffice?.ReferenceNumber ?? ZString.Empty;

	public ZString PresentationCustomsOffice => messageObject.CustomsOfficeOfPresentation?.ReferenceNumber ?? ZString.Empty;

	protected MGoodsShipmentType05FR GoodsShipment => messageObject.GoodsShipment.FirstOrDefault();

	public ZString TotalItemsCount => (GoodsShipment?.GoodsShipmentItem?.Count ?? 0).ToString();

	public ZString AgreementNumber => GoodsShipment?.AdditionalReference?.FirstOrDefault(x => x.Type == "1DEC")?.ReferenceNumber ?? ZString.Empty;

	public ZString GoodsLocationType => GoodsShipment?.Consignment.LocationOfGoods?.TypeOfLocation ?? ZString.Empty;

	ZBool ShouldMapAdress => (GoodsShipment?.Consignment.LocationOfGoods?.QualifierOfIdentification ?? ZString.Empty) == Enterprise.Customs.Business.CusGoodsLocationQualifierList.Codes.Address;

	public ZString GoodsLocationAddress => ShouldMapAdress ? (GoodsShipment?.Consignment.LocationOfGoods?.Address?.StreetAndNumber ?? ZString.Empty) : ZString.Empty;

	public ZString GoodsLocationPostCode => ShouldMapAdress ? (GoodsShipment?.Consignment.LocationOfGoods?.Address?.Postcode ?? ZString.Empty) : ZString.Empty;

	public ZString GoodsLocationCity => ShouldMapAdress ? (GoodsShipment?.Consignment.LocationOfGoods?.Address?.City ?? ZString.Empty) : ZString.Empty;

	public ZString GoodsLocationCountry => ShouldMapAdress ? (GoodsShipment?.Consignment.LocationOfGoods?.Address?.Country ?? ZString.Empty) : ZString.Empty;

	public ZString SupplierName => GoodsShipment?.Exporter?.Name ?? ZString.Empty;

	public ZString SupplierEORI => GoodsShipment?.Exporter?.IdentificationNumber ?? ZString.Empty;

	public ZString SupplierAddress => GoodsShipment?.Exporter?.Address?.StreetAndNumber ?? ZString.Empty;

	public ZString SupplierPostCode => GoodsShipment?.Exporter?.Address?.Postcode ?? ZString.Empty;

	public ZString SupplierCity => GoodsShipment?.Exporter?.Address?.City ?? ZString.Empty;

	public ZString SupplierCountry => GoodsShipment?.Exporter?.Address?.Country ?? ZString.Empty;

	public ZString ImporterName => messageObject.Importer?.Name ?? ZString.Empty;

	public ZString ImporterEORI => messageObject.Importer?.IdentificationNumber ?? ZString.Empty;

	public ZString ImporterAddress => messageObject.Importer?.Address?.StreetAndNumber ?? ZString.Empty;

	public ZString ImporterPostCode => messageObject.Importer?.Address?.Postcode ?? ZString.Empty;

	public ZString ImporterCity => messageObject.Importer?.Address?.City ?? ZString.Empty;

	public ZString ImporterCountry => messageObject.Importer?.Address?.Country ?? ZString.Empty;

	public ZString DeclarantName => messageObject.Declarant?.Name ?? ZString.Empty;

	public ZString DeclarantEORI => messageObject.Declarant?.IdentificationNumber ?? ZString.Empty;

	public ZString DeclarantAddress => messageObject.Declarant?.Address?.StreetAndNumber ?? ZString.Empty;

	public ZString DeclarantPostCode => messageObject.Declarant?.Address?.Postcode ?? ZString.Empty;

	public ZString DeclarantCity => messageObject.Declarant?.Address?.City ?? ZString.Empty;

	public ZString DeclarantCountry => messageObject.Declarant?.Address?.Country ?? ZString.Empty;

	public ZString RepresentativeEORI => messageObject.Representative?.IdentificationNumber ?? ZString.Empty;

	public ZString RepresentativeType => messageObject.Representative?.Status ?? ZString.Empty;

	public ZString RepresentativeContactName => messageObject.Representative?.ContactPerson?.Name ?? ZString.Empty;

	public ZString RepresentativeContactPhoneNumber => messageObject.Representative?.ContactPerson?.PhoneNumber ?? ZString.Empty;

	public ZString RepresentativeContactEMail => messageObject.Representative?.ContactPerson?.EMailAddress ?? ZString.Empty;

	public ZString DeclarantContactName => messageObject.Declarant?.ContactPerson?.Name ?? ZString.Empty;

	public ZString DeclarantContactPhoneNumber => messageObject.Declarant?.ContactPerson?.PhoneNumber ?? ZString.Empty;

	public ZString DeclarantContactEMail => messageObject.Declarant?.ContactPerson?.EMailAddress ?? ZString.Empty;

	public ZString ConsigneeName => GoodsShipment?.Consignee?.Name ?? ZString.Empty;

	public ZString ConsigneeEori => GoodsShipment?.Consignee?.IdentificationNumber ?? ZString.Empty;

	public ZString SupplyChainActorEORI => GoodsShipment?.AdditionalSupplyChainActor?.FirstOrDefault()?.IdentificationNumber ?? ZString.Empty;

	public ZString SupplyChainActorRole => GoodsShipment?.AdditionalSupplyChainActor?.FirstOrDefault()?.Role ?? ZString.Empty;

	public ZString AuthorizationType => messageObject.Authorisation?.FirstOrDefault()?.Type ?? ZString.Empty;

	public ZString AuthorizationNumber => messageObject.Authorisation?.FirstOrDefault()?.ReferenceNumber ?? ZString.Empty;

	public ZString AuthorizationHolder => messageObject.Authorisation?.FirstOrDefault()?.HolderOfTheAuthorisation ?? ZString.Empty;

	public ZString CountryOfDestination => GoodsShipment?.Destination?.CountryOfDestination ?? ZString.Empty;

	public ZString CountryOfDispatch => GoodsShipment?.CountryOfDispatch?.CountryOfDispatch ?? ZString.Empty;

	public ZString IncotermCode => GoodsShipment?.DeliveryTerms?.IncotermCode ?? ZString.Empty;

	public ZString IncotermUNLOCO => GoodsShipment?.DeliveryTerms?.UNLOCODE ?? ZString.Empty;

	public ZString IncotermLocation => GoodsShipment?.DeliveryTerms?.Location ?? ZString.Empty;

	public ZString IncotermCountry => GoodsShipment?.DeliveryTerms?.Country ?? ZString.Empty;

	public ZString IncotermText => GoodsShipment?.DeliveryTerms?.Text ?? ZString.Empty;

	public ZString WarehouseType => GoodsShipment?.Warehouse?.Type ?? ZString.Empty;

	public ZString TotalGrossMass => GoodsShipment?.Consignment?.GrossMass.ToString() ?? ZString.Empty;

	public ZString IsContainerised => GoodsShipment?.Consignment?.ContainerIndicator ?? ZString.Empty;

	public ZString ContainerId => GoodsShipment?.Consignment?.TransportEquipment?.FirstOrDefault()?.ContainerIdentificationNumber ?? ZString.Empty;

	public ZString TransportMeansNationalityAtBorder => GoodsShipment?.Consignment?.ActiveBorderTransportMeans?.Nationality ?? ZString.Empty;

	public ZString InlandModeOfTransport => GoodsShipment?.Consignment?.InlandModeOfTransport ?? ZString.Empty;

	public ZString ModeOfTransportAtTheBorder => GoodsShipment?.Consignment?.ModeOfTransportAtTheBorder ?? ZString.Empty;

	public ZString IsActiveAtBorder => ZString.Empty;

	public ZString ArrivalTransportMeansIdType => GoodsShipment?.Consignment?.ArrivalTransportMeans?.TypeOfIdentification ?? ZString.Empty;

	public ZString SpecialMentions
	{
		get
		{
			var result = ZString.Empty;
			if (!(GoodsShipment?.AdditionalInformation).IsNullOrEmpty())
			{
				result = string.Join(" - ", GoodsShipment.AdditionalInformation.Select(p => p.Code));
			}

			return result;
		}
	}

	public ZString FiscalReferences
	{
		get
		{
			var result = ZString.Empty;
			if (!(GoodsShipment?.AdditionalFiscalReference).IsNullOrEmpty())
			{
				result = string.Join(" - ", GoodsShipment.AdditionalFiscalReference.Select(p => p.FiscalReferenceIdentificationNumber));
			}

			return result;
		}
	}

	public ZString AdditionalReferences
	{
		get
		{
			var result = ZString.Empty;
			if (!(GoodsShipment?.AdditionalReference).IsNullOrEmpty())
			{
				result = string.Join(" - ", GoodsShipment.AdditionalReference.Select(p => string.Join(" ", p.Type, p.ReferenceNumber)));
			}

			return result;
		}
	}

	public ZString PreviousDocuments
	{
		get
		{
			var result = ZString.Empty;
			if (!(GoodsShipment?.PreviousDocument).IsNullOrEmpty())
			{
				result = string.Join(" - ", GoodsShipment.PreviousDocument.Select(p => string.Join(" ", p.Type, p.ReferenceNumber)));
			}

			return result;
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
	public ZString SupportingDocuments
	{
		get
		{
			var result = ZString.Empty;
			if (!(GoodsShipment?.SupportingDocument).IsNullOrEmpty())
			{
				result = string.Join(" - ", GoodsShipment.SupportingDocument.Select(p => string.Join(" ", p.Type, p.ReferenceNumber) + (p.DocumentLineItemNumber.IsNullOrEmpty() ? ZString.Empty : "(Fiche d'Imputation)")));
			}

			return result;
		}
	}

	public virtual ZString TransportDocuments
	{
		get
		{
			var result = ZString.Empty;
			if (!(GoodsShipment?.Consignment?.TransportDocument).IsNullOrEmpty())
			{
				result = string.Join(" - ", GoodsShipment.Consignment?.TransportDocument.Select(p => string.Join(" ", p.Type, p.ReferenceNumber)));
			}

			return result;
		}
	}

	public ZString GuaranteeNumber => messageObject.Guarantee?.FirstOrDefault()?.GuaranteeReference?.FirstOrDefault()?.GRN ?? ZString.Empty;

	public ZString GuaranteeOffice => messageObject.Guarantee?.FirstOrDefault()?.GuaranteeReference?.FirstOrDefault()?.CustomsOfficeOfGuarantee?.ReferenceNumber ?? ZString.Empty;

	public ZString TotalInvoicedAmount => GoodsShipment?.TotalAmountInvoiced.ToString() ?? ZString.Empty;

	public ZString TotalInvoicedCurrency => GoodsShipment?.InvoiceCurrency ?? ZString.Empty;

	public ZString GuaranteedAmount => messageObject.GeneralTaxation?.TotalPayableTaxAmount?.AmountUsed?.GuaranteedAmount.ToString() ?? ZString.Empty;

	public ZString UnguaranteedAmount => messageObject.GeneralTaxation?.TotalPayableTaxAmount?.AmountUsed?.UnguaranteedAmount.ToString() ?? ZString.Empty;

	public ZString TotalPayableTaxAmount => messageObject.GeneralTaxation?.TotalPayableTaxAmount?.TotalPayableTaxAmount.ToString() ?? ZString.Empty;

	public ZString AmountToBeCovered => messageObject.Guarantee?.FirstOrDefault()?.GuaranteeReference?.FirstOrDefault()?.AmountToBeCovered.ToString() ?? ZString.Empty;

	public ZString MRN => messageObject.ImportOperation.MRN;

	public DocBaseWrapperCollection<IDDImputationSheet429Wrapper> ImputationSheets => imputationSheets ?? (imputationSheets = GetImputationSheets());
	DocBaseWrapperCollection<IDDImputationSheet429Wrapper> imputationSheets;

	DocBaseWrapperCollection<IDDImputationSheet429Wrapper> GetImputationSheets()
	{
		var result = new IDDImputationSheet429WrapperCollection(Factory);
		foreach (var item in GoodsShipment?.GoodsShipmentItem)
		{
			foreach (var supportingDocument in item.SupportingDocument)
			{
				if (supportingDocument.Quantity > 0 || supportingDocument.Amount > 0)
				{
					result.Add(IDDImputationSheet429Wrapper.New(supportingDocument, item.DeclarationGoodsItemNumber, Factory));
				}
			}
		}

		return result;
	}

	public DocBaseWrapperCollection<IDD429ItemWrapper> Items => items ??= GetItems();
	DocBaseWrapperCollection<IDD429ItemWrapper> items;

	DocBaseWrapperCollection<IDD429ItemWrapper> GetItems()
	{
		int i = 0;
		var result = new IDDGoodsShipmentWrapper<IDD429ItemWrapper>(Factory);
		GoodsShipment?.GoodsShipmentItem?.ForEach(item => result.Add(IDD429ItemWrapper.New(++i, item, messageObject.DetailedTaxation?.GoodsShipment?.GoodsShipmentItem?.FirstOrDefault(itemFromDetailedTaxation => itemFromDetailedTaxation.DeclarationGoodsItemNumber.Equals(item.DeclarationGoodsItemNumber)), messageObject.GeneralTaxation?.DutiesAndTaxesSummaries, Factory)));
		return result;
	}

	public ZString ItemsCount => Items != null ? Items.Count.ToString() : "0";

	public ZString NatureOfTransaction => GoodsShipment?.NatureOfTransaction ?? ZString.Empty;

	public virtual ZString RepresentativeName => ZString.Empty;

	public virtual ZString SupplyChainActorName => ZString.Empty;

	public virtual ZString WarehouseLocation => ZString.Empty;

	public virtual ZString ModeOfPayment => GoodsShipment?.GoodsShipmentItem?.FirstOrDefault(item => item.DeclarationGoodsItemNumber.Equals("1"))?.Commodity?.CalculationOfTaxes?.DutiesAndTaxe?.FirstOrDefault()?.MethodOfPayment ?? ZString.Empty;

	public virtual ZString CRN => ZString.Empty;

	public ZString LRN => messageObject.ImportOperation.LRN;

	public ZBool IsImputationSheetsCollectionAvailable => true;

	public ZString ReverseChargeVAT => messageObject?.GeneralTaxation?.VAT?.ReverseChargeVATAmount.ToString() ?? ZString.Empty;

	public ZString VATAmountAI2 => messageObject?.GeneralTaxation?.VAT?.VATamountAI2.ToString() ?? ZString.Empty;
}
