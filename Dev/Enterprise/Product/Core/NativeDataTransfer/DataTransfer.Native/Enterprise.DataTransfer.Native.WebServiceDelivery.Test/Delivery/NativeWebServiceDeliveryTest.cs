using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.DataTransfer.Native.WebServiceDelivery.LocalService;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.WebServiceDelivery.Testing
{
	sealed class NativeWebServiceDeliveryTest : TestCaseWithFactory
	{
		public void TestShouldHandleException()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var group = SetupEmail();
			NotificationDataRegistry.Instance.EDIMessageDeliveryFailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			delivery.ErrorNotifier = new EmailNotifier();

			AssertNoExceptionThrown(
				"Should be able to handle exception",
				delegate
				{
					response.Setup(r => r.HasError).Returns(true);
					response.Setup(r => r.ErrorMessage).Returns("Error");
					response.Setup(r => r.ResponseMessage).Returns("Here we go!");
					mode.Setup(mode => mode.EK_Destination).Returns("http://bansai.com");
					messageConverter.Setup(messageConverter => messageConverter.Convert(It.IsAny<IEDICommunicationsMode>())).Returns(new RequestMessageData());
					nativeDataService.Setup(nativeDataService => nativeDataService.Update(It.IsAny<IRequestMessage>())).Returns(response.Object);
					delivery.Deliver(null, mode.Object, null);

					mocks.VerifyAll();
				});

			AssertEquals("Precondition: Email should be created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Two attachment should be created", 1, email.Attachments.Count);
		}

		public void TestLogsGetCreatedOnParent()
		{
			var parentBO = Factory.New<DummyEnterpriseBusinessObject>();
			Factory.Save();

			var deliverContext = new DeliveryContext(Factory) { ParentInfo = EntityInfo.New(parentBO) };

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode);
			query.AddToFilter(StmALogSchema.SL_Table, deliverContext.ParentInfo.TableName);
			query.AddToFilter(StmALogSchema.SL_Parent, deliverContext.ParentInfo.InternalPK);

			AssertEquals("Precondition: No DataExport events exist", 0, Factory.Load<BaseStmALog>(query).Length);

			response.Setup(response => response.HasError).Returns(false);

			mode.Setup(mode => mode.EK_Destination).Returns("http://bansai.com");
			messageConverter.Setup(messageConverter => messageConverter.Convert(It.IsAny<IEDICommunicationsMode>())).Returns(new RequestMessageData());
			nativeDataService.Setup(nativeDataService => nativeDataService.Update(It.IsAny<IRequestMessage>())).Returns(response.Object);
			delivery.Deliver(deliverContext, mode.Object, null);
			mocks.VerifyAll();

			var dataExportEvents = Factory.Load<BaseStmALog>(query);
			AssertEquals("One DataExport event exists", 1, dataExportEvents.Length);
			AssertEquals("SL_Reference in DataExport event", "Native XML Connector Export Succeeded", dataExportEvents[0].SL_Reference);
			AssertEquals("dataExportEvents[0].IsInDatabase", false, dataExportEvents[0].IsInDatabase);
			Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestHandleResponse_WhenUriIsInvalid()
		{
			mode.Setup(mode => mode.EK_Destination).Returns("Invalid URI");
			delivery.Deliver(null, mode.Object, null);
			errorNotifier.Verify(errorNotifier => errorNotifier.Notify(It.IsAny<BusinessObjectFactory>(), It.IsAny<Exception>(), It.IsAny<IEDICommunicationsMode>(), It.IsAny<Stream>(), It.IsAny<Stream>()), Times.Once);
			messageConverter.Verify(messageConverter => messageConverter.Convert(It.IsAny<IEDICommunicationsMode>()), Times.Never);
			nativeDataService.Verify(nativeDataService => nativeDataService.Update(It.IsAny<IRequestMessage>()), Times.Never);
			mocks.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestHandleResponse_WhenResponseHasError()
		{
			response.Setup(response => response.HasError).Returns(true);
			mode.Setup(mode => mode.EK_Destination).Returns("http://www.google.com");
			messageConverter.Setup(messageConverter => messageConverter.Convert(It.IsAny<IEDICommunicationsMode>())).Returns(new RequestMessageData());
			nativeDataService.Setup(nativeDataService => nativeDataService.Update(It.IsAny<IRequestMessage>())).Returns(response.Object);
			delivery.Deliver(null, mode.Object, null);
			mocks.VerifyAll();
			errorNotifier.Verify(errorNotifier => errorNotifier.Notify(It.IsAny<BusinessObjectFactory>(), It.IsAny<Exception>(), It.IsAny<IEDICommunicationsMode>(), It.IsAny<Stream>(), It.IsAny<Stream>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestHandleResponse_WhenResponseDoNotHasError()
		{
			response.Setup(response => response.HasError).Returns(false);
			mode.Setup(mode => mode.EK_Destination).Returns("http://www.google.com");
			messageConverter.Setup(messageConverter => messageConverter.Convert(It.IsAny<IEDICommunicationsMode>())).Returns(new RequestMessageData()).Verifiable();
			nativeDataService.Setup(nativeDataService => nativeDataService.Update(It.IsAny<IRequestMessage>())).Returns(response.Object).Verifiable();
			errorNotifier.Verify(errorNotifier => errorNotifier.Notify(It.IsAny<BusinessObjectFactory>(), It.IsAny<Exception>(), It.IsAny<IEDICommunicationsMode>(), It.IsAny<Stream>(), It.IsAny<Stream>()), Times.Never);
			delivery.Deliver(null, mode.Object, null);
			mocks.VerifyAll();
		}

		protected override void SetUp()
		{
			base.SetUp();
			mocks = new MockRepository(MockBehavior.Loose);
			delivery = new NativeWebServiceDelivery();
			messageConverter = mocks.Create<IEDICommunicationsModeConverter>();
			errorNotifier = mocks.Create<IErrorNotifier<IEDICommunicationsMode>>();
			response = mocks.Create<IResponseMessage>();
			nativeDataService = mocks.Create<INativeDataService>();
			mode = mocks.Create<IEDICommunicationsMode>();

			delivery.MessageConverter = messageConverter.Object;
			delivery.ErrorNotifier = errorNotifier.Object;
			delivery.Service = nativeDataService.Object;
		}

		GlbGroup SetupEmail()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "mickey.mouse@cargowise.com";
			staff.GS_Code = "ZAC";
			Enterprise.Registry.Business.NotificationDataRegistry.Instance.EDIMessageDeliveryFailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			Factory.Save();
			return group;
		}

		MockRepository mocks;
		Mock<IEDICommunicationsModeConverter> messageConverter;
		Mock<IErrorNotifier<IEDICommunicationsMode>> errorNotifier;
		Mock<IResponseMessage> response;
		Mock<INativeDataService> nativeDataService;
		NativeWebServiceDelivery delivery;
		Mock<IEDICommunicationsMode> mode;
	}
}
