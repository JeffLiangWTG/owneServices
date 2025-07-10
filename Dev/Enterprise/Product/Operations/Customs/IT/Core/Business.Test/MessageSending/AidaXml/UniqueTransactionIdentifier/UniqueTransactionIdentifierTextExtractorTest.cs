using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier.Testing;

sealed class UniqueTransactionIdentifierTextExtractorTest : TestCase
{
	public void TestExtractUniqueTransactionIdentifier_WhenContentIsEmpty()
	{
		AssertEquals("UniqueTransactionIdentifier", "", UniqueTransactionIdentifierTextExtractor.ExtractUniqueTransactionIdentifier(""));
	}

	public void TestExtractUniqueTransactionIdentifier_WhenContentDoesNotContainIUT()
	{
		AssertEquals("UniqueTransactionIdentifier", "", UniqueTransactionIdentifierTextExtractor.ExtractUniqueTransactionIdentifier("ABCDEFGHI"));
	}

	public void TestExtractUniqueTransactionIdentifier_WhenContentContainsIUTWithNamespacePrefix()
	{
		AssertEquals("UniqueTransactionIdentifier", "20220307D11000328189", UniqueTransactionIdentifierTextExtractor.ExtractUniqueTransactionIdentifier("<ns2:IUT>20220307D11000328189</ns2:IUT>"));
	}

	public void TestExtractUniqueTransactionIdentifier_WhenContentContainsIUTWithoutNamespacePrefix()
	{
		AssertEquals("UniqueTransactionIdentifier", "20220307D11000328189", UniqueTransactionIdentifierTextExtractor.ExtractUniqueTransactionIdentifier("<IUT>20220307D11000328189</IUT>"));
	}

	public void TestExtractUniqueTransactionIdentifier_WhenContentContainsIUTLowercase()
	{
		AssertEquals("UniqueTransactionIdentifier", "20220307D11000328189", UniqueTransactionIdentifierTextExtractor.ExtractUniqueTransactionIdentifier("<iut>20220307D11000328189</iut>"));
	}
}
