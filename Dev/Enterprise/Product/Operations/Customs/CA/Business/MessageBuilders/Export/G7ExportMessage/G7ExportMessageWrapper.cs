namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.CA.Registry;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Customs.Universal;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Integration;

	public class G7ExportMessageWrapper : IG7Export
	{
		public G7ExportMessageWrapper(CusEntryHeader entryHeader)
		{
			CanSendDeclarationChecker.EntryNotNullAndAttachedToDeclaration(entryHeader);
			this.entryHeader = entryHeader;
			declaration = entryHeader.Declaration;
		}

		#region IG7Export Members

		public void AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			entryHeader.Messages.Add(message);
		}

		public ZString JobIdentification
		{
			get { return declaration.JE_DeclarationReference; }
		}

		public ZString MessageStatus
		{
			get { return entryHeader.CH_Status; }
			set { entryHeader.CH_Status = value; }
		}

		public ZString JobStatus
		{
			get { return entryHeader.CH_EntryStatus; }
			set { entryHeader.CH_EntryStatus = value; }
		}

		public bool HasChanges
		{
			get { return declaration.HasChanges; }
		}

		bool ICAEDIFACTMessageAttachee.IsCancelled
		{
			get { return declaration.IsCancelled; }
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage
		{
			get { return declaration.RefreshValidationBeforeSendMessage; }
		}

		public BusinessObjectFactory Factory
		{
			get { return declaration.Factory; }
		}

		public BusinessObject TopLevelBusinessObject
		{
			get { return declaration; }
		}

		public Enterprise.Messaging.Business.EDIMessageCollection Messages
		{
			get { return entryHeader.Messages; }
		}

		public ZString DocumentMessageNumber
		{
			get { return declaration.JE_DeclarationReference; }
		}

		public ZString ExportLicenceNumber
		{
			get { return entryHeader.ExportLicenceNumber; }
		}

		public OrgHeader ExportLicenceProxy
		{
			get { return declaration.ExportLicenceProxy; }
		}

		public ZString CAEDAuthorizationID
		{
			get { return entryHeader.CAEDAuthorizationID; }
		}

		public ZString PortOfExit
		{
			get { return IsValidPortCode(declaration.CA_PortOfExit) ? declaration.CA_PortOfExit : ZString.Empty; }
		}

		public ZString PlaceOfReport
		{
			get { return IsValidPortCode(declaration.CA_PlaceOfReport) ? declaration.CA_PlaceOfReport : ZString.Empty; }
		}

		bool IsValidPortCode(ZString port)
		{
			return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, port, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today) != null;
		}

		public ZDateTime DateOfExport
		{
			get { return declaration.JE_ExportDate; }
		}

		public ZDecimal CommodityGrossWeight
		{
			get { return declaration.JE_TotalWeight; }
		}

		public ZString CommodityGrossWeightUnitOfMeasure
		{
			get { return declaration.JE_TotalWeightUnit; }
		}

		public IEnumerable<IG7Container> Containers
		{
			get { return entryHeader.Containers.Cast<IG7Container>(); }
		}

		public ZString ModeOfTransport
		{
			get { return declaration.Lookups.CBSATransportTypeList.GetDescriptionFromCode(declaration.JE_TransportMode); }
		}

		public ZString VesselName
		{
			get { return !declaration.IsSea ? ZString.Empty : declaration.JE_VesselName; }
		}

		public ZString CarrierCode
		{
			get { return declaration.ExportCarrierCode; }
		}

		public ZString CarrierName
		{
			get { return CarrierCode.IsEmpty && declaration.G7ExportCarrier != null ? declaration.G7ExportCarrier.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZString TransactionNumber
		{
			get { return EDIMessage.EntryNumberPlaceHolder; }
		}

		public ZString TransportationDocumentNumber
		{
			get { return declaration.CA_TransportDocumentNumber; }
		}

		public ZInt NumberOfPackages
		{
			get { return declaration.JE_TotalNoOfPacks; }
		}

		public ZString TypeOfPackages
		{
			get { return declaration.JE_TotalNoOfPacksPackType; }
		}

		public ZString ServiceOption
		{
			get { return ServiceOptions.Codes.G7EDIExport; }
		}

		public IDocAddress Exporter
		{
			get { return declaration.SupplierAddress; }
		}

		public ZString ExporterBusinessNumber
		{
			get
			{
				var supplier = declaration.Supplier;
				if (supplier == null)
				{
					return ZString.Empty;
				}
				return supplier.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.Canada, OrgCusCode.CACodeTypes.BusinessNumberForExport, OrgCusCode.CACodeTypes.BusinessNumberForImportExport);
			}
		}

		IDocAddress IG7Export.DeliveryParty
		{
			get { return declaration.ImporterAddress; }
		}

		ZString IG7Export.DeliveryPartyBusinessNumber
		{
			get
			{
				var importer = declaration.Importer;
				return importer == null ? ZString.Empty
						: importer.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.BusinessNumberForImportExport);
			}
		}

		public ZString CountryOfFinalDestination
		{
			get { return declaration.FinalDestination == null ? ZString.Empty : declaration.FinalDestination.RL_RN_NKCountryCode; }
		}

		public ZString BrokerSecurityNumber
		{
			get
			{
				if (declaration.IsCompanyOrgProxyTheExporter)
				{
					return ZString.Empty;
				}

				var result = CACustomsDataRegistry.Instance.AccountSecurityNo.Value;
				if (string.IsNullOrEmpty(result))
				{
					result = GlbStaff.CurrentUser.Certificates.GetFirstCertificateNumber(CertificateTypePairList.Codes.BR1);
				}

				return result;
			}
		}

		public ZDecimal InvoiceTotal
		{
			get { return declaration.TotalInvoiceAmount.Amount; }
		}

		public ZString InvoiceCurrencyCode
		{
			get { return declaration.TotalInvoiceAmount.Currency.Code; }
		}

		public ZDecimal FreightChargesInCAD
		{
			get { return entryHeader.MergedLines.Sum(l => l.OverseasFreightInLocalCurrency.Amount); }
		}

		public IEnumerable<ZString> References
		{
			get { return entryHeader.InvoiceHeaders.Select(invoice => invoice.JZ_InvoiceNumber); }
		}

		public ZString ReasonForExport
		{
			get { return declaration.CA_ReasonForExportCode; }
		}

		public IDocAddress Vendor
		{
			get { return declaration.VendorDocAddress; }
		}

		public IDocAddress Consignee
		{
			get { return declaration.ImporterAddress; }
		}

		public ZString Authentication
		{
			get { return ZString.Empty; } //TODO: what is this?
		}

		public IEnumerable<IG7ItemLine> Details
		{
			get { return (from CusEntryLine entryLine in entryHeader.MergedLines select (IG7ItemLine)new G7ItemLine(entryLine, Exporter)); }
		}

		#region G7ItemLine

		class G7ItemLine : IG7ItemLine
		{
			public G7ItemLine(CusEntryLine entryLine, IDocAddress exporter)
			{
				this.entryLine = entryLine;
				this.exporter = exporter;
			}

			public ZString CountryOfOrigin
			{
				get { return entryLine.RandomLine.EffectiveCountryOfOrigin; }
			}

			public ZString ProvinceOfOrigin
			{
				get
				{
					var result = entryLine.RandomLine.EffectiveProvinceOfOrigin;
					if (result.IsEmpty)
					{
						if (exporter != null && exporter.CountryCode == Core.Constants.CountryCodes.Canada)
						{
							result = exporter.E2_State;
						}
					}
					return result;
				}
			}

			public IEnumerable<ZString> VINs
			{
				get { return entryLine.RandomLine.CA_ConveyanceIdentificationNumber.Split(',', ' '); }
			}

			public IEnumerable<ZString> Permits
			{
				get { return entryLine.Permits; }
			}

			public ZString ProductDescription
			{
				get { return entryLine.EffectiveDescription; }
			}

			public ZInt InvoiceLineNumber
			{
				get { return 0; }
			}

			public ZString ClassificationNumber
			{
				get { return entryLine.CL_AdValoremTariff; }
			}

			public ZDecimal Quantity
			{
				get { return ShouldUseInvoiceQuantityAndUnit ? entryLine.InvoiceQuantity : entryLine.CustomsQuantity; }
			}

			public ZString UnitOfMeasure
			{
				get { return ShouldUseInvoiceQuantityAndUnit ? entryLine.InvoiceUQ : entryLine.CustomsUnitQty; }
			}

			bool ShouldUseInvoiceQuantityAndUnit
			{
				get { return entryLine.CustomsUnitQty.IsEmpty; }
			}

			public ZDecimal CustomsValue
			{
				get { return entryLine.FOB.Amount; }
			}

			public ZString CurrencyCode
			{
				get { return entryLine.FOB.Currency.Code; }
			}

			readonly CusEntryLine entryLine;
			readonly IDocAddress exporter;
		}

		#endregion

		#endregion

		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
	}
}
