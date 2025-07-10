using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using Enterprise.Customs.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class ExportSbTableContainerbackDataProviderTest : ExportSbTableContainerbackDataProviderAbstractClassBase
{
	public override void TestAmendmentDate()
	{
		Assert("to do in future WI", true);
	}

	public override void TestAmendmentNo()
	{
		Assert("to do in future WI", true);
	}

	public override void TestAmendmentType()
	{
		Assert("to do in future WI", true);
	}

	public override void TestContainerNo()
	{
		Assert("to do in future WI", true);
	}

	public override void TestContainerSize()
	{
		Assert("to do in future WI", true);
	}

	public override void TestCustomHouseCode()
	{
		Declaration.JE_CustomsOffice = "INBLR";
		AssertEquals("INBLR", CreateDataProvider().CustomHouseCode);
	}

	public override void TestMessageType()
	{
		Assert("to do in future WI", true);
	}

	public override void TestSbDate()
	{
		Assert("to do in future WI", true);
	}

	public override void TestSbNo()
	{
		Assert("to do in future WI", true);
	}

	protected override TableContainerbackDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbCACHE01DataProvider.CreateProvider(header, AdditionalDataProviderMock.Object).Sb.TableContainerback.First();
	}

	Mock<IExportSbCACHE01AdditionalDataProvider> AdditionalDataProviderMock => additionalDataProviderMock ??= new Mock<IExportSbCACHE01AdditionalDataProvider>();
	Mock<IExportSbCACHE01AdditionalDataProvider> additionalDataProviderMock;

	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		header.CH_JE = jobDeclaration.PK;
		return jobDeclaration;
	}
}
