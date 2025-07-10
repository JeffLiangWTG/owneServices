using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public class CalloutOtherPayment : CalloutPaymentDetails
	{
		public CalloutOtherPayment(Callout callout)
			: base(callout)
		{
		}

		[CargoWise.ComponentModel.MaxLength(300)]
		public ZString Note
		{
			get { return fNote; }
			set
			{
				if (fNote != value)
				{
					CheckMaximumLength(NoteInfo, value);
					SetNonPersistentPropertyValue(NoteInfo, ref fNote, value);
				}
			}
		}

		public ZPropertyInfo NoteInfo
		{
			get { return GetZPropertyInfo(nameof(Note)); }
		}

		protected override string Reference
		{
			get { return "Note: " + Note; }
		}

		protected override string PaymentMethodAsText
		{
			get { return "Other"; }
		}

		ZString fNote;
	}
}
