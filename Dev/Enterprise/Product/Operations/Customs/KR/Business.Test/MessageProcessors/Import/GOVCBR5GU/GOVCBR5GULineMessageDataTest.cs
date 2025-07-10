using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GOVCBR5GULineMessageData))]
	sealed class GOVCBR5GULineMessageDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDataItemIDDescription()
		{
			var lineData = new GOVCBR5GULineMessageData(Factory);
			lineData.ViolationCode = ViolationCodeList.Codes._01;
			AssertEquals(ViolationCodeList.Descriptions._01, lineData.ViolationDescription);
		}
	}
}
