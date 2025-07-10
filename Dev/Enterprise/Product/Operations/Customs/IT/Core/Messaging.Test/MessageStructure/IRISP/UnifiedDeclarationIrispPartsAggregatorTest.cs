using System;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class UnifiedDeclarationIrispPartsAggregatorTest : TestCase
{
	public void TestConstructorThrowsException()
	{
		AssertExceptionThrown<ArgumentNullException>(() => { new UnifiedDeclarationIrispPartsAggregator(null); });
		AssertNoExceptionThrown(() => { new UnifiedDeclarationIrispPartsAggregator(new IrispTypeR()); });
	}

	public void TestImportIrispWithPositiveMessages()
	{
		var irispFullContent = @"01N6            01N61120.X01190000238592304100    005916R         001 00007    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  20/11/19 15:32,01N61120.R01
 RL=006,RS=000,ME=002,MS=000,PE=002,PS=000
ESEGUITO   20/11/19  15:32
RIM          00010800304100P4   00001099C201119000000 000000 000000 000000 000000                                                                                                                  
ESEGUITO   20/11/19  15:32
RIM          00010900304100P4   00001100C201119000000 000000 000000 000000 000000                                                                                                                  ";

		var irispObject = MessageStructure.CustomsInterchange.LoadSafe<IrispTypeR>(irispFullContent).Interchange;
		var aggregatedPartFor000108 = irispObject.Aggregator.GetTextForMessage("000108");
		AssertMultilineASCIIEquals(@"01N6            01N61120.X01190000238592304100    005916R         001 00007    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  20/11/19 15:32,01N61120.R01
 RL=006,RS=000,ME=002,MS=000,PE=002,PS=000
ESEGUITO   20/11/19  15:32
RIM          00010800304100P4   00001099C201119000000 000000 000000 000000 000000                                                                                                                  ", aggregatedPartFor000108);

		var aggregatedPartFor000109 = irispObject.Aggregator.GetTextForMessage("000109");
		AssertMultilineASCIIEquals(@"01N6            01N61120.X01190000238592304100    005916R         001 00007    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  20/11/19 15:32,01N61120.R01
 RL=006,RS=000,ME=002,MS=000,PE=002,PS=000
ESEGUITO   20/11/19  15:32
RIM          00010900304100P4   00001100C201119000000 000000 000000 000000 000000                                                                                                                  ", aggregatedPartFor000109);

		var aggregatedPartForUnexistingMessage = irispObject.Aggregator.GetTextForMessage("000110");
		AssertMultilineASCIIEquals(ZString.Empty, aggregatedPartForUnexistingMessage);
	}

	public void TestImportIrispWithNegativeMessages()
	{
		var irispFullContent = @"01N6            01N61120.X06190000238724304100    005916R         001 00012    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  20/11/19 16:31,01N61120.R06
 RL=008,RS=000,ME=003,MS=000,PE=002,PS=000
ESEGUITO   20/11/19  16:31
RIM          00011800304100N                                        
 010500 00F 00 Errore 3 : [Valore non ammesso]-Nota: numero singoli dichiarato incoerente con dati inseriti
ESEGUITO   20/11/19  16:31
RNB          00011801304100N                                        
 000000 00F 00 Errore 524 : [Dichiarazione elaborata con errori!]     
ESEGUITO   20/11/19  16:31
RIM          00011900304100N                                        
 010000 00F 00 Errore 5 : [Data errata]-Nota: Data accettazione       ";
		var irispObject = MessageStructure.CustomsInterchange.LoadSafe<IrispTypeR>(irispFullContent).Interchange;

		var aggregatedPartFor000118 = irispObject.Aggregator.GetTextForMessage("000118");
		AssertMultilineASCIIEquals(@"01N6            01N61120.X06190000238724304100    005916R         001 00012    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  20/11/19 16:31,01N61120.R06
 RL=008,RS=000,ME=003,MS=000,PE=002,PS=000
ESEGUITO   20/11/19  16:31
RIM          00011800304100N                                        
 010500 00F 00 Errore 3 : [Valore non ammesso]-Nota: numero singoli dichiarato incoerente con dati inseriti
ESEGUITO   20/11/19  16:31
RNB          00011801304100N                                        
 000000 00F 00 Errore 524 : [Dichiarazione elaborata con errori!]     ", aggregatedPartFor000118);

		var aggregatedPartFor000119 = irispObject.Aggregator.GetTextForMessage("000119");
		AssertMultilineASCIIEquals(@"01N6            01N61120.X06190000238724304100    005916R         001 00012    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  20/11/19 16:31,01N61120.R06
 RL=008,RS=000,ME=003,MS=000,PE=002,PS=000
ESEGUITO   20/11/19  16:31
RIM          00011900304100N                                        
 010000 00F 00 Errore 5 : [Data errata]-Nota: Data accettazione       ", aggregatedPartFor000119);

		var aggregatedPartForUnexistingMessage = irispObject.Aggregator.GetTextForMessage("000110");
		AssertMultilineASCIIEquals(ZString.Empty, aggregatedPartForUnexistingMessage);
	}

	public void TestImportIrispWithBothPositiveAndNegativeMessages()
	{
		var irispFullContent = @"01N6            01N61120.X02190000238602304100    005916R         001 00008    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  20/11/19 15:38,01N61120.R02
 RL=006,RS=000,ME=002,MS=000,PE=002,PS=000
ESEGUITO   20/11/19  15:38
RIM          00011000304100N                                        
 010000 00F 00 Errore 5 : [Data errata]-Nota: Data accettazione       
ESEGUITO   20/11/19  15:38
RIM          00011100304100P4   00001101V201119000000 000000 000000 000000 000000                                                                                                                  ";

		var irispObject = MessageStructure.CustomsInterchange.LoadSafe<IrispTypeR>(irispFullContent).Interchange;

		var aggregatedPartFor000110 = irispObject.Aggregator.GetTextForMessage("000110");
		AssertMultilineASCIIEquals(@"01N6            01N61120.X02190000238602304100    005916R         001 00008    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  20/11/19 15:38,01N61120.R02
 RL=006,RS=000,ME=002,MS=000,PE=002,PS=000
ESEGUITO   20/11/19  15:38
RIM          00011000304100N                                        
 010000 00F 00 Errore 5 : [Data errata]-Nota: Data accettazione       ", aggregatedPartFor000110);

		var aggregatedPartFor000111 = irispObject.Aggregator.GetTextForMessage("000111");
		AssertMultilineASCIIEquals(@"01N6            01N61120.X02190000238602304100    005916R         001 00008    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  20/11/19 15:38,01N61120.R02
 RL=006,RS=000,ME=002,MS=000,PE=002,PS=000
ESEGUITO   20/11/19  15:38
RIM          00011100304100P4   00001101V201119000000 000000 000000 000000 000000                                                                                                                  ", aggregatedPartFor000111);

		var aggregatedPartForUnexistingMessage = irispObject.Aggregator.GetTextForMessage("000112");
		AssertMultilineASCIIEquals(ZString.Empty, aggregatedPartForUnexistingMessage);
	}

	public void TestAggregatedPartsAreStillAValidIrisp()
	{
		CombineAssertions("Assertions for IM", () =>
		{
			var irispFullContent = @"01N6            01N61120.X01190000238592304100    005916R         001 00007    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  20/11/19 15:32,01N61120.R01
 RL=006,RS=000,ME=002,MS=000,PE=002,PS=000
ESEGUITO   20/11/19  15:32
RIM          00010800304100P4   00001099C201119000000 000000 000000 000000 000000                                                                                                                  
ESEGUITO   20/11/19  15:32
RIM          00010900304100P4   00001100C201119000000 000000 000000 000000 000000                                                                                                                  ";

			var irispObject = MessageStructure.CustomsInterchange.LoadSafe<IrispTypeR>(irispFullContent).Interchange;
			var aggregatedPartFor000108 = irispObject.Aggregator.GetTextForMessage("000108");
			var irispLoadedFrom000108AggregatedPart = MessageStructure.CustomsInterchange.LoadSafe<IrispTypeR>(aggregatedPartFor000108).Interchange;
			AssertNotNull("Aggregated parts for 000108 are still a valid irisp interchange", irispLoadedFrom000108AggregatedPart);

			var aggregatedPartFor000109 = irispObject.Aggregator.GetTextForMessage("000109");
			var irispLoadedFrom000109AggregatedPart = MessageStructure.CustomsInterchange.LoadSafe<IrispTypeR>(aggregatedPartFor000109).Interchange;
			AssertNotNull("Aggregated parts for 000109 are still a valid irisp interchange", irispLoadedFrom000109AggregatedPart);

			var aggregatedPartForUnexistingMessage = irispObject.Aggregator.GetTextForMessage("000110");
			var irispLoadedFromUnexistingMessage = MessageStructure.CustomsInterchange.LoadSafe<IrispTypeR>(aggregatedPartForUnexistingMessage).Interchange;
			AssertNull("Aggregated parts for 000110 is not a valid irisp interchange", irispLoadedFromUnexistingMessage);
		});

		CombineAssertions("Assertions for IM with NB", () =>
		{
			var irispFullContent = @"01N6            01N61120.X06190000238724304100    005916R         001 00012    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  20/11/19 16:31,01N61120.R06
 RL=008,RS=000,ME=003,MS=000,PE=002,PS=000
ESEGUITO   20/11/19  16:31
RIM          00011800304100N                                        
 010500 00F 00 Errore 3 : [Valore non ammesso]-Nota: numero singoli dichiarato incoerente con dati inseriti
ESEGUITO   20/11/19  16:31
RNB          00011801304100N                                        
 000000 00F 00 Errore 524 : [Dichiarazione elaborata con errori!]     
ESEGUITO   20/11/19  16:31
RIM          00011900304100N                                        
 010000 00F 00 Errore 5 : [Data errata]-Nota: Data accettazione       ";
			var irispObject = MessageStructure.CustomsInterchange.LoadSafe<IrispTypeR>(irispFullContent).Interchange;

			var aggregatedPartFor000118 = irispObject.Aggregator.GetTextForMessage("000118");
			var irispLoadedFrom000118AggregatedPart = MessageStructure.CustomsInterchange.LoadSafe<IrispTypeR>(aggregatedPartFor000118).Interchange;
			AssertNotNull("Aggregated parts for 000118 are still a valid irisp interchange", irispLoadedFrom000118AggregatedPart);

			var aggregatedPartFor000119 = irispObject.Aggregator.GetTextForMessage("000119");
			var irispLoadedFrom000119AggregatedPart = MessageStructure.CustomsInterchange.LoadSafe<IrispTypeR>(aggregatedPartFor000119).Interchange;
			AssertNotNull("Aggregated parts for 000119 are still a valid irisp interchange", irispLoadedFrom000119AggregatedPart);

			var aggregatedPartForUnexistingMessage = irispObject.Aggregator.GetTextForMessage("000110");
			var irispLoadedFromUnexistingMessage = MessageStructure.CustomsInterchange.LoadSafe<IrispTypeR>(aggregatedPartForUnexistingMessage).Interchange;
			AssertNull("Aggregated parts for 000110 is not a valid irisp interchange", irispLoadedFromUnexistingMessage);
		});
	}
}
