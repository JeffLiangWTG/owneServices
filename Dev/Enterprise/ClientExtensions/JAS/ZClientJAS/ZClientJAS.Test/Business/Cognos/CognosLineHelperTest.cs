using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class CognosLineHelperTest : TestCase
	{
		public void TestConvertToString()
		{
			CognosLine line = new CognosLine();
			line.AccountCode = "CODE";
			line.AccountName = "NAME";
			line.CounterCompany = "SOMECOMPANY";
			line.Mode = "MI";
			line.Branch = "SYD";
			line.Business = "COM";
			line.Amount = 293m;
			line.TransactionCurrency = "USD";
			line.TransactionAmount = 23.234m;
			line.Geographical = "AFR";
			CognosLineHelper helper = new CognosLineHelper();
			AssertEquals("NAME,CODE,SOMECOMPANY,MI,SYD,COM,293.00+,USD,23.23+,AFR", helper.ConvertToString(line));
			line.TransactionAmount = 0;
			AssertEquals("NAME,CODE,SOMECOMPANY,MI,SYD,COM,293.00+,USD,,AFR", helper.ConvertToString(line));
			line.Amount = -828390.07m;
			line.TransactionAmount = -23.4m;
			AssertEquals("NAME,CODE,SOMECOMPANY,MI,SYD,COM,828390.07-,USD,23.40-,AFR", helper.ConvertToString(line));
		}
	}
}
