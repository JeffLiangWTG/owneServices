using Enterprise.ZArchitecture.Core;
using Res = Resources.Res;
using ResString = Resources.ResString;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgConstants
	{
		#region Address Lists

		public static class AddressType
		{
			public const string Documentary = "DOC";
			public const string Office = "OFC";
			public const string Postal = "PST";
			public const string Receivables = "ARM";
			public const string Payables = "APM";
			public const string Pickup = "PIC";
			public const string Delivery = "DLV";
			public const string PickupAndDelivery = "PAD";
			public const string Sales = "SQM";
			public const string Miscellaneous = "MSC";
			public const string Residential = "RSD";
			public const string CustomsAddressOfRecord = "CST";
			public const string AWB = "AWB";
			public const string EUCustomsAddress = "ECA";
		}

		public static class AccessPoint
		{
			public static class Code
			{
				public const string Dock = "DCK";
				public const string Rack = "RCK";
				public const string Interior = "INT";
				public const string InteriorViaElevator = "IVE";
				public const string InteriorViaStairs = "IVS";
				public const string Other = "OTH";
			}
		}

		public static class CommunicationRequired
		{
			public static class Code
			{
				public const string Appointment = "APP";
				public const string CallBefore = "CAL";
				public const string NotifyBefore = "NOT";
				public const string SeeNotes = "OTH";
			}
		}

		public static class ContainerHandling
		{
			public static class Code
			{
				public const string PackAndUnpack = "PU";
				public const string PackOnly = "PKO";
				public const string UnpackOnly = "UPO";
				public const string NoPackOrUnpack = "NPU";
				public const string DropAndPull = "DP";
				public const string Ask = "ASK";
				public const string Other = "OTH";
			}
		}

		public static class DockHeight
		{
			public static class Code
			{
				public const string Standard = "STD";
				public const string NonStandard = "NON";
				public const string Other = "OTH";
			}
		}

		public static class LabourRequired
		{
			public static class Code
			{
				public const string Yes = "YES";
				public const string No = "NO";
				public const string Ask = "ASK";
				public const string FCLOnly = "FCL";
				public const string OutOfGauge = "OOG";
				public const string HeavyPieces = "HVP";
				public const string Other = "OTH";
			}
		}

		#endregion

		#region Credit Rating

		public static class ARCreditRating
		{
			public static class Code
			{
				public const string VeryLowRisk = "CR1";
				public const string LowRisk = "CR2";
				public const string Normal = "CR3";
				public const string HighRisk = "CR4";
				public const string VeryHighRisk = "CR5";
			}
		}

		#endregion

		#region Attachment Type

		public static class AttachmentType
		{
			public const string FIL = "FIL";
			public const string PDF = "PDF";
			public const string PDFA = "PDFA";
			public const string PDFC = "PDFC";
			public const string TIF = "TIF";
			public const string XLS = "XLS";
			public const string XLSX = "XLSX";
			public const string HTML = "HTML";
			public const string HTMF = "HTMF";
		}

		#endregion

		#region Custom Label Type

		public static class CustomLabelType
		{
			public const string Form = "FRM";
			public const string Report = "RPT";
			public const string Document = "DOC";
			public const string OverrideExportDoc = "OED";
			public const string OverrideImportDoc = "OID";
		}

		#endregion

		#region Invoice Line Groupings

		public static class InvoiceLineGroupings
		{
			public static class Code
			{
				public const string None = "NOG";
				public const string All = "ALL";
				public const string AEC = "AEC";
				public const string OandF = "O&F";
				public const string ORF = "ORF";
				public const string FandD = "F&D";
				public const string OFD = "OFD";
				public const string OFO = "OFO";
				public const string OFF = "OFF";
				public const string OFI = "OFI";
				public const string CCG = "CCG";
				public const string CCD = "CCD";
				public const string CLC = "CLC";
				public const string FRT = "FRT";
			}
		}

		#endregion

		#region GroupOrSubTotal

		public static class GroupOrSubTotalCharges
		{
			public static class Code
			{
				public const string Alphabetical = "ALP";
				public const string Sequence = "SEQ";
				public const string SubTotalAndSequence = "SSQ";
				public const string RollUp = "ROL";
				public const string RollUpEntireConsol = "ROC";
				public const string SubTotal = "SUB";
				public const string User = "USR";
				public const string RollupAndSequence = "RSQ";
			}
		}

		#endregion

		#region ServiceDirection

		public static class ServiceDirection
		{
			public static class Code
			{
				public const string Import = "IMP";
				public const string Export = "EXP";
				public const string Domestic = "DOM";
				public const string CrossTrade = "CRO";
				public const string Unknown = "UKN";
				public const string All = "ALL";
			}
		}

		#endregion

		#region Category

		public static class Category
		{
			public const string Business = "BUS";
			public const string Government = "GOV";
			public const string NaturalPersonIndividual = "NAT";
			public const string NonGovernmentOrganisation = "NGO";
		}

		#endregion

		#region ModesForGroupOrSubTotal

		public static class ModesForGroupOrSubTotal
		{
			public static class Codes
			{
				public const string All = "ALL";
				public const string Air = "AIR";
				public const string Sea = "SEA";
				public const string FCL = "FCL";
				public const string LCL = "LCL";
				public const string Road = "ROA";
				public const string Rail = "RAI";
				public const string Courier = "COU";
			}
		}

		#endregion

		#region Merge Invoice Lines

		public static class MergeInvoiceLines
		{
			public const string NotMerge = "NON";
			public const string NotMergeUsingProductNumberInDescription = "NOP";
			public const string PartNumber = "PNO";
			public const string PartNumberUsingProductNumberInDescription = "PNP";
			public const string Classification = "CLS";
			public const string ClassificationUsingClassificationDescriptionAlways = "CLD";
			public const string Tariff = "TRF";
			public const string TariffAndDescription = "TRD";
			public const string Default = "DEF";
			public const string TariffAndMultiInvoices = "TRM";
		}

		#endregion

		#region Organisation Filter

		public static class FilterControl
		{
			#region Defaulting

			public static class Defaulting
			{
				public const string DefaultUNLOCOForSearch = "DefaultUNLOCOForSearch";
			}

			#endregion

			// The values in this region are not to be translated so constant string code sniffing is suppressed.
			#region SuppressResourceStringsCheckRegion
			#region Org Details

			public static class OrgDetails
			{
				public const string All = "All";
				public const string Common = "Common";
				public const string Code = "Code";
				public const string FullName = "Full Name";
				public const string BrandOrRelatedName = "Related Company Name";
				public const string Web = "Web Address";
			}

			public static class OrgContact
			{
				public const string ContactName = "Contact Name";
				public const string Phone = "Contact Phone";
				public const string Fax = "Contact Fax";
				public const string Mobile = "Contact Mobile";
				public const string Email = "Contact E-Mail";
			}

			public static class OrgAddress
			{
				public const string Address = "Address";
				public const string City = "City";
				public const string State = "State";
				public const string PostCode = "Post Code";
				public const string Phone = "Phone";
				public const string Mobile = "Mobile";
				public const string Fax = "Fax";
				public const string Email = "E-Mail";
				public const string Country = "Country/Region";
			}

			public static class OrgCusCode
			{
				public const string RegistrationNumber = "Registration Number";
			}

			public static class OrgBranch
			{
				public const string Branch = "Branch";
			}

			#endregion

			#region Secondary Org Type

			public static class SecondaryOrgType
			{
				public const string None = "None";

				public const string ARQualityAssured = "A/R - Quality Assured";
				public const string APQualityAssured = "A/P - Quality Assured";
				public const string IncludedInAutoRateUpdate = "A/R - Included in Automatic Rate Updates";
				public const string NotIncludedInAutoRateUpdate = "A/R - Not Included in Automatic Rate Updates";

				public const string CreditOnHold = "A/R - Credit On Hold";

				public const string LocalTransport = "Carrier - Road Transport Provider";
				public const string ShippingLine = "Carrier - Shipping Line";
				public const string Airline = "Carrier - Airline";
				public const string Rail = "Carrier - Rail";
				public const string InlandWaterway = "Carrier - Inland Waterways";
				public const string AirWholesaler = "Carrier - Air Freight Wholesaler";
				public const string SeaWholesaler = "Carrier - NVOCC";
				public const string LineHaul = "Carrier - Line Haul";
				public const string Principal = "Carrier - Principal";
				public const string VesselConsortium = "Carrier - Vessel Consortium";

				public const string HandlesAirFreight = "Forwarder - Air Freight";
				public const string HandlesSeaFreight = "Forwarder - Sea Freight";
				public const string HandlesRoadFreight = "Forwarder - Road Freight";
				public const string HandlesRailFreight = "Forwarder - Rail Freight";

				public const string Depot = "Services - Depot";
				public const string PackingDepot = "Services - Packing Depot";
				public const string UnpackingDepot = "Services - Unpacking Depot";
				public const string CTO = "Services - CTO";
				public const string AirCTO = "Services - Air CTO";
				public const string SeaCTO = "Services - Sea CTO";
				public const string RoadDepotTransitShed = "Services - Road Depot / Transit Shed";
				public const string RailHeadDepot = "Services - Rail Head / Depot";
				public const string FerryWaterTerminal = "Services – Ferry/Inland Water Terminal";
				public const string ContainerYard = "Services - Container Yard";
				public const string FumigationContractor = "Services - Fumigation Contractor";
				public const string ContainerLeasingCompany = "Services - Container Leasing Company";
				public const string VGMContractor = "Services - VGM Contractor";

				public const string ProspectiveClient = "Sales - Prospective Client";
				public const string PartialClient = "Sales - Partial Client";
				public const string NewsLetter = "Sales - News Letter";
				public const string FaxUpdate = "Sales - Fax Update";
				public const string MailOut = "Sales - Mail Out";
				public const string EmailUpdate = "Sales - E-Mail Update";
			}

			#endregion

			#region GUID Relationships

			public static class GuidRelationships
			{
				public static class Code
				{
					public const string ARAcctGroup = "A/R Debt Grp";
					public const string ARCurrency = "A/R Currency";
					public const string APAcctGroup = "A/P Credit Grp";
					public const string APBankAccount = "A/P Bank Acct";
					public const string APChargeCode = "A/P Chrge Code";
					public const string CNRCountry = "CNR Country";
					public const string CNRCurrency = "CNR Currency";
					public const string CNRSeaCartageCordinator = "CNR Sea Cart Coord";
					public const string CNRAirCartageCordinator = "CNR Air Cart Coord";
					public const string CNRSeaCustomerServiceRep = "CNR Sea Svc Rep";
					public const string CNRAirCustomerServiceRep = "CNR Air Svc Rep";
					public const string CNESeaCustomerServiceRep = "CNE Sea Svc Rep";
					public const string CNEAirCustomerServiceRep = "CNE Air Svc Rep";
					public const string CNESeaCartageCordinator = "CNE Sea Cart Coord";
					public const string CNEAirCartageCordinator = "CNE Air Cart Coord";
					public const string FWDCurrency = "FWD Currency";
					public const string SALOverallRep = "SAL Overall Rep";
					public const string SALImportAirRep = "SAL Import Air Rep";
					public const string SALImportSeaRep = "SAL Import Sea Rep";
					public const string SALExportAirRep = "SAL Export Air Rep";
					public const string SALExportSeaRep = "SAL Export Sea Rep";
					public const string SALWarehousingRep = "SAL Warehouse Rep";
					public const string SALOverallAccountManager = "SAL Ovr Acc Mgr";
					public const string SALMainImpCommodity = "SAL Main Import Commodity";
					public const string SALMainExpCommodity = "SAL Main Export Commodity";
					public const string SALTradeLaneCommodity = "SAL Trade Lane Commodity";
					public const string RMStaff = "RM Staff";
					public const string RMStaffCompany = "RM Staff Company";
				}

				public static class Description
				{
					public static string ARAcctGroup
					{
						get { return "Receivables Debtor Group"; }
					}
					public static string ARCurrency
					{
						get { return "Receivables Currency"; }
					}
					public static string APAcctGroup
					{
						get { return "Payables Creditor Group"; }
					}
					public static string APBankAccount
					{
						get { return "Payables Default Bank Account"; }
					}
					public static string APChargeCode
					{
						get { return "Payables Charge Code"; }
					}
					public static string CNRCountry
					{
						get { return "Consignor Country of Origin"; }
					}
					public static string CNRCurrency
					{
						get { return "Consignor Default Currency"; }
					}
					public static string CNRSeaCartageCordinator
					{
						get { return "Consignor Sea Port Transport Coordinator"; }
					}
					public static string CNRAirCartageCordinator
					{
						get { return "Consignor Air Port Transport Coordinator"; }
					}
					public static string CNRSeaCustomerServiceRep
					{
						get { return "Consignor Sea Service Representative"; }
					}
					public static string CNRAirCustomerServiceRep
					{
						get { return "Consignor Air Service Representative"; }
					}
					public static string CNESeaCustomerServiceRep
					{
						get { return "Consignee Sea Service Representative"; }
					}
					public static string CNEAirCustomerServiceRep
					{
						get { return "Consignee Air Service Representative"; }
					}
					public static string CNESeaCartageCordinator
					{
						get { return "Consignee Sea Port Transport Coordinator"; }
					}
					public static string CNEAirCartageCordinator
					{
						get { return "Consignee Air Port Transport Coordinator"; }
					}
					public static string FWDCurrency
					{
						get { return "Forwarder Default Currency"; }
					}
					public static string SALOverallRep
					{
						get { return "Sales Overall Representative"; }
					}
					public static string SALImportAirRep
					{
						get { return "Sales Import Air Representative"; }
					}
					public static string SALImportSeaRep
					{
						get { return "Sales Import Sea Representative"; }
					}
					public static string SALExportAirRep
					{
						get { return "Sales Export Air Representative"; }
					}
					public static string SALExportSeaRep
					{
						get { return "Sales Export Sea Representative"; }
					}
					public static string SALWarehousingRep
					{
						get { return "Sales Warehousing Representative"; }
					}
					public static string SALOverallAccountManager
					{
						get { return "Sales Overall Account Manager"; }
					}
					public static string SALMainImpCommodity
					{
						get { return "Sales Main Import Commodity"; }
					}
					public static string SALMainExpCommodity
					{
						get { return "Sales Main Export Commodity"; }
					}
					public static string SALTradeLaneCommodity
					{
						get { return "Sales Trade Lane Commodity"; }
					}
					public static string RMStaff
					{
						get { return "RM Staff Initials"; }
					}
					public static string RMStaffCompany
					{
						get { return "RM Staff Company"; }
					}
				}
			}

			#endregion

			#region Drop Edit Relationships

			public static class DropEditRelationships
			{
				public static class Code
				{
					public const string CustomsCodeType = "Customs Code Type";
					public const string GlobalRateTarriff = "Company Tariff";
					public const string ARAcctRelationship = "A/R Acct Relation";
					public const string ARConsolidation = "A/R Cons Cat";
					public const string ARStdInvoiceTerms = "A/R Std Inv Terms";
					public const string ARDisbInvoiceTerms = "A/R Disb Inv Terms";
					public const string ARGrpImportCharges = "A/R Grp Imp Chrgs";
					public const string ARGrpExportCharges = "A/R Grp Exp Chrgs";
					public const string APAcctRelationship = "A/P Acct Relation";
					public const string APConsolidation = "A/P Cons Cat";
					public const string APPaymentTerms = "A/P Payment Terms";
					public const string CNRExportCategory = "CNR Exp Category";
					public const string CNRINCOTerm = "CNR Incoterm";
					public const string CNEImportCategory = "CNE Imp Category";
					public const string CNEMergeCusLines = "CNE Merge Cus Lines";
					public const string CNESendAirDocs = "CNE Send Air Docs";
					public const string CNESendSeaDocs = "CNE Send Sea Docs";
					public const string FWDAgentCategory = "FWD Agent Cat";
					public const string SRVUsagePreference = "SRV Usage Pref";
					public const string ADRType = "ADR Address Type";
					public const string ADRLanguage = "ADR Language";
					public const string CNTType = "CNT Contact Type";
					public const string SALCategory = "SAL Client Cat";
					public const string SALClientSize = "SAL Client Size";
					public const string SALIndustryVertical = "SAL Industry Vertical";
					public const string SALPeriodOfActivity = "SAL Period of Activity";
					public const string SALTerritory = "SAL Territory";
					public const string SALOutlook = "SAL Outlook";
					public const string SALCompActivity = "SAL Comp Act";
					public const string SALTradeMode = "SAL Trade Mode";
					public const string SALTradeLaneIndustryVertical = "SAL Trade Lane Industry Vertical";
					public const string SALTradeLanePeriodOfActivity = "SAL Trade Lane Period of Activity";
					public const string SALTransportMode = "SAL Transport Mode";
					public const string SALTradeStatus = "SAL Trade Status";
					public const string SALAirCosts = "SAL Air Costs";
					public const string SALLCLCosts = "SAL LCL Costs";
					public const string SALTEUCosts = "SAL TEU Costs";
					public const string SALWarehouseCosts = "SAL Warehse Costs";
					public const string SALOtherCosts = "SAL Other Costs";
					public const string SALClientRelationship = "SAL Client Relation";
					public const string SALDesireToRemain = "SAL Desire Remain";
					public const string SALEaseToPoach = "SAL Ease To Poach";
					public const string SALElectronicIntegration = "SAL Electric Integ";
					public const string CMPServiceType = "CMP Service Type";
					public const string CMPSellStyle = "CMP Sell Style";
					public const string CMPCategory = "CMP Category";
					public const string CMPTradeMode = "CMP Trade Mode";
					public const string FCLEquipment = "FCL Equipment";
					public const string LCLEquipment = "LCL Equipment";
					public const string AirEquipment = "Air Equipment";
					public const string RMStaffRole = "RM Staff Role";
					public const string RMStaffDepartment = "RM Staff Department";
					public const string MCRCarrierCategory = "Carrier Category";
				}

				public static class Description
				{
					public static string CustomsCodeType
					{
						get { return "Customs Code Type"; }
					}
					public static string GlobalRateTarriff
					{
						get { return "Company Tariff"; }
					}
					public static string ARAcctRelationship
					{
						get { return "Receivables Accounts Relationship"; }
					}
					public static string ARConsolidation
					{
						get { return "Receivables Consolidation Category"; }
					}
					public static string ARStdInvoiceTerms
					{
						get { return "Receivables Standard Invoice Terms"; }
					}
					public static string ARDisbInvoiceTerms
					{
						get { return "Receivables Disbursement Invoice Terms"; }
					}
					public static string ARGrpImportCharges
					{
						get { return "Receivables Group Import Charges By"; }
					}
					public static string ARGrpExportCharges
					{
						get { return "Receivables Group Export Charges By"; }
					}
					public static string APAcctRelationship
					{
						get { return "Payables Accounts Relationship"; }
					}
					public static string APConsolidation
					{
						get { return "Payables Consolidation Category"; }
					}
					public static string APPaymentTerms
					{
						get { return "Payables Payment Terms"; }
					}
					public static string CNRExportCategory
					{
						get { return "Consignor Exporter Category"; }
					}
					public static string CNRINCOTerm
					{
						get { return "Consignor Incoterm"; }
					}
					public static string CNEImportCategory
					{
						get { return "Consignee Importer Category"; }
					}
					public static string CNEMergeCusLines
					{
						get { return "Consignee Merge Customs Lines By"; }
					}
					public static string CNESendAirDocs
					{
						get { return "Consignee Send Air Documents To"; }
					}
					public static string CNESendSeaDocs
					{
						get { return "Consignee Send Sea Documents To"; }
					}
					public static string FWDAgentCategory
					{
						get { return "Forwarder Agent Category"; }
					}
					public static string SRVUsagePreference
					{
						get { return "Service Provider Usage Preference"; }
					}
					public static string ADRType
					{
						get { return "Address Type"; }
					}
					public static string ADRLanguage
					{
						get { return "Address Language"; }
					}
					public static string CNTType
					{
						get { return "Contact Type"; }
					}
					public static string SALCategory
					{
						get { return "Sales Client Category"; }
					}
					public static string SALClientSize
					{
						get { return "Sales Client Size"; }
					}
					public static string SALIndustryVertical
					{
						get { return "SAL Client Vertical Market"; }
					}
					public static string SALPeriodOfActivity
					{
						get { return "SAL Client Period of Activity"; }
					}
					public static string SALTerritory
					{
						get { return "Sales Client Territory"; }
					}
					public static string SALOutlook
					{
						get { return "Sales Client Growth Outlook"; }
					}
					public static string SALCompActivity
					{
						get { return "Sales Main Competitor Activity"; }
					}
					public static string SALTradeMode
					{
						get { return "Sales Monthly Trade Mode"; }
					}
					public static string SALTradeLaneIndustryVertical
					{
						get { return "SAL Trade Lane Industry Vertical"; }
					}
					public static string SALTradeLanePeriodOfActivity
					{
						get { return "SAL Trade Lane Period of Activity"; }
					}
					public static string SALTransportMode
					{
						get { return "Sales Trade Lane Transport Mode"; }
					}
					public static string SALTradeStatus
					{
						get { return "Sales Trade Lane Status"; }
					}
					public static string SALCommissionType
					{
						get { return "Sales Representative Commission Type"; }
					}
					public static string SALAirCosts
					{
						get { return "Sales Client Effect on Air Costs"; }
					}
					public static string SALLCLCosts
					{
						get { return "Sales Client Effect on LCL Costs"; }
					}
					public static string SALTEUCosts
					{
						get { return "Sales Client Effect on TEU Costs"; }
					}
					public static string SALWarehouseCosts
					{
						get { return "Sales Client Effect on Warehousing Costs"; }
					}
					public static string SALOtherCosts
					{
						get { return "Sales Client Effect on Other Costs"; }
					}
					public static string SALClientRelationship
					{
						get { return "Sales Overall Client Relation"; }
					}
					public static string SALDesireToRemain
					{
						get { return "Sales Client Desire To Remain With Company"; }
					}
					public static string SALEaseToPoach
					{
						get { return "Sales Ease Client Can Be Poached"; }
					}
					public static string SALElectronicIntegration
					{
						get { return "Sales Amount of Electronic Integration"; }
					}
					public static string CMPServiceType
					{
						get { return "Competitor Service Type"; }
					}
					public static string CMPSellStyle
					{
						get { return "Competitor Selling Style"; }
					}
					public static string CMPCategory
					{
						get { return "Competitor Category"; }
					}
					public static string CMPTradeMode
					{
						get { return "Competitor Clients Trade Mode"; }
					}
					public static string FCLEquipment
					{
						get { return "Equipment Needed For FCL Drop Mode"; }
					}
					public static string LCLEquipment
					{
						get { return "Equipment Needed For LCL Drop Mode"; }
					}
					public static string AirEquipment
					{
						get { return "Equipment Needed For Air Drop Mode"; }
					}
					public static string RMStaffRole
					{
						get { return "RM Staff Role"; }
					}
					public static string RMStaffDepartment
					{
						get { return "RM Staff Department"; }
					}
					public static string MCRCarrierCategory
					{
						get { return "Carrier - Carrier Category"; }
					}
				}
			}

			#endregion

			#region Org Date Filter List

			public static class OrgDateFilterList
			{
				public static class Code
				{
					public const string STDARLastChecked = "AR Last QA";
					public const string STDAPLastChecked = "AP Last QA";
					public const string STDCreditReviewDate = "AR Acct Review Due";
					public const string STDCRShipExpected = "CNR 1st Ship Exp";
					public const string STDCEShipExpected = "CNE 1st Ship Exp";
					public const string SALCSDateLastCall = "Last Actual Communication";
					public const string SALCSDateNextCall = "Next Scheduled Communication";
					public const string SALCSDateLastUnactioned = "Last Un-Actioned Communication";
					public const string SALCSEstClose = "SAL Estimated Close";
					public const string SALCRClientComm = "SAL Client Commenced";
					public const string Created = "Created";
					public const string APAccountDetailsChanged = "APAccountDetailsChanged";
					public const string KnownShipperExpiryDate = "Known Shipper Expiry Date";
					public const string ImporterBondQueried = "Importer Bond Queried";
					public const string PowerOfAttorneyValidToDate = "Power Of Attorney Valid To Date";
					public const string LastScreenDate = "Last Screening Date";
				}

				public static class Description //Update to something more descriptive 
				{
					public static string STDARLastChecked
					{
						get { return "Date of Last QA Check For A/R"; }
					}
					public static string STDAPLastChecked
					{
						get { return "Date of Last QA Check For A/P"; }
					}
					public static string STDCreditReviewDate
					{
						get { return "A/R Account / Credit Review Due Date"; }
					}
					public static string STDCRShipExpected
					{
						get { return "Date of First Shipment From Buyer"; }
					}
					public static string STDCEShipExpected
					{
						get { return "Date of First Shipment From Supplier"; }
					}
					public static string SALCSDateLastCall
					{
						get { return "Date of Last Sales Call"; }
					}
					public static string SALCSDateNextCall
					{
						get { return "Date of Next Sales Call"; }
					}
					public static string SALCSDateLastUnactioned
					{
						get { return "Last Un-Actioned Communication"; }
					}
					public static string SALCSEstClose
					{
						get { return "Estimated Date to Close Business"; }
					}
					public static string SALCRClientComm
					{
						get { return "Date Sales Client Relationship Commenced"; }
					}
					public static string Created
					{
						get { return "Created"; }
					}
					public static string ImporterBondQueried
					{
						get { return "Date of Importer Bond Queried"; }
					}
					public static string PowerOfAttorneyValidToDate
					{
						get { return "Power Of Attorney Valid To Date"; }
					}
					public static string LastScreenDate
					{
						get { return "Last Screening Date"; }
					}
				}
			}
			#endregion
			#endregion

			#region UNLOCO Type

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name - not to be translated")]
			public static class UNLOCOType
			{
				public const string All = "All";
				public const string OrgPort = "Main UNLOCO";
				public const string SALTradeLanes = "Sales Trade Lanes";
				public const string CMPTradeLanes = "Competitors Lanes";
				public const string SALTradeLaneLocation = "Sales Trade Lane Origin / Destination";
				public const string ForwarderAppPort = "Forwarder Appointed Port";
				public const string CarrierAppPort = "Carrier Appointed Port";
			}

			#endregion

			#region Active Status

			public static class ActiveStatus
			{
				public static class Code
				{
					public const string AllClients = "ALL";
					public const string ActiveClients = "ACT";
					public const string InactiveClients = "NOT";
				}

				public static class Description
				{
					public static string AllClients
					{
						get { return Res.GetString("da2a597c-98a1-4cfc-8960-413d3dc12486", "All Active and Inactive"); }
					}
					public static string ActiveClients
					{
						get { return Res.GetString("4e7d99db-daab-449f-98a8-e06993cb159d", "Active Only"); }
					}
					public static string InactiveClients
					{
						get { return Res.GetString("69c726c5-46d3-4af8-8947-18f1535b5b59", "Inactive Only"); }
					}
				}
			}

			#endregion

			#region Sales Rep Status

			public static class SalesRepStatus
			{
				public static class Code
				{
					public const string AllStaff = "ALL";
					public const string SalesRep = "SALES";
					public const string NonSalesRep = "NOT";
				}
			}

			#endregion

			#region Driver Status
			public static class DriverStatus
			{
				public static class Code
				{
					public const string AllStaff = "ALL";
					public const string Driver = "DRIVER";
					public const string NonDriver = "NOT";
				}
			}

			#endregion

			#region Sys Admin Status

			public static class SysAdminStatus
			{
				public static class Code
				{
					public const string AllStaff = "ALL";
					public const string SysAdminStaff = "ADMIN";
					public const string NonSysAdminStaff = "NOT";
				}
			}

			#endregion

			#region Operational Status

			public static class OperationalStatus
			{
				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "enum-style code")]
				public static class Code
				{
					public const string AllStaff = "ALL";
					public const string NonOperationalStaff = "Non-Operational";
					public const string OperationalStaff = "Operational";
				}
			}

			#endregion

			#region Backup Operator Status

			public static class BackupOperatorStatus
			{
				public static class Code
				{
					public const string AllStaff = "ALL";
					public const string BackupOperatorStaff = "BACKUP";
					public const string NonBackupOperatorStaff = "NOT";
				}
			}

			#endregion

			#region Database Reader Status

			public static class DatabaseReaderStatus
			{
				public static class Code
				{
					public const string AllStaff = "ALL";
					public const string DatabaseReaderStaff = "READER";
					public const string NonDatabaseReaderStaff = "NOT";
				}
			}

			#endregion

			#region Database Developer Status

			public static class DatabaseDeveloperStatus
			{
				public static class Code
				{
					public const string AllStaff = "ALL";
					public const string DatabaseDeveloperStaff = "DEVELOPER";
					public const string NonDatabaseDeveloperStaff = "NOT";
				}
			}

			#endregion

			#region Is Device Only Status

			public static class IsDeviceOnlyStatus
			{
				public static class Code
				{
					public const string AllStaff = "ALL";
					public const string DeviceOnlyStaff = "DEVICE";
					public const string NonDeviceOnlyStaff = "NOT";
				}
			}

			#endregion

			#region Account Type

			public static class AccountType
			{
				public static class Code
				{
					public const string GlobalAccount = "GLO";
					public const string NonGlobalAccount = "GLX";
					public const string NationalAccount = "NAT";
					public const string NonNationalAccount = "NAX";
					public const string TemporaryAccount = "TMP";
					public const string NonTemporaryAccount = "TMX";
				}
			}

			#endregion

			#region Sales Rep Assigned

			public static class SalesRepAssigned
			{
				public static class Code
				{
					public const string All = "ALL";
					public const string Assigned = "ASN";
					public const string NotAssigned = "NOT";
				}
			}

			#endregion

			#region Rates Security

			public static class RatesSecurity
			{
				public static class Code
				{
					public const string All = "ALL";
				}

				public static class Description
				{
					public static string All
					{
						get { return Res.GetString("1e8cccf8-c35a-4503-bded-9e0ffa9a9dd9", "All Organizations with rates' security"); }
					}
				}
			}

			#endregion

			#region External Validation Status

			public static class ExternalValidationStatus
			{
				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "enum-style code")]
				public static class Code
				{
					public const string Passed = "PASSED";
					public const string Failed = "FAILED";
					public const string NotCompleted = "NOT COMPLETED";
					public const string NotRun = "NOT RUN";
				}

				public static class Description
				{
					public static string Passed
					{
						get { return Res.GetString("1b699167-e5e5-44e8-b1cb-b7306c828da4", "Last validation passed"); }
					}

					public static string Failed
					{
						get { return Res.GetString("cefe53d3-ee6c-407c-a026-8eeff4d2be16", "Last validation failed"); }
					}

					public static string NotCompleted
					{
						get { return Res.GetString("ff7a5caa-c0d4-485e-ad9a-339139641ab1", "Last validation not completed"); }
					}

					public static string NotRun
					{
						get { return Res.GetString("fcf96da4-d11e-4234-87bb-372bb3c0bbb0", "No validation run"); }
					}
				}
			}

			#endregion

			#region Primary Workplace Contacts Only

			public static class ContactIsPrimaryWorkplace
			{
				public static class Code
				{
					public const string AllContacts = "ALL";
					public const string PrimaryWorkplaceContactsOnly = "PWC";
					public const string NonPrimaryWorkplaceContacts = "NOT";
				}
			}

			#endregion

			#region Is Foreign Operator

			public static class IsForeignOperator
			{
				public static class Code
				{
					public const string Yes = "YES";
					public const string No = "NO";
					public const string All = "ALL";
				}

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name - not to be translated")]
				public const string FilterName = "Is Foreign Operator";
			}

			#endregion
		}

		#endregion

		#region Send Docs To

		public static class SendDocsTo
		{
			public const string Importer = "IMP";
			public const string Broker = "BRK";
			public const string Both = "BTH";
		}

		#endregion

		#region NMFC Participant Codes

		public static class NMFCParticipantCodes
		{
			public static class Code
			{
				public const string No = "NO";
				public const string Yes = "YES";
			}

			public static class Description
			{
				public static string No
				{
					get { return Res.GetString("b4272fda-3e4a-48fa-bb11-06dee381fa63", "Is NOT NMFC Participant Carrier"); }
				}
				public static string Yes
				{
					get { return Res.GetString("210cdda5-2cdc-45da-b796-26268d260a52", "Is NMFC Participant Carrier"); }
				}
			}
		}
		#endregion

		#region Organisation Types

		public static class OrganisationTypes
		{
			public const string ActiveClient = "ACT";
			public const string NationalAccounts = "NAT";
			public const string GlobalSupplier = "GLB";
			public const string TemporaryAcct = "TMP";
			public const string Receivables = "A_R";
			public const string Payables = "A_P";
			public const string Consignor = "CNR";
			public const string Consignee = "CNE";
			public const string Transport = "TRN";
			public const string Warehouse = "WHS";
			public const string Carrier = "CAR";
			public const string Forwarder = "FWD";
			public const string Broker = "BRK";
			public const string Services = "SVS";
			public const string Competitors = "COM";
			public const string Sales = "SAL";
		}

		#endregion

		#region Credit Agreed Payment Method

		public static class CreditAgreedPaymentMethods
		{
			public static class Code
			{
				public const string BusinessCheck = "CHK";
				public const string CreditCard = "CCD";
				public const string BankTransfer = "TRF";
				public const string CashAndBankCheck = "CBC";
				public const string DebitCard = "DBC";
				public const string CollectionRequest = "CRQ";
				public const string EPayment = "EPA";
			}
		}

		#endregion

		#region Contact Allocation Type

		public static class ContactAllocationType
		{
			public const string CAPGA = "CAP";
			public const string CEOForKRCustoms = "KRC";
			public const string ValuationAuthorityForKRCustoms = "KRV";
			public const string CNCUS = "CNC";
			public const string ContactForKRCustoms = "CKR";
			public const string HAZ = "HAZ";
			public const string CUS = "CUS";
			public const string BRForeignOperator = "NFO";
			public const string NZBiosecurity = "NZB";
			public const string NZCustoms = "NZC";
			public const string USFSV = "USF";
			public const string USPGA = "USP";
			public const string VAT = "VAT";
			public const string KRS = "KRS";
			public const string CIV = "CIV";

			public static bool ShouldAllowMultiple(string code)
			{
				return code == HAZ || code == VAT || code == ValuationAuthorityForKRCustoms;
			}
		}

		#endregion

		#region Service Level Amount Types

		public static class ServiceLevelAmountTypes
		{
			public static class Code
			{
				public const string Excess = "EXC";
				public const string Minimum = "MIN";
				public const string Maximum = "MAX";
				public const string None = "NON";
			}

			public static class Description
			{
				public static MultilingualString Excess { get { return ResString.GetMultilingualString("7e171df3-bbd7-437a-abfa-dcfa1603d735", "EXCESS"); } }
				public static MultilingualString Minimum { get { return ResString.GetMultilingualString("5083d040-670d-4369-9205-d1987cd36d89", "MINIMUM"); } }
				public static MultilingualString Maximum { get { return ResString.GetMultilingualString("0fbfdf62-69c9-4673-9b15-e783dffb1751", "MAXIMUM"); } }
				public static MultilingualString None { get { return ResString.GetMultilingualString("30455846-acee-450a-a71f-bfb25a9f2b1d", "NONE"); } }
			}
		}

		#endregion

		#region Organisation Number Fountain Types

		public static class NumberFountains
		{
			public static class Code
			{
				public const string SSCCBarCodeNumbers = "SSC";
				public const string FTZAdmissionControlNumber = "FTZ";
				public const string FTZAdmissionControlNumberForWarehouse = "FTW";
				public const string ForwardAirBillNumbers = "FWA";
				public const string TransportReferenceNumbers = "TRF";
				public const string PatentNumber = "PAT";
			}

			public static class Description
			{
				public static MultilingualString SSCCBarCodeNumbers { get { return ResString.GetMultilingualString("42229457-4af3-4b03-bab2-4c4562b2998a", "SSCC Bar Code Numbers"); } }
				public static MultilingualString FTZAdmissionControlNumber { get { return ResString.GetMultilingualString("B9219525-37FF-4250-9705-214D78DE79CA", "FTZ Admission Control Number (US)"); } }
				public static MultilingualString FTZAdmissionControlNumberForWarehouse { get { return ResString.GetMultilingualString("12345678-37FF-4250-9705-214D78DE79CA", "FTZ Admission Control Number for Warehouse (US)"); } }
				public static MultilingualString ForwardAirBillNumbers { get { return ResString.GetMultilingualString("E176E59D-7D19-4C40-AFA9-00BA722E1D4F", "Forward Air Bill Numbers"); } }
				public static MultilingualString TransportReferenceNumbers { get { return ResString.GetMultilingualString("DFCC3231-ECF3-488E-80E3-C1FBCE562E48", "Transport Reference Numbers"); } }
				public static MultilingualString PatentNumber { get { return ResString.GetMultilingualString("FE9FB9E6-51FE-4AB4-8FD1-D685C9B05CDE", "Parent Number (MX)"); } }
			}
		}

		#endregion

		#region CusCodeValidityVerification

		public static class CusCodeValidityVerification
		{
			public static MultilingualString Verified { get { return ResString.GetMultilingualString("D2BDDA01-60AE-4C26-AE1C-47D6807BC4DC", "VERIFIED"); } }
			public static MultilingualString NotVerified { get { return ResString.GetMultilingualString("6A3FB48D-7FE8-4942-B239-CFA55C58A1B3", "NOT VERIFIED"); } }
		}

		#endregion

		#region OrgCarrierAppointedAgentPorts

		public static class CarrierAgentDirections
		{
			public static class Code
			{
				public const string Both = "BTH";
				public const string Arrival = "ARV";
				public const string Departure = "DEP";
			}

			public static class Description
			{
				public static MultilingualString Both { get { return ResString.GetMultilingualString("7128c200-1209-4e5c-b326-e8a34ed8786c", "Both"); } }
				public static MultilingualString Arrival { get { return ResString.GetMultilingualString("225ab063-c51b-4baf-81de-b43fee1c5eda", "Arrival"); } }
				public static MultilingualString Departure { get { return ResString.GetMultilingualString("b70ca9c5-3c7c-4bc8-90c3-0ad4b242c358", "Departure"); } }
			}
		}

		#endregion
	}
}
