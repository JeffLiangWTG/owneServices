using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.EU.EMCS.Registry;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE801MessageProcessor))]
	abstract class IE801MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE801MessageProcessor, IIE801>
	{
		public void TestEndToEndProcessing_WhenConsigneeDeclarationMatched()
		{
			base.CreateSetupData();
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			TestEndToEndProcessing(incomingMessage);
			CombineAssertions("Process", () =>
			{
				AssertEquals("JE_MessageStatus should have been set RCV.", EDIMessage.Status.Received, declaration.JE_MessageStatus);
				AssertEquals("JE_EntryStatus should have been set REG.", EntryStatusList.Codes.REG, declaration.JE_EntryStatus);
				AssertEquals("EM_Status should have been set PRS.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
				AssertDeclarationUpdateResult(declaration, incomingMessage);
				MessageProcessorNotificationTestHelper.AssertEmail("Incoming EMCS e-AD", new[] { "An incoming EMCS Declaration created Job E00000810. For details please follow the link to the Job." }, new[] { "staff1@where.com" });
			});
		}

		public void TestEndToEndProcessing_WhenNoDeclarationMatched()
		{
			var incomingMessage = CreateNewIncomingMessage();
			TestEndToEndProcessing(incomingMessage);
			CombineAssertions("Process", () =>
			{
				var declaration = (EMCSJobDeclaration)incomingMessage.EM_LinkedObject;
				AssertNotNull("A new declaration should be created.", declaration);
				AssertEquals("JE_DeclarantType should have been set Consignee.", EMCSEntryTypeList.Codes.Consignee, declaration.JE_DeclarantType);
				AssertEquals("JE_MessageStatus should have been set RCV.", EDIMessage.Status.Received, declaration.JE_MessageStatus);
				AssertEquals("JE_EntryStatus should have been set REG.", EntryStatusList.Codes.REG, declaration.JE_EntryStatus);
				AssertEquals("EM_Status should have been set PRS.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
				AssertDeclarationUpdateResult(declaration, incomingMessage);
				MessageProcessorNotificationTestHelper.AssertEmail("Incoming EMCS e-AD", new[] { "An incoming EMCS Declaration created Job E00000001. For details please follow the link to the Job." }, new[] { "staff1@where.com" });
			});
		}

		public void TestEndToEndProcessing_UnderConcurrencyConflict()
		{
			base.CreateSetupData();
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			TestEndToEndProcessing(incomingMessage, () =>
			{
				var newFactory = NewFactory();
				newFactory.RefreshEnabled = false;
				newFactory.Load<EMCSJobDeclaration>(incomingMessage.LinkedDeclaration.PK).JE_MessageStatus = EDIMessage.Status.Acknowledged;
				newFactory.Save();
			});
			CombineAssertions("Simulate concurrency conflict", () =>
			{
				AssertEquals("JE_MessageStatus should have been set RCV.", EDIMessage.Status.Received, declaration.JE_MessageStatus);
				AssertEquals("JE_EntryStatus should have been set REG.", EntryStatusList.Codes.REG, declaration.JE_EntryStatus);
				AssertEquals("EM_Status should have been set PRS.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			});
		}

		protected void TestEndToEndProcessing(EDIMessage incomingMessage, Action actionBeforeFactorySave = null)
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "staff1@where.com";
			var glbGroup = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, GlbGroup.AllStaffGroupCode);
			var emailGroupPK = glbGroup.PK;
			Factory.Save();

			using (incomingMessage.Factory.AddDisposableService())
			{
				var registryItem = new EmcsGroupNotification(Core.Constants.EmailTo.NominatedGroup, emailGroupPK);
				using (EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, registryItem))
				{
					Processor.PreProcessMessage(incomingMessage);
					Factory.Save();
					Processor.ProcessMessage(incomingMessage);
					actionBeforeFactorySave?.Invoke();
					Factory.Save();
				}
			}
		}

		protected override ZString MessageType => EMCSGBIncomingMessageTypeList.Codes.IE801;

		protected override IE801MessageProcessor Processor => new IE801MessageProcessor(logger, typeof(TMessageType));

		protected override void CreateSetupData()
		{
			base.CreateSetupData();
			var line1 = declaration.InvoiceLines.AddNew();
			line1.JI_CustomsQuantity = 100;
			line1.ZG_DeclaredValue = 0;
			var line2 = declaration.InvoiceLines.AddNew();
			line2.JI_CustomsQuantity = 200;
			line2.ZG_DeclaredValue = 0;
		}

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("JE_MessageStatus should have been set RCV.", EDIMessage.Status.Received, declaration.JE_MessageStatus);
			AssertEquals("JE_EntryStatus should have been set REG.", EntryStatusList.Codes.REG, declaration.JE_EntryStatus);
			AssertEquals("EM_Status should have been set PRS.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);

			var query = new ZQuery();
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, EMCSJobDeclaration.Schema.TableName);
			query.AddToFilter(CusEntryNumSchema.CE_ParentID, declaration.PK);
			var entryNumber = incomingMessage.Factory.Load<CusEntryNumber>(query).Single();
			AssertEquals("CE_Category", CusEntryNumber.Categories.CustomsPermitClearanceNumber, entryNumber.CE_Category);
			AssertEquals("CE_EntryNum", "MRN1234567", entryNumber.CE_EntryNum);
			AssertEquals("CE_EntryLineReference", "1", entryNumber.CE_EntryLineReference);
			AssertEquals("CE_IssueDate", new ZDateTime(2022, 8, 30, 13, 30, 0), entryNumber.CE_IssueDate);
			AssertEquals("CE_EntryType", CusEntryNumberTypes.Standard.MovementReferenceNumber, entryNumber.CE_EntryType);

			AssertEquals("Line1 DeclaredValue", (ZDecimal)100, declaration.InvoiceLines[0].ZG_DeclaredValue);
			AssertEquals("Line2 DeclaredValue", (ZDecimal)200, declaration.InvoiceLines[1].ZG_DeclaredValue);

			MessageProcessorNotificationTestHelper.AssertEmail("EMCS e-AD registered", new[] { "Your EMCS Declaration for Job E00000810 has been registered. For details please follow the Link to the Job." }, new string[] { "staff1@where.com" });
		}

		void AssertDeclarationUpdateResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			var query = new ZQuery();
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, EMCSJobDeclaration.Schema.TableName);
			query.AddToFilter(CusEntryNumSchema.CE_ParentID, declaration.PK);
			var entryNumber = incomingMessage.Factory.Load<CusEntryNumber>(query).Single();
			AssertEquals("CE_Category", CusEntryNumber.Categories.CustomsPermitClearanceNumber, entryNumber.CE_Category);
			AssertEquals("CE_EntryNum", "MRN1234567", entryNumber.CE_EntryNum);
			AssertEquals("CE_EntryLineReference", "1", entryNumber.CE_EntryLineReference);
			AssertEquals("CE_IssueDate", new ZDateTime(2022, 8, 30, 13, 30, 0), entryNumber.CE_IssueDate);
			AssertEquals("CE_EntryType", CusEntryNumberTypes.Standard.MovementReferenceNumber, entryNumber.CE_EntryType);

			AssertEquals("12H", declaration.ZG_JourneyTime);
			AssertEquals("1", declaration.JE_MessageSubType);
			AssertEquals("3", declaration.ZG_TransportArrangement);
			AssertEquals("JE_DateAtOrigin", new ZDateTime(2022, 8, 30, 13, 30, 0), declaration.JE_DateAtOrigin);
			AssertEquals("1", declaration.ZG_OriginType);
			AssertEquals("B000222547896254786321", declaration.JE_OwnerRef);
			AssertEquals("1", declaration.InvoiceNumber);
			AssertEquals("InvoiceDate", new ZDateTime(2022, 8, 29), declaration.InvoiceDate);
			AssertEquals("12", declaration.ZG_CCTMSA);
			AssertEquals("CE001", declaration.ZG_CertOfExemption);
			AssertEquals("3", declaration.ZG_GuarantorType);
			AssertEquals("AIR", declaration.JE_TransportMode);
			AssertEquals("CI001", declaration.SpecialInstructions);

			var customsOffice = declaration.CustomsOffices.Cast<OfficeCode>().Single(x => x.CY_Code == OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival);
			AssertEquals("Office Code", "NDEA.GB", customsOffice.CY_Data);
			var officeOfDispatch = declaration.CustomsOffices.Cast<OfficeCode>().Single(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDispatch);
			AssertEquals("DIO001", officeOfDispatch.CY_Data);
			var officeOfDelivery = declaration.CustomsOffices.Cast<OfficeCode>().Single(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDelivery);
			AssertEquals("DPCO001", officeOfDelivery.CY_Data);
			var competentAuthorityOfDispatch = declaration.CustomsOffices.Cast<OfficeCode>().Single(x => x.CY_Code == EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch);
			AssertEquals("CADO001", competentAuthorityOfDispatch.CY_Data);

			AssertImportSADNumbersUpdateResult(declaration);
			AssertDocumentsUpdateResult(declaration);
			AssertTransportDetailsUpdateResult(declaration);
			AssertGuarantorUpdateResult(declaration);
			AssertConsigneeUpdateResult(declaration);
			AssertConsignorUpdateResult(declaration);
			AssertPlaceOfDispatchUpdateResult(declaration);
			AssertDeliveryPlaceUpdateResult(declaration);
			AssertTransportArrangerUpdateResult(declaration);
			AssertFirstTransporterUpdateResult(declaration);
			AssertLineUpdateResult(declaration);
		}

		void AssertImportSADNumbersUpdateResult(EMCSJobDeclaration declaration)
		{
			AssertEquals("Have 2 ImportSADNumbers", 2, declaration.ImportSADNumbers.Count);
			AssertEquals("1st Sad Number", "SAD001", declaration.ImportSADNumbers[0].CSI_Description);
			AssertEquals("2st Sad Number", "SAD002", declaration.ImportSADNumbers[1].CSI_Description);
		}

		void AssertDocumentsUpdateResult(EMCSJobDeclaration declaration)
		{
			AssertEquals("Have 2 Documents", 2, declaration.Documents.Count);
			AssertEquals("1st Document.CSI_ReferenceNumber", "Ref001", declaration.Documents[0].CSI_ReferenceNumber);
			AssertEquals("1st Document.CSI_Description", "DC001", declaration.Documents[0].CSI_Description);
			AssertEquals("1st Document.CSI_SubType", "TP001", declaration.Documents[0].CSI_SubType);

			AssertEquals("2st Document.CSI_ReferenceNumber", "Ref002", declaration.Documents[1].CSI_ReferenceNumber);
			AssertEquals("2st Document.CSI_Description", "DC002", declaration.Documents[1].CSI_Description);
			AssertEquals("2st Document.CSI_SubType", "TP002", declaration.Documents[1].CSI_SubType);
		}

		void AssertTransportDetailsUpdateResult(EMCSJobDeclaration declaration)
		{
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
		}

		void AssertGuarantorUpdateResult(EMCSJobDeclaration declaration)
		{
			AssertEquals("Address override", true, declaration.OwnerDocumentaryAddress.E2_AddressOverride);
			AssertEquals("E2_GovRegNumType", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, declaration.OwnerDocumentaryAddress.E2_GovRegNumType);
			AssertEquals("E2_GovRegNum", "GB001", declaration.OwnerDocumentaryAddress.E2_GovRegNum);
			AssertEquals("E2_RN_NKCountryCode", "GB", declaration.OwnerDocumentaryAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_City", "London", declaration.OwnerDocumentaryAddress.E2_City);
			AssertEquals("E2_Address1", "GP Address 1", declaration.OwnerDocumentaryAddress.E2_Address1);
			AssertEquals("E2_Postcode", "0001", declaration.OwnerDocumentaryAddress.E2_Postcode);
			AssertEquals("E2_CompanyName", "Guarantor Party 1", declaration.OwnerDocumentaryAddress.E2_CompanyName);
		}

		void AssertConsigneeUpdateResult(EMCSJobDeclaration declaration)
		{
			AssertEquals("Address override", true, declaration.ImporterDocumentaryAddress.E2_AddressOverride);
			AssertEquals("E2_GovRegNumType", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, declaration.ImporterDocumentaryAddress.E2_GovRegNumType);
			AssertEquals("E2_GovRegNum", "GB001", declaration.ImporterDocumentaryAddress.E2_GovRegNum);
			AssertEquals("E2_RN_NKCountryCode", "GB", declaration.ImporterDocumentaryAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_City", "London", declaration.ImporterDocumentaryAddress.E2_City);
			AssertEquals("E2_Address1", "CP Address 1", declaration.ImporterDocumentaryAddress.E2_Address1);
			AssertEquals("E2_Postcode", "0001", declaration.ImporterDocumentaryAddress.E2_Postcode);
			AssertEquals("E2_CompanyName", "Consignee Party 1", declaration.ImporterDocumentaryAddress.E2_CompanyName);
		}

		void AssertConsignorUpdateResult(EMCSJobDeclaration declaration)
		{
			AssertEquals("Address override", true, declaration.SupplierDocumentaryAddress.E2_AddressOverride);
			AssertEquals("E2_GovRegNumType", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, declaration.SupplierDocumentaryAddress.E2_GovRegNumType);
			AssertEquals("E2_GovRegNum", "GB002", declaration.SupplierDocumentaryAddress.E2_GovRegNum);
			AssertEquals("E2_RN_NKCountryCode", "GB", declaration.SupplierDocumentaryAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_City", "London", declaration.SupplierDocumentaryAddress.E2_City);
			AssertEquals("E2_Address1", "CRP Address 1", declaration.SupplierDocumentaryAddress.E2_Address1);
			AssertEquals("E2_Postcode", "0001", declaration.SupplierDocumentaryAddress.E2_Postcode);
			AssertEquals("E2_CompanyName", "Consignor Party 1", declaration.SupplierDocumentaryAddress.E2_CompanyName);
		}

		void AssertPlaceOfDispatchUpdateResult(EMCSJobDeclaration declaration)
		{
			AssertEquals("Address override", true, declaration.DispatchWarehouseDocumentaryAddress.E2_AddressOverride);
			AssertEquals("E2_GovRegNumType", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, declaration.DispatchWarehouseDocumentaryAddress.E2_GovRegNumType);
			AssertEquals("E2_GovRegNum", "GBW001", declaration.DispatchWarehouseDocumentaryAddress.E2_GovRegNum);
			AssertEquals("E2_RN_NKCountryCode", "GB", declaration.DispatchWarehouseDocumentaryAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_City", "London", declaration.DispatchWarehouseDocumentaryAddress.E2_City);
			AssertEquals("E2_Address1", "PRD Address 1", declaration.DispatchWarehouseDocumentaryAddress.E2_Address1);
			AssertEquals("E2_Postcode", "0001", declaration.DispatchWarehouseDocumentaryAddress.E2_Postcode);
			AssertEquals("E2_CompanyName", "PartyPlaceOfDispatch Party 1", declaration.DispatchWarehouseDocumentaryAddress.E2_CompanyName);
		}

		void AssertDeliveryPlaceUpdateResult(EMCSJobDeclaration declaration)
		{
			AssertEquals("Address override", true, declaration.DestinationWarehouseDocumentaryAddress.E2_AddressOverride);
			AssertEquals("E2_GovRegNumType", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, declaration.DestinationWarehouseDocumentaryAddress.E2_GovRegNumType);
			AssertEquals("E2_GovRegNum", "GB002", declaration.DestinationWarehouseDocumentaryAddress.E2_GovRegNum);
			AssertEquals("E2_RN_NKCountryCode", "GB", declaration.DestinationWarehouseDocumentaryAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_City", "London", declaration.DestinationWarehouseDocumentaryAddress.E2_City);
			AssertEquals("E2_Address1", "DP Address 1", declaration.DestinationWarehouseDocumentaryAddress.E2_Address1);
			AssertEquals("E2_Postcode", "0001", declaration.DestinationWarehouseDocumentaryAddress.E2_Postcode);
			AssertEquals("E2_CompanyName", "DeliveryPlace Party 1", declaration.DestinationWarehouseDocumentaryAddress.E2_CompanyName);
		}

		void AssertTransportArrangerUpdateResult(EMCSJobDeclaration declaration)
		{
			AssertEquals("Address override", true, declaration.CarrierAgentDocumentaryAddress.E2_AddressOverride);
			AssertEquals("E2_GovRegNumType", GermanyOrgCusCodeInfo.OrgCusCodes.UST, declaration.CarrierAgentDocumentaryAddress.E2_GovRegNumType);
			AssertEquals("E2_GovRegNum", "002", declaration.CarrierAgentDocumentaryAddress.E2_GovRegNum);
			AssertEquals("E2_RN_NKCountryCode", "DE", declaration.CarrierAgentDocumentaryAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_City", "Berlin", declaration.CarrierAgentDocumentaryAddress.E2_City);
			AssertEquals("E2_Address1", "TAP Address 1", declaration.CarrierAgentDocumentaryAddress.E2_Address1);
			AssertEquals("E2_Postcode", "0001", declaration.CarrierAgentDocumentaryAddress.E2_Postcode);
			AssertEquals("E2_CompanyName", "TransportArranger Party 1", declaration.CarrierAgentDocumentaryAddress.E2_CompanyName);
		}

		void AssertFirstTransporterUpdateResult(EMCSJobDeclaration declaration)
		{
			AssertEquals("Address override", true, declaration.TransporterDocumentaryAddress.E2_AddressOverride);
			AssertEquals("E2_GovRegNumType", GermanyOrgCusCodeInfo.OrgCusCodes.UST, declaration.TransporterDocumentaryAddress.E2_GovRegNumType);
			AssertEquals("E2_GovRegNum", "003", declaration.TransporterDocumentaryAddress.E2_GovRegNum);
			AssertEquals("E2_RN_NKCountryCode", "DE", declaration.TransporterDocumentaryAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_City", "Berlin", declaration.TransporterDocumentaryAddress.E2_City);
			AssertEquals("E2_Address1", "FTP Address 1", declaration.TransporterDocumentaryAddress.E2_Address1);
			AssertEquals("E2_Postcode", "0001", declaration.TransporterDocumentaryAddress.E2_Postcode);
			AssertEquals("E2_CompanyName", "FirstTransporter Party 1", declaration.TransporterDocumentaryAddress.E2_CompanyName);
		}

		abstract protected void AssertLineUpdateResult(EMCSJobDeclaration declaration);
	}
}
