using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocOrganisationIRS1099))]
	sealed class DocOrganisationIRS1099Test : DocOrganisationTest
	{
		public void TestPayAmount()
		{
			GlbBranch otherCompanysBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			Header.FillWithValidTestData();
			OrgHeader otherHeader = Factory.NewWithValidTestData<OrgHeader>();

			APPayment apPay1 = Factory.NewWithValidTestData<APPayment>();
			apPay1.AH_OSTotal = apPay1.AH_InvoiceAmount = apPay1.AH_OutstandingAmount = 1m;
			apPay1.AH_OH = Header.PK;

			APPayment apPay2 = Factory.NewWithValidTestData<APPayment>();
			apPay2.AH_GB = otherCompanysBranch.PK;
			apPay2.AH_OSTotal = apPay2.AH_InvoiceAmount = apPay2.AH_OutstandingAmount = 2m;
			apPay2.AH_OH = Header.PK;

			ARPayment arPay1 = Factory.NewWithValidTestData<ARPayment>();
			arPay1.AH_OSTotal = arPay1.AH_InvoiceAmount = arPay1.AH_OutstandingAmount = 4m;
			arPay1.AH_OH = Header.PK;

			APPayment apPay3 = Factory.NewWithValidTestData<APPayment>();
			apPay3.AH_PostDate = new ZDateTime(2007, 1, 1);
			apPay3.AH_OSTotal = apPay3.AH_InvoiceAmount = apPay3.AH_OutstandingAmount = 8m;
			apPay3.AH_OH = Header.PK;

			APPayment apPay4 = Factory.NewWithValidTestData<APPayment>();
			apPay4.AH_OSTotal = apPay4.AH_InvoiceAmount = apPay4.AH_OutstandingAmount = 16m;
			apPay4.AH_OH = otherHeader.PK;

			Factory.Save();

			DocOrganisationIRS1099 orgWrapper = DocOrganisationIRS1099.New(Header, Factory);
			orgWrapper.Parameters = new Dictionary<string, object> { { "Calendar Year", ZDateTime.Now.Year } };
			AssertEquals("Results for current year", 1m, orgWrapper.PayAmount);

			orgWrapper.Parameters = new Dictionary<string, object> { { "Calendar Year", 2007 } };
			AssertEquals("Results for 2007", 8m, orgWrapper.PayAmount);
		}

		protected override string ExpectedFieldMap => @"
DocOrganisationIRS1099                           (Default Field: Name)
======================================================================
Name                                    Type
----------------------------------------------------------------------
EXFreightBillTo                         DocOrganisation
IMFreightBillTo                         DocOrganisation
ABN                                     String
AdditionalAddressInformation            String
Address1                                String
Address2                                String
ApprovedExporterCode                    String
ARCreditManagementNote                  String
BusinessRegNo                           String
BusinessRegType                         String
CarrierAirlinePrefix                    String
CarrierMasterBillPrefix                 String
CartageInstructions                     String
CBR                                     String
CCC                                     String
CCD                                     String
CID                                     String
City                                    String
Code                                    String
Context                                 String
CSC                                     String
CurrentDate                             DateTime
CustomAttrib1                           String
CustomAttrib2                           String
CustomAttrib3                           String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDate3                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomDecimal3                          Decimal
CustomFlag1                             Bool
CustomFlag2                             Bool
CustomFlag3                             Bool
DGContactName                           String
DGContactPhone                          String
DisbursmentTerms                        String
ECRCode                                 String
EIN                                     String
Email                                   String
Fax                                     String
GST                                     String
HandlingInstructions                    String
HasPartAttrib1                          Bool
HasPartAttrib2                          Bool
HasPartAttrib3                          Bool
HasSerialNumber                         Bool
IsActive                                Bool
IsAirCTO                                Bool
IsAirLine                               Bool
IsAirWholesaler                         Bool
IsBroker                                Bool
IsCompetitor                            Bool
IsConsignee                             Bool
IsConsignor                             Bool
IsContainerPark                         Bool
IsCreditor                              Bool
IsDebtor                                Bool
IsForwarder                             Bool
IsInlandWaterwayProvider                Bool
IsLineHaulProvider                      Bool
IsLocalTransport                        Bool
IsMiscFreightServices                   Bool
IsPackDepot                             Bool
IsRailProvider                          Bool
IsSalesLead                             Bool
IsSeaCTO                                Bool
IsSeaWholesaler                         Bool
IsShippingLine                          Bool
IsShippingProvider                      Bool
IsTempAccount                           Bool
IsTransportClient                       Bool
IsUnpackDepot                           Bool
IsUserFlag1                             Bool
IsUserFlag10                            Bool
IsUserFlag11                            Bool
IsUserFlag12                            Bool
IsUserFlag13                            Bool
IsUserFlag14                            Bool
IsUserFlag15                            Bool
IsUserFlag16                            Bool
IsUserFlag17                            Bool
IsUserFlag18                            Bool
IsUserFlag19                            Bool
IsUserFlag2                             Bool
IsUserFlag20                            Bool
IsUserFlag21                            Bool
IsUserFlag22                            Bool
IsUserFlag23                            Bool
IsUserFlag24                            Bool
IsUserFlag25                            Bool
IsUserFlag26                            Bool
IsUserFlag27                            Bool
IsUserFlag28                            Bool
IsUserFlag29                            Bool
IsUserFlag3                             Bool
IsUserFlag30                            Bool
IsUserFlag31                            Bool
IsUserFlag32                            Bool
IsUserFlag4                             Bool
IsUserFlag5                             Bool
IsUserFlag6                             Bool
IsUserFlag7                             Bool
IsUserFlag8                             Bool
IsUserFlag9                             Bool
IsWarehouseClient                       Bool
Kennitala                               String
LocalBusinessRegNo                      String
LocalCustomsCarrierCode                 String
LocalCustomsClientCode                  String
LocalCustomsSupplierCode                String
LocalRebateUserCode                     String
LocalVATCode                            String
LSC                                     String
Mobile                                  String
Name                                    String
OrgType                                 String
PAN                                     String
PartAttrib1Name                         String
PartAttrib2Name                         String
PartAttrib3Name                         String
PayAmount                               Decimal
Phone                                   String
PostalAddress                           String
PostalAddress1                          String
PostalAddress2                          String
PostalAddressCity                       String
PostalAddressExcludeCountryIfSame       String
PostalAddressExcludeName                String
PostalAddressInEnglish                  String
PostalAddressPostCode                   String
PostCode                                String
SCAC                                    String
ShortDisbursementTerms                  String
ShortInvoiceTerms                       String
SplitFullName1                          String
SplitFullName2                          String
SSN                                     String
SSNorEIN                                String
State                                   String
Web                                     String
Notes                                   Note Collection
NotesIncludingRelated                   Note Collection
";

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			if (Header == null)
			{
				Header = Factory.NewWithValidTestData<OrgHeader>();
				Header.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			}

			var result = DocOrganisationIRS1099.New(Header, Factory);
			result.Parameters = new Dictionary<string, object> { { "Calendar Year", ZDateTime.Now.Year } };

			return result;
		}
	}
}
