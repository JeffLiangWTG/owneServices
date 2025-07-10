//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCAMessageSendingActionValidation
//
//    This class should be used for overriding validation in AutoCAMessageSendingActionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CAMessageSendingActionValidation : AutoCAMessageSendingActionValidation
	{
		public CAMessageSendingActionValidation(AutoCAMessageSendingAction parent)
			: base(parent)
		{
		}

		#region Implementation
		protected new CAMessageSendingAction Parent
		{
			get { return (CAMessageSendingAction)base.Parent; }
		}

		protected override void CheckCA_SaveWithoutSending()
		{
			base.CheckCA_SaveWithoutSending();
			ValidateCA_SaveWithoutSendingReasonText();
		}

		protected override void CheckCA_SaveWithoutSendingReasonText()
		{
			base.CheckCA_SaveWithoutSendingReasonText();
			if (Parent.CA_SaveWithoutSending && Parent.CA_SaveWithoutSendingReasonText.IsEmpty)
			{
				Parent.CA_SaveWithoutSendingReasonTextInfo.AddError(ReasonForNotSendingRequired);
			}
		}
		internal static string ReasonForNotSendingRequired
		{
			get { return Res.GetString("31c339b0-6cf6-4a0f-bf2f-915e37d7ada1", "Please enter a reason for not sending."); }
		}
		#endregion Implementation
	}
}
