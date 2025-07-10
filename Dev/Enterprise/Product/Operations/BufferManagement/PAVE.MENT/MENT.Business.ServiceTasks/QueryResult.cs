using CargoWise.Types;

namespace Enterprise.PAVE.MENT.Business.ServiceTasks
{
	public class QueryResult
	{
		public QueryResult()
		{
			Result = InserterResult.None;
			Messages = ZString.Empty;
		}

		public ZString Messages { get; set; }

		public InserterResult Result { get; set; }
	}

	public enum InserterResult
	{
		None,
		Success,
		InfrastructureFailure,
		Error
	}
}
