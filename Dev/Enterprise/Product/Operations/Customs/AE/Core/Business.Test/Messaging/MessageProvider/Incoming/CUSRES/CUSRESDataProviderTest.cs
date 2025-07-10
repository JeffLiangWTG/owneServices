using System;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CUSRESDataProviderTest : Customs.Business.Testing.DataProviderTestCase<CUSRESDataProvider, ICUSRESDataProvider>
{
	[ExpectNoExceptions]
	public void TestConstructor()
	{
		NUnit.Framework.Assert.Throws<ArgumentException>(() => new CUSRESDataProvider(ZString.Empty));
	}

	public void TestOutgoingAccessReference() => AssertEquals(ExpectedDocumentIdentifier, Provider.OutgoingAccessReference);

	public void TestEntryStatus() => AssertEquals(ExpectedEntryStatus, Provider.EntryStatus);

	public void TestDocumentIdentifier() => AssertEquals(ExpectedDocumentIdentifier, Provider.DocumentIdentifier);

	public void TestInformationRequests() => AssertType<InformationRequestProvider>(Provider.InformationRequests.Single());

	public void TestIsParsed() => CombineAssertions(() =>
	{
		Assert(Provider.IsParsed);

		var messageTextWithParseError = "UNH+090530H6666666+CUSRES:D:23A:UX'" +
			$"GEI++{ExpectedEntryStatus}'" +
			$"RFF+DM:{ExpectedDocumentIdentifier}::1'" +
			"ERP+2'" +
			"FTX++++Text'" +
			"UNT+6+090530H6666666'";
		var providerWithParseError = new CUSRESDataProvider(messageTextWithParseError);
		Assert(!providerWithParseError.IsParsed);
	});

	protected override CUSRESDataProvider GetProvider()
	{
		var messageText = "UNH+090530H6666666+CUSRES:D:23A:UN'" +
			$"GEI++{ExpectedEntryStatus}'" +
			$"RFF+DM:{ExpectedDocumentIdentifier}::1'" +
			"ERP+2'" +
			"FTX++++Text'" +
			"UNT+6+090530H6666666'";
		return new CUSRESDataProvider(messageText);
	}

	const string ExpectedDocumentIdentifier = "CUSCAR-DOC-ID";
	const string ExpectedEntryStatus = "ENT";
}
