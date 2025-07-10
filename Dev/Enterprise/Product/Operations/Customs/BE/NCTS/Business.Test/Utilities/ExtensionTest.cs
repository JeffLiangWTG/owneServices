using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class ExtensionTest : TestCaseWithFactory
{
	public void TestGetValueFromAdditionalText()
	{
		var nctsDepartureMovementHeader = Factory.New<NctsDepartureMovementHeader>();
		nctsDepartureMovementHeader.BM_AdditionalText = "K=V*InvalidationJustification=InvalidationJustificationText*O=V";
		AssertEquals("InvalidationJustificationText", nctsDepartureMovementHeader.GetInvalidationJustification());
	}

	public void TestGetInvalidationReason()
	{
		var nctsDepartureMovementHeader = Factory.New<NctsDepartureMovementHeader>();
		nctsDepartureMovementHeader.BM_AdditionalText = "K=V*InvalidationJustification=InvalidationJustificationText*O=V";

		AssertEquals("InvalidationJustificationText", nctsDepartureMovementHeader.GetValueFromAdditonalText("InvalidationJustification"));
		AssertEquals(ZString.Empty, nctsDepartureMovementHeader.GetValueFromAdditonalText("NonExistingKey"));
		AssertEquals(ZString.Empty, nctsDepartureMovementHeader.GetValueFromAdditonalText("Invalidation"));
	}

	public void TestGetEORI()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var orgCusCode = orgHeader.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		orgCusCode.OK_CustomsRegNo = "PartyID";

		AssertEquals("PartyID", orgHeader.GetEORI());
	}

	public void TestGetEORI_Null()
	{
		AssertEquals(ZString.Empty, ((OrgHeader)null).GetEORI());
	}

	public void TestGetCBR()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var orgCusCode = orgHeader.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BrokerageRegistration;
		orgCusCode.OK_CustomsRegNo = "CBR123";

		AssertEquals("CBR123", orgHeader.GetCBR());
	}

	public void TestGetCBR_Null()
	{
		AssertEquals(ZString.Empty, ((OrgHeader)null).GetCBR());
	}
}
