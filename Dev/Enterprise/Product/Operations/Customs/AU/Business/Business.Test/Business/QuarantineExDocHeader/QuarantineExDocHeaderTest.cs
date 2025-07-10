using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineExDocHeader))]
	sealed class QuarantineExDocHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPropertyAttributes()
		{
			var docHeader = Factory.New<QuarantineExDocHeader>();
			var loadingDateResourceStringData = DataBoundResourceStrings.GetDataForProperty(docHeader.QH_LoadingDateInfo);
			CombineAssertions(() =>
			{
				AssertEquals("QH_LoadingDate.Caption", "Date", loadingDateResourceStringData.Caption);
				AssertEquals("QH_LoadingDate.FullDescription", "Date in which the products were loaded.", loadingDateResourceStringData.FullDescription);
			});
		}

		public void TestAttachingInvoiceWorkCorrectly()
		{
			var invoice = Factory.New<JobComInvoiceHeaderForTest>();
			_ = new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			var header = invoice.QuarantineExDocHeaders.AddNew();
			header.QH_ApprovalNumber = "NK";
			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			invoice = Factory.Load<JobComInvoiceHeaderForTest>(invoice.PK);
			declaration.Invoices.Add(invoice);
			AssertEquals(1, invoice.QuarantineExDocHeaders.Count);
		}

		public void TestIsChangeAllowed_IsBypassedForNEXDOC()
		{
			var quarantineHeader = (QuarantineExDocHeader)GetNewBusinessObject();
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			AssertEquals(false, quarantineHeader.IsChangeAllowed(EXDOCDataFields.InspectionPort));

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals("NEXDOCS is to skip the EXDOC permision matrix", true, quarantineHeader.IsChangeAllowed(EXDOCDataFields.InspectionPort));
			}
		}

		public void TestPopulateValuesFromRegCode()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			org.CustomsCodes.AddNew(OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExternalID, "123").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			org.CustomsCodes.AddNew(OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber, "456").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;

			org.CustomsCodes.AddNew(OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser, "789").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			org.CustomsCodes.AddNew(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, "012").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;

			Factory.Save();

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;

			void AssertSetDefaultValuesFromRegCode(bool isNEXDOCActived, string expectedUserCusCode, string expectedExporterCusCode)
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, isNEXDOCActived))
				{
					quarantineHeader.QH_OH_PrintLocationOrganisation = ZGuid.Empty;
					quarantineHeader.QH_OH_PrintLocationOrganisation = org.PK;
					quarantineHeader.QH_OH_ForwardLocationOrganisation = org.PK;
					quarantineHeader.QH_OH_TransferEDIUserLocationOrganisation = org.PK;
					quarantineHeader.QH_OH_TransferExporterLocationOrganisation = org.PK;

					var userNumber = org.CustomsCodes.GetCustomsRegNo(expectedUserCusCode, Core.Constants.CountryCodes.Australia);
					var exporterNumber = org.CustomsCodes.GetCustomsRegNo(expectedExporterCusCode, Core.Constants.CountryCodes.Australia);

					CombineAssertions(() =>
					{
						AssertEquals("UserIdentifierCusCode", expectedUserCusCode, quarantineHeader.UserIdentifierCusCode);
						AssertEquals("ExporterNumberCusCode", expectedExporterCusCode, quarantineHeader.ExporterNumberCusCode);

						AssertEquals("QH_CertificateRequiredLocation", userNumber, quarantineHeader.QH_CertificateRequiredLocation);
						AssertEquals("QH_ForwardeeEDIUserIdentifier", userNumber, quarantineHeader.QH_ForwardeeEDIUserIdentifier);
						AssertEquals("QH_TransfereeEDIUserIdentifier", userNumber, quarantineHeader.QH_TransfereeEDIUserIdentifier);
						AssertEquals("QH_TransfereeExporterNumber", exporterNumber, quarantineHeader.QH_TransfereeExporterNumber);
					});
				}
			}

			AssertSetDefaultValuesFromRegCode(false, OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser, OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber);
			AssertSetDefaultValuesFromRegCode(true, OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExternalID, OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber);
		}

		public void TestINEXDOCMessageParent()
		{
			var quarantineHeader = (QuarantineExDocHeader)GetNewBusinessObject();
			quarantineHeader.QH_RequestForPermitNumber = "TEST";

			var messageParent = (INEXDOCMessageParent)quarantineHeader;

			AssertEquals("TEST", messageParent.RexNumber);
			AssertSame(quarantineHeader.Factory, messageParent.Factory);
		}

		public void TestQH_ProduceTypeDescription()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_OTH, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				var quarantineHeader = (QuarantineExDocHeader)GetNewBusinessObject();

				quarantineHeader.QH_ProduceType = ZString.Empty;
				AssertEquals(ZString.Empty, quarantineHeader.QH_ProduceTypeDescription);

				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				AssertEquals("DAIRY", quarantineHeader.QH_ProduceTypeDescription);

				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
				AssertEquals("EGGS", quarantineHeader.QH_ProduceTypeDescription);

				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
				AssertEquals("FISH", quarantineHeader.QH_ProduceTypeDescription);

				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
				AssertEquals("GRAINS AND PLANTS", quarantineHeader.QH_ProduceTypeDescription);

				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
				AssertEquals("HORTICULTURE", quarantineHeader.QH_ProduceTypeDescription);

				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
				AssertEquals("INEDIBLE MEAT", quarantineHeader.QH_ProduceTypeDescription);

				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
				AssertEquals("MEAT", quarantineHeader.QH_ProduceTypeDescription);

				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;
				AssertEquals("OTHER GOODS", quarantineHeader.QH_ProduceTypeDescription);

				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
				AssertEquals("SKINS AND HIDES", quarantineHeader.QH_ProduceTypeDescription);

				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
				AssertEquals("WOOL", quarantineHeader.QH_ProduceTypeDescription);

				AssertEquals(10, (new EXDOCCommodityCodes()).Count);
			}
		}

		public void TestChangeProduceTypeToWolOrSkn()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var quarantineHeader = (QuarantineExDocHeader)GetNewBusinessObject();
			SetTestData(quarantineHeader);
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			TestResult("SkinsAndHides", quarantineHeader);

			SetTestData(quarantineHeader);
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			TestResult("Wool", quarantineHeader);

			void TestResult(string message, QuarantineExDocHeader quarantineHeader)
			{
				CombineAssertions(message, () =>
				{
					Assert("QH_AuthorisationLocation", quarantineHeader.QH_AuthorisationLocation.IsEmpty);
					Assert("QH_AuthorisationEstablishment", quarantineHeader.QH_AuthorisationEstablishment.IsEmpty);
					Assert("QH_OA_AuthorisationEstablishment", quarantineHeader.QH_OA_AuthorisationEstablishment.IsEmpty);
					Assert("QH_AuthorisationDate", quarantineHeader.QH_AuthorisationDate.IsEmpty);
					Assert("QH_AuthorisationFlag", !quarantineHeader.QH_AuthorisationFlag);
					Assert("QH_AuthorisationComments", quarantineHeader.QH_AuthorisationComments.IsEmpty);
				});
			}
			void SetTestData(QuarantineExDocHeader quarantineHeader)
			{
				quarantineHeader.QH_AuthorisationLocation = EXDOCCodeOrganisation.Codes.Organisation;
				quarantineHeader.QH_AuthorisationEstablishment = "123";
				quarantineHeader.QH_OA_AuthorisationEstablishment = org.MainAddress.PK;
				quarantineHeader.QH_AuthorisationDate = ZDate.BrettsBirthday;
				quarantineHeader.QH_AuthorisationFlag = true;
				quarantineHeader.QH_AuthorisationComments = "Comments";
			}
		}

		public void TestIsNEXDOCSActive()
		{
			var quarantineHeader = (QuarantineExDocHeader)GetNewBusinessObject();

			AssertEquals(false, quarantineHeader.IsNEXDOCSActive);

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			AssertEquals(true, quarantineHeader.IsNEXDOCSActive);

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			AssertEquals(true, quarantineHeader.IsNEXDOCSActive);

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			AssertEquals(true, quarantineHeader.IsNEXDOCSActive);

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			AssertEquals(false, quarantineHeader.IsNEXDOCSActive);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_GRN, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals(true, quarantineHeader.IsNEXDOCSActive);
			}

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			AssertEquals(false, quarantineHeader.IsNEXDOCSActive);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_HOR, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals(true, quarantineHeader.IsNEXDOCSActive);
			}

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			AssertEquals(false, quarantineHeader.IsNEXDOCSActive);

			using (AUCustomsDataRegistry.Instance.EnableNEXDOCForInedibleMeat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, quarantineHeader.IsNEXDOCSActive);
			}

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			AssertEquals(false, quarantineHeader.IsNEXDOCSActive);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals(true, quarantineHeader.IsNEXDOCSActive);
			}

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;
			AssertEquals(false, quarantineHeader.IsNEXDOCSActive);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_OTH, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals(true, quarantineHeader.IsNEXDOCSActive);
			}

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			AssertEquals(true, quarantineHeader.IsNEXDOCSActive);

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			AssertEquals(true, quarantineHeader.IsNEXDOCSActive);

			var exdocMessage = Factory.New<RFPMessage>();
			exdocMessage.EM_ReceiveTransmit = "RCV";
			quarantineHeader.Messages.Add(exdocMessage);
			AssertEquals("Should not be active when header has a legacy EXDOC message attached.", false, quarantineHeader.IsNEXDOCSActive);
		}

		public void TestIsNEXDOCSActive_StandaloneInvoice()
		{
			var fakeDeclaration = Factory.New<JobDeclaration>();
			fakeDeclaration.MakeNonPersistent();

			var standaloneInvoice = Factory.New<JobComInvoiceHeader>();
			standaloneInvoice.JZ_JE = fakeDeclaration.PK;
			standaloneInvoice.JZ_MessageType = AUJobMessageTypeList.Codes.Quarantine;

			var quarantineHeader = standaloneInvoice.QuarantineExDocHeader;
			Assert("Should be always active on a standalone invoice header.", quarantineHeader.IsNEXDOCSActive);

			var exdocMessage = Factory.New<RFPMessage>();
			exdocMessage.EM_ReceiveTransmit = "RCV";
			quarantineHeader.Messages.Add(exdocMessage);
			Assert("Should be always active on a standalone invoice header.", quarantineHeader.IsNEXDOCSActive);
		}

		public void TestAcknowledgements()
		{
			var acks = quarantineHeader.Acknowledgements;
			AssertType<QuarantineExDocRexAcknowledgementCollection>(acks);
			AssertSame("Should use the same collection once it has been loaded.", acks, quarantineHeader.Acknowledgements);

			AssertNoExceptionThrown(() => _ = (ICusCodeDataTypeSupporter)quarantineHeader);
		}

		public void TestAmendPermissionMatrix()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineHeader.QH_ProduceTypeInfo);
			AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineHeader.QH_InspectionRequestedDateInfo);
			AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineHeader.QH_AuthorisedStartDateInfo);
			AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineHeader.QH_AuthorisedEndDateInfo);
			AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineHeader.QH_AuthorisationEstablishmentInfo);
			AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineHeader.QH_AuthorisingOfficerIDInfo);
			AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineHeader.QH_DecOfComplianceInfo);
			AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineHeader.QH_ImportedProductFlagInfo);
			AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineHeader.QH_RL_NKBorderInspectionPortInfo);
			AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineHeader.QH_ShipsStoresInfo);
			AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineHeader.QH_StorageEstablishmentInfo);
			AssertAmendPermissionReadOnlyStatus(quarantineHeader, quarantineHeader.QH_TrueAndCompleteIndicatorInfo);
		}

		public void TestHasQuarantineHeaderBeenAccepted()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;

			var sender = declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			var rfpmmm = new RFPMultiMessageManager(declaration, EXDOCMessageTypeCodes.Codes.ORD);
			rfpmmm.SendMessages(sender);

			quarantineHeader.QH_RequestForPermitNumber = "5";
			var msg = quarantineHeader.Messages.AddNew();
			msg.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			msg.EM_Status = EDIMessageStatusList.Codes.Received;
			msg.EM_MessageType = EDIMessageStatusList.Codes.Acknowledged;

			declaration.JE_MessageStatus = EDIMessageStatusList.Codes.Received;
			quarantineHeader.QH_QuarantineMessageMaxLine = ZInt.Zero;
			Assert(quarantineHeader.HasQuarantineHeaderBeenAccepted());

			//Testing RFP Number
			quarantineHeader.QH_RequestForPermitNumber = ZString.Empty;
			Assert(!quarantineHeader.HasQuarantineHeaderBeenAccepted());

			quarantineHeader.QH_RequestForPermitNumber = "5";
			Assert(quarantineHeader.HasQuarantineHeaderBeenAccepted());

			//Testing IsWaitingForResponse
			declaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			Assert(!quarantineHeader.HasQuarantineHeaderBeenAccepted());

			declaration.JE_MessageStatus = EDIMessageStatusList.Codes.Received;
			Assert(quarantineHeader.HasQuarantineHeaderBeenAccepted());

			//Testing LastIncomingMessageType
			msg.EM_MessageType = EDIMessageStatusList.Codes.Rejected;
			Assert(!quarantineHeader.HasQuarantineHeaderBeenAccepted());

			msg.EM_MessageType = EDIMessageStatusList.Codes.Acknowledged;
			Assert(quarantineHeader.HasQuarantineHeaderBeenAccepted());

			//Testing LastOutgoingMessageType
			rfpmmm = new RFPMultiMessageManager(declaration, EXDOCMessageTypeCodes.Codes.LDG);
			rfpmmm.SendMessages(sender);
			quarantineHeader.QH_QuarantineMessageMaxLine = ZInt.Zero;
			declaration.JE_MessageStatus = EDIMessageStatusList.Codes.Received;
			Assert(quarantineHeader.HasQuarantineHeaderBeenAccepted());

			rfpmmm = new RFPMultiMessageManager(declaration, EXDOCMessageTypeCodes.Codes.RPL);
			quarantineHeader.ManualAmendmentReasonForMessaging = "Vendor Testing";
			rfpmmm.SendMessages(sender);
			AssertEquals("AmendmentResponseStatus", RFPMessage.Status.AwaitingResponse, quarantineHeader.AddInfo.ZH_AmendmentResponseStatus);
			quarantineHeader.QH_QuarantineMessageMaxLine = ZInt.Zero;
			declaration.JE_MessageStatus = EDIMessageStatusList.Codes.Received;
			Assert(quarantineHeader.HasQuarantineHeaderBeenAccepted());

			rfpmmm = new RFPMultiMessageManager(declaration, EXDOCMessageTypeCodes.Codes.CRQ);
			rfpmmm.SendMessages(sender);
			quarantineHeader.QH_QuarantineMessageMaxLine = ZInt.Zero;
			declaration.JE_MessageStatus = EDIMessageStatusList.Codes.Received;
			Assert(!quarantineHeader.HasQuarantineHeaderBeenAccepted());
		}

		public void TestHasExDocMessages()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var inv = dec.Invoices.AddNew();
			var quarantineInv = inv.QuarantineExDocHeader;
			AssertEquals(false, quarantineInv.HasExDocMessages());

			var msg = quarantineInv.Messages.AddNew();
			msg.EM_ApplicationCode = EDIInterchange.ApplicationCodes.EXDOC;
			AssertEquals(true, quarantineInv.HasExDocMessages());

			quarantineInv.Messages.RemoveAll();
			msg.EM_ApplicationCode = EDIInterchange.ApplicationCodes.AMS;
			quarantineInv.Messages.Add(msg);
			AssertEquals(false, quarantineInv.HasExDocMessages());
		}

		public void TestQH_DecOfCompliance_ReadOnly()
		{
			AssertReadOnlyStatus_IndicatorDeclarations(quarantineHeader, quarantineHeader.QH_DecOfComplianceInfo);
		}

		public void TestQH_TrueAndCompleteIndicator_ReadOnly()
		{
			AssertReadOnlyStatus_IndicatorDeclarations(quarantineHeader, quarantineHeader.QH_TrueAndCompleteIndicatorInfo);
		}

		public void TestQH_ImportedProductFlag_ReadOnly()
		{
			AssertReadOnlyStatus_IndicatorDeclarations(quarantineHeader, quarantineHeader.QH_ImportedProductFlagInfo);
		}

		public void TestUpdateQH_ImportedProductFlag()
		{
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			quarantineHeader.QH_ImportedProductFlag = EXDOCYesNoEmpty.Codes.Yes;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			AssertEquals(EXDOCYesNoEmpty.Codes.No, quarantineHeader.QH_ImportedProductFlag);

			quarantineHeader.QH_ImportedProductFlag = EXDOCYesNoEmpty.Codes.Yes;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			AssertEquals(EXDOCYesNoEmpty.Codes.Yes, quarantineHeader.QH_ImportedProductFlag);

			quarantineHeader.QH_ImportedProductFlag = ZString.Empty;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			AssertEquals(EXDOCYesNoEmpty.Codes.No, quarantineHeader.QH_ImportedProductFlag);

			quarantineHeader.QH_ImportedProductFlag = ZString.Empty;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			Assert(quarantineHeader.QH_ImportedProductFlag.IsEmpty);

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			AssertEquals(EXDOCYesNoEmpty.Codes.No, quarantineHeader.QH_ImportedProductFlag);
		}

		public void TestMessagesIsNotNull()
		{
			AssertNotNull("Message is not null", quarantineHeader.Messages);
		}

		public void TestMessages()
		{
			var message1 = Factory.New<RFPMessage>();
			message1.EM_LinkUniqueID = quarantineHeader.PK;
			var message2 = Factory.New<RFPMessage>();
			message2.EM_LinkUniqueID = quarantineHeader.PK;
			message2.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			AssertEquals("Header messages contains 2", 2, quarantineHeader.Messages.Count);
			AssertEquals("First message is transmit", EDIInterchange.Direction.Transmit, quarantineHeader.Messages[0].EM_ReceiveTransmit);
			AssertEquals("First message is transmit", EDIInterchange.Direction.Receive, quarantineHeader.Messages[1].EM_ReceiveTransmit);
		}

		public void TestUxmlMessages_JobDeclaration()
		{
			var declarationMessage = CreateInboundUXMLMessage(declaration);
			Factory.Save();

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;

			AssertEquals("Header messages contains 1", 1, quarantineHeader.Messages.Count);
			Assert("has declarationMessage", quarantineHeader.Messages.Contains(declarationMessage.PK));
		}

		public void TestUxmlMessages_Shipment()
		{
			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var shipmentMessage = CreateInboundUXMLMessage(shipment);
			Factory.Save();

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;

			AssertEquals("Header messages contains 1", 1, quarantineHeader.Messages.Count);
			Assert("has shipmentMessage", quarantineHeader.Messages.Contains(shipmentMessage.PK));
		}

		public void TestUxmlMessages_DeclarationAndShipment()
		{
			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var shipmentMessage = CreateInboundUXMLMessage(shipment);
			var declarationMessage = CreateInboundUXMLMessage(declaration);
			Factory.Save();

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;

			AssertEquals("Header messages", 2, quarantineHeader.Messages.Count);
			Assert("has shipmentMessage", quarantineHeader.Messages.Contains(shipmentMessage.PK));
			Assert("has declarationMessage", quarantineHeader.Messages.Contains(declarationMessage.PK));
		}

		public void TestUxmlMessages_FilterDuplicates()
		{
			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			var linkedMessage = Factory.New<RFPMessage>();
			linkedMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NEXDOCS;
			linkedMessage.EM_LinkedObject = quarantineHeader;
			linkedMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;

			var shipmentMessage = CreateInboundUXMLMessage(shipment);
			var declarationMessage1 = CreateInboundUXMLMessage(declaration);
			var declarationMessage2 = CreateInboundUXMLMessage(declaration);
			declarationMessage2.EM_MessageText = UniversalEvent_Error;
			declarationMessage2.EM_LinkedObject = quarantineHeader;
			Factory.Save();

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;

			AssertEquals("Header messages", 4, quarantineHeader.Messages.Count);
			Assert("has linkedMessage", quarantineHeader.Messages.Contains(linkedMessage.PK));
			Assert("has shipmentMessage", quarantineHeader.Messages.Contains(shipmentMessage.PK));
			Assert("has declarationMessage1", quarantineHeader.Messages.Contains(declarationMessage1.PK));

			var decMsg2 = quarantineHeader.Messages.FindByPK(declarationMessage2.PK);
			AssertNotNull("has declarationMessage2", decMsg2);
			AssertContains("Has Interpretation", "A response message has been received from Quarantine.", (decMsg2 as RFPEDIMessage).EM_MessageInterpretation);
		}

		public void TestRequestForPermitNumber()
		{
			AssertEquals("CusEntryNumber doesn't exist, QH_RequestForPermitNumber returns empty", ZString.Empty, quarantineHeader.QH_RequestForPermitNumber);
			AssertNull("Getting QH_RequestForPermitNumber doesn't create CusEntryNumber", CusEntryNumber.Load(quarantineHeader, CusEntryNumber.EntryType.RequestForPermitStatus, Core.Constants.CountryCodes.Australia));
			quarantineHeader.QH_RequestForPermitNumber = "4430295";
			AssertNotNull("Setting QH_RequestForPermitNumber creates CusEntryNumber", CusEntryNumber.Load(quarantineHeader, CusEntryNumber.EntryType.RequestForPermitStatus, Core.Constants.CountryCodes.Australia));
			AssertEquals("QH_RequestForPermitNumber returns value from CusEntryNumber", "4430295", quarantineHeader.QH_RequestForPermitNumber);
		}

		public void TestRequestForPermitStatus()
		{
			AssertEquals("CusEntryNumber doesn't exist, RequestForPermitStatus returns empty", ZString.Empty, quarantineHeader.RequestForPermitStatus);
			AssertNull("Getting RequestForPermitStatus doesn't create CusEntryNumber", CusEntryNumber.Load(quarantineHeader, CusEntryNumber.EntryType.RequestForPermitStatus, Core.Constants.CountryCodes.Australia));
			quarantineHeader.RequestForPermitStatus = "INI";
			AssertNotNull("Setting RequestForPermitStatus creates CusEntryNumber", CusEntryNumber.Load(quarantineHeader, CusEntryNumber.EntryType.RequestForPermitStatus, Core.Constants.CountryCodes.Australia));
			AssertEquals("RequestForPermitStatus returns value from CusEntryNumber", "INI", quarantineHeader.RequestForPermitStatus);
		}

		public void TestRequestForPermitEntryType()
		{
			AssertEquals("CusEntryNumber doesn't exist, RequestForPermitEntryType returns empty", ZString.Empty, quarantineHeader.RequestForPermitEntryType);
			AssertNull("Getting RequestForPermitEntryType doesn't create CusEntryNumber", CusEntryNumber.Load(quarantineHeader, CusEntryNumber.EntryType.RequestForPermitStatus, Core.Constants.CountryCodes.Australia));
			quarantineHeader.QH_RequestForPermitNumber = "4430295";
			AssertNotNull("Setting QH_RequestForPermitNumber creates CusEntryNumber", CusEntryNumber.Load(quarantineHeader, CusEntryNumber.EntryType.RequestForPermitStatus, Core.Constants.CountryCodes.Australia));
			AssertEquals("RequestForPermitEntryType returns value from CusEntryNumber", CusEntryNumber.EntryType.RequestForPermitStatus, quarantineHeader.RequestForPermitEntryType);
		}

		public void TestRequestForPermitNumberStatusDescription()
		{
			var rfpNumber = CusEntryNumber.New(quarantineHeader, CusEntryNumber.EntryType.RequestForPermitStatus, Core.Constants.CountryCodes.Australia);
			rfpNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			AssertEquals("Request For Permit Number Status returns correct description", EXDOCComplianceStatusCodesForCusEntryNumber.Descriptions.CompCompleted, quarantineHeader.QH_RequestForPermitNumberStatusDescription);

			rfpNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.WtdrnWithdrawn;
			AssertEquals("Request For Permit Number Status 'WTD' returns correct description", EXDOCComplianceStatusCodesForCusEntryNumber.Descriptions.WtdrnWithdrawn, quarantineHeader.QH_RequestForPermitNumberStatusDescription);

			rfpNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.ReviewInReview;
			AssertEquals("Request For Permit Number Status 'REV' returns correct description", EXDOCComplianceStatusCodesForCusEntryNumber.Descriptions.ReviewInReview, quarantineHeader.QH_RequestForPermitNumberStatusDescription);
		}

		public void TestRequestForPermitNumberPrintDescription()
		{
			var rfpNumber = CusEntryNumber.New(quarantineHeader, CusEntryNumber.EntryType.RequestForPermitStatus, Core.Constants.CountryCodes.Australia);
			rfpNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			AssertEquals("Request For Permit Number Status returns correct 4 letter status", EXDOCComplianceStatusCodesForRFPDocument.Descriptions.Comp, quarantineHeader.QH_RequestForPermitNumberPrintDescription);
		}

		public void TestExportPermitNumber()
		{
			AssertEquals("CusEntryNumber doesn't exist, QH_ExportPermitNumber returns empty", ZString.Empty, quarantineHeader.QH_ExportPermitNumber);
			AssertNull("Getting QH_ExportPermitNumber doesn't create CusEntryNumber", CusEntryNumber.Load(quarantineHeader, CusEntryNumber.EntryType.ExdocPermitNumber, Core.Constants.CountryCodes.Australia));
			quarantineHeader.QH_ExportPermitNumber = "PIMA4387356";
			AssertNotNull("Setting QH_ExportPermitNumber creates CusEntryNumber", CusEntryNumber.Load(quarantineHeader, CusEntryNumber.EntryType.ExdocPermitNumber, Core.Constants.CountryCodes.Australia));
			AssertEquals("QH_ExportPermitNumber returns value from CusEntryNumber", "PIMA4387356", quarantineHeader.QH_ExportPermitNumber);
		}

		public void TestExportPermitStatus()
		{
			AssertEquals("CusEntryNumber doesn't exist, ExportPermitStatus returns empty", ZString.Empty, quarantineHeader.ExportPermitStatus);
			AssertNull("Getting ExportPermitStatus doesn't create CusEntryNumber", CusEntryNumber.Load(quarantineHeader, CusEntryNumber.EntryType.ExdocPermitNumber, Core.Constants.CountryCodes.Australia));
			quarantineHeader.ExportPermitStatus = "INI";
			AssertNotNull("Setting ExportPermitStatus creates CusEntryNumber", CusEntryNumber.Load(quarantineHeader, CusEntryNumber.EntryType.ExdocPermitNumber, Core.Constants.CountryCodes.Australia));
			AssertEquals("ExportPermitStatus returns value from CusEntryNumber", "INI", quarantineHeader.ExportPermitStatus);
		}

		public void TestCertificateRequestNumber()
		{
			AssertEquals("CusEntryNumber doesn't exist, CertificateRequestNumber returns empty", ZString.Empty, quarantineHeader.CertificateRequestNumber);
			AssertNull("Getting CertificateRequestNumber doesn't create CusEntryNumber", CusEntryNumber.Load(quarantineHeader, CusEntryNumber.EntryType.CertificateRequestID, Core.Constants.CountryCodes.Australia));
			quarantineHeader.CertificateRequestNumber = "PIMA4387356";
			AssertNotNull("Setting CertificateRequestNumber creates CusEntryNumber", CusEntryNumber.Load(quarantineHeader, CusEntryNumber.EntryType.CertificateRequestID, Core.Constants.CountryCodes.Australia));
			AssertEquals("CertificateRequestNumber returns value from CusEntryNumber", "PIMA4387356", quarantineHeader.CertificateRequestNumber);
		}

		public void TestCertificateStatus()
		{
			AssertEquals("CusEntryNumber doesn't exist, CertificateStatus returns empty", ZString.Empty, quarantineHeader.CertificateStatus);
			AssertNull("Getting CertificateStatus doesn't create CusEntryNumber", CusEntryNumber.Load(quarantineHeader, CusEntryNumber.EntryType.CertificateRequestID, Core.Constants.CountryCodes.Australia));
			quarantineHeader.CertificateStatus = "INI";
			AssertNotNull("Setting CertificateStatus creates CusEntryNumber", CusEntryNumber.Load(quarantineHeader, CusEntryNumber.EntryType.CertificateRequestID, Core.Constants.CountryCodes.Australia));
			AssertEquals("CertificateStatus returns value from CusEntryNumber", "INI", quarantineHeader.CertificateStatus);
		}

		public void TestClone_NEXDOC()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			org.CustomsCodes.AddNew(OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExternalID, "123").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			org.CustomsCodes.AddNew(OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser, "456").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			org.CustomsCodes.AddNew(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, "789").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			org.CustomsCodes.AddNew(OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber, "012").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;

			Factory.Save();

			var cusRegNo = org.CustomsCodes.GetCustomsRegNo(OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser, Core.Constants.CountryCodes.Australia);
			var exporterNumber = org.CustomsCodes.GetCustomsRegNo(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, Core.Constants.CountryCodes.Australia);

			//RFP Details
			//RFP Details/Header Details
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineHeader.QH_ProductUseIndicator = EXDOCProductUseIndicatorCodes.Codes.HumanConsumption;
			quarantineHeader.QH_ObtainExportCustomsPermit = true;
			quarantineHeader.QH_CustomsConsigneeName = "Customs Consignee";
			quarantineHeader.QH_ExemptionCode = "Exemption Code";
			//RFP Details/Health Certificate Details
			quarantineHeader.QH_CertificatePrintIndicator = "A";
			quarantineHeader.QH_AQISRegion = "ADL";
			quarantineHeader.QH_SplitHealthCertByContainer = true;
			quarantineHeader.QH_SplitHealthCertByPacker = true;
			quarantineHeader.QH_SplitHealthCertByMarks = true;
			quarantineHeader.QH_AMLCQuota = true;
			quarantineHeader.QH_ShipsStores = true;
			quarantineHeader.QH_AMLCQuotaYear = "2017-01";
			quarantineHeader.QH_QuotaType = "QER";
			//RFP Details/Print Location
			quarantineHeader.QH_PrintLocation = "ORGANISATION";
			quarantineHeader.QH_OH_PrintLocationOrganisation = org.PK;
			//RFP Details/Transport Details
			quarantineHeader.QH_RN_NKOriginCountry = "AU";
			quarantineHeader.QH_RL_NKBorderInspectionPort = "ADALV";
			quarantineHeader.QH_PackDate = new ZDateTime(2019, 3, 3);
			//RFP Details/Transport Details/Storage Temerature
			quarantineHeader.QH_AbsoluteTemperature = 1.1m;
			quarantineHeader.QH_MinimumTemperature = 0.91m;
			quarantineHeader.QH_MaximumTemperature = 1.91m;
			quarantineHeader.QH_TemperatureUM = "FAH";
			//RFP Details/Recommendation Letter Details
			var letter = quarantineHeader.RecommendationLetters.AddNew();
			letter.ZA_LetterNumber = "1";
			letter.ZA_LetterDate = new ZDateTime(2019, 3, 3);

			Factory.Save();

			//RFP Indicator Declarations
			quarantineHeader.QH_DecOfCompliance = EXDOCYesNoEmpty.Codes.No;
			quarantineHeader.QH_ImportedProductFlag = EXDOCYesNoEmpty.Codes.Yes;
			quarantineHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.Yes;
			quarantineHeader.QH_ManufacturedTreatedPackagedLabelledInAustralia = EXDOCYesNoEmpty.Codes.Yes;
			quarantineHeader.QH_LegallyImportedFlag = EXDOCYesNoEmpty.Codes.Yes;
			quarantineHeader.QH_ExporterDeclaration = "Exporter Declaration";
			//RFP Indicator Declarations/Declaration Code
			var exdocSupportingInfos = quarantineHeader.SupportingInfos.AddNew();
			exdocSupportingInfos.CSI_Type = "DEC";
			exdocSupportingInfos.CSI_Code = "DEC";
			exdocSupportingInfos.CSI_Description = "123456";
			exdocSupportingInfos.CSI_LineNo = 1;

			//RFP Inspection Details
			//RFP Inspection Details/Authorisation Establishment
			quarantineHeader.QH_AuthorisationLocation = "ORGANISATION";
			quarantineHeader.QH_OA_AuthorisationEstablishment = org.MainAddress.PK;
			quarantineHeader.QH_AuthorisationDate = new ZDate(2019, 3, 3);
			quarantineHeader.QH_AuthorisationComments = "AuthorisationComments";
			//RFP Inspection Details/Storage Establishment
			quarantineHeader.QH_StorageLocation = "ORGANISATION";
			quarantineHeader.QH_OA_StorageEstablishment = org.MainAddress.PK;
			quarantineHeader.QH_ApprovedCertifier = "H0002";
			quarantineHeader.QH_AvAnimalAge = "LESS THAN 1 YEAR";
			quarantineHeader.QH_LotNumber = "1234567";
			quarantineHeader.QH_OriginCatchZone = "Zone";
			//RFP Inspection Details/Vessel Hold
			quarantineHeader.QH_StartHoldSeal = "1";
			quarantineHeader.QH_EndHoldSeal = "11";
			//RFP Inspection Details/Inspection Process
			quarantineHeader.QH_InspectionRequestedDate = new ZDateTime(2019, 3, 3, 8, 9, 0);
			quarantineHeader.QH_AuthorisedStartDate = new ZDate(2019, 3, 3);
			quarantineHeader.QH_AuthorisedEndDate = new ZDate(2019, 4, 3);
			//RFP Inspection Details/Authorised Officer
			quarantineHeader.QH_AuthorisingOfficerID = "KYNANB";
			quarantineHeader.QH_InspectorComments = "BOO";

			//RFP Ships Compartments/Ships Compartment Inspections
			var exdocCompartment = quarantineHeader.Compartments.AddNew();
			exdocCompartment.QC_Compartments = "WOWBIGBOOBS";
			exdocCompartment.QC_RL_NKInspectionPort = "ADALV";
			exdocCompartment.QC_InspectionDate = new ZDateTime(2006, 12, 19);

			//RFP EU Transit
			quarantineHeader.QH_TransitLocationType = "C";
			quarantineHeader.QH_ApprovalNumber = "787";

			//RFP Messaging
			quarantineHeader.QH_RequestForPermitNumber = "123";
			quarantineHeader.QH_ExportPermitNumber = "456";
			quarantineHeader.QH_LastAmendDateTime = new ZDateTimeOffset(2019, 8, 29, 9, 23, 0, 1, TimeSpan.FromHours(10));
			quarantineHeader.Messages.AddNew();

			var clonedInvoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			quarantineHeader.Clone(clonedInvoiceHeader.PK);
			var clonedExDocHeader = clonedInvoiceHeader.QuarantineExDocHeader;

			CombineAssertions(() =>
			{
				AssertEquals("Parent ID set", clonedInvoiceHeader.PK, clonedExDocHeader.QH_JZ);
				//RFP Details
				AssertEquals("Produce Type", EXDOCCommodityCodes.Codes.Dairy, clonedExDocHeader.QH_ProduceType);
				AssertEquals("Produce Use", EXDOCProductUseIndicatorCodes.Codes.HumanConsumption, clonedExDocHeader.QH_ProductUseIndicator);
				AssertEquals("Get Customs EDN", true, clonedExDocHeader.QH_ObtainExportCustomsPermit);
				AssertEquals("Customs Consignee", "Customs Consignee", clonedExDocHeader.QH_CustomsConsigneeName);
				AssertEquals("Exemption Code", "Exemption Code", clonedExDocHeader.QH_ExemptionCode);
				AssertEquals("Print Option", "A", clonedExDocHeader.QH_CertificatePrintIndicator);
				AssertEquals("Production Region", "ADL", clonedExDocHeader.QH_AQISRegion);
				AssertEquals("Split by Container", true, clonedExDocHeader.QH_SplitHealthCertByContainer);
				AssertEquals("Split by Packer", true, clonedExDocHeader.QH_SplitHealthCertByPacker);
				AssertEquals("Split by Marks", true, clonedExDocHeader.QH_SplitHealthCertByMarks);
				AssertEquals("AMLC Quota", true, clonedExDocHeader.QH_AMLCQuota);
				AssertEquals("Ship Stores", true, clonedExDocHeader.QH_ShipsStores);
				AssertEquals("AMLC Quota Year", "2017-01", clonedExDocHeader.QH_AMLCQuotaYear);
				AssertEquals("Quota Type", "QER", clonedExDocHeader.QH_QuotaType);
				AssertEquals("Print Location", "ORGANISATION", clonedExDocHeader.QH_PrintLocation);
				AssertEquals("Print Location Org", org.PK, clonedExDocHeader.QH_OH_PrintLocationOrganisation);
				AssertEquals("Set QH_CertificateRequiredLocation in QH_OH_PrintLocationOrganisation", org.CustomsCodes.GetCustomsRegNo(OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExternalID, Core.Constants.CountryCodes.Australia), clonedExDocHeader.QH_CertificateRequiredLocation);
				AssertEquals("Product Source", "AU", clonedExDocHeader.QH_RN_NKOriginCountry);
				AssertEquals("Border Inspection Port", "ADALV", clonedExDocHeader.QH_RL_NKBorderInspectionPort);
				AssertEquals("Pack Date", ZDateTime.Empty, clonedExDocHeader.QH_PackDate);
				AssertEquals("Absolute", 1.1m, clonedExDocHeader.QH_AbsoluteTemperature);
				AssertEquals("Minimum", 0.91m, clonedExDocHeader.QH_MinimumTemperature);
				AssertEquals("Maximum", 1.91m, clonedExDocHeader.QH_MaximumTemperature);
				AssertEquals("Unit", "FAH", clonedExDocHeader.QH_TemperatureUM);
				AssertEquals("Recommendation Letter Details", 0, clonedExDocHeader.RecommendationLetters.Count);
				//RFP Indicator Declarations
				AssertEquals("Declaration of Compliance Indicator", ZString.Empty, clonedExDocHeader.QH_DecOfCompliance);
				AssertEquals("Imported Product Flag", "NO", clonedExDocHeader.QH_ImportedProductFlag);
				AssertEquals("True And Complete Indicator", ZString.Empty, clonedExDocHeader.QH_TrueAndCompleteIndicator);
				AssertEquals("Manufactured Treated Packaged Labelled In Australia Flag", ZString.Empty, clonedExDocHeader.QH_ManufacturedTreatedPackagedLabelledInAustralia);
				AssertEquals("Legally Imported Flag", ZString.Empty, clonedExDocHeader.QH_LegallyImportedFlag);
				AssertEquals("Exporter Declaration", ZString.Empty, clonedExDocHeader.QH_ExporterDeclaration);
				AssertEquals("Declaration Code", 0, clonedExDocHeader.SupportingInfos.Count);
				//RFP Inspection Details
				AssertEquals("Location", "CODE", clonedExDocHeader.QH_AuthorisationLocation);
				AssertEquals("Location Org", ZGuid.Empty, clonedExDocHeader.QH_OA_AuthorisationEstablishment);
				AssertEquals("AuthorisationEstablishment", ZString.Empty, clonedExDocHeader.QH_AuthorisationEstablishment);
				AssertEquals("Date", ZDate.Empty, clonedExDocHeader.QH_AuthorisationDate);
				AssertEquals("Comments", ZString.Empty, clonedExDocHeader.QH_AuthorisationComments);
				AssertEquals("Storage Establishment Location", "CODE", clonedExDocHeader.QH_StorageLocation);
				AssertEquals("Storage Establishment Org", ZGuid.Empty, clonedExDocHeader.QH_OA_StorageEstablishment);
				AssertEquals("Storage Establishment", ZString.Empty, clonedExDocHeader.QH_StorageEstablishment);
				AssertEquals("App. Certifier", ZString.Empty, clonedExDocHeader.QH_ApprovedCertifier);
				AssertEquals("Average Age of Animals", ZString.Empty, clonedExDocHeader.QH_AvAnimalAge);
				AssertEquals("Lot Number", ZString.Empty, clonedExDocHeader.QH_LotNumber);
				AssertEquals("Catch Zone", ZString.Empty, clonedExDocHeader.QH_OriginCatchZone);
				AssertEquals("Start Hold Seal", ZString.Empty, clonedExDocHeader.QH_StartHoldSeal);
				AssertEquals("End Hold Seal", ZString.Empty, clonedExDocHeader.QH_EndHoldSeal);
				AssertEquals("Inspection Requested", ZDateTime.Empty, clonedExDocHeader.QH_InspectionRequestedDate);
				AssertEquals("Inspection Start", ZDate.Empty, clonedExDocHeader.QH_AuthorisedStartDate);
				AssertEquals("Inspection End", ZDate.Empty, clonedExDocHeader.QH_AuthorisedEndDate);
				AssertEquals("Officer ID", ZString.Empty, clonedExDocHeader.QH_AuthorisingOfficerID);
				AssertEquals("Inspector Comments", ZString.Empty, clonedExDocHeader.QH_InspectorComments);
				//RFP Ships Compartments
				AssertEquals("RFP Ships Compartments", 0, clonedExDocHeader.Compartments.Count);
				//RFP EU Transit
				AssertEquals("Transit location", "C", clonedExDocHeader.QH_TransitLocationType);
				AssertEquals("Approval Number", "787", clonedExDocHeader.QH_ApprovalNumber);
				//RFP Messaging
				AssertEquals("RFP Number", ZString.Empty, clonedExDocHeader.QH_RequestForPermitNumber);
				AssertEquals("Export Permit Number", ZString.Empty, clonedExDocHeader.QH_ExportPermitNumber);
				AssertEquals("Last Amend DateTime", ZDateTimeOffset.Empty, clonedExDocHeader.QH_LastAmendDateTime);
				AssertEquals("RFP Messaging", 0, clonedExDocHeader.Messages.Count);
			});
		}

		public void TestClone_EXDOC()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			org.CustomsCodes.AddNew(OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExternalID, "123").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			org.CustomsCodes.AddNew(OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser, "456").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			org.CustomsCodes.AddNew(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, "789").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			org.CustomsCodes.AddNew(OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber, "012").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;

			Factory.Save();

			var cusRegNo = org.CustomsCodes.GetCustomsRegNo(OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser, Core.Constants.CountryCodes.Australia);
			var exporterNumber = org.CustomsCodes.GetCustomsRegNo(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, Core.Constants.CountryCodes.Australia);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				//RFP Details
				//RFP Details/Header Details
				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
				quarantineHeader.QH_ProductUseIndicator = EXDOCProductUseIndicatorCodes.Codes.HumanConsumption;
				quarantineHeader.QH_ObtainExportCustomsPermit = true;

				//RFP Details/Health Certificate Details
				quarantineHeader.QH_CertificatePrintIndicator = "A";
				quarantineHeader.QH_AQISRegion = "ADL";
				quarantineHeader.QH_SplitHealthCertByContainer = true;
				quarantineHeader.QH_SplitHealthCertByPacker = true;
				quarantineHeader.QH_SplitHealthCertByMarks = true;
				quarantineHeader.QH_AMLCQuota = true;
				quarantineHeader.QH_ShipsStores = true;
				quarantineHeader.QH_AMLCQuotaYear = "2017-01";
				quarantineHeader.QH_QuotaType = "QER";
				//RFP Details/Print Location
				quarantineHeader.QH_PrintLocation = "ORGANISATION";
				quarantineHeader.QH_OH_PrintLocationOrganisation = org.PK;
				//RFP Details/Transport Details
				quarantineHeader.QH_RN_NKOriginCountry = "AU";
				quarantineHeader.QH_RL_NKBorderInspectionPort = "ADALV";
				quarantineHeader.QH_PackDate = new ZDateTime(2019, 3, 3);
				//RFP Details/Transport Details/Storage Temperature
				quarantineHeader.QH_AbsoluteTemperature = 1.1m;
				quarantineHeader.QH_MinimumTemperature = 0.91m;
				quarantineHeader.QH_MaximumTemperature = 1.91m;
				quarantineHeader.QH_TemperatureUM = "FAH";
				//RFP Details/Recommendation Letter Details
				var letter = quarantineHeader.RecommendationLetters.AddNew();
				letter.ZA_LetterNumber = "1";
				letter.ZA_LetterDate = new ZDateTime(2019, 3, 3);

				//RFP Indicator Declarations
				quarantineHeader.QH_DecOfCompliance = EXDOCYesNoEmpty.Codes.No;
				quarantineHeader.QH_ImportedProductFlag = EXDOCYesNoEmpty.Codes.Yes;
				quarantineHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.Yes;
				quarantineHeader.QH_ManufacturedTreatedPackagedLabelledInAustralia = EXDOCYesNoEmpty.Codes.Yes;
				quarantineHeader.QH_LegallyImportedFlag = EXDOCYesNoEmpty.Codes.Yes;
				quarantineHeader.QH_ExporterDeclaration = "Exporter Declaration";
				//RFP Indicator Declarations/Declaration Code
				var exdocSupportingInfos = quarantineHeader.SupportingInfos.AddNew();
				exdocSupportingInfos.CSI_Type = "DEC";
				exdocSupportingInfos.CSI_Code = "DEC";
				exdocSupportingInfos.CSI_Description = "123456";
				exdocSupportingInfos.CSI_LineNo = 1;

				//RFP Inspection Details
				//RFP Inspection Details/Authorisation Establishment
				quarantineHeader.QH_AuthorisationLocation = "ORGANISATION";
				quarantineHeader.QH_OA_AuthorisationEstablishment = org.MainAddress.PK;
				quarantineHeader.QH_AuthorisationDate = new ZDate(2019, 3, 3);
				quarantineHeader.QH_AuthorisationComments = "AuthorisationComments";
				//RFP Inspection Details/Storage Establishment
				quarantineHeader.QH_StorageLocation = "ORGANISATION";
				quarantineHeader.QH_OA_StorageEstablishment = org.MainAddress.PK;
				quarantineHeader.QH_ApprovedCertifier = "H0002";
				quarantineHeader.QH_AvAnimalAge = "LESS THAN 1 YEAR";
				quarantineHeader.QH_LotNumber = "1234567";
				quarantineHeader.QH_OriginCatchZone = "Zone";
				//RFP Inspection Details/Vessel Hold
				quarantineHeader.QH_StartHoldSeal = "1";
				quarantineHeader.QH_EndHoldSeal = "11";
				//RFP Inspection Details/Inspection Process
				quarantineHeader.QH_InspectionRequestedDate = new ZDateTime(2019, 3, 3, 8, 9, 0);
				quarantineHeader.QH_AuthorisedStartDate = new ZDate(2019, 3, 3);
				quarantineHeader.QH_AuthorisedEndDate = new ZDate(2019, 4, 3);
				//RFP Inspection Details/Authorised Officer
				quarantineHeader.QH_AuthorisingOfficerID = "KYNANB";
				quarantineHeader.QH_InspectorComments = "BOO";

				//RFP Forward/Transfer
				//RFP Forward/Transfer/Forward Details
				quarantineHeader.QH_ForwardLocation = "ORGANISATION";
				quarantineHeader.QH_OH_ForwardLocationOrganisation = org.PK;
				quarantineHeader.QH_ForwardStatus = "COMP";
				//RFP Forward/Transfer/Transfer To Details
				quarantineHeader.QH_TransferEDIUserLocation = "ORGANISATION";
				quarantineHeader.QH_OH_TransferEDIUserLocationOrganisation = org.PK;
				quarantineHeader.QH_TransferExporterLocation = "ORGANISATION";
				quarantineHeader.QH_OH_TransferExporterLocationOrganisation = org.PK;
				quarantineHeader.QH_CancelTransferIndicator = true;

				//RFP Ships Compartments/Ships Compartment Inspections
				var exdocCompartment = quarantineHeader.Compartments.AddNew();
				exdocCompartment.QC_Compartments = "WOWBIGBOOBS";
				exdocCompartment.QC_RL_NKInspectionPort = "ADALV";
				exdocCompartment.QC_InspectionDate = new ZDateTime(2006, 12, 19);

				//RFP EU Transit
				quarantineHeader.QH_TransitLocationType = "C";
				quarantineHeader.QH_ApprovalNumber = "787";

				//RFP Messaging
				quarantineHeader.QH_RequestForPermitNumber = "123";
				quarantineHeader.QH_ExportPermitNumber = "456";
				quarantineHeader.QH_LastAmendDateTime = new ZDateTimeOffset(2019, 8, 29, 9, 23, 0, 1, TimeSpan.FromHours(10));
				quarantineHeader.Messages.AddNew();

				var clonedInvoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				quarantineHeader.Clone(clonedInvoiceHeader.PK);
				var clonedExDocHeader = clonedInvoiceHeader.QuarantineExDocHeader;

				CombineAssertions(() =>
				{
					AssertEquals("Parent ID set", clonedInvoiceHeader.PK, clonedExDocHeader.QH_JZ);
					//RFP Details
					AssertEquals("Produce Type", EXDOCCommodityCodes.Codes.Meat, clonedExDocHeader.QH_ProduceType);
					AssertEquals("Produce Use", EXDOCProductUseIndicatorCodes.Codes.HumanConsumption, clonedExDocHeader.QH_ProductUseIndicator);
					AssertEquals("Get Customs EDN", true, clonedExDocHeader.QH_ObtainExportCustomsPermit);
					AssertEquals("Customs Consignee", ZString.Empty, clonedExDocHeader.QH_CustomsConsigneeName);
					AssertEquals("Exemption Code", ZString.Empty, clonedExDocHeader.QH_ExemptionCode);
					AssertEquals("Print Option", "A", clonedExDocHeader.QH_CertificatePrintIndicator);
					AssertEquals("Production Region", "ADL", clonedExDocHeader.QH_AQISRegion);
					AssertEquals("Split by Container", true, clonedExDocHeader.QH_SplitHealthCertByContainer);
					AssertEquals("Split by Packer", true, clonedExDocHeader.QH_SplitHealthCertByPacker);
					AssertEquals("Split by Marks", true, clonedExDocHeader.QH_SplitHealthCertByMarks);
					AssertEquals("AMLC Quota", true, clonedExDocHeader.QH_AMLCQuota);
					AssertEquals("Ship Stores", true, clonedExDocHeader.QH_ShipsStores);
					AssertEquals("AMLC Quota Year", "2017-01", clonedExDocHeader.QH_AMLCQuotaYear);
					AssertEquals("Quota Type", "QER", clonedExDocHeader.QH_QuotaType);
					AssertEquals("Print Location", "ORGANISATION", clonedExDocHeader.QH_PrintLocation);
					AssertEquals("Print Location Org", org.PK, clonedExDocHeader.QH_OH_PrintLocationOrganisation);
					AssertEquals("Set QH_CertificateRequiredLocation in QH_OH_PrintLocationOrganisation", cusRegNo, clonedExDocHeader.QH_CertificateRequiredLocation);
					AssertEquals("Product Source", "AU", clonedExDocHeader.QH_RN_NKOriginCountry);
					AssertEquals("Border Inspection Port", "ADALV", clonedExDocHeader.QH_RL_NKBorderInspectionPort);
					AssertEquals("Pack Date", ZDateTime.Empty, clonedExDocHeader.QH_PackDate);
					AssertEquals("Absolute", 1.1m, clonedExDocHeader.QH_AbsoluteTemperature);
					AssertEquals("Minimum", 0.91m, clonedExDocHeader.QH_MinimumTemperature);
					AssertEquals("Maximum", 1.91m, clonedExDocHeader.QH_MaximumTemperature);
					AssertEquals("Unit", "FAH", clonedExDocHeader.QH_TemperatureUM);
					AssertEquals("Recommendation Letter Details", 0, clonedExDocHeader.RecommendationLetters.Count);
					//RFP Indicator Declarations
					AssertEquals("Declaration of Compliance Indicator", ZString.Empty, clonedExDocHeader.QH_DecOfCompliance);
					AssertEquals("Imported Product Flag", "NO", clonedExDocHeader.QH_ImportedProductFlag);
					AssertEquals("True And Complete Indicator", ZString.Empty, clonedExDocHeader.QH_TrueAndCompleteIndicator);
					AssertEquals("Manufactured Treated Packaged Labelled In Australia Flag", ZString.Empty, clonedExDocHeader.QH_ManufacturedTreatedPackagedLabelledInAustralia);
					AssertEquals("Legally Imported Flag", ZString.Empty, clonedExDocHeader.QH_LegallyImportedFlag);
					AssertEquals("Exporter Declaration", ZString.Empty, clonedExDocHeader.QH_ExporterDeclaration);
					AssertEquals("Declaration Code", 0, clonedExDocHeader.SupportingInfos.Count);
					//RFP Inspection Details
					AssertEquals("Location", "CODE", clonedExDocHeader.QH_AuthorisationLocation);
					AssertEquals("Location Org", ZGuid.Empty, clonedExDocHeader.QH_OA_AuthorisationEstablishment);
					AssertEquals("AuthorisationEstablishment", ZString.Empty, clonedExDocHeader.QH_AuthorisationEstablishment);
					AssertEquals("Date", ZDate.Empty, clonedExDocHeader.QH_AuthorisationDate);
					AssertEquals("Comments", ZString.Empty, clonedExDocHeader.QH_AuthorisationComments);
					AssertEquals("Storage Establishment Location", "CODE", clonedExDocHeader.QH_StorageLocation);
					AssertEquals("Storage Establishment Org", ZGuid.Empty, clonedExDocHeader.QH_OA_StorageEstablishment);
					AssertEquals("Storage Establishment", ZString.Empty, clonedExDocHeader.QH_StorageEstablishment);
					AssertEquals("App. Certifier", ZString.Empty, clonedExDocHeader.QH_ApprovedCertifier);
					AssertEquals("Average Age of Animals", ZString.Empty, clonedExDocHeader.QH_AvAnimalAge);
					AssertEquals("Lot Number", ZString.Empty, clonedExDocHeader.QH_LotNumber);
					AssertEquals("Catch Zone", ZString.Empty, clonedExDocHeader.QH_OriginCatchZone);
					AssertEquals("Start Hold Seal", ZString.Empty, clonedExDocHeader.QH_StartHoldSeal);
					AssertEquals("End Hold Seal", ZString.Empty, clonedExDocHeader.QH_EndHoldSeal);
					AssertEquals("Inspection Requested", ZDateTime.Empty, clonedExDocHeader.QH_InspectionRequestedDate);
					AssertEquals("Inspection Start", ZDate.Empty, clonedExDocHeader.QH_AuthorisedStartDate);
					AssertEquals("Inspection End", ZDate.Empty, clonedExDocHeader.QH_AuthorisedEndDate);
					AssertEquals("Officer ID", ZString.Empty, clonedExDocHeader.QH_AuthorisingOfficerID);
					AssertEquals("Inspector Comments", ZString.Empty, clonedExDocHeader.QH_InspectorComments);
					//RFP Forward/Transfer
					AssertEquals("Forward to EDI User", "ORGANISATION", clonedExDocHeader.QH_ForwardLocation);
					AssertEquals("Forward to EDI User Org", org.PK, clonedExDocHeader.QH_OH_ForwardLocationOrganisation);
					AssertEquals("ForwardeeEDIUserIdentifier", cusRegNo, clonedExDocHeader.QH_ForwardeeEDIUserIdentifier);
					AssertEquals("Status", "COMP", clonedExDocHeader.QH_ForwardStatus);
					AssertEquals("Transfert to EDI User", "ORGANISATION", clonedExDocHeader.QH_TransferEDIUserLocation);
					AssertEquals("Transfert to EDI User Org", org.PK, clonedExDocHeader.QH_OH_TransferEDIUserLocationOrganisation);
					AssertEquals("QH_TransfereeEDIUserIdentifier", cusRegNo, clonedExDocHeader.QH_TransfereeEDIUserIdentifier);
					AssertEquals("Transfer to Exporter", "ORGANISATION", clonedExDocHeader.QH_TransferExporterLocation);
					AssertEquals("Transfer to Exporter Org", org.PK, clonedExDocHeader.QH_OH_TransferExporterLocationOrganisation);
					AssertEquals("QH_TransfereeExporterNumber", exporterNumber, clonedExDocHeader.QH_TransfereeExporterNumber);
					AssertEquals("Cancel Transfer", true, clonedExDocHeader.QH_CancelTransferIndicator);
					//RFP Ships Compartments
					AssertEquals("RFP Ships Compartments", 0, clonedExDocHeader.Compartments.Count);
					//RFP EU Transit
					AssertEquals("Transit location", "C", clonedExDocHeader.QH_TransitLocationType);
					AssertEquals("Approval Number", "787", clonedExDocHeader.QH_ApprovalNumber);
					//RFP Messaging
					AssertEquals("RFP Number", ZString.Empty, clonedExDocHeader.QH_RequestForPermitNumber);
					AssertEquals("Export Permit Number", ZString.Empty, clonedExDocHeader.QH_ExportPermitNumber);
					AssertEquals("Last Amend DateTime", ZDateTimeOffset.Empty, clonedExDocHeader.QH_LastAmendDateTime);
					AssertEquals("RFP Messaging", 0, clonedExDocHeader.Messages.Count);
				});
			}
		}

		public void TestConsigneeDetails()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TSTIMP";
			importer.OH_FullName = "WALLACE THE IMPORTER";
			importer.MainAddress.OA_Address1 = "62 WEST";
			importer.MainAddress.OA_Address2 = "WALLABY ST";
			importer.MainAddress.OA_City = "WASHINGTON";
			importer.MainAddress.OA_PostCode = "654321";
			importer.OH_RL_NKClosestPort = "USLGB";
			importer.MainAddress.OA_State = "DC";
			quarantineHeader.Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("RFP Print Consignee Details are formatted correctly", ImporterOrg1, quarantineHeader.ConsigneeDetails);
			importer.MainAddress.OA_City = ZString.Empty;
			importer.OH_RL_NKClosestPort = ZString.Empty;
			AssertEquals("RFP Print Consignee Details have unknown for the city", ImporterOrg2, quarantineHeader.ConsigneeDetails);
			importer.OH_RL_NKClosestPort = "USLGB";
			importer.MainAddress.OA_City = "WASHINGTON";
			importer.OH_RL_NKClosestPort = ZString.Empty;
			AssertEquals("RFP Print Consignee Details have no country", ImporterOrg3, quarantineHeader.ConsigneeDetails);
			importer.OH_RL_NKClosestPort = "USLGB";
			importer.MainAddress.OA_State = ZString.Empty;
			AssertEquals("RFP Print Consignee Details have no state", ImporterOrg4, quarantineHeader.ConsigneeDetails);
			importer.MainAddress.OA_State = "DC";
			importer.MainAddress.OA_PostCode = ZString.Empty;
			AssertEquals("RFP Print Consignee Details have no postcode", ImporterOrg5, quarantineHeader.ConsigneeDetails);
			importer.MainAddress.OA_PostCode = "654321";
			importer.MainAddress.OA_Address1 = "THIS HAS TO HAVE APPROXIMATELY 50 CHARACTERS";
			importer.MainAddress.OA_Address2 = "LINE IS A CONTINUATION TO GET OVER A TOTAL OF 70";
			AssertEquals("RFP Print Consignee Details has the address shortened to 70", ImporterOrg6, quarantineHeader.ConsigneeDetails);
			importer.OH_FullName = "WALLACE THE IMPORTER";
			importer.MainAddress.OA_Address1 = "62 WEST";
			importer.MainAddress.OA_Address2 = ZString.Empty;
			importer.MainAddress.OA_City = ZString.Empty;
			importer.MainAddress.OA_PostCode = ZString.Empty;
			importer.OH_RL_NKClosestPort = ZString.Empty;
			importer.MainAddress.OA_State = ZString.Empty;
			AssertEquals("RFP Print Consignee Details miniumum requirements is correct", ImporterOrg7, quarantineHeader.ConsigneeDetails);
			quarantineHeader.Declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("RFP Print Consignee Details is empty", ZString.Empty, quarantineHeader.ConsigneeDetails);
		}

		public void TestQH_PrintLocation()
		{
			quarantineHeader.QH_PrintLocation = EXDOCCodeOrganisation.Codes.Organisation;
			quarantineHeader.QH_OH_PrintLocationOrganisation = ZGuid.NewZGuid();
			Assert("Location Organisation not empty", !quarantineHeader.QH_OH_PrintLocationOrganisation.IsEmpty);
			quarantineHeader.QH_PrintLocation = EXDOCCodeOrganisation.Codes.Code;
			Assert("Location Organisation empty", quarantineHeader.QH_OH_PrintLocationOrganisation.IsEmpty);
		}

		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Imported Product Flag", EXDOCYesNoEmpty.Codes.No, quarantineHeader.QH_ImportedProductFlag);
				AssertEquals("Print Location Default", EXDOCCodeOrganisation.Codes.Code, quarantineHeader.QH_PrintLocation);
				AssertEquals("Storage Location Default", EXDOCCodeOrganisation.Codes.Code, quarantineHeader.QH_StorageLocation);
				AssertEquals("Authorisation Location Default", EXDOCCodeOrganisation.Codes.Code, quarantineHeader.QH_AuthorisationLocation);
				AssertEquals("Forward Location Default", EXDOCCodeOrganisation.Codes.Code, quarantineHeader.QH_ForwardLocation);
				AssertEquals("Transfer EDI Location Default", EXDOCCodeOrganisation.Codes.Code, quarantineHeader.QH_TransferEDIUserLocation);
				AssertEquals("Transfer Exporter Location Default", EXDOCCodeOrganisation.Codes.Code, quarantineHeader.QH_TransferExporterLocation);
			});
		}

		public void TestQH_QuotaTypeMaxLength()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				AssertEquals("QH_QuotaType max length for EXDOC", 5, quarantineHeader.QH_QuotaTypeInfo.MaxLength);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals("QH_QuotaType max length for NEXDOC", 3, quarantineHeader.QH_QuotaTypeInfo.MaxLength);
			}
		}

		public void TestQH_StorageLocation()
		{
			quarantineHeader.QH_StorageLocation = EXDOCCodeOrganisation.Codes.Organisation;
			quarantineHeader.QH_OA_StorageEstablishment = ZGuid.NewZGuid();
			Assert("Location Organisation not empty", !quarantineHeader.QH_OA_StorageEstablishment.IsEmpty);
			quarantineHeader.QH_StorageLocation = EXDOCCodeOrganisation.Codes.Code;
			Assert("Location Organisation empty", quarantineHeader.QH_OA_StorageEstablishment.IsEmpty);
		}

		public void TestQH_AuthorisationLocation()
		{
			quarantineHeader.QH_AuthorisationLocation = EXDOCCodeOrganisation.Codes.Organisation;
			quarantineHeader.QH_OA_AuthorisationEstablishment = ZGuid.NewZGuid();
			Assert("Location Organisation not empty", !quarantineHeader.QH_OA_AuthorisationEstablishment.IsEmpty);
			quarantineHeader.QH_AuthorisationLocation = EXDOCCodeOrganisation.Codes.Code;
			Assert("Location Organisation empty", quarantineHeader.QH_OA_AuthorisationEstablishment.IsEmpty);
		}

		public void TestQH_AuthorisationFlag()
		{
			Assert(!quarantineHeader.QH_AuthorisationFlag);
			quarantineHeader.QH_AuthorisationFlag = true;
			Assert(quarantineHeader.QH_AuthorisationFlag);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var header = otherFactory.Load<QuarantineExDocHeader>(quarantineHeader.PK);
			Assert(header.QH_AuthorisationFlag);
		}

		public void TestQH_ForwardLocation()
		{
			quarantineHeader.QH_ForwardLocation = EXDOCCodeOrganisation.Codes.Organisation;
			quarantineHeader.QH_OH_ForwardLocationOrganisation = ZGuid.NewZGuid();
			Assert("Location Organisation not empty", !quarantineHeader.QH_OH_ForwardLocationOrganisation.IsEmpty);
			quarantineHeader.QH_ForwardLocation = EXDOCCodeOrganisation.Codes.Code;
			Assert("Location Organisation empty", quarantineHeader.QH_OH_ForwardLocationOrganisation.IsEmpty);
		}

		public void TestQH_TransferEDIUserLocation()
		{
			quarantineHeader.QH_TransferEDIUserLocation = EXDOCCodeOrganisation.Codes.Organisation;
			quarantineHeader.QH_OH_TransferEDIUserLocationOrganisation = ZGuid.NewZGuid();
			Assert("Location Organisation not empty", !quarantineHeader.QH_OH_TransferEDIUserLocationOrganisation.IsEmpty);
			quarantineHeader.QH_TransferEDIUserLocation = EXDOCCodeOrganisation.Codes.Code;
			Assert("Location Organisation empty", quarantineHeader.QH_OH_TransferEDIUserLocationOrganisation.IsEmpty);
		}

		public void TestQH_TransferExporterLocation()
		{
			quarantineHeader.QH_TransferExporterLocation = EXDOCCodeOrganisation.Codes.Organisation;
			quarantineHeader.QH_OH_TransferExporterLocationOrganisation = ZGuid.NewZGuid();
			Assert("Location Organisation not empty", !quarantineHeader.QH_OH_TransferExporterLocationOrganisation.IsEmpty);
			quarantineHeader.QH_TransferExporterLocation = EXDOCCodeOrganisation.Codes.Code;
			Assert("Location Organisation empty", quarantineHeader.QH_OH_TransferExporterLocationOrganisation.IsEmpty);
		}

		public void TestINEXDOCResponseMembers()
		{
			declaration.JE_DeclarationReference = "111";
			quarantineHeader.QH_RequestForPermitNumber = "222";
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			quarantineHeader.QH_ExportPermitNumber = "333";
			declaration.DeclarationNumber = "444";
			var response = quarantineHeader as INEXDOCResponse;
			AssertEquals(nameof(INEXDOCResponse.JobNumber), "111", response.JobNumber);
			AssertEquals(nameof(INEXDOCResponse.RexNumber), "222", response.RexNumber);
			AssertEquals(nameof(INEXDOCResponse.RexStatus), EXDOCComplianceStatusCodesForCusEntryNumber.Descriptions.CompCompleted, response.RexStatus);
			AssertEquals(nameof(INEXDOCResponse.ExportPermitNumber), "333", response.ExportPermitNumber);
			AssertEquals(nameof(INEXDOCResponse.CustomsAuthorityNumber), "444", response.CustomsAuthorityNumber);
			AssertEquals(nameof(INEXDOCResponse.HtmlTemplatePath), "Enterprise.Customs.AU.Declaration.Business.Data.Xml.Universal.NEXDOC.HtmlTemplates.Response.html", response.HtmlTemplatePath);
		}

		public void TestQH_ForwardRequiresAcceptance()
		{
			quarantineHeader.QH_ForwardRequiresAcceptance = true;
			Factory.Save();
			AssertContains("ForwardRequiresAcceptance=Y", quarantineHeader.QH_AddInfo);
			quarantineHeader.QH_ForwardRequiresAcceptance = false;
			Factory.Save();
			AssertNotContains("ForwardRequiresAcceptance=", quarantineHeader.QH_AddInfo);
		}

		public void TestCertificateNumbers()
		{
			var entryNum1 = Factory.New<CusEntryNumber>();
			entryNum1.CE_ParentTable = AutoQuarantineExDocHeader.Schema.TableName;
			entryNum1.CE_EntryType = CusEntryNumberTypes.Australia.QuarantineCertificateNumber;
			entryNum1.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNum1.CE_ParentID = quarantineHeader.PK;
			entryNum1.CE_EntryNum = "AU001";

			var entryNum2 = Factory.New<CusEntryNumber>();
			entryNum2.CE_ParentTable = AutoQuarantineExDocHeader.Schema.TableName;
			entryNum2.CE_EntryType = CusEntryNumberTypes.Australia.ECN;
			entryNum2.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNum2.CE_ParentID = quarantineHeader.PK;
			entryNum2.CE_EntryNum = "AU002";

			var entryNum3 = Factory.New<CusEntryNumber>();
			entryNum3.CE_ParentTable = AutoQuarantineExDocHeader.Schema.TableName;
			entryNum3.CE_EntryType = CusEntryNumberTypes.Australia.QuarantineCertificateNumber;
			entryNum3.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNum3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNum3.CE_ParentID = quarantineHeader.PK;
			entryNum3.CE_EntryNum = "AU003";

			Factory.Save();

			var certificateNumbers = quarantineHeader.CertificateNumbers;
			AssertEquals("CertificateNumbers should have 2 item.", 2, certificateNumbers.Count);
			Assert("CertificateNumbers should contain the correct CusEntryNumber", certificateNumbers.Contains(entryNum1));
			Assert("CertificateNumbers should contain the correct CusEntryNumber", certificateNumbers.Contains(entryNum3));

			quarantineHeader.Delete();
			Factory.Save();
			AssertEquals("CertificateNumbers should been deleted while deleting QuarantineHeader.", true, entryNum1.IsDeleted);
			AssertEquals("CertificateNumbers should been deleted while deleting QuarantineHeader.", true, entryNum3.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObject() => quarantineHeader;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			quarantineHeader.Compartments.AddNew();
			quarantineHeader.SupportingInfos.AddNew();
			quarantineHeader.RecommendationLetters.AddNew();
			return quarantineHeader;
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => quarantineHeader;

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			declaration = helper.Declaration;
			quarantineHeader = helper.Header1.QuarantineExDocHeader;
		}

		JobDeclaration declaration;
		QuarantineExDocHeader quarantineHeader;

		internal static void AssertAmendPermissionReadOnlyStatus(QuarantineExDocHeader quarantineHeader, ZPropertyInfo info)
		{
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			Assert("Should be read only", info.ReadOnly);
			quarantineHeader.RequestForPermitStatus = ZString.Empty;
			Assert("Should not be read only", !info.ReadOnly);
		}

		static void AssertReadOnlyStatus_IndicatorDeclarations(QuarantineExDocHeader quarantineHeader, ZPropertyInfo info)
		{
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			Assert(!info.ReadOnly);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
			Assert(!info.ReadOnly);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal;
			Assert(!info.ReadOnly);

			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected;
			Assert(info.ReadOnly);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended;
			Assert(info.ReadOnly);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CtrdCertificateReady;
			Assert(info.ReadOnly);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady;
			Assert(info.ReadOnly);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert(info.ReadOnly);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			Assert(info.ReadOnly);
		}

		EDIMessage CreateInboundUXMLMessage(BusinessObject parent)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "~" + nextInterchangeNum++;
			interchange.EI_From = "NEXDOCS";

			var message = interchange.ContainedMessages.AddNew();
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;

			AssertEquals("interchange contains message", interchange.PK, message.EM_EI);

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = AutoEvents.MessageReceivedCode;
				log.SL_Table = parent.TableName;
				log.SL_Parent = parent.PK;
			}

			var pivot = Factory.New<GenPivot>();
			pivot.XX_RelationType = Core.Constants.GenPivotTypes.XmlEdiMessage;
			pivot.Relation1Object = log;
			pivot.Relation2Object = message;

			return message;
		}

		const string UniversalEvent_Error = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataProvider>NEXDOCS</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>B00001307</Key>
					<Type>CustomsDeclaration</Type>	
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2017-09-29T03:20:49Z</EventTime>
		<EventType>MRR</EventType> 
		<EventReference>MST=LODGE</EventReference>
		<ContextCollection>
			<Context>
				<Type>MessageStatus</Type>
				<Value>ERO</Value>
			</Context>
			<Context>
				<Type>message</Type>
				<Value>Error in operation:</Value>
			</Context>
			<Context>
				<Type>Message</Type>
				<Value>OSB Validate action failed validation</Value>
			</Context>
			<Context>
				<Type>message</Type>
				<Value>Invalid date value: 2017-07-43</Value>
			</Context>
			<Context>
				<Type>message</Type>
				<Value>Expected element 'ownerExporterId@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0' before the end of the content in element exporterDetails@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0</Value>
			</Context>
			<Context>
				<Type>message</Type>
				<Value>Expected element 'productType@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0' instead of 'category@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0' here in element productDetails@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string ImporterOrg1 = @"WALLACE THE IMPORTER
