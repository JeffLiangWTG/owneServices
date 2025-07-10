using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDeparturePayInfo))]
	class NctsDeparturePayInfoTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			AssertType<NctsDeparturePayInfoTypeDecider>("TypeDecider Type", NctsDeparturePayInfo.TypeDecider);
		}

		public void TestMoveHeader()
		{
			var moveHeader = Factory.New<NctsDepartureMovementHeader>();
			var payInfo = moveHeader.PayInfoCollection.AddNew();
			AssertSame("Parent MoveHeader", moveHeader, payInfo.MoveHeader);
		}
	}
}
