using CargoWise.Data.Diagnostics;

namespace Enterprise.ZArchitecture.Core
{
	public class QueryStackTraceRecorder : QueryStackTraceRecorderCore
	{
		#region Factory Method

		protected QueryStackTraceRecorder()
		{
		}

		public static IQueryStackTraceRecorder Instance
		{
			get
			{
				return QueryStackTraceRecorderCore.InstanceCore;
			}
		}

		#endregion
	}
}
