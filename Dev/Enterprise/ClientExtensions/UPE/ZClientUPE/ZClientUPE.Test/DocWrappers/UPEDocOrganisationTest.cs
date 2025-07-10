using System;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEDocOrganisation))]
	public class UPEDocOrganisationTest : GenericWrapperWithNotesTest
	{
		public void TestNew()
		{
			Organisation.OH_FullName = "Name";
			UPEDocOrganisation doc = UPEDocOrganisation.New(Factory, Organisation.PK);
			AssertEquals("Name", doc.Name);
		}

		public void TestAccountNumber()
		{
			OrgCusCode accountNumber = Organisation.CustomsCodes.AddNew();
			accountNumber.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			accountNumber.OK_CustomsRegNo = "AccountNumber";
			accountNumber.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AssertEquals("AccountNumber", "AccountNumber", DocOrganisation.AccountNumber);
		}

		public void TestUPSContactFax()
		{
			UPEDataRegistry.Instance.UPSContactFax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Fax");
			AssertEquals("Fax", DocOrganisation.UPSContactFax);
		}

		#region Letter of Authority
		[TestDate(2006, 5, 5)]
		public void TestLetterOfAuthorityExpirationDate()
		{
			Organisation.LetterOfAuthorityExpirationDate = ZDateTime.Empty;
			AssertEquals("LetterOfAuthorityExpirationDate", ZDateTime.Now.AddDays(30), DocOrganisation.LetterOfAuthorityExpirationDate);
			Organisation.LetterOfAuthorityExpirationDate = new ZDateTime(2005, 1, 2);
			AssertEquals("LetterOfAuthorityExpirationDate", Organisation.LetterOfAuthorityExpirationDate, DocOrganisation.LetterOfAuthorityExpirationDate);
		}

		#endregion
		#region Implementation
		public override void TestWrapperMappingsEmpty()
		{
			//If the source is null, this wrapper is null, so let's ignore this test
			Assert(true);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return UPEDocOrganisation.New(Factory, Organisation.PK);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override ZString ExpectedDefaultFormatting => @"
EXFreightBillTo : 
IMFreightBillTo : 
Registry : (No Default Field Value Available on Registry)
";
		protected override string ExpectedFieldMap => @"
UPEDocOrganisation                               (Default Field: Name)
======================================================================
Name                                    Type
----------------------------------------------------------------------
EXFreightBillTo                         DocOrganisation
IMFreightBillTo                         DocOrganisation
ABN                                     String
AccountNumber                           String
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
LetterOfAuthorityExpirationDate         DateTime
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
UPSContactFax                           String
Web                                     String
Notes                                   Note Collection
NotesIncludingRelated                   Note Collection
";
		UPEOrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.NewWithValidTestData<UPEOrgHeader>();
				}

				return fOrganisation;
			}
		}

		UPEOrgHeader fOrganisation;
		UPEDocOrganisation DocOrganisation
		{
			get
			{
				if (fDocOrganisation == null)
				{
					fDocOrganisation = UPEDocOrganisation.New(Factory, Organisation.PK);
				}

				return fDocOrganisation;
			}
		}

		UPEDocOrganisation fDocOrganisation;
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return UPEDocOrganisation.New(Factory, Organisation.PK);
		}

		#endregion
		public override void TestWrapperNotes()
		{
			Organisation.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "These notes exist against the organisation and should be algamated");
			Organisation.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "This is the second line that gets added to the first line using the indexer");
			Organisation.Notes.AddNew(false, PredefinedNoteTypes.Instance.AgentNotes.Description, "Some agent notes that should not be joined together as there are no others");
			AssertEquals("Notes Count", 3, DocOrganisation.Notes.Count);
			var noteWrapper = DocOrganisation.Notes[PredefinedNoteTypes.Instance.HandlingInstructions.Description];
			AssertNotNull("The handling instructions is found using the indexer", noteWrapper);
			AssertEquals("The Text is algamated", "These notes exist against the organisation and should be algamated\r\nThis is the second line that gets added to the first line using the indexer", noteWrapper.Text);
			AssertEquals("noteWrapper.Description", "Goods Handling Instructions", noteWrapper.Description);
			noteWrapper = DocOrganisation.Notes[PredefinedNoteTypes.Instance.AgentNotes.Description];
			AssertNotNull("Int indexer gets the agent Notes", noteWrapper);
			AssertEquals("noteWrapper.Text", "Some agent notes that should not be joined together as there are no others", noteWrapper.Text);
			AssertEquals("noteWrapper.Description", "Agent Notes", noteWrapper.Description);
		}
	}
}
