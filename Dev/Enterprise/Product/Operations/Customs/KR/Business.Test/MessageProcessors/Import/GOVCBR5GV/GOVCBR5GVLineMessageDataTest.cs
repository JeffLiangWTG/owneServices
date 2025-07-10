using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GOVCBR5GVLineMessageData))]
	sealed class GOVCBR5GVLineMessageDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDataItemIDDescription()
		{
			var lineData = new GOVCBR5GVLineMessageData(Factory);
			lineData.FirstLineDataItemID = ImportAmendmentDataItemIDList.Codes.B407;
			AssertEquals(ImportAmendmentDataItemIDList.Descriptions.B407, lineData.FirstLineDataItemIDDescription);
			AssertEquals(ZString.Empty, lineData.SecondLineDataItemIDDescription);

			lineData.SecondLineDataItemID = ImportAmendmentDataItemIDList.Codes.B408;
			AssertEquals(ImportAmendmentDataItemIDList.Descriptions.B407, lineData.FirstLineDataItemIDDescription);
			AssertEquals(ImportAmendmentDataItemIDList.Descriptions.B408, lineData.SecondLineDataItemIDDescription);
		}
	}
}
