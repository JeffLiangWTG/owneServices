using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using AdditionalInfo = Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos.AdditionalInfo;
using SupportingDocument = Enterprise.Customs.GB.Business.Declaration.SupportingDocument;

namespace Enterprise.Customs.GB.Business.Messaging
{
	public enum CusDecMessageTypeFunction { Original = 9, Replacement = 5, Delete = 1 }

	/// <summary>
	/// Main translation layer between existing Customs system and GEMS specific SADH messaging system
	/// </summary>
	public abstract class GbHeader : IHeader
	{
		protected GbHeader(CusEntryHeader entryHeader)
		{
			if (entryHeader == null)
			{
				throw new ArgumentNullException(nameof(entryHeader));
			}
			if (entryHeader.Declaration == null)
			{
				throw new ArgumentException("entryHeader has a null Declaration");
			}

			EntryHeader = entryHeader as Declaration.CusEntryHeader;
			declaration = entryHeader.Declaration as Declaration.JobDeclaration;

			if (entryHeader.Declaration == null)
			{
				throw new ArgumentException("entryHeader has a GB Declaration");
			}
		}

		public virtual ZString WarehouseId
		{
			get
			{
				if (declaration != null && declaration.WarehouseAddress != null && declaration.WarehouseAddress.Header != null && declaration.WarehouseAddress.Header.CustomsCodes != null)
				{
					return declaration.WarehouseAddress.Header.CustomsCodes.GetCustomsRegNo(declaration.CusEntryInstruction.WarehouseTypeCode, Core.Constants.CountryCodes.UnitedKingdom, declaration.WarehouseAddress.PK);
				}
				return ZString.Empty;
			}
		}

		public ZString InvoiceCurrency // INV-CRRN.  As of Chief 31A this is applicable to both imports and exports.
		{
			get
			{
				ZString result = null;
				if (EntryHeader.IsMultiInvoiceCurrency)
				{
					result = Core.Constants.CurrencyCodes.UnitedKingdom;
				}
				else
				{
					if (EntryHeader.MergedLines.Count > 0)
					{
						RefCurrency currency = (from CusEntryLine l in EntryHeader.MergedLines where l.CL_CustomsPostedStatus == Enterprise.Customs.Business.EntryLineStatusList.Codes.Active select l.InvoiceCurrency).FirstOrDefault();
						result = currency != null ? currency.RX_Code : ZString.Empty;
					}
				}
				return result;
			}
		}

		public bool IsInventoryControlledAirImport
		{
			get
			{
				return declaration.IsInventoryControlledAirImport;
			}
		}

		#region Get date time according to chief format mask
		public static ZDateTime GetDateFromChiefFormat(string dateAsString, string dateFormatKey)
		{
			// http://unece.org/trade/untdid/d01a/tred/tred2379.htm
			ZDateTime result = ZDateTime.Empty;

			switch (dateFormatKey)
			{
				case "203": //CCYYMMDDHHMM Calendar date including time with minutes: 
					ZDateTime.TryParseExact(dateAsString, out result, "yyyyMMddHHmm");
					break;

				case "102": //CCYYMMDD    Calendar date 
					ZDateTime.TryParseExact(dateAsString, out result, "yyyyMMdd");
					break;
			}
			return result;
		}

		public static string GetDateStringInChiefFormat(ZDateTime date, int dateFormatKey)
		{
			// http://unece.org/trade/untdid/d01a/tred/tred2379.htm

			switch (dateFormatKey)
			{
				case 203: //CCYYMMDDHHMM Calendar date including time with minutes: 
					return date.ToString("yyyyMMddHHmm");

				case 102: //CCYYMMDD    Calendar date 
					return date.ToString("yyyyMMdd");
			}
			return date.ToShortDateString();  // locale-specific
		}

		#endregion

		public ZString GovernmentContractorTurn
		{
			get
			{
				var result = GovernmentContractor != null ? GovernmentContractor.EoriCode : ZString.Empty;
				if (result.Length >= 2 && result.Left(2).IsLettersOnlyOrEmpty)
				{
					result = result.Substring(2);
				}
				return result;
			}
		}

