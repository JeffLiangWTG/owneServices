using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

[TestedType(typeof(DepartureMovementHeaderDocManagerInfo))]
class DepartureMovementHeaderDocManagerInfoTest : DocManagerInfoTestCase
{
	public void TestDocType()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);

		var info = header.MovementHeader.DocManagerInfo;

		AssertEquals(Core.Constants.DocManagerCodes.NctsMoveHeader, info.DocManagerCode);
	}

	public override BusinessObject GetEmptyParentBusinessObject()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		return header.MovementHeader;
	}

	public override BusinessObject GetPopulatedParentBusinessObject() => GetEmptyParentBusinessObject();
}
