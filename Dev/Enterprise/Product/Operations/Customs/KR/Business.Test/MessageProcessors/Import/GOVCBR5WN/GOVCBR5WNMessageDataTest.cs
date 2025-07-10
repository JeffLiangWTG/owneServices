using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GOVCBR5WNMessageData))]
	sealed class GOVCBR5WNMessageDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFormatted()
		{
			var message5WN = new GOVCBR5WNMessageData(Factory);
			message5WN.NoticeNumber = "030112100576919";

			AssertEquals("030-11-21-00576919", message5WN.FormattedNoticeNumber);
		}
	}
}
