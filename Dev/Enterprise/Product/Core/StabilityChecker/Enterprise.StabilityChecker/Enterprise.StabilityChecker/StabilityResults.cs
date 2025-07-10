using System.Linq;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.StabilityChecker
{
	[XmlSerializerAssembly("Enterprise.StabilityChecker.XmlSerializers")]
	public sealed class StabilityResults : RegistryBusinessObjectTemplate
	{
		public StabilityResults()
		{
			DateTimeCalculated = ZDateTime.MinSmallDateTimeValue;
		}

		public ZDateTime DateTimeCalculated
		{
			get { return dateTimeCalculated; }
			set { dateTimeCalculated = new ZDateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second); }
		}
		ZDateTime dateTimeCalculated;

		public StabilityResultCollection Results
		{
			get
			{
				return results ?? (results = new StabilityResultCollection());
			}
		}
		StabilityResultCollection results;

		protected override RegistryBusinessObjectTemplate GetClone(ZArchitecture.Environment.FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new StabilityResults();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ZDateTime dateTimeCalculated;
			if (ZDateTime.TryParseISO8601Date(reader.ReadElementString(Schema.DateTimeCalculated), out dateTimeCalculated))
			{
				DateTimeCalculated = dateTimeCalculated;
			}
			results = (StabilityResultCollection)ZXmlSerializer.New(typeof(StabilityResultCollection)).Deserialize(reader);
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			writer.WriteElementString(Schema.DateTimeCalculated, DateTimeCalculated.ToISO8601String());
			ZXmlSerializer.New(typeof(StabilityResultCollection)).Serialize(writer, results);
		}

		public override bool Equals(object obj)
		{
			var other = obj as StabilityResults;
			return other != null &&
				other.DateTimeCalculated == DateTimeCalculated &&
				other.Results.ContainsSameElementsInAnyOrder(Results);
		}

		public override int GetHashCode()
		{
			return DateTimeCalculated.GetHashCode();
		}

		static class Schema
		{
			internal const string DateTimeCalculated = "DateTimeCalculated";
			internal const string Result = "StabilityResult";
		}
	}

	[XmlSerializerAssembly("Enterprise.StabilityChecker.XmlSerializers")]
	[XmlRoot("Results")]
	public sealed class StabilityResultCollection : RegistryBusinessObjectCollectionTemplate
	{
		protected override RegistryBusinessObjectCollectionTemplate GetClone(ZArchitecture.Environment.FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new StabilityResultCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StabilityResult();
		}

		public new StabilityResult this[int index]
		{
			get { return (StabilityResult)base[index]; }
		}

		public new StabilityResult AddNew()
		{
			return (StabilityResult)base.AddNew();
		}

		public bool HasUserNotificationIssues
		{
			get
			{
				return this.Cast<StabilityResult>().Any(x => x.StabilityLevel != StabilityResultLevel.Healthy &&
																										 x.StabilityLevel != StabilityResultLevel.Exception &&
																										 x.IsUserRelatedNotification);
			}
		}

		public bool HasNonUserRelatedIssues
		{
			get
			{
				return this.Cast<StabilityResult>().Any(x => x.StabilityLevel != StabilityResultLevel.Healthy &&
																										 x.StabilityLevel != StabilityResultLevel.Exception &&
																										 !x.IsUserRelatedNotification);
			}
		}
	}
}
