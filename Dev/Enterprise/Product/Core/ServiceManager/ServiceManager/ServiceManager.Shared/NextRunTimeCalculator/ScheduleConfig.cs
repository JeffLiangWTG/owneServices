using System;
using System.Xml.Serialization;

namespace Enterprise.ServiceManager.Shared
{
	[Serializable]
	public class ScheduleConfig
	{
		[XmlElement("NextRunTimeCalculatorSeconds", typeof(NextRunTimeCalculatorSeconds))]
		[XmlElement("NextRunTimeCalculatorMinutes", typeof(NextRunTimeCalculatorMinutes))]
		[XmlElement("NextRunTimeCalculatorHours", typeof(NextRunTimeCalculatorHours))]
		[XmlElement("NextRunTimeCalculatorDays", typeof(NextRunTimeCalculatorDays))]
		[XmlElement("NextRunTimeCalculatorWorkingDays", typeof(NextRunTimeCalculatorWorkingDays))]
		[XmlElement("NextRunTimeCalculatorWeeks", typeof(NextRunTimeCalculatorWeeks))]
		[XmlElement("NextRunTimeCalculatorMonthsByDate", typeof(NextRunTimeCalculatorMonthsByDate))]
		[XmlElement("NextRunTimeCalculatorMonthsByLastDay", typeof(NextRunTimeCalculatorMonthsByLastDay))]
		[XmlElement("NextRunTimeCalculatorMonthsByDayOfWeek", typeof(NextRunTimeCalculatorMonthsByDayOfWeek))]
		[XmlElement("NextRunTimeCalculatorYearsByDate", typeof(NextRunTimeCalculatorYearsByDate))]
		[XmlElement("NextRunTimeCalculatorYearsByDayOfMonth", typeof(NextRunTimeCalculatorYearsByDayOfMonth))]
		public object? Calculator { get; set; }

		[XmlElement("SecondaryProcessesMaxCount", typeof(int))]
		public int SecondaryProcessesMaxCount { get; set; }

		[XmlElement("ConfigString", typeof(string))]
		public string? ConfigString { get; set; }

		internal static string TimeFormat => @"hh\:mm\:ss";

		public static readonly int MaxSecondaryProcessesMaxCount = 100;
		public static readonly int InvalidSecondaryProcessesMaxCount = MaxSecondaryProcessesMaxCount + 1;
	}
}
