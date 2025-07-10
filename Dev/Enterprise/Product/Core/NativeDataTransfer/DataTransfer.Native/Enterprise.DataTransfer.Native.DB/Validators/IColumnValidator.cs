namespace Enterprise.DataTransfer.Native.DB.Validators
{
	public class ValidationResult
	{
		public ValidationResult(bool success)
		{
			Success = success;
		}

		public ValidationResult(bool success, string error)
			: this(success)
		{
			Error = error;
		}

		public bool Success { get; }
		public string Error { get; }
	}

	public interface IColumnValidator
	{
		ValidationResult Validate(object value);
	}
}
