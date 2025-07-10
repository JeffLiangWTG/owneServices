using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.Message.MessageBuilder;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILGEN920MessageBuilderTest : ILMessageBuilderBaseTest
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("company is must", () => new ILGEN920MessageBuilder(null, null));
		}

		protected override string GetExpectedMessageSubType()
		{
			return ILEDIMessageSubTypeList.Codes.SyncAcknowledgementMessage;
		}

		protected override string GetExpectedMessageType()
		{
			return "GEN";
		}

		protected override BusinessObject GetLinkedObject()
		{
			return null;
		}

		protected override IMessageBuilder GetMessageBuilder()
		{
			return new ILGEN920MessageBuilder(company, correlationIdList);
		}

		protected override IBusinessObjectCollection GetMessageOwnerCollection()
		{
			return null;
		}

		protected override string GetExpectedMessageText()
		{
			return @"<?xml version=""1.0"" encoding=""utf-8""?>
<NG_9200_OutgoingMessageDeliveryApproval xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://malam.com/customs/SystemTables/NG_9200_OutgoingMessageDeliveryApproval"">
  <RequestContentHeader>
    <TransmitionDateTime xmlns=""http://malam.com/customs/EAICommon.xsd"">2024-09-09T00:00:00Z</TransmitionDateTime>
    <SenderID xmlns=""http://malam.com/customs/EAICommon.xsd"">560038416</SenderID>
    <RecieverID xmlns=""http://malam.com/customs/EAICommon.xsd"">0</RecieverID>
    <Convertor xsi:nil=""true"" xmlns=""http://malam.com/customs/EAICommon.xsd"" />
  </RequestContentHeader>
  <ListOfCorrelationIDs>
    <CorrelationIDs>8055B076-0BAB-4E44-BB7D-696DCD350964</CorrelationIDs>
  </ListOfCorrelationIDs>
  <ListOfCorrelationIDs>
    <CorrelationIDs>181222C7-AC0B-4DFB-B048-7BD2600822FC</CorrelationIDs>
  </ListOfCorrelationIDs>
</NG_9200_OutgoingMessageDeliveryApproval>";
		}

		protected override void SetUp()
		{
			base.SetUp();
			company = GlbCompany.CurrentCompany;
			correlationIdList = new List<ZString>();
			correlationIdList.Add("8055B076-0BAB-4E44-BB7D-696DCD350964");
			correlationIdList.Add("181222C7-AC0B-4DFB-B048-7BD2600822FC");
		}
		GlbCompany company;
		List<ZString> correlationIdList;
	}
}
