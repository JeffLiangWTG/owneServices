using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class NoGuiDeferredAmendmentSavingOptions : IDeferredAmendmentSavingOptions
	{
		public ZBool ShouldTakeReasonForSavingWithoutSendingSeparately => false;

		public ZBool ShouldSendMessages => false;

		public ZBool ShouldSaveWithoutSendingAmendment => !IsCancelled;

		public ZBool IsCancelled { get; set; }

		public ZBool SignificantAmendmentsHaveBeenMade => false;

		public void SetSaveWithEntryChangesValueForTestingTo(ZBool value) { }

		public void SetSaveWithoutEntryChangesValueForTestingTo(ZBool value) { }

		public void SetSendAmendmentValueForTestingTo(ZBool value) { }
	}
}
