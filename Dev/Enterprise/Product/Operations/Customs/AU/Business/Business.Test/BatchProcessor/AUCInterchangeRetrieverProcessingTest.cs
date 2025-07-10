using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.MailManager.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCInterchangeRetrieverProcessingTest : TestCaseWithFactory
	{
		public void TestERMMessageIsCreatedWithCorrectType()
		{
			using (var processor = new AUCInterchangeRetrieverForTesting())
			{
				string interchangeText = @"UNA:+.? '
UNB+UNOC:3+AAA336C::AAA336C+AAA374M+120922:1537+00000000408394++++1++1'
UNH+000001+CUSRES:D:99B:UN'
BGM+963:::ERM+CCF_AAA374M_1_ACR_1:1+11'
FTX+ACX+++ACR'
FTX+ADN+++9'
FTX+AAO+++CUSRES'
FTX+AAH+++CCF'
NAD+MR+AAA374M::95'
RFF+AAY:A00008674/DAT2::2'
DTM+310:20120906230958:204'
ERP+1'
ERC+CCFERROR:80:95'
ERC+15:6:95'
FTX+AAO+++The mandatory field ORIGINALPORTOFLOADING is missing from BODY'
CNT+55:1'
UNT+15+000001'
UNZ+1+00000000408394'";

				var interchange = processor.CreateInterchangeAndMessagesExposed(Factory, interchangeText.Replace("\r\n", ""));
				Factory.Save();
				AssertNotNull(interchange);
				AssertEquals("CMR Interchange", EDIInterchange.ApplicationCodes.CMR, interchange.EI_ApplicationCode);
				var inboundMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
				AssertNull(inboundMessage);
				new AUCInboundInterchangeProcessor(new LoggingInformation()).ExecuteBatch();
				inboundMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
				AssertNotNull(inboundMessage);
				AssertEquals("Correct Type", Declaration.Business.CMRMessage.CMRMessageTypes.AIRCR, inboundMessage.EM_MessageType);
			}
		}

		sealed class AUCInterchangeRetrieverForTesting : AUCInterchangeRetriever
		{
			public AUCInterchangeRetrieverForTesting() : base() { }

			public EDIInterchange CreateInterchangeAndMessagesExposed(BusinessObjectFactory factory, string interchangeString)
			{
				return CreateInterchangeAndMessages(factory, interchangeString, factory.GetNull<MailItem>());
			}
		}
	}
}
