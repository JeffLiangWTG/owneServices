using CargoWise.Customs.IN.MessageContracts;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.AirCgm.Testing;

[TestedType(typeof(AirCgmCMCHI01DataProvider))]
sealed class FooterDataProviderTest : AirCgmIFooterDataProviderBase
{
	public override void TestSequenceOrControlNo()
	{
		AssertEquals(Constants.Messaging.INMessageNumPlaceHolder, CreateDataProvider().SequenceOrControlNo);
	}

	public override void TestTrec()
	{
		AssertEquals("TREC", CreateDataProvider().Trec);
	}

	protected override IFooterDataProvider CreateDataProvider()
		=> AirCgmCMCHI01DataProvider.CreateProvider(header, Mock.Of<IAirCgmCMCHI01AdditionalDataProvider>()).Footer;
}
