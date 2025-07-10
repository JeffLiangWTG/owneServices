using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class IssueWorkItemCreationThreshold : RegistryBusinessObjectTemplate
	{
		public IssueWorkItemCreationThreshold()
			: base()
		{
		}

		public IssueWorkItemCreationThreshold(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IssueWorkItemCreationThreshold(fallbackLevel, factory) { ThresholdTimespan = thresholdTimespan, IssueOccurrenceThreshold = issueOccurrenceThreshold };
		}

		#region Properties

		#region ThresholdTimespan

		public ZInt ThresholdTimespan
		{
			get { return thresholdTimespan; }
			set
			{
				SetNonPersistentPropertyValue(ThresholdTimespanInfo, ref thresholdTimespan, value);
				if (!IsValidationSuspended)
				{
					ValidateThresholdTimespan();
				}
			}
		}
		ZInt thresholdTimespan;

		public ZPropertyInfo ThresholdTimespanInfo
		{
			get { return GetZPropertyInfo(Schema.ThresholdTimespan); }
		}

		#endregion

		#region IssueOccurrenceThreshold

		public ZInt IssueOccurrenceThreshold
		{
			get { return issueOccurrenceThreshold; }
			set
			{
				SetNonPersistentPropertyValue(IssueOccurrenceThresholdInfo, ref issueOccurrenceThreshold, value);
				if (!IsValidationSuspended)
				{
					ValidateIssueOccurrenceThreshold();
				}
			}
		}
		ZInt issueOccurrenceThreshold;

		public ZPropertyInfo IssueOccurrenceThresholdInfo
		{
			get { return GetZPropertyInfo(Schema.IssueOccurrenceThreshold); }
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateThresholdTimespan();
			ValidateIssueOccurrenceThreshold();
		}

		public void ValidateThresholdTimespan()
		{
			ThresholdTimespanInfo.ClearAllNotifications();
			CompareValidation.CheckGreaterThanOrEqualTo(ThresholdTimespanInfo, 1);
		}

		public void ValidateIssueOccurrenceThreshold()
		{
			IssueOccurrenceThresholdInfo.ClearAllNotifications();
			CompareValidation.CheckGreaterThanOrEqualTo(IssueOccurrenceThresholdInfo, 1);
		}

		#endregion

		#region Xml

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IssueOccurrenceThreshold = reader.ReadElementStringAsZInt(Schema.IssueOccurrenceThreshold);
			ThresholdTimespan = reader.ReadElementStringAsZInt(Schema.ThresholdTimespan);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IssueOccurrenceThreshold, IssueOccurrenceThreshold.ToString());
			writer.WriteElementString(Schema.ThresholdTimespan, ThresholdTimespan.ToString());
		}

		static class Schema
		{
			internal const string IssueOccurrenceThreshold = "IssueOccurrenceThreshold";
			internal const string ThresholdTimespan = "ThresholdTimespan";
		}

		#endregion
	}
}


