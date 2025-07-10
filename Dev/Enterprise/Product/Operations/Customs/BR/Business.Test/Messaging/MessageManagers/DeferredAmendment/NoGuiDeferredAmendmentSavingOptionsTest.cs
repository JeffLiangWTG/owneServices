using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(NoGuiDeferredAmendmentSavingOptions))]
	public class NoGuiDeferredAmendmentSavingOptionsTest : TestCase
	{
		public void TestIDeferredAmendmentSavingOptions()
		{
			var savingOptions = new NoGuiDeferredAmendmentSavingOptions() as IDeferredAmendmentSavingOptions;
			Assert("ShouldTakeReasonForSavingWithoutSendingSeparately", !savingOptions.ShouldTakeReasonForSavingWithoutSendingSeparately);
			Assert("ShouldSendMessages", !savingOptions.ShouldSendMessages);
			Assert("ShouldSaveWithoutSendingAmendment", savingOptions.ShouldSaveWithoutSendingAmendment);
			Assert("SignificantAmendmentsHaveBeenMade", !savingOptions.SignificantAmendmentsHaveBeenMade);
			Assert("IsCancelled", !savingOptions.IsCancelled);

			savingOptions.IsCancelled = true;
			Assert("ShouldSaveWithoutSendingAmendment", !savingOptions.ShouldSaveWithoutSendingAmendment);
		}
	}
}
