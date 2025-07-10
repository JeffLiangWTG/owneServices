using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocOrganisation))]
	internal class DocOrganisationTest : GenericWrapperWithNotesTest
	{
		public void TestDocOrganisationIsGenericWrapperWithNotes()
		{
			Assert(OrgWrapper is GenericWrapperWithNotes);

			var parentBo = OrgWrapper.GetType().GetMethod("GetParentBOForNoteStorageEDocsAndDocData", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(OrgWrapper, null);
			AssertEquals(Header, parentBo);
		}

		public void TestDGContactName()
		{
			DocOrganisation docOrg = DocOrganisation.New(Header, Factory);
			AssertEquals("", docOrg.DGContactName);

			OrgContact contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "ABC";
			Header.MiscServ.OM_OC_EXDefaultDGContact = contact.PK;
			AssertEquals("ABC", docOrg.DGContactName);
		}

		public void TestDGContactPhone()
		{
			DocOrganisation docOrg = DocOrganisation.New(Header, Factory);
			AssertEquals("", docOrg.DGContactPhone);

			OrgContact contact = Factory.New<OrgContact>();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "Test Address";
			contact.OC_OA_OrgAddress = org.MainAddress.PK;
			contact.BranchAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			contact.OC_HomePhone = "0255559999";
			Header.MiscServ.OM_OC_EXDefaultDGContact = contact.PK;
			AssertEquals("", docOrg.DGContactPhone);

			Header.MiscServ.OM_EXDefaultDGContactPhoneUsed = PhoneTypeList.Codes.HOM;
			AssertEquals("+61 2 5555 9999", docOrg.DGContactPhone);
		}

		public void TestBusinessObjectToLogAgainst()
		{
			IBODocDataProvider docOrg = DocOrganisation.New(Header, Factory);
			AssertEquals(Header, docOrg.BusinessObjectToLogAgainst);
		}

		public void TestPartAttributes()
		{
			Header.MiscServ.OM_IMPartAttrib1Name = "PA1";
			Header.MiscServ.OM_IMPartAttrib2Name = "PA2";
			Header.MiscServ.OM_IMPartAttrib3Name = "PA3";
			Header.MiscServ.OM_IMPartAttrib1Type = "NON";
			Header.MiscServ.OM_IMPartAttrib2Type = "NON";
			Header.MiscServ.OM_IMPartAttrib3Type = "NON";
			Header.MiscServ.OM_IMUseSerialNumber = true;
			DocOrganisation docOrg = DocOrganisation.New(Header, Factory);
			AssertEquals("PA1", docOrg.PartAttrib1Name);
			AssertEquals("PA2", docOrg.PartAttrib2Name);
			AssertEquals("PA3", docOrg.PartAttrib3Name);
			AssertEquals(true, docOrg.HasPartAttrib1);
			AssertEquals(true, docOrg.HasPartAttrib2);
			AssertEquals(true, docOrg.HasPartAttrib3);
			AssertEquals(true, docOrg.HasSerialNumber);
			Header.MiscServ.OM_IMPartAttrib1Type = "";
			Header.MiscServ.OM_IMPartAttrib2Type = "";
			Header.MiscServ.OM_IMPartAttrib3Type = "";
			Header.MiscServ.OM_IMUseSerialNumber = false;
			AssertEquals(false, docOrg.HasPartAttrib1);
			AssertEquals(false, docOrg.HasPartAttrib2);
			AssertEquals(false, docOrg.HasPartAttrib3);
			AssertEquals(false, docOrg.HasSerialNumber);
		}

		#region TestMiscServIsNeverNull

		public void TestMiscServIsNeverNull()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var address = org.MainAddress;
			var jda = Factory.New<JobDocAddress>();
			jda.E2_AddressOverride = true;

			var orgAsDocOrg = DocOrganisation.New(org, Factory);
			var addressAsDocOrg = DocOrganisation.New(address, Factory);
			var jdaAsDcoOrg = DocOrganisation.New(jda, Factory);

			AssertNotNull("OrgHeader as a DocOrganisation", orgAsDocOrg.MiscServ);
			AssertNotNull("OrgAddress as a DocOrganisation", addressAsDocOrg.MiscServ);
			AssertNotNull("JobDocAddress as a DocOrganisation", jdaAsDcoOrg.MiscServ);
		}

		#endregion

		#region TestDocOrganisation with DocAddress

		public void TestDocOrganisationWithDocAddress_NoOrgNoOverride()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			JobDocAddress jobDocAddress = JobDocAddress.New(shipment);

			AssertNull("Should return null as there is no org and no override", DocOrganisation.New(jobDocAddress, Factory));
		}

		public void TestDocOrganisationWithDocAddress_OverrideWithNoOrganisation()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			JobDocAddress jobDocAddress = JobDocAddress.New(shipment);

			jobDocAddress.E2_AddressOverride = true;
			DocOrganisation orgWrapper = DocOrganisation.New(jobDocAddress, Factory);
			AssertNotNull("Should return null as there is no org and no override", orgWrapper);
			AssertEquals("DocOrganisation should be wrapping a OrgAddressSource", nameof(OrgAddressSource), orgWrapper.WrappedObject.GetType().Name);
		}

		public void TestDocOrganisationWithDocAddress_WithOrganisation()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			JobDocAddress jobDocAddress = JobDocAddress.New(shipment);

			jobDocAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			DocOrganisation orgWrapper = DocOrganisation.New(jobDocAddress, Factory);
			AssertNotNull("Should return null as there is an org", orgWrapper);
			AssertEquals("DocOrganisation should be wrapping a OrgHeaderSource", nameof(OrgHeaderSource), orgWrapper.WrappedObject.GetType().Name);
		}

		#endregion

		#region Spliting of Name

		public void TestSplitOfNameWithLengthMoreThan25()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "123456789 123456789 123456789 123456789 123456789";

			ZString wrappedVersion = OrgWrapper.WrapTextForAColumn(header.OH_FullName, 25);
			ZString expectedVersionForName1 = wrappedVersion.Split('\n')[0];
			ZString expectedVersionForName2 = wrappedVersion.Split('\n')[1];
			OrgWrapper = DocOrganisation.New(header, Factory);

			AssertEquals("Name 1", expectedVersionForName1, OrgWrapper.SplitFullName1);
			AssertEquals("Name 2", expectedVersionForName2, OrgWrapper.SplitFullName2);
		}

		public void TestSplitOfNameWithLengthLessThan25()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "less than 25 chars";

			ZString wrappedVersion = OrgWrapper.WrapTextForAColumn(header.OH_FullName, 25);
			ZString expectedVersionForName1 = wrappedVersion.Split('\n')[0];
			ZString expectedVersionForName2 = ZString.Empty;
			OrgWrapper = DocOrganisation.New(header, Factory);

			AssertEquals("Name 1", expectedVersionForName1, OrgWrapper.SplitFullName1);
			AssertEquals("Name 2", expectedVersionForName2, OrgWrapper.SplitFullName2);
		}

		#endregion

		#region Addresses

		public void TestMainAddress()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.MainAddress.OA_Address1 = "SampleAddress";
			OrgWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("Main address correct", "SampleAddress", OrgWrapper.Address1);
		}

		public void TestDeliverAddress()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.MainAddress.OA_Address1 = "Org Address";
			header.MainAddress.OA_Address2 = "Org Address 2";
			header.MainAddress.OA_City = "Org City";
			AssertEquals("Address for Org", OrgWrapper.PostalAddress, OrgWrapper.DeliverAddress.PostalAddress);

			OrgAddress pADAddress = header.Addresses.AddNew();
			pADAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			pADAddress.OA_Address1 = "PAD Address 1";
			pADAddress.OA_Address2 = "PAD Address 2";
			pADAddress.OA_City = "PAD Address City";

			DocAddress pADAddressWrapper = DocAddress.New(pADAddress, Factory);
			OrgWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("Address for PAD", pADAddressWrapper.PostalAddress, OrgWrapper.DeliverAddress.PostalAddress);

			OrgAddress dLVAddress = header.Addresses.AddNew();
			dLVAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			dLVAddress.OA_Address1 = "DLV Address 1";
			dLVAddress.OA_Address2 = "DLV Address 2";
			dLVAddress.OA_City = "DLV Address City";

			pADAddress.AddressCapability.SetIsNotMainAddress(OrgConstants.AddressType.PickupAndDelivery);
			dLVAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Delivery);

			DocAddress dLVAddressWrapper = DocAddress.New(dLVAddress, Factory);
			OrgWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("Address for DLV", dLVAddressWrapper.PostalAddress, OrgWrapper.DeliverAddress.PostalAddress);

			dLVAddress.AddressCapability.SetIsNotMainAddress(OrgConstants.AddressType.Delivery);
			pADAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.PickupAndDelivery);
			AssertEquals("Address for PAD; default", pADAddressWrapper.PostalAddress, OrgWrapper.DeliverAddress.PostalAddress);
		}

		public void TestOrganisationPADPostalAddress()
		{
			var header = Factory.New<OrgHeader>();
			header.MainAddress.OA_Address1 = "Org Address";
			header.MainAddress.OA_Address2 = "Org Address 2";
			header.MainAddress.OA_City = "Org City";
			AssertNull("Address for PAD is null", OrgWrapper.OrganisationPADPostalAddress);

			OrgAddress pADAddress = header.Addresses.AddNew();
			pADAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			pADAddress.OA_Address1 = "PAD Address 1";
			pADAddress.OA_Address2 = "PAD Address 2";
			pADAddress.OA_City = "PAD Address City";
			DocAddress pADAddressWrapper = DocAddress.New(pADAddress, Factory);
			OrgWrapper = DocOrganisation.New(header, Factory);
			AssertNotNull("Address for PAD is not null", OrgWrapper.OrganisationPADPostalAddress);
			AssertEquals("Address for PAD", pADAddressWrapper.PostalAddress, OrgWrapper.OrganisationPADPostalAddress.PostalAddress);
		}

		public void TestAppAgPorts()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.MainAddress.OA_Address1 = "hell";
			OrgAppointedAgentPorts appAgPorts = header.AppointedAgentPorts.AddNew();
			appAgPorts.O5_OA_AgentOfficeAddress = header.MainAddress.PK;
			appAgPorts.O5_AirAgentStatus = AgentStatusList.Codes.Appointed;
			appAgPorts.O5_SeaAgentStatus = AgentStatusList.Codes.Published;
			appAgPorts.O5_RoadAgentStatus = AgentStatusList.Codes.Handles;
			appAgPorts.O5_RailAgentStatus = AgentStatusList.Codes.Appointed;
			appAgPorts.O5_PortOrCountry = "USLAX";
			appAgPorts.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.RoadDepotShed;
			OrgWrapper = DocOrganisation.New(header, Factory);
			DocAppointedAgentPorts docAppAgPorts = DocAppointedAgentPorts.New(appAgPorts, Factory);
			AssertNotNull(OrgWrapper.AppAgPorts);
			AssertEquals(docAppAgPorts.AgentPortOrCountry, OrgWrapper.AppAgPorts[0].AgentPortOrCountry);
			AssertEquals(docAppAgPorts.SeaAgentStatus, OrgWrapper.AppAgPorts[0].SeaAgentStatus);
		}

		public void TestPickUpAddress()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.MainAddress.OA_Address1 = "Org Address";
			header.MainAddress.OA_Address2 = "Org Address 2";
			header.MainAddress.OA_City = "Org City";
			header.MainAddress.OA_RN_NKCountryCode = "AU";
			AssertEquals("Address for Org", OrgWrapper.PostalAddress, OrgWrapper.PickUpAddress.PostalAddress);

			OrgAddress pADAddress = header.Addresses.AddNew();
			pADAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			pADAddress.OA_Address1 = "PAD Address 1";
			pADAddress.OA_Address2 = "PAD Address 2";
			pADAddress.OA_City = "PAD Address City";
			pADAddress.OA_RN_NKCountryCode = "AU";

			DocAddress pADAddressWrapper = DocAddress.New(pADAddress, Factory);
			OrgWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("Address for PAD", pADAddressWrapper.PostalAddress, OrgWrapper.PickUpAddress.PostalAddress);

			OrgAddress pICAddress = header.Addresses.AddNew();
			pICAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			pICAddress.OA_Address1 = "PIC Address 1";
			pICAddress.OA_Address2 = "PIC Address 2";
			pICAddress.OA_City = "PIC Address City";
			pICAddress.OA_RN_NKCountryCode = "AU";

			pADAddress.AddressCapability.SetIsNotMainAddress(OrgConstants.AddressType.PickupAndDelivery);
			pICAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Pickup);

			DocAddress pICAddressWrapper = DocAddress.New(pICAddress, Factory);
			OrgWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("Address for PIC", pICAddressWrapper.PostalAddress, OrgWrapper.PickUpAddress.PostalAddress);

			pICAddress.AddressCapability.SetIsNotMainAddress(OrgConstants.AddressType.Pickup);
			pADAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.PickupAndDelivery);
			AssertEquals("Address for PAD; default", pADAddressWrapper.PostalAddress, OrgWrapper.DeliverAddress.PostalAddress);
		}

		public void TestARAddress()
		{
			var header = Factory.New<OrgHeader>();
			header.MainAddress.OA_Address1 = "Org Address";
			header.MainAddress.OA_Address2 = "Org Address 2";
			header.MainAddress.OA_City = "Org City";
			header.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			OrgWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("Office address", OrgWrapper.PostalAddress, OrgWrapper.ARAddress.PostalAddress);

			OrgAddress postalAddress = header.Addresses.AddNew();
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Postal);
			postalAddress.OA_Address1 = "Postal Address 1";
			postalAddress.OA_Address2 = "Postal Address 2";
			postalAddress.OA_City = "Postal Address City";
			OrgWrapper = DocOrganisation.New(header, Factory);
			DocAddress postalAddressWrapper = DocAddress.New(postalAddress, Factory);
			AssertEquals("Postal address should be used over Office address", postalAddressWrapper.PostalAddress, OrgWrapper.ARAddress.PostalAddress);

			OrgAddress receivablesAddress = header.Addresses.AddNew();
			receivablesAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			receivablesAddress.OA_Address1 = "Receivables Address 1";
			receivablesAddress.OA_Address2 = "Receivables Address 2";
			receivablesAddress.OA_City = "Receivables Address City";
			DocAddress receivablesAddressWrapper = DocAddress.New(receivablesAddress, Factory);

			OrgWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("Receivables address should be used over postal and office address", receivablesAddressWrapper.PostalAddress, OrgWrapper.ARAddress.PostalAddress);
		}

		public void TestPostalAddress()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			Header.MainAddress.Address1 = "";
			AssertEquals("", OrgWrapper.PostalAddress);
			AssertEquals("", OrgWrapper.PostalAddressExcludeName);
			Header.OH_FullName = "MY COMPANY";
			Header.MainAddress.OA_Address1 = "12A TOWER BUILDING";
			Header.MainAddress.OA_Address2 = "HIGH TOWER";
			Header.MainAddress.OA_City = "Star City";
			Header.MainAddress.OA_PostCode = "1111";
			Header.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			Header.MainAddress.OA_State = "NSW";
			AssertEquals("MY COMPANY\n12A TOWER BUILDING\nHIGH TOWER\nSTAR CITY NSW 1111\nAUSTRALIA", OrgWrapper.PostalAddress);
			AssertEquals("12A TOWER BUILDING\nHIGH TOWER\nSTAR CITY NSW 1111\nAUSTRALIA", OrgWrapper.PostalAddressExcludeName);
			AssertEquals("Same country", "MY COMPANY\n12A TOWER BUILDING\nHIGH TOWER\nSTAR CITY NSW 1111", OrgWrapper.PostalAddressExcludeCountryIfSame);
			Header.MainAddress.OA_RL_NKRelatedPortCode = "JPAAM";
			Header.MainAddress.OA_State = "01";
			AssertMultilineASCIIEquals("Different country", "MY COMPANY\n12A TOWER BUILDING\nHIGH TOWER\nSTAR CITY, HOKKAIDO\n1111\nJAPAN", OrgWrapper.PostalAddressExcludeCountryIfSame);
		}

		public void TestSelectedAddress()
		{
			OrgHeader sampleOrganisation = Factory.New<OrgHeader>();
			OrgAddress dlvAddress = sampleOrganisation.Addresses.AddNew();
			dlvAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			OrgAddress pstAddress = sampleOrganisation.Addresses.AddNew();
			pstAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Postal);

			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = false;
			docAddress.OrganisationPK = sampleOrganisation.PK;
			docAddress.E2_OA_Address = dlvAddress.PK;
			AssertEquals("Selected Address of JobDocAddress", dlvAddress.PK, ((JobDocAddress)DocOrganisation.New(docAddress, Factory).SelectedAddress.WrappedObject).Address.PK);
			docAddress.E2_OA_Address = pstAddress.PK;
			AssertEquals("Postal Address of JobDocAddress", pstAddress.PK, ((JobDocAddress)DocOrganisation.New(docAddress, Factory).SelectedAddress.WrappedObject).Address.PK);

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Address1 = "Address1";
			docAddress.E2_Address2 = "Address2";
			docAddress.E2_City = "City";
			docAddress.E2_State = "State";
			docAddress.E2_AddressType = "OFC";
			docAddress.E2_Email = "Email";
			docAddress.E2_CompanyName = "Company Name";
			docAddress.E2_Contact = "Contact";
			docAddress.E2_Fax = "+61 (2) 65-4321";
			docAddress.E2_Phone = "+61 (2) 12-3456";
			docAddress.E2_Postcode = "9999";
			DocOrganisation docOrg = DocOrganisation.New(docAddress, Factory);
			AssertEquals("Address1", docOrg.SelectedAddress.Address1);
			AssertEquals("Address2", docOrg.SelectedAddress.Address2);
			AssertEquals("City", docOrg.SelectedAddress.City);
			AssertEquals("State", docOrg.SelectedAddress.State);
			AssertEquals("OFC", docOrg.SelectedAddress.AddressType);
			AssertEquals("Email", ((JobDocAddress)docOrg.SelectedAddress.WrappedObject).E2_Email);
			AssertEquals("Company Name", docOrg.SelectedAddress.CompanyName);
			AssertEquals("+61 (2) 65-4321", docOrg.SelectedAddress.Fax);
			AssertEquals("+61 (2) 12-3456", docOrg.SelectedAddress.Phone);
			AssertEquals("9999", docOrg.SelectedAddress.PostCode);
		}
		#endregion

		#region Invoices

		public void TestDisbursementTerms()
		{
			var newOrg = Factory.New<OrgHeader>();
			DocOrganisation oRGWrapper = DocOrganisation.New(newOrg, Factory);

			ZArchitecture.Core.CodeDescriptionPairList termList = new ARInvoiceTermsList();
			ZString expected = termList.GetDescriptionFromCode("COD").Trim().ToUpper();
			AssertEquals("Invoice terms", expected, oRGWrapper.DisbursmentTerms);

			newOrg.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = "COD";
			AssertEquals("Invoice terms", expected, oRGWrapper.DisbursmentTerms);

			newOrg.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = "INV";
			newOrg.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceDays = 12;

			expected = "12 DAYS " + termList.GetDescriptionFromCode("INV").Trim().ToUpper();
			AssertEquals("Invoice terms", expected, oRGWrapper.DisbursmentTerms);
		}

		public void TestShortDisbursementTerms()
		{
			var newOrg = Factory.New<OrgHeader>();
			DocOrganisation oRGWrapper = DocOrganisation.New(newOrg, Factory);

			ZArchitecture.Core.CodeDescriptionPairList termList = new ARInvoiceTermsList();
			ZString expected = termList.GetDescriptionFromCode("COD").Trim().ToUpper();
			AssertEquals(expected, oRGWrapper.ShortDisbursementTerms);

			newOrg.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = "COD";
			AssertEquals(expected, oRGWrapper.ShortDisbursementTerms);

			newOrg.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = "INV";
			newOrg.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceDays = 12;

			expected = "12 DAYS";
			AssertEquals(expected, oRGWrapper.ShortDisbursementTerms);
		}

		public void TestShortInvoiceTerms()
		{
			var newOrg = Factory.New<OrgHeader>();
			DocOrganisation oRGWrapper = DocOrganisation.New(newOrg, Factory);

			ZArchitecture.Core.CodeDescriptionPairList termList = new ARInvoiceTermsList();
			ZString expected = termList.GetDescriptionFromCode("COD").Trim().ToUpper();
			AssertEquals(expected, oRGWrapper.ShortInvoiceTerms);

			newOrg.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "COD";
			AssertEquals(expected, oRGWrapper.ShortInvoiceTerms);

			newOrg.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "INV";
			newOrg.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 12;

			expected = "12 DAYS";
			AssertEquals("Invoice terms", expected, oRGWrapper.ShortInvoiceTerms);
		}

		#endregion

		#region Custom Fields
		public void TestCarrierMasterBillPrefix()
		{
			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "QAN";
			Header.MiscServ.OM_RM_Airline = airline.PK;
			AssertEquals("QAN", OrgWrapper.CarrierMasterBillPrefix);
		}

		public void TestApprovedExporterCode()
		{
			AssertEquals("", OrgWrapper.ApprovedExporterCode);
			Header.CountryData.OV_EXExportPermissionDetails = "ABC";
			AssertEquals("ABC", OrgWrapper.ApprovedExporterCode);
		}

		public void TestABN()
		{
			var newOrg = Factory.New<OrgHeader>();
			newOrg.OH_Code = "TESTXX";
			var aBN = newOrg.CustomsCodes.AddNew();
			aBN.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			aBN.OK_CustomsRegNo = "Test ABN";
			aBN.OK_RN_NKCodeCountry = "AU";

			DocOrganisation oRGWrapper = DocOrganisation.New(newOrg, Factory);
			AssertEquals("Test ABN", oRGWrapper.ABN);
		}

		public void TestCBR()
		{
			AssertEquals("No CBR", "", OrgWrapper.CBR);

			var cBR = Header.CustomsCodes.AddNew();
			cBR.OK_CodeType = OrgCusCode.CodeTypes.BrokerageRegistration;
			cBR.OK_CustomsRegNo = "080808";
			cBR.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals("CBR", "080808", OrgWrapper.CBR);
		}

		public void TestCCC()
		{
			AssertEquals("No CCC", "", OrgWrapper.CCC);

			var cCC = Header.CustomsCodes.AddNew();
			cCC.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cCC.OK_CustomsRegNo = "090909";
			cCC.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals("CCC", "090909", OrgWrapper.CCC);
		}

		public void TestLSC()
		{
			AssertEquals("No LSC", "", OrgWrapper.LSC);

			var lSC = Header.CustomsCodes.AddNew();
			lSC.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			lSC.OK_CustomsRegNo = "101010";
			lSC.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals("LSC", "101010", OrgWrapper.LSC);
		}

		public void TestGST()
		{
			AssertEquals("No GST", "", OrgWrapper.GST);

			var gST = Header.CustomsCodes.AddNew();
			gST.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			gST.OK_CustomsRegNo = "123456";
			gST.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals("GST", "123456", OrgWrapper.GST);
		}

		public void TestCSC()
		{
			AssertEquals("No CSC", "", OrgWrapper.CSC);

			var cSC = Header.CustomsCodes.AddNew();
			cSC.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			cSC.OK_CustomsRegNo = "BLAH";
			cSC.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals("CSC", "BLAH", OrgWrapper.CSC);
		}

		public void TestCCD()
		{
			AssertEquals("No CCD", "", OrgWrapper.CCD);

			var cCD = Header.CustomsCodes.AddNew();
			cCD.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			cCD.OK_CustomsRegNo = "BLAH";
			cCD.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals("CCD", "BLAH", OrgWrapper.CCD);
		}

		public void TestCID()
		{
			AssertEquals("No CID", "", OrgWrapper.CID);

			var cID = Header.CustomsCodes.AddNew();
			cID.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
			cID.OK_CustomsRegNo = "MEH";
			cID.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals("CID", "MEH", OrgWrapper.CID);
		}

		public void TestPAN()
		{
			AssertEquals("No PAN", "", OrgWrapper.PAN);

			var pAN = Header.CustomsCodes.AddNew();
			pAN.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.PAN;
			pAN.OK_CustomsRegNo = "0123456789";
			pAN.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.India;

			AssertEquals("PAN", "0123456789", OrgWrapper.PAN);
		}

		public void TestKennitala()
		{
			AssertEquals("No Kennitala", "", OrgWrapper.Kennitala);

			var kennitala = Header.CustomsCodes.AddNew();
			kennitala.OK_CodeType = OrgCusCode.CodeTypes.Kennitala;
			kennitala.OK_CustomsRegNo = "Blaticus";
			kennitala.OK_RN_NKCodeCountry = Constants.CountryCodes.Iceland;

			AssertEquals("Kennitala", "Blaticus", OrgWrapper.Kennitala);
		}

		#endregion

		#region MarketingFields

		public void TestIsUserFlag1()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag1);
			Header.OH_IsUserFlag1 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag1);
		}

		public void TestIsUserFlag2()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag2);
			Header.OH_IsUserFlag2 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag2);
		}

		public void TestIsUserFlag3()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag3);
			Header.OH_IsUserFlag3 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag3);
		}

		public void TestIsUserFlag4()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag4);
			Header.OH_IsUserFlag4 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag4);
		}

		public void TestIsUserFlag5()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag5);
			Header.OH_IsUserFlag5 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag5);
		}

		public void TestIsUserFlag6()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag6);
			Header.OH_IsUserFlag6 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag6);
		}

		public void TestIsUserFlag7()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag7);
			Header.OH_IsUserFlag7 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag7);
		}

		public void TestIsUserFlag8()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag8);
			Header.OH_IsUserFlag8 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag8);
		}

		public void TestIsUserFlag9()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag9);
			Header.OH_IsUserFlag9 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag9);
		}

		public void TestIsUserFlag10()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag10);
			Header.OH_IsUserFlag10 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag10);
		}

		public void TestIsUserFlag11()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag11);
			Header.OH_IsUserFlag11 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag11);
		}

		public void TestIsUserFlag12()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag12);
			Header.OH_IsUserFlag12 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag12);
		}

		public void TestIsUserFlag13()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag13);
			Header.OH_IsUserFlag13 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag13);
		}

		public void TestIsUserFlag14()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag14);
			Header.OH_IsUserFlag14 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag14);
		}

		public void TestIsUserFlag15()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag15);
			Header.OH_IsUserFlag15 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag15);
		}

		public void TestIsUserFlag16()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag16);
			Header.OH_IsUserFlag16 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag16);
		}

		public void TestIsUserFlag17()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag17);
			Header.OH_IsUserFlag17 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag17);
		}

		public void TestIsUserFlag18()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag18);
			Header.OH_IsUserFlag18 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag18);
		}

		public void TestIsUserFlag19()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag19);
			Header.OH_IsUserFlag19 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag19);
		}

		public void TestIsUserFlag20()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag20);
			Header.OH_IsUserFlag20 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag20);
		}

		public void TestIsUserFlag21()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag21);
			Header.OH_IsUserFlag21 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag21);
		}

		public void TestIsUserFlag22()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag22);
			Header.OH_IsUserFlag22 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag22);
		}

		public void TestIsUserFlag23()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag23);
			Header.OH_IsUserFlag23 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag23);
		}

		public void TestIsUserFlag24()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag24);
			Header.OH_IsUserFlag24 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag24);
		}

		public void TestIsUserFlag25()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag25);
			Header.OH_IsUserFlag25 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag25);
		}

		public void TestIsUserFlag26()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag26);
			Header.OH_IsUserFlag26 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag26);
		}

		public void TestIsUserFlag27()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag27);
			Header.OH_IsUserFlag27 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag27);
		}

		public void TestIsUserFlag28()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag28);
			Header.OH_IsUserFlag28 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag28);
		}

		public void TestIsUserFlag29()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag29);
			Header.OH_IsUserFlag29 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag29);
		}

		public void TestIsUserFlag30()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag30);
			Header.OH_IsUserFlag30 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag30);
		}

		public void TestIsUserFlag31()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag31);
			Header.OH_IsUserFlag31 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag31);
		}

		public void TestIsUserFlag32()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUserFlag32);
			Header.OH_IsUserFlag32 = ZBool.True;
			Assert(OrgWrapper.IsUserFlag32);
		}

		#endregion

		#region Basic Organisation Details

		public void TestCountry()
		{
			var lOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Header.OH_RL_NKClosestPort = lOCO.Code;
			AssertEquals(lOCO.Country.RN_Desc, OrgWrapper.Country.Name);
		}

		public void TestContacts()
		{
			AssertEquals(0, OrgWrapper.Contacts.Count);

			Header.Contacts.AddNew().OC_ContactName = "C1";
			Header.Contacts.AddNew().OC_ContactName = "C2";
			Header.Contacts.AddNew().OC_ContactName = "C3";

			Header.OH_Code = "TST";
			Header.Factory.Save();

			AssertEquals(3, OrgWrapper.Contacts.Count);
		}

		public void TestAddress1()
		{
			Header.MainAddress.OA_Address1 = "123 Bourke Street";
			AssertEquals("123 Bourke Street", OrgWrapper.Address1);
		}

		public void TestAddress2()
		{
			Header.MainAddress.OA_Address2 = "123 Lala Land";
			AssertEquals("123 Lala Land", OrgWrapper.Address2);
		}

		public void TestCity()
		{
			Header.MainAddress.OA_City = "Star City";
			AssertEquals("Star City", OrgWrapper.City);
		}

		public void TestCode()
		{
			Header.OH_Code = "TESTCODE";
			AssertEquals("TESTCODE", OrgWrapper.Code);
		}

		public void TestEmail()
		{
			Header.MainAddress.OA_Email = "blah@email.com";
			AssertEquals("blah@email.com", OrgWrapper.Email);
		}

		public void TestExportFreightContact()
		{
			var org1 = Factory.New<OrgHeader>();
			OrgContact contact1 = org1.Contacts.AddNew();
			contact1.Documents.AddNew().OD_DocumentGroup = ContactType.ExportFreightAgent.Code;
			contact1.OC_ContactName = "Mr Export Freight Agent";
			DocOrganisation docOrg1 = DocOrganisation.New(org1, Factory);
			AssertEquals("Expecting Org to have ExportFreight Contact", contact1.OC_ContactName, docOrg1.ExportFowardingContact.ContactName);

			var org2 = Factory.New<OrgHeader>();
			OrgContact contact2 = org2.Contacts.AddNew();
			contact2.Documents.AddNew().OD_DocumentGroup = ContactType.All.Code;
			contact2.OC_ContactName = "ALL Contact";
			DocOrganisation docOrg2 = DocOrganisation.New(org2, Factory);
			AssertEquals("Expecting Org to have the All Contact", contact2.OC_ContactName, docOrg2.ExportFowardingContact.ContactName);

			var org3 = Factory.New<OrgHeader>();
			DocOrganisation docOrg3 = DocOrganisation.New(org3, Factory);
			AssertEquals("Expecting Org to not have an empty contact", true, !docOrg3.ExportFowardingContact.ContactName.IsEmpty);
		}

		public void TestImportFreightContact()
		{
			var org1 = Factory.New<OrgHeader>();
			OrgContact contact1 = org1.Contacts.AddNew();
			contact1.Documents.AddNew().OD_DocumentGroup = ContactType.ImportFreightAgent.Code;
			contact1.OC_ContactName = "Mr Import Freight Agent";
			DocOrganisation docOrg1 = DocOrganisation.New(org1, Factory);
			AssertEquals("Expecting Org to have Import Freight Contact", contact1.OC_ContactName, docOrg1.ImportFowardingContact.ContactName);

			var org2 = Factory.New<OrgHeader>();
			OrgContact contact2 = org2.Contacts.AddNew();
			contact2.Documents.AddNew().OD_DocumentGroup = ContactType.All.Code;
			contact2.OC_ContactName = "ALL Contact";
			DocOrganisation docOrg2 = DocOrganisation.New(org2, Factory);
			AssertEquals("Expecting Org to have the All Contact", contact2.OC_ContactName, docOrg2.ImportFowardingContact.ContactName);
		}

		public void TestFax()
		{
			Header.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, "AU")).RL_Code;
			Header.MainAddress.OA_Fax_Formatted = "02 5555 9999";
			AssertEquals("+61 2 5555 9999", OrgWrapper.Fax);
		}

		public void TestBusinessRegNo()
		{
			Header.PrimaryRegistrationNumber.Number = "ABN 123 1234";
			AssertEquals("ABN 123 1234", OrgWrapper.BusinessRegNo);
		}

		public void TestStaffAssignments()
		{
			Header.StaffAssignments.RemoveAndDeleteAll();
			OrgStaffAssignments item = Header.StaffAssignments.AddNew();
			item.O8_Department = "SEA";
			AssertEquals("Count", 1, OrgWrapper.StaffAssignments.Count);
			AssertEquals("SEA", OrgWrapper.StaffAssignments[0].O8_Department);
		}

		public void TestBusinessRegType()
		{
			Header.PrimaryRegistrationNumber.NumberTypeForDisplay = "GST";
			AssertEquals("GST", OrgWrapper.BusinessRegType);
		}

		public void TestName()
		{
			Header.OH_FullName = "FULL NAME TESTING";
			AssertEquals("FULL NAME TESTING", OrgWrapper.Name);
		}

		public void TestBranch()
		{
			Header.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbBranch.CurrentBranch.GB_BranchName, OrgWrapper.Branch.BranchName);
		}

		public void TestIsActive()
		{
			Assert(OrgWrapper.IsActive);
			Header.OH_IsActive = ZBool.False;
			AssertEquals(ZBool.False, OrgWrapper.IsActive);
		}

		public void TestIsAirCTO()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsAirCTO);
			Header.OH_IsAirCTO = ZBool.True;
			Assert(OrgWrapper.IsAirCTO);
		}

		public void TestIsAirLine()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsAirLine);
			Header.OH_IsAirLine = ZBool.True;
			Assert(OrgWrapper.IsAirLine);
		}

		public void TestIsAirWholesaler()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsAirWholesaler);
			Header.OH_IsAirWholesaler = ZBool.True;
			Assert(OrgWrapper.IsAirWholesaler);
		}

		public void TestIsBroker()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsBroker);
			Header.OH_IsBroker = ZBool.True;
			Assert(OrgWrapper.IsBroker);
		}

		public void TestIsCompetitor()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsCompetitor);
			Header.OH_IsCompetitor = ZBool.True;
			Assert(OrgWrapper.IsCompetitor);
		}

		public void TestIsConsignee()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsConsignee);
			Header.OH_IsConsignee = ZBool.True;
			Assert(OrgWrapper.IsConsignee);
		}

		public void TestIsConsignor()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsConsignor);
			Header.OH_IsConsignor = ZBool.True;
			Assert(OrgWrapper.IsConsignor);
		}

		public void TestIsContainerPark()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsContainerPark);
			Header.OH_IsContainerYard = ZBool.True;
			Assert(OrgWrapper.IsContainerPark);
		}

		public void TestIsCreditor()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsCreditor);
			Header.OH_IsCreditor = ZBool.True;
			Assert(OrgWrapper.IsCreditor);
		}

		public void TestIsDebtor()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsDebtor);
			Header.OH_IsDebtor = ZBool.True;
			Assert(OrgWrapper.IsDebtor);
		}

		public void TestIsForwarder()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsForwarder);
			Header.OH_IsForwarder = ZBool.True;
			Assert(OrgWrapper.IsForwarder);
		}

		public void TestIsInlandWaterwayProvider()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsInlandWaterwayProvider);
			Header.OH_IsInlandWaterwayProvider = ZBool.True;
			Assert(OrgWrapper.IsInlandWaterwayProvider);
		}

		public void TestIsLineHaulProvider()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsLineHaulProvider);
			Header.OH_IsLineHaulProvider = ZBool.True;
			Assert(OrgWrapper.IsLineHaulProvider);
		}

		public void TestIsLocalTransport()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsLocalTransport);
			Header.OH_IsLocalTransport = ZBool.True;
			Assert(OrgWrapper.IsLocalTransport);
		}

		public void TestIsMiscFreightServices()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsMiscFreightServices);
			Header.OH_IsMiscFreightServices = ZBool.True;
			Assert(OrgWrapper.IsMiscFreightServices);
		}

		public void TestIsPackDepot()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsPackDepot);
			Header.OH_IsPackDepot = ZBool.True;
			Assert(OrgWrapper.IsPackDepot);
		}

		public void TestIsRailProvider()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsRailProvider);
			Header.OH_IsRailProvider = ZBool.True;
			Assert(OrgWrapper.IsRailProvider);
		}

		public void TestIsSalesLead()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsSalesLead);
			Header.OH_IsSalesLead = ZBool.True;
			Assert(OrgWrapper.IsSalesLead);
		}

		public void TestIsSeaCTO()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsSeaCTO);
			Header.OH_IsSeaCTO = ZBool.True;
			Assert(OrgWrapper.IsSeaCTO);
		}

		public void TestIsSeaWholesaler()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsSeaWholesaler);
			Header.OH_IsSeaWholesaler = ZBool.True;
			Assert(OrgWrapper.IsSeaWholesaler);
		}

		public void TestIsShippingLine()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsShippingLine);
			Header.OH_IsShippingLine = ZBool.True;
			Assert(OrgWrapper.IsShippingLine);
		}

		public void TestIsShippingProvider()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsShippingProvider);
			Header.OH_IsShippingProvider = ZBool.True;
			Assert(OrgWrapper.IsShippingProvider);
		}

		public void TestIsTempAccount()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsTempAccount);
			Header.OH_IsTempAccount = ZBool.True;
			Assert(OrgWrapper.IsTempAccount);
		}

		public void TestIsTransportClient()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsTransportClient);
			Header.OH_IsTransportClient = ZBool.True;
			Assert(OrgWrapper.IsTransportClient);
		}

		public void TestIsUnpackDepot()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsUnpackDepot);
			Header.OH_IsUnpackDepot = ZBool.True;
			Assert(OrgWrapper.IsUnpackDepot);
		}

		public void TestIsWarehouseClient()
		{
			AssertEquals(ZBool.False, OrgWrapper.IsWarehouseClient);
			Header.OH_IsWarehouseClient = ZBool.True;
			Assert(OrgWrapper.IsWarehouseClient);
		}

		public void TestMobile()
		{
			Header.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, "AU")).RL_Code;
			Header.MainAddress.OA_Mobile = "0407 838383";
			AssertEquals("+61 407 838 383", OrgWrapper.Mobile);
		}

		public void TestPhone()
		{
			Header.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, "AU")).RL_Code;
			Header.MainAddress.OA_Phone_Formatted = "08 83838383";
			AssertEquals("+61 8 8383 8383", OrgWrapper.Phone);
		}

		public void TestPostCode()
		{
			Header.MainAddress.OA_PostCode = "8383";
			AssertEquals("8383", OrgWrapper.PostCode);
		}

		public void TestLoco()
		{
			Header.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			AssertEquals(GlbBranch.CurrentBranch.GB_RL_NKHomePort, OrgWrapper.Loco.Code);
		}

		public void TestState()
		{
			Header.MainAddress.OA_State = "VIC";
			AssertEquals("VIC", OrgWrapper.State);
		}

		public void TestWeb()
		{
			Header.MainWebURL.PU_URL = "www.dotcom.com";
			AssertEquals("www.dotcom.com", OrgWrapper.Web);
		}

		public void TestContext()
		{
			//If you need to change this then please change the #if's in Notes.xls
			AssertEquals("ORGANISATION", OrgWrapper.Context);
		}

		public void TestAllNotes()
		{
			AssertEquals(0, OrgWrapper.AllNotes.Length);

			ZString note = "Testing notes from OrgHeader.\nThis should be the second line of the note.";
			var note1 = Header.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			note1.ST_NoteDataAsText = note;
			AssertEquals(3, OrgWrapper.AllNotes.Length);

			note = "Testing yet another notes from OrgHeader.\nThis should be a different note for Orgheader.";
			var note2 = Header.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.InvoicingPreferences.Description;
			note2.ST_NoteDataAsText = note;
			AssertEquals(7, OrgWrapper.AllNotes.Length);
		}

		public void TestLocalCustomsClientCode()
		{
			var cusCode = Header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			cusCode.OK_CustomsRegNo = "Customs Client Code";
			AssertEquals("Customs Client Code", "Customs Client Code", OrgWrapper.LocalCustomsClientCode);
		}

		public void TestLocalCustomsCarrierCode()
		{
			var cusCode = Header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "CarrierCode";
			AssertEquals("CarrierCode", "CarrierCode", OrgWrapper.LocalCustomsCarrierCode);
		}

		public void TestLocalBusinessRegNo()
		{
			var header = Factory.New<OrgHeader>();
			header.LocalBusinessRegNo = "12345";
			OrgWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("Local Business Rego No", "12345", OrgWrapper.LocalBusinessRegNo);
		}

		public void TestLocalCustomsSupplierCode()
		{
			var cusCode = Header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			cusCode.OK_CustomsRegNo = "SupplierCode";
			AssertEquals("SupplierCode", "SupplierCode", OrgWrapper.LocalCustomsSupplierCode);
		}

		public void TestLocalVATCode()
		{
			var cusCode = Header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = "VATCode";
			AssertEquals("VATCode", "VATCode", OrgWrapper.LocalVATCode);
		}

		public void TestHandlingInstruction()
		{
			var detailedDescription = Header.Notes.AddNew();
			detailedDescription.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			detailedDescription.ST_ParentID = Header.PK;
			detailedDescription.ST_Table = Header.TableName;
			detailedDescription.ST_NoteDataAsText = "Handling instruction\nLine Two";

			var otherNotes = Header.Notes.AddNew();
			otherNotes.ST_ParentID = Header.PK;
			otherNotes.ST_Table = Header.TableName;
			otherNotes.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			otherNotes.ST_NoteDataAsText = "This is other notes and should not be included.";

			AssertEquals("Handling Instructions", "Handling instruction\nLine Two", OrgWrapper.HandlingInstructions);
		}

		public void TestCartageInstruction()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			FreightHelperClass.AddNote(Header, pickupDesc, "Header Pickup Instructions");
			FreightHelperClass.AddNote(Header, deliveryDesc, "Header Delivery Instructions");

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("All Cartage Instructions", "Header Delivery Instructions", OrgWrapper.CartageInstructions);

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("All Cartage Instructions", "Header Pickup Instructions", OrgWrapper.CartageInstructions);
		}

		public void TestGetCartageInstructionsByTransportMode()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			StmNoteContexts stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.I;
			FreightHelperClass.AddNote(Header, pickupDesc, "Header Pickup Instructions", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.S;
			FreightHelperClass.AddNote(Header, deliveryDesc, "Header Delivery Instructions", stmNoteContext);

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("All Cartage Instructions", "Header Delivery Instructions", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode(Constants.TransportModes.Sea, ""));
			AssertEquals("All Cartage Instructions", "", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode(Constants.TransportModes.Air, "LCL"));

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("All Cartage Instructions", "Header Pickup Instructions", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode(Constants.TransportModes.Air, ""));
			AssertEquals("All Cartage Instructions", "", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode(Constants.TransportModes.Sea, "FCL"));
		}

		public void TestGetCartageInstructionsByContainerMode()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			StmNoteContexts stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, pickupDesc, "Header Pickup Instructions FCL", stmNoteContext);
			FreightHelperClass.AddNote(Header, deliveryDesc, "Header Delivery Instructions FCL", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.L;
			FreightHelperClass.AddNote(Header, deliveryDesc, "Header Delivery Instructions LCL", stmNoteContext);

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			ZString actual = OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("SEA", "LCL");
			AssertEquals("LCL Cartage Instructions", "Header Delivery Instructions LCL", actual);
			actual = OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("SEA", "FCL");
			AssertEquals("FCL Cartage Instructions", "Header Delivery Instructions FCL", actual);
			actual = OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("SEA", "LSE");
			AssertEquals("LSE - Cartage Instructions - no match: ", "", actual);

			stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.S;
			FreightHelperClass.AddNote(Header, deliveryDesc, "Header Delivery Instructions SEA", stmNoteContext);

			actual = OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("SEA", "ALL");
			AssertEquals("SEA Cartage Instructions", "Header Delivery Instructions SEA", actual);

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			actual = OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("", "FCL");
			AssertEquals("FCL Cartage Instructions", "Header Pickup Instructions FCL", actual);
			actual = OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("AIR", "LSE");
			AssertEquals("LSE - no match - No Cartage Instructions: ", "", actual);

			stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.A;
			FreightHelperClass.AddNote(Header, deliveryDesc, "ALL Instructions", stmNoteContext);

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			actual = OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("SEA", "LCL");
			AssertEquals("LCL Cartage Instructions", "Header Delivery Instructions SEA\nALL Instructions", actual);
			actual = OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("SEA", "FCL");
			AssertEquals("FCL Cartage Instructions", "Header Delivery Instructions SEA\nALL Instructions", actual);
		}

		public void TestGetCartageInstructionsByDirectionAndTransportOrContainerMode()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			var stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.I;
			FreightHelperClass.AddNote(Header, pickupDesc, "Pickup Cartage Instructions - Air", stmNoteContext);
			FreightHelperClass.AddNote(Header, deliveryDesc, "Delivery Cartage Instructions - Air", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.E;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.I;
			FreightHelperClass.AddNote(Header, pickupDesc, "Pickup Cartage Instructions - Air/Export", stmNoteContext);
			FreightHelperClass.AddNote(Header, deliveryDesc, "Delivery Cartage Instructions - Air/Export", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.S;
			FreightHelperClass.AddNote(Header, pickupDesc, "Pickup Cartage Instructions - Sea", stmNoteContext);
			FreightHelperClass.AddNote(Header, deliveryDesc, "Delivery Cartage Instructions - Sea", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.I;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.S;
			FreightHelperClass.AddNote(Header, pickupDesc, "Pickup Cartage Instructions - Sea/Import", stmNoteContext);
			FreightHelperClass.AddNote(Header, deliveryDesc, "Delivery Cartage Instructions - Sea/Import", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.L;
			FreightHelperClass.AddNote(Header, pickupDesc, "Pickup Cartage Instructions LCL", stmNoteContext);
			FreightHelperClass.AddNote(Header, deliveryDesc, "Delivery Cartage Instructions LCL", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, pickupDesc, "Pickup GHI FCL", stmNoteContext);
			FreightHelperClass.AddNote(Header, deliveryDesc, "Delivery GHI FCL", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.I;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, pickupDesc, "Pickup GHI FCL/Import", stmNoteContext);
			FreightHelperClass.AddNote(Header, deliveryDesc, "Delivery GHI FCL/Import", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.E;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, pickupDesc, "Pickup GHI FCL/Export", stmNoteContext);
			FreightHelperClass.AddNote(Header, deliveryDesc, "Delivery GHI FCL/Export", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, pickupDesc, "Pickup GHI FCL/ALL Directions", stmNoteContext);
			FreightHelperClass.AddNote(Header, deliveryDesc, "Delivery GHI FCL/ALL Directions", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.B;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, pickupDesc, "Pickup GHI FCL/Both Import & Export", stmNoteContext);
			FreightHelperClass.AddNote(Header, deliveryDesc, "Delivery GHI FCL/Both Import & Export", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Module = StmNoteContextModule.A;
			stmNoteContext.Direction = StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.A;
			FreightHelperClass.AddNote(Header, pickupDesc, "Pickup GHI - All", stmNoteContext);
			FreightHelperClass.AddNote(Header, deliveryDesc, "Delivery GHI - All", stmNoteContext);

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Import Air FCL Cartage Instructions", "Delivery Cartage Instructions - Air\nDelivery GHI - All\nDelivery GHI FCL\nDelivery GHI FCL/Import\nDelivery GHI FCL/ALL Directions\nDelivery GHI FCL/Both Import & Export", OrgWrapper.GetCartageInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), "AIR", "FCL"));
			AssertEquals("No match for LSE container mode, just return Cartage notes for Import AIR, in this case the notes created for ALL Air", "Delivery Cartage Instructions - Air\nDelivery GHI - All", OrgWrapper.GetCartageInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), "AIR", "LSE"));
			AssertEquals("Import Sea", "Delivery Cartage Instructions - Sea\nDelivery Cartage Instructions - Sea/Import\nDelivery GHI - All", OrgWrapper.GetCartageInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), "SEA", ""));
			AssertEquals("Import Sea LCL", "Delivery Cartage Instructions - Sea\nDelivery Cartage Instructions - Sea/Import\nDelivery GHI - All\nDelivery Cartage Instructions LCL", OrgWrapper.GetCartageInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), "SEA", "LCL"));
			AssertEquals("Import Sea FCL", "Delivery Cartage Instructions - Sea\nDelivery Cartage Instructions - Sea/Import\nDelivery GHI - All\nDelivery GHI FCL\nDelivery GHI FCL/Import\nDelivery GHI FCL/ALL Directions\nDelivery GHI FCL/Both Import & Export", OrgWrapper.GetCartageInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), "SEA", "FCL"));

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Export Air LCL Cartage Instructions", "Pickup Cartage Instructions - Air\nPickup Cartage Instructions - Air/Export\nPickup GHI - All\nPickup Cartage Instructions LCL", OrgWrapper.GetCartageInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.E), "AIR", "LCL"));
			AssertEquals("Export Air FCL Cartage Instructions", "Pickup Cartage Instructions - Air\nPickup Cartage Instructions - Air/Export\nPickup GHI - All\nPickup GHI FCL\nPickup GHI FCL/Export\nPickup GHI FCL/ALL Directions\nPickup GHI FCL/Both Import & Export", OrgWrapper.GetCartageInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.E), "AIR", "FCL"));
		}

		public void TestGetHandlingInstructionsByDirectionAndTransportOrContainerMode()
		{
			string handlingDesc = PredefinedNoteTypes.Instance.HandlingInstructions.Description;

			var stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.I;
			FreightHelperClass.AddNote(Header, handlingDesc, "Handling Instructions - Air", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.E;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.I;
			FreightHelperClass.AddNote(Header, handlingDesc, "Handling Instructions - Air/Export", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.S;
			FreightHelperClass.AddNote(Header, handlingDesc, "Handling Instructions - Sea", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.I;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.S;
			FreightHelperClass.AddNote(Header, handlingDesc, "Handling Instructions - Sea/Import", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.L;
			FreightHelperClass.AddNote(Header, handlingDesc, "Handling Instructions LCL", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, handlingDesc, "GHI FCL", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.I;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, handlingDesc, "GHI FCL/Import", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.E;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, handlingDesc, "GHI FCL/Export", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, handlingDesc, "GHI FCL/ALL Directions", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.B;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, handlingDesc, "GHI FCL/Both Import & Export", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Module = StmNoteContextModule.A;
			stmNoteContext.Direction = StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.A;
			FreightHelperClass.AddNote(Header, handlingDesc, "GHI - All", stmNoteContext);

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Import Air FCL Handling Instructions", "Handling Instructions - Air\nGHI - All\nGHI FCL\nGHI FCL/Import\nGHI FCL/ALL Directions\nGHI FCL/Both Import & Export", OrgWrapper.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), "AIR", "FCL"));
			AssertEquals("No match for LSE container mode, just return handling notes for Import AIR, in this case the notes created for ALL Air", "Handling Instructions - Air\nGHI - All", OrgWrapper.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), "AIR", "LSE"));
			AssertEquals("Import Sea", "Handling Instructions - Sea\nHandling Instructions - Sea/Import\nGHI - All", OrgWrapper.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), "SEA", ""));
			AssertEquals("Import Sea LCL", "Handling Instructions - Sea\nHandling Instructions - Sea/Import\nGHI - All\nHandling Instructions LCL", OrgWrapper.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), "SEA", "LCL"));
			AssertEquals("Import Sea FCL", "Handling Instructions - Sea\nHandling Instructions - Sea/Import\nGHI - All\nGHI FCL\nGHI FCL/Import\nGHI FCL/ALL Directions\nGHI FCL/Both Import & Export", OrgWrapper.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), "SEA", "FCL"));

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Export Air LCL Handling Instructions", "Handling Instructions - Air\nHandling Instructions - Air/Export\nGHI - All\nHandling Instructions LCL", OrgWrapper.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.E), "AIR", "LCL"));
			AssertEquals("Export Air FCL Handling Instructions", "Handling Instructions - Air\nHandling Instructions - Air/Export\nGHI - All\nGHI FCL\nGHI FCL/Export\nGHI FCL/ALL Directions\nGHI FCL/Both Import & Export", OrgWrapper.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.E), "AIR", "FCL"));
		}

		public void TestGetSpecialInstructionsByDirectionAndTransportOrContainerMode()
		{
			string specialDesc = PredefinedNoteTypes.Instance.SpecialInstructions.Description;

			var stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.I;
			FreightHelperClass.AddNote(Header, specialDesc, "Special Instructions - Air", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.E;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.I;
			FreightHelperClass.AddNote(Header, specialDesc, "Special Instructions - Air/Export", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.S;
			FreightHelperClass.AddNote(Header, specialDesc, "Special Instructions - Sea", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.I;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.S;
			FreightHelperClass.AddNote(Header, specialDesc, "Special Instructions - Sea/Import", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.L;
			FreightHelperClass.AddNote(Header, specialDesc, "Special Instructions LCL", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, specialDesc, "GHI FCL", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.I;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, specialDesc, "GHI FCL/Import", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.E;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, specialDesc, "GHI FCL/Export", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, specialDesc, "GHI FCL/ALL Directions", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Direction |= StmNoteContextDirection.B;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, specialDesc, "GHI FCL/Both Import & Export", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.Module = StmNoteContextModule.A;
			stmNoteContext.Direction = StmNoteContextDirection.A;
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.A;
			FreightHelperClass.AddNote(Header, specialDesc, "GHI - All", stmNoteContext);

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Import Air FCL Special Instructions", "Special Instructions - Air\nGHI - All\nGHI FCL\nGHI FCL/Import\nGHI FCL/ALL Directions\nGHI FCL/Both Import & Export", OrgWrapper.GetSpecialInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), "AIR", "FCL"));
			AssertEquals("No match for LSE container mode, just return Special notes for Import AIR, in this case the notes created for ALL Air", "Special Instructions - Air\nGHI - All", OrgWrapper.GetSpecialInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), "AIR", "LSE"));
			AssertEquals("Import Sea", "Special Instructions - Sea\nSpecial Instructions - Sea/Import\nGHI - All", OrgWrapper.GetSpecialInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), "SEA", ""));
			AssertEquals("Import Sea LCL", "Special Instructions - Sea\nSpecial Instructions - Sea/Import\nGHI - All\nSpecial Instructions LCL", OrgWrapper.GetSpecialInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), "SEA", "LCL"));
			AssertEquals("Import Sea FCL", "Special Instructions - Sea\nSpecial Instructions - Sea/Import\nGHI - All\nGHI FCL\nGHI FCL/Import\nGHI FCL/ALL Directions\nGHI FCL/Both Import & Export", OrgWrapper.GetSpecialInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), "SEA", "FCL"));

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Export Air LCL Special Instructions", "Special Instructions - Air\nSpecial Instructions - Air/Export\nGHI - All\nSpecial Instructions LCL", OrgWrapper.GetSpecialInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.E), "AIR", "LCL"));
			AssertEquals("Export Air FCL Special Instructions", "Special Instructions - Air\nSpecial Instructions - Air/Export\nGHI - All\nGHI FCL\nGHI FCL/Export\nGHI FCL/ALL Directions\nGHI FCL/Both Import & Export", OrgWrapper.GetSpecialInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.E), "AIR", "FCL"));
		}

		public void TestARCreditManagementNote()
		{
			Header.OH_IsDebtor = true;

			var creditMgtNotes = Header.Notes.AddNew();
			creditMgtNotes.ST_Description = PredefinedNoteTypes.Instance.AccountsReceivableCreditManagementNote.Description;
			creditMgtNotes.ST_ParentID = Header.PK;
			creditMgtNotes.ST_Table = Header.TableName;
			creditMgtNotes.ST_NoteDataAsText = "A/R Credit Management Notes";

			AssertEquals("AR Credit Notes", "A/R Credit Management Notes", OrgWrapper.ARCreditManagementNote);
		}

		public void TestEIN()
		{
			var cusCode = Header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			cusCode.OK_CustomsRegNo = "12-345678901";
			AssertEquals("12-3456789", OrgWrapper.EIN);
		}

		public void TestSSN()
		{
			var cusCode = Header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			cusCode.OK_CustomsRegNo = "SSNCode";
			AssertEquals("SSNCode", OrgWrapper.SSN);
		}

		public void TestSSNorEIN()
		{
			var cusCode = Header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			cusCode.OK_CustomsRegNo = "12-345678901";
			AssertEquals("12-3456789", OrgWrapper.SSNorEIN);

			cusCode = Header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			cusCode.OK_CustomsRegNo = "SSNCode";
			AssertEquals("SSNCode", OrgWrapper.SSNorEIN);
		}

		#endregion

		#region InvoiceOrders

		public void TestInvoiceOrders()
		{
			AssertEquals(Header.InvoiceOrders, OrgWrapper.InvoiceOrders);
		}

		#endregion

		#region Implementation
		internal OrgHeader Header;
		DocOrganisation OrgWrapper;
		public override void TestWrapperMappingsEmpty()
		{
			//If the source is null, this wrapper is null, so let's ignore this test
			Assert(true);
		}

		protected override ZString ExpectedDefaultFormatting => @"
