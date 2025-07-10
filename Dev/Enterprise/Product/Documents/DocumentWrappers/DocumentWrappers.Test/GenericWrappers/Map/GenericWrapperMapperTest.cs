using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.Mapping;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	sealed class GenericWrapperMapperTest : TestCaseWithFactory
	{
		public void TestGetMapAsTableListOnlyGoesOneLevelIntoStandardDocumentWrappers()
		{
			List<MapTable> tableList = GenericWrapperMapper.GetMapAsTableList(typeof(GenericWrapperWith2LevelsOfDocBaseWrappers), true, false);
			AssertEquals(1, tableList.Count);
			tableList = GenericWrapperMapper.GetMapAsTableList(typeof(GenericWrapperWith2LevelsOfDocBaseWrappers), true, true);
			AssertEquals(2, tableList.Count);
		}

		[DeveloperOnlyTest]
		public void TestGetFullFreightJobMap()
		{
			AssertMultilineASCIIEquals(">>> Use Araxis Merge to see changes in the full map since the last checkin. <<<    NB: This will only blow on a developer machine, update the map just before you check in. That way you can see what you changed since the last time you checked in. Is like a final review."
				, CurrentFullFreightJobMap.Trim()
				, GenericWrapperMapper.GetMapAsText(typeof(FreightWrapper), true, false));
		}

		#region CurrentFullFreightJobMap
		const string CurrentFullFreightJobMap = @"
Freight                                     (Default Field: JobNumber)
======================================================================
Name                                    Type
----------------------------------------------------------------------
ContainerYardEmptyPickupAddress         Address
ContainerYardEmptyReturnAddress         Address
DeliveryAddress                         Address
ExportReceivalAddress                   Address
ExportReceivingCTOAddress               Address
ExportReceivingDepotAddress             Address
GoodsAvailableAt                        Address
ImportArrivalCTOAddress                 Address
PickupAddress                           Address
PickupCFSAddress                        Address
UnpackCFSAddress                        Address
CartageInfo                             CartageInfo
CaratagePickupMode                      CodeAndDescription
ConsolContainerMode                     CodeAndDescription
ConsolTransportMode                     CodeAndDescription
ConsolType                              CodeAndDescription
OrderTransportMode                      CodeAndDescription
ReleaseType                             CodeAndDescription
ServiceLevel                            CodeAndDescription
ShipmentContainerMode                   CodeAndDescription
ShipmentStatus                          CodeAndDescription
ShipmentTransportMode                   CodeAndDescription
ShipmentType                            CodeAndDescription
ShippedOnBoardType                      CodeAndDescription
ExportAgent                             ExportAgentOrganisation
ParentJob                               Freight
TranshipmentFreightConsol               Freight
GateTransport                           GateTransport
IncoTerm                                INCO Term
InvoicingJob                            Invoicing Job
LocalForwarder                          LocalForwarderOrganisation
DeliveryLocation                        Location
FreightPayableAt                        Location
PickupLocation                          Location
CollectAmount                           Money
FreightRate                             Money
GoodsValue                              Money
InsuranceValue                          Money
AssuredParty                            Organisation
BookingParty                            Organisation
Buyer                                   Organisation
Carrier                                 Organisation
ClaimsPayableBy                         Organisation
Client                                  Organisation
Consignee                               Organisation
Consignor                               Organisation
ConsolCreditor                          Organisation
Consolidator                            Organisation
CTOArrival                              Organisation
DeliveryAgent                           Organisation
ExportBroker                            Organisation
ImportAgent                             Organisation
ImportBroker                            Organisation
InsuredBy                               Organisation
JobHeaderLocalClient                    Organisation
MainShipToParty                         Organisation
NotifyParty                             Organisation
PickupAgent                             Organisation
Principal                               Organisation
RecommendedAgent                        Organisation
SellingParty                            Organisation
StuffingLocation                        Organisation
Supplier                                Organisation
SurveyReportParty                       Organisation
ShipmentInnerPacksQty                   PackQTY
ShipmentOuterPacksQty                   PackQTY
Destination                             PlaceAndDate
FirstForeignPort                        PlaceAndDate
LastForeignPort                         PlaceAndDate
Origin                                  PlaceAndDate
PortOfFirstArrival                      PlaceAndDate
QueryClaim                              Query Claim
Rating                                  Rating Information
InterestedRoute                         Route
RunSheet                                RunSheet
SalesRep                                StaffMember
ChargeableWeight                        ValueAndUnit
StorageTime                             ValueAndUnit
Volume                                  Volume
WarehouseJob                            WarehouseJob
Weight                                  Weight
ActualReceive                           DateTime
AdditionalTerms                         String
ArrivalReference                        String
BookingReference                        String
CAPreviousCCN                           String
CargoControlNumberForCanada             String
CATransactionNo                         String
ConsolDateCreated                       DateTime
ConsolNumber                            String
ConsolPaymentType                       String
ConsolReference                         String
ContainerLayoutStyle                    String
ContainerSummary                        String
CTOArrivalBerth                         String
CustomAttribute1                        String
CustomAttribute2                        String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomerReference                       String
CustomFlag1                             Bool
CustomFlag2                             Bool
CustomsEntryNumber                      String
DeliveryCartageAdvised                  DateTime
DeliveryEstimated                       DateTime
DeliveryFrom                            DateTime
DeliveryGoodsDelivered                  DateTime
DeliveryRequiredBy                      DateTime
DepartureOrArrivalText                  String
ExportAgentsReference                   String
ExWorksRequiredBy                       DateTime
FactoryEx                               DateTime
FreightDepotType                        String
GoodsDescription                        String
HasTACImage                             Bool
Hazardous                               Bool
HBLContainerMode                        String
HouseBill                               String
HouseBillHeading                        String
HouseBillIssue                          DateTime
ImportAgentsReference                   String
JobNumber                               String
JobNumberBarcodeText                    String
JobNumberBarcodeTextForFont             String
JobNumberBarcodeTextWithoutDocManagerCodes  String
JobNumberHeading                        String
LoadingMeters                           Decimal
LocalForwarderReference                 String
MarksAndNumbers                         String
MasterBill                              String
MasterBillHeading                       String
MasterBillIssue                         DateTime
NoCopyBills                             Int
NoOriginalBills                         Int
NotClearedByAgentExpiryDate             DateTime
NotClearedByAgentIssueDate              DateTime
NotClearedByAgentNumber                 String
NotClearedByAgentStatement              String
OrderDate                               DateTime
OrderNumbersWithOwnersReference         String
OwnerReference                          String
PickupCartageAdvised                    DateTime
PickupDateOfReceipt                     DateTime
PickupTruckWaitCharge                   Decimal
PickupTruckWaitTime                     String
PickupFrom                              DateTime
PickupGoodsPickedup                     DateTime
PickupInterimReceipt                    String
PickupLabourCharge                      Decimal
PickupLabourTime                        String
PickupRequiredBy                        DateTime
PreviousCargoControlNumberForCanada     String
QuoteNumber                             String
Refrigerated                            Bool
SecondaryHeading                        String
SecondaryNumber                         String
ShipmentDateCreated                     DateTime
ShippedOnBoardDate                      DateTime
ShippersReference                       String
TransportReference                      String
UnAllocatedPackages                     Int
UnAllocatedVolume                       Decimal
UnAllocatedWeight                       Decimal
WarehouseLocation                       String

TransportAddresses                      Address Collection
TransportAddressesWithWarehousing       Address Collection
AutoRatedInfosForJobRevenue             AutoRateInformation Collection
Charges                                 Charge Collection
CommercialInvoices                      CommercialInvoice Collection
CommercialInvoiceLines                  CommercialInvoiceLine Collection
Containers                              Container Collection
TranshipmentContainers                  Container Collection
Services                                ContainerService Collection
Costs                                   Cost Collection
CustomsEntries                          CustomsEntry Collection
ExchangeRates                           ExchangeRate Collection
FreightConsolidations                   Freight Collection
FreightJobs                             Freight Collection
TransportBookings                       Freight Collection
BookingInstructions                     Instruction Collection
LocalTransportLegs                      LocalTransportLeg Collection
Milestones                              Milestone Collection
Notes                                   Note Collection
NotesIncludingRelated                   Note Collection
Orders                                  Order Collection
Packages                                Package Collection
PackProducts                            PackProductWrapper Collection
PickupDeliveryConfirmations             PickupDeliveryConfirmations Collection
RequiredDocuments                       RequiredDocuments Collection
ConsolRoutes                            Route Collection
ShipmentRoutes                          Route Collection
UNDGs                                   UNDGSubstance Collection
ConsigneeRequiredTaxNumber              String
MasterBillConsigneeOverrideRequiredTaxNumber  String
ConsignorRequiredTaxNumber              String
MasterBillShipperOverrideRequiredTaxNumber  String
NotifyPartyRequiredTaxNumber            String

Route                                       (Default Field: Transport)
======================================================================
Name                                    Type
----------------------------------------------------------------------
DestinationAddress                      Address
OriginAddress                           Address
TransportType                           CodeAndDescription
Destination                             Location
Origin                                  Location
Carrier                                 Organisation
Transport                               Transport
ActualArrival                           DateTime
ActualDeparture                         DateTime
AvailabilityDate                        DateTime
CarriersReference                       String
DateOfArrival                           DateTime
DateOfDeparture                         DateTime
DGFCLCutOff                             DateTime
DGFCLReceivalCommences                  DateTime
EstimatedArrival                        DateTime
EstimatedDeparture                      DateTime
FCLCutOff                               DateTime
FCLReceivalCommences                    DateTime
FCLStorageCommences                     DateTime
IsCargoOnly                             Bool
IsInternational                         Bool
LCLAvailabilityDate                     DateTime
LCLCutOff                               DateTime
LCLReceivalCommences                    DateTime
LCLStorageCommences                     DateTime
LegNo                                   Int

CommercialInvoice                       (Default Field: InvoiceNumber)
======================================================================
Name                                    Type
----------------------------------------------------------------------
IncoTerm                                CodeAndDescription
CountryOfOrigin                         Country
FreightJob                              Freight
ExcludedChargesTotal                    Money
ExcludedCommission                      Money
ExcludedDiscount                        Money
ExcludedDutiableOtherCharges            Money
ExcludedExWorksCharges                  Money
ExcludedInlandFreight                   Money
ExcludedLandingCharges                  Money
ExcludedNonDutiableOtherCharges         Money
ExcludedOverseasFreight                 Money
ExcludedOverseasInsurance               Money
ExcludedPackingCharges                  Money
IncludedChargesTotal                    Money
IncludedCommission                      Money
IncludedDiscount                        Money
IncludedDutiableOtherCharges            Money
IncludedExWorksCharges                  Money
IncludedInlandFreight                   Money
IncludedLandingCharges                  Money
IncludedNonDutiableOtherCharges         Money
IncludedOverseasFreight                 Money
IncludedOverseasInsurance               Money
IncludedPackingCharges                  Money
InvoiceAmount                           Money
Importer                                Organisation
Supplier                                Organisation
Volume                                  ValueAndUnit
Weight                                  ValueAndUnit
AdditionalInformation                   String
AdditionalPaymentTerms                  String
ExchangeRate                            Decimal
ExportersBankAccountNo                  String
ExportersBankName                       String
ExportersBankSWIFTCode                  String
ImporterRequiredVATNumber               String
InsurancePolicyNumber                   String
InsuredValue                            String
InvoiceDate                             DateTime
InvoiceNumber                           String
JobNumber                               String
LetterOfCreditDate                      String
LetterOfCreditNumber                    String
LocalChamberOfCommerceInfo              String
NotaryPublicInfo                        String
SupplierRequiredVATNumber               String

InvoiceLines                            CommercialInvoiceLine Collection
UnclassifiedInvoiceLines                CommercialInvoiceLine Collection

CommercialInvoiceLine                          (Default Field: LineNo)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Invoice                                 CommercialInvoice
CountryOfOrigin                         Country
LinePrice                               Money
UnitPrice                               Money
CustomsQuantity                         ValueAndUnit
LineQuantity                            ValueAndUnit
NetWeight                               ValueAndUnit
Volume                                  ValueAndUnit
Weight                                  ValueAndUnit
ClassificationDetails                   String
ConcessionCode                          String
CustomAttrib1                           String
CustomAttrib2                           String
CustomAttrib3                           String
CustomAttrib4                           String
CustomAttrib5                           String
CustomAttrib6                           String
Description                             String
DutyAmount                              Decimal
DutyAmountsAsString                     String
DutyRateDescription                     String
GSTRate                                 Decimal
LineNo                                  Int
LookupCode                              String
MergedLineNumber                        String
OrderNumber                             String
PartNumber                              String
RefCountryCode                          String
TariffCode                              String

CustomsEntry                              (Default Field: EntryNumber)
======================================================================
Name                                    Type
----------------------------------------------------------------------
EntryType                               CodeAndDescription
Country                                 String
EntryCategory                           String
EntryNumber                             String
Information                             String
IssueDate                               DateTime

Container                                 (Default Field: ContainerNo)
======================================================================
Name                                    Type
----------------------------------------------------------------------
ArrivalContainerYardAddress             Address
DepartureContainerYardAddress           Address
ContainerQuality                        CodeAndDescription
DeliveryMode                            CodeAndDescription
Mode                                    CodeAndDescription
Type                                    ContainerType
ExportDetention                         Detention Information
ImportDetention                         Detention Information
FreightJob                              Freight
CFSClient                               Organisation
AirVentFlow                             ValueAndUnit
PackCount                               ValueAndUnit
SetPointTemperature                     ValueAndUnit
VolumeGoods                             Volume
WeightDunnage                           Weight
WeightGoods                             Weight
WeightGross                             Weight
WeightTare                              Weight
AMSNumber                               String
ArrivalCartageRef                       String
ArrivalEstimatedDelivery                DateTime
ArrivalReleaseNumber                    String
ArrivalSlotReference                    String
ArrivalSlotTime                         DateTime
BookingReference                        String
Chilled                                 Bool
ClipOnUnit                              String
ContainerCount                          Int
ContainerJobID                          String
ContainerNo                             String
ContainerNumberOrTypeCount              String
ContainerYardEmptyReturnGateIn          DateTime
ControlledAtmosphere                    Bool
Damaged                                 Bool
DepartureEstimatedPickup                DateTime
DepartureSlotReference                  String
DepartureSlotTime                       DateTime
EmptyReadyForReturn                     DateTime
EmptyRequired                           DateTime
EmptyReturnedBy                         DateTime
ExportDepotCustomsReference             String
Frozen                                  Bool
Height                                  Decimal
HumidityPercentage                      Byte
IsChargeable                            String
IsHazardous                             Bool
IsPalletized                            String
IsReefer                                Bool
ITReferenceNumber                       String
Length                                  Decimal
Packages                                String
Pallets                                 String
PrintTACImage                           Bool
ReleaseNumber                           String
SealNo                                  String
SealNo2                                 String
SealNo3                                 String
UnpackShed                              String
WharfGateOut                            DateTime
Width                                   Decimal

Commodities                             Commodity Collection
TranshipmentContainers                  Container Collection
Services                                ContainerService Collection
UNDGSubstances                          UNDGSubstance Collection

Commodity                          (Default Field: CodeAndDescription)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Code                                    String
CodeAndDescription                      String
Description                             String

ContainerService                                 (Default Field: Type)
======================================================================
Name                                    Type
----------------------------------------------------------------------
LocationAddress                         Address
Type                                    CodeAndDescription
Contractor                              Organisation
RequestedBy                             Organisation
DateBooked                              DateTime
DateCompleted                           DateTime
Note                                    String
ReferenceNumber                         String
ServiceCount                            Decimal
ServiceDocumentTitle                    String
ServiceDuration                         String
ServiceNote                             String

UNDGSubstance                     (Default Field: UNNumberWithVariant)
======================================================================
Name                                    Type
----------------------------------------------------------------------
DGContact                               Contact
ContainingPackage                       Package
Volume                                  Volume
Weight                                  Weight
EMSCode                                 String
FlashPoint                              String
IMOClass                                String
IsLimitedQuantity                       Bool
MarinePollutantWarning                  String
PackingGroup                            String
PackingInstructions                     String
ProperShippingName                      String
SubLabel1                               String
SubLabel2                               String
Summary                                 String
SummaryWithContainingPackageID          String
SummaryWithEMSCode                      String
TechnicalName                           String
UNNumber                                String
UNNumberWithVariant                     String
Variant                                 String
Variation                               String

RequiredDocuments                         (Default Field: Description)
======================================================================
Name                                    Type
----------------------------------------------------------------------
DateReceived                            DateTimeOffset
Description                             String
IsOriginalRequired                      Bool
IsReceived                              Bool
Type                                    String

PickupDeliveryConfirmations                 (Default Field: SignedFor)
======================================================================
Name                                    Type
----------------------------------------------------------------------
ConfirmationType                        String
Notes                                   String
PackagesConfirmed                       Int
PickupDeliveryTime                      DateTime
SignedFor                               String

LocalTransportLeg
======================================================================
Name                                    Type
----------------------------------------------------------------------
DeliverTo                               Address
PickupFrom                              Address
WaitPoint                               Address
Container                               Container
Cartage                                 Freight
BookedMove                              LocalTransportBookedMove
Client                                  Organisation
DeliveryClose                           String
DeliveryCloseHeading                    String
DeliveryReady                           String
DeliveryReadyHeading                    String
DeliverySignedFor                       String
DeliveryTimeDemurrage                   String
DeliveryTimeIn                          DateTime
DeliveryTimeOut                         DateTime
DisplayOrder                            Int
HasSignature                            Bool
HasWaitPoint                            Bool
IsDeliveryCTO                           Bool
IsDeliveryTheRequestedBooking           Bool
IsPickupContainerYard                   Bool
IsPickupCTO                             Bool
IsPickupTheRequestedBooking             Bool
IsWaitPointTheRequestedBooking          Bool
LegNotes                                String
PickupClose                             String
PickupCloseHeading                      String
PickupReady                             String
PickupReadyHeading                      String
PickupTimeDemurrage                     String
PickupTimeIn                            DateTime
PickupTimeOut                           DateTime
PlannedDeliveryTime                     DateTime
PlannedDeliveryTimeEnd                  DateTime
PlannedPickupTime                       DateTime
PlannedPickupTimeEnd                    DateTime
PlannedWaitPointTime                    DateTime
PlannedWaitPointTimeEnd                 DateTime
Remarks                                 String
Sequence                                Int
WaitPointClose                          String
WaitPointCloseHeading                   String
WaitPointReady                          String
WaitPointReadyHeading                   String
WaitPointTimeDemurrage                  String
WaitPointTimeIn                         DateTime
WaitPointTimeOut                        DateTime

Order                                         (Default Field: OrderNo)
======================================================================
Name                                    Type
----------------------------------------------------------------------
OrderDate                               DateTime
OrderNo                                 String

Lines                                   OrderLine Collection

OrderLine                                 (Default Field: Description)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Product                                 CodeAndDescription
Status                                  CodeAndDescription
ItemPrice                               Money
TotalLinePrice                          Money
QuantityInvoiced                        ValueAndUnit
QuantityOrdered                         ValueAndUnit
QuantityReceived                        ValueAndUnit
QuantityRemaining                       ValueAndUnit
Description                             String
InnerPacks                              Decimal
LineNumber                              Int
OuterPacks                              Decimal
RequiredDate                            DateTime
TotalInnerPacks                         Decimal

Package                                      (Default Field: Packages)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Commodity                               CodeAndDescription
Container                               Container
Dimensions                              Dimensions
Parent                                  Freight
Origin                                  Location
PackedItem                              PackedItem
DamagedPackages                         PackQTY
OutturnedPackages                       PackQTY
Packages                                PackQTY
PillagedPackages                        PackQTY
FumigatedPackages                       PackQTY
NonStackablePackages                    PackQTY
TopLoadOnlyPackages                     PackQTY
HeatTreatedPackages                     PackQTY
ISPMPalletPackages                      PackQTY
OutturnedVolume                         Volume
Volume                                  Volume
OutturnedWeight                         Weight
Weight                                  Weight
ContainerNo                             String
CustomAttribute1                        String
CustomAttribute2                        String
CustomAttribute3                        String
CustomAttribute4                        String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomFlag1                             Bool
CustomFlag2                             Bool
Description                             String
DisplayOrder                            String
HarmonizedCode                          String
HasPackedItem                           Bool
HouseBill                               String
Indent                                  String
IsExclusive                             Bool
IsPackageIdValidSSCCBarCode             Bool
IsTopLevelNonContainerisedPackage       Bool
IsTopLevelPackage                       Bool
ItmNumber                               Short
LinePrice                               Decimal
MarksAndNumbers                         String
MasterBill                              String
OutturnComment                          String
PackageBarcode                          String
PackageBarcodeWithSSCCPrefix            String
PackageBarcodeWithSSCCPrefixNonOptimisedEncoding  String
PackingOrder                            Int
PostcodeBarcode                         String
PostcodeBarcodeNumber                   String
PostcodeBarcodeNumberWithPrefix         String
PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText  String
RefNumber                               String
RefNumberWithSSCCPrefix                 String

PackedItems                             PackedItem Collection
Products                                PackProductWrapper Collection
UNDGSubstances                          UNDGSubstance Collection

PackProductWrapper                        (Default Field: ProductCode)
======================================================================
Name                                    Type
----------------------------------------------------------------------
ProductCode                             String

PackedItem
======================================================================
Name                                    Type
----------------------------------------------------------------------
Batch                                   String
BatchLabel                              String
Code                                    String
Description                             String
DescriptionSupplement                   String
Expiry                                  String
ExpiryLabel                             String
PackageInfo                             String
ProductBarcode                          String
ProductBarcodeWithPrefixes              String
ProductCodeStockUnitBarcodeNumber       String
TotalQty                                Decimal
TotalQtyUQ                              String

ExchangeRate
======================================================================
Name                                    Type
----------------------------------------------------------------------
Currency                                Currency
BuyRate                                 Decimal
SellRate                                Decimal
SellRateAgent                           Decimal

Cost
======================================================================
Name                                    Type
----------------------------------------------------------------------
ChargeCode                              CodeAndDescription
LocalCost                               Money
OSCost                                  Money
Creditor                                Organisation

Charge
======================================================================
Name                                    Type
----------------------------------------------------------------------
LocalCost                               Charge Breakdown
LocalSell                               Charge Breakdown
OSCost                                  Charge Breakdown
OSSell                                  Charge Breakdown
ChargeCode                              CodeAndDescription
CalculationDescription                  String
Description                             String

Milestone                                 (Default Field: Description)
======================================================================
Name                                    Type
----------------------------------------------------------------------
ActualDate                              DateTime
DateTime                                DateTime
Description                             String
EstimatedIsShownFlag                    String
EstimatedToBeShown                      Bool
ScheduledDate                           DateTime
Type                                    String
TypeDescription                         String

BookingInstruction
======================================================================
Name                                    Type
----------------------------------------------------------------------
Address                                 Address
RequiredFrom                            LabelValuePair
RequiredTo                              LabelValuePair
ConfirmationDescription                 String
ConfirmationID                          String
ConfirmationQuantity                    Int
ConfirmationReferenceNum                String
DropMode                                String
Equipment                               String
EstimatedOrSlot                         DateTime
HasSingleConfirmationForWholePackage    Bool
InstructionType                         String
PackageDimensions                       String
PackageDivotID                          String
PackageDivotQuantity                    Int
PackageDivotSequence                    Int
PackageID                               String
PackageType                             String
Sequence                                Int
ServiceInstruction                      String
Status                                  String

Address Collection - Available Types
======================================================================
Code                     Description
----------------------------------------------------------------------
Delivery                 Delivery Address
Main                     Office Address
Payables                 Accounts Payable Mailing Address
Pickup                   Consignment Pickup Address
Postal                   Consignment Postal Address
Receivables              Accounts Receivable Mailing Address
Sales                    Address for Sales Related Documents
Transport                Consignment Pickup or Delivery Address

Address                         (Default Field: CompanyNameAndAddress)
======================================================================
Name                                    Type
----------------------------------------------------------------------
AccessPoint                             CodeAndDescription
CommunicationRequired                   CodeAndDescription
ContainerHandling                       CodeAndDescription
DockHeight                              CodeAndDescription
LabourRequired                          CodeAndDescription
Country                                 Country
Location                                Location
Address                                 String
AddressAsASingleLine                    String
AddressCaption                          String
AddressLine1                            String
AddressLine2                            String
City                                    String
CompanyCode                             String
CompanyName                             String
CompanyNameAndAddress                   String
ContactName                             String
DeliverFromTime                         String
DeliverToTime                           String
DoNotAttendFromTime                     String
DoNotAttendToTime                       String
Email                                   String
Fax                                     String
FurtherConstraints                      String
Mobile                                  String
OtherWarehouseFacilities                String
Phone                                   String
PickupFromTime                          String
PickupToTime                            String
PostCode                                String

CustomsCodes                            RegistrationNumberCode Collection

RegistrationNumberCode Collection - Available Types
======================================================================
Code                     Description
----------------------------------------------------------------------
APC                      Accounts Payable Suppliers Reference
CAC                      Credit Agency Code
CAR                      Shipping Company Carrier/Principal Code
CBP                      Customs Brokerage Printer Code
CBR                      Customs Brokerage Registration Code
CBS                      Customs Brokerage Site Code
CCC                      Customs Carrier Code
CCD                      Customs Client Code
CCP                      Customs Controlled Premises Code
CMM                      Container Management Messaging Agreed Code
CMP                      Customs Manifest Provider Code
CSC                      Customs Supplier Code
DLV                      Deliverance System Code
DRV                      Driver's License Number
ECR                      External Creditor Account
EDR                      External Debtor Account
EID                      EDI Site ID
GBR                      Government Business Code
GCR                      Government Corporation Code
GS1                      GS1 Company Prefix
GST                      Government GST Code
GTN                      Global Tracking Name
GTX                      Government Tax File Code
INT                      INTTRA Code
LSC                      Legacy System Code
PAS                      Passport Number
PIM                      Participant Identification and Messaging Address
SID                      SEPA Creditor Identifier
UNC                      Universal Netting Code
UOC                      Universal Office Code

RegistrationNumberCode       (Default Field: RegistrationNumberOrCode)
======================================================================
Name                                    Type
----------------------------------------------------------------------
PremisesAddress                         Address
Type                                    CodeAndDescription
CountryOfIssue                          Country
RegistrationNumberOrCode                String

Note Collection - Available Types
======================================================================
Code                     Description
----------------------------------------------------------------------
A/P Account Management Notes  
A/R Account Management Notes  
A/R Credit Management Note  
Agent Notes              
AutoRating Log     
AWB Rateline Overtyped Notes  
Booking Notes            
Business Object Creation Stack Log  
Carrier Booking Request  
Cartage History Notes    
Certificate of Origin Notes  
Client Visible Job Notes  
Container Release Note   
Customs Delivery Instructions  
Customs Instruction Notes  
Customs Message Remarks  
Customs Quarantine Messaging Remarks  
Dangerous Goods Additional Handling Information  
Delivery Order / Receipt Notes  
Detailed Goods Description  
EXDOC Additional Information  
EXDOC Amendment Reason   
EXDOC Letter Of Credit   
EXDOC Notify Text        
Export Customs Handling Notes  
Export Pickup Instructions  
Export Receival Advice Remarks  
Extended Commercial Description  
Extra Order Details      
Fax/Email Transmission Log  
Forwarding Instruction Notes  
Full Job Role Description  
Gate Pass Notes          
Goods Handling Instructions  
Import Customs Handling Notes  
Import Delivery Instructions  
Inactive Record Details  
Internal Work Notes      
Invoice Details          
Invoicing Preferences      
Issue Resolution Notes   
Issue Working Notes      
Load List Instructions   
Manifest Goods Description  
Marks & Numbers          
Message Interpretation   
Message to Print on 7501  
Message to Print on B3 Document  
Opportunity Follow Up Note  
Order Management Note    
Order Management Update  
Order Update History     
Outturn Notes            
Payment Handling Instructions  
Picking Instructions     
Prealert/Arrival Notice Remarks  
Quote Cover Page Text
Receive Confirmation Instructions    
Special Instructions     
Survey Instruction       
Trade Lane Charge Information  
Trade Lane Charge Internal Note  
Transhipment Notes       
Unmatched Org Details    
Web User Note            

Note
======================================================================
Name                                    Type
----------------------------------------------------------------------
CreatedDate                             DateTime
Description                             String
Text                                    String

Organisation                    (Default Field: CompanyNameAndAddress)
======================================================================
Name                                    Type
----------------------------------------------------------------------
MainAddress                             Address
DGContact                               DGContact
ClosestPort                             Location
PartAttribute1                          PartAttribute
PartAttribute2                          PartAttribute
PartAttribute3                          PartAttribute
CompanyAddress                          String
CompanyCode                             String
CompanyName                             String
CompanyNameAndAddress                   String
ContactEmail                            String
ContactFax                              String
ContactName                             String
ContactPhone                            String
FirstLineOfCompanyNameAndAddress        String
LocalBusinessRegNo                      String
LocalVATCode                            String
MainEmail                               String
MainFax                                 String
MainPhone                               String
SCAC                                    String
TypeDescription                         String

Addresses                               Address Collection
AssignedStaff                           AssignedStaffMember Collection
Contacts                                Contact Collection
Notes                                   Note Collection
CustomsCodes                            RegistrationNumberCode Collection

Contact Collection - Available Types
======================================================================
Code                     Description
----------------------------------------------------------------------
Administration           Administration Contact
AirWholesaler            Air Wholesaler Contact
All                      All Purpose Contact
Consignee                Consignee Contact
Consignor                Consignor Contact
CTO                      CTO Contact
CustomerService          Customer Service Contact
Depot                    Depot Contact
ExportAirDepot           Export Air Depot Contact
ExportAirFreightAgent    Export Air Freight Agent Contact
ExportBroker             Export Broker Contact
ExportDepot              Export Depot Contact
ExportFreightAgent       Export Freight Agent Contact
ExportSeaDepot           Export Sea Depot Contact
ExportSeaFreightAgent    Export Sea Freight Agent Contact
FreightAgent             Freight Agent Contact
ImportAirDepot           Import Air Depot Contact
ImportAirFreightAgent    Import Air Freight Agent Contact
ImportBroker             Import Broker Contact
ImportDepot              Import Depot Contact
ImportFreightAgent       Import Freight Agent Contact
ImportSeaDepot           Import Sea Depot Contact
ImportSeaFreightAgent    Import Sea Freight Agent Contact
LocalClient              Local Client Contact
LocalTransport           Port Transport Contact
Marketing                Marketing Contact
NotifyParty              Notify Party Contact
Payables                 Accounts Payable Contact
Receivables              Accounts Receivable Contact
Sales                    Sales Contact
ShippingLine             Shipping Line Contact
TransportServices        Transport Services Contact
Warehouse                Warehouse Contact
Warehouse3PL             Warehouse 3PL Contact

Contact                                      (Default Field: FullName)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Address                                 Address
AttachmentType                          CodeAndDescription
Language                                CodeAndDescription
NotifyMode                              CodeAndDescription
Email                                   String
Extension                               String
Fax                                     String
FullName                                String
HomePhone                               String
JobCategory                             String
JobTitle                                String
Mobile                                  String
OtherPhone                              String
Pager                                   String
Phone                                   String
Salutation                              String

AssignedStaffMember                             (Default Field: Staff)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Staff                                   StaffMember
Category                                String
Relationship                            String

DGContact                                        (Default Field: Name)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Name                                    String
Phone                                   String
PhoneType                               String

Location                            (Default Field: UNLOCOAndPortName)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Country                                 Country
IATACode                                String
PortName                                String
State                                   String
UNLOCO                                  String
UNLOCOAndPortName                       String

Country                                   (Default Field: CodeAndName)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Code                                    String
CodeAndName                             String
EconomicGrouping                        String
ISONumericCode                          String
Name                                    String

PartAttribute                                    (Default Field: Name)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Description                             String
IsExpiryDateUsedByOrganisation          Bool
IsMandatory                             Bool
IsPackingDateUsedByOrganisation         Bool
IsPartAttributeUsedByOrganisation       Bool
Name                                    String
Type                                    String

CodeAndDescription                 (Default Field: CodeAndDescription)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Code                                    String
CodeAndDescription                      String
Description                             String

StaffMember                                  (Default Field: FullName)
======================================================================
Name                                    Type
----------------------------------------------------------------------
EmailAddress                            SecureDetail
FaxNum                                  SecureDetail
HomePhone                               SecureDetail
MobilePhone                             SecureDetail
WorkExtension                           SecureDetail
WorkPhone                               SecureDetail
FirstName                               String
FullName                                String
Title                                   String

SecureDetail                                   (Default Field: Public)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Internal                                String
IsPublished                             Bool
Public                                  String

Invoicing Job                                  (Default Field: JobNum)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Debtor                                  Organisation
JobNum                                  String

Rating Information
======================================================================
Name                                    Type
----------------------------------------------------------------------
OneOffShipment                          RatingOneOffShipment
SecondSignatory                         StaffMember
CoverPageFooterText                     String
CoverPageText                           String
InvoiceTermsText                        String
IsReprint                               Bool
PrimarySource                           String
QuotationTitle                          String
ValidFrom                               DateTime
ValidUntil                              DateTime

TrailingPages                           Image Collection
PublishedAirFreightAgents               Organisation Collection
PublishedSeaFreightAgents               Organisation Collection
PageSets                                Pricing Page Set Collection

Image                                           (Default Field: Image)
======================================================================
Name                                    Type
----------------------------------------------------------------------

Pricing Page Set
======================================================================
Name                                    Type
----------------------------------------------------------------------

OriginAndDestinationRates               Compound Line Collection
OriginDestinationAndChargeableTableRows  Compound Line Collection
OriginDestinationAndContainerTableRows  Compound Line Collection
OriginDestinationAndFreightRates        Compound Line Collection
OriginDestinationAndFreightTableRows    Compound Line Collection
PricingPages                            Pricing Page Collection

Pricing Page
======================================================================
Name                                    Type
----------------------------------------------------------------------
ClosingText                             String
Index                                   String
IndexNum                                Int
IsGSTApplicable                         Bool
OpeningText                             String

CFX                                     Pricing Page CFX Collection
DestinationRates                        Pricing Page Line Collection
FreightRates                            Pricing Page Line Collection
OriginRates                             Pricing Page Line Collection
ChargeableTableRows                     Pricing Page Table Row Collection
CompactDestinationTableRows             Pricing Page Table Row Collection
CompactFreightTableRows                 Pricing Page Table Row Collection
CompactOriginTableRows                  Pricing Page Table Row Collection
ContainerTableRows                      Pricing Page Table Row Collection
Entries                                 Rate Entry Collection

Rate Entry
======================================================================
Name                                    Type
----------------------------------------------------------------------
CommodityCode                           CodeAndDescription
ServiceLevel                            CodeAndDescription
TransportMode                           CodeAndDescription
Consignee                               Organisation
Consignor                               Organisation
Provider                                Organisation
Destination                             Rating Area
Origin                                  Rating Area
Via                                     Rating Area
Direction                               Rating Direction
Frequency                               Rating Frequency
TransitTime                             Rating Transit Time
ContractNumber                          String
DeliveryAddressPostCode                 String
DiscountDescription                     String
IsSupplementary                         Bool
Mode                                    String
OverseasCountries                       String
PageHeader                              String
PickUpAddressPostCode                   String
ValidFrom                               DateTime
ValidUntil                              DateTime

Pricing Page Table Row
======================================================================
Name                                    Type
----------------------------------------------------------------------
ConversionFactor                        Rating Conversion Factor
Index                                   Int

OtherCharges                            Pricing Page Line Collection
SubRows                                 Pricing Page Table Sub Row Collection
Entries                                 Rate Entry Collection

Pricing Page Table Sub Row
======================================================================
Name                                    Type
----------------------------------------------------------------------
Charge                                  CodeAndDescription
Currency                                Currency
CaptionGroup                            Int
Index                                   Int
Split                                   String

Columns                                 Pricing Page Table Column Collection

Pricing Page Table Column                       (Default Field: Value)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Heading                                 String
Value                                   String

Pricing Page Line
======================================================================
Name                                    Type
----------------------------------------------------------------------
IncoTerm                                CodeAndDescription
ParentEntry                             Rate Entry
Destination                             Rating Area
Origin                                  Rating Area
Via                                     Rating Area
Amount                                  MultilingualString
Currency                                String
Description                             String
IsGSTApplicable                         Bool
LineIndex                               String
LineIndexNum                            Int
NumOfIndents                            Int
SetIndex                                String
SetIndexNum                             Int
Units                                   MultilingualString
Validity                                String

Pricing Page CFX                         (Default Field: NameAndValue)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Name                                    String
NameAndValue                            String
Value                                   Decimal

Compound Line
======================================================================
Name                                    Type
----------------------------------------------------------------------
Page                                    Pricing Page
RateLine                                Pricing Page Line
Row                                     Pricing Page Table Row
SubRow                                  Pricing Page Table Sub Row
Label                                   String
LabelOrdinal                            Int
SubLabel                                String
SubLabelOrdinal                         Int

RatingOneOffShipment
======================================================================
Name                                    Type
----------------------------------------------------------------------
Commodity                               Commodity
InsuranceValue                          Money
Direction                               Rating Direction
Frequency                               Rating Frequency
TransitTime                             Rating Transit Time
Mode                                    String
NumberOfEntries                         Int
NumberOfEntryLines                      Int

Rating Direction                          (Default Field: Description)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Code                                    String
Description                             String
IsDestinationLocal                      Bool
IsOriginLocal                           Bool

Rating Frequency                         (Default Field: FriendlyText)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Unit                                    Unit
FriendlyText                            String
Value                                   Int

Unit                               (Default Field: CodeAndDescription)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Code                                    String
CodeAndDescription                      String
Description                             String

Rating Transit Time                      (Default Field: FriendlyText)
======================================================================
Name                                    Type
----------------------------------------------------------------------
FriendlyText                            String
Value                                   String

Money                           (Default Field: AmountAndCurrencyCode)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Currency                                Currency
Amount                                  Decimal
AmountAndCurrencyCode                   String

Currency                           (Default Field: CodeAndDescription)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Code                                    String
CodeAndDescription                      String
DecimalPlaces                           Int
Description                             String

Rating Area                                      (Default Field: Name)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Country                                 Country
Port                                    Location
Code                                    String
IsCountry                               Bool
IsPort                                  Bool
IsZone                                  Bool
Name                                    String

Rating Conversion Factor                       (Default Field: Metric)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Factor                                  Decimal
Metric                                  String

CartageInfo
======================================================================
Name                                    Type
----------------------------------------------------------------------
AvailableDate                           DateTime
CutOffDate                              DateTime
CutOffOrAvailableDate                   DateTime
EmailSubjectNumber                      String
EquipmentType                           String
FullCartageInstructions                 String
FullHandlingInstructions                String
IsAir                                   Bool
JourneyOneDeliverToContactName          String
JourneyOneDeliverToContactPhone         String
JourneyOneDeliverToDate                 DateTime
JourneyOneDeliverToDateHeading          String
JourneyOneDeliverToHeading              String
JourneyOneDeliverToRequiredByDate       DateTime
JourneyOneDeliverToRequiredByDateHeading  String
JourneyOnePickUpContactName             String
JourneyOnePickUpContactPhone            String
JourneyOnePickUpDate                    DateTime
JourneyOnePickUpDateHeading             String
JourneyOnePickUpHeading                 String
JourneyOnePickUpReleaseNum              String
JourneyOnePickUpRequiredByDate          DateTime
JourneyOnePickUpRequiredByDateHeading   String
JourneyOnePickUpSlofRef                 String
JourneyTwoDeliverToContactName          String
JourneyTwoDeliverToContactPhone         String
JourneyTwoDeliverToDate                 DateTime
JourneyTwoDeliverToDateHeading          String
JourneyTwoDeliverToHeading              String
JourneyTwoDeliverToReleaseNum           String
JourneyTwoDeliverToRequiredByDate       DateTime
JourneyTwoDeliverToRequiredByDateHeading  String
JourneyTwoDeliverToSlofRef              String
JourneyTwoPickUpContactName             String
JourneyTwoPickUpContactPhone            String
JourneyTwoPickUpDate                    DateTime
JourneyTwoPickUpDateHeading             String
JourneyTwoPickUpHeading                 String
JourneyTwoPickUpRequiredByDate          DateTime
JourneyTwoPickUpRequiredByDateHeading   String
LegNotes                                String
PickupOrStorageCommenceDate             DateTime
PickupOrStorageCommenceDateHeading      String
PrintAsContainers                       Bool
PrintJourneyOne                         Bool
PrintJourneyTwo                         Bool
PrintTwoJourneys                        Bool
ReceivalDate                            DateTime
StorageCommenceDate                     DateTime

LocalForwarderOrganisation      (Default Field: CompanyNameAndAddress)
======================================================================
Name                                    Type
----------------------------------------------------------------------
MainAddress                             Address
DGContact                               DGContact
ClosestPort                             Location
PartAttribute1                          PartAttribute
PartAttribute2                          PartAttribute
PartAttribute3                          PartAttribute
CompanyAddress                          String
CompanyCode                             String
CompanyName                             String
CompanyNameAndAddress                   String
ContactEmail                            String
ContactFax                              String
ContactName                             String
ContactPhone                            String
FirstLineOfCompanyNameAndAddress        String
LocalBusinessRegNo                      String
LocalVATCode                            String
MainEmail                               String
MainFax                                 String
MainPhone                               String
SCAC                                    String
TypeDescription                         String

Addresses                               Address Collection
AssignedStaff                           AssignedStaffMember Collection
Contacts                                Contact Collection
Notes                                   Note Collection
CustomsCodes                            RegistrationNumberCode Collection

ExportAgentOrganisation         (Default Field: CompanyNameAndAddress)
======================================================================
Name                                    Type
----------------------------------------------------------------------
MainAddress                             Address
DGContact                               DGContact
ClosestPort                             Location
PartAttribute1                          PartAttribute
PartAttribute2                          PartAttribute
PartAttribute3                          PartAttribute
CompanyAddress                          String
CompanyCode                             String
CompanyName                             String
CompanyNameAndAddress                   String
ContactEmail                            String
ContactFax                              String
ContactName                             String
ContactPhone                            String
FirstLineOfCompanyNameAndAddress        String
LocalBusinessRegNo                      String
LocalVATCode                            String
MainEmail                               String
MainFax                                 String
MainPhone                               String
SCAC                                    String
TypeDescription                         String

Addresses                               Address Collection
AssignedStaff                           AssignedStaffMember Collection
Contacts                                Contact Collection
Notes                                   Note Collection
CustomsCodes                            RegistrationNumberCode Collection

PlaceAndDate                                 (Default Field: Location)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Location                                Location
ActualDate                              DateTime
Date                                    DateTime
EstimatedDate                           DateTime

PackQTY                   (Default Field: ValueAndUnitCodeBlankIfZero)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Unit                                    Unit
Value                                   Decimal
ValueAndUnitCode                        String
ValueAndUnitCodeBlankIfZero             String

ValueAndUnit              (Default Field: ValueAndUnitCodeBlankIfZero)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Unit                                    Unit
Value                                   Decimal
ValueAndUnitCode                        String
ValueAndUnitCodeBlankIfZero             String

Weight                    (Default Field: ValueAndUnitCodeBlankIfZero)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Unit                                    Unit
InFreightWeightUnit                     Weight
InKilograms                             Weight
InPounds                                Weight
Value                                   Decimal
ValueAndUnitCode                        String
ValueAndUnitCodeBlankIfZero             String

Volume                    (Default Field: ValueAndUnitCodeBlankIfZero)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Unit                                    Unit
InCubicFeet                             Volume
InCubicMeters                           Volume
InFreightVolumeUnit                     Volume
Value                                   Decimal
ValueAndUnitCode                        String
ValueAndUnitCodeBlankIfZero             String

INCO Term                          (Default Field: CodeAndDescription)
======================================================================
Name                                    Type
----------------------------------------------------------------------
PaymentType                             CodeAndDescription
Code                                    String
CodeAndDescription                      String
Description                             String

Query Claim                                 (Default Field: JobNumber)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Contact                                 Contact
TransactionAmount                       Money
AccountType                             String
Amount                                  String
AssignedTo                              String
BranchCode                              String
Creator                                 String
Details                                 String
InvoiceNo                               String
JobNumber                               String
NextFollowUp                            DateTime
ReasonCode                              String
ReasonDescription                       String
ShortDescription                        String
StatusCode                              String
StatusDescription                       String
TypeCode                                String
TypeDescription                         String

WarehouseJob
======================================================================
Name                                    Type
----------------------------------------------------------------------
ConsigneeAddress                        Address
GoodsBillToAddress                      Address
SupplierDocAddress                      Address
TransportBillToAddress                  Address
TransportCoAddress                      Address
CODType                                 CodeAndDescription
DropMode                                CodeAndDescription
FulfillRule                             CodeAndDescription
IncoTerm                                CodeAndDescription
PickOption                              CodeAndDescription
ServiceLevel                            CodeAndDescription
ConfirmationInstructions                LabelValuePair
CustomerReference                       LabelValuePair
FinalisedDate                           LabelValuePair
HandlingInstructions                    LabelValuePair
PickingInstructions                     LabelValuePair
PickMethod                              LabelValuePair
PickNo                                  LabelValuePair
PickNumberReference                     LabelValuePair
PrimaryBarcode                          LabelValuePair
RequiredDate                            LabelValuePair
SecondaryReference                      LabelValuePair
SOPOrderNumber                          LabelValuePair
SOPRequiredDate                         LabelValuePair
SOPServiceLevel                         LabelValuePair
SOPSpecialInstructions                  LabelValuePair
SOPStagingAreaName                      LabelValuePair
SOPTransportCompany                     LabelValuePair
SplitNumber                             LabelValuePair
StagingAreaName                         LabelValuePair
Status                                  LabelValuePair
TransportReference                      LabelValuePair
WarehouseName                           LabelValuePair
WhoCreated                              LabelValuePair
WhoFinalised                            LabelValuePair
CODAmount                               Money
Insurance                               Money
TotalExtendedLinePrice                  Money
Client                                  Organisation
Consignee                               Organisation
JobClient                               Organisation
Supplier                                Organisation
TransportCompany                        Organisation
Destination                             PlaceAndDate
PackagesSent                            ValueAndUnit
CubicSent                               Volume
TotalOuterPackagesVolume                Volume
Warehouse                               WarehouseBO
TotalOuterPackagesWeight                Weight
WeightSent                              Weight
ABN                                     String
AccountCode                             String
ACSEstCode                              String
ArrivalDate                             DateTime
ATOEstCode                              String
BOLNumber                               String
CartageAdviceClosingText                String
CartageAdviceOpeningText                String
CartageDropMode                         String
ClientGCR                               String
ConsolidatedInvoiceRef                  String
ContainerNumberAndTypeLine              String
CP_IssueNo                              String
CurrencyCode                            String
DebtorCodeAndName                       String
DocumentTitle                           String
EnableDangerousGoodsDetails             Bool
EnableExtendedLinePrice                 Bool
FromDate                                DateTime
HasMultipleStockKeepingUnits            Bool
HasMultipleVolumeUnits                  Bool
HasMultipleWeightUnits                  Bool
InvoiceNumber                           String
IsCustomsTransaction                    Bool
IsWorkOrder                             Bool
IsWorkOrderPick                         Bool
JobNumber                               String
JobNumberBarcodeText                    String
JobNumberBarcodeTextForFont             String
JobNumberBarcodeTextWithoutDocManagerCodes  String
JobNumberHeading                        String
OuterPackagesContents                   String
PalletsSent                             Short
PrimaryBarcodeText                      String
PrintDGDetails                          String
ProductLinesCount                       Int
References                              String
ReferencesExtended                      String
ReportDescription                       String
SecondaryHeading                        String
SecondaryNumber                         String
SelectedArea                            String
SelectedClient                          String
SelectedCommodityCode                   String
SelectedCycle                           String
SelectedPickMethod                      String
SelectedRow                             String
SelectedSupplierPart                    String
SOPConsigneeAddressLabel                String
StocktakeNumber                         String
SubTypeDesc                             String
ToDate                                  DateTime
TotalNumberOfLabels                     Int
TotalNumberOfPackageLabels              Int
UnitsSent                               Decimal
WarehouseCartageCoordinatorName         String
WarehouseCartageCoordinatorPhone        String
WorkOrderLevels10th                     String
WorkOrderLevels1st                      String
WorkOrderLevels2nd                      String
WorkOrderLevels3rd                      String
WorkOrderLevels4th                      String
WorkOrderLevels5th                      String
WorkOrderLevels6th                      String
WorkOrderLevels7th                      String
WorkOrderLevels8th                      String
WorkOrderLevels9th                      String

Containers                              Container Collection
Packages                                Package Collection
Jobs                                    WarehouseJob Collection
BOMStagingAreaParts                     WarehouseJobLine Collection
JobLines                                WarehouseJobLine Collection
PackingLines                            WarehouseJobLine Collection
PalletizedInventory                     WarehouseJobLine Collection
PickingLines                            WarehouseJobLine Collection
VarianceLines                           WarehouseJobLine Collection
WorkOrderLines                          WarehouseJobLine Collection

WarehouseJobLine
======================================================================
Name                                    Type
----------------------------------------------------------------------
RecommendedUnitPrice                    LabelValuePair
UnitDiscountAmount                      LabelValuePair
UnitDiscountPercent                     LabelValuePair
UnitPriceAfterDiscount                  LabelValuePair
UnitsMet                                LabelValuePair
UnitsOrdered                            LabelValuePair
UnitsPicked                             LabelValuePair
UnitsShort                              LabelValuePair
ExtendedLinePrice                       Money
DangerousGoodsSubstance                 UNDGSubstance
AdditionalMoneys                        String
ArrivalDate                             DateTime
Attributes                              String
AttributeUnits                          String
BOMIndentation                          String
BOMLevel                                Int
ClientCode                              String
ClientName                              String
CustomAttrib1                           String
CustomAttrib2                           String
CustomAttrib3                           String
CustomAttrib4                           String
CustomAttrib5                           String
CustomAttrib6                           String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDate3                             DateTime
CustomDate4                             DateTime
CustomDate5                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomDecimal3                          Decimal
CustomDecimal4                          Decimal
CustomDecimal5                          Decimal
CustomFlag1                             Bool
CustomFlag2                             Bool
CustomFlag3                             Bool
CustomFlag4                             Bool
CustomFlag5                             Bool
CustomsAddInfo                          String
CustomsCtryOfOrigin                     String
CustomsEntryDate                        DateTime
CustomsEntryKey                         String
CustomsEntryLineNo                      Short
CustomsEntryNo                          String
CustomsQuantity                         Decimal
CustomsQuantityUQ                       String
CustomsSecondQuantity                   Decimal
CustomsSecondUnitQty                    String
CustomsThirdQuantity                    Decimal
CustomsThirdUnitQty                     String
ManufacturerAddress                     Address
CustomsTariffItem                       String
CustomsTILV                             Decimal
CustomsVFD                              Decimal
CustomTextBlob1                         String
DestLocationCaption                     String
DestPalletIDCaption                     String
DestWarehouseCaption                    String
ExpectedReceiptQuantity                 Decimal
ExpiryDate                              DateTime
ExpiryDateWithLabel                     String
ExtraDetails                            String
FirstDate                               String
FromLocationCaption                     String
FromPalletIDCaption                     String
FromWarehouseCaption                    String
GroupedInventoryUnits                   Decimal
GroupedReceivedWeight                   String
GroupedReceiveUnits                     Decimal
GroupedUnits                            Decimal
InventoryQty                            String
IsTopLevelOrEvenIndex                   String
IsTopLevelOrOddIndex                    String
LastCount                               Decimal
LeftOverAttributes                      String
LineComment                             String
LineNo                                  String
LocationColumn                          Short
LocationLevel                           Short
LocationString                          String
LocationString2                         String
PackingDate                             DateTime
PackingDateWithLabel                    String
PackQty                                 Decimal
Packs                                   Decimal
PacksUQ                                 String
PalletID                                String
PalletID2                               String
PalletIDBarcode                         String
Pallets                                 Decimal
PartAttribute1                          String
PartAttribute1Name                      String
PartAttribute1WithLabel                 String
PartAttribute2                          String
PartAttribute2Name                      String
PartAttribute2WithLabel                 String
PartAttribute3                          String
PartAttribute3Name                      String
PartAttribute3WithLabel                 String
PickMethod                              String
PositionAfterSorting                    String
PrimaryPreference                       String
ProductBrandName                        String
ProductCode                             String
ProductCodeBarcode                      String
ProductDescription                      String
ProductModel                            String
ReceivedQty                             String
ReleaseUnitsAndUQ                       String
SecondDate                              String
StagingAreaName                         String
Status                                  String
SubTotalUnits                           Decimal
SupplierProductDesc                     String
SystemUnits                             Decimal
Tariff                                  String
TransferFromWarehouseName               String
TransferToWarehouseName                 String
Units                                   Decimal
UnitsUQ                                 String
Variance                                Decimal
Volume                                  Decimal
VolumeUQ                                String
Weight                                  Decimal
WeightUQ                                String

WarehouseJobLine
======================================================================
Name                                    Type
----------------------------------------------------------------------
RecommendedUnitPrice                    LabelValuePair
UnitDiscountAmount                      LabelValuePair
UnitDiscountPercent                     LabelValuePair
UnitPriceAfterDiscount                  LabelValuePair
UnitsMet                                LabelValuePair
UnitsOrdered                            LabelValuePair
UnitsPicked                             LabelValuePair
UnitsShort                              LabelValuePair
ExtendedLinePrice                       Money
DangerousGoodsSubstance                 UNDGSubstance
AdditionalMoneys                        String
ArrivalDate                             DateTime
Attributes                              String
AttributeUnits                          String
BOMIndentation                          String
BOMLevel                                Int
ClientCode                              String
ClientName                              String
CustomAttrib1                           String
CustomAttrib2                           String
CustomAttrib3                           String
CustomAttrib4                           String
CustomAttrib5                           String
CustomAttrib6                           String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDate3                             DateTime
CustomDate4                             DateTime
CustomDate5                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomDecimal3                          Decimal
CustomDecimal4                          Decimal
CustomDecimal5                          Decimal
CustomFlag1                             Bool
CustomFlag2                             Bool
CustomFlag3                             Bool
CustomFlag4                             Bool
CustomFlag5                             Bool
CustomsAddInfo                          String
CustomsCtryOfOrigin                     String
CustomsEntryDate                        DateTime
CustomsEntryKey                         String
CustomsEntryLineNo                      Short
CustomsEntryNo                          String
CustomsQuantity                         Decimal
CustomsQuantityUQ                       String
CustomsTariffItem                       String
CustomsTILV                             Decimal
CustomsVFD                              Decimal
CustomTextBlob1                         String
DestLocationCaption                     String
DestPalletIDCaption                     String
DestWarehouseCaption                    String
ExpectedReceiptQuantity                 Decimal
ExpiryDate                              DateTime
ExpiryDateWithLabel                     String
ExtraDetails                            String
FirstDate                               String
FromLocationCaption                     String
FromPalletIDCaption                     String
FromWarehouseCaption                    String
GroupedInventoryUnits                   Decimal
GroupedReceivedWeight                   String
GroupedReceiveUnits                     Decimal
GroupedUnits                            Decimal
InventoryQty                            String
IsTopLevelOrEvenIndex                   String
IsTopLevelOrOddIndex                    String
LastCount                               Decimal
LeftOverAttributes                      String
LineComment                             String
LineNo                                  String
LocationColumn                          Short
LocationLevel                           Short
LocationString                          String
LocationString2                         String
PackingDate                             DateTime
PackingDateWithLabel                    String
PackQty                                 Decimal
Packs                                   Decimal
PacksUQ                                 String
PalletID                                String
PalletID2                               String
PalletIDBarcode                         String
Pallets                                 Decimal
PartAttribute1                          String
PartAttribute1Name                      String
PartAttribute1WithLabel                 String
PartAttribute2                          String
PartAttribute2Name                      String
PartAttribute2WithLabel                 String
PartAttribute3                          String
PartAttribute3Name                      String
PartAttribute3WithLabel                 String
PickMethod                              String
PositionAfterSorting                    String
ProductBrandName                        String
ProductCode                             String
ProductCodeBarcode                      String
ProductDescription                      String
ProductModel                            String
ReceivedQty                             String
ReleaseUnitsAndUQ                       String
SecondDate                              String
StagingAreaName                         String
Status                                  String
SubTotalUnits                           Decimal
SupplierProductDesc                     String
SystemUnits                             Decimal
TransferFromWarehouseName               String
TransferToWarehouseName                 String
Units                                   Decimal
UnitsUQ                                 String
Variance                                Decimal
Volume                                  Decimal
VolumeUQ                                String
Weight                                  Decimal
WeightUQ                                String

WarehouseJobLine
======================================================================
Name                                    Type
----------------------------------------------------------------------
RecommendedUnitPrice                    LabelValuePair
UnitDiscountAmount                      LabelValuePair
UnitDiscountPercent                     LabelValuePair
UnitPriceAfterDiscount                  LabelValuePair
UnitsMet                                LabelValuePair
UnitsOrdered                            LabelValuePair
UnitsPicked                             LabelValuePair
UnitsShort                              LabelValuePair
ExtendedLinePrice                       Money
DangerousGoodsSubstance                 UNDGSubstance
AdditionalMoneys                        String
ArrivalDate                             DateTime
Attributes                              String
AttributeUnits                          String
BOMIndentation                          String
BOMLevel                                Int
ClientCode                              String
ClientName                              String
CustomAttrib1                           String
CustomAttrib2                           String
CustomAttrib3                           String
CustomAttrib4                           String
CustomAttrib5                           String
CustomAttrib6                           String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDate3                             DateTime
CustomDate4                             DateTime
CustomDate5                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomDecimal3                          Decimal
CustomDecimal4                          Decimal
CustomDecimal5                          Decimal
CustomFlag1                             Bool
CustomFlag2                             Bool
CustomFlag3                             Bool
CustomFlag4                             Bool
CustomFlag5                             Bool
CustomsAddInfo                          String
CustomsCtryOfOrigin                     String
CustomsEntryDate                        DateTime
CustomsEntryKey                         String
CustomsEntryLineNo                      Short
CustomsEntryNo                          String
CustomsQuantity                         Decimal
CustomsQuantityUQ                       String
CustomsTariffItem                       String
CustomsTILV                             Decimal
CustomsVFD                              Decimal
CustomTextBlob1                         String
DestLocationCaption                     String
DestPalletIDCaption                     String
DestWarehouseCaption                    String
ExpectedReceiptQuantity                 Decimal
ExpiryDate                              DateTime
ExpiryDateWithLabel                     String
ExtraDetails                            String
FirstDate                               String
FromLocationCaption                     String
FromPalletIDCaption                     String
FromWarehouseCaption                    String
GroupedInventoryUnits                   Decimal
GroupedReceivedWeight                   String
GroupedReceiveUnits                     Decimal
GroupedUnits                            Decimal
InventoryQty                            String
IsTopLevelOrEvenIndex                   String
IsTopLevelOrOddIndex                    String
LastCount                               Decimal
LeftOverAttributes                      String
LineComment                             String
LineNo                                  String
LocationColumn                          Short
LocationLevel                           Short
LocationString                          String
LocationString2                         String
PackingDate                             DateTime
PackingDateWithLabel                    String
PackQty                                 Decimal
Packs                                   Decimal
PacksUQ                                 String
PalletID                                String
PalletID2                               String
PalletIDBarcode                         String
Pallets                                 Decimal
PartAttribute1                          String
PartAttribute1Name                      String
PartAttribute1WithLabel                 String
PartAttribute2                          String
PartAttribute2Name                      String
PartAttribute2WithLabel                 String
PartAttribute3                          String
PartAttribute3Name                      String
PartAttribute3WithLabel                 String
PickMethod                              String
PositionAfterSorting                    String
ProductBrandName                        String
ProductCode                             String
ProductCodeBarcode                      String
ProductDescription                      String
ProductModel                            String
ReceivedQty                             String
ReleaseUnitsAndUQ                       String
SecondDate                              String
StagingAreaName                         String
Status                                  String
SubTotalUnits                           Decimal
SupplierProductDesc                     String
SystemUnits                             Decimal
TransferFromWarehouseName               String
TransferToWarehouseName                 String
Units                                   Decimal
UnitsUQ                                 String
Variance                                Decimal
Volume                                  Decimal
VolumeUQ                                String
Weight                                  Decimal
WeightUQ                                String

WarehouseJobLine
======================================================================
Name                                    Type
----------------------------------------------------------------------
RecommendedUnitPrice                    LabelValuePair
UnitDiscountAmount                      LabelValuePair
UnitDiscountPercent                     LabelValuePair
UnitPriceAfterDiscount                  LabelValuePair
UnitsMet                                LabelValuePair
UnitsOrdered                            LabelValuePair
UnitsPicked                             LabelValuePair
UnitsShort                              LabelValuePair
ExtendedLinePrice                       Money
DangerousGoodsSubstance                 UNDGSubstance
AdditionalMoneys                        String
ArrivalDate                             DateTime
Attributes                              String
AttributeUnits                          String
BOMIndentation                          String
BOMLevel                                Int
ClientCode                              String
ClientName                              String
CustomAttrib1                           String
CustomAttrib2                           String
CustomAttrib3                           String
CustomAttrib4                           String
CustomAttrib5                           String
CustomAttrib6                           String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDate3                             DateTime
CustomDate4                             DateTime
CustomDate5                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomDecimal3                          Decimal
CustomDecimal4                          Decimal
CustomDecimal5                          Decimal
CustomFlag1                             Bool
CustomFlag2                             Bool
CustomFlag3                             Bool
CustomFlag4                             Bool
CustomFlag5                             Bool
CustomsAddInfo                          String
CustomsCtryOfOrigin                     String
CustomsEntryDate                        DateTime
CustomsEntryKey                         String
CustomsEntryLineNo                      Short
CustomsEntryNo                          String
CustomsQuantity                         Decimal
CustomsQuantityUQ                       String
CustomsTariffItem                       String
CustomsTILV                             Decimal
CustomsVFD                              Decimal
CustomTextBlob1                         String
DestLocationCaption                     String
DestPalletIDCaption                     String
DestWarehouseCaption                    String
ExpectedReceiptQuantity                 Decimal
ExpiryDate                              DateTime
ExpiryDateWithLabel                     String
ExtraDetails                            String
FirstDate                               String
FromLocationCaption                     String
FromPalletIDCaption                     String
FromWarehouseCaption                    String
GroupedInventoryUnits                   Decimal
GroupedReceivedWeight                   String
GroupedReceiveUnits                     Decimal
GroupedUnits                            Decimal
InventoryQty                            String
IsTopLevelOrEvenIndex                   String
IsTopLevelOrOddIndex                    String
LastCount                               Decimal
LeftOverAttributes                      String
LineComment                             String
LineNo                                  String
LocationColumn                          Short
LocationLevel                           Short
LocationString                          String
LocationString2                         String
PackingDate                             DateTime
PackingDateWithLabel                    String
PackQty                                 Decimal
Packs                                   Decimal
PacksUQ                                 String
PalletID                                String
PalletID2                               String
PalletIDBarcode                         String
Pallets                                 Decimal
PartAttribute1                          String
PartAttribute1Name                      String
PartAttribute1WithLabel                 String
PartAttribute2                          String
PartAttribute2Name                      String
PartAttribute2WithLabel                 String
PartAttribute3                          String
PartAttribute3Name                      String
PartAttribute3WithLabel                 String
PickMethod                              String
PositionAfterSorting                    String
ProductBrandName                        String
ProductCode                             String
ProductCodeBarcode                      String
ProductDescription                      String
ProductModel                            String
ReceivedQty                             String
ReleaseUnitsAndUQ                       String
SecondDate                              String
StagingAreaName                         String
Status                                  String
SubTotalUnits                           Decimal
SupplierProductDesc                     String
SystemUnits                             Decimal
TransferFromWarehouseName               String
TransferToWarehouseName                 String
Units                                   Decimal
UnitsUQ                                 String
Variance                                Decimal
Volume                                  Decimal
VolumeUQ                                String
Weight                                  Decimal
WeightUQ                                String

WarehouseJobLine
======================================================================
Name                                    Type
----------------------------------------------------------------------
RecommendedUnitPrice                    LabelValuePair
UnitDiscountAmount                      LabelValuePair
UnitDiscountPercent                     LabelValuePair
UnitPriceAfterDiscount                  LabelValuePair
UnitsMet                                LabelValuePair
UnitsOrdered                            LabelValuePair
UnitsPicked                             LabelValuePair
UnitsShort                              LabelValuePair
ExtendedLinePrice                       Money
DangerousGoodsSubstance                 UNDGSubstance
AdditionalMoneys                        String
ArrivalDate                             DateTime
Attributes                              String
AttributeUnits                          String
BOMIndentation                          String
BOMLevel                                Int
ClientCode                              String
ClientName                              String
CustomAttrib1                           String
CustomAttrib2                           String
CustomAttrib3                           String
CustomAttrib4                           String
CustomAttrib5                           String
CustomAttrib6                           String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDate3                             DateTime
CustomDate4                             DateTime
CustomDate5                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomDecimal3                          Decimal
CustomDecimal4                          Decimal
CustomDecimal5                          Decimal
CustomFlag1                             Bool
CustomFlag2                             Bool
CustomFlag3                             Bool
CustomFlag4                             Bool
CustomFlag5                             Bool
CustomsAddInfo                          String
CustomsCtryOfOrigin                     String
CustomsEntryDate                        DateTime
CustomsEntryKey                         String
CustomsEntryLineNo                      Short
CustomsEntryNo                          String
CustomsQuantity                         Decimal
CustomsQuantityUQ                       String
CustomsTariffItem                       String
CustomsTILV                             Decimal
CustomsVFD                              Decimal
CustomTextBlob1                         String
DestLocationCaption                     String
DestPalletIDCaption                     String
DestWarehouseCaption                    String
ExpectedReceiptQuantity                 Decimal
ExpiryDate                              DateTime
ExpiryDateWithLabel                     String
ExtraDetails                            String
FirstDate                               String
FromLocationCaption                     String
FromPalletIDCaption                     String
FromWarehouseCaption                    String
GroupedInventoryUnits                   Decimal
GroupedReceivedWeight                   String
GroupedReceiveUnits                     Decimal
GroupedUnits                            Decimal
InventoryQty                            String
IsTopLevelOrEvenIndex                   String
IsTopLevelOrOddIndex                    String
LastCount                               Decimal
LeftOverAttributes                      String
LineComment                             String
LineNo                                  String
LocationColumn                          Short
LocationLevel                           Short
LocationString                          String
LocationString2                         String
PackingDate                             DateTime
PackingDateWithLabel                    String
PackQty                                 Decimal
Packs                                   Decimal
PacksUQ                                 String
PalletID                                String
PalletID2                               String
PalletIDBarcode                         String
Pallets                                 Decimal
PartAttribute1                          String
PartAttribute1Name                      String
PartAttribute1WithLabel                 String
PartAttribute2                          String
PartAttribute2Name                      String
PartAttribute2WithLabel                 String
PartAttribute3                          String
PartAttribute3Name                      String
PartAttribute3WithLabel                 String
PickMethod                              String
PositionAfterSorting                    String
ProductBrandName                        String
ProductCode                             String
ProductCodeBarcode                      String
ProductDescription                      String
ProductModel                            String
ReceivedQty                             String
ReleaseUnitsAndUQ                       String
SecondDate                              String
StagingAreaName                         String
Status                                  String
SubTotalUnits                           Decimal
SupplierProductDesc                     String
SystemUnits                             Decimal
TransferFromWarehouseName               String
TransferToWarehouseName                 String
Units                                   Decimal
UnitsUQ                                 String
Variance                                Decimal
Volume                                  Decimal
VolumeUQ                                String
Weight                                  Decimal
WeightUQ                                String

WarehouseJobLine
======================================================================
Name                                    Type
----------------------------------------------------------------------
RecommendedUnitPrice                    LabelValuePair
UnitDiscountAmount                      LabelValuePair
UnitDiscountPercent                     LabelValuePair
UnitPriceAfterDiscount                  LabelValuePair
UnitsMet                                LabelValuePair
UnitsOrdered                            LabelValuePair
UnitsPicked                             LabelValuePair
UnitsShort                              LabelValuePair
ExtendedLinePrice                       Money
DangerousGoodsSubstance                 UNDGSubstance
AdditionalMoneys                        String
ArrivalDate                             DateTime
Attributes                              String
AttributeUnits                          String
BOMIndentation                          String
BOMLevel                                Int
ClientCode                              String
ClientName                              String
CustomAttrib1                           String
CustomAttrib2                           String
CustomAttrib3                           String
CustomAttrib4                           String
CustomAttrib5                           String
CustomAttrib6                           String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDate3                             DateTime
CustomDate4                             DateTime
CustomDate5                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomDecimal3                          Decimal
CustomDecimal4                          Decimal
CustomDecimal5                          Decimal
CustomFlag1                             Bool
CustomFlag2                             Bool
CustomFlag3                             Bool
CustomFlag4                             Bool
CustomFlag5                             Bool
CustomsAddInfo                          String
CustomsCtryOfOrigin                     String
CustomsEntryDate                        DateTime
CustomsEntryKey                         String
CustomsEntryLineNo                      Short
CustomsEntryNo                          String
CustomsQuantity                         Decimal
CustomsQuantityUQ                       String
CustomsTariffItem                       String
CustomsTILV                             Decimal
CustomsVFD                              Decimal
CustomTextBlob1                         String
DestLocationCaption                     String
DestPalletIDCaption                     String
DestWarehouseCaption                    String
ExpectedReceiptQuantity                 Decimal
ExpiryDate                              DateTime
ExpiryDateWithLabel                     String
ExtraDetails                            String
FirstDate                               String
FromLocationCaption                     String
FromPalletIDCaption                     String
FromWarehouseCaption                    String
GroupedInventoryUnits                   Decimal
GroupedReceivedWeight                   String
GroupedReceiveUnits                     Decimal
GroupedUnits                            Decimal
InventoryQty                            String
IsTopLevelOrEvenIndex                   String
IsTopLevelOrOddIndex                    String
LastCount                               Decimal
LeftOverAttributes                      String
LineComment                             String
LineNo                                  String
LocationColumn                          Short
LocationLevel                           Short
LocationString                          String
LocationString2                         String
PackingDate                             DateTime
PackingDateWithLabel                    String
PackQty                                 Decimal
Packs                                   Decimal
PacksUQ                                 String
PalletID                                String
PalletID2                               String
PalletIDBarcode                         String
Pallets                                 Decimal
PartAttribute1                          String
PartAttribute1Name                      String
PartAttribute1WithLabel                 String
PartAttribute2                          String
PartAttribute2Name                      String
PartAttribute2WithLabel                 String
PartAttribute3                          String
PartAttribute3Name                      String
PartAttribute3WithLabel                 String
PickMethod                              String
PositionAfterSorting                    String
ProductBrandName                        String
ProductCode                             String
ProductCodeBarcode                      String
ProductDescription                      String
ProductModel                            String
ReceivedQty                             String
ReleaseUnitsAndUQ                       String
SecondDate                              String
StagingAreaName                         String
Status                                  String
SubTotalUnits                           Decimal
SupplierProductDesc                     String
SystemUnits                             Decimal
TransferFromWarehouseName               String
TransferToWarehouseName                 String
Units                                   Decimal
UnitsUQ                                 String
Variance                                Decimal
Volume                                  Decimal
VolumeUQ                                String
Weight                                  Decimal
WeightUQ                                String

LabelValuePair                                  (Default Field: Value)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Label                                   String
LabelAndValue                           String
Value                                   String
ValueAsDate                             DateTime

WarehouseBO                                      (Default Field: Name)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Address                                 Address
Code                                    String
Name                                    String
NameAndAddress                          String
PhoneAndFax                             String
TypeDescription                         String

RunSheet
======================================================================
Name                                    Type
----------------------------------------------------------------------
DriversName                             String
EndDate                                 DateTime
StartDate                               DateTime
TransportCompanyName                    String
VehicleRegistration                     String

Transport                                   (Default Field: Reference)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Mode                                    CodeAndDescription
FlightDate                              DateTime
FlightNo                                String
LloydsNo                                String
Reference                               String
ReferenceLabel                          String
VesselName                              String
VoyageDate                              DateTime
VoyageNo                                String

ContainerType                      (Default Field: CodeAndDescription)
======================================================================
Name                                    Type
----------------------------------------------------------------------
TareWeight                              ValueAndUnit
Code                                    String
CodeAndDescription                      String
Description                             String
ISOCode                                 String

Detention Information          (Default Field: FormattedDetentionDays)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Empty                                   Detention Information
Port                                    Location
DetentionDays                           Int
FormattedDetentionDays                  String
FreeDays                                Int
IsNearDue                               Bool
IsNotDue                                Bool
IsOverdue                               Bool
LastFreeDay                             DateTime
OverdueDays                             Int
Released                                DateTime
Returned                                DateTime

LocalTransportBookedMove
======================================================================
Name                                    Type
----------------------------------------------------------------------
DeliveryDocAddress                      Address
PickupDocAddress                        Address
Container                               Container
Cartage                                 Freight
BookedDimensionUnits                    String
BookedHeight                            Decimal
BookedLength                            Decimal
BookedPackages                          Int
BookedPackType                          String
BookedVolume                            Decimal
BookedVolumeUnit                        String
BookedWeight                            Decimal
BookedWeightUnit                        String
BookedWidth                             Decimal
DropMode                                String

LocalTransportLegs                      LocalTransportLeg Collection

Freight                                     (Default Field: JobNumber)
======================================================================
Name                                    Type
----------------------------------------------------------------------
ContainerYardEmptyPickupAddress         Address
ContainerYardEmptyReturnAddress         Address
DeliveryAddress                         Address
ExportReceivalAddress                   Address
ExportReceivingCTOAddress               Address
ExportReceivingDepotAddress             Address
GoodsAvailableAt                        Address
ImportArrivalCTOAddress                 Address
PickupAddress                           Address
PickupCFSAddress                        Address
UnpackCFSAddress                        Address
CartageInfo                             CartageInfo
CaratagePickupMode                      CodeAndDescription
ConsolContainerMode                     CodeAndDescription
ConsolTransportMode                     CodeAndDescription
ConsolType                              CodeAndDescription
OrderTransportMode                      CodeAndDescription
ReleaseType                             CodeAndDescription
ServiceLevel                            CodeAndDescription
ShipmentContainerMode                   CodeAndDescription
ShipmentStatus                          CodeAndDescription
ShipmentTransportMode                   CodeAndDescription
ShipmentType                            CodeAndDescription
ShippedOnBoardType                      CodeAndDescription
ExportAgent                             ExportAgentOrganisation
ParentJob                               Freight
TranshipmentFreightConsol               Freight
GateTransport                           GateTransport
IncoTerm                                INCO Term
InvoicingJob                            Invoicing Job
LocalForwarder                          LocalForwarderOrganisation
DeliveryLocation                        Location
FreightPayableAt                        Location
PickupLocation                          Location
CollectAmount                           Money
FreightRate                             Money
GoodsValue                              Money
InsuranceValue                          Money
AssuredParty                            Organisation
BookingParty                            Organisation
Buyer                                   Organisation
Carrier                                 Organisation
ClaimsPayableBy                         Organisation
Client                                  Organisation
Consignee                               Organisation
Consignor                               Organisation
ConsolCreditor                          Organisation
Consolidator                            Organisation
CTOArrival                              Organisation
DeliveryAgent                           Organisation
ExportBroker                            Organisation
ImportAgent                             Organisation
ImportBroker                            Organisation
InsuredBy                               Organisation
JobHeaderLocalClient                    Organisation
MainShipToParty                         Organisation
NotifyParty                             Organisation
PickupAgent                             Organisation
Principal                               Organisation
RecommendedAgent                        Organisation
SellingParty                            Organisation
StuffingLocation                        Organisation
Supplier                                Organisation
SurveyReportParty                       Organisation
ShipmentInnerPacksQty                   PackQTY
ShipmentOuterPacksQty                   PackQTY
Destination                             PlaceAndDate
FirstForeignPort                        PlaceAndDate
LastForeignPort                         PlaceAndDate
Origin                                  PlaceAndDate
PortOfFirstArrival                      PlaceAndDate
QueryClaim                              Query Claim
Rating                                  Rating Information
InterestedRoute                         Route
RunSheet                                RunSheet
SalesRep                                StaffMember
ChargeableWeight                        ValueAndUnit
StorageTime                             ValueAndUnit
Volume                                  Volume
WarehouseJob                            WarehouseJob
Weight                                  Weight
ActualReceive                           DateTime
AdditionalTerms                         String
ArrivalReference                        String
BookingReference                        String
CAPreviousCCN                           String
CargoControlNumberForCanada             String
CATransactionNo                         String
ConsolDateCreated                       DateTime
ConsolNumber                            String
ConsolPaymentType                       String
ConsolReference                         String
ContainerLayoutStyle                    String
ContainerSummary                        String
CTOArrivalBerth                         String
CustomAttribute1                        String
CustomAttribute2                        String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomerReference                       String
CustomFlag1                             Bool
CustomFlag2                             Bool
CustomsEntryNumber                      String
DeliveryCartageAdvised                  DateTime
DeliveryEstimated                       DateTime
DeliveryFrom                            DateTime
DeliveryGoodsDelivered                  DateTime
DeliveryRequiredBy                      DateTime
DepartureOrArrivalText                  String
ExportAgentsReference                   String
ExWorksRequiredBy                       DateTime
FactoryEx                               DateTime
FreightDepotType                        String
GoodsDescription                        String
HasTACImage                             Bool
Hazardous                               Bool
HBLContainerMode                        String
HouseBill                               String
HouseBillHeading                        String
HouseBillIssue                          DateTime
ImportAgentsReference                   String
JobNumber                               String
JobNumberBarcodeText                    String
JobNumberBarcodeTextForFont             String
JobNumberBarcodeTextWithoutDocManagerCodes  String
JobNumberHeading                        String
LoadingMeters                           Decimal
LocalForwarderReference                 String
MarksAndNumbers                         String
MasterBill                              String
MasterBillHeading                       String
MasterBillIssue                         DateTime
NoCopyBills                             Int
NoOriginalBills                         Int
NotClearedByAgentExpiryDate             DateTime
NotClearedByAgentIssueDate              DateTime
NotClearedByAgentNumber                 String
NotClearedByAgentStatement              String
OrderDate                               DateTime
OrderNumbersWithOwnersReference         String
OwnerReference                          String
PickupCartageAdvised                    DateTime
PickupDateOfReceipt                     DateTime
PickupTruckWaitCharge                   Decimal
PickupTruckWaitTime                     String
PickupFrom                              DateTime
PickupGoodsPickedup                     DateTime
PickupInterimReceipt                    String
PickupLabourCharge                      Decimal
PickupLabourTime                        String
PickupRequiredBy                        DateTime
PreviousCargoControlNumberForCanada     String
QuoteNumber                             String
Refrigerated                            Bool
SecondaryHeading                        String
SecondaryNumber                         String
ShipmentDateCreated                     DateTime
ShippedOnBoardDate                      DateTime
ShippersReference                       String
TransportReference                      String
UnAllocatedPackages                     Int
UnAllocatedVolume                       Decimal
UnAllocatedWeight                       Decimal
WarehouseLocation                       String

TransportAddresses                      Address Collection
TransportAddressesWithWarehousing       Address Collection
AutoRatedInfosForJobRevenue             AutoRateInformation Collection
Charges                                 Charge Collection
CommercialInvoices                      CommercialInvoice Collection
CommercialInvoiceLines                  CommercialInvoiceLine Collection
Containers                              Container Collection
TranshipmentContainers                  Container Collection
Services                                ContainerService Collection
Costs                                   Cost Collection
CustomsEntries                          CustomsEntry Collection
ExchangeRates                           ExchangeRate Collection
FreightConsolidations                   Freight Collection
FreightJobs                             Freight Collection
TransportBookings                       Freight Collection
BookingInstructions                     Instruction Collection
LocalTransportLegs                      LocalTransportLeg Collection
Milestones                              Milestone Collection
Notes                                   Note Collection
NotesIncludingRelated                   Note Collection
Orders                                  Order Collection
Packages                                Package Collection
PackProducts                            PackProductWrapper Collection
PickupDeliveryConfirmations             PickupDeliveryConfirmations Collection
RequiredDocuments                       RequiredDocuments Collection
ConsolRoutes                            Route Collection
ShipmentRoutes                          Route Collection
UNDGs                                   UNDGSubstance Collection
ConsigneeRequiredTaxNumber              String
MasterBillConsigneeOverrideRequiredTaxNumber  String
ConsignorRequiredTaxNumber              String
MasterBillShipperOverrideRequiredTaxNumber  String
NotifyPartyRequiredTaxNumber            String

Container                                 (Default Field: ContainerNo)
======================================================================
Name                                    Type
----------------------------------------------------------------------
ArrivalContainerYardAddress             Address
DepartureContainerYardAddress           Address
ContainerQuality                        CodeAndDescription
DeliveryMode                            CodeAndDescription
Mode                                    CodeAndDescription
Type                                    ContainerType
ExportDetention                         Detention Information
ImportDetention                         Detention Information
FreightJob                              Freight
CFSClient                               Organisation
AirVentFlow                             ValueAndUnit
PackCount                               ValueAndUnit
SetPointTemperature                     ValueAndUnit
VolumeGoods                             Volume
WeightDunnage                           Weight
WeightGoods                             Weight
WeightGross                             Weight
WeightTare                              Weight
AMSNumber                               String
ArrivalCartageRef                       String
ArrivalEstimatedDelivery                DateTime
ArrivalReleaseNumber                    String
ArrivalSlotReference                    String
ArrivalSlotTime                         DateTime
BookingReference                        String
Chilled                                 Bool
ClipOnUnit                              String
ContainerCount                          Int
ContainerJobID                          String
ContainerNo                             String
ContainerNumberOrTypeCount              String
ContainerYardEmptyReturnGateIn          DateTime
ControlledAtmosphere                    Bool
Damaged                                 Bool
DepartureEstimatedPickup                DateTime
DepartureSlotReference                  String
DepartureSlotTime                       DateTime
EmptyReadyForReturn                     DateTime
EmptyRequired                           DateTime
EmptyReturnedBy                         DateTime
ExportDepotCustomsReference             String
Frozen                                  Bool
Height                                  Decimal
HumidityPercentage                      Byte
IsChargeable                            String
IsHazardous                             Bool
IsPalletized                            String
IsReefer                                Bool
ITReferenceNumber                       String
Length                                  Decimal
Packages                                String
Pallets                                 String
PrintTACImage                           Bool
ReleaseNumber                           String
SealNo                                  String
SealNo2                                 String
SealNo3                                 String
UnpackShed                              String
WharfGateOut                            DateTime
Width                                   Decimal

Commodities                             Commodity Collection
TranshipmentContainers                  Container Collection
Services                                ContainerService Collection
UNDGSubstances                          UNDGSubstance Collection

Dimensions                (Default Field: ValueAndUnitCodeBlankIfZero)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Unit                                    Unit
Height                                  Decimal
HeightAndUnitCode                       String
HeightAndUnitCodeBlankIfZero            String
Length                                  Decimal
LengthAndUnitCode                       String
LengthAndUnitCodeBlankIfZero            String
Value                                   String
ValueAndUnitCode                        String
ValueAndUnitCodeBlankIfZero             String
Width                                   Decimal
WidthAndUnitCode                        String
WidthAndUnitCodeBlankIfZero             String

Charge Breakdown                              (Default Field: WithTax)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Currency                                Currency
Tax                                     Money
WithoutTax                              Money
WithTax                                 Money
";
		#endregion

		public void TestGenerateMapIncludingChildrenAndRelatedObjectsWithoutIFirstLevelIBusiness()
		{
			AssertMultilineASCIIEquals(">>> Use Araxis Merge to see changes in the map. <<<"
				, TestMapIncludingChildrenAndRelatedObjectsWithoutIFirstLevelIBusiness.Trim()
				, GenericWrapperMapper.GetMapAsText(typeof(MainWrapper), true, false));
		}
		#region TestMapIncludingChildrenAndRelatedObjectsWithoutIFirstLevelIBusiness
		const string TestMapIncludingChildrenAndRelatedObjectsWithoutIFirstLevelIBusiness = @"
Adult
======================================================================
Name                                    Type
----------------------------------------------------------------------
Relation                                Relation
Code                                    String
Description                             String

ChildrenBought                          Child Collection
ChildrenSold                            Child Collection

Child Collection - Available Types
======================================================================
Code                     Description
----------------------------------------------------------------------
Dumb                     Dumb It Down
Dumber                   Dumber Then Dumb

Child
======================================================================
Name                                    Type
----------------------------------------------------------------------
ChildFieldA                             String
ChildFieldB                             Decimal

Relation                                (Default Field: RelatedFieldA)
======================================================================
Name                                    Type
----------------------------------------------------------------------
RelatedFieldA                           String
RelatedFieldB                           Int
";
		#endregion

		public void TestGenerateMapIncludingChildrenAndRelatedObjectsWithIFirstLevelIBusiness()
		{
			AssertMultilineASCIIEquals(">>> Use Araxis Merge to see changes in the map. <<<"
				, TestMapIncludingChildrenAndRelatedObjectsWithIFirstLevelIBusiness.Trim()
				, GenericWrapperMapper.GetMapAsText(typeof(MainWrapper), true, true));
		}
		#region TestMapIncludingChildrenAndRelatedObjectsWithIFirstLevelIBusiness
		const string TestMapIncludingChildrenAndRelatedObjectsWithIFirstLevelIBusiness = @"
Adult
======================================================================
Name                                    Type
----------------------------------------------------------------------
Cousin                                  LongDistanceRelation
Relation                                Relation
Code                                    String
Description                             String

ChildrenBought                          Child Collection
ChildrenSold                            Child Collection

Child Collection - Available Types
======================================================================
Code                     Description
----------------------------------------------------------------------
Dumb                     Dumb It Down
Dumber                   Dumber Then Dumb

Child
======================================================================
Name                                    Type
----------------------------------------------------------------------
ChildFieldA                             String
ChildFieldB                             Decimal

Relation                                (Default Field: RelatedFieldA)
======================================================================
Name                                    Type
----------------------------------------------------------------------
RelatedFieldA                           String
RelatedFieldB                           Int

LongDistanceRelation
======================================================================
Name                                    Type
----------------------------------------------------------------------
Age                                     Int
FullName                                String
";
		#endregion

		#region Test Objects
		[WrapperTypeName("Adult")]
		public class MainWrapper : GenericWrapper
		{
			public MainWrapper(BusinessObjectFactory factory)
				: base(factory.New<DummyBusinessObject>(), factory)
			{
			}

			protected new DummyBusinessObject WrappedBO
			{
				get { return (DummyBusinessObject)base.WrappedBO; }
			}

			public ZString Code
			{
				get { return WrappedBO.Z0_Code; }
			}

			public ChildWrapperCollection ChildrenBought
			{
				get { return new ChildWrapperCollection(Factory); }
			}

			public ChildWrapperCollection ChildrenSold
			{
				get { return new ChildWrapperCollection(Factory); }
			}

			public RelationWrapper Relation
			{
				get { return null; }
			}

			public ZString Description
			{
				get { return WrappedBO.Z0_Description; }
			}

			public LongDistanceRelation Cousin
			{
				get { return new LongDistanceRelation(Factory); }
			}
		}

		[DefaultField("RelatedFieldA")]
		public class RelationWrapper : GenericWrapper
		{
			public RelationWrapper(BusinessObjectFactory factory)
				: base(factory.New<DummyBusinessObject>(), factory)
			{
			}

			protected new DummyBusinessObject WrappedBO
			{
				get { return (DummyBusinessObject)base.WrappedBO; }
			}

			public ZString RelatedFieldA
			{
				get { return WrappedBO.Z0_Code; }
			}

			public ZInt RelatedFieldB
			{
				get { return WrappedBO.Z0_Number; }
			}
		}

		public class ChildWrapper : GenericWrapper
		{
			public ChildWrapper(BusinessObjectFactory factory)
				: base(factory.New<DummyChildBusinessObject>(), factory)
			{
			}

			protected new DummyChildBusinessObject WrappedBO
			{
				get { return (DummyChildBusinessObject)base.WrappedBO; }
			}

			public ZString ChildFieldA
			{
				get { return WrappedBO.Z0_Code; }
			}

			public ZDecimal ChildFieldB
			{
				get { return WrappedBO.Z0_Decimal; }
			}

			public ZGuid ChildC
			{
				get { return WrappedBO.Z0_Guid; }
			}
		}

		[CustomIndexerList(typeof(DummyListForTesting))]
		public class ChildWrapperCollection : GenericWrapperCollection<ChildWrapper>
		{
			public ChildWrapperCollection(BusinessObjectFactory factory)
				: base(new DummyChildBusinessObjectCollection(factory), factory)
			{
			}
		}

		public class LongDistanceRelation : NonPersistentBusinessObject
		{
			public LongDistanceRelation(BusinessObjectFactory factory) : base(factory) { }

			public DummyBusinessObject NestedIBusinessObject
			{
				get { return Factory.New<DummyBusinessObject>(); }
			}

			public DummyBusinessObjectCollection NestedIBusinessObjectCollection
			{
				get { return new DummyBusinessObjectCollection(Factory); }
			}

			public ZInt Age
			{
				get { return new ZInt(); }
			}

			public ZString FullName
			{
				get { return new ZString(); }
			}
		}

		class GenericWrapperWith2LevelsOfDocBaseWrappers : GenericWrapper
		{
			public GenericWrapperWith2LevelsOfDocBaseWrappers()
				: base(null, null)
			{
			}

			public DocBaseWrapper1 DocBaseWrapper1
			{
				get { return new DocBaseWrapper1(); }
			}
		}

		class DocBaseWrapper1 : DocBaseWrapper
		{
			public DocBaseWrapper1()
				: base(null, null)
			{
			}

			public DocBaseWrapper2 DocBaseWrapper2
			{
				get { return new DocBaseWrapper2(); }
			}
		}

		class DocBaseWrapper2 : DocBaseWrapper
		{
			public DocBaseWrapper2()
				: base(null, null)
			{
			}
		}

		#endregion
	}
}
