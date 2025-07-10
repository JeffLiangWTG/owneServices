using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Messaging.MessageStructure.IVISTO;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IvistoTest : TestCase
{
	public void TestReadEmptyContent()
	{
		var parseResult = CustomsInterchange.LoadSafe<Ivisto>("");
		AssertNotNullOrEmpty(parseResult.ErrorText);
		Assert(!parseResult.IsValid);
		var ivistoObject = parseResult.Interchange;
		AssertNull(ivistoObject);
	}

	public void TestReadMalformedContent()
	{
		var content = @"0T60            0T600115.Q92180000621979025100    006738F         046 00003    INVIO IN AMBIENTE REALE       
Data:15/01/2018  Ora:XX:03:00
TIVISTO  18ITQTC010000011E5IT025100IT068100LA SPEZIA                          15012018Uscita conclusa                         ";
		var parseResult = CustomsInterchange.LoadSafe<Ivisto>(content);
		AssertNotNullOrEmpty(parseResult.ErrorText);
		Assert(!parseResult.IsValid);
		var ivistoObject = parseResult.Interchange;
		AssertNull(ivistoObject);
	}

	public void TestReadValidContent()
	{
		var content = @"0T60            0T600115.Q92180000621979025100    006738F         046 00004    INVIO IN AMBIENTE REALE       
Data:15/01/2018  Ora:14:03:13
TIVISTO  18ITQTC010000011E5IT025100IT068100LA SPEZIA                          15012018Uscita conclusa                         
TIVISTO  19ITQV6010020145E8IT221100IT221100RAVENNA                            08072019Uscita conclusa                         ";
		var parseResult = CustomsInterchange.LoadSafe<Ivisto>(content);
		AssertNullOrEmpty(parseResult.ErrorText);
		Assert(parseResult.IsValid);
		var ivistoObject = parseResult.Interchange;
		AssertNotNull(ivistoObject);
		AssertNotNull(ivistoObject.Header);
		AssertEquals("0T60", ivistoObject.Header.AuthorizedUserCode);
		AssertEquals("0T600115.Q92", ivistoObject.Header.FileName);
		AssertEquals("025100", ivistoObject.Header.CustomsOfficeSectionCode);
		AssertEquals("", ivistoObject.Header.CountryCode);
		AssertEquals("006738F", ivistoObject.Header.TaxCodeOrVATRegistrationNumber);
		AssertEquals(46, ivistoObject.Header.ProgressiveSeatAuthorizedAccount);
		AssertEquals(4, ivistoObject.Header.NumberOfRecordsInTheFile);
		AssertEquals("INVIO IN AMBIENTE REALE", ivistoObject.Header.TransmissionEnvironment);
		Assert(ivistoObject.Header.IsProductionTransmissionEnvironment);

		AssertEquals(new ZDateTime(2018, 01, 15, 14, 03, 13), ivistoObject.ElaborationDateTime);

		AssertNotNull(ivistoObject.ApplicationResponses);
		AssertEquals(2, ivistoObject.ApplicationResponses.Count);

		var applicationResponse1 = ivistoObject.ApplicationResponses[0];
		AssertEquals("T", applicationResponse1.RecordType);
		AssertEquals("IVISTO", applicationResponse1.MessageCode);
		AssertEquals("18ITQTC010000011E5", applicationResponse1.Mrn);
		AssertEquals("IT025100", applicationResponse1.ExportCustomsOffice);
		AssertEquals("IT068100", applicationResponse1.EffectiveExitCustomsOffice);
		AssertEquals("LA SPEZIA", applicationResponse1.EffectiveExitCustomsOfficeName);
		AssertEquals(new ZDate(2018, 01, 15), applicationResponse1.ExitOrRejectedExitDate);
		AssertEquals("Uscita conclusa", applicationResponse1.ExitCustomsOfficeResult);

		var applicationResponse2 = ivistoObject.ApplicationResponses[1];
		AssertEquals("T", applicationResponse2.RecordType);
		AssertEquals("IVISTO", applicationResponse2.MessageCode);
		AssertEquals("19ITQV6010020145E8", applicationResponse2.Mrn);
		AssertEquals("IT221100", applicationResponse2.ExportCustomsOffice);
		AssertEquals("IT221100", applicationResponse2.EffectiveExitCustomsOffice);
		AssertEquals("RAVENNA", applicationResponse2.EffectiveExitCustomsOfficeName);
		AssertEquals(new ZDate(2019, 07, 08), applicationResponse2.ExitOrRejectedExitDate);
		AssertEquals("Uscita conclusa", applicationResponse2.ExitCustomsOfficeResult);
	}
}
