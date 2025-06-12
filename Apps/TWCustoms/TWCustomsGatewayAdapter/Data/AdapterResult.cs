namespace CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter.Data
{
	public class AdapterResult
	{
		public static AdapterResult OK => new AdapterResult() { };

		public string ErrorCode { get; }

		public string ErrorMessge { get; }

		public bool HasError => !string.IsNullOrEmpty(ErrorCode);

		public AdapterResult(string errorCodeCode, string errorMessge)
		{
			ErrorCode = errorCodeCode;
			ErrorMessge = errorMessge;
		}

		private AdapterResult()
		{
		}
	}
}