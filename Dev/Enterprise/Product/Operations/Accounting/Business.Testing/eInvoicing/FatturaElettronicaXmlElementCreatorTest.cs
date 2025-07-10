using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	public class FatturaElettronicaXmlElementCreatorTest : TestCaseWithFactory
	{
		public void TestCreateIndirizzoTypeWithNullAddress()
		{
			var methodInfo = typeof(FatturaElettronicaXmlElementCreator).GetMethod("CreateIndirizzoType", BindingFlags.Static | BindingFlags.Public);
			this.AssertXMLEqualsByDiff($@"<Sede>
  <Indirizzo />
  <NumeroCivico>22</NumeroCivico>
  <CAP>00144</CAP>
  <Comune>Roma</Comune>
  <Provincia>RM</Provincia>
  <Nazione>IT</Nazione>
</Sede>", InvokeCreateIndirizzoType(methodInfo, null, "22", "00144", "Roma", "RM", Core.Constants.CountryCodes.Italy).ToString());
		}

		public void TestCreateIndirizzoTypeWithEmptyAddress()
		{
			var methodInfo = typeof(FatturaElettronicaXmlElementCreator).GetMethod("CreateIndirizzoType", BindingFlags.Static | BindingFlags.Public);
			this.AssertXMLEqualsByDiff($@"<Sede>
  <Indirizzo />
  <NumeroCivico>22</NumeroCivico>
  <CAP>00144</CAP>
  <Comune>Roma</Comune>
  <Provincia>RM</Provincia>
  <Nazione>IT</Nazione>
</Sede>", InvokeCreateIndirizzoType(methodInfo, "", "22", "00144", "Roma", "RM", Core.Constants.CountryCodes.Italy).ToString());
		}

		public void TestCreateIndirizzoTypeWhenAddressLengthIsLessOrEqual60()
		{
			var methodInfo = typeof(FatturaElettronicaXmlElementCreator).GetMethod("CreateIndirizzoType", BindingFlags.Static | BindingFlags.Public);
			this.AssertXMLEqualsByDiff($@"<Sede>
  <Indirizzo>Viale Europa</Indirizzo>
  <NumeroCivico>22</NumeroCivico>
  <CAP>00144</CAP>
  <Comune>Roma</Comune>
  <Provincia>RM</Provincia>
  <Nazione>IT</Nazione>
</Sede>", InvokeCreateIndirizzoType(methodInfo, "Viale Europa", "22", "00144", "Roma", "RM", Core.Constants.CountryCodes.Italy).ToString());

			this.AssertXMLEqualsByDiff($@"<Sede>
  <Indirizzo>AddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddr</Indirizzo>
  <NumeroCivico>22</NumeroCivico>
  <CAP>00144</CAP>
  <Comune>Roma</Comune>
  <Provincia>RM</Provincia>
  <Nazione>IT</Nazione>
</Sede>", InvokeCreateIndirizzoType(methodInfo, "AddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddr", "22", "00144", "Roma", "RM", Core.Constants.CountryCodes.Italy).ToString());
		}

		public void TestCreateIndirizzoTypeWhenAddressLengthIsMorethan60()
		{
			var methodInfo = typeof(FatturaElettronicaXmlElementCreator).GetMethod("CreateIndirizzoType", BindingFlags.Static | BindingFlags.Public);
			this.AssertXMLEqualsByDiff($@"<Sede>
  <Indirizzo>AddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddr</Indirizzo>
  <NumeroCivico>22</NumeroCivico>
  <CAP>00144</CAP>
  <Comune>Roma</Comune>
  <Provincia>RM</Provincia>
  <Nazione>IT</Nazione>
</Sede>", InvokeCreateIndirizzoType(methodInfo, "AddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddr", "22", "00144", "Roma", "RM", Core.Constants.CountryCodes.Italy).ToString());
		}

		public void TestCreateIndirizzoTypeWhenAddressEndWithBlankSpaces()
		{
			var methodInfo = typeof(FatturaElettronicaXmlElementCreator).GetMethod("CreateIndirizzoType", BindingFlags.Static | BindingFlags.Public);
			this.AssertXMLEqualsByDiff("Test trim the space between <Address1 Address2>.", $@"<Sede>
  <Indirizzo>AddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAd1</Indirizzo>
  <NumeroCivico>22</NumeroCivico>
  <CAP>00144</CAP>
  <Comune>Roma</Comune>
  <Provincia>RM</Provincia>
  <Nazione>IT</Nazione>
</Sede>", InvokeCreateIndirizzoType(methodInfo, "AddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAddrAd1 Addr2", "22", "00144", "Roma", "RM", Core.Constants.CountryCodes.Italy).ToString());
		}

		public void TestCreateIndirizzoTypeWithSpecialCharacterInComune()
		{
			var methodInfo = typeof(FatturaElettronicaXmlElementCreator).GetMethod("CreateIndirizzoType", BindingFlags.Static | BindingFlags.Public);
			this.AssertXMLEqualsByDiff("Test removing special characters in comune.", $@"<Sede>
  <Indirizzo>Address</Indirizzo>
  <NumeroCivico>22</NumeroCivico>
  <CAP>00144</CAP>
  <Comune>Ro ma</Comune>
  <Provincia>RM</Provincia>
  <Nazione>IT</Nazione>
</Sede>", InvokeCreateIndirizzoType(methodInfo, "Address", "22", "00144", "Ro€ma", "RM", Core.Constants.CountryCodes.Italy).ToString());
		}

		public void TestCreateAnagraficaTypeWithSpecialCharacters()
		{
			var methodInfo = typeof(FatturaElettronicaXmlElementCreator).GetMethod("CreateAnagraficaType", BindingFlags.Static | BindingFlags.Public);
			this.AssertXMLEqualsByDiff("Test removing special characters from Anagrafica.", $@"<Anagrafica>
  <Denominazione> 10/hr</Denominazione>
  <Titolo>Title</Titolo>
  <CodEORI>Code</CodEORI>
</Anagrafica>", InvokeCreateAnagraficaType(methodInfo, "€10/hr", "€10/hr", "Title", "Code").ToString());
		}

		public void TestGetIndividualNames()
		{
			var methodInfo = typeof(FatturaElettronicaXmlElementCreator).GetMethod("GetIndividualNames", BindingFlags.Static | BindingFlags.NonPublic);
			AssertEquals(new Tuple<string, string>("Alice", "Wonderland"), InvokeGetIndividualNames(methodInfo, "Alice Wonderland"));
			AssertEquals(new Tuple<string, string>("Alice", "Wonderland"), InvokeGetIndividualNames(methodInfo, " Alice  Wonderland "));
			AssertEquals(new Tuple<string, string>("King", "Of Hearts"), InvokeGetIndividualNames(methodInfo, "King Of Hearts"));
			AssertEquals(new Tuple<string, string>("King", "Of  Hearts"), InvokeGetIndividualNames(methodInfo, " King  Of  Hearts "));
			AssertEquals(new Tuple<string, string>("Dodo", "Dodo"), InvokeGetIndividualNames(methodInfo, "Dodo"));
			AssertEquals(new Tuple<string, string>(string.Empty, string.Empty), InvokeGetIndividualNames(methodInfo, string.Empty));
			AssertEquals(new Tuple<string, string>(string.Empty, string.Empty), InvokeGetIndividualNames(methodInfo, null));
		}

		object InvokeGetIndividualNames(MethodInfo methodInfo, string value)
		{
			return methodInfo.Invoke(null, new object[] { new ZString?(value) });
		}

		public void TestCreateIndirizzoType_ProvinciaElement()
		{
			var methodInfo = typeof(FatturaElettronicaXmlElementCreator).GetMethod("CreateIndirizzoType", BindingFlags.Static | BindingFlags.Public);
			this.AssertXMLEqualsByDiff($@"<Sede>
  <Indirizzo>Viale Europa</Indirizzo>
  <NumeroCivico>22</NumeroCivico>
  <CAP>00144</CAP>
  <Comune>Roma</Comune>
  <Provincia>RM</Provincia>
  <Nazione>IT</Nazione>
</Sede>", InvokeCreateIndirizzoType(methodInfo, "Viale Europa", "22", "00144", "Roma", "RM", Core.Constants.CountryCodes.Italy).ToString());

			this.AssertXMLEqualsByDiff($@"<Sede>
  <Indirizzo>Viale Europa</Indirizzo>
  <NumeroCivico>22</NumeroCivico>
  <CAP>00144</CAP>
  <Comune>Roma</Comune>
  <Nazione>IT</Nazione>
</Sede>", InvokeCreateIndirizzoType(methodInfo, "Viale Europa", "22", "00144", "Roma", "Roma", Core.Constants.CountryCodes.Italy).ToString());

			this.AssertXMLEqualsByDiff($@"<Sede>
  <Indirizzo>Spring Street</Indirizzo>
  <NumeroCivico>123</NumeroCivico>
  <CAP>2015</CAP>
  <Comune>Alexandria</Comune>
  <Nazione>AU</Nazione>
</Sede>", InvokeCreateIndirizzoType(methodInfo, "Spring Street", "123", "2015", "Alexandria", "NSW", Core.Constants.CountryCodes.Australia).ToString());

			this.AssertXMLEqualsByDiff($@"<Sede>
  <Indirizzo>Main Street</Indirizzo>
  <NumeroCivico>200</NumeroCivico>
  <CAP>85123</CAP>
  <Comune>Phoenix</Comune>
  <Nazione>US</Nazione>
</Sede>", InvokeCreateIndirizzoType(methodInfo, "Main Street", "200", "85123", "Phoenix", "AZ", Core.Constants.CountryCodes.UnitedStates).ToString());

			this.AssertXMLEqualsByDiff($@"<Sede>
  <Indirizzo>Renmin Lu</Indirizzo>
  <NumeroCivico>63</NumeroCivico>
  <CAP>266033</CAP>
  <Comune>Quingdao Shi</Comune>
  <Nazione>CN</Nazione>
</Sede>", InvokeCreateIndirizzoType(methodInfo, "Renmin Lu", "63", "266033", "Quingdao Shi", "37", Core.Constants.CountryCodes.China).ToString());

			this.AssertXMLEqualsByDiff($@"<Sede>
  <Indirizzo>Sarmiento</Indirizzo>
  <NumeroCivico>151</NumeroCivico>
  <CAP>C1000ZAA</CAP>
  <Comune>Buenos Aires</Comune>
  <Nazione>AR</Nazione>
</Sede>", InvokeCreateIndirizzoType(methodInfo, "Sarmiento", "151", "C1000ZAA", "Buenos Aires", "B", Core.Constants.CountryCodes.Argentina).ToString());
		}

		object InvokeCreateIndirizzoType(MethodInfo methodInfo, string address, string number, string postCode, string city, string state, string country)
		{
			return methodInfo.Invoke(null, new object[] { address, number, postCode, city, state, country, "Sede" });
		}

		object InvokeCreateAnagraficaType(MethodInfo methodInfo, string companyName, string cognome, string title, string codEORI)
		{
			return methodInfo.Invoke(null, new object[] { companyName, false, title, codEORI, "Anagrafica" });
		}
	}
}
