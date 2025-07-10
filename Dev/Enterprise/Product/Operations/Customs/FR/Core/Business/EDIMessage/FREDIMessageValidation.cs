using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class FREDIMessageValidation : EDIMessageValidation
	{
		public FREDIMessageValidation(FREDIMessage parent) : base(parent)
		{
		}

		protected new FREDIMessage Parent => (FREDIMessage)base.Parent;

		protected override void CheckEM_HeldUntilDateIsValidZDateTimeRange()
		{
		}
	}
}
