using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.Ccsuk.Connection.Testing
{
	class CcsukEdiMessageDiverterTest : TestCaseWithFactory
	{
		public void TestMessageDiversion()
		{
			string restOfHeaderAndBodyAndFooter = ":IATA+CUKFFW98000CAR:IATA+100617:1159+101A3115957000+++A'UNH+09612467967794+CUSRES:D:04A:UN:109700+<<SYSCAR>>'BGM+IFD::109++29'DTM+7:201006171259:203'LOC+14+GBLHR::109'LOC+22+120::109'LOC+44+120::109'RFF+ABT:001815C:1'RFF+ABO:8-B00001000:J'RFF+ABS:A1'RFF+AHZ::H'DOC+960+B00010212'MOA+40:0.00'MOA+55:0.00'MOA+1:0.00'MOA+150:0.00'MOA+9:0.00'MOA+74:0.00'MOA+176:0.00'CST+1'TAX+5'MOA+40:0.00'MOA+55:0.00'MOA+1:0.00'MOA+150:0.00'MOA+38:0.00'UNT+26+09612467967794'UNZ+1+101A3115957000'";
			string cUKCTM98CHFIMP = "UNB+UNOA:2+CUKCTM98CHFIMP" + restOfHeaderAndBodyAndFooter;
			string cUKCTM98CHFEXP = "UNB+UNOA:2+CUKCTM98CHFEXP" + restOfHeaderAndBodyAndFooter;
			string cUKCTM98PRT001 = "UNB+UNOA:2+CUKCTM98PRT001" + restOfHeaderAndBodyAndFooter;
			string cUKCTM98000PRT = "UNB+UNOA:2+CUKCTM98000PRT" + restOfHeaderAndBodyAndFooter;
			string defaultSender = "UNB+UNOA:2+DEFAULTXXXXXXX" + restOfHeaderAndBodyAndFooter;
			string genralMessage = "UNB+UNOA:2+SOMEONE" + restOfHeaderAndBodyAndFooter.Replace("BGM+IFD", "BGM+GENRAL");
			string cukNesSender = "UNB+UNOA:2+CUKSYS98CCSNES" + restOfHeaderAndBodyAndFooter;
			string exportFsn = "UNB+UNOA:2+CUKSYS98CCSNES" + restOfHeaderAndBodyAndFooter.Replace("BGM+IFD", "BGM+CIMFSN");
			string contrlFromNes = "UNB+UNOA:1+CUKSYS98CCSNES:IATA+CUKFFW98000CAR:IATA+121221:1038+002A2103830002+++A'UNH+20121221103830+CONTRL:1:912:UN+3DDB76B56D46490EB7851F8BFFB9922E'UCM+13255+CUKG2G:A:04A:BT'UCX+4'UCR+1'UCD+2:1+10'FTX+AAO+++G2G RECEIVED WHEN NOT IN FALLBACK'UNT+7+20121221103830'UNZ+1+101A3115957000'";

			Dictionary<string, string> pairsToTest = new Dictionary<string, string>();
			pairsToTest.Add(cUKCTM98CHFIMP, ApplicationCodeList.Codes.GbEdifactShared);
			pairsToTest.Add(cUKCTM98CHFEXP, ApplicationCodeList.Codes.GbEdifactShared);
			pairsToTest.Add(cUKCTM98PRT001, ApplicationCodeList.Codes.GbEdifactShared);
			pairsToTest.Add(cUKCTM98000PRT, ApplicationCodeList.Codes.GbEdifactShared);
			pairsToTest.Add(cukNesSender, ApplicationCodeList.Codes.GbEdifactShared);
			pairsToTest.Add(genralMessage, ApplicationCodeList.Codes.GbCcsuk);
			pairsToTest.Add(defaultSender, ApplicationCodeList.Codes.GbCcsuk);
			pairsToTest.Add(exportFsn, ApplicationCodeList.Codes.GbCcsuk);
			pairsToTest.Add(contrlFromNes, ApplicationCodeList.Codes.GbCcsuk);

			foreach (string key in pairsToTest.Keys)
			{
				EDIInterchange interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, key, ApplicationCodeList.Codes.GbCcsuk);
				AssertEquals(pairsToTest[key], CcsukEdiMessageDiverter.GetTargetApplicationCodeBasedOnInterchangeParties(interchange));
			}
		}
	}
}
