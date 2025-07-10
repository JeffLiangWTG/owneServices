using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Common.US.DIS.Testing
{
	class StatusListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestHasBeenLodgedAtCustoms()
		{
			NUnit.Framework.Assert.That(!StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.AOS));
			NUnit.Framework.Assert.That(StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.COS));
			NUnit.Framework.Assert.That(!StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.EOS));
			NUnit.Framework.Assert.That(StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.ARS));
			NUnit.Framework.Assert.That(StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.ERS));
			NUnit.Framework.Assert.That(StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.CRS));
			NUnit.Framework.Assert.That(StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.AWS));
			NUnit.Framework.Assert.That(StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.EWS));
			NUnit.Framework.Assert.That(!StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.CWS));
			NUnit.Framework.Assert.That(!StatusList.HasBeenLodgedAtCustoms(""));
		}

		[ExpectNoExceptions]
		public void TestIsWaitingForResponse()
		{
			NUnit.Framework.Assert.That(StatusList.IsWaitingForResponse(StatusList.Codes.AOS));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(StatusList.Codes.COS));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(StatusList.Codes.EOS));
			NUnit.Framework.Assert.That(StatusList.IsWaitingForResponse(StatusList.Codes.ARS));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(StatusList.Codes.ERS));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(StatusList.Codes.CRS));
			NUnit.Framework.Assert.That(StatusList.IsWaitingForResponse(StatusList.Codes.AWS));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(StatusList.Codes.EWS));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(StatusList.Codes.CWS));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(""));
		}

		[ExpectNoExceptions]
		public void TestGetCodeByDocumentReviewStatus()
		{
			NUnit.Framework.Assert.That(StatusList.GetCodeByDocumentReviewStatus("REJECTED"), Is.EqualTo(StatusList.Codes.ERV));
			NUnit.Framework.Assert.That(StatusList.GetCodeByDocumentReviewStatus("ACCEPTED"), Is.EqualTo(StatusList.Codes.ERA));
			NUnit.Framework.Assert.That(StatusList.GetCodeByDocumentReviewStatus("UNDER_REVIEW"), Is.EqualTo(StatusList.Codes.ERR));
			NUnit.Framework.Assert.That(StatusList.GetCodeByDocumentReviewStatus(""), Is.EqualTo(""));
			NUnit.Framework.Assert.That(StatusList.GetCodeByDocumentReviewStatus(null), Is.EqualTo(""));
		}
	}
}