		public ZString RegisteredConsigneeTurn
		{
			get
			{
				var result = ZString.Empty;
				var fiscalReference = EntryHeader.FiscalReferences.FirstOrDefault();
				if (fiscalReference != null)
				{
					result = fiscalReference.CFR_Reference;
					if (result.Length >= 2 && result.Left(2).IsLettersOnlyOrEmpty)
					{
						result = result.Substring(2);
					}
				}

				return result;
			}
		}

		public ZString RegisteredConsignorTurn
		{
			get
			{
				if (ConsigneeImporter != null)
				{
					return ConsigneeImporter.EoriCode;
				}
				return ZString.Empty;
			}
		}

		public virtual ZString GetMessageCode(CusDecMessageTypeFunction type)
		{
			switch (type)
			{
				case CusDecMessageTypeFunction.Original:
				case CusDecMessageTypeFunction.Replacement:
					return MESSAGE_CODE;
				case CusDecMessageTypeFunction.Delete:
					return "XTC";
			}
			throw new ArgumentOutOfRangeException(nameof(type), "Type must be 9, 5, or 1.");
		}

		public ZString MESSAGE_CODE
		{
			get
			{
				return EntryHeader?.EntryInstruction?.CEI_Style ?? string.Empty;
			}
		}

		public ZString FIR_DAN
		{
			get { return this.EntryHeader.Declaration.JE_DefermentAccountNumber; }
		}

		public ZString FIR_DAN_PFX
		{
			get { return EntryHeader.Declaration.JE_PaymentMethod; }
		}

		public virtual ZString DECLN_CRRN
		{
			get
			{
				return ZString.Empty; // Change for GBP or EUR when relevant
			}
		}

		public ZString DECLT_REP
		{
			get
			{
				// use turn codes as this will identify the party uniquely
				return declaration.RepresentationTypeNo;//
			}
		}

		// ECS stuff:
		public List<ZString> CountriesOfRouting  //ROUTE-CNTRY (many-of)
		{
			get
			{
				return EntryHeader.CountriesOfRouting;
			}
		}

		public Declaration.CusEntryHeader EntryHeader { get; private set; }

		protected Declaration.JobDeclaration declaration;

		public abstract ZString JobType { get; }

		public ZString JobMode
		{
			get { return EntryHeader.Declaration.IsAir ? "A" : "O"; }
		}

		public ZString EntryMessageType
		{
			get { return EntryHeader.Declaration.CusEntryInstruction.CEI_Style; }
		}

		public ZString TradersOwnReference // TDR-OWN-REF-ENT
		{
			get
			{
				// This is almost the same as JobDeclaration.TradersOwnReference but we have to use PLACEHOLDERS here
				return JobDeclaration.DeclarationReferencePlaceHolder;
			}
		}

		public ZString JobReferenceGemsOnly
		{
			get
			{
				return JobDeclaration.DeclarationReferencePlaceHolder;
			}
		}

		protected abstract IDocAddress Customer { get; }

		public IDocAddress ConsigneeDocAddress { get { return EntryHeader.Declaration.ImporterDocumentaryAddress; } }
		public IDocAddress ShipperDocAddress { get { return EntryHeader.Declaration.SupplierDocumentaryAddress; } }

		public ZString CustomerAccountNumber { get { return GetAccountNumber(Customer); } }
		public ZString CustomerShortCode { get { return Customer == null || Customer.Organisation == null ? ZString.Empty : Customer.Organisation.Code; } }

		public ZString CustomerName { get { return Customer == null ? ZString.Empty : Customer.E2_CompanyNameTruncated; } }
		public ZString CustomerAddress1 { get { return Customer == null ? ZString.Empty : Customer.E2_Address1; } }
		public ZString CustomerAddress2 { get { return Customer == null ? ZString.Empty : Customer.E2_Address2; } }
		public ZString City { get { return Customer == null ? ZString.Empty : Customer.E2_City; } }
		public ZString State { get { return Customer == null ? ZString.Empty : Customer.E2_State; } }
		public ZString PostCode { get { return Customer == null ? ZString.Empty : Customer.E2_Postcode; } }
		public ZString CountryCode { get { return Customer == null ? ZString.Empty : Customer.CountryCode; } }
		public ZString CustomerTelephone { get { return Customer == null ? ZString.Empty : Customer.E2_Phone; } }
		public ZString CustomerFax { get { return Customer == null ? ZString.Empty : Customer.E2_Fax; } }

