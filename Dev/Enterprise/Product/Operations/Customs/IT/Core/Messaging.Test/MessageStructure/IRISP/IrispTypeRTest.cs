using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IrispTypeRTest : TestCase
{
	public void TestReadEmptyContent()
	{
		var parseResult = CustomsInterchange.LoadSafe<IrispTypeR>("");
		AssertNotNullOrEmpty(parseResult.ErrorText);
		Assert(!parseResult.IsValid);
		AssertNull(parseResult.Interchange);
	}

	public void TestReadInvalidContent()
	{
		var parseResult = CustomsInterchange.LoadSafe<IrispTypeR>(@"004R            004R1004.X04190015588917275100    01790970139     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  04/10/19 06:04,004R10081119F081119 000000X91B0ZSVINCOLATA                                                                                                  ");
		AssertNotNullOrEmpty(parseResult.ErrorText);
		Assert(!parseResult.IsValid);
		AssertNull(parseResult.Interchange);
	}

	#region Irisp for IM

	public void TestReadImportPositiveIrispWithoutNb()
	{
		var parseResult = CustomsInterchange.LoadSafe<IrispTypeR>(@"004R            004R1004.X04190015588917275100    01790970139     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  04/10/19 06:04,004R1004.R04
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   04/10/19  06:04
RIM          21309000275100P4 T 00031664X041019014135A000273G081119F081119 000000X91B0ZSVINCOLATA                                                                                                  ");
		AssertNullOrEmpty(parseResult.ErrorText);
		Assert(parseResult.IsValid);
		var irispObject = parseResult.Interchange;
		AssertNotNull(irispObject);
		AssertNotNull(irispObject.Header);
		AssertEquals("004R            004R1004.X04190015588917275100    01790970139     001 00005    INVIO IN AMBIENTE REALE       ", irispObject.Header.InnerText);
		AssertEquals("Header authorized user code", "004R", irispObject.Header.AuthorizedUserCode);
		AssertEquals("Header file name", "004R1004.X04", irispObject.Header.FileName);
		AssertEquals("Header customs office section code", "275100", irispObject.Header.CustomsOfficeSectionCode);
		AssertEquals("Header country code", "", irispObject.Header.CountryCode);
		AssertEquals("Header vat registration number", "01790970139", irispObject.Header.TaxCodeOrVATRegistrationNumber);
		AssertEquals("Header progressive seat authorized account", 1, irispObject.Header.ProgressiveSeatAuthorizedAccount);
		AssertEquals("Header number of record in the file", 5, irispObject.Header.NumberOfRecordsInTheFile);
		AssertEquals("Header transmission environment", "INVIO IN AMBIENTE REALE", irispObject.Header.TransmissionEnvironment);
		Assert("Is production transmission environment", irispObject.Header.IsProductionTransmissionEnvironment);

		var idocElaborationInfo = irispObject.IdocElaborationInfo;
		AssertNotNull(idocElaborationInfo);
		AssertEquals(" RICEVUTO  04/10/19 06:04,004R1004.R04", idocElaborationInfo.InnerText);
		AssertEquals("Idoc date time of receipt", new ZDateTime(2019, 10, 04, 06, 04, 00), idocElaborationInfo.IdocDateTimeOfReceipt);
		AssertEquals("Idoc name", "004R1004.R04", idocElaborationInfo.IdocName);

		var controlResult = irispObject.ControlResult;
		AssertNotNull(controlResult);
		AssertEquals(" RL=002,RS=000,ME=001,MS=000,PE=001,PS=000", controlResult.InnerText);
		AssertEquals("Number of records read", 2, controlResult.NumberOfRecordsRead);
		AssertEquals("Number of records rejected", 0, controlResult.NumberOfRecordsRejected);
		AssertEquals("Number of messages read", 1, controlResult.NumberOfMessagesProcessed);
		AssertEquals("Number of messages rejected", 0, controlResult.NumberOfMessagesRejected);
		AssertEquals("Number of procedures read", 1, controlResult.NumberOfProceduresProcessed);
		AssertEquals("Number of procedures rejected", 0, controlResult.NumberOfProceduresRejected);

		Assert(irispObject.IsPositive);
		Assert(!irispObject.IsPartialPositive);
		Assert(!irispObject.IsNegative);

		var responseMessages = irispObject.ResponseMessages;
		AssertNotNull(responseMessages);
		AssertEquals("Response messages count", 1, responseMessages.Count);

		var negativeMessages = irispObject.GetSadNegativeResponseMessages();
		AssertNotNull(negativeMessages);
		AssertEquals("Negative response messages count", 0, negativeMessages.Count());

		var positiveMessages = irispObject.GetSadPositiveResponseMessages();
		AssertNotNull(positiveMessages);
		AssertEquals("Positive response messages count", 1, positiveMessages.Count());

		var positiveMessage = positiveMessages.ElementAt(0);
		AssertMultilineASCIIEquals(@"ESEGUITO   04/10/19  06:04
RIM          21309000275100P4 T 00031664X041019014135A000273G081119F081119 000000X91B0ZSVINCOLATA                                                                                                  ", positiveMessage.InnerText);
		AssertEquals("Idoc elaboration date time", new ZDateTime(2019, 10, 04, 06, 04, 00), positiveMessage.IdocElaborationDateTime);

		AssertEquals("R", positiveMessage.RecordType);
		AssertEquals("IM", positiveMessage.MessageCode);
		AssertEquals("213090", positiveMessage.DeclarationNumber);
		AssertEquals(0, positiveMessage.MessageProgressiveNumber);
		AssertEquals("275100", positiveMessage.CustomsOffice);
		AssertEquals("P", positiveMessage.OperationResult);

		AssertEquals("4", positiveMessage.RegisterCode);
		AssertEquals("T", positiveMessage.RegisterSeries);
		AssertEquals("00031664", positiveMessage.RegistrationNumber);
		AssertEquals("X", positiveMessage.RegistrationNumberCin);
		AssertEquals(new ZDate(2019, 10, 04), positiveMessage.RegistrationDate);
		AssertEquals("014135", positiveMessage.DebitAccountNumber);
		AssertEquals("A", positiveMessage.DebitAccountNumberCin);
		AssertEquals("000273", positiveMessage.A93Number);
		AssertEquals("G", positiveMessage.FirstPaymentMethod);
		AssertEquals(new ZDate(2019, 11, 08), positiveMessage.FirstPaymentDueDate);
		AssertEquals("F", positiveMessage.SecondPaymentMethod);
		AssertEquals(new ZDate(2019, 11, 08), positiveMessage.SecondPaymentDueDate);
		AssertEquals("", positiveMessage.ThirdPaymentMethod);
		AssertEquals(ZDate.Empty, positiveMessage.ThirdPaymentDueDate);
		AssertEquals("X91B0Z", positiveMessage.ReleaseCode);
		AssertEquals("SVINCOLATA", positiveMessage.ReleaseNotes);
		AssertEquals("", positiveMessage.MrnCode);
		AssertNull(positiveMessage.GuaranteeAmount);
		AssertEquals("", positiveMessage.Notes);
		AssertEquals("4-T-00031664-X-04/10/2019", positiveMessage.FullRegistrationInfo);
		AssertEquals("4 T-31664X", positiveMessage.RegistrationInfo);
		Assert(positiveMessage.IsCleared);
		Assert(!positiveMessage.IsAwaitingResponse);
		Assert(positiveMessage.IsRegistered);
		Assert(!positiveMessage.IsUnderControl);
		Assert(positiveMessage.HasA93FirstPayment);
		Assert(positiveMessage.HasA93SecondPayment);
		Assert(!positiveMessage.HasA93ThirdPayment);
	}

	public void TestReadImportNegativeIrispWithoutNb()
	{
		var parseResult = CustomsInterchange.LoadSafe<IrispTypeR>(@"02VM            02VM1004.X01190015592475136101    007775D         001 00006    INVIO IN AMBIENTE REALE       
 RICEVUTO  04/10/19 08:07,02VM1004.R01
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   04/10/19  08:07
RIM          37507700136101N                                        
 013302 00L 00 Errore 0 : [Codice addizionale in input mancante]      ");
		AssertNullOrEmpty(parseResult.ErrorText);
		Assert(parseResult.IsValid);
		var irispObject = parseResult.Interchange;
		AssertNotNull(irispObject);
		AssertNotNull(irispObject.Header);
		AssertEquals("02VM            02VM1004.X01190015592475136101    007775D         001 00006    INVIO IN AMBIENTE REALE       ", irispObject.Header.InnerText);
		AssertEquals("Header authorized user code", "02VM", irispObject.Header.AuthorizedUserCode);
		AssertEquals("Header file name", "02VM1004.X01", irispObject.Header.FileName);
		AssertEquals("Header customs office section code", "136101", irispObject.Header.CustomsOfficeSectionCode);
		AssertEquals("Header country code", "", irispObject.Header.CountryCode);
		AssertEquals("Header vat registration number", "007775D", irispObject.Header.TaxCodeOrVATRegistrationNumber);
		AssertEquals("Header progressive seat authorized account", 1, irispObject.Header.ProgressiveSeatAuthorizedAccount);
		AssertEquals("Header number of record in the file", 6, irispObject.Header.NumberOfRecordsInTheFile);
		AssertEquals("Header transmission environment", "INVIO IN AMBIENTE REALE", irispObject.Header.TransmissionEnvironment);
		Assert("Is production transmission environment", irispObject.Header.IsProductionTransmissionEnvironment);

		var idocElaborationInfo = irispObject.IdocElaborationInfo;
		AssertNotNull(idocElaborationInfo);
		AssertEquals(" RICEVUTO  04/10/19 08:07,02VM1004.R01", idocElaborationInfo.InnerText);
		AssertEquals("Idoc date time of receipt", new ZDateTime(2019, 10, 04, 08, 07, 00), idocElaborationInfo.IdocDateTimeOfReceipt);
		AssertEquals("Idoc name", "02VM1004.R01", idocElaborationInfo.IdocName);

		var controlResult = irispObject.ControlResult;
		AssertNotNull(controlResult);
		AssertEquals(" RL=002,RS=000,ME=001,MS=000,PE=001,PS=000", controlResult.InnerText);
		AssertEquals("Number of records read", 2, controlResult.NumberOfRecordsRead);
		AssertEquals("Number of records rejected", 0, controlResult.NumberOfRecordsRejected);
		AssertEquals("Number of messages read", 1, controlResult.NumberOfMessagesProcessed);
		AssertEquals("Number of messages rejected", 0, controlResult.NumberOfMessagesRejected);
		AssertEquals("Number of procedures read", 1, controlResult.NumberOfProceduresProcessed);
		AssertEquals("Number of procedures rejected", 0, controlResult.NumberOfProceduresRejected);

		Assert(!irispObject.IsPositive);
		Assert(!irispObject.IsPartialPositive);
		Assert(irispObject.IsNegative);

		var responseMessages = irispObject.ResponseMessages;
		AssertNotNull(responseMessages);
		AssertEquals("Response messages count", 1, responseMessages.Count);

		var positiveMessages = irispObject.GetSadPositiveResponseMessages();
		AssertNotNull(positiveMessages);
		AssertEquals("Positive response messages count", 0, positiveMessages.Count());

		var negativeMessages = irispObject.GetSadNegativeResponseMessages();
		AssertNotNull(negativeMessages);
		AssertEquals("Negative response messages count", 1, negativeMessages.Count());

		var negativeMessage = negativeMessages.ElementAt(0);
		AssertMultilineASCIIEquals(@"ESEGUITO   04/10/19  08:07
RIM          37507700136101N                                        
 013302 00L 00 Errore 0 : [Codice addizionale in input mancante]      ", negativeMessage.InnerText);
		AssertEquals("Idoc elaboration date time", new ZDateTime(2019, 10, 04, 08, 07, 00), negativeMessage.IdocElaborationDateTime);

		AssertEquals("R", negativeMessage.RecordType);
		AssertEquals("IM", negativeMessage.MessageCode);
		AssertEquals("375077", negativeMessage.DeclarationNumber);
		AssertEquals(0, negativeMessage.MessageProgressiveNumber);
		AssertEquals("136101", negativeMessage.CustomsOffice);
		AssertEquals("N", negativeMessage.OperationResult);

		var irregularityRecord = negativeMessage.Irregularity;
		AssertNotNull(irregularityRecord);

		AssertEquals(1, irregularityRecord.ItemNumber);
		AssertEquals("3302", irregularityRecord.FieldIdentificative);
		AssertEquals(0, irregularityRecord.SequentialNumberOfFieldInRecord);
		AssertEquals("L", irregularityRecord.ErrorType);
		AssertEquals(0, irregularityRecord.ControlNumber);
		AssertEquals("Errore 0 : [Codice addizionale in input mancante]", irregularityRecord.ErrorDescription);
		AssertEquals("33.2", irregularityRecord.FieldSadBoxNumber);
	}

	public void TestReadImportNegativeIrispWithNegativeNb()
	{
		var parseResult = CustomsInterchange.LoadSafe<IrispTypeR>(@"01N6            01N61120.X06190000238724304100    005916R         001 00012    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  20/11/19 16:31,01N61120.R06
 RL=008,RS=000,ME=003,MS=000,PE=002,PS=000
ESEGUITO   20/11/19  16:31
RIM          00011800304100N                                        
 010500 00F 00 Errore 3 : [Valore non ammesso]-Nota: numero singoli dichiarato incoerente con dati inseriti
ESEGUITO   20/11/19  16:31
RNB          00011801304100N                                        
 000000 00F 00 Errore 524 : [Dichiarazione elaborata con errori!]     ");
		AssertNullOrEmpty(parseResult.ErrorText);
		Assert(parseResult.IsValid);
		var irispObject = parseResult.Interchange;
		AssertNotNull(irispObject);
		AssertNotNull(irispObject.Header);
		AssertEquals("01N6            01N61120.X06190000238724304100    005916R         001 00012    INVIO IN AMBIENTE DI PROVA    ", irispObject.Header.InnerText);
		AssertEquals("Header authorized user code", "01N6", irispObject.Header.AuthorizedUserCode);
		AssertEquals("Header file name", "01N61120.X06", irispObject.Header.FileName);
		AssertEquals("Header customs office section code", "304100", irispObject.Header.CustomsOfficeSectionCode);
		AssertEquals("Header country code", "", irispObject.Header.CountryCode);
		AssertEquals("Header vat registration number", "005916R", irispObject.Header.TaxCodeOrVATRegistrationNumber);
		AssertEquals("Header progressive seat authorized account", 1, irispObject.Header.ProgressiveSeatAuthorizedAccount);
		AssertEquals("Header number of record in the file", 12, irispObject.Header.NumberOfRecordsInTheFile);
		AssertEquals("Header transmission environment", "INVIO IN AMBIENTE DI PROVA", irispObject.Header.TransmissionEnvironment);
		Assert("Is production transmission environment", !irispObject.Header.IsProductionTransmissionEnvironment);

		var idocElaborationInfo = irispObject.IdocElaborationInfo;
		AssertNotNull(idocElaborationInfo);
		AssertEquals(" RICEVUTO  20/11/19 16:31,01N61120.R06", idocElaborationInfo.InnerText);
		AssertEquals("Idoc date time of receipt", new ZDateTime(2019, 11, 20, 16, 31, 00), idocElaborationInfo.IdocDateTimeOfReceipt);
		AssertEquals("Idoc name", "01N61120.R06", idocElaborationInfo.IdocName);

		var controlResult = irispObject.ControlResult;
		AssertNotNull(controlResult);
		AssertEquals(" RL=008,RS=000,ME=003,MS=000,PE=002,PS=000", controlResult.InnerText);
		AssertEquals("Number of records read", 8, controlResult.NumberOfRecordsRead);
		AssertEquals("Number of records rejected", 0, controlResult.NumberOfRecordsRejected);
		AssertEquals("Number of messages read", 3, controlResult.NumberOfMessagesProcessed);
		AssertEquals("Number of messages rejected", 0, controlResult.NumberOfMessagesRejected);
		AssertEquals("Number of procedures read", 2, controlResult.NumberOfProceduresProcessed);
		AssertEquals("Number of procedures rejected", 0, controlResult.NumberOfProceduresRejected);

		Assert(!irispObject.IsPositive);
		Assert(!irispObject.IsPartialPositive);
		Assert(irispObject.IsNegative);

		var responseMessages = irispObject.ResponseMessages;
		AssertNotNull(responseMessages);
		AssertEquals("Response messages count", 2, responseMessages.Count);

		var positiveMessages = irispObject.GetSadPositiveResponseMessages();
		AssertNotNull(positiveMessages);
		AssertEquals("Positive response messages count", 0, positiveMessages.Count());

		CombineAssertions("IM negative message", () =>
		{
			var negativeMessages = irispObject.GetSadNegativeResponseMessages();
			AssertNotNull(negativeMessages);
			AssertEquals("IM negative response messages count", 1, negativeMessages.Count());

			var negativeMessage = negativeMessages.ElementAt(0);
			AssertMultilineASCIIEquals(@"ESEGUITO   20/11/19  16:31
RIM          00011800304100N                                        
 010500 00F 00 Errore 3 : [Valore non ammesso]-Nota: numero singoli dichiarato incoerente con dati inseriti", negativeMessage.InnerText);
			AssertEquals("Idoc elaboration date time", new ZDateTime(2019, 11, 20, 16, 31, 00), negativeMessage.IdocElaborationDateTime);

			AssertEquals("R", negativeMessage.RecordType);
			AssertEquals("IM", negativeMessage.MessageCode);
			AssertEquals("000118", negativeMessage.DeclarationNumber);
			AssertEquals(0, negativeMessage.MessageProgressiveNumber);
			AssertEquals("304100", negativeMessage.CustomsOffice);
			AssertEquals("N", negativeMessage.OperationResult);

			var irregularityRecord = negativeMessage.Irregularity;
			AssertNotNull(irregularityRecord);

			AssertEquals(1, irregularityRecord.ItemNumber);
			AssertEquals("0500", irregularityRecord.FieldIdentificative);
			AssertEquals(0, irregularityRecord.SequentialNumberOfFieldInRecord);
			AssertEquals("F", irregularityRecord.ErrorType);
			AssertEquals(0, irregularityRecord.ControlNumber);
			AssertEquals("Errore 3 : [Valore non ammesso]-Nota: numero singoli dichiarato incoerente con dati inseriti", irregularityRecord.ErrorDescription);
			AssertEquals("5", irregularityRecord.FieldSadBoxNumber);
		});

		CombineAssertions("NB negative message", () =>
		{
			var nbNegativeMessages = irispObject.GetNbNegativeResponseMessages();
			AssertNotNull(nbNegativeMessages);
			AssertEquals("NB Negative response messages count", 1, nbNegativeMessages.Count());

			var negativeMessage = nbNegativeMessages.ElementAt(0);
			AssertMultilineASCIIEquals(@"ESEGUITO   20/11/19  16:31
RNB          00011801304100N                                        
 000000 00F 00 Errore 524 : [Dichiarazione elaborata con errori!]     ", negativeMessage.InnerText);
			AssertEquals("Idoc elaboration date time", new ZDateTime(2019, 11, 20, 16, 31, 00), negativeMessage.IdocElaborationDateTime);

			AssertEquals("R", negativeMessage.RecordType);
			AssertEquals("NB", negativeMessage.MessageCode);
			AssertEquals("000118", negativeMessage.DeclarationNumber);
			AssertEquals(1, negativeMessage.MessageProgressiveNumber);
			AssertEquals("304100", negativeMessage.CustomsOffice);
			AssertEquals("N", negativeMessage.OperationResult);

			var irregularityRecord = negativeMessage.Irregularity;
			AssertNotNull(irregularityRecord);

			AssertEquals(0, irregularityRecord.ItemNumber);
			AssertEquals("0000", irregularityRecord.FieldIdentificative);
			AssertEquals(0, irregularityRecord.SequentialNumberOfFieldInRecord);
			AssertEquals("F", irregularityRecord.ErrorType);
			AssertEquals(0, irregularityRecord.ControlNumber);
			AssertEquals("Errore 524 : [Dichiarazione elaborata con errori!]", irregularityRecord.ErrorDescription);
			AssertEquals("", irregularityRecord.FieldSadBoxNumber);
		});
	}

	public void TestReadImportPositiveIrispWithPositiveNb()
	{
		var parseResult = CustomsInterchange.LoadSafe<IrispTypeR>(@"041G            041G0128.XJ2200001455200371101    00831370432     001 00007    INVIO IN AMBIENTE REALE       
 RICEVUTO  28/01/20 08:04,041G0128.RJ2
 RL=003,RS=000,ME=002,MS=000,PE=001,PS=000
ESEGUITO   28/01/20  08:04
RIM          00087900371101P4 T 00005514Q280120013413G000165G230220F230220 000000S6EHAWSVINCOLATA                                                
ESEGUITO   28/01/20  08:04
RNB          00087901371101P ");

		var irispObject = parseResult.Interchange;
		var responseMessages = irispObject.ResponseMessages;
		AssertNotNull(responseMessages);
		AssertEquals("Response messages count", 2, responseMessages.Count);

		var positiveMessages = irispObject.GetSadPositiveResponseMessages();
		AssertNotNull(positiveMessages);
		AssertEquals("Positive response messages count", 1, positiveMessages.Count());

		var positiveNbMessages = irispObject.GetNbPositiveResponseMessages();
		AssertNotNull(positiveNbMessages);
		AssertEquals("Positive NB response messages count", 1, positiveNbMessages.Count());

		var negativeMessages = irispObject.GetSadNegativeResponseMessages();
		AssertNotNull(negativeMessages);
		AssertEquals("Negative response messages count", 0, negativeMessages.Count());

		var negativeNbMessages = irispObject.GetSadNegativeResponseMessages();
		AssertNotNull(negativeNbMessages);
		AssertEquals("Negative NB response messages count", 0, negativeNbMessages.Count());

		var nbResponseMessage = positiveNbMessages.First();
		CombineAssertions("Assertions for NB positive message", () =>
		{
			AssertEquals("CustomsOffice", "371101", nbResponseMessage.CustomsOffice);
			AssertEquals("DeclarationNumber", "000879", nbResponseMessage.DeclarationNumber);
			AssertEquals("IdocElaborationDateTime", new ZDateTime(2020, 01, 28, 8, 4, 0), nbResponseMessage.IdocElaborationDateTime);
			AssertEquals("InnerText", "ESEGUITO   28/01/20  08:04\r\nRNB          00087901371101P ", nbResponseMessage.InnerText);
			AssertEquals("MessageCode", "NB", nbResponseMessage.MessageCode);
			AssertEquals("MessageProgressiveNumber", 1, nbResponseMessage.MessageProgressiveNumber);
			AssertEquals("OperationResult", "P", nbResponseMessage.OperationResult);
			AssertEquals("RecordType", "R", nbResponseMessage.RecordType);
		});
	}

	#endregion

	#region Irisp for ET

	public void TestReadPositiveIrispET()
	{
		var parseResult = CustomsInterchange.LoadSafe<IrispTypeR>(@"0RPE            0RPE1003.X81190015527541119101    02046110033     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  03/10/19 06:47,0RPE1003.R81
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   03/10/19  06:47
RET          91595600119101P1   00012466K031019000000 000000 000000 000000 000000                               19ITQSV010012466E6                                                                 ");
		AssertNullOrEmpty(parseResult.ErrorText);
		Assert(parseResult.IsValid);
		var irispObject = parseResult.Interchange;
		AssertNotNull(irispObject);
		AssertNotNull(irispObject.Header);
		AssertEquals("0RPE            0RPE1003.X81190015527541119101    02046110033     001 00005    INVIO IN AMBIENTE REALE       ", irispObject.Header.InnerText);
		AssertEquals("Header authorized user code", "0RPE", irispObject.Header.AuthorizedUserCode);
		AssertEquals("Header file name", "0RPE1003.X81", irispObject.Header.FileName);
		AssertEquals("Header customs office section code", "119101", irispObject.Header.CustomsOfficeSectionCode);
		AssertEquals("Header country code", "", irispObject.Header.CountryCode);
		AssertEquals("Header vat registration number", "02046110033", irispObject.Header.TaxCodeOrVATRegistrationNumber);
		AssertEquals("Header progressive seat authorized account", 1, irispObject.Header.ProgressiveSeatAuthorizedAccount);
		AssertEquals("Header number of record in the file", 5, irispObject.Header.NumberOfRecordsInTheFile);
		AssertEquals("Header transmission environment", "INVIO IN AMBIENTE REALE", irispObject.Header.TransmissionEnvironment);
		Assert("Is production transmission environment", irispObject.Header.IsProductionTransmissionEnvironment);

		var idocElaborationInfo = irispObject.IdocElaborationInfo;
		AssertNotNull(idocElaborationInfo);
		AssertEquals(" RICEVUTO  03/10/19 06:47,0RPE1003.R81", idocElaborationInfo.InnerText);
		AssertEquals("Idoc date time of receipt", new ZDateTime(2019, 10, 03, 06, 47, 00), idocElaborationInfo.IdocDateTimeOfReceipt);
		AssertEquals("Idoc name", "0RPE1003.R81", idocElaborationInfo.IdocName);

		var controlResult = irispObject.ControlResult;
		AssertNotNull(controlResult);
		AssertEquals(" RL=002,RS=000,ME=001,MS=000,PE=001,PS=000", controlResult.InnerText);
		AssertEquals("Number of records read", 2, controlResult.NumberOfRecordsRead);
		AssertEquals("Number of records rejected", 0, controlResult.NumberOfRecordsRejected);
		AssertEquals("Number of messages read", 1, controlResult.NumberOfMessagesProcessed);
		AssertEquals("Number of messages rejected", 0, controlResult.NumberOfMessagesRejected);
		AssertEquals("Number of procedures read", 1, controlResult.NumberOfProceduresProcessed);
		AssertEquals("Number of procedures rejected", 0, controlResult.NumberOfProceduresRejected);

		Assert(irispObject.IsPositive);
		Assert(!irispObject.IsPartialPositive);
		Assert(!irispObject.IsNegative);

		var responseMessages = irispObject.ResponseMessages;
		AssertNotNull(responseMessages);
		AssertEquals("Response messages count", 1, responseMessages.Count);

		var negativeMessages = irispObject.GetSadNegativeResponseMessages();
		AssertNotNull(negativeMessages);
		AssertEquals("Negative response messages count", 0, negativeMessages.Count());

		var positiveMessages = irispObject.GetSadPositiveResponseMessages();
		AssertNotNull(positiveMessages);
		AssertEquals("Positive response messages count", 1, positiveMessages.Count());

		var positiveMessage = positiveMessages.ElementAt(0);
		AssertMultilineASCIIEquals(@"ESEGUITO   03/10/19  06:47
RET          91595600119101P1   00012466K031019000000 000000 000000 000000 000000                               19ITQSV010012466E6                                                                 ", positiveMessage.InnerText);
		AssertEquals("Idoc elaboration date time", new ZDateTime(2019, 10, 03, 06, 47, 00), positiveMessage.IdocElaborationDateTime);

		AssertEquals("R", positiveMessage.RecordType);
		AssertEquals("ET", positiveMessage.MessageCode);
		AssertEquals("915956", positiveMessage.DeclarationNumber);
		AssertEquals(0, positiveMessage.MessageProgressiveNumber);
		AssertEquals("119101", positiveMessage.CustomsOffice);
		AssertEquals("P", positiveMessage.OperationResult);

		AssertEquals("1", positiveMessage.RegisterCode);
		AssertEquals("", positiveMessage.RegisterSeries);
		AssertEquals("00012466", positiveMessage.RegistrationNumber);
		AssertEquals("K", positiveMessage.RegistrationNumberCin);
		AssertEquals(new ZDate(2019, 10, 03), positiveMessage.RegistrationDate);
		AssertEquals("", positiveMessage.DebitAccountNumber);
		AssertEquals("", positiveMessage.DebitAccountNumberCin);
		AssertEquals("", positiveMessage.A93Number);
		AssertEquals("", positiveMessage.FirstPaymentMethod);
		AssertEquals(ZDate.Empty, positiveMessage.FirstPaymentDueDate);
		AssertEquals("", positiveMessage.SecondPaymentMethod);
		AssertEquals(ZDate.Empty, positiveMessage.SecondPaymentDueDate);
		AssertEquals("", positiveMessage.ThirdPaymentMethod);
		AssertEquals(ZDate.Empty, positiveMessage.ThirdPaymentDueDate);
		AssertEquals("", positiveMessage.ReleaseCode);
		AssertEquals("", positiveMessage.ReleaseNotes);
		AssertEquals("19ITQSV010012466E6", positiveMessage.MrnCode);
		AssertNull(positiveMessage.GuaranteeAmount);
		AssertEquals("", positiveMessage.Notes);
		AssertEquals("1--00012466-K-03/10/2019", positiveMessage.FullRegistrationInfo);
		AssertEquals("1 -12466K", positiveMessage.RegistrationInfo);
		Assert(!positiveMessage.IsCleared);
		Assert(positiveMessage.IsRegistered);
		Assert(!positiveMessage.IsUnderControl);
		Assert(!positiveMessage.HasA93FirstPayment);
		Assert(!positiveMessage.HasA93SecondPayment);
		Assert(!positiveMessage.HasA93ThirdPayment);
	}

	public void TestReadNegativeIrispET()
	{
		var parseResult = CustomsInterchange.LoadSafe<IrispTypeR>(@"6FQW            6FQW1003.X81190015531060137101    007746G         001 00006    INVIO IN AMBIENTE REALE       
 RICEVUTO  03/10/19 08:30,6FQW1003.R81
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   03/10/19  08:30
RET          60126100137101N                                        
 014003 00F 00 Errore 781 : [Scheda Partita non convalidata]          ");
		AssertNullOrEmpty(parseResult.ErrorText);
		Assert(parseResult.IsValid);
		var irispObject = parseResult.Interchange;
		AssertNotNull(irispObject);
		AssertNotNull(irispObject.Header);
		AssertEquals("6FQW            6FQW1003.X81190015531060137101    007746G         001 00006    INVIO IN AMBIENTE REALE       ", irispObject.Header.InnerText);
		AssertEquals("Header authorized user code", "6FQW", irispObject.Header.AuthorizedUserCode);
		AssertEquals("Header file name", "6FQW1003.X81", irispObject.Header.FileName);
		AssertEquals("Header customs office section code", "137101", irispObject.Header.CustomsOfficeSectionCode);
		AssertEquals("Header country code", "", irispObject.Header.CountryCode);
		AssertEquals("Header vat registration number", "007746G", irispObject.Header.TaxCodeOrVATRegistrationNumber);
		AssertEquals("Header progressive seat authorized account", 1, irispObject.Header.ProgressiveSeatAuthorizedAccount);
		AssertEquals("Header number of record in the file", 6, irispObject.Header.NumberOfRecordsInTheFile);
		AssertEquals("Header transmission environment", "INVIO IN AMBIENTE REALE", irispObject.Header.TransmissionEnvironment);
		Assert("Is production transmission environment", irispObject.Header.IsProductionTransmissionEnvironment);

		var idocElaborationInfo = irispObject.IdocElaborationInfo;
		AssertNotNull(idocElaborationInfo);
		AssertEquals(" RICEVUTO  03/10/19 08:30,6FQW1003.R81", idocElaborationInfo.InnerText);
		AssertEquals("Idoc date time of receipt", new ZDateTime(2019, 10, 03, 08, 30, 00), idocElaborationInfo.IdocDateTimeOfReceipt);
		AssertEquals("Idoc name", "6FQW1003.R81", idocElaborationInfo.IdocName);

		var controlResult = irispObject.ControlResult;
		AssertNotNull(controlResult);
		AssertEquals(" RL=002,RS=000,ME=001,MS=000,PE=001,PS=000", controlResult.InnerText);
		AssertEquals("Number of records read", 2, controlResult.NumberOfRecordsRead);
		AssertEquals("Number of records rejected", 0, controlResult.NumberOfRecordsRejected);
		AssertEquals("Number of messages read", 1, controlResult.NumberOfMessagesProcessed);
		AssertEquals("Number of messages rejected", 0, controlResult.NumberOfMessagesRejected);
		AssertEquals("Number of procedures read", 1, controlResult.NumberOfProceduresProcessed);
		AssertEquals("Number of procedures rejected", 0, controlResult.NumberOfProceduresRejected);

		Assert(!irispObject.IsPositive);
		Assert(!irispObject.IsPartialPositive);
		Assert(irispObject.IsNegative);

		var responseMessages = irispObject.ResponseMessages;
		AssertNotNull(responseMessages);
		AssertEquals("Response messages count", 1, responseMessages.Count);

		var positiveMessages = irispObject.GetSadPositiveResponseMessages();
		AssertNotNull(positiveMessages);
		AssertEquals("Positive response messages count", 0, positiveMessages.Count());

		var negativeMessages = irispObject.GetSadNegativeResponseMessages();
		AssertNotNull(negativeMessages);
		AssertEquals("Negative response messages count", 1, negativeMessages.Count());

		var negativeMessage = negativeMessages.ElementAt(0);
		AssertMultilineASCIIEquals(@"ESEGUITO   03/10/19  08:30
RET          60126100137101N                                        
 014003 00F 00 Errore 781 : [Scheda Partita non convalidata]          ", negativeMessage.InnerText);
		AssertEquals("Idoc elaboration date time", new ZDateTime(2019, 10, 03, 08, 30, 00), negativeMessage.IdocElaborationDateTime);

		AssertEquals("R", negativeMessage.RecordType);
		AssertEquals("ET", negativeMessage.MessageCode);
		AssertEquals("601261", negativeMessage.DeclarationNumber);
		AssertEquals(0, negativeMessage.MessageProgressiveNumber);
		AssertEquals("137101", negativeMessage.CustomsOffice);
		AssertEquals("N", negativeMessage.OperationResult);

		var irregularityRecord = negativeMessage.Irregularity;
		AssertNotNull(irregularityRecord);

		AssertEquals(1, irregularityRecord.ItemNumber);
		AssertEquals("4003", irregularityRecord.FieldIdentificative);
		AssertEquals(0, irregularityRecord.SequentialNumberOfFieldInRecord);
		AssertEquals("F", irregularityRecord.ErrorType);
		AssertEquals(0, irregularityRecord.ControlNumber);
		AssertEquals("Errore 781 : [Scheda Partita non convalidata]", irregularityRecord.ErrorDescription);
		AssertEquals("40.3", irregularityRecord.FieldSadBoxNumber);
	}

	#endregion

	public void TestReadWhenLastRowHasNotExpectedLength()
	{
		var parseResult = CustomsInterchange.LoadSafe<IrispTypeR>(@"004R            004R1004.X04190015588917275100    01790970139     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  04/10/19 06:04,004R1004.R04
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   04/10/19  06:04
RIM          21309000275100P4 T 00031664X041019014135A000273G081119F081119 000000X91B0ZSVINCOLATA");
		Assert(parseResult.IsValid);
		AssertNotNull(parseResult.Interchange);
	}
}
