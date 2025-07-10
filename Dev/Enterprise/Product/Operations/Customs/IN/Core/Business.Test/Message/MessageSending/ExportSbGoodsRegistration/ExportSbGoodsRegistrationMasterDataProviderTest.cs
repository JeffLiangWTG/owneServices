using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSbGoodsRegistration;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business.MessageSending.ExportSb;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSbGoodsRegistration.Testing;

[TestedType(typeof(ExportSbGoodsRegistrationCACHE05DataProvider))]
sealed class ExportSbGoodsRegistrationMasterDataProviderTest : ExportSbGoodsRegistrationMasterDataProviderAbstractClassBase
{
	public override void TestCustomHouseCode()
	{
		AssertEquals("TBA", CreateDataProvider().CustomHouseCode);
	}

	public override void TestGatewayPort()
	{
		AssertEquals("TBA", CreateDataProvider().GatewayPort);
	}

	public override void TestGrossWeight()
	{
		AssertEquals("TBA", CreateDataProvider().GrossWeight);
	}

	public override void TestHawbDate()
	{
		AssertEquals("TBA", CreateDataProvider().HawbDate);
	}

	public override void TestHawbNumber()
	{
		AssertEquals("TBA", CreateDataProvider().HawbNumber);
	}

	public override void TestMarksNumbers()
	{
		AssertEquals("TBA", CreateDataProvider().MarksNumbers);
	}

	public override void TestMawbDate()
	{
		AssertEquals("TBA", CreateDataProvider().MawbDate);
	}

	public override void TestMawbNumber()
	{
		AssertEquals("TBA", CreateDataProvider().MawbNumber);
	}

	public override void TestMessageType()
	{
		AssertEquals("TBA", CreateDataProvider().MessageType);
	}

	public override void TestNatureOfCargo()
	{
		AssertEquals("TBA", CreateDataProvider().NatureOfCargo);
	}

	public override void TestNetWeight()
	{
		AssertEquals("TBA", CreateDataProvider().NetWeight);
	}

	public override void TestNumberOfContainers()
	{
		AssertEquals("TBA", CreateDataProvider().NumberOfContainers);
	}

	public override void TestNumberOfLoosePackets()
	{
		AssertEquals("TBA", CreateDataProvider().NumberOfLoosePackets);
	}

	public override void TestSbDate()
	{
		AssertEquals("TBA", CreateDataProvider().SbDate);
	}

	public override void TestSbNo()
	{
		AssertEquals("TBA", CreateDataProvider().SbNo);
	}

	public override void TestSealType()
	{
		AssertEquals("TBA", CreateDataProvider().SealType);
	}

	public override void TestTotalNumberOfPackages()
	{
		AssertEquals("TBA", CreateDataProvider().TotalNumberOfPackages);
	}

	public override void TestTranshipperCode()
	{
		var orgHeader = Factory.New<OrgHeader>();
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		orgHeader.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.PAN, "123", Core.Constants.CountryCodes.India);
		var transhipper = Declaration.TranshipperDocAddress;
		transhipper.OrganisationPK = orgHeader.PK;
		AssertEquals("123", CreateDataProvider().TranshipperCode);
	}

	public override void TestUnitofMeasurement()
	{
		AssertEquals("TBA", CreateDataProvider().UnitofMeasurement);
	}

	public override void TestWarehouseCode()
	{
		AssertEquals("TBA", CreateDataProvider().WarehouseCode);
	}

	protected override MasterDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbGoodsRegistrationCACHE05DataProvider.CreateProvider(MessageSendingObject).Goodsregistration.Master.First();
	}

	DeclarationMessageSendingObject MessageSendingObject => messageSendingObject ??= new DeclarationMessageSendingObject(Header);

	CusEntryHeader Header => header ??= Factory.New<CusEntryHeader>();

	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		Header.CH_JE = jobDeclaration.PK;
		return jobDeclaration;
	}
}
