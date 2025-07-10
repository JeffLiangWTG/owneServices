using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	public class CreditCODOperationalActionRunnerTest : TestCaseWithFactory
	{
		public void TestCreditCODForCreditPreviousEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "1";
			entryHeader.CH_BGMReference = "3-B00001000";
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.EntryNumber = "2";
			entryHeader2.CH_BGMReference = "3-B00001001";

			var applicator = new FrCreditCODApplicator(Factory);
			var itemApplicator = applicator.FrCreditCODItemApplicators.AddNew();
			itemApplicator.CreditMethod = CreditMethodList.Codes.CreditPreviousEntry;
			itemApplicator.ReleasingEntryReference = "3-B00001000";
			itemApplicator.PreviousEntryReference = "3-B00001001";

			var log = new Services.OperationalActions.Support.Testing.DummyOperationalActionSectionLog();
			var runner = new CreditCODOperationalActionRunner(log);
			runner.CreditCOD(applicator.FrCreditCODItemApplicators);

			var ediMessages = Factory.Load<CODSendMessage>(new ZQuery()).ToArray();
			AssertEquals(1, ediMessages.Length);

			var message = ediMessages[0];
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("1", message.EM_MessageNum);
			AssertContains(@"<?xml version=""1.0"" encoding=""utf-8""?><Message xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><EnveloppeMessage><schemaID>MessageCOD</schemaID><schemaVersion>18122012</schemaVersion><numseq>0</numseq></EnveloppeMessage><Declaration><Entete><codact>3</codact><refdos>3-B00001000</refdos></Entete><Gens><Gen><refdec>2</refdec><typflux>EXP</typflux></Gen></Gens></Declaration></Message>".Replace(System.Environment.NewLine, ""), message.EM_MessageText);
			AssertSame(entryHeader, message.EM_LinkedObject);
			AssertEquals(@"INFO: Create COD message 1 in entry header 3-B00001000 successfully", string.Join("\r\n", log.messages));
		}

		public void TestCreditCODForCreditPreviousEntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "1";
			entryHeader.CH_BGMReference = "3-B00001000";
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.EntryNumber = "2";
			entryHeader2.CH_BGMReference = "3-B00001001";
			var entryLine2 = entryHeader2.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 1;

			var applicator = new FrCreditCODApplicator(Factory);
			var itemApplicator = applicator.FrCreditCODItemApplicators.AddNew();
			itemApplicator.CreditMethod = CreditMethodList.Codes.CreditPreviousEntryLine;
			itemApplicator.ReleasingEntryReference = "3-B00001000";
			itemApplicator.PreviousEntryReference = "3-B00001001";
			itemApplicator.PreviousEntryLineNo = 1;
			itemApplicator.Amount = 5m;

			var log = new Services.OperationalActions.Support.Testing.DummyOperationalActionSectionLog();
			var runner = new CreditCODOperationalActionRunner(log);
			runner.CreditCOD(applicator.FrCreditCODItemApplicators);

			var ediMessages = Factory.Load<CODSendMessage>(new ZQuery()).ToArray();
			AssertEquals(1, ediMessages.Length);

			var message = ediMessages[0];
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("1", message.EM_MessageNum);
			AssertContains(@"<?xml version=""1.0"" encoding=""utf-8""?><Message xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><EnveloppeMessage><schemaID>MessageCOD</schemaID><schemaVersion>18122012</schemaVersion><numseq>0</numseq></EnveloppeMessage><Declaration><Entete><codact>1</codact><refdos>3-B00001000</refdos></Entete><Articles><Article><refdec>2</refdec><typflux>EXP</typflux><numart>1</numart><Apur><apurementREC><indicateurApurement>0</indicateurApurement></apurementREC></Apur></Article></Articles></Declaration></Message>".Replace(System.Environment.NewLine, ""), message.EM_MessageText);
			AssertSame(entryHeader, message.EM_LinkedObject);
			AssertEquals(@"INFO: Create COD message 1 in entry header 3-B00001000 successfully", string.Join("\r\n", log.messages));
		}

		public void TestCreditCODForCreditPartialAmountFromPreviousEntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "1";
			entryHeader.CH_BGMReference = "3-B00001000";
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.EntryNumber = "2";
			entryHeader2.CH_BGMReference = "3-B00001001";
			var entryLine2 = entryHeader2.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 1;

			var applicator = new FrCreditCODApplicator(Factory);
			var itemApplicator = applicator.FrCreditCODItemApplicators.AddNew();
			itemApplicator.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			itemApplicator.ReleasingEntryReference = "3-B00001000";
			itemApplicator.PreviousEntryReference = "3-B00001001";
			itemApplicator.PreviousEntryLineNo = 1;
			itemApplicator.Amount = 5m;

			var log = new Services.OperationalActions.Support.Testing.DummyOperationalActionSectionLog();
			var runner = new CreditCODOperationalActionRunner(log);
			runner.CreditCOD(applicator.FrCreditCODItemApplicators);

			var ediMessages = Factory.Load<CODSendMessage>(new ZQuery()).ToArray();
			AssertEquals(1, ediMessages.Length);

			var message = ediMessages[0];
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("1", message.EM_MessageNum);
			AssertContains(@"<?xml version=""1.0"" encoding=""utf-8""?><Message xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><EnveloppeMessage><schemaID>MessageCOD</schemaID><schemaVersion>18122012</schemaVersion><numseq>0</numseq></EnveloppeMessage><Declaration><Entete><codact>1</codact><refdos>3-B00001000</refdos></Entete><Articles><Article><refdec>2</refdec><typflux>EXP</typflux><numart>1</numart><Apur><apurementREC><indicateurApurement>1</indicateurApurement><mnt>5</mnt><refdecapur>1</refdecapur></apurementREC></Apur></Article></Articles></Declaration></Message>".Replace(System.Environment.NewLine, ""), message.EM_MessageText);
			AssertSame(entryHeader, message.EM_LinkedObject);
			AssertEquals(@"INFO: Create COD message 1 in entry header 3-B00001000 successfully", string.Join("\r\n", log.messages));
		}

		public void TestCreditCODIfNoEntryHeader()
		{
			var applicator = new FrCreditCODApplicator(Factory);
			var itemApplicator = applicator.FrCreditCODItemApplicators.AddNew();
			itemApplicator.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			itemApplicator.ReleasingEntryReference = "1";
			itemApplicator.PreviousEntryReference = "2";
			itemApplicator.Amount = 5m;

			var log = new Services.OperationalActions.Support.Testing.DummyOperationalActionSectionLog();
			var runner = new CreditCODOperationalActionRunner(log);
			runner.CreditCOD(applicator.FrCreditCODItemApplicators);

			var ediMessages = Factory.Load<CODSendMessage>(new ZQuery()).ToArray();
			AssertEquals(0, ediMessages.Length);
			AssertEquals(@"INFO: There were no entry header found for the entry number 1.", string.Join("\r\n", log.messages));
		}

		public void TestCreditCOD_Grouping()
		{
			var declarationA = Factory.New<JobDeclaration>();
			var entryHeaderA = declarationA.CustomsEntryHeaders.AddNew();
			entryHeaderA.EntryNumber = "1";
			entryHeaderA.CH_BGMReference = "1-B00001000";
			var entryLineA = entryHeaderA.AllEntryLines.AddNew();
			entryLineA.CL_LineNumber = 1;
			var entryHeaderA2 = declarationA.CustomsEntryHeaders.AddNew();
			entryHeaderA2.EntryNumber = "2";
			entryHeaderA2.CH_BGMReference = "2-B00001000";
			var entryLineA2 = entryHeaderA2.AllEntryLines.AddNew();
			entryLineA2.CL_LineNumber = 1;

			var declarationB = Factory.New<JobDeclaration>();
			var entryHeaderB1 = declarationB.CustomsEntryHeaders.AddNew();
			entryHeaderB1.EntryNumber = "3";
			entryHeaderB1.CH_BGMReference = "1-B00001001";
			var entryLineB1 = entryHeaderB1.AllEntryLines.AddNew();
			entryLineB1.CL_LineNumber = 1;
			var entryHeaderB2 = declarationB.CustomsEntryHeaders.AddNew();
			entryHeaderB2.EntryNumber = "4";
			entryHeaderB2.CH_BGMReference = "2-B00001001";
			var entryLineB2 = entryHeaderB2.AllEntryLines.AddNew();
			entryLineB2.CL_LineNumber = 1;
			var entryHeaderB3 = declarationB.CustomsEntryHeaders.AddNew();
			entryHeaderB3.EntryNumber = "3";
			entryHeaderB3.CH_BGMReference = "3-B00001001";
			var entryLineB3 = entryHeaderB3.AllEntryLines.AddNew();
			entryLineB3.CL_LineNumber = 1;
			var entryHeaderB4 = declarationB.CustomsEntryHeaders.AddNew();
			entryHeaderB4.EntryNumber = "4";
			entryHeaderB4.CH_BGMReference = "4-B00001001";
			var entryLineB4 = entryHeaderB4.AllEntryLines.AddNew();
			entryLineB4.CL_LineNumber = 1;
			var entryHeaderB5 = declarationB.CustomsEntryHeaders.AddNew();
			entryHeaderB5.EntryNumber = "5";
			entryHeaderB5.CH_BGMReference = "5-B00001001";
			var entryLineB5 = entryHeaderB5.AllEntryLines.AddNew();
			entryLineB5.CL_LineNumber = 1;
			var entryHeaderB6 = declarationB.CustomsEntryHeaders.AddNew();
			entryHeaderB6.EntryNumber = "6";
			entryHeaderB6.CH_BGMReference = "6-B00001001";
			var entryLineB6 = entryHeaderB6.AllEntryLines.AddNew();
			entryLineB6.CL_LineNumber = 1;

			var applicator = new FrCreditCODApplicator(Factory);
			var itemApplicator1 = applicator.FrCreditCODItemApplicators.AddNew();
			itemApplicator1.CreditMethod = CreditMethodList.Codes.CreditPreviousEntry;
			itemApplicator1.ReleasingEntryReference = "1-B00001000";
			itemApplicator1.PreviousEntryReference = "1-B00001001";
			var itemApplicator2 = applicator.FrCreditCODItemApplicators.AddNew();
			itemApplicator2.CreditMethod = CreditMethodList.Codes.CreditPreviousEntry;
			itemApplicator2.ReleasingEntryReference = "1-B00001000";
			itemApplicator2.PreviousEntryReference = "2-B00001001";
			var itemApplicator3 = applicator.FrCreditCODItemApplicators.AddNew();
			itemApplicator3.CreditMethod = CreditMethodList.Codes.CreditPreviousEntryLine;
			itemApplicator3.ReleasingEntryReference = "1-B00001000";
			itemApplicator3.PreviousEntryReference = "3-B00001001";
			itemApplicator3.PreviousEntryLineNo = 1;
			var itemApplicator4 = applicator.FrCreditCODItemApplicators.AddNew();
			itemApplicator4.CreditMethod = CreditMethodList.Codes.CreditPreviousEntryLine;
			itemApplicator4.ReleasingEntryReference = "1-B00001000";
			itemApplicator4.PreviousEntryReference = "4-B00001001";
			itemApplicator4.PreviousEntryLineNo = 1;
			var itemApplicator5 = applicator.FrCreditCODItemApplicators.AddNew();
			itemApplicator5.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			itemApplicator5.ReleasingEntryReference = "1-B00001000";
			itemApplicator5.PreviousEntryReference = "5-B00001001";
			itemApplicator5.PreviousEntryLineNo = 1;
			itemApplicator5.Amount = 22;
			var itemApplicator6 = applicator.FrCreditCODItemApplicators.AddNew();
			itemApplicator6.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			itemApplicator6.ReleasingEntryReference = "1-B00001000";
			itemApplicator6.PreviousEntryReference = "6-B00001001";
			itemApplicator6.PreviousEntryLineNo = 1;
			itemApplicator6.Amount = 33;

			var log = new Services.OperationalActions.Support.Testing.DummyOperationalActionSectionLog();
			var runner = new CreditCODOperationalActionRunner(log);
			runner.CreditCOD(applicator.FrCreditCODItemApplicators);

			var ediMessages = Factory.Load<CODSendMessage>(new ZQuery()).OrderBy(x => x.EM_SystemCreateTimeUtc).ToArray();
			AssertEquals(2, ediMessages.Length);

			var message1 = ediMessages[0];
			AssertEquals(EDIMessage.Status.Queued, message1.EM_Status);
			AssertEquals("1", message1.EM_MessageNum);
			AssertXMLContains(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Message xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<EnveloppeMessage>
<schemaID>MessageCOD</schemaID>
<schemaVersion>18122012</schemaVersion>
<numseq>0</numseq>
</EnveloppeMessage>
<Declaration>
<Entete>
<codact>3</codact>
<refdos>1-B00001000</refdos>
</Entete>
<Gens>
<Gen>
<refdec>3</refdec>
<typflux>EXP</typflux>
</Gen>
<Gen>
<refdec>4</refdec>
<typflux>EXP</typflux>
</Gen>
</Gens>
</Declaration>
</Message>".Replace(System.Environment.NewLine, ""), message1.EM_MessageText);
			AssertSame(entryHeaderA, message1.EM_LinkedObject);

			var message2 = ediMessages[1];
			AssertEquals(EDIMessage.Status.Queued, message2.EM_Status);
			AssertEquals("2", message2.EM_MessageNum);
			AssertXMLContains(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Message xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<EnveloppeMessage>
<schemaID>MessageCOD</schemaID>
<schemaVersion>18122012</schemaVersion>
<transactionId>0000000001</transactionId>
<numseq>0</numseq>
</EnveloppeMessage>
<Declaration>
<Entete>
<codact>1</codact>
<refdos>1-B00001000</refdos>
</Entete>
<Articles>
<Article>
<refdec>3</refdec>
<typflux>EXP</typflux>
<numart>1</numart>
<Apur>
<apurementREC>
<indicateurApurement>0</indicateurApurement>
</apurementREC>
</Apur>
</Article>
<Article>
<refdec>4</refdec>
<typflux>EXP</typflux>
<numart>1</numart>
<Apur>
<apurementREC>
<indicateurApurement>0</indicateurApurement>
</apurementREC>
</Apur>
</Article>
<Article>
<refdec>5</refdec>
<typflux>EXP</typflux>
<numart>1</numart>
<Apur>
<apurementREC>
<indicateurApurement>1</indicateurApurement>
<mnt>22</mnt>
<refdecapur>1</refdecapur>
</apurementREC>
</Apur>
</Article>
<Article>
<refdec>6</refdec>
<typflux>EXP</typflux>
<numart>1</numart>
<Apur>
<apurementREC>
<indicateurApurement>1</indicateurApurement>
<mnt>33</mnt>
<refdecapur>1</refdecapur>
</apurementREC>
</Apur>
</Article>
</Articles>
</Declaration>
</Message>".Replace(System.Environment.NewLine, ""), message2.EM_MessageText);
			AssertSame(entryHeaderA, message2.EM_LinkedObject);

			AssertEquals(@"INFO: Create COD message 1 in entry header 1-B00001000 successfully
INFO: Create COD message 2 in entry header 1-B00001000 successfully", string.Join("\r\n", log.messages));
		}
	}
}
