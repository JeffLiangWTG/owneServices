namespace Enterprise.Customs.EU.H7.Business
{
	public class ValidationConfiguration
	{
		public ValidationMessage ValidationMessage => validationMessage ??= GetValidationMessage();
		ValidationMessage validationMessage;

		protected virtual ValidationMessage GetValidationMessage()
		{
			return new ValidationMessage();
		}
	}
}
