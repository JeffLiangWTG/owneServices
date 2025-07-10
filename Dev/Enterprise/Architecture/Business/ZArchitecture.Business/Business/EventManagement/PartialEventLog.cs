using System;
using CargoWise.Common;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business
{
	sealed class PartialEventLog
	{
		public PartialEventLog(IStmALog log)
		{
			this.log = Argument.NotNull(log, "log");

			this.partial = new Lazy<int>(() => ParseParameterValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Partial));
			this.total = new Lazy<int>(() => ParseParameterValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total));
		}

		readonly IStmALog log;
		readonly Lazy<int> partial;
		readonly Lazy<int> total;

		public IStmALog Log
		{
			get { return log; }
		}

		public int Partial
		{
			get { return partial.Value; }
		}

		public int Total
		{
			get { return total.Value; }
		}

		public string Location
		{
			get
			{
				return log.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location)
					? log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location]
					: string.Empty;
			}
		}

		int ParseParameterValue(string parameterName)
		{
			if (!log.Parameters.ContainsKey(parameterName))
			{
				return 0;
			}

			int result;

			if (!int.TryParse(log.Parameters[parameterName], out result))
			{
				return 0;
			}

			return result;
		}
	}
}
