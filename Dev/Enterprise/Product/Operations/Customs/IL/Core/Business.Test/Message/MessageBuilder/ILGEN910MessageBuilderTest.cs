using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.Message.MessageBuilder;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILGEN910MessageBuilderTest : ILMessageBuilderBaseTest
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("company is must", () => new ILGEN910MessageBuilder(null, null));
			AssertExceptionThrown<ArgumentNullException>("logger is must", () => new ILGEN910MessageBuilder(GlbCompany.CurrentCompany, null));
		}

		[TestDate(2025, 03, 26, 15, 15, 0)]
		public void TestOutgoingMessageRequest_9100_WhenPeekWayIs3()
		{
			var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			credential.GP_MailBoxID = "560038416";
			Factory.Save();

			var dcaParameters_Company2 = new DCAParameters() { PeekWay = PeekWayList.Codes._3, AllServices = false, MaxMessagesPerIteration = 400 };
			dcaParameters_Company2.Services.Add(new DCAService() { Name = "Service1", NextRunDateTime = ZDateTime.UtcNow.AddDays(-1) });
			dcaParameters_Company2.Services.Add(new DCAService() { Name = "Service2" });

			using (ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dcaParameters_Company2))
			{
				var messageBuilder = GetMessageBuilder();
				var result = messageBuilder.PopulateMessages();
				Factory.Save();

				CombineAssertions("When PeekWay Is 3", () =>
				{
					AssertNotNull(result);
					Assert("Success", result.IsSuccess);
					var dcaParameters = ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					foreach (var service in dcaParameters.Services.Cast<DCAService>())
					{
						AssertEquals("Next Run Time has been set to now", ZDateTime.UtcNow, service.NextRunDateTime);
					}

					var query = new ZQuery();
					query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ILEDIInterchange.ApplicationCodes.ILCustoms);
					query.AddToFilter(EDIMessageSchema.EM_MessageType, GetExpectedMessageType());
					query.AddToFilter(EDIMessageSchema.EM_MessageSubType, GetExpectedMessageSubType());

					var messages = Factory.Load<EDIMessage>(query);
					AssertNotNull("Message were created", messages);
					AssertEquals("There should be 2 messages", 2, messages.Length);

					foreach (var message in messages)
					{
						AssertEquals("EM_ApplicationCode", "ILC", message.EM_ApplicationCode);
						AssertEquals("EM_MessageType", GetExpectedMessageType(), message.EM_MessageType);
						AssertEquals("EM_MessageSubType", GetExpectedMessageSubType(), message.EM_MessageSubType);
						AssertEquals("EM_Status", "QUE", message.EM_Status);
						AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);

						AssertEquals("EM_GP", ZGuid.Empty, message.EM_GP);
						AssertEquals("EM_MessageOwner", "ILCOM_REGISTERNO", message.EM_MessageOwner);
					}

					var expectedMessageService1 = new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("OutgoingMessageRequest_9100_Service1.xml"));
					var expectedMessageService2 = new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("OutgoingMessageRequest_9100_Service2.xml"));
					AssertContainsExactElementsInAnyOrder("EM_MessageText", new string[] { expectedMessageService1, expectedMessageService2 }, messages.Select(x => EliminateMeaninglessDiffs(x.EM_MessageText)));
				});
			}
		}

		[TestDate(2025, 03, 26, 15, 15, 0)]
		public void TestOutgoingMessageRequest_9100_WhenPeekWayIs2()
		{
			var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			credential.GP_MailBoxID = "560038416";
			Factory.Save();

			var dcaParameters_PeekWay2 = new DCAParameters() { PeekWay = PeekWayList.Codes._2, MaxMessagesPerIteration = 400 };
			using (ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dcaParameters_PeekWay2))
			{
				var messageBuilder = GetMessageBuilder();
				var result = messageBuilder.PopulateMessages();
				Factory.Save();

				CombineAssertions("When PeekWay Is 2 ", () =>
				{
					AssertNotNull(result);
					Assert("Success", result.IsSuccess);
					var query = new ZQuery();
					query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ILEDIInterchange.ApplicationCodes.ILCustoms);
					query.AddToFilter(EDIMessageSchema.EM_MessageType, GetExpectedMessageType());
					query.AddToFilter(EDIMessageSchema.EM_MessageSubType, GetExpectedMessageSubType());

					var messages = Factory.Load<EDIMessage>(query);
					AssertNotNull("Message were created", messages);
					AssertEquals("There should be 1 messages", 1, messages.Length);
					var message = messages.Single();
					AssertEquals("EM_ApplicationCode", "ILC", message.EM_ApplicationCode);
					AssertEquals("EM_MessageType", GetExpectedMessageType(), message.EM_MessageType);
					AssertEquals("EM_MessageSubType", GetExpectedMessageSubType(), message.EM_MessageSubType);
					AssertEquals("EM_Status", "QUE", message.EM_Status);
					AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
					AssertEquals("EM_GP", ZGuid.Empty, message.EM_GP);
					AssertEquals("EM_MessageOwner", "ILCOM_REGISTERNO", message.EM_MessageOwner);

					var expectedMessageService1 = new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("OutgoingMessageRequest_9100_WhenPeekWayIs2.xml"));
					var actualValue = EliminateMeaninglessDiffs(message.EM_MessageText.ToString());
					AssertEquals("EM_MessageText", expectedMessageService1, actualValue);
				});
			}
		}

		string EliminateMeaninglessDiffs(string xmlContent)
		{
			return xmlContent.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
		}

		protected override string GetExpectedMessageSubType() => ILEDIMessageSubTypeList.Codes.SyncOutgoingMessageRequest;

		protected override string GetExpectedMessageType() => ILMessageTypeList.Codes.GEN;

		protected override BusinessObject GetLinkedObject() => null;

		protected override IMessageBuilder GetMessageBuilder() => new ILGEN910MessageBuilder(GlbCompany.CurrentCompany, new LoggerWrapper(new TestServiceLogger()));

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => null;

		protected override string GetExpectedMessageText() => new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("OutgoingMessageRequest_9100.xml"));

		protected override void SetUp()
		{
			base.SetUp();
			disposable = ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DCAParameters() { PeekWay = "2", AllServices = true, MaxMessagesPerIteration = 300 });
		}
		IDisposable disposable;

		protected override void TearDown()
		{
			base.TearDown();
			disposable?.Dispose();
		}
	}
}
