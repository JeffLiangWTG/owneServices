using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class eNettHelperTest : TestCaseWithFactory
	{
		public void TestGetBrokersENettRegistrationNumber()
		{
			OrgHeader supplierBuyerOrganization = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();

			var brokerAIR = NewBroker("TSTBRK001");
			var brokerSEA = NewBroker("TSTBRK002");
			var brokerBuyer = NewBroker("TSTBRK003");

			NewSupplierBuyerLink(supplierBuyerOrganization, brokerSEA, brokerBuyer, "ALL", "");
			NewConsigneeRelatedParty(supplierBuyerOrganization, "SEA", "", brokerSEA.PK);
			NewConsigneeRelatedParty(consignee, "AIR", "", brokerAIR.PK);
			NewConsigneeRelatedParty(consignee, "SEA", "", brokerSEA.PK);

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = nameof(FreightMode.AIR);
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";

			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = shipment.TablePrefix;

			Factory.Save();

			APInvoice transaction = Factory.NewWithValidTestData<APInvoice>();
			transaction.AH_OH = consignee.PK;
			transaction.AH_JH = jobHeader.PK;

			AssertEquals(ZString.Empty, eNettHelper.GetBrokersENettRegistrationNumber(transaction));

			consignee.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Broker;
			AssertEquals("TSTBRK001", eNettHelper.GetBrokersENettRegistrationNumber(transaction));

			consignee.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Both;
			AssertEquals("TSTBRK001", eNettHelper.GetBrokersENettRegistrationNumber(transaction));

			shipment.JS_TransportMode = nameof(FreightMode.SEA);
			AssertEquals(ZString.Empty, eNettHelper.GetBrokersENettRegistrationNumber(transaction));

			consignee.MiscServ.OM_IMSendSeaImportDocsTo = OrgConstants.SendDocsTo.Broker;
			AssertEquals("TSTBRK002", eNettHelper.GetBrokersENettRegistrationNumber(transaction));

			consignee.MiscServ.OM_IMSendSeaImportDocsTo = OrgConstants.SendDocsTo.Both;
			AssertEquals("TSTBRK002", eNettHelper.GetBrokersENettRegistrationNumber(transaction));

			shipment.ConsigneePK = supplierBuyerOrganization.PK;
			transaction.AH_OH = supplierBuyerOrganization.PK;
			AssertEquals("TSTBRK003", eNettHelper.GetBrokersENettRegistrationNumber(transaction));
		}

		public void TestGetBrokersENettRegistrationNumber_ContainerMode()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();

			var orgBrokerSEA = NewBroker("BrokerSEA");
			var orgBrokerSEAFCL = NewBroker("BrokerSEAFCL");
			var orgBrokerALL = NewBroker("BrokerALL");

			var brokerBuyerSEA = NewBroker("brokerBuyerSEA");
			var brokerBuyerSEAFCL = NewBroker("brokerBuyerSEAFCL");
			var brokerBuyerALL = NewBroker("brokerBuyerALL");

			NewSupplierBuyerLink(consignee, orgBrokerSEA, brokerBuyerSEA, "SEA", "");
			NewSupplierBuyerLink(consignee, orgBrokerSEAFCL, brokerBuyerSEAFCL, "SEA", "FCL");
			NewSupplierBuyerLink(consignee, orgBrokerALL, brokerBuyerALL, "ALL", "");

			NewConsigneeRelatedParty(consignee, "SEA", "", orgBrokerSEA.PK);
			NewConsigneeRelatedParty(consignee, "SEA", "FCL", orgBrokerSEAFCL.PK);
			NewConsigneeRelatedParty(consignee, "ALL", "", orgBrokerALL.PK);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUSYD";

			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = shipment.TablePrefix;
			Factory.Save();

			APInvoice transaction = Factory.NewWithValidTestData<APInvoice>();
			transaction.AH_OH = consignee.PK;

			transaction.AH_JH = jobHeader.PK;

			consignee.MiscServ.OM_IMSendSeaImportDocsTo = OrgConstants.SendDocsTo.Broker;
			AssertEquals("brokerBuyerSEAFCL", eNettHelper.GetBrokersENettRegistrationNumber(transaction));

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			AssertEquals("brokerBuyerSEA", eNettHelper.GetBrokersENettRegistrationNumber(transaction));

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			AssertEquals("brokerBuyerALL", eNettHelper.GetBrokersENettRegistrationNumber(transaction));
		}

		public void TestCheckRelatedENettMessages()
		{
			var transaction = Factory.NewWithValidTestData<APInvoice>();
			AssertStaticMethods(transaction, false, false);

			var message = Factory.NewWithValidTestData<EDIMessage>();
			AssertStaticMethods(transaction, false, false);

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.eNett;
			message.EM_MessageType = "ENE";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_LinkTable = AccTransactionHeader.Schema.TableName;
			AssertStaticMethods(transaction, false, false);

			message.EM_LinkUniqueID = transaction.PK;
			AssertStaticMethods(transaction, true, false);

			message.EM_Status = EDIMessage.Status.Sent;
			AssertStaticMethods(transaction, true, true);

			message.EM_LinkTable = ForwardingShipment.Schema.TableName;
			AssertStaticMethods(transaction, false, false);
		}

		void AssertStaticMethods(TransactionHeader transaction, bool exists, bool sent)
		{
			AssertEquals("Related eNett Message exists", exists, eNettHelper.ENettMessageAlreadyExists(transaction));
			AssertEquals("Related eNett Message exists", exists, eNettHelper.ENettMessageAlreadyExists(Factory, transaction.PK));
			AssertEquals("Related eNett Message has been sent", sent, eNettHelper.ENettMessageHasBeenSucessfullySent(transaction));
			AssertEquals("Related eNett Message has been sent", sent, eNettHelper.ENettMessageHasBeenSucessfullySent(Factory, transaction.PK));
		}

		OrgHeader NewBroker(string customsCodes)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.eNettRegistrationNumber, customsCodes);
			orgHeader.OH_IsDebtor = true;
			return orgHeader;
		}

		OrgRelatedParty NewConsigneeRelatedParty(OrgHeader orgheader, string transportMode, string containerMode, ZGuid relatedPartyPK)
		{
			var relatedParty = orgheader.ConsigneeRelatedParties.AddNew();
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			relatedParty.PR_FreightTransportMode = transportMode;
			relatedParty.PR_FreightContainerMode = containerMode;
			relatedParty.PR_OH_RelatedParty = relatedPartyPK;
			return relatedParty;
		}

		OrgSupplierBuyerLink NewSupplierBuyerLink(OrgHeader parent, OrgHeader supplier, OrgHeader broker, string transportMode, string containerMode)
		{
			var supplierBuyer = parent.SupplierLinks.AddNew(supplier);
			supplierBuyer.OL_SendImportDocsTo = OrgConstants.SendDocsTo.Broker;
			supplierBuyer.OL_OH_ImportBroker = broker.PK;
			supplierBuyer.OrgSupBuyLinkTrnModes[0].PF_TransportMode = transportMode;
			supplierBuyer.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = containerMode;
			return supplierBuyer;
		}
	}
}