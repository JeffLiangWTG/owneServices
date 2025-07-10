using System;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	[XmlSerializerAssembly("Enterprise.BufferManagement.Business.XmlSerializers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped")]
	public class TagRuleThrottlingThresholdCollection : RegistryBusinessObjectCollectionTemplate
	{
		public TagRuleThrottlingThresholdCollection()
		{
		}

		public TagRuleThrottlingThresholdCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new TagRuleThrottlingThreshold this[int index]
		{
			get { return (TagRuleThrottlingThreshold)Elements[index]; }
		}

		public new TagRuleThrottlingThreshold AddNew()
		{
			var threshold = (TagRuleThrottlingThreshold)base.AddNew();

			return threshold;
		}

		public TagRuleThrottlingThreshold AddNew(int runTime, int runInterval)
		{
			if (IsRunTimeSpecified(runTime))
			{
				throw new InvalidOperationException("Each run time must be unique.");
			}

			var threshold = AddNew();
			threshold.RunTime = runTime;
			threshold.RunInterval = runInterval;

			return threshold;
		}

		public ZBool IsRunTimeSpecified(int runTime)
		{
			return Elements.Cast<TagRuleThrottlingThreshold>().Any(x => x.RunTime == runTime);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int GetRunIntervalMinutesForRunTime(int runTimeInSeconds, string tagRuleName)
		{
			if (runTimeInSeconds < 0)
			{
				ErrorReporter.ReportOnce("GetRunIntervalMinutesForRunTime with negative runTimeInSeconds", FormattableString.Invariant($"runTimeInSeconds is less than 0: {runTimeInSeconds} for TagRule: {tagRuleName}")); // Developer exception message
				runTimeInSeconds = 0;
			}

			var sortedElements = Elements.Cast<TagRuleThrottlingThreshold>().OrderByDescending(x => x.RunTime);
			var threshold = sortedElements.FirstOrDefault(x => x.RunTime <= runTimeInSeconds);

			return threshold == null ? TagRuleThrottlingThreshold.NotSpecifiedValue : (int)threshold.RunInterval;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TagRuleThrottlingThreshold(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TagRuleThrottlingThresholdCollection(fallbackLevel, factory);
		}

		public void CopyTo(TagRuleThrottlingThreshold[] array, int index)
		{
			Elements.ToArray().CopyTo(array, index);
		}
	}
}
