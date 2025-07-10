using System.ComponentModel;
using System.Diagnostics;
using CargoWise.Common;
using Enterprise.Integration;

namespace Enterprise.ServiceManager.Runner
{
	class ProcessEnvironmentRecorder
	{
		public ProcessEnvironmentRecorder(ILogger logger, ICurrentProcessInfoProvider? currentProcessInfoProvider = null)
		{
			this.logger = logger;
			this.currentProcessInfoProvider = currentProcessInfoProvider ?? new CurrentProcessInfoProvider();
		}

		TimeSpan? TotalProcessorTime
		{
			get
			{
				try
				{
					return currentProcessInfoProvider.TotalProcessorTime;
				}
				catch (Win32Exception ex) when (ex.NativeErrorCode == 5)
				{
					logger.Log(LogType.Warning, "Unable to calculate processor time for service task run.  Access is denied.");
					return null;
				}
			}
		}

		public IDisposable StartRun(string code)
		{
			startProcessorTime = TotalProcessorTime;
			var startClockTime = Stopwatch.StartNew();
			return new DisposableAction(() => Record(code, startClockTime));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "This is logging so want real machine time")]
		void Record(string code, Stopwatch startClockTime)
		{
			var environmentKey = "ServiceTaskSummary_" + code;
			var data = System.Environment.GetEnvironmentVariable(environmentKey);

			Int64 runCount = 0;
			var processorString = string.Empty;
			var clockTimeString = string.Empty;

			if (data != null)
			{
				var splitData = data.Split(' ');
				runCount = Int64.Parse(splitData[0].Split('=')[1]);
				processorString = splitData[1];
				clockTimeString = splitData[2];
			}

			runCount++;

			var processorTime = new TimeSerializer("CPUTime", processorString);
			var totalProcessorTime = TotalProcessorTime;
			if (startProcessorTime.HasValue && totalProcessorTime.HasValue)
			{
				processorTime.AddTimeSpan(totalProcessorTime.Value - startProcessorTime.Value);
			}

			var clockTime = new TimeSerializer("ClockTime", clockTimeString);
			clockTime.AddTimeSpan(startClockTime.Elapsed);

			System.Environment.SetEnvironmentVariable(environmentKey, "RunCount=" + runCount.ToString() + " " + processorTime.ToString() + " " + clockTime.ToString());
			System.Environment.SetEnvironmentVariable("ServiceTaskStatus", "Idle at " + DateTime.Now.ToLongTimeString());
		}

		TimeSpan? startProcessorTime;
		readonly ICurrentProcessInfoProvider currentProcessInfoProvider;
		readonly ILogger logger;

		class TimeSerializer
		{
			public TimeSerializer(string key, string originalData)
			{
				this.key = key;
				if (!string.IsNullOrEmpty(originalData))
				{
					var splitData = originalData.Split('=');
					TimeSpan.TryParse(splitData[1], out originalTime);
				}
			}
			TimeSpan originalTime;
			readonly string key;

			public void AddTimeSpan(TimeSpan time)
			{
				originalTime += time;
			}

			public override string ToString()
			{
				return key + "=" + originalTime.ToString();
			}
		}
	}
}
