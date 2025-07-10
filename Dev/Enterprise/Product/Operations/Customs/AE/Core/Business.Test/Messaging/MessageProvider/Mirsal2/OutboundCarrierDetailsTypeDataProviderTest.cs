using CargoWise.Customs.AE.MessageContracts.Mirsal2;
using CargoWise.Types;
using Enterprise.Customs.AE.Business.MessageSending.Mirsal2.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class OutboundCarrierDetailsTypeDataProviderTest : Mirsal2OutboundCarrierDetailsTypeDataProviderAbstractClassBase
{
	public override void TestCarrierNumber()
	{
		Assert("to do in future WI", true);
	}

	public override void TestCarrierRegistrationNo()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDateOfDeparture()
	{
		Declaration.JE_ExportDate = ZDateTime.Now;
		AssertEquals(Declaration.JE_ExportDate.ToDateTime(), CreateDataProvider().DateOfDeparture);
	}

	public override void TestTransportMode()
	{
		Assert("to do in future WI", true);
	}

	protected override OutboundCarrierDetailsTypeDataProviderAbstractClass CreateDataProvider()
	{
		return DeclarationRequestDataProvider.CreateProvider(header, additionalDataProvider).Declaration.OutboundCarrierDetails;
	}

	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		jobDeclaration.ActiveEntryHeaders.Add(header);
		var importer = Factory.New<OrgHeader>();
		jobDeclaration.JE_OH_Importer = importer.PK;
		return jobDeclaration;
	}
}
