namespace CargoWise.UniversalCopy
{
	public class CopyResult
	{
		public CopyResult(object @object, string errorMessage)
		{
			Object = @object;
			ErrorMessage = errorMessage;
		}
		public object Object { get; }
		public string ErrorMessage { get; }
	}
}
