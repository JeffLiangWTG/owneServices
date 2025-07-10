using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	sealed class JobDeclarationEventParentFinderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetLogParentsForEventUsingContextForCSWResponseMessage()
		{
			var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			entryHeader.CH_Status = "AWO";
			entryHeader.CH_BGMReference = "000000000000194233";

			var outgoingMessage = Factory.NewWithValidTestData<XmlEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "UDM";
			outgoingMessage.EM_MessageType = "XUS";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entryHeader.Messages.Add(outgoingMessage);
			Factory.SaveForTesting();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(CSWResponseMessageEventProcessorTest.incomingMDLEvent);

			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			AssertEquals(1, logParents.Length);
			AssertEquals(entryHeader, logParents[0]);
		}

		JobDeclaration Declaration => Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001139"));

		JobDeclarationEventParentFinder GetNewEventParentFinderWithLogger(IXmlImportLogger logger)
		{
			return new JobDeclarationEventParentFinder(Factory.BOFactory, new JobDeclarationDataContextManager(), logger);
		}

		protected override void SetUp()
		{
			base.SetUp();

			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			try
			{
				//GlbCompany.CurrentCompany.SetCountry changes GC_RN_NKCountryCode, and needs to be saved to db as JobDeclarationFilter(DBOnlyQuery) is performed in FilterObject.
				GlbCompany.CurrentCompany.Factory.Save();
			}
			finally
			{
				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
			}

			var factory = new BusinessObjectFactory();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_DeclarationReference = "B00001139";
			declaration.JE_HouseBill = "20247654321";
			declaration.JE_MasterBill = "081-203212";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			factory.Save();
		}
	}
}