		public ZString ShipperAccountNumber { get { return GetAccountNumber(ShipperDocAddress); } }
		public ZString ShipperShortCode { get { return ShipperDocAddress == null || ShipperDocAddress.Organisation == null ? ZString.Empty : ShipperDocAddress.Organisation.Code; } }
		public ZString ShipperName { get { return ShipperDocAddress == null ? ZString.Empty : ShipperDocAddress.E2_CompanyNameTruncated; } }
		public ZString ShipperAddress1 { get { return ShipperDocAddress == null ? ZString.Empty : ShipperDocAddress.E2_Address1; } }
		public ZString ShipperAddress2 { get { return ShipperDocAddress == null ? ZString.Empty : ShipperDocAddress.E2_Address2; } }
		public ZString ShipperCity { get { return ShipperDocAddress == null ? ZString.Empty : ShipperDocAddress.E2_City; } }
		public ZString ShipperState { get { return ShipperDocAddress == null ? ZString.Empty : ShipperDocAddress.E2_State; } }
		public ZString ShipperPostCode { get { return ShipperDocAddress == null ? ZString.Empty : (ShipperDocAddress.E2_Postcode.IsEmpty ? new ZString("NA") : ShipperDocAddress.E2_Postcode); } }
		public ZString ShipperCountryCode { get { return ShipperDocAddress == null ? ZString.Empty : ShipperDocAddress.CountryCode; } }
		public ZString ShipperTelephone { get { return ShipperDocAddress == null ? ZString.Empty : ShipperDocAddress.E2_Phone; } }
		public ZString ShipperFax { get { return ShipperDocAddress == null ? ZString.Empty : ShipperDocAddress.E2_Fax; } }
		public ZString ShipperTURNNumber { get { return ShipperDocAddress == null ? ZString.Empty : ((OrgHeader)ShipperDocAddress.Organisation).GetEuIdentificationNumber(); } }

		public ZString ConsigneeAccountNumber { get { return GetAccountNumber(ConsigneeDocAddress); } }
		public ZString ConsigneeShortCode { get { return ConsigneeDocAddress == null || ConsigneeDocAddress.Organisation == null ? ZString.Empty : ConsigneeDocAddress.Organisation.Code; } }
		public ZString ConsigneeName { get { return ConsigneeDocAddress == null ? ZString.Empty : ConsigneeDocAddress.E2_CompanyNameTruncated; } }
		public ZString ConsigneeAddress1 { get { return ConsigneeDocAddress == null ? ZString.Empty : ConsigneeDocAddress.E2_Address1; } }
		public ZString ConsigneeAddress2 { get { return ConsigneeDocAddress == null ? ZString.Empty : ConsigneeDocAddress.E2_Address2; } }
		public ZString ConsigneeCity { get { return ConsigneeDocAddress == null ? ZString.Empty : ConsigneeDocAddress.E2_City; } }
		public ZString ConsigneeState { get { return ConsigneeDocAddress == null ? ZString.Empty : ConsigneeDocAddress.E2_State; } }
		public ZString ConsigneePostCode { get { return ConsigneeDocAddress == null ? ZString.Empty : (ConsigneeDocAddress.E2_Postcode.IsEmpty ? new ZString("NA") : ConsigneeDocAddress.E2_Postcode); } }
		public ZString ConsigneeCountryCode { get { return ConsigneeDocAddress == null ? ZString.Empty : ConsigneeDocAddress.CountryCode; } }
		public ZString ConsigneeTelephone { get { return ConsigneeDocAddress == null ? ZString.Empty : ConsigneeDocAddress.E2_Phone; } }
		public ZString ConsigneeFax { get { return ConsigneeDocAddress == null ? ZString.Empty : ConsigneeDocAddress.E2_Fax; } }

		IOrganisation IHeader.Warehouse
		{
			get { return OrganisationProvider.Get(declaration.WarehouseAddress); }
		}

		IOrganisation IHeader.NotifyParty
		{
			get { return OrganisationProvider.Get(declaration.NotifyPartyDocumentaryAddress); }
		}

