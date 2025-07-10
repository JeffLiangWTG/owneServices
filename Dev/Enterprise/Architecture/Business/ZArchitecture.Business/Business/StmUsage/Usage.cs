using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Enterprise.ZArchitecture.Business.Statistics.Xml
{
	public interface IUsage : IUsages
	{
		string Name { get; }
		string SubName { get; }
		DateTime StartTimeUTC { get; }
		DateTime EndTimeUTC { get; }
		double ElapsedWithoutChildrenSeconds { get; }
		double ElapsedWithChildrenSeconds { get; }
		int ActionCount { get; }
	}

	class Usage : Usages, IUsage
	{
		public Usage()
			: base()
		{
		}

		public static Usage New(IPerformanceStatisticCollectorToken token)
		{
			return new Usage
			{
				Name = token.Name ?? string.Empty,
				SubName = token.SubName ?? string.Empty,
				StartTimeUTC = token.TimeStartedUtc.ToDateTime(),
				EndTimeUTC = token.TimeEndedUtc.ToDateTime(),
				ElapsedWithoutChildrenSeconds = token.ElapsedExcludingChildren.TotalSeconds,
				ElapsedWithChildrenSeconds = token.ElapsedIncludingChildren.TotalSeconds,
				ActionCount = token.ActionCount
			};
		}

		[XmlElement("N")]
		public string Name { get; set; }

		[XmlElement("SN")]
		public string SubName { get; set; }

		[XmlElement("SDT", DataType = "dateTime")]
		public DateTime StartTimeUTC { get; set; }

		[XmlElement("EDT", DataType = "dateTime")]
		public DateTime EndTimeUTC { get; set; }

		[XmlElement("ExcSec")]
		public double ElapsedWithoutChildrenSeconds { get; set; }

		[XmlElement("IncSec")]
		public double ElapsedWithChildrenSeconds { get; set; }

		[XmlElement("AC")]
		public int ActionCount { get; set; }

		public override IEnumerator<IUsage> GetEnumerator()
		{
			var usages = new List<IUsage>() { this };
			foreach (var child in Nodes)
			{
				usages.AddRange(child);
			}
			return usages.GetEnumerator();
		}
	}
}
