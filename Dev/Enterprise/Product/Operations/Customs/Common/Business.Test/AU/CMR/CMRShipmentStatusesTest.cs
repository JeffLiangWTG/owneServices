using NUnit.Framework;
namespace Enterprise.Customs.Common.AU.CMR.Testing
{
	class CMRShipmentStatusesTest : CMRStatusesTest
	{
		[ExpectNoExceptions]
		public void TestCMRShipmentStatuses()
		{
			NUnit.Framework.Assert.That(new CMRShipmentStatuses().GetDescriptionFromCode(CMRBaseStatuses.Codes.NotSent), Is.EqualTo("No customs status received"));
		}
	}
}
