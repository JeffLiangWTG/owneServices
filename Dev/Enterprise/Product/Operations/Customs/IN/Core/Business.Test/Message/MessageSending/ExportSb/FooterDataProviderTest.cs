using CargoWise.Customs.IN.MessageContracts.ExportSb;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class FooterDataProviderTest : ExportSbFooterDataProviderAbstractClassBase
{
	public override void TestSequenceOrControlNo()
	{
		AssertEquals(Constants.Messaging.INMessageNumPlaceHolder, CreateDataProvider().SequenceOrControlNo);
	}

	protected override FooterDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbCACHE01DataProvider.CreateProvider(header, new ExportSbCACHE01AdditionalDataProvider(messageSendingObject)).Footer;
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.CustomsEntryHeaders.Add(header);
	}

	JobDeclaration declaration;
}