		public ZString ArrivalIATA { get { return declaration.IsAir ? declaration.JE_RL_NKPortOfArrival.SubstringSafe(2, 3) : ZString.Empty; } }
		public ZString FlightNumber { get { return declaration.IsAir ? declaration.JE_VoyageFlightNo : ZString.Empty; } }
		public ZString VesselName { get { return declaration.IsSea ? declaration.JE_VesselName : ZString.Empty; } }
		public ZDate FlightDate { get { return declaration.IsAir ? declaration.JE_ExportDate.Date : ZDate.Empty; } }
		public ZString IATAPortOfOrigin { get { return declaration.JE_RL_NKOrigin.SubstringSafe(2, 3); } }
		public ZString IATAPortOfDestination { get { return declaration.JE_RL_NKFinalDestination.SubstringSafe(2, 3); } }
		public ZString AirportOfLoading { get { return declaration.IsAir ? declaration.JE_RL_NKPortOfLoading.SubstringSafe(2, 3) : ZString.Empty; } }
		public ZString PortOfDeparture { get { return declaration.JE_RL_NKPortOfLoading.SubstringSafe(2, 3); } }
		public virtual ZString CountryOfDeparture { get { return declaration.JE_RL_NKPortOfLoading.Left(2); } }
		public virtual ZString CountryOfArrival { get { return declaration.JE_RL_NKPortOfArrival.Left(2); } }

		public ZInt ExpectedNumberOfPackages { get { return declaration.JE_TotalNoOfPacks; } }
		public ZDecimal GrossWeightInKilograms { get { return EntryHeader.GrossWeight.InKilogramsSafe; } }
		public ZDecimal ChargeableWeightInKilograms { get { return 0m; } } // not supported by Cargowise
		public ZDecimal VolumeInCubicMetres { get { return new ZVolume(declaration.JE_TotalVolume, declaration.JE_TotalVolumeUnit).InCubicMetres; } }
		public ZString GoodsDescription { get { return declaration.JE_GoodsDescription; } }

		public ZString DestinationCountry { get { return declaration.JE_GoodsDestination; } }

		public ZString OriginCountry { get { return declaration.JE_GoodsOrigin; } }

		public ZString CustomerClientKey { get { return Customer == null || Customer.Organisation == null ? ZString.Empty : ((OrgHeader)Customer.Organisation).GetEuIdentificationNumber(); } }

		public ZString Owner { get { return declaration.JE_CustomsProfile; } }

		public ZString ShedPhysicalCodeFromDatabaseOrHeathrowERT => declaration.ShedPhysicalCodeFromDatabaseOrHeathrowERT;

		public ZString ShedCode
		{
			get { return declaration.SubLocation; }
		}

		public ZString SubLocationOfGoods { get { return declaration.JE_SubLocationOfGoods.Left(3); } }

		public ZString HouseBill
		{
			get
			{
				/*
				Sent: Thursday, 13 November 2008 10:41 PM
				To: Brett Shearer
				Cc: helpdesk@asm.org.uk
				Subject: RE: Variance of lengths of CHouse through GEMS interface

				Brett


				All HAWB records on the inventory system (CCSUK) are eight digits. Industry practice for HAWB’s of a different length are.
				1	HAWB of less than eight digits are padded with preceding zero’s. IE actual HAWB 1234 would become 00001234.
				2	HAWB of more than eight digits. Industry normally uses the last eight digits IE actual HAWB12345678123456 would become 78123456.

				I hope the above explains industry practice.

				Best regards,

				Kevin Marshall-Jobson

				Industry Support Specialist | Agency Sector Management (UK) Limited | www.asm.org.uk

				*/
				return declaration.IsAir ? LeftPadHouseBillWithZeros(declaration.JE_HouseBill) : declaration.JE_HouseBill;
			}
		}

		ZString LeftPadHouseBillWithZeros(ZString dirtyHawb)
		{
			return !dirtyHawb.IsEmpty && dirtyHawb.Length < 8 ? dirtyHawb.PadLeft(8, '0') : dirtyHawb;
		}

		public ZString AirMasterBill { get { return declaration.IsAir ? declaration.JE_MasterBill : ZString.Empty; } }

		public ZString ShipmentType { get { return declaration.ZG_ShipmentType; } }

