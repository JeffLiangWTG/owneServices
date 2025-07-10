using System;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	[Serializable]
	public class NewReportConfigurationConcurrencyException : Exception
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant String")]
		const string concurrencyError = "This configuration has been modified recently, please refresh your configurations";

		public NewReportConfigurationConcurrencyException(string msg = concurrencyError) : base(msg)
		{ }

#if NETFRAMEWORK
		protected NewReportConfigurationConcurrencyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
#endif
	}
}
