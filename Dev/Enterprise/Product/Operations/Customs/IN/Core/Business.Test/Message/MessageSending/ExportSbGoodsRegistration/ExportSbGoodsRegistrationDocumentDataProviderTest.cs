using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSbGoodsRegistration;
using Enterprise.Customs.IN.Business.MessageSending.ExportSb;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSbGoodsRegistration.Testing;

[TestedType(typeof(ExportSbGoodsRegistrationCACHE05DataProvider))]
sealed class ExportSbGoodsRegistrationDocumentDataProviderTest : ExportSbGoodsRegistrationDocumentDataProviderAbstractClassBase
{
	public override void TestCustomHouseCode()
	{
		AssertEquals("TBA", CreateDataProvider().CustomHouseCode);
	}

	public override void TestDocumentName()
	{
		AssertEquals("TBA", CreateDataProvider().DocumentName);
	}

	public override void TestDocumentSrNo()
	{
		AssertEquals("TBA", CreateDataProvider().DocumentSrNo);
	}

	public override void TestMessageType()
	{
		AssertEquals("TBA", CreateDataProvider().MessageType);
	}

	public override void TestSbDate()
	{
		AssertEquals("TBA", CreateDataProvider().SbDate);
	}

	public override void TestSbNo()
	{
		AssertEquals("TBA", CreateDataProvider().SbNo);
	}

	protected override DocumentDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbGoodsRegistrationCACHE05DataProvider.CreateProvider(messageSendingObject).Goodsregistration.Document.First();
	}
}
