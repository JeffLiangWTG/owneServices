using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GOVCBR5GVMessageData))]
	sealed class GOVCBR5GVMessageDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSupplementationType()
		{
			var messageData = new GOVCBR5GVMessageData(Factory);
			messageData.ComplementReasonCode = "2";
			AssertEquals("보완사항 통보", messageData.SupplementationType);
			messageData.ComplementReasonCode = "5";
			AssertEquals("보완사항 안내", messageData.SupplementationType);
		}
		public void TestSupplementaryCodeDescription()
		{
			var messageData = new GOVCBR5GVMessageData(Factory);
			messageData.ComplementReasonCode = "2";
			AssertEquals("보완요구사항(일부)", messageData.SupplementaryCodeDescription);
			messageData.ComplementReasonCode = "3";
			AssertEquals("보완요구사항(P/L → 서류변경)", messageData.SupplementaryCodeDescription);
			messageData.ComplementReasonCode = "4";
			AssertEquals("보완요구사항(서류변경 → P/L)", messageData.SupplementaryCodeDescription);
			messageData.ComplementReasonCode = "5";
			AssertEquals("보완안내", messageData.SupplementaryCodeDescription);
		}
	}
}
