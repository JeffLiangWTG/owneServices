namespace Enterprise.Customs.IE.H7.Business
{
	public class ValidationConfiguration : EU.H7.Business.ValidationConfiguration
	{
		public new ValidationMessage ValidationMessage => (ValidationMessage)base.ValidationMessage;

		protected override EU.H7.Business.ValidationMessage GetValidationMessage() => new ValidationMessage();
	}
}
