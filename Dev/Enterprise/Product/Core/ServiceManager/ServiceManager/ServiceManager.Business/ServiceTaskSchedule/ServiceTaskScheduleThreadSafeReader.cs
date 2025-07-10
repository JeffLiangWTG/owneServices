using System;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class ServiceTaskScheduleThreadSafeReader : IServiceTaskScheduleThreadSafeReader, IDisposable
	{
		public ServiceTaskScheduleThreadSafeReader(ServiceTaskSchedule schedule)
		{
			this.schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));

			schedule.S5_IsActiveInfo.ValueChanged += IsActiveChangedEventHandler;
			schedule.S5_ScheduleDescriptionInfo.ValueChanged += ScheduleDescriptionChangedEventHandler;
			schedule.S5_TypeOfDocumentInfo.ValueChanged += TypeOfDocumentChangedEventHandler;
			schedule.S5_TaskPeriodInfo.ValueChanged += SchedulePeriodChangedEventHandler;
			schedule.S5_TaskPeriodCountInfo.ValueChanged += SchedulePeriodChangedEventHandler;
			schedule.S5_OverdueDurationInSecondsInfo.ValueChanged += OverdueDurationChangedEventHandler;
			schedule.S5_ScheduleStateInfo.ValueChanged += ScheduleStateChangedEventHandler;
			schedule.Reloaded += ScheduleReloadedEventHandler;
			UpdateAllCachedValues();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				schedule.S5_IsActiveInfo.ValueChanged -= IsActiveChangedEventHandler;
				schedule.S5_ScheduleDescriptionInfo.ValueChanged -= ScheduleDescriptionChangedEventHandler;
				schedule.S5_TypeOfDocumentInfo.ValueChanged -= TypeOfDocumentChangedEventHandler;
				schedule.S5_TaskPeriodInfo.ValueChanged -= SchedulePeriodChangedEventHandler;
				schedule.S5_TaskPeriodCountInfo.ValueChanged -= SchedulePeriodChangedEventHandler;
				schedule.S5_OverdueDurationInSecondsInfo.ValueChanged -= OverdueDurationChangedEventHandler;
				schedule.S5_ScheduleStateInfo.ValueChanged -= ScheduleStateChangedEventHandler;
				schedule.Reloaded -= ScheduleReloadedEventHandler;
			}
		}

		public bool IsActive { get; private set; }
		public string ScheduleDescription { get; private set; }
		public string ScheduleCategory { get; private set; }
		public TimeSpan SchedulePeriodDuration { get; private set; }
		public string ConfigString { get; private set; }
		public TimeSpan OverdueDuration { get; private set; }
		public int SecondaryProcessesMaxCount => schedule.SecondaryProcessesMaxCount;
		public DateTime LastRunTime => schedule.LastRunTime.IsValid ? schedule.LastRunTime.ToDateTime() : DateTime.MinValue;
		public DateTime LastErrorTime => schedule.LastErrorTime.IsValid ? schedule.LastErrorTime.ToDateTime() : DateTime.MinValue;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Not a duration")]
		public int ErrorCountLast24Hours => schedule.ErrorCountLast24Hours;

		void IsActiveChangedEventHandler(object sender, EventArgs e) => IsActive = schedule.S5_IsActive;
		void ScheduleDescriptionChangedEventHandler(object sender, EventArgs e) => ScheduleDescription = schedule.S5_ScheduleDescription;
		void TypeOfDocumentChangedEventHandler(object sender, EventArgs e) => ScheduleCategory = schedule.S5_TypeOfDocument;
		void SchedulePeriodChangedEventHandler(object sender, EventArgs e) => SchedulePeriodDuration = schedule.SchedulePeriodDuration;
		void OverdueDurationChangedEventHandler(object sender, EventArgs e) => OverdueDuration = TimeSpan.FromSeconds(schedule.S5_OverdueDurationInSeconds);
		void ScheduleStateChangedEventHandler(object sender, EventArgs e) => ConfigString = schedule.ConfigString;
		void ScheduleReloadedEventHandler(object sender, EventArgs e) => UpdateAllCachedValues();

		void UpdateAllCachedValues()
		{
			IsActive = schedule.S5_IsActive;
			ScheduleDescription = schedule.S5_ScheduleDescription;
			ScheduleCategory = schedule.S5_TypeOfDocument;
			SchedulePeriodDuration = schedule.SchedulePeriodDuration;
			OverdueDuration = TimeSpan.FromSeconds(schedule.S5_OverdueDurationInSeconds);
			ConfigString = schedule.ConfigString;
		}

		readonly ServiceTaskSchedule schedule;
	}
}
