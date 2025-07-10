namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Schema;
	using CargoWise.Types;
	using Enterprise.Core;
	using Enterprise.DocumentEngineCore.DocumentSupport;
	using Enterprise.DocumentEngineCore.DocWrappers;
	using Enterprise.DocumentWrappersCore.Testing;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Business.Testing;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(OrgHeaderSource))]
	public class OrgHeaderSourceTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { OrgHeaderSource.New(Header, Factory) };
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
			OrgHeaderSource docOrg = OrgHeaderSource.New(Header, Factory);
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

		#region Spliting of Name

		public void TestSplitOfNameWithLengthMoreThan25()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "123456789 123456789 123456789 123456789 123456789";

			ZString wrappedVersion = OrgWrapper.WrapTextForAColumn(header.OH_FullName, 25);
			ZString expectedVersionForName1 = wrappedVersion.Split('\n')[0];
			ZString expectedVersionForName2 = wrappedVersion.Split('\n')[1];
			OrgWrapper = OrgHeaderSource.New(header, Factory);

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
			OrgWrapper = OrgHeaderSource.New(header, Factory);

			AssertEquals("Name 1", expectedVersionForName1, OrgWrapper.SplitFullName1);
			AssertEquals("Name 2", expectedVersionForName2, OrgWrapper.SplitFullName2);
		}

		#endregion

		#region Addresses

		public void TestMainAddress()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.MainAddress.OA_Address1 = "SampleAddress";
			OrgWrapper = OrgHeaderSource.New(header, Factory);
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
			OrgWrapper = OrgHeaderSource.New(header, Factory);
			AssertEquals("Address for PAD", pADAddressWrapper.PostalAddress, OrgWrapper.DeliverAddress.PostalAddress);

			OrgAddress dLVAddress = header.Addresses.AddNew();
			dLVAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			dLVAddress.OA_Address1 = "DLV Address 1";
			dLVAddress.OA_Address2 = "DLV Address 2";
			dLVAddress.OA_City = "DLV Address City";

			pADAddress.AddressCapability.SetIsNotMainAddress(OrgConstants.AddressType.PickupAndDelivery);
			dLVAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Delivery);

			DocAddress dLVAddressWrapper = DocAddress.New(dLVAddress, Factory);
			OrgWrapper = OrgHeaderSource.New(header, Factory);
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
			OrgWrapper = OrgHeaderSource.New(header, Factory);
			AssertNotNull("Address for PAD is not null", OrgWrapper.OrganisationPADPostalAddress);
			AssertEquals("Address for PAD", pADAddressWrapper.PostalAddress, OrgWrapper.OrganisationPADPostalAddress.PostalAddress);
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
			OrgWrapper = OrgHeaderSource.New(header, Factory);
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
			OrgWrapper = OrgHeaderSource.New(header, Factory);
			AssertEquals("Address for PIC", pICAddressWrapper.PostalAddress, OrgWrapper.PickUpAddress.PostalAddress);

			pICAddress.AddressCapability.SetIsNotMainAddress(OrgConstants.AddressType.Pickup);
			pADAddress.AddressCapability.SetIsNotMainAddress(OrgConstants.AddressType.PickupAndDelivery);
			AssertEquals("Address for PAD; default", pADAddressWrapper.PostalAddress, OrgWrapper.DeliverAddress.PostalAddress);
		}

		public void TestARAddress()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress officeAddress = header.Addresses.AddNew();
			SetupAddress(header.MainAddress, "Main Office Address", OrgConstants.AddressType.Office, true);
			SetupAddress(officeAddress, "Office Address", OrgConstants.AddressType.Office, false);
			OrgWrapper = OrgHeaderSource.New(header, Factory);
			AssertEquals("Main Office address", OrgWrapper.PostalAddress, OrgWrapper.ARAddress.PostalAddress);

			OrgAddress mainPostalAddress = header.Addresses.AddNew();
			OrgAddress postalAddress = header.Addresses.AddNew();
			SetupAddress(mainPostalAddress, "Main Postal Address", OrgConstants.AddressType.Postal, true);
			SetupAddress(postalAddress, "Postal Address", OrgConstants.AddressType.Postal, false);
			DocAddress mainPostalAddressWrapper = DocAddress.New(mainPostalAddress, Factory);
			OrgWrapper = OrgHeaderSource.New(header, Factory);
			AssertEquals("Main Postal address should be used over Office address", mainPostalAddressWrapper.PostalAddress, OrgWrapper.ARAddress.PostalAddress);

			OrgAddress mainReceivablesAddress = header.Addresses.AddNew();
			OrgAddress receivablesAddress = header.Addresses.AddNew();
			SetupAddress(mainReceivablesAddress, "Main Receivables Address", OrgConstants.AddressType.Receivables, true);
			SetupAddress(receivablesAddress, "Receivables Address", OrgConstants.AddressType.Receivables, false);
			DocAddress mainReceivablesAddressWrapper = DocAddress.New(mainReceivablesAddress, Factory);
			OrgWrapper = OrgHeaderSource.New(header, Factory);
			AssertEquals("Main Receivables address should be used over postal and office address", mainReceivablesAddressWrapper.PostalAddress, OrgWrapper.ARAddress.PostalAddress);
		}

		public void TestARAddressLanguage()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Language = Constants.Languages.English;
			SetupAddress(header.MainAddress, "Main Office Address", OrgConstants.AddressType.Office, true);

			OrgAddress receivableAddress1 = header.Addresses.AddNew();
			SetupAddress(receivableAddress1, "English Receivable Address", OrgConstants.AddressType.Receivables, true, Constants.Languages.English);
			OrgAddress receivableAddress2 = header.Addresses.AddNew();
			SetupAddress(receivableAddress2, "Chinese Receivable Address", OrgConstants.AddressType.Receivables, true, Constants.Languages.ChineseSimplified);
			OrgAddress postalAddress1 = header.Addresses.AddNew();
			SetupAddress(postalAddress1, "Postal Address English", OrgConstants.AddressType.Postal, true, Constants.Languages.English);
			OrgAddress postalAddress2 = header.Addresses.AddNew();
			SetupAddress(postalAddress2, "Postal Address Chinese", OrgConstants.AddressType.Postal, true, Constants.Languages.ChineseSimplified);
			OrgAddress officeAddress1 = header.Addresses.AddNew();
			SetupAddress(officeAddress1, "Office Address Chinese", OrgConstants.AddressType.Office, true, Constants.Languages.ChineseSimplified);
			Factory.Save();
			OrgWrapper = OrgHeaderSource.New(header, Factory);

			OrgHeader loginOrgProxy = Factory.Load<OrgHeader>(Env.CurrentCompany.OrganisationPK);
			AssertEquals("Current login company language is EN", Constants.Languages.English, loginOrgProxy.OH_Language);
			AssertEquals("ARAddress should use the English receivable address", "English Receivable Address", OrgWrapper.ARAddress.Address1);

			header.OH_Language = Constants.Languages.ChineseSimplified;
			loginOrgProxy.OH_Language = Constants.Languages.ChineseSimplified;
			Factory.Save();
			OrgWrapper = OrgHeaderSource.New(header, Factory);
			AssertEquals("ARAddress should use the Chinese receivable address", "Chinese Receivable Address", OrgWrapper.ARAddress.Address1);

			header.OH_Language = Constants.Languages.ChineseSimplified;
			loginOrgProxy.OH_Language = Constants.Languages.English;
			Factory.Save();
			AssertEquals("ARAddress should use the English receivable address", "English Receivable Address", OrgWrapper.ARAddress.Address1);

			header.OH_Language = Constants.Languages.ChineseSimplified;
			loginOrgProxy.OH_Language = Constants.Languages.ChineseSimplified;
			header.Addresses.Remove(receivableAddress2);
			receivableAddress2.Delete();
			Factory.Save();
			AssertEquals("ARAddress should use the English receivable address when no Chinese address is found", "English Receivable Address", OrgWrapper.ARAddress.Address1);

			header.Addresses.Remove(receivableAddress1);
			receivableAddress1.Delete();
			Factory.Save();
			AssertEquals("ARAddress should use the Chinese postal address when no receivable address is found", "Postal Address Chinese", OrgWrapper.ARAddress.Address1);

			header.Addresses.Remove(postalAddress1);
			header.Addresses.Remove(postalAddress2);
			postalAddress1.Delete();
			postalAddress2.Delete();
			Factory.Save();
			AssertEquals("ARAddress should use the Chinese office address when no receivable or postal address is found ", "Office Address Chinese", OrgWrapper.ARAddress.Address1);

			header.Addresses.Remove(officeAddress1);
			officeAddress1.Delete();
			Factory.Save();
			AssertEquals("ARAddress should use the main address when no receivable or postal address is found ", "Main Office Address", OrgWrapper.ARAddress.Address1);
		}

		void SetupAddress(OrgAddress address, string description, string addressCapability, bool isMain, string language = Constants.Languages.English)
		{
			address.OA_Address1 = description;
			address.OA_Address2 = description + " 2";
			address.OA_City = "Org City";
			address.OA_Language = language;
			address.AddressCapability.SetCapabilityEnabled(addressCapability);

			if (isMain)
			{
				address.AddressCapability.SetIsMainAddress(addressCapability);
			}
			else
			{
				address.AddressCapability.SetIsNotMainAddress(addressCapability);
			}
		}

		public void TestPostalAddress()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertEquals("AUSTRALIA", OrgWrapper.PostalAddress);
			AssertEquals("", OrgWrapper.PostalAddressExcludeCountryIfSame);
			AssertEquals("AUSTRALIA", OrgWrapper.PostalAddressExcludeName);
			Header.OH_FullName = "My Company";
			Header.MainAddress.OA_Address1 = "12A TOWER BUILDING";
			Header.MainAddress.OA_Address2 = "HIGH TOWER";
			Header.MainAddress.OA_City = "Star City";
			Header.MainAddress.OA_PostCode = "1111";
			Header.OH_RL_NKClosestPort = "AUSYD";
			Header.MainAddress.OA_State = "NSW";
			AssertEquals("MY COMPANY\n12A TOWER BUILDING\nHIGH TOWER\nSTAR CITY NSW 1111\nAUSTRALIA", OrgWrapper.PostalAddress);
			AssertEquals("12A TOWER BUILDING\nHIGH TOWER\nSTAR CITY NSW 1111\nAUSTRALIA", OrgWrapper.PostalAddressExcludeName);
		}

		public void TestPostalAddressInEnglish()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var staffDE = Factory.New<GlbStaff>();
			staffDE.GS_Code = "ABC";
			staffDE.GS_LoginName = "Dieter";
			staffDE.GS_FullName = "Dieter";
			staffDE.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			Factory.Save();

			using (Env.Instance.SetTemporaryUserContext(staffDE.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("AUSTRALIA", OrgWrapper.PostalAddressInEnglish);

				Header.OH_FullName = "My Company";
				Header.MainAddress.OA_Address1 = "12A TOWER BUILDING";
				Header.MainAddress.OA_Address2 = "HIGH TOWER";
				Header.MainAddress.OA_City = "Star City";
				Header.MainAddress.OA_PostCode = "1111";
				Header.OH_RL_NKClosestPort = "DEHAM";
				AssertEquals("PostalAddress", "MY COMPANY\n12A TOWER BUILDING\nHIGH TOWER\n1111 STAR CITY\nDEUTSCHLAND", OrgWrapper.PostalAddress);
				AssertEquals("PostalAddressInEnglish", "MY COMPANY\n12A TOWER BUILDING\nHIGH TOWER\n1111 STAR CITY\nGERMANY", OrgWrapper.PostalAddressInEnglish);
			}
		}

		#endregion

		#region Invoices
		public void TestDisbursementTerms()
		{
			var newOrg = Factory.New<OrgHeader>();
			OrgHeaderSource oRGWrapper = OrgHeaderSource.New(newOrg, Factory);

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
			OrgHeaderSource oRGWrapper = OrgHeaderSource.New(newOrg, Factory);

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
			OrgHeaderSource oRGWrapper = OrgHeaderSource.New(newOrg, Factory);

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

			OrgHeaderSource oRGWrapper = OrgHeaderSource.New(newOrg, Factory);
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

		#endregion

		#region Basic Organisation Details
		public void TestCountry()
		{
			var lOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Header.MainAddress.OA_RL_NKRelatedPortCode = lOCO.Code;
			AssertEquals(lOCO.Country.RN_Desc, OrgWrapper.Country.Name);
		}

		public void TestContacts()
		{
			AssertEquals(0, OrgWrapper.Contacts.Count);

			Header.Contacts.AddNew().OC_ContactName = "C1";
			Header.Contacts.AddNew().OC_ContactName = "C2";

			OrgContact systemGeneratedContact = Header.Contacts.AddNew();
			systemGeneratedContact.OC_ContactName = "C3";
			systemGeneratedContact.OC_SystemCreateUser = "~BP";

			Header.OH_Code = "TST";
			Header.Factory.Save();

			Env.Registry.ShowSystemGeneratedContactsOnOrgDocuments = false;
			AssertEquals(2, OrgWrapper.Contacts.Count);

			Env.Registry.ShowSystemGeneratedContactsOnOrgDocuments = true;
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

		public void TestAppointedAgentPorts()
		{
			OrgAppointedAgentPorts agPorts = Header.AppointedAgentPorts.AddNew();
			agPorts.O5_AirAgentStatus = "APP";
			AssertEquals("APP", OrgWrapper.AppointedAgentPorts[0].AirAgentStatus);
		}

		public void TestExportFreightContact()
		{
			var org1 = Factory.New<OrgHeader>();
			OrgContact contact1 = org1.Contacts.AddNew();
			contact1.Documents.AddNew().OD_DocumentGroup = ContactType.ExportFreightAgent.Code;
			contact1.OC_ContactName = "Mr Export Freight Agent";
			OrgHeaderSource docOrg1 = OrgHeaderSource.New(org1, Factory);
			AssertEquals("Expecting Org to have ExportFreight Contact", contact1.OC_ContactName, docOrg1.ExportFowardingContact.ContactName);

			var org2 = Factory.New<OrgHeader>();
			OrgContact contact2 = org2.Contacts.AddNew();
			contact2.Documents.AddNew().OD_DocumentGroup = ContactType.All.Code;
			contact2.OC_ContactName = "ALL Contact";
			OrgHeaderSource docOrg2 = OrgHeaderSource.New(org2, Factory);
			AssertEquals("Expecting Org to have the All Contact", contact2.OC_ContactName, docOrg2.ExportFowardingContact.ContactName);

			var org3 = Factory.New<OrgHeader>();
			OrgHeaderSource docOrg3 = OrgHeaderSource.New(org3, Factory);
			AssertEquals("Expecting Org to not have an empty contact", true, !docOrg3.ExportFowardingContact.ContactName.IsEmpty);
		}

		public void TestImportFreightContact()
		{
			var org1 = Factory.New<OrgHeader>();
			OrgContact contact1 = org1.Contacts.AddNew();
			contact1.Documents.AddNew().OD_DocumentGroup = ContactType.ImportFreightAgent.Code;
			contact1.OC_ContactName = "Mr Import Freight Agent";
			OrgHeaderSource docOrg1 = OrgHeaderSource.New(org1, Factory);
			AssertEquals("Expecting Org to have Import Freight Contact", contact1.OC_ContactName, docOrg1.ImportFowardingContact.ContactName);

			var org2 = Factory.New<OrgHeader>();
			OrgContact contact2 = org2.Contacts.AddNew();
			contact2.Documents.AddNew().OD_DocumentGroup = ContactType.All.Code;
			contact2.OC_ContactName = "ALL Contact";
			OrgHeaderSource docOrg2 = OrgHeaderSource.New(org2, Factory);
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

		public void TestBusinessRegType()
		{
			Header.PrimaryRegistrationNumber.NumberTypeForDisplay = "ABN";
			AssertEquals("ABN", OrgWrapper.BusinessRegType);
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

		public void TestIsUserFlags()
		{
			var userFlagColumnPrefixName = OrgHeaderSchema.Constants.OH_IsUserFlag1.Remove(OrgHeaderSchema.Constants.OH_IsUserFlag1.Length - 1);
			var userFlagColumns = OrgHeaderSchema.All.Cast<SchemaColumn>().Where(col => col.Name.StartsWith(userFlagColumnPrefixName));

			foreach (var userFlagColumn in userFlagColumns)
			{
				var index = int.Parse(userFlagColumn.Name.Substring(userFlagColumnPrefixName.Length));
				var exptedUserFlagPropertyName = "IsUserFlag" + index;
				var userFlagProperty = OrgWrapper.GetType().GetProperty(exptedUserFlagPropertyName);

				AssertNotNull("There should be a property named '" + exptedUserFlagPropertyName + "' for column:" + userFlagColumn.Name, userFlagProperty);

				AssertEquals(exptedUserFlagPropertyName, ZBool.False, userFlagProperty.GetValue(OrgWrapper));
				Header[userFlagColumn] = ZBool.True;
				AssertEquals(exptedUserFlagPropertyName, ZBool.True, userFlagProperty.GetValue(OrgWrapper));
			}
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
			Header.MainAddress.OA_Phone = "08 83838383";
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
			OrgWrapper = OrgHeaderSource.New(header, Factory);
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
			AssertEquals("Sea Delivery Instructions", "Header Delivery Instructions", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode(Constants.TransportModes.Sea, ""));
			AssertEquals("Air Delivery Instructions", "Header Delivery Instructions", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode(Constants.TransportModes.Air, ""));

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Air Pickup Instructions", "Header Pickup Instructions", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode(Constants.TransportModes.Air, ""));
			AssertEquals("Sea Pickup Instructions", "Header Pickup Instructions", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode(Constants.TransportModes.Sea, ""));
		}

		public void TestGetCartageInstructionsByContainerMode()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			StmNoteContexts stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.L;
			FreightHelperClass.AddNote(Header, pickupDesc, "Header Pickup Instructions LCL", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, deliveryDesc, "Header Delivery Instructions FCL", stmNoteContext);

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("FCL Cartage Delivery Instructions", "Header Delivery Instructions FCL", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("AIR", "FCL"));
			AssertEquals("LSE Cartage - no match - No Cartage Delivery Instructions", "", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("AIR", "LSE"));
			AssertEquals("ALL Cartage Delivery Instructions", "Header Delivery Instructions FCL", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("AIR", "ALL"));

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("LCL Cartage Pickup Instructions", "Header Pickup Instructions LCL", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("AIR", "LCL"));
			AssertEquals("LSE Cartage  - no match - No Cartage Pickup Instructions", "", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("AIR", "LSE"));
			AssertEquals("All Cartage Instructions", "Header Pickup Instructions LCL", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("AIR", "ALL"));

			stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.I;
			FreightHelperClass.AddNote(Header, pickupDesc, "GHI Pickup - Air context", stmNoteContext);
			FreightHelperClass.AddNote(Header, deliveryDesc, "GHI Delivery - Air context", stmNoteContext);

			stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.S;
			FreightHelperClass.AddNote(Header, pickupDesc, "GHI Pickup - Sea context", stmNoteContext);
			FreightHelperClass.AddNote(Header, deliveryDesc, "GHI Delivery - Sea context", stmNoteContext);

			stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.A;
			FreightHelperClass.AddNote(Header, pickupDesc, "GHI Pickup - ALL contexts", stmNoteContext);
			FreightHelperClass.AddNote(Header, deliveryDesc, "GHI Delivery - ALL contexts", stmNoteContext);

			AssertEquals("AIR Cartage Instructions", "GHI Pickup - Air context\nGHI Pickup - ALL contexts", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("AIR", "LCL"));
			AssertEquals("AIR Cartage Instructions", "GHI Pickup - Sea context\nGHI Pickup - ALL contexts", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("SEA", "LCL"));
			AssertEquals("AIR Cartage Instructions", "Header Pickup Instructions LCL\nGHI Pickup - Air context\nGHI Pickup - Sea context\nGHI Pickup - ALL contexts", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("ALL", "LCL"));

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("AIR Cartage Instructions", "GHI Delivery - Air context\nGHI Delivery - ALL contexts", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("AIR", "LCL"));
			AssertEquals("AIR Cartage Instructions", "GHI Delivery - Sea context\nGHI Delivery - ALL contexts", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("SEA", "LCL"));
			AssertEquals("AIR Cartage Instructions", "Header Delivery Instructions FCL\nGHI Delivery - Air context\nGHI Delivery - Sea context\nGHI Delivery - ALL contexts", OrgWrapper.GetCartageInstructionsByTransportOrContainerMode("ALL", "LCL"));
		}

		public void TestGetHandlingInstructionsByTransportMode()
		{
			string handlingDesc = PredefinedNoteTypes.Instance.HandlingInstructions.Description;

			StmNoteContexts stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.I;
			FreightHelperClass.AddNote(Header, handlingDesc, "Handling Instructions - Air", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.S;
			FreightHelperClass.AddNote(Header, handlingDesc, "Handling Instructions - Sea", stmNoteContext);

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Sea Handling Instructions", "Handling Instructions - Sea", OrgWrapper.GetHandlingInstructionsByTransportOrContainerMode(Constants.TransportModes.Sea, ""));
			AssertEquals("Air Handling Instructions", "Handling Instructions - Air", OrgWrapper.GetHandlingInstructionsByTransportOrContainerMode(Constants.TransportModes.Air, ""));

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Air Handling Instructions", "Handling Instructions - Air", OrgWrapper.GetHandlingInstructionsByTransportOrContainerMode(Constants.TransportModes.Air, ""));
			AssertEquals("Sea Handling Instructions", "Handling Instructions - Sea", OrgWrapper.GetHandlingInstructionsByTransportOrContainerMode(Constants.TransportModes.Sea, ""));
		}

		public void TestGetHandlingInstructionsByContainerMode()
		{
			string handlingDesc = PredefinedNoteTypes.Instance.HandlingInstructions.Description;

			StmNoteContexts stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.L;
			FreightHelperClass.AddNote(Header, handlingDesc, "Handling Instructions LCL", stmNoteContext);
			stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.F;
			FreightHelperClass.AddNote(Header, handlingDesc, "Handling Instructions FCL", stmNoteContext);

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("FCL Handling Instructions", "Handling Instructions FCL", OrgWrapper.GetHandlingInstructionsByTransportOrContainerMode("AIR", "FCL"));
			AssertEquals("No match - No Handling Instructions", "", OrgWrapper.GetHandlingInstructionsByTransportOrContainerMode("AIR", "LSE"));

			OrgWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("LCL Handling Instructions", "Handling Instructions LCL", OrgWrapper.GetHandlingInstructionsByTransportOrContainerMode("AIR", "LCL"));
			AssertEquals("No match - No Handling Instructions", "", OrgWrapper.GetHandlingInstructionsByTransportOrContainerMode("AIR", "LSE"));

			stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.I;
			FreightHelperClass.AddNote(Header, handlingDesc, "GHI - Air context", stmNoteContext);

			stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.S;
			FreightHelperClass.AddNote(Header, handlingDesc, "GHI - Sea context", stmNoteContext);

			stmNoteContext = new StmNoteContexts();
			stmNoteContext.FreightMode |= StmNoteContextFreightMode.A;
			FreightHelperClass.AddNote(Header, handlingDesc, "GHI - ALL contexts", stmNoteContext);

			AssertEquals("AIR Handling Instructions", "GHI - Air context\nGHI - ALL contexts", OrgWrapper.GetHandlingInstructionsByTransportOrContainerMode("AIR", "LCL"));
			AssertEquals("AIR Handling Instructions", "GHI - Sea context\nGHI - ALL contexts", OrgWrapper.GetHandlingInstructionsByTransportOrContainerMode("SEA", "LCL"));
			AssertEquals("AIR Handling Instructions", "Handling Instructions LCL\nHandling Instructions FCL\nGHI - Air context\nGHI - Sea context\nGHI - ALL contexts", OrgWrapper.GetHandlingInstructionsByTransportOrContainerMode("ALL", "LCL"));
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

		public void TestGetSpecialInstructionsByTransportOrContainerMode()
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

		public void TestStaffAssignments()
		{
			Header.StaffAssignments.RemoveAndDeleteAll();
			OrgStaffAssignments item = Header.StaffAssignments.AddNew();
			item.O8_Department = "SEA";
			AssertEquals("Count", Header.StaffAssignments.Count, OrgWrapper.StaffAssignments.Count);
			AssertEquals("SEA", OrgWrapper.StaffAssignments[0].O8_Department);
		}

		#endregion

		#region Implementation
		protected OrgHeader Header;
		protected OrgHeaderSource OrgWrapper;
		protected override void SetUp()
		{
			Header = Factory.New<OrgHeader>();
			Header.OH_Code = "UXV";
			OrgWrapper = (OrgHeaderSource)GetDocumentWrappers()[0];
			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			base.SetUp();
		}

		#endregion
	}
}
