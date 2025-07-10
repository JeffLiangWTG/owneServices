using System.Data;

namespace Enterprise.DbUpgrader.Transformation.Common;

public struct SessionWaitStats
{
	public SessionWaitStats(string waitType, long waitTimeMs, long waitingTaskCount, long maxWaitTimeMs, long signalWaitTimeMs)
	{
		WaitType = waitType;
		WaitTimeMs = waitTimeMs;
		WaitingTaskCount = waitingTaskCount;
		MaxWaitTimeMs = maxWaitTimeMs;
		SignalWaitTimeMs = signalWaitTimeMs;
	}

	public SessionWaitStats(IDataRecord reader)
		: this((string)reader["wait_type"], (long)reader["wait_time_ms"], (long)reader["waiting_tasks_count"], (long)reader["max_wait_time_ms"], (long)reader["signal_wait_time_ms"])
	{
	}

	public void Deconstruct(out string waitType, out long waitTimeMs, out long waitingTaskCount, out long maxWaitTimeMs, out long signalWaitTimeMs)
	{
		waitType = WaitType;
		waitTimeMs = WaitTimeMs;
		waitingTaskCount = WaitingTaskCount;
		maxWaitTimeMs = MaxWaitTimeMs;
		signalWaitTimeMs = SignalWaitTimeMs;
	}

	public string WaitType { get; }
	public long WaitTimeMs { get; }
	public long WaitingTaskCount { get; }
	public long MaxWaitTimeMs { get; }
	public long SignalWaitTimeMs { get; }
}
