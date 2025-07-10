using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Netting;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.DataTransfer.Universal.Netting.Testing
{
	public class NettingTransactionReExporterTest : TestCaseWithFactory
	{
		public void TestReExport()
		{
			using (Factory.AddDisposableService())
			{
				var testObjectCreator = new TestObjectCreator(Factory);
				var senderEHubID = "EDISND001";
				var receiverEHubID = "EDIRCV001";
				var nettingSystemEHubID = "EDIWNS001";
				var ns = SetupNettingSystem(nettingSystemEHubID);

				var recipientParticipant = Factory.New<NettingOrganisation>();
				recipientParticipant.NSO_OH_Organisation = testObjectCreator.ABIGAS.PK;
				recipientParticipant.NSO_NS_NettingSystem = ns.PK;
				recipientParticipant.NSO_NettingType = "FUL";

				testObjectCreator.ABIGAS.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, receiverEHubID);
				AddEdiCommunicationMode(GlbBranch.CurrentBranch.OrgProxy, receiverEHubID);

				Factory.Save();

				var message = GetOriginalMessage(senderEHubID, nettingSystemEHubID);
				var universalTransaction = GetUniversalTransaction("AR", receiverEHubID, 120M, referenceNumber: "S1000000");

				AssertNotNull("Role 'WNS' should be present in role collection", universalTransaction.DataContext.RecipientRoleCollection.FirstOrDefault(x => x.Code.GetValueOrDefault() == RecipientRoleType.WNS));
				var reExporter = new UniversalTransactionReExporter(Factory, new TestErrorLogger());
				var ner = Factory.NewWithValidTestData<NettingReceivableTransaction>();
				Assert("Message should be successfully re-exported", reExporter.ReExport(message, universalTransaction, ner));
				Factory.Save();

				AssertNotNull("Role 'WNS' should be present in role collection", universalTransaction.DataContext.RecipientRoleCollection.FirstOrDefault(x => x.Code.GetValueOrDefault() == RecipientRoleType.WNS));
				AssertNotNull("Role 'IDB' should be present in role collection as well", universalTransaction.DataContext.RecipientRoleCollection.FirstOrDefault(x => x.Code.GetValueOrDefault() == RecipientRoleType.IDB));
			}
		}

		[ExpectNoExceptions]
		public void TestReExport_MissingEdiCommunicationMode()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var senderEHubID = "EDISND001";
			var receiverEHubID = "EDIRCV001";
			var nettingSystemEHubID = "EDIWNS001";
			var ns = SetupNettingSystem(nettingSystemEHubID);

			var recipientParticipant = Factory.New<NettingOrganisation>();
			recipientParticipant.NSO_OH_Organisation = testObjectCreator.ABIGAS.PK;
			recipientParticipant.NSO_NS_NettingSystem = ns.PK;
			recipientParticipant.NSO_NettingType = "FUL";

			testObjectCreator.ABIGAS.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, receiverEHubID);

			Factory.Save();

			var message = GetOriginalMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransaction("AR", receiverEHubID, 120M, referenceNumber: "S1000000");

			var errorLog = new TestErrorLogger();
			var reExporter = new UniversalTransactionReExporter(Factory, errorLog);
			var ner = Factory.NewWithValidTestData<NettingReceivableTransaction>();

			AssertEquals("Message should not be exported", false, reExporter.ReExport(message, universalTransaction, ner));

			Assert(errorLog.HasErrors);
			Assert(errorLog.Logs.Contains("No EDI Communications setup found for E Hub ID: 'EDIRCV001' in Netting Center Organization."));
		}

		IEDIMessage GetOriginalMessage(string sender, string receiver)
		{
			var newFactory = new BusinessObjectFactory();
			var message = newFactory.New<IEDIMessage>();

			var interchange = newFactory.New<IEDIInterchange>();
			interchange.EI_From = sender;
			interchange.EI_To = receiver;

			message.EM_EI = interchange.PK;

			return message;
		}

		UniversalTransaction GetUniversalTransaction(string ledger, string receiverEHubID, decimal amount, string referenceNumber = "")
		{
			var universalTransaction = new UniversalTransaction(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.DataContext = DataContextFactory.New();
			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.WNS } }
			});

			universalTransaction.Ledger = ledger;
			universalTransaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);

			var country = new Country();
			country.Code = "AU";
			country.Name = "Australia";

			universalTransaction.OrganizationAddress.Country = country;

			universalTransaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			UniversalDataBuss.DataObjects.Universal.RegistrationNumber hid = new UniversalDataBuss.DataObjects.Universal.RegistrationNumber();
			var type = new RegistrationNumberType();
			type.Code = "HID";

			hid.Type = type;
			hid.CountryOfIssue = country;
			hid.Value = receiverEHubID;
			universalTransaction.OrganizationAddress.RegistrationNumberCollection.Add(hid);

			universalTransaction.TransactionDate = ZDateTime.Now;
			universalTransaction.DueDate = ZDateTime.Now;
			universalTransaction.OSTotal = amount;
			var currency = new Currency();
			currency.Code = "AUD";
			currency.Description = "Australian Dollar";
			universalTransaction.OSCurrency = currency;

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = GetNewDataContext();

			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.ContainerCollection.Add(container);

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			var additionalReference = new AdditionalReference();
			additionalReference.Type = new EntryType();
			shipment.AdditionalReferenceCollection.Add(additionalReference);

			var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			shipment.SubShipmentCollection.Add(subShipment);
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());
			universalTransaction.ShipmentCollection.Add(shipment);

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { Content = CollectionContent.Complete });
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.PackingLineCollection.Add(packingLine);

			subShipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.Instance);
			subShipment.LocalProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>());
			var orderNumber = new OrderNumber();
			subShipment.LocalProcessing.OrderNumberCollection.Add(orderNumber);

			var postingJournal1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			postingJournal1.Job = new EntityReference();
			postingJournal1.Job.Key = "S001001";
			postingJournal1.OSTotalAmount = amount / 2;
			postingJournal1.OSCurrency = currency;

			var postingJournal2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			postingJournal2.Job = new EntityReference();
			postingJournal2.Job.Key = "S001002";
			postingJournal2.OSTotalAmount = amount - postingJournal1.OSTotalAmount;
			postingJournal2.OSCurrency = currency;

			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.PostingJournalCollection.Add(postingJournal1);
			universalTransaction.PostingJournalCollection.Add(postingJournal2);

			return universalTransaction;
		}

		IDataContextDataObject GetNewDataContext()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.ForwardingShipment, "S1000000");

			return dataContext;
		}

		NettingSystem SetupNettingSystem(string eHubID)
		{
			var ns = Factory.New<NettingSystem>();
			ns.NS_Code = eHubID;
			ns.NS_GC = GlbCompany.CurrentCompany.PK;
			ns.NS_Description = "bla bla";

			var creator = new TestObjectCreator(Factory);
			var nsOrg = creator.CreateOrgHeader("TSTNETT1", true, true);

			nsOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, eHubID);

			return ns;
		}

		void AddEdiCommunicationMode(OrgHeader orgHeader, string eHubId)
		{
			EDICommunicationsModeDependentCollection modes_ns = new EDICommunicationsModeDependentCollection(orgHeader);
			var nsMode = modes_ns.AddNew();
			nsMode.EK_Module = EDICommunicationsMode.Modules.Netting;
			nsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction;
			nsMode.EK_CommunicationsTransport = "HUB";
			nsMode.EK_Destination = eHubId;

			orgHeader.Factory.Save();
		}
	}
}
