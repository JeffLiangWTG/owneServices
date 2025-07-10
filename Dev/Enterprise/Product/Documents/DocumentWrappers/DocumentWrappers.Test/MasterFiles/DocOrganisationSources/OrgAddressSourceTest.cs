using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(OrgAddressSource))]
	public class OrgAddressSourceTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { OrgAddressSource.New(JobDocAddress, Factory) };
		}

		public void TestPartAttributes()
		{
			AssertEquals(false, OrgWrapper.HasPartAttrib1);
			AssertEquals(false, OrgWrapper.HasPartAttrib2);
			AssertEquals(false, OrgWrapper.HasPartAttrib3);
			AssertEquals(false, OrgWrapper.HasSerialNumber);
		}

		#region Spliting of Name

		public void TestSplitOfNameWithLengthMoreThan25()
		{
			JobDocAddress.E2_CompanyName = "123456789 123456789 123456789 123456789 123456789";

			ZString wrappedVersion = OrgWrapper.WrapTextForAColumn(JobDocAddress.E2_CompanyName, 25);
			ZString expectedVersionForName1 = wrappedVersion.Split('\n')[0];
			ZString expectedVersionForName2 = wrappedVersion.Split('\n')[1];
			OrgWrapper = OrgAddressSource.New(JobDocAddress, Factory);

			AssertEquals("Name 1", expectedVersionForName1, OrgWrapper.SplitFullName1);
			AssertEquals("Name 2", expectedVersionForName2, OrgWrapper.SplitFullName2);
		}

		public void TestSplitOfNameWithLengthLessThan25()
		{
			JobDocAddress.E2_CompanyName = "less than 25 chars";

			ZString wrappedVersion = OrgWrapper.WrapTextForAColumn(JobDocAddress.E2_CompanyName, 25);
			ZString expectedVersionForName1 = wrappedVersion.Split('\n')[0];
			ZString expectedVersionForName2 = ZString.Empty;
			OrgWrapper = OrgAddressSource.New(JobDocAddress, Factory);

			AssertEquals("Name 1", expectedVersionForName1, OrgWrapper.SplitFullName1);
			AssertEquals("Name 2", expectedVersionForName2, OrgWrapper.SplitFullName2);
		}

		#endregion

		#region Test Marketing Flags

		public void TestIsUserFlag1()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag1);
		}

		public void TestIsUserFlag2()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag2);
		}

		public void TestIsUserFlag3()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag3);
		}

		public void TestIsUserFlag4()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag4);
		}

		public void TestIsUserFlag5()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag5);
		}

		public void TestIsUserFlag6()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag6);
		}

		public void TestIsUserFlag7()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag7);
		}

		public void TestIsUserFlag8()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag8);
		}

		public void TestIsUserFlag9()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag9);
		}

		public void TestIsUserFlag10()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag10);
		}

		public void TestIsUserFlag11()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag11);
		}

		public void TestIsUserFlag12()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag12);
		}

		public void TestIsUserFlag13()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag13);
		}

		public void TestIsUserFlag14()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag14);
		}

		public void TestIsUserFlag15()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag15);
		}

		public void TestIsUserFlag16()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag16);
		}

		public void TestIsUserFlag17()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag17);
		}

		public void TestIsUserFlag18()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag18);
		}

		public void TestIsUserFlag19()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag19);
		}

		public void TestIsUserFlag20()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag20);
		}

		public void TestIsUserFlag21()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag21);
		}

		public void TestIsUserFlag22()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag22);
		}

		public void TestIsUserFlag23()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag23);
		}

		public void TestIsUserFlag24()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag24);
		}

		public void TestIsUserFlag25()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag25);
		}

		public void TestIsUserFlag26()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag26);
		}

		public void TestIsUserFlag27()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag27);
		}

		public void TestIsUserFlag28()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag28);
		}

		public void TestIsUserFlag29()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag29);
		}

		public void TestIsUserFlag30()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag30);
		}

		public void TestIsUserFlag31()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag31);
		}

		public void TestIsUserFlag32()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag32);
		}

		#endregion

		#region Addresses

		public void TestDeliverAddress()
		{
			AssertNull(OrgWrapper.DeliverAddress);
		}

		public void TestOrganisationPADPostalAddress()
		{
			AssertNull(OrgWrapper.OrganisationPADPostalAddress);
		}

		public void TestPickUpAddress()
		{
			AssertNull(OrgWrapper.PickUpAddress);
		}

		public void TestARAddress()
		{
			AssertNull(OrgWrapper.ARAddress);
		}

		public void TestPostalAddress()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			JobDocAddress.E2_AddressOverride = false;
			AssertEquals("PostalAddress", "", OrgWrapper.PostalAddress);
			AssertEquals("PostalAddressExcludeName", "", OrgWrapper.PostalAddressExcludeName);
			AssertEquals("PostalAddressExcludeCountryIfSame", "", OrgWrapper.PostalAddressExcludeCountryIfSame);
			JobDocAddress.E2_CompanyName = "My Company";
			JobDocAddress.E2_Address1 = "12A TOWER BUILDING";
			JobDocAddress.E2_Address2 = "HIGH TOWER";
			JobDocAddress.E2_City = "Star City";
			JobDocAddress.E2_Postcode = "1111";
			JobDocAddress.E2_State = "NSW";
			JobDocAddress.E2_RN_NKCountryCode = "AU";
			AssertEquals("PostalAddress", "MY COMPANY\n12A TOWER BUILDING\nHIGH TOWER\nSTAR CITY NSW 1111\nAUSTRALIA", OrgWrapper.PostalAddress);
			AssertEquals("PostalAddressExcludeName", "12A TOWER BUILDING\nHIGH TOWER\nSTAR CITY NSW 1111\nAUSTRALIA", OrgWrapper.PostalAddressExcludeName);
			AssertEquals("PostalAddressExcludeCountryIfSame: same country", "MY COMPANY\n12A TOWER BUILDING\nHIGH TOWER\nSTAR CITY NSW 1111", OrgWrapper.PostalAddressExcludeCountryIfSame);
			JobDocAddress.E2_RN_NKCountryCode = "JP";
			AssertMultilineASCIIEquals("PostalAddressExcludeCountryIfSame: different country", "MY COMPANY\n12A TOWER BUILDING\nHIGH TOWER\nSTAR CITY, NSW\n1111\nJAPAN", OrgWrapper.PostalAddressExcludeCountryIfSame);
		}

		public void TestPostalAddressInEnglish()
		{
			var staffDE = Factory.New<GlbStaff>();
			staffDE.GS_Code = "ABC";
			staffDE.GS_LoginName = "Dieter";
			staffDE.GS_FullName = "Dieter";
			staffDE.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			Factory.Save();

			using (Env.Instance.SetTemporaryUserContext(staffDE.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				JobDocAddress.E2_AddressOverride = false;
				AssertEquals("PostalAddress", "", OrgWrapper.PostalAddressInEnglish);

				JobDocAddress.E2_CompanyName = "My Company";
				JobDocAddress.E2_Address1 = "12A TOWER BUILDING";
				JobDocAddress.E2_Address2 = "HIGH TOWER";
				JobDocAddress.E2_City = "Star City";
				JobDocAddress.E2_Postcode = "1111";
				JobDocAddress.E2_RN_NKCountryCode = "DE";
				AssertEquals("PostalAddress", "MY COMPANY\n12A TOWER BUILDING\nHIGH TOWER\n1111 STAR CITY\nDEUTSCHLAND", OrgWrapper.PostalAddress);
				AssertEquals("PostalAddressInEnglish", "MY COMPANY\n12A TOWER BUILDING\nHIGH TOWER\n1111 STAR CITY\nGERMANY", OrgWrapper.PostalAddressInEnglish);
			}
		}

		public void TestDeliverDocAddress()
		{
			AssertEquals("DeliverDocAddress", JobDocAddress, OrgWrapper.DeliverDocAddress.WrappedObject);
		}

		public void TestPickUpDocAddress()
		{
			AssertEquals("PickUpDocAddress", JobDocAddress, OrgWrapper.DeliverDocAddress.WrappedObject);
		}

		public void TestDocAddresses()
		{
			AssertEquals("DocAddresses", 1, OrgWrapper.DocAddresses.Count);
			AssertEquals("DocAddresses", JobDocAddress, OrgWrapper.DocAddresses[0].WrappedObject);
		}

		#endregion

		#region Invoices

		public void TestDisbursementTerms()
		{
			AssertEquals("Invoice terms", "", OrgWrapper.DisbursmentTerms);
		}

		public void TestShortDisbursementTerms()
		{
			AssertEquals("", OrgWrapper.ShortDisbursementTerms);
		}

		public void TestShortInvoiceTerms()
		{
			AssertEquals("Invoice terms", "", OrgWrapper.ShortInvoiceTerms);
		}

		#endregion

		#region Custom Fields
		public void TestCarrierMasterBillPrefix()
		{
			AssertEquals("", OrgWrapper.CarrierMasterBillPrefix);
		}

		public void TestApprovedExporterCode()
		{
			AssertEquals("", OrgWrapper.ApprovedExporterCode);
		}

		public void TestABN()
		{
			AssertEquals("", OrgWrapper.ABN);
		}

		public void TestCBR()
		{
			AssertEquals("CBR", "", OrgWrapper.CBR);
		}

		public void TestCCC()
		{
			AssertEquals("CCC", "", OrgWrapper.CCC);
		}

		public void TestGST()
		{
			AssertEquals("GST", "", OrgWrapper.GST);
		}

		public void TestCSC()
		{
			AssertEquals("CSC", "", OrgWrapper.CSC);
		}

		public void TestCCD()
		{
			AssertEquals("CCD", "", OrgWrapper.CCD);
		}

		public void TestCID()
		{
			AssertEquals("CID", "", OrgWrapper.CID);
		}

		public void TestPAN()
		{
			AssertEquals("PAN", "", OrgWrapper.PAN);
		}

		#endregion

		#region Basic Organisation Details
		public void TestCountry()
		{
			AssertEquals("", OrgWrapper.Country.Name);
			var lOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			JobDocAddress.E2_RN_NKCountryCode = lOCO.RL_RN_NKCountryCode;
			AssertEquals(lOCO.Country.RN_Desc, OrgWrapper.Country.Name);
		}

		public void TestContacts()
		{
			AssertEquals(1, OrgWrapper.Contacts.Count);
		}

		public void TestAddress1()
		{
			JobDocAddress.E2_Address1 = "123 Bourke Street";
			AssertEquals("123 Bourke Street", OrgWrapper.Address1);
		}

		public void TestAddress2()
		{
			JobDocAddress.E2_Address2 = "123 Lala Land";
			AssertEquals("123 Lala Land", OrgWrapper.Address2);
		}

		public void TestCity()
		{
			JobDocAddress.E2_City = "Star City";
			AssertEquals("Star City", OrgWrapper.City);
		}

		public void TestCode()
		{
			AssertEquals("", OrgWrapper.Code);
		}

		public void TestEmail()
		{
			JobDocAddress.E2_Email = "blah@email.com";
			AssertEquals("blah@email.com", OrgWrapper.Email);
		}

		public void TestExportFreightContact()
		{
			AssertEquals("Contact", OrgWrapper.ExportFowardingContact.ContactName);
		}

		public void TestImportFreightContact()
		{
			AssertEquals("Contact", OrgWrapper.ImportFowardingContact.ContactName);
		}

		public void TestFax()
		{
			JobDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			JobDocAddress.E2_Fax = "02 5555 9999";
			AssertEquals("+61 2 5555 9999", OrgWrapper.Fax);
		}

		public void TestBusinessRegNo()
		{
			AssertEquals("", OrgWrapper.BusinessRegNo);
		}
		public void TestName()
		{
			JobDocAddress.E2_CompanyName = "FULL NAME TESTING";
			AssertEquals("FULL NAME TESTING", OrgWrapper.Name);
		}

		public void TestBranch()
		{
			AssertNull(OrgWrapper.Branch);
		}

		public void TestIsActive()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsActive);
		}

		public void TestIsAirCTO()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsAirCTO);
		}

		public void TestIsAirLine()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsAirLine);
		}

		public void TestIsAirWholesaler()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsAirWholesaler);
		}

		public void TestIsBroker()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsBroker);
		}

		public void TestIsCompetitor()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsCompetitor);
		}

		public void TestIsConsignee()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsConsignee);
		}

		public void TestIsConsignor()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsConsignor);
		}

		public void TestIsContainerPark()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsContainerPark);
		}

		public void TestIsCreditor()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsCreditor);
		}

		public void TestIsDebtor()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsDebtor);
		}

		public void TestIsForwarder()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsForwarder);
		}

		public void TestIsLineHaulProvider()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsLineHaulProvider);
		}

		public void TestIsLocalTransport()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsLocalTransport);
		}

		public void TestIsMiscFreightServices()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsMiscFreightServices);
		}

		public void TestIsPackDepot()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsPackDepot);
		}

		public void TestIsRailProvider()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsRailProvider);
		}

		public void TestIsSalesLead()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsSalesLead);
		}

		public void TestIsSeaCTO()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsSeaCTO);
		}

		public void TestIsSeaWholesaler()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsSeaWholesaler);
		}

		public void TestIsShippingLine()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsShippingLine);
		}

		public void TestIsShippingProvider()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsShippingProvider);
		}

		public void TestIsTempAccount()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsTempAccount);
		}

		public void TestIsTransportClient()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsTransportClient);
		}

		public void TestIsUnpackDepot()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUnpackDepot);
		}

		public void TestIsWarehouseClient()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsWarehouseClient);
		}

		public void TestMobile()
		{
			AssertEquals("", OrgWrapper.Mobile);
		}

		public void TestPhone()
		{
			JobDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			JobDocAddress.E2_Phone = "08 83838383";
			AssertEquals("+61 8 8383 8383", OrgWrapper.Phone);
		}

		public void TestPostCode()
		{
			JobDocAddress.E2_Postcode = "8383";
			AssertEquals("8383", OrgWrapper.PostCode);
		}

		public void TestLoco()
		{
			AssertNull(OrgWrapper.Loco);
		}

		public void TestState()
		{
			JobDocAddress.E2_State = "VIC";
			AssertEquals("VIC", OrgWrapper.State);
		}

		public void TestWeb()
		{
			AssertEquals("", OrgWrapper.Web);
		}

		public void TestAllNotes()
		{
			AssertEquals(0, OrgWrapper.AllNotes.Length);
		}

		public void TestLocalCustomsClientCode()
		{
			AssertEquals("Customs Client Code", "", OrgWrapper.LocalCustomsClientCode);
		}

		public void TestLocalCustomsCarrierCode()
		{
			AssertEquals("CarrierCode", "", OrgWrapper.LocalCustomsCarrierCode);
		}

		public void TestLocalBusinessRegNo()
		{
			AssertEquals("Local Business Rego No", "", OrgWrapper.LocalBusinessRegNo);
		}

		public void TestLocalCustomsSupplierCode()
		{
			AssertEquals("SupplierCode", "", OrgWrapper.LocalCustomsSupplierCode);
		}

		public void TestLocalVATCode()
		{
			AssertEquals("VATCode", "", OrgWrapper.LocalVATCode);
		}

		public void TestHandlingInstruction()
		{
			AssertEquals("Handling Instructions", "", OrgWrapper.HandlingInstructions);
		}

		public void TestCartageInstruction()
		{
			AssertEquals("CartageInstructions", "", OrgWrapper.CartageInstructions);
		}

		public void TestGetCartageInstructionsByTransportMode()
		{
			AssertEquals("GetCartageInstructionsByTransportMode", "", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode(Constants.TransportModes.Air, ""));
			AssertEquals("GetCartageInstructionsByTransportMode", "", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode(Constants.TransportModes.Sea, ""));
			AssertEquals("GetCartageInstructionsByTransportMode", "", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode(Constants.TransportModes.All, ""));
			AssertEquals("GetCartageInstructionsByTransportMode", "", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode(Constants.TransportModes.Road, ""));
		}

		public void TestStaffAssignments()
		{
			JobDocAddress.E2_AddressOverride = true;
			AssertNotNull(JobDocAddress.Organisation);
			JobDocAddress.Organisation.StaffAssignments.RemoveAndDeleteAll();
			OrgStaffAssignments item = JobDocAddress.Organisation.StaffAssignments.AddNew();
			item.O8_Department = "SEA";
			AssertEquals("Count", 1, OrgWrapper.StaffAssignments.Count);
			AssertEquals("SEA", OrgWrapper.StaffAssignments[0].O8_Department);

			JobDocAddress.E2_AddressOverride = false;
			AssertNull(JobDocAddress.Organisation);
			AssertEquals("Count", 0, OrgWrapper.StaffAssignments.Count);
		}

		#endregion

		#region Empty country

		public void TestEmptyCountry_ShouldNotCrashAndBurnIfFactoryIsSaved()
		{
			AssertNull(JobDocAddress.Country);
			AssertNotNull(OrgWrapper.Country);
			Assert(string.IsNullOrEmpty(OrgWrapper.Country.Code));
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region Implementation
		JobDocAddress JobDocAddress;
		OrgAddressSource OrgWrapper;
		protected override void SetUp()
		{
			JobDocAddress = CreateNewJobDocAddressWithTestData();
			OrgWrapper = (OrgAddressSource)GetDocumentWrappers()[0];
			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			base.SetUp();
		}

		JobDocAddress CreateNewJobDocAddressWithTestData()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			JobDocAddress result = JobDocAddress.New(shipment);

			result.E2_AddressOverride = true;
			result.E2_Contact = "Contact";
			result.E2_CompanyName = "Company";
			result.E2_Address1 = "11";
			result.E2_Address2 = "21";
			result.E2_City = "31";
			result.E2_State = "41";
			result.E2_Postcode = "51";
			result.E2_RN_NKCountryCode = "XY";
			result.E2_Phone = "61";
			result.E2_Fax = "71";
			result.E2_Email = "81";

			return result;
		}

		#endregion
	}
}
