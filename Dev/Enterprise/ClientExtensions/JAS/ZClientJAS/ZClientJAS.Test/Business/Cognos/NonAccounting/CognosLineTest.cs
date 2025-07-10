using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class CognosLineTest : TestCase
	{
		public void TestProperties()
		{
			CognosLine line = new CognosLine();
			line.AccountName = "NAME";
			line.AccountCode = "CODE";
			line.CounterCompany = "COMP";
			line.Mode = "MODE";
			line.Branch = "BRANCH";
			line.Business = "BUSINESS";
			line.Amount = 34m;
			line.TransactionCurrency = "CURRENCY";
			line.TransactionAmount = 77m;
			line.Geographical = "GEOGRAPHICAL";
			ICognosLine lineAsInterface = line;
			AssertEquals("NAME", lineAsInterface.AccountName);
			AssertEquals("CODE", lineAsInterface.AccountCode);
			AssertEquals("COMP", lineAsInterface.CounterCompany);
			AssertEquals("MODE", lineAsInterface.Mode);
			AssertEquals("BRANCH", lineAsInterface.Branch);
			AssertEquals("BUSINESS", lineAsInterface.Business);
			AssertEquals(34m, lineAsInterface.Amount);
			AssertEquals("CURRENCY", lineAsInterface.TransactionCurrency);
			AssertEquals(77m, lineAsInterface.TransactionAmount);
			AssertEquals("GEOGRAPHICAL", lineAsInterface.Geographical);
		}
	}
}
