using System;
using System.Threading;

namespace CargoWise.IO
{
	public static class PdhWrapper
	{
		public static double GetCounter(string counterName)
		{
			double output = 0;
			PdhQueryHandle queryHandle = null;
			PdhCounterHandle counterHandle = null;

			try
			{
				var result = PdhApi.OpenQuery(null, IntPtr.Zero, out queryHandle);
				if (result != PdhStatus.CStatusValidData)
				{
					throw new PdhException(String.Format("Unable to open a performance query.\r\nError Code: {0}.", result));
				}
				result = PdhApi.AddCounter(queryHandle, counterName, IntPtr.Zero, out counterHandle);
				if (result != PdhStatus.CStatusValidData)
				{
					throw new PdhException(String.Format("Unable to add a performance counter.\r\nError Code: {0}", result));
				}

				int maxTrial = 10;
				while (maxTrial > 0)
				{
					maxTrial = maxTrial - 1;
					PdhStatus collectResult = PdhApi.CollectQueryData(queryHandle);
					if (collectResult != PdhStatus.CStatusValidData)
					{
						throw new PdhException(String.Format("The runtime performance counter returns no data.\r\nError Code: {0}", collectResult));
					}
					PdhFormatCounterValue counterValue;
					if (PdhApi.GetFormattedCounterValue(counterHandle, PdhCounterFormat.Double | PdhCounterFormat.NoCap100 | PdhCounterFormat.NoScale, IntPtr.Zero, out counterValue) == PdhStatus.CStatusValidData && counterValue.CStatus == PdhCounterStatus.CStatusValidData)
					{
						output = counterValue.DoubleValue;
						break;
					}
					Thread.Sleep(1000);
				}

				if (maxTrial < 1)
				{
					throw new PdhException(String.Format("Unable to get counter value."));
				}
			}
			finally
			{
				if (queryHandle != null && !queryHandle.IsClosed)
				{
					queryHandle.Close();
				}
				if (counterHandle != null && !counterHandle.IsClosed)
				{
					counterHandle.Close();
				}
			}

			return output;
		}
	}
}
