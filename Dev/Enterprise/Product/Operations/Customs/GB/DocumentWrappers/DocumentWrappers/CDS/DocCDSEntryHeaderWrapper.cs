using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.GB.DocumentWrappers
{
	public class DocCDSEntryHeaderWrapper : DocBaseWrapper
	{
		public DocCDSEntryHeaderWrapper(CusEntryHeader entryHeader, BusinessObjectFactory factory) : base(entryHeader, factory)
		{
			Argument.NotNull(entryHeader, nameof(entryHeader));
			this.entryHeader = entryHeader;
		}

		#region BOs

		public CusEntryHeader EntryHeader => entryHeader;
		readonly CusEntryHeader entryHeader;

		#endregion

		#region Properties

		public ZString FormattedEntryNumber => EntryHeader.EntryNumber;

		public Image ClientLogo => SystemDataRegistry.Instance.CompanyLogo.GetFallBackValueAtAllLevels(EntryHeader.RegistryCompanyPK, EntryHeader.RegistryBranchPK, Guid.Empty);

		public ZString EntryStyle
		{
			get
			{
				var stringBuilder = new ZStringBuilder();

				if (entryHeader.Declaration != null && entryHeader.Declaration.JE_ApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF)
				{
					stringBuilder.AppendIfNotEmpty(EntryHeader.Declaration.JE_EntryStyle);
				}
				else
				{
					stringBuilder.AppendIfNotEmpty(EntryHeader.EntryInstruction.CEI_Style);
				}

				return stringBuilder.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		public ZString ExitedStatusDescription => EntryHeader.Lookups.ExportExitStatusList.GetDescriptionFromCode(EntryHeader.CH_ExitedStatus);

		public ZString ExporterFullName => EntryHeader.Declaration?.SupplierDocumentaryAddress?.CompanyName ?? ZString.Empty;
		public ZString ExporterStreetAndNumber => EntryHeader.Declaration?.SupplierDocumentaryAddress?.Address1 ?? ZString.Empty;
		public ZString ExporterEORI => EntryHeader.SupplierEoriOfMainOffice;

		public ZString ImporterCaption => EntryHeader.IsExport ? "[8] Importer [3/9]" : "[8] Importer [3/15]";
		public ZString ImporterFullName => EntryHeader.Declaration?.ImporterDocumentaryAddress?.CompanyName ?? ZString.Empty;
		public ZString ImporterStreetAndNumber => EntryHeader.Declaration?.ImporterDocumentaryAddress?.Address1 ?? ZString.Empty;
		public ZString ImporterEORICaption => EntryHeader.IsExport ? "No [3/10]" : "No [3/16]";
		public ZString ImporterEORI => EntryHeader.ImporterEoriOfMainOffice;

		public ZString DeclarantFullName => EntryHeader.Declaration?.Declarant?.Header?.OH_FullName ?? ZString.Empty;
		public ZString DeclarantStreetAndNumber => EntryHeader.Declaration?.Declarant?.Address1 ?? ZString.Empty;
		public ZString DeclarantEORI => EntryHeader.RepresentativeOrDeclarantEoriOfMainOffice;

		public OrgHeader SellerConsignor => EntryHeader.IsExport ? EntryHeader.Declaration?.ShipperAddress?.Header : EntryHeader.Declaration?.Seller;
		public ZString SellerConsignorCaption => EntryHeader.IsExport ? "[2] Consignor [3/7]" : "[2] Seller [3/24]";
		public ZString SellerConsignorFullName => SellerConsignor?.OH_FullName ?? ZString.Empty;
		public ZString SellerConsignorStreetAndNumber => (EntryHeader.IsExport ? EntryHeader.Declaration?.ShipperAddress : EntryHeader.Declaration?.SellerAddress)?.Address1 ?? ZString.Empty;
		public ZString SellerConsignorEORICaption => EntryHeader.IsExport ? "No [3/8]" : "No [3/25]";
		public ZString SellerConsignorEORI => SellerConsignor?.GetEuIdentificationNumber() ?? ZString.Empty;

		JobDocAddress Buyer => buyer ??= GetBuyerDocAddress();
		JobDocAddress buyer;
		JobDocAddress GetBuyerDocAddress()
		{
			var result = EntryHeader.Declaration?.BuyerDocAddress;
			if (result != null && result.IsEmpty)
			{
				var shipmentBuyer = EntryHeader.Declaration?.Shipment?.BuyerDocAddress;
				if (shipmentBuyer != null)
				{
					result = shipmentBuyer;
				}
			}
			return result;
		}
		public ZString BuyerFullName => Buyer?.CompanyName ?? ZString.Empty;
		public ZString BuyerStreetAndNumber => Buyer?.Address1 ?? ZString.Empty;
		public ZString BuyerEORI => Buyer.GetEuIdentificationNumber();

		public ZString RepresentativeFullName => EntryHeader.Declaration?.Representative?.CompanyName ?? ZString.Empty;
		public ZString RepresentativeStreetAndNumber => EntryHeader.Declaration?.Representative?.Address1 ?? ZString.Empty;
		public ZString RepresentativeEORI => EntryHeader.RepresentativeEoriOfMainOffice;

		public ZString Box20DeliveryTerms
		{
			get
			{
				var randomInvoiceHeader = EntryHeader.RandomHeader;
				var declaration = EntryHeader.Declaration;
				var incoTerm = !randomInvoiceHeader.JZ_IncoTerm.IsEmpty ? randomInvoiceHeader.JZ_IncoTerm : declaration.JE_ShipmentIncoTerm;
				var incoTermPlace = !randomInvoiceHeader.JZ_IncoTermPlace.IsEmpty ? randomInvoiceHeader.JZ_IncoTermPlace : declaration.JE_ShipmentIncoTermPlace;
				var separator = !incoTerm.IsEmpty && !incoTermPlace.IsEmpty ? " | " : "";
				return $"{incoTerm}{separator}{incoTermPlace}";
			}
		}

		public ZDecimal Box22InvoiceTotal => EntryHeader.TotalPrice.Amount;
		public ZString Box22InvoiceCurrency => EntryHeader.TotalPrice?.Currency?.Code ?? ZString.Empty;
		public ZString Box25BorderTransportMode => EntryHeader.Declaration.JE_TransportMode;
		public ZString Box26InlandTransportMode => EntryHeader.Declaration.JE_TransportModeInland;
		public ZString Box44OfficeOfPresentation => EntryHeader.Declaration.JE_CustomsOffice;
		public ZString Box44SupervisingOffice => EntryHeader.Declaration.SupervisingOfficeDocAddress.Organisation?.LocalCustomsClientCode ?? ZString.Empty;

		public ZString Box35GrossMass
		{
			get
			{
				var result = EntryHeader.GrossWeight.InKilogramsSafe.Round(3);
				return result.IsEmpty ? (ZString)eadBlankBoxDashes : (ZString)result.ToString();
			}
		}

		public ZString Box22InvoiceTotalCurrency
		{
			get { return EntryHeader.TotalPrice.Currency == null ? "" : EntryHeader.TotalPrice.Currency.Code; }
		}

		public ZString Box31Containers => FormatMultipleLines(EntryHeader.Containers.Select(c => c.CO_ContainerNumber));

		public ZString Box18TransportReference
		{
			get
			{
				ZString result;
				var declaration = EntryHeader.Declaration;
				switch (declaration.JE_TransportMode)
				{
					case Customs.Business.TransportTypeList.Codes.Air:
						result = declaration.JE_VoyageFlightNo + (declaration.JE_ExportDate.IsValid ? " / " + declaration.JE_ExportDate.ToShortDateString() : "");
						break;
					case Customs.Business.TransportTypeList.Codes.Sea:
						result = declaration.JE_VesselName + (declaration.JE_VoyageFlightNo.IsValid ? " / " + declaration.JE_VoyageFlightNo : "");
						break;
					case Customs.Business.TransportTypeList.Codes.Road:
					case Business.CodeDescriptionPairLists.GBTransportTypeList.Codes.ROR:
					case Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport:
					case Customs.Business.TransportTypeList.Codes.OwnPropulsion:
					case Customs.Business.TransportTypeList.Codes.Rail:
						result = declaration.ZG_Box18TransportID;
						break;
					default:
						result = declaration.JE_VesselName;
						break;
				}
				return result.IsEmpty ? (ZString)eadBlankBoxDashes : result;
			}
		}

		public ZString Box40SummaryDeclarationAndPreviousDocsCombined => FormatMultipleLines(EntryHeader.PreviousDocuments
			.Select(d => JoinFields(d.CSI_SubType, d.CSI_Code, d.CSI_ReferenceNumber + (d.CSI_DateOfIssue.IsValid ? "-" + d.DateOfIssueInFormat : string.Empty))));

		public ZString Box45AdditionsAndDeductions => FormatMultipleLines(EntryHeader.CDSChargeDeductions
			.Select(c => JoinFields(c.Key, c.Value.ToString())));

		public ZString Box44AuthorisationHolders => FormatMultipleLines(EntryHeader.EntryInstruction?.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner
			.Select(a => JoinFields(a.AGC_Code, a.AGC_Number)));

		public ZString Box44AdditionalFiscalReferences => FormatMultipleLines(EntryHeader.EntryInstruction?.FiscalReferences
			.Select(f => JoinFields(f.CFR_Code, f.CFR_Reference)));

		IEnumerable<GBGuarantee> Guarantees => EntryHeader.Declaration.Guarantees.Cast<GBGuarantee>().Where(x => x.IsGuarantee && x.IsRelatedToEntryInstruction(EntryHeader.EntryInstruction));

		ZString FormatGuarantee(GBGuarantee guarantee) => guarantee != null ? JoinFields(guarantee.PW_Password, $"{guarantee.PW_BondNumber2} {guarantee.PW_BondNumber}".Trim()) : ZString.Empty;

		public ZString Box52Guarantee1 => FormatGuarantee(Guarantees.ElementAtOrDefault(0));

		public ZString Box52Guarantee2 => FormatGuarantee(Guarantees.ElementAtOrDefault(1));

		public ZString Box52GuaranteesRemaining => FormatMultipleLines(Guarantees.Skip(2).Select(FormatGuarantee));

		public ZString Box49WarehouseID => EntryHeader.EntryInstruction?.WarehouseIDFor27 ?? ZString.Empty;

		public ZDecimal ExchangeRateElement415SadBox23 => EntryHeader.IsMultiInvoiceCurrency ? new ZDecimal(1m) : EntryHeader.RandomHeader.JZ_InvoiceCurrExRate;

		#endregion

		public DocCDSEntryLineWrapperCollection Items
		{
			get { return items ?? (items = new DocCDSEntryLineWrapperCollection(entryHeader)); }
		}
		DocCDSEntryLineWrapperCollection items;

		public static DocCDSEntryHeaderWrapper New(BusinessObject bizO, BusinessObjectFactory factoryToWrap)
		{
			return bizO is CusEntryHeader header ? new DocCDSEntryHeaderWrapper(header, factoryToWrap) : null;
		}

		ZString JoinFields(params ZString[] fields) => ZString.Join(fieldSeparator, fields);

		ZString FormatMultipleLines(IEnumerable<ZString> lines) => lines != null ? new ZStringBuilder(lines).ToStringWithDelimiterBetweenAppends(lineSeparator) : ZString.Empty;

		const string lineSeparator = "\r\n";
		const string fieldSeparator = " | ";
		const string eadBlankBoxDashes = "---";
	}
}
