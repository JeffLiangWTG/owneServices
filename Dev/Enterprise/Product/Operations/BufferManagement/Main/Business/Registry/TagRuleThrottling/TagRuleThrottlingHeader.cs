using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.BufferManagement.Business
{
	[XmlSerializerAssembly("Enterprise.BufferManagement.Business.XmlSerializers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped")]
	public class TagRuleThrottlingHeader : RegistryBusinessObjectTemplateWithChildCollection
	{
		public abstract class Schema
		{
			public const string IsThrottlingEnabled = "IsThrottlingEnabled";
			public const string ThresholdCollection = "ThresholdCollection";
		}

		public ZBool IsThrottlingEnabled
		{
			get { return isThrottlingEnabled; }
			set
			{
				SetNonPersistentPropertyValue(IsThrottlingEnabledInfo, ref isThrottlingEnabled, value);
				isThrottlingEnabled = value;

				if (!IsValidationSuspended)
				{
					ValidateIsThrottlingEnabled();
				}
			}
		}
		ZBool isThrottlingEnabled;

		public virtual void ValidateIsThrottlingEnabled()
		{
			IsThrottlingEnabledInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo IsThrottlingEnabledInfo
		{
			get { return GetZPropertyInfo(Schema.IsThrottlingEnabled); }
		}

		public TagRuleThrottlingThresholdCollection ThresholdCollection
		{
			get
			{
				if (thresholdCollection == null)
				{
					thresholdCollection = new TagRuleThrottlingThresholdCollection(CurrentFallbackLevel, Factory);
					RegisterEditableChildObject(thresholdCollection);
				}
				return thresholdCollection;
			}
		}
		TagRuleThrottlingThresholdCollection thresholdCollection;

		protected override IEnumerable<RegistryBusinessObjectCollectionTemplate> ChildCollections
		{
			get { return new[] { ThresholdCollection }; }
		}

		public TagRuleThrottlingHeader()
		{
		}

		protected TagRuleThrottlingHeader(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected TagRuleThrottlingHeader(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public TagRuleThrottlingHeader(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int GetRunIntervalMinutesForRunTime(int runTimeInSeconds, string tagRuleName)
		{
			return ThresholdCollection.GetRunIntervalMinutesForRunTime(runTimeInSeconds, tagRuleName);
		}

		public static TagRuleThrottlingHeader GetDefaultThresholds()
		{
			var header = new TagRuleThrottlingHeader();
			header.IsThrottlingEnabled = ZBool.True;
			header.ThresholdCollection.AddNew(30, 15);
			header.ThresholdCollection.AddNew(60, 30);
			header.ThresholdCollection.AddNew(90, 45);
			header.ThresholdCollection.AddNew(120, 60);

			return header;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TagRuleThrottlingHeader(fallbackLevel, factory)
			{
				IsThrottlingEnabled = IsThrottlingEnabled,
				thresholdCollection = ThresholdCollection
			};
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ZBool isThrottlingEnabledParsed;
			var isThrottlingEnabledValue = reader.ReadElementString(Schema.IsThrottlingEnabled);

			if (!ZBool.TryParse(isThrottlingEnabledValue, out isThrottlingEnabledParsed))
			{
				throw new FormatException("The IsThrottlingEnabled XML element does not have a properly formatted ZBool value. Value: " + isThrottlingEnabledValue);
			}

			IsThrottlingEnabled = isThrottlingEnabledParsed;
			thresholdCollection = (TagRuleThrottlingThresholdCollection)TagRuleThrottlingThresholdCollectionSerialiser.Deserialize(reader);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteStartElement(Schema.IsThrottlingEnabled);
			writer.WriteValue(IsThrottlingEnabled);
			writer.WriteEndElement();
			TagRuleThrottlingThresholdCollectionSerialiser.Serialize(writer, ThresholdCollection);
		}

		ZXmlSerializer TagRuleThrottlingThresholdCollectionSerialiser
		{
			get
			{
				return tagRuleThrottlingThresholdCollectionSerialiser ?? (tagRuleThrottlingThresholdCollectionSerialiser = ZXmlSerializer.New(typeof(TagRuleThrottlingThresholdCollection)));
			}
		}
		ZXmlSerializer tagRuleThrottlingThresholdCollectionSerialiser;

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			var target = ((TagRuleThrottlingHeader)(clone));
			base.CopyValuesToClone(target);
			target.IsThrottlingEnabled = IsThrottlingEnabled;
			target.thresholdCollection = ThresholdCollection;
		}
	}
}
