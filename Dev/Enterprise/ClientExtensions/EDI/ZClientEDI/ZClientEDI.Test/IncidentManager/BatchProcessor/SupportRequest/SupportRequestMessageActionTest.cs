using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.BatchProcessor;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.VersionReporting.BatchProcessor.Testing
{
	public class SupportRequestMessageActionTest : TestCaseWithFactory
	{
		public void TestExecuteAction()
		{
			List<ITransactionParticipant> participant;
			SupportRequestMessageActionForTest action = new SupportRequestMessageActionForTest(new BusinessObjectFactoryProvider(Factory));
			SupportRequestProcessorForTest processor = new SupportRequestProcessorForTest();
			action.Processor = processor;
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.ClientReferenceNumber = "1234";
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest));
			var interchange = SystemMessage.CreateSecureInterchange(Factory, SystemMessageList.Descriptions.CustomerServiceRequest, serializer, request);
			var message = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			((IMessageAction)action).ExecuteAction(message, null, out participant);
			AssertEquals("ProcessCalls", 1, processor.ProcessCalls);
			AssertEquals("ClientReferenceNumber Deserialized", "1234", processor.Request.ClientReferenceNumber);
		}

		public void TestSendNotificationEmail()
		{
			var mailGroup = Factory.New<GlbGroup>();
			var staff = Factory.New<GlbStaff>();
			staff.GS_EmailAddress = "test@123.com.au";
			mailGroup.GG_Code = "AAA";
			mailGroup.Staff.Add(staff);
			Factory.Save();
			EDIDataRegistry.Instance.InternalNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mailGroup.PK.ToGuid());
			var action = new SupportRequestMessageAction(new BusinessObjectFactoryProvider(Factory)) as IMessageAction;
			action.SendNotificationEmail("Success", "", null, true);
			AssertEquals("No email if message successfully processed", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			action.SendNotificationEmail("Fail", "", null, false);
			AssertEquals("Should be notification email if message fails to process", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		class SupportRequestProcessorForTest : ISupportRequestProcessor
		{
			public Xsd.CustomerServiceRequest Request;
			public bool FromEHub;
			public int ProcessCalls;
			public void Process(Xsd.CustomerServiceRequest request, bool fromEHub)
			{
				++ProcessCalls;
				Request = request;
				FromEHub = fromEHub;
			}

			public void ProcessNewEdocs(BusinessObjectFactory factory, ZGuid incidentRequestPk, IEnumerable<ZGuid> eDocPks)
			{
				throw new NotImplementedException();
			}
		}

		class SupportRequestMessageActionForTest : SupportRequestMessageAction
		{
			public SupportRequestMessageActionForTest(BusinessObjectFactoryProvider provider) : base(provider)
			{
			}

			public ISupportRequestProcessor Processor;
			protected override ISupportRequestProcessor CreateProcessor(INotifications notifications)
			{
				return Processor ?? base.CreateProcessor(notifications);
			}
		}
	}
}
