using CargoWise.Customs.IN.MessageContracts;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.SeaCgm.Testing;

[TestedType(typeof(SeaCgmCMCHI21DataProvider))]
sealed class FooterDataProviderTest : SeaCgmIFooterDataProviderBase
{
	public override void TestTrec()
	{
		AssertEquals("TREC", CreateDataProvider().Trec);
	}

	public override void TestSequenceOrControlNo()
	{
		AssertEquals(Constants.Messaging.INMessageNumPlaceHolder, CreateDataProvider().SequenceOrControlNo);
	}

	protected override IFooterDataProvider CreateDataProvider()
		=> SeaCgmCMCHI21DataProvider.CreateProvider(header, Mock.Of<ISeaCgmCMCHI21AdditionalDataProvider>()).Footer;
}