EXFreightBillTo : 
IMFreightBillTo : 
Registry : (No Default Field Value Available on Registry)
";
		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return DocOrganisation.New(Header, Factory);
		}

		protected override void SetUp()
		{
			Header = Factory.NewWithValidTestData<OrgHeader>();
			Header.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			OrgWrapper = GetNewDocumentWrapper() as DocOrganisation;
			Env.Registry.SetOrgUsePhoneNumberFormatting(true);

			base.SetUp();
		}

		protected override string ExpectedFieldMap => @"
DocOrganisation                                  (Default Field: Name)
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

		#endregion

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			if (Header == null)
			{
				Header = Factory.NewWithValidTestData<OrgHeader>();
				Header.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			}

			return DocOrganisation.New(Header, Factory);
		}

		public override void TestWrapperNotes()
		{
			Header.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "These notes exist against the organisation and should be algamated");
			Header.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "This is the second line that gets added to the first line using the indexer");
			Header.Notes.AddNew(false, PredefinedNoteTypes.Instance.AgentNotes.Description, "Some agent notes that should not be joined together as there are no others");

			AssertEquals("Notes Count", 3, OrgWrapper.Notes.Count);

			var noteWrapper = OrgWrapper.Notes[PredefinedNoteTypes.Instance.HandlingInstructions.Description];
			AssertNotNull("The handling instructions is found using the indexer", noteWrapper);
			AssertEquals("The Text is algamated", "These notes exist against the organisation and should be algamated\r\nThis is the second line that gets added to the first line using the indexer", noteWrapper.Text);
			AssertEquals("noteWrapper.Description", "Goods Handling Instructions", noteWrapper.Description);

			noteWrapper = OrgWrapper.Notes[PredefinedNoteTypes.Instance.AgentNotes.Description];
			AssertNotNull("Int indexer gets the agent Notes", noteWrapper);
			AssertEquals("noteWrapper.Text", "Some agent notes that should not be joined together as there are no others", noteWrapper.Text);
			AssertEquals("noteWrapper.Description", "Agent Notes", noteWrapper.Description);
		}
	}
}
