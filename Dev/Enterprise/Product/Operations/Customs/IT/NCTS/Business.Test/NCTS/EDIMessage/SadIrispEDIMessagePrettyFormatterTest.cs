using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class SadIrispEDIMessagePrettyFormatterTest : TestCaseWithFactory
{
	public void TestNctsMessageInterpretation_Error()
	{
		string expectedInterpretation = "<h2>IRISP malformed</h2>";

		irispMessage.EM_MessageText = @"INVALID IRISP CONTENT";
		AssertMultilineASCIIEquals(expectedInterpretation, irispMessage.EM_MessageInterpretation);
	}

	public void TestNctsMessageIntepretation_ExportPositiveIrisp()
	{
		string expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>037V0430.Rab</<p><br/><br/>" +
			"<h3 style='display:inline'>Status: </h3><p style='display:inline;color:green'>Registered</<p><br/><br/>" +
			"<h3 style='display:inline'>Customs Office: </h3><p style='display:inline;color:'>IT137100</<p><br/><br/>" +
			"<h3 style='display:inline'>Reg. No: </h3><p style='display:inline;color:'>2--00000140-R-30/04/2021</<p><br/><br/>" +
			"<h3 style='display:inline'>MRN: </h3><p style='display:inline;color:'>21ITQXT020000140E0</<p><br/><br/><br/>";

		irispMessage.EM_MessageText = @"037V            037V0430.Xab210000104414137100    02714410277     001 00005    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  30/04/21 15:37,037V0430.Rab
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   30/04/21  15:37
RET          05018100137100P2   00000140R300421000000 000000 000000 000000 000000                               21ITQXT020000140E0                                                                 ";

		AssertMultilineASCIIEquals(expectedInterpretation, irispMessage.EM_MessageInterpretation);
	}

	public void TestNctsMessageIntepretation_ExportNegativeIrisp()
	{
		string expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>004V1006.R27</<p><br/><br/>" +
			"<h3 style='color:red;margin-left: 10pt'>Error 1</h3><br/>" +
			"<table style='margin-left: 20pt'>" +
			"<tr><td><p>Entry Line:</p></td><td>1</td></tr>" +
			"<tr><td><p>Field:</p></td><td>29.1 Customs office - Reference number</td></tr>" +
			"<tr><td><p>Occurrence:</p></td><td>0</td></tr>" +
			"<tr><td><p>Error Type:</p></td><td>F</td></tr>" +
			"<tr><td><p>Error Message:</p></td><td>1 - [Campo obbligatorio assente]</td></tr>" +
			"</table><br/>";

		irispMessage.EM_MessageText = @"004V            004V1006.X27200000233460279100    01878690229     001 00006    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  06/10/20 14:30,004V1006.R27
 RL=003,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   06/10/20  14:30
RET          67139800279100N                                        
 012901 00F 00 Errore 1 : [Campo obbligatorio assente]                ";

		AssertMultilineASCIIEquals(expectedInterpretation, irispMessage.EM_MessageInterpretation);
	}

	public void TestNctsMessageIntepretation_NegativeStandaloneNbIrisp()
	{
		string expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>00200114.RK4</<p><br/><br/>" +
			"<h3 style='color:#5d85cc'><b>NB Records</b></h3>" +
			"<h3 style='color:red;margin-left: 10pt'>Error 1</h3><br/>" +
			"<table style='margin-left: 20pt'>" +
			"<tr><td><p>Entry Line:</p></td><td>0</td></tr>" +
			"<tr><td><p>Field:</p></td><td>J Number of packages</td></tr>" +
			"<tr><td><p>Occurrence:</p></td><td>0</td></tr>" +
			"<tr><td><p>Error Type:</p></td><td>L</td></tr>" +
			"<tr><td><p>Error Message:</p></td><td>364 - [Colli incoerenti con archivio]</td></tr>" +
			"</table><br/>";

		irispMessage.EM_MessageText = @"0020            00200114.XK4200000583290371100    005908A         001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  14/01/20 15:09,00200114.RK4
 RL=003,RS=000,ME=002,MS=000,PE=001,PS=000
ESEGUITO   14/01/20  15:09
RNB          32213801371100N                                        
 000J00 00L 00 Errore 364 : [Colli incoerenti con archivio]           ";

		AssertMultilineASCIIEquals(expectedInterpretation, irispMessage.EM_MessageInterpretation);
	}

	public void TestNctsMessageIntepretation_PartialPositiveStandaloneNbIrisp()
	{
		var expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>00200114.RK4</<p><br/><br/>" +
"<h3 style='color:#5d85cc'><b>NB Records</b></h3>" +
"<h3 style='color:red;margin-left: 10pt'>Error 1</h3><br/>" +
"<table style='margin-left: 20pt'>" +
"<tr><td><p>Entry Line:</p></td><td>0</td></tr>" +
"<tr><td><p>Field:</p></td><td>J Number of packages</td></tr>" +
"<tr><td><p>Occurrence:</p></td><td>0</td></tr>" +
"<tr><td><p>Error Type:</p></td><td>L</td></tr>" +
"<tr><td><p>Error Message:</p></td><td>364 - [Colli incoerenti con archivio]</td></tr>" +
"</table><br/>";

		irispMessage.EM_MessageText = @"0020            00200114.XK4200000583290371100    005908A         001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  14/01/20 15:09,00200114.RK4
 RL=003,RS=000,ME=002,MS=000,PE=001,PS=000
ESEGUITO   14/01/20  15:09
RNB          32213700371100P 
ESEGUITO   14/01/20  15:09
RNB          32213800371100N                                        
 000J00 00L 00 Errore 364 : [Colli incoerenti con archivio]           ";

		AssertMultilineASCIIEquals(expectedInterpretation, irispMessage.EM_MessageInterpretation);
	}

	public void TestNctsMessageIntepretation_PositiveStandaloneNbIrisp()
	{
		string expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>004V0328.R26</<p><br/><br/>" +
			"<h3 style='color:#5d85cc'><b>NB Records</b></h3>" +
			"<h3 style='color:green'><b>All Approved</b></h3><br/>";

		irispMessage.EM_MessageText = @"004V            004V0328.X26180000121143035101    01878690229     001 00005    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  28/03/18 14:55,004V0328.R26
 RL=001,RS=000,ME=001,MS=000,PE=000,PS=000
ESEGUITO   28/03/18  14:55
RNB          67129700035101P ";

		AssertMultilineASCIIEquals(expectedInterpretation, irispMessage.EM_MessageInterpretation);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		irispMessage = Factory.New<ITEDIMessage>();
		irispMessage.EM_MessageType = SADConstants.CustomsInterchangeType.IrispX;
		nctsHeader.Messages.Add(irispMessage);
	}

	NctsHeader nctsHeader;
	ITEDIMessage irispMessage;
}
