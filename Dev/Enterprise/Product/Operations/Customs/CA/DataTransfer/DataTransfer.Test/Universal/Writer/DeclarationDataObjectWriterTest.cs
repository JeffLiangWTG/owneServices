using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;
using TransportTypeList = Enterprise.Customs.CA.Business.TransportTypeList;
using YesNoList = Enterprise.Customs.Business.YesNoList;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectWriterTest
		: OrganizationAddressTestHelper
	{
		public void TestOnlyPopulateAddInfoFieldsUsedInCusEntryLine()
		{
			var notifier = new SendsMessagesToCustomsShutterUpperer();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			line.JI_FormattedTariff = "7326909031";
			line.CA_99TariffCode = "0123";
			line.CA_AuthorityNumber = "1234";
			line.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsComputedValue;
			line.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;

			declaration.DoMerge(notifier);
			var b2Declaration = declaration.GetNewCopyToB2Declaration();
			b2Declaration.DoMerge(notifier);
			Factory.SaveForTesting();
			var entryLines = b2Declaration.B3EntryHeader.AllEntryLines;
			AssertEquals("Merged to 1 entry lines", 1, entryLines.Count);
			var entryLine = entryLines.First();
			AssertEquals("1", entryLine.CA_B2SubHeader.ToString());
			AssertEquals("1", entryLine.CA_B2LineNo.ToString());
			entryLine.CA_B2LineNo = "2";
			Factory.SaveForTesting();

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, b2Declaration);

			var b3EntryData = declarationData.EntryHeaderCollection.FirstOrDefault(x => x.Type.Code.Value == MessageTypeList.Codes.B3CUSDEC);
			AssertEquals(1, b3EntryData.EntryLineCollection.Count);

			var entryLineAddInfoData = b3EntryData.EntryLineCollection.First().AddInfoCollection;
			AssertEquals("Only 2 addinfo data populated", 2, entryLineAddInfoData.Count);
			AssertEquals("2", entryLineAddInfoData.FirstOrDefault(x => x.Key.Value == "B2LineNo").Value);
			AssertEquals("1", entryLineAddInfoData.FirstOrDefault(x => x.Key.Value == "B2SubHeader").Value);
		}

		public void TestImportDeclarationMappings()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var vendorOrg = Factory.New<OrgHeader>();
			vendorOrg.OH_Code = "IANVENDOR";
			declaration.JE_OH_Supplier = vendorOrg.PK;

			var importerOrg = Factory.New<OrgHeader>();
			importerOrg.OH_Code = "IANIMPORTER";
			declaration.JE_OH_Importer = importerOrg.PK;

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			declaration.JE_MasterBill = "08112346541";
			declaration.JE_HouseBill = "734646544";
			declaration.JE_WarehouseReleaseDate = new ZDateTime(2015, 9, 28, 0, 33, 0);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2015, 9, 29, 1, 30, 0);
			declaration.CA_EstReleaseDate = new ZDateTime(2015, 9, 30);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2015, 10, 1);
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "20000001";

			declaration.AdditionalReferenceNumbers.AddNewIfNotExist(CanadaAdditionalReferenceNumberTypes.Codes.CCN, "80367346464544");
			declaration.AdditionalReferenceNumbers.AddNewIfNotExist(CanadaAdditionalReferenceNumberTypes.Codes.PCN, "801666665555");
			declaration.CargoControlNumbers.AddNew("80367346464544B");

			Factory.SaveForTesting();

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);

			CombineAssertions(delegate
			{
				AssertEquals("Sub-location ETD", declaration.JE_WarehouseReleaseDate, declarationData.DateCollection.FirstOrDefault(x => x.Type == DateType.WarehouseRelease).Value);
				AssertEquals("Actual Release Date", declaration.JE_EntryAuthorisationDate, declarationData.DateCollection.FirstOrDefault(x => x.Type == DateType.EntryAuthorisation).Value);

				AssertEquals(2, declarationData.AddInfoGroupCollection.Count);
				AssertEquals("80367346464544", declarationData.AddInfoGroupCollection[0].AddInfoCollection.First().Value);
				AssertEquals("80367346464544B", declarationData.AddInfoGroupCollection[1].AddInfoCollection.First().Value);
			});
		}

		public void TestB2AdjustmentsMappings()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;

			var mailToOrg = Factory.New<OrgHeader>();
			mailToOrg.OH_Code = "INCMAILTO";
			declaration.JE_OH_NotifyParty = mailToOrg.PK;

			declaration.CA_B2Type = B2TypeList.Codes.Specific;
			declaration.CA_OriginalTransactionNo = "55555000000063";
			declaration.JE_CustomsOffice = "0497";
			declaration.CA_K84AccountingDate = new ZDateTime(2015, 09, 29);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2015, 10, 04);
			declaration.CA_SecurityNo = "SECURITY NO";

			Factory.SaveForTesting();

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);

			CombineAssertions(delegate
			{
				AssertEquals("Release Date", declaration.JE_EntryAuthorisationDate, declarationData.DateCollection.FirstOrDefault(x => x.Type == DateType.EntryAuthorisation).Value);

				var organizationCollection = declarationData.OrganizationAddressCollection;
				AssertEquals("Mail To", "INCMAILTO", organizationCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == Constants.AddressType.MailTo).OrganizationCode);
			});
		}

		public void TestPopulateDeclarationAddInfo()
		{
			CACustomsDataRegistry.Instance.FrenchLanguageIndicator.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			Factory.SaveForTesting();

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			CombineAssertions(delegate
			{
				AssertEquals("Y", declarationData.AddInfoCollection.First(x => x.Key.Value == Constants.AddInfoKeys.FrenchPreferred).Value);
			});
		}

		public void TestPopulateDeclarationAddInfoFromRealField()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "0809";
			declaration.JE_LocationOfGoods = "3368";
			((AddInfoJobDeclaration)((IAddInfoManager)declaration).AddInfo).CA_PortOfClearance = "0440";

			Factory.SaveForTesting();

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);

			CombineAssertions(delegate
			{
				AssertEquals("0809", declarationData.CustomsOffice.Code);
				AssertEquals("3368", declarationData.LocationAtClearance.Code);
				AssertEquals("0809", declarationData.AddInfoCollection.First(x => x.Key.Value == Constants.AddInfoKeys.Declaration.PortOfClearance).Value);
				AssertEquals("3368", declarationData.AddInfoCollection.First(x => x.Key.Value == Constants.AddInfoKeys.Declaration.SubLocationCode).Value);
			});
		}

		public void TestPopulateCustomsAddressOfRecord()
		{
			var importer = CreateOrganisation("Importer", "IMP");
			var addr1 = importer.Addresses.AddNew();
			addr1.OA_Address1 = "importer address1";
			addr1.OA_CompanyNameOverride = "Importer1";
			addr1.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

			var importerOfRecord = CreateOrganisation("Importer of Record", "IOR");
			var addr2 = importerOfRecord.Addresses.AddNew();
			addr2.OA_Address1 = "ior address1";
			addr2.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;

			Factory.SaveForTesting();

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			var importerDocumentaryAddress = declarationData.OrganizationAddressCollection.FirstOrDefault(o => o.AddressType.GetValueOrDefault() == "ImporterDocumentaryAddress");
			AssertNotNull(importerDocumentaryAddress);
			AssertEquals("IMP", importerDocumentaryAddress.OrganizationCode.GetValueOrDefault());
			AssertEquals("importer address1", importerDocumentaryAddress.Address1.GetValueOrDefault());
			AssertEquals("Importer1", importerDocumentaryAddress.CompanyName.GetValueOrDefault());

			var importerOfRecordAddress = declarationData.OrganizationAddressCollection.FirstOrDefault(o => o.AddressType.GetValueOrDefault() == "ImporterOfRecord");
			AssertNotNull(importerOfRecordAddress);
			AssertEquals("IOR", importerOfRecordAddress.OrganizationCode.GetValueOrDefault());
			AssertEquals("Importer of Record", importerOfRecordAddress.CompanyName.GetValueOrDefault());
			AssertEquals("ior address1", importerOfRecordAddress.Address1.GetValueOrDefault());
		}

		public void TestPopulateCFIAAccountOwner()
		{
			var country = RefCountry.LoadFromCountryCode(Factory.BOFactory, Core.Constants.CountryCodes.Canada);

			var branchProxyOrg = CreateOrganisation("Branch", "ABC#@1");
			branchProxyOrg.SetCustomsCode("CFI", country, "123");

			var companyProxyOrg = CreateOrganisation("Company", "ABC#@2");
			companyProxyOrg.SetCustomsCode("CFI", country, "456");

			var importerOrg = CreateOrganisation("Importer", "ABC#@3");
			importerOrg.SetCustomsCode("CFI", country, "789");

			var importerAddInfo = OrgImpAddInfo.Get(importerOrg);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importerOrg.PK;

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CM1";

			var branch = company.Branches.AddNew();
			branch.GB_Code = "BR1";

			declaration.JE_GB = branch.PK;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "20000001";

			Factory.SaveForTesting();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;

			importerAddInfo.ZO_CFIAFeePaymentMethod = "IMP";
			CombineAssertions(delegate
			{
				var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
				var organizationAddress = declarationData.OrganizationAddressCollection.FirstOrDefault(o => o.AddressType.GetValueOrDefault() == Constants.AddressType.CFIAAccountOwner);
				AssertNotNull(organizationAddress);
				AssertEquals("ABC#@3", organizationAddress.OrganizationCode.GetValueOrDefault());
				var registrationNumber = organizationAddress.RegistrationNumberCollection.FirstOrDefault(o => o.Type.Code.GetValueOrDefault() == OrgCusCode.CACodeTypes.CFIAAccountNumber);
				AssertNotNull(registrationNumber);
				AssertEquals("789", registrationNumber.Value);
			});

			importerAddInfo.ZO_CFIAFeePaymentMethod = "BKR";
			company.GC_OH_OrgProxy = companyProxyOrg.PK;

			CombineAssertions(delegate
			{
				var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
				var organizationAddress = declarationData.OrganizationAddressCollection.FirstOrDefault(o => o.AddressType.GetValueOrDefault() == Constants.AddressType.CFIAAccountOwner);
				AssertNotNull(organizationAddress);
				AssertEquals("ABC#@2", organizationAddress.OrganizationCode.GetValueOrDefault());
				var registrationNumber = organizationAddress.RegistrationNumberCollection.FirstOrDefault(o => o.Type.Code.GetValueOrDefault() == OrgCusCode.CACodeTypes.CFIAAccountNumber);
				AssertNotNull(registrationNumber);
				AssertEquals("456", registrationNumber.Value);
			});

			branch.GB_OH_OrgProxy = branchProxyOrg.PK;

			CombineAssertions(delegate
			{
				var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
				var organizationAddress = declarationData.OrganizationAddressCollection.FirstOrDefault(o => o.AddressType.GetValueOrDefault() == Constants.AddressType.CFIAAccountOwner);
				AssertNotNull(organizationAddress);
				AssertEquals("ABC#@1", organizationAddress.OrganizationCode.GetValueOrDefault());
				var registrationNumber = organizationAddress.RegistrationNumberCollection.FirstOrDefault(o => o.Type.Code.GetValueOrDefault() == OrgCusCode.CACodeTypes.CFIAAccountNumber);
				AssertNotNull(registrationNumber);
				AssertEquals("123", registrationNumber.Value);
			});
		}

		public void TestPopulateCustomsBrokerByUniversalXmlWriter()
		{
			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.MainAddress.Address1 = "Broker Address1";
			broker.MainAddress.Address2 = "Broker Address2";
			broker.MainAddress.OA_Email = "broker@mail.com";
			broker.MainAddress.OA_RL_NKRelatedPortCode = "SGSIN";

			broker.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "20171207", Core.Constants.CountryCodes.Canada);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CM1";
			company.GC_OH_OrgProxy = broker.PK;

			var branch = company.Branches.AddNew();
			branch.GB_Code = "BR1";
			branch.GB_RL_NKHomePort = "CAAAB";
			branch.GB_Address1 = "testAdd1";

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ST1";
			staff.GS_FullName = "staffname";
			staff.GS_EmailAddress = "staff@mail.com";

			declaration.JE_GB = branch.PK;
			declaration.JE_GS_NKCusAgent = staff.GS_Code;

			using (CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "54321"))
			{
				Factory.SaveForTesting();

				var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);

				CombineAssertions(delegate
				{
					var organizationAddress = declarationData.OrganizationAddressCollection.First(o => o.AddressType.GetValueOrDefault() == Constants.AddressType.CustomsBroker);

					AssertEquals("Port", "SGSIN", organizationAddress.Port.Code);
					AssertEquals("Address1", "Broker Address1", organizationAddress.Address1);
					AssertEquals("Contact", "staffname", organizationAddress.Contact);
					AssertEquals("Email", "staff@mail.com", organizationAddress.Email);

					var registrationNumbers = organizationAddress.RegistrationNumberCollection;
					AssertEquals(2, registrationNumbers.Count);

					var ascNumber = registrationNumbers.First(c => c.Type.Code.HasValue && c.Type.Code.Value == OrgCusCode.CACodeTypes.AccountSecurityCode);
					AssertEquals("ASC Type", "ASC", ascNumber.Type.Code);
					AssertEquals("ASC CountryOfIssue", "CA", ascNumber.CountryOfIssue.Code);
					AssertEquals("ASC Value", "54321", ascNumber.Value);

					var brmNumber = registrationNumbers.First(c => c.Type.Code.HasValue && c.Type.Code.Value == OrgCusCode.CACodeTypes.BusinessNumberForImportExport);
					AssertEquals("BRM Type", "BRM", brmNumber.Type.Code);
					AssertEquals("BRM CountryOfIssue", "CA", brmNumber.CountryOfIssue.Code);
					AssertEquals("BRM Value", "20171207", brmNumber.Value);
				});

				broker.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker, "20171208", Core.Constants.CountryCodes.Canada);
				declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
				CombineAssertions(delegate
				{
					var organizationAddress = declarationData.OrganizationAddressCollection.First(o => o.AddressType.GetValueOrDefault() == Constants.AddressType.CustomsBroker);
					var registrationNumbers = organizationAddress.RegistrationNumberCollection;
					AssertEquals(2, registrationNumbers.Count);

					var brmNumber = registrationNumbers.First(c => c.Type.Code.HasValue && c.Type.Code.Value == OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker);
					AssertEquals("BRB Type", "BRB", brmNumber.Type.Code);
					AssertEquals("BRB CountryOfIssue", "CA", brmNumber.CountryOfIssue.Code);
					AssertEquals("BRB Value", "20171208", brmNumber.Value);
				});
			}
		}

		public void TestPopulateExamLocationName()
		{
			var otherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(otherFactory);
			helper.CreateCusCodeType("SUBLC", "Sub Location Codes");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "SUBLC", "1111", "1111 desc", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "SUBLC", "2222", "2222 desc", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			otherFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ExamLocationCode = "1111";

			Factory.SaveForTesting();

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertEquals("ExamLocationName is exported", "1111 desc", declarationData.AddInfoCollection.First(x => x.Key.Value == Constants.AddInfoKeys.Declaration.ExamLocationName).Value);
		}

		public void TestPopulatePARSETA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2018, 12, 5);

			Factory.SaveForTesting();

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertEquals("PARSETA is exported", "2018-12-05 00:00:00.000", declarationData.AddInfoCollection.First(x => x.Key.Value == Constants.AddInfoKeys.Declaration.PARSETA).Value);
		}

		public void TestPopulateAllocatedContacts()
		{
			var importerOrg = CreateOrganisation("Importer", "ABC#@1");
			importerOrg.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "20171207", Core.Constants.CountryCodes.Canada);
			var importerContact = CreateContactForOrganization(importerOrg, "contact1");
			var importerAlloc = importerContact.Allocations.AddNew();
			importerAlloc.PC_Type = OrgConstants.ContactAllocationType.CAPGA;
			var shipToPartyOrg = CreateOrganisation("ShipToParty", "ABC#@2");
			var shipToPartyContact = CreateContactForOrganization(shipToPartyOrg, "contact2");
			var shipToPartyAlloc = shipToPartyContact.Allocations.AddNew();
			shipToPartyAlloc.PC_Type = OrgConstants.ContactAllocationType.CAPGA;
			var manufacturerOrg = CreateOrganisation("Manufacturer", "ABC#@3");
			var manufacturerContact = CreateContactForOrganization(manufacturerOrg, "contact3");
			var manufacturerAlloc = manufacturerContact.Allocations.AddNew();
			manufacturerAlloc.PC_Type = OrgConstants.ContactAllocationType.CAPGA;
			var harvestingPartyOrg = CreateOrganisation("HarvestingParty", "ABC#@4");
			var harvestingPartyContact = CreateContactForOrganization(harvestingPartyOrg, "contact4");
			var harvestingPartyAlloc = harvestingPartyContact.Allocations.AddNew();
			harvestingPartyAlloc.PC_Type = OrgConstants.ContactAllocationType.CAPGA;
			var processorOrg = CreateOrganisation("Processor", "ABC#@5");
			var processorContact = CreateContactForOrganization(processorOrg, "contact5");
			processorContact.OC_IsActive = false;
			var processorAlloc = processorContact.Allocations.AddNew();
			processorAlloc.PC_Type = OrgConstants.ContactAllocationType.CAPGA;
			var cA = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CA");
			importerOrg.SetCustomsCode("CFI", cA, "123");
			importerOrg.MainAddress.OA_Email = "importer@wisetechglobal.com";
			var importerAddInfo = OrgImpAddInfo.Get(importerOrg);
			importerAddInfo.ZO_CFIAFeePaymentMethod = "IMP";

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CM1";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BR1";
			branch.GB_OH_OrgProxy = importerOrg.PK;
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ST1";
			staff.GS_FullName = "staffname";
			staff.GS_EmailAddress = "staff@mail.com";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importerOrg.PK;
			declaration.JE_GB = branch.PK;
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "20000001";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_ShipToPartyAddress = shipToPartyOrg.MainAddress.PK;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_OA_ManufacturerAddress = manufacturerOrg.MainAddress.PK;
			invoiceLine.CA_DFOInd = YesNoList.Codes.Yes;
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;
			var dfoPGAHeader = invoiceLine.DFOPGAHeader;
			dfoPGAHeader.CA_OA_HarvestingParty = harvestingPartyOrg.MainAddress.PK;
			dfoPGAHeader.CA_OA_Processor = processorOrg.MainAddress.PK;
			Factory.SaveForTesting();

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertOrganizationAddressUsingAllocatedContact(declarationData, false);

			var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
			AssertNotNull(manager);
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration))) as DeclarationDataObjectWriter;
			AssertNotNull(writer);
			writer.IsExportingForIIDMessaging = true;
			using (((IExternalFetchHintSupporter)declaration.Factory).SetupCreator())
			{
				declarationData = writer.GetDataObject(declaration);
			}

			AssertOrganizationAddressUsingAllocatedContact(declarationData, true);
			AssertOrganizationAddressCollectionContains(declarationData.OrganizationAddressCollection, Constants.AddressType.CFIAAccountOwner, "importer@wisetechglobal.com", false);
			importerAddInfo.ZO_CFIAFeePaymentMethod = "BKR";
			using (((IExternalFetchHintSupporter)declaration.Factory).SetupCreator())
			{
				declarationData = writer.GetDataObject(declaration);
			}
			AssertOrganizationAddressCollectionContains(declarationData.OrganizationAddressCollection, Constants.AddressType.CFIAAccountOwner, "importer@wisetechglobal.com", true);
		}

		public void TestPopulateCustomsBrokerWithRegistryOverride()
		{
			var company = CompanyWithProxy();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BR1";

			var declarationStaff = SetupStaff("DS1", "", "", "", "", "");
			var companyStaff = SetupStaff("CM1", "staffname", "staff@mail.com", "1234567890", "1234567890", "0987654321");
			var branchStaff = SetupStaff("BR1", "BranchPerson", "branch@mail.com", "0123845", "38472394", "3249823");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			declaration.JE_GB = branch.PK;
			declaration.JE_GS_NKCusAgent = declarationStaff.GS_Code;

			CACustomsDataRegistry.Instance.DefaultBranchPgaContact.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, companyStaff.PK.ToGuid());

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			var brokerAddress = declarationData.OrganizationAddressCollection.First(o => o.AddressType.GetValueOrDefault() == Constants.AddressType.CustomsBroker);

			AssertEquals(companyStaff.GS_FullName, brokerAddress.Contact);
			AssertEquals(companyStaff.GS_EmailAddress, brokerAddress.Email);
			AssertEquals(companyStaff.GS_FaxNum, brokerAddress.Fax);
			AssertEquals(companyStaff.GS_MobilePhone, brokerAddress.Mobile);
			AssertEquals(companyStaff.GS_WorkPhone, brokerAddress.Phone);

			CACustomsDataRegistry.Instance.DefaultBranchPgaContact.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, branchStaff.PK.ToGuid());

			declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			brokerAddress = declarationData.OrganizationAddressCollection.First(o => o.AddressType.GetValueOrDefault() == Constants.AddressType.CustomsBroker);

			AssertEquals(branchStaff.GS_FullName, brokerAddress.Contact);
			AssertEquals(branchStaff.GS_EmailAddress, brokerAddress.Email);
			AssertEquals(branchStaff.GS_FaxNum, brokerAddress.Fax);
			AssertEquals(branchStaff.GS_MobilePhone, brokerAddress.Mobile);
			AssertEquals(branchStaff.GS_WorkPhone, brokerAddress.Phone);
		}

		public void TestPopulateCustomsBrokerWithHomeBranchFaxAndPhone()
		{
			var company = CompanyWithProxy();
			var declarationBranch = company.Branches.AddNew();
			declarationBranch.GB_Code = "BR1";

			var staffHomeBranch = company.Branches.AddNew();
			staffHomeBranch.GB_Code = "BR2";
			staffHomeBranch.GB_Phone = "61280012200";
			staffHomeBranch.GB_Fax = "61280012201";

			var declarationStaff = SetupStaff("DS1", "", "", "", "", "");
			var branchStaff = SetupStaff("BR1", "BranchPerson", "branch@mail.com", ZString.Empty, "38472394", ZString.Empty);
			branchStaff.GS_GB_HomeBranch = staffHomeBranch.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			declaration.JE_GB = declarationBranch.PK;
			declaration.JE_GS_NKCusAgent = declarationStaff.GS_Code;

			CACustomsDataRegistry.Instance.DefaultBranchPgaContact.SetTemporaryValue(Guid.Empty, declarationBranch.PK.ToGuid(), Guid.Empty, branchStaff.PK.ToGuid());

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			var brokerAddress = declarationData.OrganizationAddressCollection.First(o => o.AddressType.GetValueOrDefault() == Constants.AddressType.CustomsBroker);

			AssertEquals(branchStaff.GS_FullName, brokerAddress.Contact);
			AssertEquals(branchStaff.GS_EmailAddress, brokerAddress.Email);
			AssertEquals(staffHomeBranch.GB_Fax, brokerAddress.Fax);
			AssertEquals(branchStaff.GS_MobilePhone, brokerAddress.Mobile);
			AssertEquals(staffHomeBranch.GB_Phone, brokerAddress.Phone);
		}

		public void TestPopulateCFIAForBranchFromRegistryOverrideWhenPaymentMethodIsBroker()
		{
			var company = CompanyWithProxy();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BR1";

			var declarationStaff = SetupStaff("DS1", "", "", "", "", "");
			var branchStaff = SetupStaff("BR1", "BranchPerson", "branch@mail.com", "0123845", "38472394", "3249823");

			var country = RefCountry.LoadFromCountryCode(Factory.BOFactory, Core.Constants.CountryCodes.Canada);
			var branchProxyOrg = CreateOrganisation("Branch", "ABC#@1");
			branchProxyOrg.SetCustomsCode("CFI", country, "123");
			branch.GB_OH_OrgProxy = branchProxyOrg.PK;

			var importerOrg = CreateOrganisation("Importer", "ABC#@3");
			var importerAddInfo = OrgImpAddInfo.Get(importerOrg);
			importerAddInfo.ZO_CFIAFeePaymentMethod = "BKR";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			declaration.JE_GB = branch.PK;
			declaration.JE_GS_NKCusAgent = declarationStaff.GS_Code;
			declaration.JE_OH_Importer = importerOrg.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;

			CACustomsDataRegistry.Instance.DefaultBranchPgaContact.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, branchStaff.PK.ToGuid());
			var branchDeclarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			var branchCFIAAddress = branchDeclarationData.OrganizationAddressCollection.First(o => o.AddressType.GetValueOrDefault() == Constants.AddressType.CFIAAccountOwner);
			AssertEquals(branchStaff.GS_FullName, branchCFIAAddress.Contact);
			AssertEquals(branchStaff.GS_EmailAddress, branchCFIAAddress.Email);
			AssertEquals(branchStaff.GS_FaxNum, branchCFIAAddress.Fax);
			AssertEquals(branchStaff.GS_MobilePhone, branchCFIAAddress.Mobile);
			AssertEquals(branchStaff.GS_WorkPhone, branchCFIAAddress.Phone);
		}

		public void TestPopulateCFIAForCompanyFromRegistryOverrideWhenPaymentMethodIsBroker()
		{
			var company = CompanyWithProxy();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BR1";

			var declarationStaff = SetupStaff("DS1", "", "", "", "", "");
			var companyStaff = SetupStaff("CM1", "CompanyPerson", "company@mail.com", "1234567890", "1234567890", "0987654321");

			var importerOrg = CreateOrganisation("Importer", "ABC#@3");
			var importerAddInfo = OrgImpAddInfo.Get(importerOrg);
			importerAddInfo.ZO_CFIAFeePaymentMethod = "BKR";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			declaration.JE_GB = branch.PK;
			declaration.JE_GS_NKCusAgent = declarationStaff.GS_Code;
			declaration.JE_OH_Importer = importerOrg.PK;

			var country = RefCountry.LoadFromCountryCode(Factory.BOFactory, Core.Constants.CountryCodes.Canada);
			var companyProxyOrg = CreateOrganisation("Company", "ABC#@2");
			companyProxyOrg.SetCustomsCode("CFI", country, "456");
			company.GC_OH_OrgProxy = companyProxyOrg.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;

			CACustomsDataRegistry.Instance.DefaultBranchPgaContact.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, companyStaff.PK.ToGuid());
			var companyDeclarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			var companyCFIAAddress = companyDeclarationData.OrganizationAddressCollection.First(o => o.AddressType.GetValueOrDefault() == Constants.AddressType.CFIAAccountOwner);
			AssertEquals(companyStaff.GS_FullName, companyCFIAAddress.Contact);
			AssertEquals(companyStaff.GS_EmailAddress, companyCFIAAddress.Email);
			AssertEquals(companyStaff.GS_FaxNum, companyCFIAAddress.Fax);
			AssertEquals(companyStaff.GS_MobilePhone, companyCFIAAddress.Mobile);
			AssertEquals(companyStaff.GS_WorkPhone, companyCFIAAddress.Phone);
		}

		public void TestPopulateCFIAFromRegistryOverrideWhereFaxAndPhoneFallbackToHomebranch()
		{
			var company = CompanyWithProxy();
			var declarationBranch = company.Branches.AddNew();
			declarationBranch.GB_Code = "BR1";
			var staffHomeBranch = company.Branches.AddNew();
			staffHomeBranch.GB_Code = "BR2";
			staffHomeBranch.GB_Fax = "0889322741";
			staffHomeBranch.GB_Phone = "0889322740";

			var declarationStaff = SetupStaff("DS1", "", "", "", "", "");
			var companyStaff = SetupStaff("CM1", "CompanyPerson", "company@mail.com", ZString.Empty, "1234567890", ZString.Empty);
			companyStaff.GS_GB_HomeBranch = staffHomeBranch.PK;

			var importerOrg = CreateOrganisation("Importer", "ABC#@3");
			var importerAddInfo = OrgImpAddInfo.Get(importerOrg);
			importerAddInfo.ZO_CFIAFeePaymentMethod = "BKR";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			declaration.JE_GB = declarationBranch.PK;
			declaration.JE_GS_NKCusAgent = declarationStaff.GS_Code;
			declaration.JE_OH_Importer = importerOrg.PK;

			var country = RefCountry.LoadFromCountryCode(Factory.BOFactory, Core.Constants.CountryCodes.Canada);
			var companyProxyOrg = CreateOrganisation("Company", "ABC#@2");
			companyProxyOrg.SetCustomsCode("CFI", country, "456");
			company.GC_OH_OrgProxy = companyProxyOrg.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;

			CACustomsDataRegistry.Instance.DefaultBranchPgaContact.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, companyStaff.PK.ToGuid());
			var companyDeclarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			var companyCFIAAddress = companyDeclarationData.OrganizationAddressCollection.First(o => o.AddressType.GetValueOrDefault() == Constants.AddressType.CFIAAccountOwner);
			AssertEquals(companyStaff.GS_FullName, companyCFIAAddress.Contact);
			AssertEquals(companyStaff.GS_EmailAddress, companyCFIAAddress.Email);
			AssertEquals(staffHomeBranch.GB_Fax, companyCFIAAddress.Fax);
			AssertEquals(companyStaff.GS_MobilePhone, companyCFIAAddress.Mobile);
			AssertEquals(staffHomeBranch.GB_Phone, companyCFIAAddress.Phone);
		}

		public void TestNoCFIADataInUXMLForNoCFIALines()
		{
			var company = CompanyWithProxy();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BR1";

			var declarationStaff = SetupStaff("DS1", "", "", "", "", "");
			var companyStaff = SetupStaff("CM1", "CompanyPerson", "company@mail.com", "1234567890", "1234567890", "0987654321");

			var importerOrg = CreateOrganisation("Importer", "ABC#@3");
			var importerAddInfo = OrgImpAddInfo.Get(importerOrg);
			importerAddInfo.ZO_CFIAFeePaymentMethod = "BKR";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			declaration.JE_GB = branch.PK;
			declaration.JE_GS_NKCusAgent = declarationStaff.GS_Code;
			declaration.JE_OH_Importer = importerOrg.PK;

			var country = RefCountry.LoadFromCountryCode(Factory.BOFactory, Core.Constants.CountryCodes.Canada);
			var companyProxyOrg = CreateOrganisation("Company", "ABC#@2");
			companyProxyOrg.SetCustomsCode("CFI", country, "456");
			company.GC_OH_OrgProxy = companyProxyOrg.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.No;

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			var cfiaAddressFound = declarationData.OrganizationAddressCollection.Any(o => o.AddressType.GetValueOrDefault() == Constants.AddressType.CFIAAccountOwner);
			Assert(!cfiaAddressFound);

			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;
			declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			cfiaAddressFound = declarationData.OrganizationAddressCollection.Any(o => o.AddressType.GetValueOrDefault() == Constants.AddressType.CFIAAccountOwner);
			Assert(cfiaAddressFound);
		}

		public void TestGetNewCustomsEntryHeaderDataObjectWriter()
		{
			((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			var declaration = Factory.New<JobDeclaration>();
			var writer = new DeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())));
			writer.GetDataObject(declaration);
			AssertType<CustomsEntryHeaderDataObjectWriter>("Customs entry header data object writer type", writer.GetNewCustomsEntryHeaderDataObjectWriterExposed());
		}

		void AssertOrganizationAddressCollectionContains(List<OrganizationAddress> organizationAddressCollection, ZString addressType, ZString email, ZBool doesContain)
		{
			var organizationAddress = organizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == addressType && x.Email.GetValueOrDefault() == email);
			if (doesContain)
			{
				AssertNotNull(addressType, organizationAddress);
			}
			else
			{
				AssertNull(addressType, organizationAddress);
			}
		}

		OrgContact CreateContactForOrganization(OrgHeader orgHeader, ZString contactName)
		{
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = contactName;
			contact.OC_Email = $"{contactName}@wisetechglobal.com";
			contact.OC_Fax = "123";
			contact.OC_Mobile = "456";
			contact.OC_Phone = "789";
			return contact;
		}

		void AssertOrganizationAddressUsingAllocatedContact(Shipment declarationData, ZBool isUsing)
		{
			AssertOrganizationAddressCollectionContains(declarationData.OrganizationAddressCollection, nameof(DocAddressType.ImporterDocumentaryAddress), "contact1@wisetechglobal.com", isUsing);
			AssertOrganizationAddressCollectionContains(declarationData.OrganizationAddressCollection, Constants.AddressType.CFIAAccountOwner, "contact1@wisetechglobal.com", isUsing);
			var invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection[0];
			var invoiceOrgAddressColl = invoiceData.OrganizationAddressCollection;
			AssertOrganizationAddressCollectionContains(invoiceOrgAddressColl, nameof(DocAddressType.ShipToParty), "contact2@wisetechglobal.com", isUsing);
			var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
			var invoiceLineOrgAddressColl = invoiceLineData.OrganizationAddressCollection;
			AssertOrganizationAddressCollectionContains(invoiceLineOrgAddressColl, nameof(DocAddressType.Manufacturer), "contact3@wisetechglobal.com", isUsing);
			var invoiceLineDFOPGAHeaderOrgAddressColl = invoiceLineData.AddInfoGroupCollection.FirstOrDefault(group => group.Type.Code.GetValueOrDefault() == CusAddInfoTypeAttribute.Codes.CADFOPGAHeader).OrganizationAddressCollection;
			AssertOrganizationAddressCollectionContains(invoiceLineDFOPGAHeaderOrgAddressColl, Constants.AddressType.HarvestingParty, "contact4@wisetechglobal.com", isUsing);
			AssertOrganizationAddressCollectionContains(invoiceLineDFOPGAHeaderOrgAddressColl, Constants.AddressType.FoodProcessor, "contact5@wisetechglobal.com", false);
			AssertOrganizationAddressCollectionContains(declarationData.OrganizationAddressCollection, Constants.AddressType.CustomsBroker, "staff@mail.com", true);
		}

		GlbCompany CompanyWithProxy()
		{
			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "20171207", Core.Constants.CountryCodes.Canada);

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CM1";
			company.GC_OH_OrgProxy = broker.PK;
			return company;
		}

		GlbStaff SetupStaff(ZString code, ZString name, ZString email, ZString fax, ZString mobile, ZString phone)
		{
			var user = Factory.New<GlbStaff>();
			user.GS_Code = code;
			user.GS_FullName = name;
			user.GS_EmailAddress = email;
			user.GS_FaxNum = fax;
			user.GS_MobilePhone = mobile;
			user.GS_WorkPhone = phone;
			return user;
		}

		OrgHeader CreateOrganisation(ZString name, ZString code)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = name;
			org.OH_Code = code;
			org.MainAddress.OA_Address1 = name + " ADDRESS 1";
			return org;
		}
	}

	class DeclarationDataObjectWriterForTest : DeclarationDataObjectWriter
	{
		public DeclarationDataObjectWriterForTest(IDataWritingManager manager) : base(manager)
		{
		}

		public Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectWriter GetNewCustomsEntryHeaderDataObjectWriterExposed() => GetNewCustomsEntryHeaderDataObjectWriter();
	}
}
