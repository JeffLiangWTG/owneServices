
using CargoWise.Types;
namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class ResponseOrError<ExpectedType> where ExpectedType : Body
	{
		public ResponseOrError(ExpectedType data, ErrorResponse error)
		{
			this.Data = data;
			this.Error = error;
		}

		public ExpectedType Data { get; set; }
		public ErrorResponse Error { get; set; }

		public override string ToString()
		{
			var sb = new ZStringBuilder();
			sb.Append("Expected Type: " + typeof(ExpectedType).FullName);
			if (Data != null)
			{
				sb.Append("Data: " + Data.PayloadAsString);
			}
			else
			{
				sb.Append("Data is null");
			}
			if (Error != null)
			{
				sb.Append("Error payload: " + Error.PayloadAsString);
				sb.Append("Error reason: " + Error.ReasonForFailure);
				sb.Append("Error response code: " + Error.ResponseCode);
				sb.Append("Error response reason: " + Error.ResponseReasonCode);
				sb.Append("Error success flag: " + Error.WasOperationSuccessful);
			}
			else
			{
				sb.Append("Error is null");
			}
			return sb.ToStringWithNewLineBetweenAppends();
		}
	}
}
