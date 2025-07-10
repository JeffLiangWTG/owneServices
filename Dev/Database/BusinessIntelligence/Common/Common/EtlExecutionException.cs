namespace CargoWise.Bi.Common
{
	using System;
	using System.Text.RegularExpressions;
	using CargoWise.Data;

	[Serializable]
	public class EtlExecutionException : Exception
	{
		public DbErrorType ExceptionType;
		public EtlExecutionException(string message) : base(message)
		{
			if (serverNotConfigured.IsMatch(message))
			{
				ExceptionType = DbErrorType.ServerIsNotConfigured;
			}
		}
		public EtlExecutionException(string message, SqlException ex) : base(message)
		{
			ExceptionType = new DbErrorMatch(ex).ExceptionType;
		}

#if NETFRAMEWORK
		protected EtlExecutionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		static readonly Regex serverNotConfigured = new Regex(@"Server\s+'.+'\s+is\s+not\s+configured\s+for\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	}
}
