using System;

namespace Enterprise.Dash.Integration
{
	[Serializable]
	public class DashException : Exception
	{
		public DashException()
		{
		}

		public DashException(string message) : base(message)
		{
		}

		public DashException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
