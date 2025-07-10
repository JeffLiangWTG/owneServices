using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class FRECSInterchangeProviderTest : TestCaseWithFactory
	{
		public void TestInterchangePopulatedCorrectly()
		{
			FRCustomsDataRegistry.Instance.RecipientID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABCDEFG");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_ParentTableCode = "JE";
			exitHeader.CEH_ParentID = declaration.PK;
			exitHeader.CEH_ReferenceNumber = "123";
			var exitDetail = Factory.New<CusExitDetail>();
			exitDetail.CED_CEH = exitHeader.PK;
			exitDetail.CED_Status = "TTT";

			var helper = new ECSMessageTestHelper();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			var messageText = helper.CresteEMMessageText("39433691100042");
			var ediMessage1 = helper.CreateTestEDIMessage(Factory, branch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.ECS, MessageSubTypeList.Codes.ARR, EDIMessageStatusList.Codes.Queued, messageText);
			ediMessage1.EM_LinkedObject = exitDetail;
			Factory.Save();

			var processor = new FRECSOutgoingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);
			ediMessage1.Reload();

			var interchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GenericMessageDelivery));
			var interchange1 = interchanges.Cast<EDIInterchange>().First(x => x.PK == ediMessage1.EM_EI);
			AssertEquals("1", interchange1.EI_InterchangeNum);
			var header1 = "</SenderID><RecipientID>ABCDEFG</RecipientID><InterchangeType>ECS</InterchangeType><InterchangeNumber>1</InterchangeNumber>";
			helper.AssertEDIInterchange(interchange1, "GMD", "TRX", "ECS", GlbCompany.CurrentCompany.LicenceKeyIdentifier, "ABCDEFG", "HQU", header1, messageText);
			AssertEquals("SNT", ediMessage1.EM_Status);

			var ediMessage2 = helper.CreateTestEDIMessage(Factory, branch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.ECS, MessageSubTypeList.Codes.DEP, EDIMessageStatusList.Codes.Queued, messageText);
			ediMessage2.EM_LinkedObject = exitDetail;
			Factory.Save();
			processor.ProcessMessage(CancellationToken.None);
			ediMessage2.Reload();

			var interchanges2 = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GenericMessageDelivery));
			var interchange2 = interchanges2.Cast<EDIInterchange>().First(x => x.PK == ediMessage2.EM_EI);
			AssertEquals("2", interchange2.EI_InterchangeNum);
			var header2 = "</SenderID><RecipientID>ABCDEFG</RecipientID><InterchangeType>ECS</InterchangeType><InterchangeNumber>2</InterchangeNumber>";
			helper.AssertEDIInterchange(interchange2, "GMD", "TRX", "ECS", GlbCompany.CurrentCompany.LicenceKeyIdentifier, "ABCDEFG", "HQU", header2, messageText);
			AssertEquals("SNT", ediMessage2.EM_Status);
		}
	}
}
