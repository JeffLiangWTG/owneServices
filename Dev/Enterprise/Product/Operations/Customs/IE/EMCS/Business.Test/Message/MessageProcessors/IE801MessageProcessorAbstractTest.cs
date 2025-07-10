using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.EU.EMCS.Registry;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.EMCS.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE801MessageProcessor))]
	abstract class IE801MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE801MessageProcessor, IIE801>
	{
		protected override ZString MessageType => EMCSIncomingMessageTypeList.Codes.IE801;

		protected override ZString MessageFriendlyName => "EMCS IE801 Message Processor";

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

			var expectedInterpretation = "Electronic Administrative Document received.<br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"></table><br />e-AD Number : MRN1234567 <br />";
			AssertContains("Interpretation should be set", expectedInterpretation, incomingMessage.EM_MessageInterpretation);

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

			MessageProcessorNotificationTestHelper.AssertEmail(
				"EMCS e-AD registered",
				new[] { "Your EMCS Declaration for Job E00000810 has been registered. For details please follow the Link to the Job." },
				new string[] { "staff1@where.com" });
		}

		protected override void AssertEndToEndProcessing()
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

			MessageProcessorNotificationTestHelper.AssertEmail(
				"EMCS e-AD registered",
				new[] { "Your EMCS Declaration for Job E00000810 has been registered. For details please follow the Link to the Job." },
				new string[] { "staff1@where.com" });
		}

		public void TestEndToEndProcessing_WhenConsigneeDeclarationMatched()
		{
			base.CreateSetupData();
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			using (incomingMessage.Factory.AddDisposableService())
			{
				var user = Factory.NewWithValidTestData<GlbStaff>();
				user.GS_EmailAddress = "staff1@where.com";
				var glbGroup = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, GlbGroup.AllStaffGroupCode);
				var emailGroupPK = glbGroup.PK;
				Factory.Save();
				var registryItem = new EmcsGroupNotification(Core.Constants.EmailTo.NominatedGroup, emailGroupPK);
				using (EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, registryItem))
				{
					Processor.PreProcessMessage(incomingMessage);
					Processor.ProcessMessage(incomingMessage);
					Factory.Save();
				}

				CombineAssertions("Process", () =>
				{
					AssertEquals("JE_MessageStatus should have been set RCV.", EDIMessage.Status.Received, declaration.JE_MessageStatus);
					AssertEquals("JE_EntryStatus should have been set REG.", EntryStatusList.Codes.REG, declaration.JE_EntryStatus);
					AssertEquals("EM_Status should have been set PRS.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);

					AssertDeclarationUpdateResult(declaration, incomingMessage);

					MessageProcessorNotificationTestHelper.AssertEmail(
						"Incoming EMCS e-AD",
						new[] { "An incoming EMCS Declaration created Job E00000810. For details please follow the link to the Job." },
						new string[] { "staff1@where.com" });
				});
			}
		}

		public void TestEndToEndProcessing_WhenNoDeclarationMatched()
		{
			var incomingMessage = CreateNewIncomingMessage();
			using (incomingMessage.Factory.AddDisposableService())
			{
				var credential1 = Factory.NewWithValidTestData<EMCSGlbCompanyCredential>();
				credential1.GP_MailBoxID = "Certificate Identifier1";
				var credential2 = Factory.NewWithValidTestData<EMCSGlbCompanyCredential>();
				credential2.GP_MailBoxID = "Certificate Identifier2";
				incomingMessage.EM_GP = credential1.PK;
				var user = Factory.NewWithValidTestData<GlbStaff>();
				user.GS_EmailAddress = "staff1@where.com";
				var glbGroup = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, GlbGroup.AllStaffGroupCode);
				var emailGroupPK = glbGroup.PK;
				Factory.Save();

				var messageBranchPK = incomingMessage.EM_GB;
				var registryItem = new EmcsGroupNotification(Core.Constants.EmailTo.NominatedGroup, emailGroupPK);
				using (EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.SetTemporaryValue(Guid.Empty, messageBranchPK.ToGuid(), Guid.Empty, registryItem))
				{
					Processor.PreProcessMessage(incomingMessage);
					Processor.ProcessMessage(incomingMessage);
					Factory.Save();
				}

				CombineAssertions("Process", () =>
				{
					var declaration = (EMCSJobDeclaration)incomingMessage.EM_LinkedObject;
					AssertNotNull("A new declaration should be created.", declaration);
					AssertEquals("New declaration is in the message branch", messageBranchPK, declaration.JE_GB);

					AssertEquals("JE_CustomsProfile should have been linked with the credential", "Certificate Identifier1", declaration.JE_CustomsProfile);
					AssertEquals("JE_DeclarantType should have been set Consignee", EMCSEntryTypeList.Codes.Consignee, declaration.JE_DeclarantType);
					AssertEquals("JE_MessageStatus should have been set RCV.", EDIMessage.Status.Received, declaration.JE_MessageStatus);
					AssertEquals("JE_EntryStatus should have been set REG.", EntryStatusList.Codes.REG, declaration.JE_EntryStatus);
					AssertEquals("EM_Status should have been set PRS.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);

					AssertDeclarationUpdateResult(declaration, incomingMessage);

					MessageProcessorNotificationTestHelper.AssertEmail(
						"Incoming EMCS e-AD",
						new[] { "An incoming EMCS Declaration created Job E00000001. For details please follow the link to the Job." },
						new string[] { "staff1@where.com" });
				});
			}
		}

		public void TestEndToEndProcessing_WhenBranchCompanyMissMatched()
		{
			base.CreateSetupData();
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			using (incomingMessage.Factory.AddDisposableService())
			{
				var credential1 = Factory.NewWithValidTestData<EMCSGlbCompanyCredential>();
				credential1.GP_MailBoxID = "Certificate Identifier1";
				var credential2 = Factory.NewWithValidTestData<EMCSGlbCompanyCredential>();
				credential2.GP_MailBoxID = "Certificate Identifier2";
				incomingMessage.EM_GP = credential1.PK;

				var user = Factory.NewWithValidTestData<GlbStaff>();
				user.GS_EmailAddress = "staff1@where.com";
				var glbGroup = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, GlbGroup.AllStaffGroupCode);
				var emailGroupPK = glbGroup.PK;
				var otherCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
				incomingMessage.EM_GB = otherCompanyBranch.PK;
				Factory.Save();

				var registryItem = new EmcsGroupNotification(Core.Constants.EmailTo.NominatedGroup, emailGroupPK);
				using (EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.SetTemporaryValue(Guid.Empty, otherCompanyBranch.PK.ToGuid(), Guid.Empty, registryItem))
				{
					Processor.PreProcessMessage(incomingMessage);
					Processor.ProcessMessage(incomingMessage);
					Factory.Save();
				}

				CombineAssertions("Process", () =>
				{
					AssertNotEquals("Incoming message is for a different company to the outgoing message", outgoingMessage.Branch.GB_GC, incomingMessage.Branch.GB_GC);

					var declaration = (EMCSJobDeclaration)incomingMessage.EM_LinkedObject;
					AssertNotNull("A new declaration should be created.", declaration);
					AssertEquals("New declaration is in other company", otherCompanyBranch.GB_GC, declaration.CompanyPK);

					AssertEquals("JE_CustomsProfile should have been linked with the credential", "Certificate Identifier1", declaration.JE_CustomsProfile);
					AssertEquals("JE_DeclarantType should have been set Consignee", EMCSEntryTypeList.Codes.Consignee, declaration.JE_DeclarantType);
					AssertEquals("JE_MessageStatus should have been set RCV.", EDIMessage.Status.Received, declaration.JE_MessageStatus);
					AssertEquals("JE_EntryStatus should have been set REG.", EntryStatusList.Codes.REG, declaration.JE_EntryStatus);
					AssertEquals("EM_Status should have been set PRS.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);

					AssertDeclarationUpdateResult(declaration, incomingMessage);

					MessageProcessorNotificationTestHelper.AssertEmail(
						"Incoming EMCS e-AD",
						new[] { "An incoming EMCS Declaration created Job E00000001. For details please follow the link to the Job." },
						new string[] { "staff1@where.com" });
				});
			}
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
			AssertEquals(new ZDateTime(2022, 8, 30, 13, 30, 0), declaration.JE_DateAtOrigin);
			AssertEquals("1", declaration.ZG_OriginType);
			AssertEquals("B000222547896254786321", declaration.JE_OwnerRef);
			AssertEquals("1", declaration.InvoiceNumber);
			AssertEquals(new ZDateTime(2022, 8, 29), declaration.InvoiceDate);
			AssertEquals("12", declaration.ZG_CCTMSA);
			AssertEquals("CE001", declaration.ZG_CertOfExemption);
			AssertEquals("3", declaration.ZG_GuarantorType);
			AssertEquals("AIR", declaration.JE_TransportMode);
			AssertEquals("CI001", declaration.SpecialInstructions);

			var customsOffice = declaration.CustomsOffices.Cast<OfficeCode>().Single(x => x.CY_Code == OfficeCodes_EMCS.Codes.OfficeOfDestination);
			AssertEquals("OfficeOfDestination Office Code", ZString.Empty, customsOffice.CY_Data);
			var officeOfDispatch = declaration.CustomsOffices.Cast<OfficeCode>().Single(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDispatch);
			AssertEquals("OfficeOfDispatch Office Code", "DIO001", officeOfDispatch.CY_Data);
			var officeOfDelivery = declaration.CustomsOffices.Cast<OfficeCode>().Single(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDelivery);
			AssertEquals("OfficeOfDelivery Office Code", "DPCO001", officeOfDelivery.CY_Data);
			var competentAuthorityOfDispatch = declaration.CustomsOffices.Cast<OfficeCode>().Single(x => x.CY_Code == EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch);
			AssertEquals("CompetentAuthorityOfDispatch Office Code", "CADO001", competentAuthorityOfDispatch.CY_Data);

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

			var lines = declaration.FilteredInvoiceLines;
			AssertEquals("Created line numbers", 4, lines.Count);
			AssertLineUpdateResult(lines.Find(l => l.JI_LineNo == 1).Single(), 1, "SN001", "Shipping Marks 1", 5);
			AssertLineUpdateResult(lines.Find(l => l.JI_LineNo == 2).Single(), 2, "SN002", "Shipping Marks 1", 5);
			AssertLineUpdateResult(lines.Find(l => l.JI_LineNo == 3).Single(), 3);
			AssertLineUpdateResult(lines.Find(l => l.JI_LineNo == 4).Single(), 4, "SN004", "Shipping Marks 2", 4);
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
			AssertEquals("E2_GovRegNum", "TN001", declaration.OwnerDocumentaryAddress.E2_GovRegNum);
			AssertEquals("E2_RN_NKCountryCode", "IE", declaration.OwnerDocumentaryAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_City", "Dublin", declaration.OwnerDocumentaryAddress.E2_City);
			AssertEquals("E2_Address1", "GP Address 1", declaration.OwnerDocumentaryAddress.E2_Address1);
			AssertEquals("E2_Postcode", "0001", declaration.OwnerDocumentaryAddress.E2_Postcode);
			AssertEquals("E2_CompanyName", "Guarantor Party 1", declaration.OwnerDocumentaryAddress.E2_CompanyName);
		}

		void AssertConsigneeUpdateResult(EMCSJobDeclaration declaration)
		{
			AssertEquals("Address override", true, declaration.ImporterDocumentaryAddress.E2_AddressOverride);
			AssertEquals("E2_GovRegNumType", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, declaration.ImporterDocumentaryAddress.E2_GovRegNumType);
			AssertEquals("E2_GovRegNum", "TI001", declaration.ImporterDocumentaryAddress.E2_GovRegNum);
			AssertEquals("E2_RN_NKCountryCode", "IE", declaration.ImporterDocumentaryAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_City", "Dublin", declaration.ImporterDocumentaryAddress.E2_City);
			AssertEquals("E2_Address1", "CP Address 1", declaration.ImporterDocumentaryAddress.E2_Address1);
			AssertEquals("E2_Postcode", "0001", declaration.ImporterDocumentaryAddress.E2_Postcode);
			AssertEquals("E2_CompanyName", "Consignee Party 1", declaration.ImporterDocumentaryAddress.E2_CompanyName);
		}

		void AssertConsignorUpdateResult(EMCSJobDeclaration declaration)
		{
			AssertEquals("Address override", true, declaration.SupplierDocumentaryAddress.E2_AddressOverride);
			AssertEquals("E2_GovRegNumType", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, declaration.SupplierDocumentaryAddress.E2_GovRegNumType);
			AssertEquals("E2_GovRegNum", "TN002", declaration.SupplierDocumentaryAddress.E2_GovRegNum);
			AssertEquals("E2_RN_NKCountryCode", "IE", declaration.SupplierDocumentaryAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_City", "Dublin", declaration.SupplierDocumentaryAddress.E2_City);
			AssertEquals("E2_Address1", "CRP Address 1", declaration.SupplierDocumentaryAddress.E2_Address1);
			AssertEquals("E2_Postcode", "0001", declaration.SupplierDocumentaryAddress.E2_Postcode);
			AssertEquals("E2_CompanyName", "Consignor Party 1", declaration.SupplierDocumentaryAddress.E2_CompanyName);
		}

		void AssertPlaceOfDispatchUpdateResult(EMCSJobDeclaration declaration)
		{
			AssertEquals("Address override", true, declaration.DispatchWarehouseDocumentaryAddress.E2_AddressOverride);
			AssertEquals("E2_GovRegNumType", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, declaration.DispatchWarehouseDocumentaryAddress.E2_GovRegNumType);
			AssertEquals("E2_GovRegNum", "RTW001", declaration.DispatchWarehouseDocumentaryAddress.E2_GovRegNum);
			AssertEquals("E2_RN_NKCountryCode", "IE", declaration.DispatchWarehouseDocumentaryAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_City", "Dublin", declaration.DispatchWarehouseDocumentaryAddress.E2_City);
			AssertEquals("E2_Address1", "PRD Address 1", declaration.DispatchWarehouseDocumentaryAddress.E2_Address1);
			AssertEquals("E2_Postcode", "0001", declaration.DispatchWarehouseDocumentaryAddress.E2_Postcode);
			AssertEquals("E2_CompanyName", "PartyPlaceOfDispatch Party 1", declaration.DispatchWarehouseDocumentaryAddress.E2_CompanyName);
		}

		void AssertDeliveryPlaceUpdateResult(EMCSJobDeclaration declaration)
		{
			AssertEquals("Address override", true, declaration.DestinationWarehouseDocumentaryAddress.E2_AddressOverride);
			AssertEquals("E2_GovRegNumType", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, declaration.DestinationWarehouseDocumentaryAddress.E2_GovRegNumType);
			AssertEquals("E2_GovRegNum", "TI002", declaration.DestinationWarehouseDocumentaryAddress.E2_GovRegNum);
			AssertEquals("E2_RN_NKCountryCode", "IE", declaration.DestinationWarehouseDocumentaryAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_City", "Dublin", declaration.DestinationWarehouseDocumentaryAddress.E2_City);
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

		void AssertLineUpdateResult(EMCSJobComInvoiceLine invoiceLine, ZShort lineNumber, string packageSealNumber = null, string packageMarksAndNumbers = null, int packageUnitCount = 0)
		{
			AssertEquals("JI_LineNo", lineNumber, invoiceLine.JI_LineNo);
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
			AssertEquals("ZG_WineCountryOrigin", "IE", invoiceLine.ZG_WineCountryOrigin);
			AssertEquals("JI_WineDetailsComments", "WPOI001", invoiceLine.JI_WineDetailsComments);
			AssertEquals("OperationCodeData1.CY_Code", "WO001", invoiceLine.OperationCodeDataCollection[0].CY_Code);
			AssertEquals("OperationCodeData2.CY_Code", "WO002", invoiceLine.OperationCodeDataCollection[1].CY_Code);

			if (packageSealNumber != null)
			{
				var package = invoiceLine.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().Single(p => p.SealNumber == packageSealNumber);
				AssertEquals("Package UQ", "CT", package.UnitType);
				AssertEquals("Package Quantity", packageUnitCount, package.UnitCount);
				AssertEquals("B5_SealNumber", packageSealNumber, package.SealNumber);
				AssertEquals("B5_SealComment", "SC001", package.SealComment);
				AssertEquals("B5_MarksAndNumbers", packageMarksAndNumbers, package.MarksAndNumbers);
				AssertEquals("Linked to the line", true, package.IsForInvoiceLine);
				AssertEquals("IsMainPack", true, invoiceLine.ZG_IsMainPack);
			}
		}
	}
}