62 WEST WALLABY ST
WASHINGTON 654321 US DC
";

		const string ImporterOrg2 = @"WALLACE THE IMPORTER
62 WEST WALLABY ST
UNKNOWN 654321 DC
";

		const string ImporterOrg3 = @"WALLACE THE IMPORTER
62 WEST WALLABY ST
WASHINGTON 654321 CA
";

		const string ImporterOrg4 = @"WALLACE THE IMPORTER
62 WEST WALLABY ST
WASHINGTON 654321 US
";

		const string ImporterOrg5 = @"WALLACE THE IMPORTER
62 WEST WALLABY ST
WASHINGTON US DC
";

		const string ImporterOrg6 = @"WALLACE THE IMPORTER
THIS HAS TO HAVE APPROXIMATELY 50 CHARACTERS LINE IS A CONTINUATION TO
WASHINGTON 654321 US DC
";

		const string ImporterOrg7 = @"WALLACE THE IMPORTER
62 WEST 
UNKNOWN
";

		int nextInterchangeNum = 1;

		class JobComInvoiceHeaderForTest : JobComInvoiceHeader
		{
			public JobComInvoiceHeaderForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			[ChildEditable(true)]
			[ChildEditableTestExclude]
			public QuarantineExDocHeaderCollectionForTest QuarantineExDocHeaders
			{
				get
				{
					if (quarantineExDocHeaders == null)
					{
						quarantineExDocHeaders = new QuarantineExDocHeaderCollectionForTest(this);
						RegisterEditableChildObject(quarantineExDocHeaders);
					}
					return quarantineExDocHeaders;
				}
			}
			QuarantineExDocHeaderCollectionForTest quarantineExDocHeaders;
		}

		class QuarantineExDocHeaderCollectionForTest : ActiveBusinessObjectCollection<QuarantineExDocHeader>
		{
			public QuarantineExDocHeaderCollectionForTest(JobComInvoiceHeaderForTest invoice)
				: base(invoice)
			{
			}
		}
	}
}
