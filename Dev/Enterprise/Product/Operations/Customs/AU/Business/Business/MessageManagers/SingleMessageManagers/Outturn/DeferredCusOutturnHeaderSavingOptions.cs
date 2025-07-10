using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DeferredCusOutturnHeaderSavingOptions : AutoDeferredCusOutturnHeaderSavingOptions, IDeferredAmendmentSavingOptions
	{
		public DeferredCusOutturnHeaderSavingOptions(Customs.Business.CusOutturnHeader header)
			: base(header.Factory)
		{
			this.header = header;
		}
		readonly Customs.Business.CusOutturnHeader header;

		public ZString OutturnReference
		{
			get { return header.C6_SendersMessageReference; }
		}

		protected override void SetDefaultValues()
		{
			ShouldSaveAndSendMessages = true;
		}

		ZBool IDeferredAmendmentSavingOptions.IsCancelled
		{
			get { return IsCancel; }
			set { IsCancel = value; }
		}

#if DEBUG
		void IDeferredAmendmentSavingOptions.SetSaveWithEntryChangesValueForTestingTo(ZBool value)
		{
		}

		void IDeferredAmendmentSavingOptions.SetSaveWithoutEntryChangesValueForTestingTo(ZBool value)
		{
		}

		void IDeferredAmendmentSavingOptions.SetSendAmendmentValueForTestingTo(ZBool value)
		{
		}
#endif

		ZBool IDeferredAmendmentSavingOptions.ShouldSaveWithoutSendingAmendment
		{
			get { return ShouldSaveWithoutSendingMessages; }
		}

		ZBool IDeferredAmendmentSavingOptions.ShouldSendMessages
		{
			get { return ShouldSaveAndSendMessages; }
		}

		ZBool IDeferredAmendmentSavingOptions.ShouldTakeReasonForSavingWithoutSendingSeparately
		{
			get { return false; }
		}

		ZBool IDeferredAmendmentSavingOptions.SignificantAmendmentsHaveBeenMade
		{
			get { return true; }
		}
	}
}
