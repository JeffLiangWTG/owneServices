using CargoWise.Customs.AE.MessageContracts.Mirsal2;
using CargoWise.Types;
using Enterprise.Customs.AE.Business.MessageSending.Mirsal2.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class InboundCarrierDetailsTypeDataProviderTest : Mirsal2InboundCarrierDetailsTypeDataProviderAbstractClassBase
{
	public override void TestCarrierNumber()
	{
		Assert("to do in future WI", true);
	}

	public override void TestCarrierRegistrationNo()
	{
		Declaration.JE_VoyageFlightNo = "TEST";
		AssertEquals(Declaration.JE_VoyageFlightNo, CreateDataProvider().CarrierRegistrationNo);
	}

	public override void TestDateOfArrival()
	{
		Declaration.JE_DateOfArrival = ZDateTime.Now;
		AssertEquals(Declaration.JE_DateOfArrival.ToDateTime(), CreateDataProvider().DateOfArrival);
	}

	public override void TestTransportMode()
	{
		Declaration.JE_TransportMode = "1";
		AssertEquals((decimal)1, CreateDataProvider().TransportMode);
	}

	protected override InboundCarrierDetailsTypeDataProviderAbstractClass CreateDataProvider()
	{
		return DeclarationRequestDataProvider.CreateProvider(header, additionalDataProvider).Declaration.InboundCarrierDetails;
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
