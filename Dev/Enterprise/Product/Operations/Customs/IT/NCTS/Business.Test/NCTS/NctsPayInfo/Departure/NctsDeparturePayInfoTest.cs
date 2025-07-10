using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsDeparturePayInfo))]
sealed class NctsDeparturePayInfoTest : EnterpriseBusinessObjectTestCase
{
	public void TestMoveHeaderType()
	{
		var moveHeader = Factory.New<NctsDepartureMovementHeader>();
		var payInfo = moveHeader.PayInfoCollection.AddNew();
		AssertType<NctsDepartureMovementHeader>("MoveHeader Type", payInfo.MoveHeader);
	}
}
