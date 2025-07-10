using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class ExportSbTableStuffDataProvidertest : ExportSbTableStuffDataProviderAbstractClassBase
{
	public override void TestCustomHouseCode()
	{
		Declaration.JE_CustomsOffice = "INBLR";
		AssertEquals("INBLR", CreateDataProvider().CustomHouseCode);
	}

	public override void TestFactoryStuffed()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
		Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
		AssertEquals("Y", CreateDataProvider().FactoryStuffed);
	}

	public override void TestJobDate()
	{
		AssertNull(CreateDataProvider().JobDate);

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

	public override void TestSampleAccompanied()
	{
		Declaration.JE_StuffingAt = StuffingAtList.Codes.CFS;
		AssertEquals(ZString.Empty, CreateDataProvider().SampleAccompanied);

		Declaration.JE_SampleAccompanied = YesNoList.Codes.Yes;
		AssertEquals("Y", CreateDataProvider().SampleAccompanied);
	}

	protected override TableStuffDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbCACHE01DataProvider.CreateProvider(header, AdditionalDataProvider).Sb.TableStuff.First();
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
