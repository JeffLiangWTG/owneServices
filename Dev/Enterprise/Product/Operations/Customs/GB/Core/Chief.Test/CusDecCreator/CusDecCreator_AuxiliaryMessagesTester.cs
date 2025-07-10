using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Edifact.D04A.Messages.CUSDEC;

namespace Enterprise.Customs.GB.Chief.CusDec.Testing
{
	public class CusDecCreator_AuxiliaryMessagesTester : TestCaseWithFactory
	{
		public void TestDLU()
		{
			var errors = new ErrorCollector();
			var dluCreator = new CusDecCreator_Interrogation_DLU(new Interrogate_DLU(), errors);
			var cusDec = dluCreator.CreateInterrogationMessage("123456789000", "GBLIC123456", "");
			string messageText = dluCreator.CreateEdifactString(Chief.CusDec.CusDecCreator.CharacterSetDefault);
			AssertEquals("UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:04A:UN:109784+<<SYSCAR>>'BGM+DLU::109++13'RFF+EX:GBLIC123456'RFF+ASM:123456789000'UNS+D'UNS+S'UNT+7+<<MSGNO PLACEHOLDER>>'", messageText);
		}

		public void TestREQ()
		{
			TestShared(new Interrogate_Req(),
				"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:04A:UN:109761+<<SYSCAR>>'BGM+REQ::109++13'RFF+ABO:<<UCRREF>>:<<UCRPART>>'UNS+D'UNS+S'UNT+6+<<MSGNO PLACEHOLDER>>'");
		}

		public void TestLEM()
		{
			TestShared(new Interrogate_Lem(),
				"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:04A:UN:109783+<<SYSCAR>>'BGM+LEM::109++13'RFF+ABO:<<UCRREF>>:<<UCRPART>>'UNS+D'UNS+S'UNT+6+<<MSGNO PLACEHOLDER>>'");
		}

		public void TestDECd()
		{
			TestShared(new Interrogate_DecDucr(),
				"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:04A:UN:109782+<<SYSCAR>>'BGM+DEC::109++13'RFF+ABO:<<UCRREF>>'UNS+D'UNS+S'UNT+6+<<MSGNO PLACEHOLDER>>'");
		}

		public void TestDECm()
		{
			TestShared(new Interrogate_DecMucr(),
				"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:04A:UN:109782+<<SYSCAR>>'BGM+DEC::109++13'RFF+ABO:MUCRHERE'UNS+D'UNS+S'UNT+6+<<MSGNO PLACEHOLDER>>'");
		}

		public void TestDEM()
		{
			TestShared(new Interrogate_Dem("123456"),
				"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:04A:UN:109781+<<SYSCAR>>'BGM+DEM::109++13'RFF+AES:123456'UNS+D'UNS+S'UNT+6+<<MSGNO PLACEHOLDER>>'");
		}

		public void TestDEVd()
		{
			TestShared(new Interrogate_DevDucr(),
				"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:04A:UN:109780+<<SYSCAR>>'BGM+DEV::109++13'CST++EXD'RFF+ABO:<<UCRREF>>:<<UCRPART>>'UNS+D'UNS+S'UNT+7+<<MSGNO PLACEHOLDER>>'");
		}

		public void TestDES()
		{
			TestShared(new Interrogate_Des(),
				"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:04A:UN:109804+<<SYSCAR>>'BGM+DES::109++13'CST'RFF+ABO:<<UCRREF>>:<<UCRPART>>'UNS+D'UNS+S'UNT+7+<<MSGNO PLACEHOLDER>>'");
		}

		void TestShared(QueryMessageFunction how, string expected)
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterUCR = "mucrhere";
			declaration.JE_EntrySubStyle = "D";
			Factory.Save();
			header = declaration.CustomsEntryHeaders.AddNew();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.DoMerge();
			errors = new ErrorCollector();

			CusDecCreator creator = new CusDecCreator(declaration, header, how, errors);
			CUSDECMessage cusDec = creator.CreateInterrogationMessage();
			string messageText = creator.CreateEdifactString(Chief.CusDec.CusDecCreator.CharacterSetDefault);
			AssertEquals("Expecting to see this edifact when we try to make an interrogation message of type " + how.GetType().Name,
				expected, messageText);
		}

		JobDeclaration declaration;
		CusEntryHeader header;
		ErrorCollector errors;
	}
}
