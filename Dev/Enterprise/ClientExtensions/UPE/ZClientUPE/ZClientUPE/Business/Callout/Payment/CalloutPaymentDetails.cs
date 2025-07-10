using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public abstract class CalloutPaymentDetails : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CalloutPaymentDetails(Callout callout)
			: base(callout.Factory)
		{
			this.Callout = callout;
		}

		public ZDecimal TotalAmount
		{
			get { return fTotalAmount; }
			set
			{
				SetNonPersistentPropertyValue(TotalAmountInfo, ref fTotalAmount, value);
			}
		}

		public ZPropertyInfo TotalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalAmount)); }
		}

		public void CreateCalloutPaymentNote()
		{
			CalloutPaymentNote note = (CalloutPaymentNote)Factory.New(typeof(CalloutPaymentNote));
			string noteTextFormat = "Payment Method: {0}" + System.Environment.NewLine + "Total Amount: {1}" + System.Environment.NewLine + "{2}";
			note.ST_NoteDataAsText = string.Format(noteTextFormat, PaymentMethodAsText, TotalAmount, Reference);
			Callout.Notes.GetAllNotes(); // this is to make sure CalloutPaymentNote is added as a CalloutPaymentNote and is read only straight away, not after saving
			Callout.Notes.Add(note);
		}

		#region Implementation

		protected abstract string Reference { get; }

		protected abstract string PaymentMethodAsText { get; }

		public readonly Callout Callout;
		ZDecimal fTotalAmount;

		#endregion
	}
}
