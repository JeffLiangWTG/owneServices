using System;
using System.Diagnostics.CodeAnalysis;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class NullServiceTaskScheduleThreadSafeReader : IServiceTaskScheduleThreadSafeReader
	{
		NullServiceTaskScheduleThreadSafeReader()
		{
		}

		public static NullServiceTaskScheduleThreadSafeReader Instance => instance;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly NullServiceTaskScheduleThreadSafeReader instance = new NullServiceTaskScheduleThreadSafeReader();

		public bool IsActive => false;
		public string ScheduleDescription => string.Empty;
		public string ScheduleCategory => string.Empty;
		public TimeSpan SchedulePeriodDuration => TimeSpan.Zero;
		public string ConfigString => string.Empty;
		public TimeSpan OverdueDuration => TimeSpan.Zero;
		public int SecondaryProcessesMaxCount => 0;
		public DateTime LastRunTime => DateTime.MinValue;
		public DateTime LastErrorTime => DateTime.MinValue;

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Not a duration")]
		public int ErrorCountLast24Hours => 0;
	}
}
