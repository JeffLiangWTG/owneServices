using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Common.CA.DIF.Testing
{
	class StatusListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestHasBeenLodgedAtCustoms()
		{
			NUnit.Framework.Assert.That(!StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.AwaitingOriginal));
			NUnit.Framework.Assert.That(StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.AcknowledgedOriginal));
			NUnit.Framework.Assert.That(!StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.ErrorAcknowledgedOriginal));
			NUnit.Framework.Assert.That(StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.AwaitingAmendment));
			NUnit.Framework.Assert.That(StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.ErrorAcknowledgedAmendment));
			NUnit.Framework.Assert.That(StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.AcknowledgedAmendment));
			NUnit.Framework.Assert.That(StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.AwaitingChange));
			NUnit.Framework.Assert.That(StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.ErrorAcknowledgedChange));
			NUnit.Framework.Assert.That(StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.AcknowledgedChange));
			NUnit.Framework.Assert.That(StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.AwaitingWithdrawal));
			NUnit.Framework.Assert.That(StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.ErrorAcknowledgedWithdrawal));
			NUnit.Framework.Assert.That(!StatusList.HasBeenLodgedAtCustoms(StatusList.Codes.AcknowledgedWithdrawal));
			NUnit.Framework.Assert.That(!StatusList.HasBeenLodgedAtCustoms(""));
		}

		[ExpectNoExceptions]
		public void TestIsWaitingForResponse()
		{
			NUnit.Framework.Assert.That(StatusList.IsWaitingForResponse(StatusList.Codes.AwaitingOriginal));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(StatusList.Codes.AcknowledgedOriginal));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(StatusList.Codes.ErrorAcknowledgedOriginal));
			NUnit.Framework.Assert.That(StatusList.IsWaitingForResponse(StatusList.Codes.AwaitingAmendment));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(StatusList.Codes.ErrorAcknowledgedAmendment));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(StatusList.Codes.AcknowledgedAmendment));
			NUnit.Framework.Assert.That(StatusList.IsWaitingForResponse(StatusList.Codes.AwaitingChange));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(StatusList.Codes.ErrorAcknowledgedChange));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(StatusList.Codes.AcknowledgedChange));
			NUnit.Framework.Assert.That(StatusList.IsWaitingForResponse(StatusList.Codes.AwaitingWithdrawal));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(StatusList.Codes.ErrorAcknowledgedWithdrawal));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(StatusList.Codes.AcknowledgedWithdrawal));
			NUnit.Framework.Assert.That(!StatusList.IsWaitingForResponse(""));
		}
	}
}
