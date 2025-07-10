using NUnit.Framework;

namespace Enterprise.Client.OSP.Data_Import.Testing
{
	public class IFTMIN5MOrgAndAddressDetailsDataRowTest : TestCase
	{
		public void TestProperties()
		{
			string testString = "CZ Vat CODE        VELVETEX SPA                       VIA SCHIO - MACROLOTTO             PRATO                    FG59100   IT    002607";
			IFTMIN5MOrgAndAddressDetailsDataRow dataRow = new IFTMIN5MOrgAndAddressDetailsDataRow(testString);
			AssertEquals("Vat CODE", dataRow.VATCode);
			AssertEquals("VELVETEX SPA", dataRow.Name);
			AssertEquals("VIA SCHIO - MACROLOTTO", dataRow.Address);
			AssertEquals("PRATO", dataRow.City);
			AssertEquals("FG", dataRow.Province);
			AssertEquals("59100", dataRow.ZIPCode);
			AssertEquals("IT", dataRow.CountryCode);
			AssertEquals("002607", dataRow.CustomerCode);
		}
	}
}
