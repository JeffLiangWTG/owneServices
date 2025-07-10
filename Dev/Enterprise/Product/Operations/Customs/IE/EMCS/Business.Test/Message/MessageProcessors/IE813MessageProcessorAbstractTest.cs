using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.EMCS.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE813MessageProcessor))]
	abstract class IE813MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE813MessageProcessor, IIE813>
	{
		protected override ZString MessageText => IEXmlObjectSerializer.Serialize(ie813);

		protected override ZString MessageFriendlyName => "EMCS IE813 Message Processor";

		protected override ZString MessageType => EMCSIncomingMessageTypeList.Codes.IE813;

		protected override IE813MessageProcessor Processor => new IE813MessageProcessor(logger, typeof(TMessageType));

		public void TestDeclarationNotUpdatedForConsignor()
		{
			CreateSetupData();
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			declaration.ZG_JourneyTime = "8H";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.SpecialInstructions = "Some instruction";
			declaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Other;
			declaration.InvoiceNumber = "INV001";
			declaration.InvoiceDate = new ZDate(2020, 2, 15);
			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee;
			declaration.ZG_GuarantorType = "24";
			declaration.EADNumber = "EAD123";
			declaration.CarrierAgentDocumentaryAddress.Address1 = "CarrierAgent Address1";
			declaration.OwnerDocumentaryAddress.Address1 = "OwnerDocumentaryAddress Address1";
			declaration.DestinationWarehouseDocumentaryAddress.Address1 = "DestinationWarehouseDocumentaryAddress Address1";
			declaration.TransporterDocumentaryAddress.Address1 = "TransporterDocumentaryAddress Address1";
			declaration.CusContainers.AddNew().ZG_UnitCode = "3";

			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);

				CombineAssertions(() =>
				{
					AssertEquals("ZG_JourneyTime", "8H", declaration.ZG_JourneyTime);
					AssertEquals("JE_TransportMode", Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
					AssertEquals("SpecialInstructions", "Some instruction", declaration.SpecialInstructions);
					AssertEquals("ZG_TransportArrangement", EMCSTransportArrangementList.Codes.Other, declaration.ZG_TransportArrangement);
					AssertEquals("InvoiceNumber", "INV001", declaration.InvoiceNumber);
					AssertEquals("InvoiceDate", new ZDate(2020, 2, 15), declaration.InvoiceDate);
					AssertEquals("JE_MessageSubType", EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee, declaration.JE_MessageSubType);
					AssertEquals("ZG_GuarantorType", "24", declaration.ZG_GuarantorType);
					AssertEquals("EADNumber", "EAD123", declaration.EADNumber);
					AssertEquals("CarrierAgentDocumentaryAddress", "CarrierAgent Address1", declaration.CarrierAgentDocumentaryAddress.Address1);
					AssertEquals("OwnerDocumentaryAddress", "OwnerDocumentaryAddress Address1", declaration.OwnerDocumentaryAddress.Address1);
					AssertEquals("DestinationWarehouseDocumentaryAddress", "DestinationWarehouseDocumentaryAddress Address1", declaration.DestinationWarehouseDocumentaryAddress.Address1);
					AssertEquals("TransporterDocumentaryAddress", "TransporterDocumentaryAddress Address1", declaration.TransporterDocumentaryAddress.Address1);
					AssertEquals("TransportDetails", "3", string.Join(", ", declaration.CusContainers.Cast<EMCSCusContainer>().Select(x => x.ZG_UnitCode)));
				});
			}
		}

		protected abstract TMessageType CreateMissingOrEmptyElementsMessage();

		public void TestMissingOrEmptyElements()
		{
			ie813 = CreateMissingOrEmptyElementsMessage();
			CreateSetupData();
			declaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Other;
			declaration.InvoiceNumber = "INV001";
			declaration.InvoiceDate = new ZDate(2020, 2, 15);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.SpecialInstructions = "Some instruction";
			declaration.ZG_GuarantorType = "24";
			declaration.CusContainers.AddNew().ZG_UnitCode = "3";
			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				CombineAssertions(() =>
				{
					AssertEquals("SequenceNumber updated", "8888", declaration.SequenceNumber);
					AssertEquals("ZG_TransportArrangement not updated", EMCSTransportArrangementList.Codes.Other, declaration.ZG_TransportArrangement);
					AssertEquals("InvoiceDate not updated", new ZDate(2020, 2, 15), declaration.InvoiceDate);
					AssertEquals("ZG_GuarantorType not updated", "24", declaration.ZG_GuarantorType);
					AssertEquals("JE_TransportMode not updated", Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
					AssertEquals("SpecialInstructions not updated", "Some instruction", declaration.SpecialInstructions);
					AssertEquals("Old OwnerDocumentaryAddress not deleted", false, oldOwnerDocumentaryAddress.IsDeleted);
					AssertEquals("Old OwnerDocumentaryAddress not updated", "OwnerDocumentaryAddress Address1", oldOwnerDocumentaryAddress.Address1);
					AssertEquals("Old DestinationWarehouseDocumentaryAddress not deleted", false, oldDestinationWarehouseDocumentaryAddress.IsDeleted);
					AssertEquals("Old DestinationWarehouseDocumentaryAddress not updated", "DestinationWarehouseDocumentaryAddress Address1", declaration.DestinationWarehouseDocumentaryAddress.Address1);
					AssertEquals("Old CarrierAgentDocumentaryAddress not deleted", false, oldCarrierAgentDocumentaryAddress.IsDeleted);
					AssertEquals("Old CarrierAgentDocumentaryAddress not updated", "CarrierAgent Address1", declaration.CarrierAgentDocumentaryAddress.Address1);
					AssertEquals("Old TransporterDocumentaryAddress not deleted", false, oldTransporterDocumentaryAddress.IsDeleted);
					AssertEquals("Old TransporterDocumentaryAddress not updated", "TransporterDocumentaryAddress Address1", declaration.TransporterDocumentaryAddress.Address1);
					AssertEquals("Existing TransportDetails are not deleted when message doesn't have TransportDetails", 1, declaration.CusContainers.Count);
				});
			}
			ie813 = CreateDefaultIE813Type();
		}

		protected abstract void UpdateTraderIdEmpty();

		public void TestUpdateDeclarationSequenceNumber_TraderIdEmpty()
		{
			UpdateTraderIdEmpty();
			CreateSetupData();
			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				AssertEquals("SequenceNumber updated", "8888", declaration.SequenceNumber);
			}
		}

		protected abstract void UpdateTraderId2NotMatchImporter();

		public void TestUpdateDeclarationSequenceNumber_TraderIdNotMatchImporter()
		{
			UpdateTraderId2NotMatchImporter();
			CreateSetupData();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "A1";
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, Core.Constants.CountryCodes.Germany, "DETI000", orgAddress.PK);
			var cusCode2 = orgHeader.CustomsCodes.AddNew();
			cusCode2.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, Core.Constants.CountryCodes.Latvia, "DETI123", orgAddress.PK);
			var cusCode3 = orgHeader.CustomsCodes.AddNew();
			cusCode3.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, Core.Constants.CountryCodes.Germany, "DETI123", orgAddress.PK);
			declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
			Factory.Save();

			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				AssertNotEquals("SequenceNumber not updated", "8888", declaration.SequenceNumber);
			}
		}

		public void TestUpdateDeclarationSequenceNumber_NoImporterDocumentaryAddress()
		{
			CreateSetupData();
			declaration.ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;

			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				AssertNotEquals("SequenceNumber not updated", "8888", declaration.SequenceNumber);
			}
		}

		protected abstract void UpdateTransportMode2Zero();

		public void TestUpdateDeclarationTransportMode_Zero()
		{
			UpdateTransportMode2Zero();
			CreateSetupData();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.SpecialInstructions = "Some instruction";
			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				CombineAssertions(() =>
				{
					AssertEquals("JE_TransportMode updated", Core.Constants.TransportModes.Other, declaration.JE_TransportMode);
					AssertEquals("SpecialInstructions cleared", ZString.Empty, declaration.SpecialInstructions);
				});
			}
		}

		protected abstract void UpdateTransportMode2Invalid();

		public void TestUpdateDeclarationTransportMode_Invalid()
		{
			UpdateTransportMode2Invalid();
			CreateSetupData();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.SpecialInstructions = "Some instruction";
			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				CombineAssertions(() =>
				{
					AssertEquals("JE_TransportMode not updated", ZString.Empty, declaration.JE_TransportMode);
					AssertEquals("SpecialInstructions updated", "New instruction", declaration.SpecialInstructions);
				});
			}
		}

		protected abstract void UpdateGuarantor2Null();

		public void TestUpdateDeclarationGuarantor_GuarantorNull()
		{
			UpdateGuarantor2Null();
			CreateSetupData();
			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				CombineAssertions(() =>
				{
					AssertEquals("Old OwnerDocumentaryAddress deleted", true, oldOwnerDocumentaryAddress.IsDeleted);
					AssertEquals("New OwnerDocumentaryAddress empty", ZString.Empty, declaration.OwnerDocumentaryAddress.Address1);
				});
			}
		}

		protected abstract void UpdateTransportArranger2Null();

		public void TestUpdateDeclarationCarrierAgent_TransportArrangerNull()
		{
			UpdateTransportArranger2Null();
			CreateSetupData();
			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				CombineAssertions(() =>
				{
					AssertEquals("Old CarrierAgentDocumentaryAddress deleted", true, oldCarrierAgentDocumentaryAddress.IsDeleted);
					AssertEquals("New CarrierAgentDocumentaryAddress empty", ZString.Empty, declaration.CarrierAgentDocumentaryAddress.Address1);
				});
			}
		}

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("Entry Status", EntryStatusList.Codes.CHG, declaration.JE_EntryStatus);
			AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("Declaration - Message Status", "", declaration.JE_MessageStatus);
			AssertEquals("Declaration - Sequence number updated", "8888", declaration.SequenceNumber);
			AssertEquals("ZG_JourneyTime updated", "02D", declaration.ZG_JourneyTime);
			AssertEquals("JE_TransportMode updated", ZString.Empty, declaration.JE_TransportMode);
			AssertEquals("SpecialInstructions cleared", "New instruction", declaration.SpecialInstructions);
			AssertEquals("ZG_TransportArrangement updated", EMCSTransportArrangementList.Codes.OwnerOfGoods, declaration.ZG_TransportArrangement);
			AssertEquals("InvoiceNumber updated", "1", declaration.InvoiceNumber);
			AssertEquals("InvoiceDate updated", new ZDate(2020, 7, 25), declaration.InvoiceDate);
			AssertEquals("JE_MessageSubType updated", EMCSDestinationTypeList.Codes.DestinationDirectDelivery, declaration.JE_MessageSubType);
			AssertEquals("ZG_GuarantorType updated", "3", declaration.ZG_GuarantorType);
			AssertEquals("Old OwnerDocumentaryAddress deleted", true, oldOwnerDocumentaryAddress.IsDeleted);
			AssertEquals("New OwnerDocumentaryAddress updated", "Gp Address 1", declaration.OwnerDocumentaryAddress.Address1);
			AssertEquals("EADNumber updated", "MRN198761234", declaration.EADNumber);
			AssertEquals("Old DestinationWarehouseDocumentaryAddress deleted", true, oldDestinationWarehouseDocumentaryAddress.IsDeleted);
			AssertEquals("New DestinationWarehouseDocumentaryAddress updated", "DP Address 1", declaration.DestinationWarehouseDocumentaryAddress.Address1);
			AssertEquals("Old CarrierAgentDocumentaryAddress deleted", true, oldCarrierAgentDocumentaryAddress.IsDeleted);
			AssertEquals("New CarrierAgentDocumentaryAddress updated", "TAP Address 1", declaration.CarrierAgentDocumentaryAddress.Address1);
			AssertEquals("Old TransporterDocumentaryAddress deleted", true, oldTransporterDocumentaryAddress.IsDeleted);
			AssertEquals("New TransporterDocumentaryAddress updated", "TP Address 1", declaration.TransporterDocumentaryAddress.Address1);
			AssertContainsExactElementsInAnyOrder("Existing TransportDetails are deleted and new TransportDetails are created from message", new ZString[] { "1", "2" }, declaration.CusContainers.Cast<EMCSCusContainer>().Select(x => x.ZG_UnitCode));
			MessageProcessorNotificationTestHelper.AssertEmail(
				"EMCS Change of Destination Response",
				new[] { "Your EMCS Declaration for Job E00000810 received a notification of a changed destination. For details please follow the link to the Job." },
				new string[] { "staff1@where.com" });
		}

		protected override void AssertEndToEndProcessing()
		{
			AssertEquals("Entry Status", EntryStatusList.Codes.CHG, declaration.JE_EntryStatus);
			AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("Declaration - Message Status", "", declaration.JE_MessageStatus);
			AssertEquals("Declaration - Sequence number updated", "8888", declaration.SequenceNumber);
			AssertEquals("ZG_JourneyTime updated", "02D", declaration.ZG_JourneyTime);
			AssertEquals("JE_TransportMode updated", ZString.Empty, declaration.JE_TransportMode);
			AssertEquals("SpecialInstructions cleared", "New instruction", declaration.SpecialInstructions);
			AssertEquals("ZG_TransportArrangement updated", EMCSTransportArrangementList.Codes.OwnerOfGoods, declaration.ZG_TransportArrangement);
			AssertEquals("InvoiceNumber updated", "1", declaration.InvoiceNumber);
			AssertEquals("InvoiceDate updated", new ZDate(2020, 7, 25), declaration.InvoiceDate);
			AssertEquals("JE_MessageSubType updated", EMCSDestinationTypeList.Codes.DestinationDirectDelivery, declaration.JE_MessageSubType);
			AssertEquals("ZG_GuarantorType updated", "3", declaration.ZG_GuarantorType);
			AssertEquals("Old OwnerDocumentaryAddress deleted", true, oldOwnerDocumentaryAddress.IsDeleted);
			AssertEquals("New OwnerDocumentaryAddress updated", "Gp Address 1", declaration.OwnerDocumentaryAddress.Address1);
			AssertEquals("EADNumber updated", "MRN198761234", declaration.EADNumber);
			AssertEquals("Old DestinationWarehouseDocumentaryAddress deleted", true, oldDestinationWarehouseDocumentaryAddress.IsDeleted);
			AssertEquals("New DestinationWarehouseDocumentaryAddress updated", "DP Address 1", declaration.DestinationWarehouseDocumentaryAddress.Address1);
			AssertEquals("Old CarrierAgentDocumentaryAddress deleted", true, oldCarrierAgentDocumentaryAddress.IsDeleted);
			AssertEquals("New CarrierAgentDocumentaryAddress updated", "TAP Address 1", declaration.CarrierAgentDocumentaryAddress.Address1);
			AssertEquals("Old TransporterDocumentaryAddress deleted", true, oldTransporterDocumentaryAddress.IsDeleted);
			AssertEquals("New TransporterDocumentaryAddress updated", "TP Address 1", declaration.TransporterDocumentaryAddress.Address1);
			AssertContainsExactElementsInAnyOrder("Existing TransportDetails are deleted and new TransportDetails are created from message", new ZString[] { "1", "2" }, declaration.CusContainers.Cast<EMCSCusContainer>().Select(x => x.ZG_UnitCode));
			MessageProcessorNotificationTestHelper.AssertEmail(
				"EMCS Change of Destination Response",
				new[] { "Your EMCS Declaration for Job E00000810 received a notification of a changed destination. For details please follow the link to the Job." },
				new string[] { "staff1@where.com" });
		}

		protected override void CreateSetupData()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "A1";
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, Core.Constants.CountryCodes.Latvia, "LVTI002", orgAddress.PK);
			Factory.Save();

			base.CreateSetupData();
			declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.SpecialInstructions = "Some instruction";
			oldCarrierAgentDocumentaryAddress = declaration.CarrierAgentDocumentaryAddress;
			oldCarrierAgentDocumentaryAddress.Address1 = "CarrierAgent Address1";
			oldOwnerDocumentaryAddress = declaration.OwnerDocumentaryAddress;
			oldOwnerDocumentaryAddress.Address1 = "OwnerDocumentaryAddress Address1";
			oldDestinationWarehouseDocumentaryAddress = declaration.DestinationWarehouseDocumentaryAddress;
			oldDestinationWarehouseDocumentaryAddress.Address1 = "DestinationWarehouseDocumentaryAddress Address1";
			oldTransporterDocumentaryAddress = declaration.TransporterDocumentaryAddress;
			declaration.TransporterDocumentaryAddress.Address1 = "TransporterDocumentaryAddress Address1";
		}

		protected abstract TMessageType CreateDefaultIE813Type();
		protected TMessageType ie813;

		JobDocAddress oldCarrierAgentDocumentaryAddress;
		JobDocAddress oldOwnerDocumentaryAddress;
		JobDocAddress oldDestinationWarehouseDocumentaryAddress;
		JobDocAddress oldTransporterDocumentaryAddress;

		protected override void SetUp()
		{
			base.SetUp();
			ie813 = CreateDefaultIE813Type();
		}
	}
}
