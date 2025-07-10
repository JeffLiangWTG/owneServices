using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SadIrispEDIMessagePrettyFormatterTest : TestCaseWithFactory
{
	public void TestEM_MessageIntepretation_PositiveIrisp()
	{
		CombineAssertions("When declaration is cleared", () =>
		{
			string expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>004R1004.R04</<p><br/><br/>" +
			"<h3 style='display:inline'>Status: </h3><p style='display:inline;color:green'>Cleared</<p><br/><br/>" +
			"<h3 style='display:inline'>Customs Office: </h3><p style='display:inline;color:'>IT275100</<p><br/><br/>" +
			"<h3 style='display:inline'>Reg. No: </h3><p style='display:inline;color:'>4-T-00031664-X-04/10/2019</<p><br/><br/>" +
			"<h3 style='display:inline'>Clearance Code: </h3><p style='display:inline;color:'>X91B0Z</<p><br/><br/>" +
			"<h3 style='display:inline'>A93 number: </h3><p style='display:inline;color:'>000273</<p><br/><br/>" +
			"<h3 style='display:inline'>First method of payment: </h3><p style='display:inline;color:'>G</<p><br/><br/>" +
			"<h3 style='display:inline'>First expiry date: </h3><p style='display:inline;color:'>08/11/2019</<p><br/><br/>" +
			"<h3 style='display:inline'>Second method of payment: </h3><p style='display:inline;color:'>F</<p><br/><br/>" +
			"<h3 style='display:inline'>Second expiry date: </h3><p style='display:inline;color:'>08/11/2019</<p><br/><br/>" +
			"<h3 style='display:inline'>Third method of payment: </h3><p style='display:inline;color:'>G</<p><br/><br/>" +
			"<h3 style='display:inline'>Third expiry date: </h3><p style='display:inline;color:'>09/11/2019</<p><br/><br/><br/>";

			irispMessage.EM_MessageText = @"004R            004R1004.X04190015588917275100    01790970139     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  04/10/19 06:04,004R1004.R04
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   04/10/19  06:04
RIM          21309000275100P4 T 00031664X041019014135A000273G081119F081119G091119X91B0ZSVINCOLATA                                                                                                  ";
			var interpretation = new SadIrispEDIMessagePrettyFormatter(Factory).GetFormattedText(irispMessage.EM_MessageText);
			AssertMultilineASCIIEquals(expectedInterpretation, interpretation);
		});

		CombineAssertions("When declaration is registered", () =>
		{
			string expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>004R1004.R04</<p><br/><br/>" +
			"<h3 style='display:inline'>Status: </h3><p style='display:inline;color:green'>Registered</<p><br/><br/>" +
			"<h3 style='display:inline'>Customs Office: </h3><p style='display:inline;color:'>IT275100</<p><br/><br/>" +
			"<h3 style='display:inline'>Reg. No: </h3><p style='display:inline;color:'>4-T-00031664-X-04/10/2019</<p><br/><br/>" +
			"<h3 style='display:inline'>Clearance Code: </h3><p style='display:inline;color:'>X91B0Z</<p><br/><br/>" +
			"<h3 style='display:inline'>A93 number: </h3><p style='display:inline;color:'>000273</<p><br/><br/><br/>";

			irispMessage.EM_MessageText = @"004R            004R1004.X04190015588917275100    01790970139     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  04/10/19 06:04,004R1004.R04
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   04/10/19  06:04
RIM          21309000275100P4 T 00031664X041019014135A000273 000000 000000 000000X91B0Z                                                                                                            ";

			var interpretation = new SadIrispEDIMessagePrettyFormatter(Factory).GetFormattedText(irispMessage.EM_MessageText);
			AssertMultilineASCIIEquals(expectedInterpretation, interpretation);
		});

		CombineAssertions("When declaration is under control", () =>
		{
			string expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>004R1004.R04</<p><br/><br/>" +
			"<h3 style='display:inline'>Status: </h3><p style='display:inline;color:red'>Under control</<p><br/><br/>" +
			"<h3 style='display:inline'>Customs Office: </h3><p style='display:inline;color:'>IT275100</<p><br/><br/>" +
			"<h3 style='display:inline'>Reg. No: </h3><p style='display:inline;color:'>4-T-00031664-X-04/10/2019</<p><br/><br/>" +
			"<h3 style='display:inline'>Clearance Code: </h3><p style='display:inline;color:'>X91B0Z</<p><br/><br/>" +
			"<h3 style='display:inline'>A93 number: </h3><p style='display:inline;color:'>000273</<p><br/><br/><br/>";

			irispMessage.EM_MessageText = @"004R            004R1004.X04190015588917275100    01790970139     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  04/10/19 06:04,004R1004.R04
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   04/10/19  06:04
RIM          21309000275100P4 T 00031664X041019014135A000273 000000 000000 000000X91B0ZNON SVINCOLABILE                                                                                            ";

			var interpretation = new SadIrispEDIMessagePrettyFormatter(Factory).GetFormattedText(irispMessage.EM_MessageText);
			AssertMultilineASCIIEquals(expectedInterpretation, interpretation);
		});
	}

	public void TestEM_MessageIntepretation_ImportNegativeIrisp()
	{
		string expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>02VM1004.R01</<p><br/><br/>" +
			"<h3 style='color:red;margin-left: 10pt'>Error 1</h3><br/>" +
			"<table style='margin-left: 20pt'>" +
			"<tr><td><p>Entry Line:</p></td><td>1</td></tr>" +
			"<tr><td><p>Field:</p></td><td>33.2 Additional Codes - Number of Occurrences</td></tr>" +
			"<tr><td><p>Occurrence:</p></td><td>0</td></tr>" +
			"<tr><td><p>Error Type:</p></td><td>L</td></tr>" +
			"<tr><td><p>Error Message:</p></td><td>0 - [Codice addizionale in input mancante]</td></tr>" +
			"</table><br/>";

		irispMessage.EM_MessageText = @"02VM            02VM1004.X01190015592475136101    007775D         001 00006    INVIO IN AMBIENTE REALE       
 RICEVUTO  04/10/19 08:07,02VM1004.R01
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   04/10/19  08:07
RIM          37507700136101N                                        
 013302 00L 00 Errore 0 : [Codice addizionale in input mancante]      ";

		var interpretation = new SadIrispEDIMessagePrettyFormatter(Factory).GetFormattedText(irispMessage.EM_MessageText);
		AssertMultilineASCIIEquals(expectedInterpretation, interpretation);
	}

	public void TestEM_MessageIntepretation_ExportNegativeIrisp()
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

		var interpretation = new SadIrispEDIMessagePrettyFormatter(Factory).GetFormattedText(irispMessage.EM_MessageText);
		AssertMultilineASCIIEquals(expectedInterpretation, interpretation);
	}

	public void TestEM_MessageInterpretation_NegativeIrispWithNegativeNb()
	{
		string expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>01N61120.R06</<p><br/><br/>" +
			"<h3 style='color:red;margin-left: 10pt'>Error 1</h3><br/>" +
			"<table style='margin-left: 20pt'>" +
			"<tr><td><p>Entry Line:</p></td><td>1</td></tr>" +
			"<tr><td><p>Field:</p></td><td>5 Total Items</td></tr>" +
			"<tr><td><p>Occurrence:</p></td><td>0</td></tr>" +
			"<tr><td><p>Error Type:</p></td><td>F</td></tr>" +
			"<tr><td><p>Error Message:</p></td><td>3 - [Valore non ammesso]-Nota- numero singoli dichiarato incoerente con dati inseriti</td></tr>" +
			"</table><br/>" +
			"<hr/>" +
			"<h3 style='color:#5d85cc'><b>NB Records</b></h3>" +
			"<h3 style='color:red;margin-left: 10pt'>Error 1</h3><br/>" +
			"<table style='margin-left: 20pt'>" +
			"<tr><td><p>Entry Line:</p></td><td>0</td></tr>" +
			"<tr><td><p>Field:</p></td><td>0000</td></tr>" +
			"<tr><td><p>Occurrence:</p></td><td>0</td></tr>" +
			"<tr><td><p>Error Type:</p></td><td>F</td></tr>" +
			"<tr><td><p>Error Message:</p></td><td>524 - [Dichiarazione elaborata con errori!]</td></tr>" +
			"</table><br/>";

		irispMessage.EM_MessageText = @"01N6            01N61120.X06190000238724304100    005916R         001 00012    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  20/11/19 16:31,01N61120.R06
 RL=008,RS=000,ME=003,MS=000,PE=002,PS=000
ESEGUITO   20/11/19  16:31
RIM          00011800304100N                                        
 010500 00F 00 Errore 3 : [Valore non ammesso]-Nota: numero singoli dichiarato incoerente con dati inseriti
ESEGUITO   20/11/19  16:31
RNB          00011801304100N                                        
 000000 00F 00 Errore 524 : [Dichiarazione elaborata con errori!]     ";

		var interpretation = new SadIrispEDIMessagePrettyFormatter(Factory).GetFormattedText(irispMessage.EM_MessageText);
		AssertMultilineASCIIEquals(expectedInterpretation, interpretation);
	}

	public void TestEM_MessageIntepretation_Error()
	{
		string expectedInterpretation = "<h2>IRISP malformed</h2>";
		irispMessage.EM_MessageText = @"INVALID IRISP CONTENT";
		var interpretation = new SadIrispEDIMessagePrettyFormatter(Factory).GetFormattedText(irispMessage.EM_MessageText);
		AssertMultilineASCIIEquals(expectedInterpretation, interpretation);
	}

	public void TestEM_MessageInterpretation_PositiveIrispWithPositiveNb()
	{
		string expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>041G0128.RJ2</<p><br/><br/>" +
			"<h3 style='display:inline'>Status: </h3><p style='display:inline;color:green'>Cleared</<p><br/><br/>" +
			"<h3 style='display:inline'>Customs Office: </h3><p style='display:inline;color:'>IT371101</<p><br/><br/>" +
			"<h3 style='display:inline'>Reg. No: </h3><p style='display:inline;color:'>4-T-00005514-Q-28/01/2020</<p><br/><br/>" +
			"<h3 style='display:inline'>Clearance Code: </h3><p style='display:inline;color:'>S6EHAW</<p><br/><br/>" +
			"<h3 style='display:inline'>A93 number: </h3><p style='display:inline;color:'>000165</<p><br/><br/>" +
			"<h3 style='display:inline'>First method of payment: </h3><p style='display:inline;color:'>G</<p><br/><br/>" +
			"<h3 style='display:inline'>First expiry date: </h3><p style='display:inline;color:'>23/02/2020</<p><br/><br/>" +
			"<h3 style='display:inline'>Second method of payment: </h3><p style='display:inline;color:'>F</<p><br/><br/>" +
			"<h3 style='display:inline'>Second expiry date: </h3><p style='display:inline;color:'>23/02/2020</<p><br/><br/><br/>" +
			"<hr/>" +
			"<h3 style='color:#5d85cc'><b>NB Records</b></h3>" +
			"<h3 style='color:green'><b>All Approved</b></h3><br/>";

		irispMessage.EM_MessageText = @"041G            041G0128.XJ2200001455200371101    00831370432     001 00007    INVIO IN AMBIENTE REALE       
 RICEVUTO  28/01/20 08:04,041G0128.RJ2
 RL=003,RS=000,ME=002,MS=000,PE=001,PS=000
ESEGUITO   28/01/20  08:04
RIM          00087900371101P4 T 00005514Q280120013413G000165G230220F230220 000000S6EHAWSVINCOLATA                                                
ESEGUITO   28/01/20  08:04
RNB          00087901371101P ";

		var interpretation = new SadIrispEDIMessagePrettyFormatter(Factory).GetFormattedText(irispMessage.EM_MessageText);
		AssertMultilineASCIIEquals(expectedInterpretation, interpretation);
	}

	public void TestEM_MessageInterpretation_PositiveIrispWithNegativeNb()
	{
		string expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>00200120.RXE</<p><br/><br/>" +
			"<h3 style='display:inline'>Status: </h3><p style='display:inline;color:green'>Registered</<p><br/><br/>" +
			"<h3 style='display:inline'>Customs Office: </h3><p style='display:inline;color:'>IT371100</<p><br/><br/>" +
			"<h3 style='display:inline'>Reg. No: </h3><p style='display:inline;color:'>6--00000006-Z-20/01/2020</<p><br/><br/>" +
			"<h3 style='display:inline'>A93 number: </h3><p style='display:inline;color:'>000370</<p><br/><br/>" +
			"<h3 style='display:inline'>First method of payment: </h3><p style='display:inline;color:'>G</<p><br/><br/>" +
			"<h3 style='display:inline'>First expiry date: </h3><p style='display:inline;color:'>23/02/2020</<p><br/><br/>" +
			"<h3 style='display:inline'>Second method of payment: </h3><p style='display:inline;color:'>F</<p><br/><br/>" +
			"<h3 style='display:inline'>Second expiry date: </h3><p style='display:inline;color:'>23/02/2020</<p><br/><br/><br/>" +
			"<hr/>" +
			"<h3 style='color:#5d85cc'><b>NB Records</b></h3>" +
			"<h3 style='color:red;margin-left: 10pt'>Error 1</h3><br/>" +
			"<table style='margin-left: 20pt'>" +
			"<tr><td><p>Entry Line:</p></td><td>0</td></tr>" +
			"<tr><td><p>Field:</p></td><td>0000</td></tr>" +
			"<tr><td><p>Occurrence:</p></td><td>0</td></tr>" +
			"<tr><td><p>Error Type:</p></td><td>L</td></tr>" +
			"<tr><td><p>Error Message:</p></td><td>15 - [Incoerenza di saldo partita - Quantita residue calcolate non tutte a zero]-Nota- Partita 2 Num. 138L Sing. 1 - 371101</td></tr>" +
			"</table><br/>";

		irispMessage.EM_MessageText = @"0020            00200120.XXE200000905859371100    005908A         001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  20/01/20 12:00,00200120.RXE
 RL=003,RS=000,ME=002,MS=000,PE=001,PS=000
ESEGUITO   20/01/20  12:00
RIM          32218900371100P6   00000006Z200120014605S000370G230220F230220 000000                                                                                                                  
ESEGUITO   20/01/20  12:00
RNB          32218901371100N                                        
 000000 00L 00 Errore 15 : [Incoerenza di saldo partita - Quantita residue calcolate non tutte a zero]-Nota: Partita 2 Num. 138L Sing. 1 - 371101";

		var interpretation = new SadIrispEDIMessagePrettyFormatter(Factory).GetFormattedText(irispMessage.EM_MessageText);
		AssertMultilineASCIIEquals(expectedInterpretation, interpretation);

		expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>00200114.RK4</<p><br/><br/>" +
			"<h3 style='display:inline'>Status: </h3><p style='display:inline;color:green'>Registered</<p><br/><br/>" +
			"<h3 style='display:inline'>Customs Office: </h3><p style='display:inline;color:'>IT371100</<p><br/><br/>" +
			"<h3 style='display:inline'>Reg. No: </h3><p style='display:inline;color:'>6--00000002-R-14/01/2020</<p><br/><br/>" +
			"<h3 style='display:inline'>A93 number: </h3><p style='display:inline;color:'>000232</<p><br/><br/>" +
			"<h3 style='display:inline'>First method of payment: </h3><p style='display:inline;color:'>G</<p><br/><br/>" +
			"<h3 style='display:inline'>First expiry date: </h3><p style='display:inline;color:'>08/02/2020</<p><br/><br/>" +
			"<h3 style='display:inline'>Second method of payment: </h3><p style='display:inline;color:'>F</<p><br/><br/>" +
			"<h3 style='display:inline'>Second expiry date: </h3><p style='display:inline;color:'>08/02/2020</<p><br/><br/><br/>" +
			"<hr/>" +
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
RIM          32213800371100P6   00000002R140120014605S000232G080220F080220 000000                                                                                                                  
ESEGUITO   14/01/20  15:09
RNB          32213801371100N                                        
 000J00 00L 00 Errore 364 : [Colli incoerenti con archivio]           ";

		interpretation = new SadIrispEDIMessagePrettyFormatter(Factory).GetFormattedText(irispMessage.EM_MessageText);
		AssertMultilineASCIIEquals(expectedInterpretation, interpretation);
	}

	public void TestEM_MessageInterpretation_PositiveIrispWithPartialPositiveNb()
	{
		var expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>00200120.RXE</<p><br/><br/>" +
			"<h3 style='display:inline'>Status: </h3><p style='display:inline;color:green'>Registered</<p><br/><br/>" +
			"<h3 style='display:inline'>Customs Office: </h3><p style='display:inline;color:'>IT371100</<p><br/><br/>" +
			"<h3 style='display:inline'>Reg. No: </h3><p style='display:inline;color:'>6--00000006-Z-20/01/2020</<p><br/><br/>" +
			"<h3 style='display:inline'>A93 number: </h3><p style='display:inline;color:'>000370</<p><br/><br/>" +
			"<h3 style='display:inline'>First method of payment: </h3><p style='display:inline;color:'>G</<p><br/><br/>" +
			"<h3 style='display:inline'>First expiry date: </h3><p style='display:inline;color:'>23/02/2020</<p><br/><br/>" +
			"<h3 style='display:inline'>Second method of payment: </h3><p style='display:inline;color:'>F</<p><br/><br/>" +
			"<h3 style='display:inline'>Second expiry date: </h3><p style='display:inline;color:'>23/02/2020</<p><br/><br/><br/>" +
			"<hr/>" +
			"<h3 style='color:#5d85cc'><b>NB Records</b></h3>" +
			"<h3 style='color:red;margin-left: 10pt'>Error 1</h3><br/>" +
			"<table style='margin-left: 20pt'>" +
			"<tr><td><p>Entry Line:</p></td><td>0</td></tr>" +
			"<tr><td><p>Field:</p></td><td>0000</td></tr>" +
			"<tr><td><p>Occurrence:</p></td><td>0</td></tr>" +
			"<tr><td><p>Error Type:</p></td><td>L</td></tr>" +
			"<tr><td><p>Error Message:</p></td><td>15 - [Incoerenza di saldo partita - Quantita residue calcolate non tutte a zero]-Nota- Partita 2 Num. 138L Sing. 1 - 371101</td></tr>" +
			"</table><br/>";

		irispMessage.EM_MessageText = @"0020            00200120.XXE200000905859371100    005908A         001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  20/01/20 12:00,00200120.RXE
 RL=003,RS=000,ME=002,MS=000,PE=001,PS=000
ESEGUITO   20/01/20  12:00
RIM          32218900371100P6   00000006Z200120014605S000370G230220F230220 000000
ESEGUITO   20/01/20  12:00
RNB          32218901371100P 
ESEGUITO   20/01/20  12:00
RNB          32218902371100N                                        
 000000 00L 00 Errore 15 : [Incoerenza di saldo partita - Quantita residue calcolate non tutte a zero]-Nota: Partita 2 Num. 138L Sing. 1 - 371101";

		var interpretation = new SadIrispEDIMessagePrettyFormatter(Factory).GetFormattedText(irispMessage.EM_MessageText);
		AssertMultilineASCIIEquals(expectedInterpretation, interpretation);
	}

	public void TestEM_MessageIntepretation_PositiveStandaloneNbIrisp()
	{
		string expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>004V0328.R26</<p><br/><br/>" +
			"<h3 style='color:#5d85cc'><b>NB Records</b></h3>" +
			"<h3 style='color:green'><b>All Approved</b></h3><br/>";

		irispMessage.EM_MessageText = @"004V            004V0328.X26180000121143035101    01878690229     001 00005    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  28/03/18 14:55,004V0328.R26
 RL=001,RS=000,ME=001,MS=000,PE=000,PS=000
ESEGUITO   28/03/18  14:55
RNB          67129700035101P ";

		var interpretation = new SadIrispEDIMessagePrettyFormatter(Factory).GetFormattedText(irispMessage.EM_MessageText);
		AssertMultilineASCIIEquals(expectedInterpretation, interpretation);
	}

	public void TestEM_MessageIntepretation_NegativeStandaloneNbIrisp()
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

		var interpretation = new SadIrispEDIMessagePrettyFormatter(Factory).GetFormattedText(irispMessage.EM_MessageText);
		AssertMultilineASCIIEquals(expectedInterpretation, interpretation);
	}

	public void TestEM_MessageIntepretation_PartialPositiveStandaloneNbIrisp()
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

		var interpretation = new SadIrispEDIMessagePrettyFormatter(Factory).GetFormattedText(irispMessage.EM_MessageText);
		AssertMultilineASCIIEquals(expectedInterpretation, interpretation);
	}

	protected override void SetUp()
	{
		base.SetUp();
		irispMessage = Factory.New<ITEDIMessage>();
	}

	ITEDIMessage irispMessage;
}
