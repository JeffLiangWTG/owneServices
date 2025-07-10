using System;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	[XmlSerializerAssembly("Enterprise.BufferManagement.Business.XmlSerializers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped")]
	public class TagRuleThrottlingThreshold : RegistryBusinessObjectTemplate
	{
		public abstract class Schema
		{
			public const string RunTime = "RunTime";
			public const string RunInterval = "RunInterval";
		}

		public ZInt RunTime
		{
			get { return runTime; }
			set
			{
				SetNonPersistentPropertyValue(RunTimeInfo, ref runTime, value);
				if (!IsValidationSuspended)
				{
					ValidateRunTime();
				}
			}
		}
		ZInt runTime;

		public virtual void ValidateRunTime()
		{
			RunTimeInfo.ClearAllNotifications();
			CompareValidation.CheckNumberGreaterThanZero(RunTimeInfo);
		}

		public virtual ZPropertyInfo RunTimeInfo
		{
			get { return GetZPropertyInfo(Schema.RunTime); }
		}

		public ZInt RunInterval
		{
			get { return runInterval; }
			set
			{
				SetNonPersistentPropertyValue(RunIntervalInfo, ref runInterval, value);
				if (!IsValidationSuspended)
				{
					ValidateRunInterval();
				}
			}
		}
		ZInt runInterval;

		public virtual void ValidateRunInterval()
		{
			RunIntervalInfo.ClearAllNotifications();
			CompareValidation.CheckNumberGreaterThanZero(RunIntervalInfo);

			var intervalInSeconds = RunInterval * 60;

			if (intervalInSeconds <= RunTime)
			{
				RunIntervalInfo.AddError(Res.GetString("522e0a56-1a96-4316-962c-1a432809ebec", "Value must be longer in duration than the specified run time. Run time: {0} seconds. Interval entered: {1} seconds.", RunTime, intervalInSeconds));
			}
		}

		public virtual ZPropertyInfo RunIntervalInfo
		{
			get { return GetZPropertyInfo(Schema.RunInterval); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateRunTime();
			ValidateRunInterval();
		}

		public TagRuleThrottlingThreshold()
		{
		}

		protected TagRuleThrottlingThreshold(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected TagRuleThrottlingThreshold(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public TagRuleThrottlingThreshold(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public const int NotSpecifiedValue = -1;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TagRuleThrottlingThreshold(fallbackLevel, factory)
			{
				runTime = RunTime,
				runInterval = RunInterval
			};
		}

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteStartElement(Schema.RunTime);
			writer.WriteValue(RunTime);
			writer.WriteEndElement();
			writer.WriteStartElement(Schema.RunInterval);
			writer.WriteValue(RunInterval);
			writer.WriteEndElement();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			RunTime = GetZIntFromXml(reader, Schema.RunTime);
			RunInterval = GetZIntFromXml(reader, Schema.RunInterval);
		}

		static ZInt GetZIntFromXml(XmlReaderWrapper reader, string elementName)
		{
			ZInt parsed;
			var stringValue = reader.ReadElementString(elementName);

			if (!ZInt.TryParse(stringValue, out parsed))
			{
				throw new FormatException(string.Format(CultureInfo.InvariantCulture, "The {0} XML element does not have a properly formatted ZInt value. Value: {1}", elementName, stringValue));
			}

			return parsed;
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			var target = ((TagRuleThrottlingThreshold)(clone));
			base.CopyValuesToClone(target);
			target.RunTime = RunTime;
			target.RunInterval = RunInterval;
		}
	}
}