		#region IHeader Members

		public ZDateTime DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises
		{
			get { return declaration.ZG_LCPInspect; }
		}

		public ZDateTime DateAndTimeTheGoodsWillBeLeavingLCPPremises
		{
			get { return declaration.ZG_LCPDepart; }
		}

		public ZDate GoodsDepartureDate
		{
			get { return declaration.JE_ExportDate.Date; }
		}

		public ZDateTime TaxPointDateAndTime
		{
			get { return declaration.JE_EntryAuthorisationDate; }
		}

		public IOrganisation ConsignorExporter
		{
			get { return OrganisationProvider.Get(ShipperDocAddress); }
		}

		public IOrganisation ConsigneeImporter
		{
			get { return OrganisationProvider.Get(ConsigneeDocAddress); }
		}

		public IOrganisation Premises
		{
			get { return OrganisationProvider.Get(declaration.WarehouseDocAddress?.Organisation); }
		}

		public ZString TypeOfRepresentation
		{
			get { return declaration.RepresentationTypeNo; }
		}

		public IOrganisation DeclarantOrRepresentative
		{
			get
			{
				if (declaration.Declarant != null)
				{
					return OrganisationProvider.Get(declaration.Declarant);
				}
				return OrganisationProvider.Get(declaration.Branch.OrgProxy);
			}
		}

		public virtual ZString CountryOfExport
		{
			get { return declaration.JE_GoodsOrigin; }
		}

		public virtual ZString CountryOfDestination
		{
			get { return declaration.JE_GoodsDestination; }
		}

		public ZString TransportIdentityOnDepartureBox18
		{
			get
			{
				switch (declaration.JE_TransportMode.ToString())
				{
					case Customs.Business.TransportTypeList.Codes.Air:
						return declaration.JE_VoyageFlightNo + " " + declaration.JE_ExportDate.ToString("dd/MM/yyyy");
					case Customs.Business.TransportTypeList.Codes.Sea:
						return !declaration.JE_VesselName.IsEmpty ? declaration.JE_VesselName : (ZString)"";
					default:
						return declaration.ZG_Box18TransportNationality + " " + declaration.ZG_Box18TransportID;
				}
			}
		}

		public ZString Box18TransportID => declaration.ZG_Box18TransportID;

		public ZString TransportNationalityOnDepartureBox18
		{
			get { return declaration.ZG_Box18TransportNationality; }
		}

		// Box 21 - TRPT-ID
		public ZString TransportIdentityAtTheBorderBox21
		{
			get
			{
				if (TransportModeAtTheBorderBox25 == ModeOfTransportList.Codes._4_AirTransport)
				{
					return declaration.JE_VoyageFlightNo;
				}
				else
				{
					return declaration.JE_VesselName;
				}
			}
		}

		// Box 21 - TRPT-CNTRY
		public virtual ZString TransportNationalityAtTheBorderBox21
		{
			get { return declaration.JE_RN_NKTransportNationality; }
		}

		public ZBool FECTransportNationalityAtTheBorder
		{
			get { return declaration.JE_FecFLG; }
		}

		// Box 25 - TRPT-MODE-CODE
		public ZString TransportModeAtTheBorderBox25
		{
			get { return declaration.ModeOfTransportAtTheBorder; }
		}

		// Box 26 TRPT-MODE-INLD
		public ZString TransportModeInlandBox26
		{
			get { return declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland, true); }
		}

		public ZString LocationOfGoods => declaration.LocationOfGoodsForDocumentsAndMessaging;

		public ZString LocationOfGoodsWithoutGbPrefix => declaration.LocationOfGoodsWithoutGbPrefix;

		public IEnumerable<ILine> Lines
		{
			get
			{
				foreach (var entryLine in (from CusEntryLine cel in EntryHeader.AllEntryLines orderby cel.CL_LineNumber select cel))  // All entry lines, not just MergedLInes, to cope with 'deleted' ones too
				{
					yield return GetNewGbLine(entryLine);
				}
			}
		}

		protected abstract GbLine GetNewGbLine(CusEntryLine entryLine);

