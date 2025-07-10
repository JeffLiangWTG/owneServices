using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.EU.EMCS.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED801ConsigneeMessageProcessor))]
	class ED801ConsigneeMessageProcessorTest : MessageProcessorAbstractTest<ED801ConsigneeMessageProcessor, EmcsInboundEDIMessage<IED801>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IED801)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertEquals(declaration, message.EM_LinkedObject);
		}

		public void TestCreateANewDeclaration()
		{
			declaration.Delete();
			Factory.Save();
			ProcessMessage(message);
			message.Reload();
			AssertEquals("MRN98761234", ((EMCSJobDeclaration)message.EM_LinkedObject).EADNumber);
		}

		public void TestStatus()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.REG, declaration.JE_EntryStatus);
				AssertEquals("Declaration Message Status", EDIMessage.Status.Received, declaration.JE_MessageStatus);
				AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertDocumentLinkingSubscribers(new BusinessObject[] { declaration });
			});
		}

		public void TestEadNumber()
		{
			ProcessMessage(message);
			var query = new ZQuery();
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(JobDeclaration));
			query.AddToFilter(CusEntryNumSchema.CE_ParentID, declaration.PK);
			var entryNumber = message.Factory.Load<CusEntryNumber>(query).Single();

			CombineAssertions(() =>
			{
				AssertEquals("CE_Category", CusEntryNumber.Categories.CustomsPermitClearanceNumber, entryNumber.CE_Category);
				AssertEquals("CE_EntryNum", "MRN98761234", entryNumber.CE_EntryNum);
				AssertEquals("CE_EntryLineReference", "1", entryNumber.CE_EntryLineReference);
				AssertEquals("CE_EntryLineReference", new ZDateTime(2020, 6, 11, 16, 59, 59), entryNumber.CE_IssueDate);
				AssertEquals("CE_EntryType", CusEntryNumberTypes.Standard.MovementReferenceNumber, entryNumber.CE_EntryType);
			});
		}

		public void TestJourneyTime()
		{
			ProcessMessage(message);
			AssertEquals("2D", declaration.ZG_JourneyTime);
		}

		public void TestMessageSubType()
		{
			ProcessMessage(message);
			AssertEquals("1", declaration.JE_MessageSubType);
		}
		public void TestTransportArrangement()
		{
			ProcessMessage(message);
			AssertEquals("3", declaration.ZG_TransportArrangement);
		}

		public void TestDateAtOrigin()
		{
			ProcessMessage(message);
			AssertEquals(new ZDateTime(2020, 7, 27, 16, 59, 59), declaration.JE_DateAtOrigin);
		}

		public void TestDateAtOrigin_Invalid()
		{
			declaration.JE_DateAtOrigin = new ZDateTime(2020, 7, 27, 01, 01, 01);
			dataProviderMock.Setup(m => m.DispatchTime).Returns(ZDateTime.Empty);
			ProcessMessage(message);
			AssertEquals("Reset", ZDateTime.Empty, declaration.JE_DateAtOrigin);
		}

		public void TestOriginType()
		{
			ProcessMessage(message);
			AssertEquals("1", declaration.ZG_OriginType);
		}

		public void TestOwnerRef()
		{
			ProcessMessage(message);
			AssertEquals("B000222547896254786321", declaration.JE_OwnerRef);
		}

		public void TestInvoiceNumber()
		{
			ProcessMessage(message);
			AssertEquals("1", declaration.InvoiceNumber);
		}

		public void TestInvoiceDate()
		{
			ProcessMessage(message);
			AssertEquals(new ZDateTime(2020, 7, 25), declaration.InvoiceDate);
		}

		public void TestInvoiceDate_Invalid()
		{
			declaration.InvoiceDate = new ZDateTime(2020, 7, 27, 01, 01, 01);
			dataProviderMock.Setup(m => m.InvoiceDate).Returns(new ZDate(ZDateTime.Empty));
			ProcessMessage(message);
			AssertEquals("Reset", ZDateTime.Empty, declaration.InvoiceDate);
		}

		public void TestMemberStateCode()
		{
			ProcessMessage(message);
			AssertEquals("12", declaration.ZG_CCTMSA);
		}

		public void TestMemberStateCode_Empty()
		{
			declaration.ZG_CCTMSA = "10";
			dataProviderMock.Setup(m => m.MemberStateCode).Returns(ZString.Empty);
			ProcessMessage(message);
			AssertEquals("Reset", ZString.Empty, declaration.ZG_CCTMSA);
		}

		public void TestCertOfExemption()
		{
			ProcessMessage(message);
			AssertEquals("CE001", declaration.ZG_CertOfExemption);
		}

		public void TestCertOfExemption_Empty()
		{
			declaration.ZG_CertOfExemption = "01";
			dataProviderMock.Setup(m => m.CertificateOfExemption).Returns(ZString.Empty);
			ProcessMessage(message);
			AssertEquals("ZG_CCTMSA is not changed", ZString.Empty, declaration.ZG_CertOfExemption);
		}

		public void TestGuarantorType()
		{
			ProcessMessage(message);
			AssertEquals("3", declaration.ZG_GuarantorType);
		}

		public void TestTransportMode()
		{
			ProcessMessage(message);
			AssertEquals("AIR", declaration.JE_TransportMode);
		}

		public void TestSpecialInstructions()
		{
			ProcessMessage(message);
			AssertEquals("CI001", declaration.SpecialInstructions);
		}

		public void TestCustomsOfficeOfArrival()
		{
			ProcessMessage(message);
			var customsOffice = declaration.CustomsOffices.Cast<EMCSOfficeCode>().Single(x => x.CY_Code == OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival);
			AssertEquals("Office Code", "DE000050", customsOffice.CY_Data);
		}

		public void TestOfficeOfDispatch()
		{
			var co1 = declaration.CustomsOffices.AddNew();
			co1.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDispatch;
			co1.CY_Data = "CO001";

			ProcessMessage(message);
			var officeOfDispatch = declaration.CustomsOffices.Cast<EMCSOfficeCode>().Single(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDispatch);
			AssertEquals("DIO001", officeOfDispatch.CY_Data);
		}

		public void TestOfficeOfDispatch_Empty()
		{
			var co1 = declaration.CustomsOffices.AddNew();
			co1.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDispatch;
			co1.CY_Data = "CO001";
			dataProviderMock.Setup(m => m.DispatchImportOffice).Returns(ZString.Empty);

			ProcessMessage(message);
			AssertEquals("No Office", false, declaration.CustomsOffices.Cast<EMCSOfficeCode>().Any(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDispatch));
		}

		public void TestDeliveryPlaceCustomsOffice()
		{
			var co1 = declaration.CustomsOffices.AddNew();
			co1.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDelivery;
			co1.CY_Data = "CO001";

			ProcessMessage(message);
			var officeOfDelivery = declaration.CustomsOffices.Cast<EMCSOfficeCode>().Single(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDelivery);
			AssertEquals("DPCO001", officeOfDelivery.CY_Data);
		}

		public void TestDeliveryPlaceCustomsOffice_Empty()
		{
			var co1 = declaration.CustomsOffices.AddNew();
			co1.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDelivery;
			co1.CY_Data = "CO001";
			dataProviderMock.Setup(m => m.DeliveryPlaceCustomsOffice).Returns(ZString.Empty);
			ProcessMessage(message);
			AssertEquals("No Customs Office Empty", false, declaration.CustomsOffices.Cast<EMCSOfficeCode>().Any(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDelivery));
		}

		public void TestCompetentAuthorityOfDispatch()
		{
			var co1 = declaration.CustomsOffices.Cast<EMCSOfficeCode>().Single(x => x.CY_Code == EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch);
			co1.CY_Data = "CO001";

			ProcessMessage(message);
			var competentAuthorityOfDispatch = declaration.CustomsOffices.Cast<EMCSOfficeCode>().Single(x => x.CY_Code == EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch);
			AssertEquals("CADO001", competentAuthorityOfDispatch.CY_Data);
		}

		public void TestCompetentAuthorityOfDispatch_Empty()
		{
			var co1 = declaration.CustomsOffices.Cast<EMCSOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch);
			co1.CY_Data = "CO001";
			dataProviderMock.Setup(m => m.CompetentAuthorityDispatchOffice).Returns("");

			ProcessMessage(message);
			var competentAuthorityOfDispatch = declaration.CustomsOffices.Cast<EMCSOfficeCode>().Where(x => x.CY_Code == EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch);
			AssertEquals("Only have 1 CompetentAuthorityOfDispatch", 1, competentAuthorityOfDispatch.Count());
			AssertEquals("CompetentAuthorityOfDispatch is not updated", "CO001", competentAuthorityOfDispatch.FirstOrDefault().CY_Data);
		}

		public void TestImportSADNumbers()
		{
			var sad1 = declaration.ImportSADNumbers.AddNew();
			sad1.CSI_Description = "Test1";
			var sad2 = declaration.ImportSADNumbers.AddNew();
			sad2.CSI_Description = "Test2";
			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Have 2 ImportSADNumbers", 2, declaration.ImportSADNumbers.Count);
				AssertEquals("1st Sad Number", "SAD001", declaration.ImportSADNumbers[0].CSI_Description);
				AssertEquals("2st Sad Number", "SAD002", declaration.ImportSADNumbers[1].CSI_Description);
			});
		}

		public void TestImportSADNumbers_Empty()
		{
			var sad1 = declaration.ImportSADNumbers.AddNew();
			sad1.CSI_Description = "Test1";
			var sad2 = declaration.ImportSADNumbers.AddNew();
			sad2.CSI_Description = "Test2";
			dataProviderMock.Setup(m => m.ImportSadNumbers).Returns((IReadOnlyCollection<ZString>)Enumerable.Empty<ZString>());
			ProcessMessage(message);
			AssertEquals("Cleared", 0, declaration.ImportSADNumbers.Count);
		}

		public void TestDocumentCertificates()
		{
			var doc1 = declaration.Documents.AddNew();
			doc1.CSI_ReferenceNumber = "Test1";
			doc1.CSI_Description = "DS1";

			var doc2 = declaration.Documents.AddNew();
			doc2.CSI_ReferenceNumber = "Test2";
			doc2.CSI_Description = "DS2";

			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Have 2 Documents", 2, declaration.Documents.Count);
				AssertEquals("1st Document.CSI_ReferenceNumber", "Ref001", declaration.Documents[0].CSI_ReferenceNumber);
				AssertEquals("1st Document.CSI_Description", "DC001", declaration.Documents[0].CSI_Description);
				AssertEquals("1st Document.CSI_SubType", "TP001", declaration.Documents[0].CSI_SubType);

				AssertEquals("2st Document.CSI_ReferenceNumber", "Ref002", declaration.Documents[1].CSI_ReferenceNumber);
				AssertEquals("2st Document.CSI_Description", "DC002", declaration.Documents[1].CSI_Description);
				AssertEquals("2st Document.CSI_SubType", "TP002", declaration.Documents[1].CSI_SubType);
			});
		}

		public void TestDocumentCertificates_Empty()
		{
			var doc1 = declaration.Documents.AddNew();
			doc1.CSI_ReferenceNumber = "Test1";
			doc1.CSI_Description = "DS1";

			var doc2 = declaration.Documents.AddNew();
			doc2.CSI_ReferenceNumber = "Test2";
			doc2.CSI_Description = "DS2";

			dataProviderMock.Setup(m => m.DocumentCertificates).Returns((IReadOnlyCollection<IEMCSDocumentCert>)Enumerable.Empty<IEMCSDocumentCert>());
			ProcessMessage(message);
			AssertEquals("Cleared", 0, declaration.Documents.Count);
		}

		public void TestTransportDetails()
		{
			var transport1 = declaration.CusContainers.AddNew();
			transport1.ZG_UnitCode = "11";
			transport1.CO_ContainerNumber = "CN01";
			transport1.CO_Seal = "SL01";
			transport1.SealDetails = "SD01";
			transport1.Comment = "CT01";

			var transport2 = declaration.CusContainers.AddNew();
			transport2.ZG_UnitCode = "22";
			transport2.CO_ContainerNumber = "CN02";
			transport2.CO_Seal = "SL02";
			transport2.SealDetails = "SD02";
			transport2.Comment = "CT02";

			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Have 2 transportDetails", 2, declaration.CusContainers.ContainerNumbers.Count());
				AssertEquals("1st transportDetails.ZG_UnitCode", "1", declaration.CusContainers[0].ZG_UnitCode);
				AssertEquals("1st transportDetails.CO_ContainerNumber", "CO0001", declaration.CusContainers[0].CO_ContainerNumber);
				AssertEquals("1st transportDetails.CO_Seal", "SEAL1", declaration.CusContainers[0].CO_Seal);
				AssertEquals("1st transportDetails.SealDetails", "SI001", declaration.CusContainers[0].SealDetails);
				AssertEquals("1st transportDetails.Comment", "CI001", declaration.CusContainers[0].Comment);

				AssertEquals("2st transportDetails.ZG_UnitCode", "2", declaration.CusContainers[1].ZG_UnitCode);
				AssertEquals("2st transportDetails.CO_ContainerNumber", "CO0002", declaration.CusContainers[1].CO_ContainerNumber);
				AssertEquals("2st transportDetails.CO_Seal", "SEAL2", declaration.CusContainers[1].CO_Seal);
				AssertEquals("2st transportDetails.SealDetails", "SI002", declaration.CusContainers[1].SealDetails);
				AssertEquals("2st transportDetails.Comment", "CI002", declaration.CusContainers[1].Comment);
			});
		}

		public void TestTransportDetails_Empty()
		{
			var transport1 = declaration.CusContainers.AddNew();
			transport1.ZG_UnitCode = "11";
			transport1.CO_ContainerNumber = "CN01";
			transport1.CO_Seal = "SL01";
			transport1.SealDetails = "SD01";
			transport1.Comment = "CT01";

			var transport2 = declaration.CusContainers.AddNew();
			transport2.ZG_UnitCode = "22";
			transport2.CO_ContainerNumber = "CN02";
			transport2.CO_Seal = "SL02";
			transport2.SealDetails = "SD02";
			transport2.Comment = "CT02";

			dataProviderMock.Setup(m => m.TransportDetails).Returns((IReadOnlyCollection<IEMCSTransportDetails>)Enumerable.Empty<IEMCSTransportDetails>());
			ProcessMessage(message);
			AssertEquals("Cleared", 0, declaration.CusContainers.ContainerNumbers.Count());
		}

		public void TestGuarantor_MatchTENOrg()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var tenCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			tenCusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, Core.Constants.CountryCodes.Germany, "TN001");
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Match TEN number", orgHeader.PK, declaration.OwnerDocumentaryAddress.OrganisationPK);
				AssertEquals("Not override", false, declaration.OwnerDocumentaryAddress.E2_AddressOverride);
			});
		}

		public void TestGuarantor_MatchUSTOrg()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.ModifyOrgCusCode(orgHeader.PK, GermanyOrgCusCodeInfo.OrgCusCodes.UST, Core.Constants.CountryCodes.Germany, "VN001");
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Match UST number", orgHeader.PK, declaration.OwnerDocumentaryAddress.OrganisationPK);
				AssertEquals("Not override", false, declaration.OwnerDocumentaryAddress.E2_AddressOverride);
			});
		}

		public void TestGuarantor_AddressOverrideTEN()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Address override", true, declaration.OwnerDocumentaryAddress.E2_AddressOverride);
				AssertEquals("E2_GovRegNumType", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, declaration.OwnerDocumentaryAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "TN001", declaration.OwnerDocumentaryAddress.E2_GovRegNum);
				AssertEquals("E2_RN_NKCountryCode", "DE", declaration.OwnerDocumentaryAddress.E2_RN_NKCountryCode);
				AssertEquals("E2_City", "Berlin", declaration.OwnerDocumentaryAddress.E2_City);
				AssertEquals("E2_Address1", "Gp Address 1", declaration.OwnerDocumentaryAddress.E2_Address1);
				AssertEquals("E2_Postcode", "0001", declaration.OwnerDocumentaryAddress.E2_Postcode);
				AssertEquals("E2_CompanyName", "Guarantor Party 1", declaration.OwnerDocumentaryAddress.E2_CompanyName);
			});
		}

		public void TestGuarantor_AddressOverrideUST()
		{
			var guarantorMock = new Mock<IEMCSPartyGuarantor>();
			guarantorMock.Setup(m => m.TraderExciseNumber).Returns("");
			guarantorMock.Setup(m => m.VatNumber).Returns("VN001");
			guarantorMock.Setup(m => m.Language).Returns("DE");
			guarantorMock.Setup(m => m.Name).Returns("Guarantor Party 1");
			guarantorMock.Setup(m => m.Address).Returns("Gp Address 1");
			guarantorMock.Setup(m => m.City).Returns("Berlin");
			guarantorMock.Setup(m => m.Postcode).Returns("0001");
			guarantorMock.Setup(m => m.Country).Returns("DE");
			dataProviderMock.Setup(m => m.Guarantor).Returns(guarantorMock.Object);

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Address override", true, declaration.OwnerDocumentaryAddress.E2_AddressOverride);
				AssertEquals("E2_GovRegNumType", GermanyOrgCusCodeInfo.OrgCusCodes.UST, declaration.OwnerDocumentaryAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "VN001", declaration.OwnerDocumentaryAddress.E2_GovRegNum);
			});
		}

		public void TestGuarantor_null()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.ModifyOrgCusCode(orgHeader.PK, GermanyOrgCusCodeInfo.OrgCusCodes.UST, Core.Constants.CountryCodes.Germany, "VN001");
			declaration.OwnerDocumentaryAddress.OrganisationPK = orgHeader.PK;
			Factory.Save();
			dataProviderMock.Setup(m => m.Guarantor).Returns((IEMCSPartyGuarantor)null);
			ProcessMessage(message);
			AssertEquals("Reset", ZGuid.Empty, declaration.OwnerDocumentaryAddress.OrganisationPK);
		}

		public void TestConsignee_MatchOrg()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, Core.Constants.CountryCodes.Germany, "TI001");
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Match TEN number", orgHeader.PK, declaration.ImporterDocumentaryAddress.OrganisationPK);
				AssertEquals("Not override", false, declaration.ImporterDocumentaryAddress.E2_AddressOverride);
			});
		}

		public void TestConsignee_AddressOverride()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Address override", true, declaration.ImporterDocumentaryAddress.E2_AddressOverride);
				AssertEquals("E2_GovRegNumType", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, declaration.ImporterDocumentaryAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "TI001", declaration.ImporterDocumentaryAddress.E2_GovRegNum);
				AssertEquals("E2_RN_NKCountryCode", "DE", declaration.ImporterDocumentaryAddress.E2_RN_NKCountryCode);
				AssertEquals("E2_City", "Berlin", declaration.ImporterDocumentaryAddress.E2_City);
				AssertEquals("E2_Address1", "CP Address 1", declaration.ImporterDocumentaryAddress.E2_Address1);
				AssertEquals("E2_Postcode", "0001", declaration.ImporterDocumentaryAddress.E2_Postcode);
				AssertEquals("E2_CompanyName", "Consignee Party 1", declaration.ImporterDocumentaryAddress.E2_CompanyName);
			});
		}

		public void TestConsignee_null()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
			Factory.Save();
			dataProviderMock.Setup(m => m.Consignee).Returns((IEMCSPartyConsignee)null);
			ProcessMessage(message);
			AssertEquals("Reset", ZGuid.Empty, declaration.ImporterDocumentaryAddress.OrganisationPK);
		}

		public void TestConsignor_MatchOrg()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, Core.Constants.CountryCodes.Germany, "TN002");
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Match TEN number", orgHeader.PK, declaration.SupplierDocumentaryAddress.OrganisationPK);
				AssertEquals("Not override", false, declaration.SupplierDocumentaryAddress.E2_AddressOverride);
			});
		}

		public void TestConsignor_AddressOverride()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Address override", true, declaration.SupplierDocumentaryAddress.E2_AddressOverride);
				AssertEquals("E2_GovRegNumType", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, declaration.SupplierDocumentaryAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "TN002", declaration.SupplierDocumentaryAddress.E2_GovRegNum);
				AssertEquals("E2_RN_NKCountryCode", "DE", declaration.SupplierDocumentaryAddress.E2_RN_NKCountryCode);
				AssertEquals("E2_City", "Berlin", declaration.SupplierDocumentaryAddress.E2_City);
				AssertEquals("E2_Address1", "CRP Address 1", declaration.SupplierDocumentaryAddress.E2_Address1);
				AssertEquals("E2_Postcode", "0001", declaration.SupplierDocumentaryAddress.E2_Postcode);
				AssertEquals("E2_CompanyName", "Consignor Party 1", declaration.SupplierDocumentaryAddress.E2_CompanyName);
			});
		}

		public void TestConsignor_null()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			Factory.Save();
			dataProviderMock.Setup(m => m.Consignor).Returns((IEMCSPartyConsignor)null);
			ProcessMessage(message);
			AssertEquals("Reset", ZGuid.Empty, declaration.SupplierDocumentaryAddress.OrganisationPK);
		}

		public void TestPlaceOfDispatch_MatchOrg()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgMainAddress = orgHeader.Addresses.AddNewMainAddress();
			orgMainAddress.OA_Address1 = "Main 1";
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "A1";
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, Core.Constants.CountryCodes.Germany, "RTW001", orgAddress.PK);
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Match TID number", orgAddress.PK, declaration.DispatchWarehouseDocumentaryAddress.E2_OA_Address);
				AssertEquals("Not override", false, declaration.DispatchWarehouseDocumentaryAddress.E2_AddressOverride);
			});
		}

		public void TestPlaceOfDispatch_AddressOverride()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Address override", true, declaration.DispatchWarehouseDocumentaryAddress.E2_AddressOverride);
				AssertEquals("E2_GovRegNumType", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, declaration.DispatchWarehouseDocumentaryAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "RTW001", declaration.DispatchWarehouseDocumentaryAddress.E2_GovRegNum);
				AssertEquals("E2_RN_NKCountryCode", "DE", declaration.DispatchWarehouseDocumentaryAddress.E2_RN_NKCountryCode);
				AssertEquals("E2_City", "Berlin", declaration.DispatchWarehouseDocumentaryAddress.E2_City);
				AssertEquals("E2_Address1", "PRD Address 1", declaration.DispatchWarehouseDocumentaryAddress.E2_Address1);
				AssertEquals("E2_Postcode", "0001", declaration.DispatchWarehouseDocumentaryAddress.E2_Postcode);
				AssertEquals("E2_CompanyName", "PartyPlaceOfDispatch Party 1", declaration.DispatchWarehouseDocumentaryAddress.E2_CompanyName);
			});
		}
		public void TestPlaceOfDispatch_null()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declaration.DispatchWarehouseDocumentaryAddress.OrganisationPK = orgHeader.PK;
			Factory.Save();
			dataProviderMock.Setup(m => m.PlaceOfDispatch).Returns((IEMCSPartyPlaceOfDispatch)null);
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("organization Reset", ZGuid.Empty, declaration.DispatchWarehouseDocumentaryAddress.OrganisationPK);
				AssertEquals("Address Reset", ZGuid.Empty, declaration.DispatchWarehouseDocumentaryAddress.E2_OA_Address);
			});
		}

		public void TestDeliveryPlace_MatchOrg()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgMainAddress = orgHeader.Addresses.AddNewMainAddress();
			orgMainAddress.OA_Address1 = "Main 1";
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "A1";

			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, Core.Constants.CountryCodes.Germany, "TI002", orgAddress.PK);
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Match TID number", orgAddress.PK, declaration.DestinationWarehouseDocumentaryAddress.E2_OA_Address);
				AssertEquals("Not override", false, declaration.DestinationWarehouseDocumentaryAddress.E2_AddressOverride);
			});
		}

		public void TestDeliveryPlace_AddressOverride()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Address override", true, declaration.DestinationWarehouseDocumentaryAddress.E2_AddressOverride);
				AssertEquals("E2_GovRegNumType", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, declaration.DestinationWarehouseDocumentaryAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "TI002", declaration.DestinationWarehouseDocumentaryAddress.E2_GovRegNum);
				AssertEquals("E2_RN_NKCountryCode", "DE", declaration.DestinationWarehouseDocumentaryAddress.E2_RN_NKCountryCode);
				AssertEquals("E2_City", "Berlin", declaration.DestinationWarehouseDocumentaryAddress.E2_City);
				AssertEquals("E2_Address1", "DP Address 1", declaration.DestinationWarehouseDocumentaryAddress.E2_Address1);
				AssertEquals("E2_Postcode", "0001", declaration.DestinationWarehouseDocumentaryAddress.E2_Postcode);
				AssertEquals("E2_CompanyName", "DeliveryPlace Party 1", declaration.DestinationWarehouseDocumentaryAddress.E2_CompanyName);
			});
		}

		public void TestDeliveryPlace_null()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declaration.DestinationWarehouseDocumentaryAddress.OrganisationPK = orgHeader.PK;
			Factory.Save();
			dataProviderMock.Setup(m => m.DeliveryPlace).Returns((IEMCSPartyDeliveryPlace)null);
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Organization Reset", ZGuid.Empty, declaration.DestinationWarehouseDocumentaryAddress.OrganisationPK);
				AssertEquals("Address Reset", ZGuid.Empty, declaration.DestinationWarehouseDocumentaryAddress.E2_OA_Address);
			});
		}

		public void TestTransportArranger_MatchOrg()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.ModifyOrgCusCode(orgHeader.PK, GermanyOrgCusCodeInfo.OrgCusCodes.UST, Core.Constants.CountryCodes.Germany, "VN002");
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Match UST number", orgHeader.PK, declaration.CarrierAgentDocumentaryAddress.OrganisationPK);
				AssertEquals("Not override", false, declaration.CarrierAgentDocumentaryAddress.E2_AddressOverride);
			});
		}

		public void TestTransportArranger_AddressOverride()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Address override", true, declaration.CarrierAgentDocumentaryAddress.E2_AddressOverride);
				AssertEquals("E2_GovRegNumType", GermanyOrgCusCodeInfo.OrgCusCodes.UST, declaration.CarrierAgentDocumentaryAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "VN002", declaration.CarrierAgentDocumentaryAddress.E2_GovRegNum);
				AssertEquals("E2_RN_NKCountryCode", "DE", declaration.CarrierAgentDocumentaryAddress.E2_RN_NKCountryCode);
				AssertEquals("E2_City", "Berlin", declaration.CarrierAgentDocumentaryAddress.E2_City);
				AssertEquals("E2_Address1", "TAP Address 1", declaration.CarrierAgentDocumentaryAddress.E2_Address1);
				AssertEquals("E2_Postcode", "0001", declaration.CarrierAgentDocumentaryAddress.E2_Postcode);
				AssertEquals("E2_CompanyName", "TransportArranger Party 1", declaration.CarrierAgentDocumentaryAddress.E2_CompanyName);
			});
		}

		public void TestTransportArranger_null()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declaration.CarrierAgentDocumentaryAddress.OrganisationPK = orgHeader.PK;
			Factory.Save();
			dataProviderMock.Setup(m => m.TransportArranger).Returns((IEMCSPartyTransporter)null);
			ProcessMessage(message);
			AssertEquals("Reset", ZGuid.Empty, declaration.CarrierAgentDocumentaryAddress.OrganisationPK);
		}

		public void TestFirstTransporter_MatchOrg()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.ModifyOrgCusCode(orgHeader.PK, GermanyOrgCusCodeInfo.OrgCusCodes.UST, Core.Constants.CountryCodes.Germany, "VN003");
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Match UST number", orgHeader.PK, declaration.TransporterDocumentaryAddress.OrganisationPK);
				AssertEquals("Not override", false, declaration.TransporterDocumentaryAddress.E2_AddressOverride);
			});
		}

		public void TestFirstTransporter_AddressOverride()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Address override", true, declaration.TransporterDocumentaryAddress.E2_AddressOverride);
				AssertEquals("E2_GovRegNumType", GermanyOrgCusCodeInfo.OrgCusCodes.UST, declaration.TransporterDocumentaryAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "VN003", declaration.TransporterDocumentaryAddress.E2_GovRegNum);
				AssertEquals("E2_RN_NKCountryCode", "DE", declaration.TransporterDocumentaryAddress.E2_RN_NKCountryCode);
				AssertEquals("E2_City", "Berlin", declaration.TransporterDocumentaryAddress.E2_City);
				AssertEquals("E2_Address1", "FTP Address 1", declaration.TransporterDocumentaryAddress.E2_Address1);
				AssertEquals("E2_Postcode", "0001", declaration.TransporterDocumentaryAddress.E2_Postcode);
				AssertEquals("E2_CompanyName", "FirstTransporter Party 1", declaration.TransporterDocumentaryAddress.E2_CompanyName);
			});
		}

		public void TestFirstTransporter_null()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declaration.TransporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
			Factory.Save();
			dataProviderMock.Setup(m => m.FirstTransporter).Returns((IEMCSPartyTransporter)null);
			ProcessMessage(message);
			AssertEquals("Reset", ZGuid.Empty, declaration.TransporterDocumentaryAddress.OrganisationPK);
		}

		public void TestLinesDeleted()
		{
			var invoiceLine = declaration.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00001";
			Factory.Save();
			var lineMock2 = CreateLine(2, "S200", "22084012");
			lineMock2.Setup(m => m.Packages).Returns((IReadOnlyCollection<IEMCSPackageInComing>)Enumerable.Empty<IEMCSPackageInComing>());
			dataProviderMock.Setup(m => m.Lines).Returns(new IED801Line[] { lineMock1.Object, lineMock2.Object });
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Invoice lines are replaced", 2, declaration.FilteredInvoiceLines.Count);
				var invoiceLine1 = declaration.FilteredInvoiceLines[0];
				var invoiceLine2 = declaration.FilteredInvoiceLines[1];
				AssertEquals("invoiceLine1.JI_LineNo", (ZShort)1, invoiceLine1.JI_LineNo);
				AssertEquals("invoiceLine1.ZG_ExciseProductCode", "W200", invoiceLine1.ZG_ExciseProductCode);
				AssertEquals("invoiceLine1.JI_Tariff", "22084011", invoiceLine1.JI_Tariff);
				AssertEquals("invoiceLine2.JI_LineNo", (ZShort)2, invoiceLine2.JI_LineNo);
				AssertEquals("invoiceLine2.ZG_ExciseProductCode", "S200", invoiceLine2.ZG_ExciseProductCode);
				AssertEquals("invoiceLine2.JI_Tariff", "22084012", invoiceLine2.JI_Tariff);
			});
		}

		public void TestPopulateOneInvoiceLine()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				var invoiceLine = declaration.FilteredInvoiceLines[0];
				AssertEquals("JI_LineNo", (ZShort)1, invoiceLine.JI_LineNo);
				AssertEquals("ZG_ExciseProductCode", "W200", invoiceLine.ZG_ExciseProductCode);
				AssertEquals("JI_Tariff", "22084011", invoiceLine.JI_Tariff);
				AssertEquals("ZG_FiscalMarkUsed", true, invoiceLine.ZG_FiscalMarkUsed);
				AssertEquals("ZG_FiscalMark", "FM001", invoiceLine.ZG_FiscalMark);
				AssertEquals("JI_Origin", "CN", invoiceLine.ZG_Origin);
				AssertEquals("JI_NDescription", "CD001", invoiceLine.JI_NDescription);
				AssertEquals("JI_BrandName", "BP001", invoiceLine.JI_BrandName);
				AssertEquals("JI_CustomsQuantity", 2m, invoiceLine.JI_CustomsQuantity);
				AssertEquals("ZG_AddInfo/DeclaredValue", 2m, invoiceLine.ZG_DeclaredValue);
				AssertEquals("JI_Weight", 3m, invoiceLine.JI_Weight);
				AssertEquals("JI_NetWeight", 4m, invoiceLine.JI_NetWeight);
				AssertEquals("ZG_AlcoholicStrength", 5m, invoiceLine.ZG_AlcoholicStrength);
				AssertEquals("ZG_DegreePlato", 6m, invoiceLine.ZG_DegreePlato);
				AssertEquals("ZG_Density", 8m, invoiceLine.ZG_Density);
				AssertEquals("ZG_SizeOfProducer", 7m, invoiceLine.ZG_SizeOfProducer);
				AssertEquals("ZG_GrowingZone", "1", invoiceLine.ZG_GrowingZone);
				AssertEquals("ZG_WineCategory", "1", invoiceLine.ZG_WineCategory);
				AssertEquals("ZG_WineCountryOrigin", "DE", invoiceLine.ZG_WineCountryOrigin);
				AssertEquals("JI_WineDetailsComments", "WPOI001", invoiceLine.JI_WineDetailsComments);
				AssertEquals("ZG_MaturationPeriodOrAgeOfProducts", "12 Months", invoiceLine.ZG_MaturationPeriodOrAgeOfProducts);
			});
		}

		public void TestFiscalMark()
		{
			lineMock1.Setup(m => m.FiscalMarkUsedFlag).Returns(false);
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				var invoiceLine = declaration.FilteredInvoiceLines[0];
				AssertEquals("ZG_FiscalMarkUsed", false, invoiceLine.ZG_FiscalMarkUsed);
				AssertEquals("ZG_FiscalMark is independent of JI_FiscalMarkUsed", "FM001", invoiceLine.ZG_FiscalMark);
			});
		}

		public void TestLine_WineOperationCodes()
		{
			var existingInvoiceLine = declaration.InvoiceHeader.InvoiceLines.AddNew();
			var wineCode = existingInvoiceLine.OperationCodeDataCollection.AddNew();
			wineCode.CY_Code = "WO000";
			Factory.Save();
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				var invoiceLine = declaration.FilteredInvoiceLines[0];
				AssertEquals("2 Operation Codes", 2, invoiceLine.OperationCodeDataCollection.Count);
				AssertEquals("OperationCodeData1.CY_Code", "WO001", invoiceLine.OperationCodeDataCollection[0].CY_Code);
				AssertEquals("OperationCodeData2.CY_Code", "WO002", invoiceLine.OperationCodeDataCollection[1].CY_Code);
			});
		}

		public void TestMainPackagesUpdated()
		{
			var package = declaration.EMCSPackages.AddNew();
			package.B5_UnitCount = 4;
			package.B5_UnitType = "PK";
			Factory.Save();
			ProcessMessage(message);
			AssertEquals("Declaration packages updated with 2 packages", 2, declaration.EMCSPackages.Count);
		}

		public void TestLine_Packages()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				var invoiceLine1 = declaration.FilteredInvoiceLines[0];
				var package1 = invoiceLine1.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().FirstOrDefault(p => p.IsForInvoiceLine);
				AssertEquals("Package 1 UQ", "CT", package1.UnitType);
				AssertEquals("Package 1 Quantity", 5, package1.UnitCount);
				AssertEquals("B5_SealNumber", "SN001", package1.SealNumber);
				AssertEquals("B5_SealComment", "SC001", package1.SealComment);
				AssertEquals("B5_MarksAndNumbers", "Shipping Marks 1", package1.MarksAndNumbers);
				AssertEquals("Linked to the line", true, package1.IsForInvoiceLine);
				AssertEquals("IsMainPack", true, invoiceLine1.ZG_IsMainPack);

				var invoiceLine2 = declaration.FilteredInvoiceLines[1];
				var package2 = invoiceLine2.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().FirstOrDefault(p => p.IsForInvoiceLine);
				AssertEquals("Package 2 UQ", "VQ", package2.UnitType);
				AssertEquals("Package 2 Quantity", ZLong.Zero, package2.UnitCount);
				AssertEquals("Linked to the line", true, package2.IsForInvoiceLine);
				AssertEquals("IsMainPack", true, invoiceLine2.ZG_IsMainPack);

				var invoiceLine3 = declaration.FilteredInvoiceLines[2];
				var package3 = invoiceLine3.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().FirstOrDefault(p => p.IsForInvoiceLine);
				AssertEquals("Package 3 UQ", "CT", package3.UnitType);
				AssertEquals("Package 3 Quantity", 5, package3.UnitCount);
				AssertEquals("Linked to the line", true, package3.IsForInvoiceLine);
				AssertEquals("IsMainPack", false, invoiceLine3.ZG_IsMainPack);
			});
		}

		public void TestPopulateLogbookLocalReferenceNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookLocalReferenceNumber", "B000222547896254786321", message.GetLogbookLocalReferenceNumber());
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", "MRN98761234", message.GetLogbookRegistrationNumber());
		}

		public void TestGenerateEmail()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			var glbGroup = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, GlbGroup.AllStaffGroupCode);
			var emailGroupPK = glbGroup.PK;
			Factory.Save();
			var registryItem = new EmcsGroupNotification(Core.Constants.EmailTo.NominatedGroup, emailGroupPK);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, registryItem))
			{
				ProcessMessage(message);
				Factory.Save(); // emulate save from Batch processor because we send email with OnSaving
			}
			AssertEmail("Generate Email", declaration);
		}

		public void TestGenerateEmailNewDeclaration()
		{
			declaration.Delete();
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			var glbGroup = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, GlbGroup.AllStaffGroupCode);
			var emailGroupPK = glbGroup.PK;
			Factory.Save();
			var registryItem = new EmcsGroupNotification(Core.Constants.EmailTo.NominatedGroup, emailGroupPK);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, registryItem))
			{
				ProcessMessage(message);
				Factory.Save(); // emulate save from Batch processor because we send email with OnSaving
			}
			AssertEmail("Generate Email", declaration);
		}

		void AssertEmail(string testCase, EMCSJobDeclaration declaration)
		{
			var query = new ZDBOnlyQuery(typeof(EMCSJobDeclaration));
			query.OrderBy = EMCSJobDeclaration.Schema.JE_DeclarationReference + " desc";
			declaration = Factory.LoadTop1<EMCSJobDeclaration>(query);
			var reference = declaration.JE_DeclarationReference;
			var subject = $"Incoming EMCS e-AD Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = @"<strong>Incoming EMCS e-AD Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EMCS&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $"An incoming EMCS Declaration created Job {reference}. For details please follow the link to the Job."
				+ "<br /><br />ARC: MRN98761234";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			AssertEmailForSingleRecipient(testCase, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary);
		}

		protected override ZString MessageFriendlyName => "EMCS ED801 Consignee Message Processor";

		protected override DE.Business.DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED801>> Processor => new ED801ConsigneeMessageProcessor(logger);

		protected override bool ExpectedMustHaveLinkedObject => false;

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.CreateDeclarationWithEadReference("MRN98761234", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignee);
			var emcsEvent = new Mock<IEMCSEvent>();
			emcsEvent.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761234");
			emcsEvent.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock = new Mock<IED801>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("DE90003480001003");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Emb);
			dataProviderMock.Setup(m => m.MessageSender).Returns("DE000050");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("B000222547896254786321");
			dataProviderMock.Setup(m => m.DateAndTimeOfValidationOfEadEsad).Returns(new ZDateTime(2020, 6, 11, 16, 59, 59));
			dataProviderMock.Setup(m => m.ExciseMovement).Returns(emcsEvent.Object);

			dataProviderMock.Setup(m => m.DispatchImportOffice).Returns("DIO001");
			dataProviderMock.Setup(m => m.DeliveryPlaceCustomsOffice).Returns("DPCO001");
			dataProviderMock.Setup(m => m.CompetentAuthorityDispatchOffice).Returns("CADO001");
			dataProviderMock.Setup(m => m.JourneyTime).Returns("2D");
			dataProviderMock.Setup(m => m.DestinationTypeCode).Returns("1");
			dataProviderMock.Setup(m => m.TransportArrangement).Returns("3");
			dataProviderMock.Setup(m => m.DispatchTime).Returns(new ZDateTime(2020, 7, 27, 16, 59, 59));
			dataProviderMock.Setup(m => m.OriginTypeCode).Returns("1");
			dataProviderMock.Setup(m => m.InvoiceNumber).Returns("1");
			dataProviderMock.Setup(m => m.InvoiceDate).Returns(new ZDate(new ZDateTime(2020, 7, 25, 16, 59, 59)));
			dataProviderMock.Setup(m => m.ImportSadNumbers).Returns(new ZString[2] { "SAD001", "SAD002" });
			dataProviderMock.Setup(m => m.GuarantorTypeCode).Returns("3");
			dataProviderMock.Setup(m => m.TransportModeCode).Returns("4");
			dataProviderMock.Setup(m => m.ComplementaryInfo).Returns("CI001");
			dataProviderMock.Setup(m => m.MemberStateCode).Returns("12");
			dataProviderMock.Setup(m => m.CertificateOfExemption).Returns("CE001");

			var guarantorMock = new Mock<IEMCSPartyGuarantor>();
			guarantorMock.Setup(m => m.TraderExciseNumber).Returns("TN001");
			guarantorMock.Setup(m => m.VatNumber).Returns("VN001");
			guarantorMock.Setup(m => m.Language).Returns("DE");
			guarantorMock.Setup(m => m.Name).Returns("Guarantor Party 1");
			guarantorMock.Setup(m => m.Address).Returns("Gp Address 1");
			guarantorMock.Setup(m => m.City).Returns("Berlin");
			guarantorMock.Setup(m => m.Postcode).Returns("0001");
			guarantorMock.Setup(m => m.Country).Returns("DE");

			dataProviderMock.Setup(m => m.Guarantor).Returns(guarantorMock.Object);

			var consigneeMock = new Mock<IEMCSPartyConsignee>();
			consigneeMock.Setup(m => m.TraderId).Returns("TI001");
			consigneeMock.Setup(m => m.Language).Returns("DE");
			consigneeMock.Setup(m => m.Name).Returns("Consignee Party 1");
			consigneeMock.Setup(m => m.Address).Returns("CP Address 1");
			consigneeMock.Setup(m => m.City).Returns("Berlin");
			consigneeMock.Setup(m => m.Postcode).Returns("0001");
			consigneeMock.Setup(m => m.Country).Returns("DE");

			dataProviderMock.Setup(m => m.Consignee).Returns(consigneeMock.Object);

			var consignorMock = new Mock<IEMCSPartyConsignor>();
			consignorMock.Setup(m => m.TraderExciseNumber).Returns("TN002");
			consignorMock.Setup(m => m.Language).Returns("DE");
			consignorMock.Setup(m => m.Name).Returns("Consignor Party 1");
			consignorMock.Setup(m => m.Address).Returns("CRP Address 1");
			consignorMock.Setup(m => m.City).Returns("Berlin");
			consignorMock.Setup(m => m.Postcode).Returns("0001");
			consignorMock.Setup(m => m.Country).Returns("DE");

			dataProviderMock.Setup(m => m.Consignor).Returns(consignorMock.Object);

			var partyPlaceOfDispatchMock = new Mock<IEMCSPartyPlaceOfDispatch>();
			partyPlaceOfDispatchMock.Setup(m => m.ReferenceOfTaxWarehouse).Returns("RTW001");
			partyPlaceOfDispatchMock.Setup(m => m.Language).Returns("DE");
			partyPlaceOfDispatchMock.Setup(m => m.Name).Returns("PartyPlaceOfDispatch Party 1");
			partyPlaceOfDispatchMock.Setup(m => m.Address).Returns("PRD Address 1");
			partyPlaceOfDispatchMock.Setup(m => m.City).Returns("Berlin");
			partyPlaceOfDispatchMock.Setup(m => m.Postcode).Returns("0001");
			partyPlaceOfDispatchMock.Setup(m => m.Country).Returns("DE");

			dataProviderMock.Setup(m => m.PlaceOfDispatch).Returns(partyPlaceOfDispatchMock.Object);

			var deliveryPlaceMock = new Mock<IEMCSPartyDeliveryPlace>();
			deliveryPlaceMock.Setup(m => m.TraderId).Returns("TI002");
			deliveryPlaceMock.Setup(m => m.Language).Returns("DE");
			deliveryPlaceMock.Setup(m => m.Name).Returns("DeliveryPlace Party 1");
			deliveryPlaceMock.Setup(m => m.Address).Returns("DP Address 1");
			deliveryPlaceMock.Setup(m => m.City).Returns("Berlin");
			deliveryPlaceMock.Setup(m => m.Postcode).Returns("0001");
			deliveryPlaceMock.Setup(m => m.Country).Returns("DE");

			dataProviderMock.Setup(m => m.DeliveryPlace).Returns(deliveryPlaceMock.Object);

			var transportArrangerMock = CreatePartyTransporter("VN002", "TransportArranger Party 1", "TAP Address 1");
			dataProviderMock.Setup(m => m.TransportArranger).Returns(transportArrangerMock);

			var firstTransporterMock = CreatePartyTransporter("VN003", "FirstTransporter Party 1", "FTP Address 1");
			dataProviderMock.Setup(m => m.FirstTransporter).Returns(firstTransporterMock);

			var documentCertMock1 = new Mock<IEMCSDocumentCert>();
			documentCertMock1.Setup(m => m.Reference).Returns("Ref001");
			documentCertMock1.Setup(m => m.Description).Returns("DC001");
			documentCertMock1.Setup(m => m.Type).Returns("TP001");

			var documentCertMock2 = new Mock<IEMCSDocumentCert>();
			documentCertMock2.Setup(m => m.Reference).Returns("Ref002");
			documentCertMock2.Setup(m => m.Description).Returns("DC002");
			documentCertMock2.Setup(m => m.Type).Returns("TP002");

			dataProviderMock.Setup(m => m.DocumentCertificates).Returns(new IEMCSDocumentCert[] { documentCertMock1.Object, documentCertMock2.Object });

			var transportDetailsMock1 = CreateTransportDetails("1");
			var transportDetailsMock2 = CreateTransportDetails("2");
			dataProviderMock.Setup(m => m.TransportDetails).Returns(new IEMCSTransportDetails[] { transportDetailsMock1, transportDetailsMock2 });

			lineMock1 = CreateLine(1, "W200", "22084011");
			var lineMock2 = CreateLine(2, "W202", "22084011");
			var lineMock3 = CreateLine(3, "W300", "22084013");

			//countable package with count > 0, main pack
			var packageMock1 = CreatePackage("CT", 5, true);

			//not countable package with count = 0, main pack
			var packageMock2 = CreatePackage("VQ", 0, false);

			//countable package with count = 0, inner pack
			var packageMock3 = CreatePackage("CT", 0, true);

			lineMock1.Setup(m => m.Packages).Returns(new IEMCSPackageInComing[] { packageMock1 });
			lineMock2.Setup(m => m.Packages).Returns(new IEMCSPackageInComing[] { packageMock2 });
			lineMock3.Setup(m => m.Packages).Returns(new IEMCSPackageInComing[] { packageMock3 });

			dataProviderMock.Setup(m => m.Lines).Returns(new IED801Line[] { lineMock1.Object, lineMock2.Object, lineMock3.Object });

			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED801>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;
			Factory.Save();
		}
		EMCSJobDeclaration declaration;
		Mock<EmcsInboundEDIMessage<IED801>> messageMock;
		Mock<IED801> dataProviderMock;
		Mock<IED801Line> lineMock1;
		EmcsInboundEDIMessage<IED801> message;

		IEMCSPartyTransporter CreatePartyTransporter(string vatNumber, string name, string address)
		{
			var partyTransporterMock = new Mock<IEMCSPartyTransporter>();
			partyTransporterMock.Setup(m => m.VatNumber).Returns(vatNumber);
			partyTransporterMock.Setup(m => m.Language).Returns("DE");
			partyTransporterMock.Setup(m => m.Name).Returns(name);
			partyTransporterMock.Setup(m => m.Address).Returns(address);
			partyTransporterMock.Setup(m => m.City).Returns("Berlin");
			partyTransporterMock.Setup(m => m.Postcode).Returns("0001");
			partyTransporterMock.Setup(m => m.Country).Returns("DE");
			return partyTransporterMock.Object;
		}

		IEMCSTransportDetails CreateTransportDetails(ZString unitCode)
		{
			var transportDetailsMock = new Mock<IEMCSTransportDetails>();
			transportDetailsMock.Setup(m => m.UnitCode).Returns(unitCode);
			transportDetailsMock.Setup(m => m.IdentityOfUnit).Returns("CO000" + unitCode);
			transportDetailsMock.Setup(m => m.CommercialSealIdentification).Returns("SEAL" + unitCode);
			transportDetailsMock.Setup(m => m.ComplementaryInformation).Returns("CI00" + unitCode);
			transportDetailsMock.Setup(m => m.SealInformation).Returns("SI00" + unitCode);
			return transportDetailsMock.Object;
		}

		Mock<IED801Line> CreateLine(ZInt lineNo, ZString exciseCode, ZString tariff)
		{
			var lineMock = new Mock<IED801Line>();
			lineMock.Setup(m => m.BodyRecordUniqueReference).Returns(lineNo);
			lineMock.Setup(m => m.ExciseProductCode).Returns(exciseCode);
			lineMock.Setup(m => m.CnCode).Returns(tariff);
			lineMock.Setup(m => m.FiscalMarkUsedFlag).Returns(true);
			lineMock.Setup(m => m.FiscalMark).Returns("FM001");
			lineMock.Setup(m => m.DesignationOfOrigin).Returns("CN");
			lineMock.Setup(m => m.CommercialDescription).Returns("CD001");
			lineMock.Setup(m => m.BrandNameOfProducts).Returns("BP001");
			lineMock.Setup(m => m.Quantity).Returns(2m);
			lineMock.Setup(m => m.GrossWeight).Returns(3m);
			lineMock.Setup(m => m.NetWeight).Returns(4m);
			lineMock.Setup(m => m.AlcoholicStrength).Returns(5m);
			lineMock.Setup(m => m.DegreePlato).Returns(6m);
			lineMock.Setup(m => m.SizeOfProducer).Returns(new ZDecimal("7"));
			lineMock.Setup(m => m.Density).Returns(8m);
			lineMock.Setup(m => m.WineGrowingZoneCode).Returns("1");
			lineMock.Setup(m => m.WineProductCategory).Returns("1");
			lineMock.Setup(m => m.WineProductThirdCountryOfOrigin).Returns("DE");
			lineMock.Setup(m => m.WineProductOtherInfo).Returns("WPOI001");
			lineMock.Setup(m => m.WineOperationCodes).Returns(new ZString[] { "WO001", "WO002" });
			lineMock.Setup(m => m.MaturationPeriodOrAgeOfProducts).Returns("12 Months");
			return lineMock;
		}

		IEMCSPackageInComing CreatePackage(ZString unitOfQuantity, ZLong quantity, ZBool isCountable)
		{
			var packageMock = new Mock<IEMCSPackageInComing>();
			packageMock.Setup(m => m.KindOfPackages).Returns(unitOfQuantity);
			packageMock.Setup(m => m.NumberOfPackages).Returns(quantity);
			packageMock.Setup(m => m.SealNumber).Returns("SN001");
			packageMock.Setup(m => m.SealInformation).Returns("SC001");
			packageMock.Setup(m => m.ShippingMarks).Returns("Shipping Marks 1");
			packageMock.Setup(m => m.IsNumberOfPackagesProvided).Returns(isCountable);
			return packageMock.Object;
		}
	}
}
