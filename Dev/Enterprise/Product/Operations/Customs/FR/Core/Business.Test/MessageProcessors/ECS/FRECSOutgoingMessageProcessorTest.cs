using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class FRECSOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessorFilters()
		{
			FRCustomsDataRegistry.Instance.RecipientID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABCDEFG");

			var deBranch = Factory.New<GlbBranch>();
			deBranch.GB_RN_NKCountryCode = "DE";
			deBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			deBranch.GB_Code = "DEB";
			deBranch.GB_IsActive = false;

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "NEW";
			var otherBranch = Factory.New<GlbBranch>();
			otherBranch.GB_RN_NKCountryCode = "FR";
			otherBranch.GB_GC = company.PK;
			otherBranch.GB_Code = "FRO";

			var helper = new ECSMessageTestHelper();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			var msg1 = helper.CreateTestEDIMessage(Factory, branch.PK, EDIInterchangeTypeList.Codes.AUCustoms, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.ECS, MessageSubTypeList.Codes.ARR, EDIMessageStatusList.Codes.Queued, helper.CresteEMMessageText("001"));
			var msg2 = helper.CreateTestEDIMessage(Factory, deBranch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.ECS, MessageSubTypeList.Codes.ARR, EDIMessageStatusList.Codes.Queued, helper.CresteEMMessageText("002"));
			var msg3 = helper.CreateTestEDIMessage(Factory, branch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Receive, MessageTypeList.Codes.ECS, MessageSubTypeList.Codes.ARR, EDIMessageStatusList.Codes.Queued, helper.CresteEMMessageText("003"));
			var msg4 = helper.CreateTestEDIMessage(Factory, branch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.CIN, MessageSubTypeList.Codes.ARR, EDIMessageStatusList.Codes.Queued, helper.CresteEMMessageText("004"));
			var msg5 = helper.CreateTestEDIMessage(Factory, branch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.ECS, MessageSubTypeList.Codes.EXC, EDIMessageStatusList.Codes.Queued, helper.CresteEMMessageText("005"));
			var msg6 = helper.CreateTestEDIMessage(Factory, branch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.ECS, MessageSubTypeList.Codes.ARR, EDIMessageStatusList.Codes.Cancelled, helper.CresteEMMessageText("006"));
			var msg7 = helper.CreateTestEDIMessage(Factory, branch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.ECS, MessageSubTypeList.Codes.ARR, EDIMessageStatusList.Codes.Queued, helper.CresteEMMessageText("007"));
			var msg8 = helper.CreateTestEDIMessage(Factory, branch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.ECS, MessageSubTypeList.Codes.DEP, EDIMessageStatusList.Codes.Queued, helper.CresteEMMessageText("008"));
			var msg9 = helper.CreateTestEDIMessage(Factory, otherBranch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.ECS, MessageSubTypeList.Codes.DEP, EDIMessageStatusList.Codes.Queued, helper.CresteEMMessageText("008"));

			Factory.Save();

			var processor = new FRECSOutgoingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			msg1.Reload();
			msg2.Reload();
			msg3.Reload();
			msg4.Reload();
			msg5.Reload();
			msg6.Reload();
			msg7.Reload();
			msg8.Reload();
			msg9.Reload();

			var interchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GenericMessageDelivery));
			AssertEquals(3, interchanges.Length);

			var interchange0 = interchanges.Cast<EDIInterchange>().First(x => x.PK == msg2.EM_EI);
			var header = "</SenderID><RecipientID>ABCDEFG</RecipientID><InterchangeType>ECS</InterchangeType><InterchangeNumber>";
			helper.AssertEDIInterchange(interchange0, "GMD", "TRX", "ECS", GlbCompany.CurrentCompany.LicenceKeyIdentifier, "ABCDEFG", "HQU", header, helper.CresteEMMessageText("002"));

			var interchange1 = interchanges.Cast<EDIInterchange>().First(x => x.PK == msg7.EM_EI);
			helper.AssertEDIInterchange(interchange1, "GMD", "TRX", "ECS", GlbCompany.CurrentCompany.LicenceKeyIdentifier, "ABCDEFG", "HQU", header, helper.CresteEMMessageText("007"));

			var interchange2 = interchanges.Cast<EDIInterchange>().First(x => x.PK == msg8.EM_EI);
			helper.AssertEDIInterchange(interchange2, "GMD", "TRX", "ECS", GlbCompany.CurrentCompany.LicenceKeyIdentifier, "ABCDEFG", "HQU", header, helper.CresteEMMessageText("008"));

			AssertEquals("QUE", msg1.EM_Status);
			AssertEquals("SNT", msg2.EM_Status);
			AssertEquals("QUE", msg3.EM_Status);
			AssertEquals("QUE", msg4.EM_Status);
			AssertEquals("QUE", msg5.EM_Status);
			AssertEquals("CAN", msg6.EM_Status);
			AssertEquals("SNT", msg7.EM_Status);
			AssertEquals("SNT", msg8.EM_Status);
			AssertEquals("QUE", msg5.EM_Status);
		}
	}
}
