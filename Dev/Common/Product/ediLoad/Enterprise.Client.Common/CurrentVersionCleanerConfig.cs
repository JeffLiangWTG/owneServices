using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using Enterprise.Upgrades;

namespace Enterprise.Client.Common
{
	[Serializable]
	public class CurrentVersionCleanerConfig : ICurrentVersionCleanerConfigWithLogs
	{
		[XmlIgnore]
		public TimeSpan VersionInactiveDurationInDays { get; set; }

		[XmlIgnore]
		public TimeSpan CurrentVersionCleanupIntervalInDays { get; set; }

		public DateTime LastStartTime { get; set; }

		public DateTime LastSuccessTime { get; set; }

		public DateTime NextRuntime { get; set; }

		public string CurrentVersionFile { get; set; }

		// XmlSerializer does not support TimeSpan, so use this property for 
		// serialization instead.
		[Browsable(false)]
		[XmlElement(DataType = "duration", ElementName = "VersionInactiveDurationInDays")]
		public string VersionInactiveDurationInDaysString
		{
			get => XmlConvert.ToString(VersionInactiveDurationInDays);
			set => VersionInactiveDurationInDays = string.IsNullOrEmpty(value) ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
		}

		// XmlSerializer does not support TimeSpan, so use this property for 
		// serialization instead.
		[Browsable(false)]
		[XmlElement(DataType = "duration", ElementName = "CurrentVersionCleanupIntervalInDays")]
		public string CurrentVersionCleanupIntervalInDaysString
		{
			get => XmlConvert.ToString(CurrentVersionCleanupIntervalInDays);
			set => CurrentVersionCleanupIntervalInDays = string.IsNullOrEmpty(value) ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
		}

		public ApplicationUsageLogFile LogFile => logFile;
		[NonSerialized]
		readonly ApplicationUsageLogFile logFile = new ApplicationUsageLogFile();
	}
}