		public IEnumerable<IStatement> Statements
		{
			get
			{
				var additonalInformatyionStatementsThatShouldNotbeSentToChief = new List<string>() { "RCONR", "RCONE", "PREMS", "GCONT" };  // only for putting on the C88
				foreach (AdditionalInfo info in EntryHeader.AdditionalInfos)
				{
					if (!additonalInformatyionStatementsThatShouldNotbeSentToChief.Contains(info.CSI_Code))
					{
						yield return new Statement(info);
					}
				}

				if (GB.Registry.GBCustomsDataRegistry.Instance.Eori_SendAiStatementsForEoriSuffixes.Value)
				{
					foreach (Statement aiEori in GetShipperConsigneeDeclarantAiStatementsForEori(EntryHeader))
					{
						yield return aiEori;
					}
				}
			}
		}

		List<Statement> GetShipperConsigneeDeclarantAiStatementsForEori(CusEntryHeader entryHeader)
		{
			List<Statement> list = new List<Statement>();
			List<EoriSuffixAndType> branchSuffixes = EuEoriProviderAndValidator.GetBranchSuffixesListForBox44(entryHeader);
			foreach (EoriSuffixAndType suffixPair in branchSuffixes)
			{
				// Makes a new AI in form HDR-AI-STMT="AG123", HDR-AI-STMT-TXT=""
				Statement s = new Statement(suffixPair.Type + suffixPair.Number, string.Empty);
				list.Add(s);
			}
			return list;
		}

		public ZString DeclarationCurrency
		{
			get { return ZString.Empty; } // This is only to be entered if the UK joins the EURO otherwise do not enter this information.
		}

		public IEnumerable<ISupportingDocument> SupportingDocuments
		{
			get
			{
				foreach (SupportingDocument info in EntryHeader.SupportingDocuments)
				{
					yield return new Document(info);
				}
			}
		}

		public IEnumerable<IPreviousDocument> PreviousDocuments
		{
			get
			{
				foreach (PreviousDocument info in EntryHeader.PreviousDocuments)
				{
					yield return new PreviousDocumentWrapper(info);
				}
			}
		}

		public virtual ZString DeclarationUniqueConsignmentReference
		{
			get { return CusEntryHeader.UCRReferencePlaceHolder; }
		}

		public virtual ZString DeclarationUniqueConsignmentReferencePartSuffix
		{
			get { return CusEntryHeader.UCRPartPlaceHolder; }
		}

		public IOrganisation SupervisingOffice
		{
			get { return OrganisationProvider.Get(declaration.SupervisingOfficeDocAddress); }
		}

		public ZString MasterUniqueConsignmentReference
		{
			get { return EntryHeader.CH_MasterUCR.IsEmpty ? declaration.JE_MasterUCR : EntryHeader.CH_MasterUCR; }
		}

		public ZString ThirdPartyApplReference
		{
			get { return JobDeclaration.DeclarationReferencePlaceHolder; }
		}

		public IOrganisation GovernmentContractor
		{
			get { return OrganisationProvider.Get(declaration.GovernmentContractorDocAddress); }
		}

		public ZString DeclarationType
		{
			get { return declaration.JE_EntryStyle + declaration.JE_EntrySubStyle; }
		}

		public ZString TransportChargesMethodOfPayment
		{
			get
			{
				var transportChargesMOPs = EntryHeader?.MergedLines
					.SelectMany(x => x.InvoiceLines
					.Select(y => ((JobComInvoiceHeader)y.InvoiceHeader).ZG_TransportChargesMethodOfPayment))
					.Where(z => !z.IsEmpty)
					.Distinct().ToList();

				return transportChargesMOPs.Count == 1 ? transportChargesMOPs.First() : ZString.Empty;
			}
		}

