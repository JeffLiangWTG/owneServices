using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class ExportSbTableRotationDataProviderTest : ExportSbTableRotationDataProviderAbstractClassBase
{
	public override void TestCustomHouseCode()
	{
		Declaration.JE_CustomsOffice = "INBLR";
		AssertEquals("INBLR", CreateDataProvider().CustomHouseCode);
	}

	public override void TestJobDate()
	{
		header.CH_SystemCreateTimeUtc = new ZDateTime(2024, 6, 13);
		AssertEquals(new ZDateTime(2024, 6, 13).ToLocalBranchTime().ToDateTime(), CreateDataProvider().JobDate);
	}

	public override void TestJobNumber()
	{
		header.CH_BGMReference = "1234";
		AssertEquals("1234", CreateDataProvider().JobNumber);
	}

	public override void TestMessageType()
	{
		messageSendingObject.MessageType = DeclarationMessageTypeList.Codes.Fresh;
		AssertEquals(DeclarationMessageTypeList.Codes.Fresh, CreateDataProvider().MessageType);
	}

	public override void TestRotationDate()
	{
		AssertNull(CreateDataProvider().RotationDate);

		Declaration.JE_RotationDate = new ZDateTime(2025, 2, 08);
		AssertEquals(new ZDateTime(2025, 2, 08), CreateDataProvider().RotationDate);
	}

	public override void TestRotationNumber()
	{
		Declaration.JE_RotationNumber = "1234567";
		AssertEquals("1234567", CreateDataProvider().RotationNumber);
	}

	protected override TableRotationDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbCACHE01DataProvider.CreateProvider(header, AdditionalDataProvider).Sb.TableRotation.First();
	}

	ExportSbCACHE01AdditionalDataProvider AdditionalDataProvider => new ExportSbCACHE01AdditionalDataProvider(messageSendingObject);

	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		header.CH_JE = jobDeclaration.PK;
		return jobDeclaration;
	}
}