		public ZString CarrierName
		{
			get { return declaration.ShippingLine != null ? declaration.ShippingLine.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZString PlaceOfArrivalInTheEU
		{
			get { return declaration.JE_RL_NKPortOfArrival; }
		}

		public ZString OfficeOfExit => declaration.OfficeOfExit;

		public ZDateTime IntendedDateAndTimeOfArrivalAtThePlaceOfArrival
		{
			get { return declaration.JE_DateOfArrival; }
		}

		public IEnumerable<ISeal> Seals
		{
			get
			{
				foreach (CusContainer container in declaration.CusContainers)
				{
					if (!container.CO_Seal.IsEmpty)
					{
						yield return new Seal(container.CO_Seal);
					}
					if (!container.CO_SecondSeal.IsEmpty)
					{
						yield return new Seal(container.CO_SecondSeal);
					}
				}
			}
		}

		public ZBool FECAcceptanceReport
		{
			get
			{
				return GB.Registry.GBCustomsDataRegistry.Instance.ProcInst_ACC.Value;
			}
		}

		public ZBool FECExpectedTransportNationalityAtTheBorder
		{
			get { return EntryHeader.FECExpectedTransportNationalityAtTheBorder; }
		}

		public ZBool FECRouteF
		{
			get { return EntryHeader.Declaration.JE_RouteFRequested; }
		}

		ZInt IHeader.TotalPackages
		{
			get { return TotalPackages; }
		}

		ZString IHeader.AmendmentReason
		{
			get { return EntryHeader.CH_CustomsMessageRemarks; }
		}

		#endregion

		public ZString PlaceOfLoading
		{
			get { return declaration.PortOfLoading == null ? ZString.Empty : declaration.PortOfLoading.Code; }
		}

		public ZString PlaceOfUnLoading
		{
			get { return declaration.PortOfArrival == null ? ZString.Empty : declaration.PortOfArrival.Code; }
		}

		public ZInt TotalPackages
		{
			get { return EntryHeader.PackagesCount; }
		}

		public ZString AmendmentReason
		{
			get { return EntryHeader.CH_CustomsMessageRemarks; }
		}

		public ZDateTime DeclarationDate
		{
			get { return declaration.JE_EntryAuthorisationDate.IsEmpty ? ZDateTime.Now : declaration.JE_EntryAuthorisationDate; }
		}

		public ZString CommercialReferenceNumber
		{
			get { return declaration.JE_OwnerRef; }   // box 7 ref
		}

		ZString GetAccountNumber(IDocAddress docAddress)
		{
			OrgHeader orgHeader = docAddress.Organisation as OrgHeader;
			if (orgHeader == null)
			{
				return ZString.Empty;
			}
			return orgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.UnitedKingdomCodeTypes.GemsCustomerCode, Core.Constants.CountryCodes.UnitedKingdom);
		}

		public static RequirementLevel LocationOfGoodsRequirementLevelShared(Declaration.JobDeclaration declaration)
		{
			// See DES205 and DES208. 
			if (declaration == null)
			{
				return RequirementLevel.Unrecognised;
			}

			// imports
			if (declaration.IsICR || declaration.IsIFD)
			{
				return RequirementLevel.Mandatory;
			}
			else if (declaration.IsIFW || declaration.IsISW)
			{
				return RequirementLevel.NotAllowed;
			}
			else if (declaration.IsISD)
			{
				return declaration.IsFSD ? RequirementLevel.Optional : RequirementLevel.Mandatory;
			}
			// exports
			else if (declaration.IsELP || declaration.IsESD || declaration.IsECR || declaration.IsEXS)
			{
				return RequirementLevel.Mandatory;
			}
			else if (declaration.IsESP || declaration.IsEFD)
			{
				return RequirementLevel.Optional;
			}
			return RequirementLevel.Unrecognised;
		}

		public RequirementLevel LocationOfGoodsRequirementLevel
		{
			get
			{
				return LocationOfGoodsRequirementLevelShared(this.declaration);
			}
		}

		public bool IsSFD { get { return declaration.IsSFD; } }
		public bool IsFSD { get { return declaration.IsFSD; } }
		public bool IsICR { get { return declaration.IsICR; } }
		public bool IsIFD { get { return declaration.IsIFD; } }
		public bool IsIFW { get { return declaration.IsIFW; } }
		public bool IsISD { get { return declaration.IsISD; } }
		public bool IsISW { get { return declaration.IsISW; } }

		public bool IsESD { get { return declaration.IsESD; } }
		public bool IsELP { get { return declaration.IsELP; } }
		public bool IsEFD { get { return declaration.IsEFD; } }
		public bool IsECR { get { return declaration.IsECR; } }
		public bool IsESP { get { return declaration.IsESP; } }
		public bool IsEXS { get { return declaration.IsEXS; } }
	}
}
