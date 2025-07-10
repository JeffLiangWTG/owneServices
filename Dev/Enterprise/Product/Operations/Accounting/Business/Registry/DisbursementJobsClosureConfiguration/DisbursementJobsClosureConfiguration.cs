using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	public interface IDisbursementJobsClosureConfiguration
	{
		ZInt JobLevelOfShortfallUpTo { get; }
		ZInt JobLevelOfSurplusUpTo { get; }
		ZInt AggregatedLevelOfShortfallUpTo { get; }
		ZInt AggregatedLevelOfSurplusUpTo { get; }
	}

	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class DisbursementJobsClosureConfiguration : RegistryBusinessObjectTemplate, IDisbursementJobsClosureConfiguration
	{
		#region Schema

		public abstract class Schema
		{
			public const string JobLevelOfShortfallUpTo = "JobLevelOfShortfallUpTo";
			public const string JobLevelOfSurplusUpTo = "JobLevelOfSurplusUpTo";
			public const string AggregatedLevelOfShortfallUpTo = "AggregatedLevelOfShortfallUpTo";
			public const string AggregatedLevelOfSurplusUpTo = "AggregatedLevelOfSurplusUpTo";
		}

		#endregion

		#region Bound Properties

		public ZInt JobLevelOfShortfallUpTo
		{
			get { return jobLevelOfShortfallUpTo; }
			set
			{
				SetNonPersistentPropertyValue(JobLevelOfShortfallUpToInfo, ref jobLevelOfShortfallUpTo, value);
				if (!IsValidationSuspended)
				{
					ValidateJobLevelOfShortfallUpTo();
				}
			}
		}
		ZInt jobLevelOfShortfallUpTo;

		public ZPropertyInfo JobLevelOfShortfallUpToInfo => GetZPropertyInfo(Schema.JobLevelOfShortfallUpTo);

		public ZInt JobLevelOfSurplusUpTo
		{
			get { return jobLevelOfSurplusUpTo; }
			set
			{
				SetNonPersistentPropertyValue(JobLevelOfSurplusUpToInfo, ref jobLevelOfSurplusUpTo, value);
				if (!IsValidationSuspended)
				{
					ValidateJobLevelOfSurplusUpTo();
				}
			}
		}
		ZInt jobLevelOfSurplusUpTo;

		public ZPropertyInfo JobLevelOfSurplusUpToInfo => GetZPropertyInfo(Schema.JobLevelOfSurplusUpTo);

		public ZInt AggregatedLevelOfShortfallUpTo
		{
			get { return aggregatedLevelOfShortfallUpTo; }
			set
			{
				SetNonPersistentPropertyValue(AggregatedLevelOfShortfallUpToInfo, ref aggregatedLevelOfShortfallUpTo, value);
				if (!IsValidationSuspended)
				{
					ValidateAggregatedLevelOfShortfallUpTo();
				}
			}
		}
		ZInt aggregatedLevelOfShortfallUpTo;

		public ZPropertyInfo AggregatedLevelOfShortfallUpToInfo => GetZPropertyInfo(Schema.AggregatedLevelOfShortfallUpTo);

		public ZInt AggregatedLevelOfSurplusUpTo
		{
			get { return aggregatedLevelOfSurplusUpTo; }
			set
			{
				SetNonPersistentPropertyValue(AggregatedLevelOfSurplusUpToInfo, ref aggregatedLevelOfSurplusUpTo, value);
				if (!IsValidationSuspended)
				{
					ValidateAggregatedLevelOfSurplusUpTo();
				}
			}
		}
		ZInt aggregatedLevelOfSurplusUpTo;

		public ZPropertyInfo AggregatedLevelOfSurplusUpToInfo => GetZPropertyInfo(Schema.AggregatedLevelOfSurplusUpTo);

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateJobLevelOfShortfallUpTo();
			ValidateJobLevelOfSurplusUpTo();
			ValidateAggregatedLevelOfShortfallUpTo();
			ValidateAggregatedLevelOfSurplusUpTo();
		}

		void ValidateJobLevelOfShortfallUpTo()
		{
			CheckValueNotLessThanZero(JobLevelOfShortfallUpToInfo, jobLevelOfShortfallUpTo);
		}

		void ValidateJobLevelOfSurplusUpTo()
		{
			CheckValueNotLessThanZero(JobLevelOfSurplusUpToInfo, jobLevelOfSurplusUpTo);
		}

		void ValidateAggregatedLevelOfShortfallUpTo()
		{
			CheckValueNotLessThanZero(AggregatedLevelOfShortfallUpToInfo, aggregatedLevelOfShortfallUpTo);
		}

		void ValidateAggregatedLevelOfSurplusUpTo()
		{
			CheckValueNotLessThanZero(AggregatedLevelOfSurplusUpToInfo, aggregatedLevelOfSurplusUpTo);
		}

		void CheckValueNotLessThanZero(ZPropertyInfo propertyInfo, ZInt value)
		{
			propertyInfo.ClearAllNotifications();

			if (value < 0)
			{
				propertyInfo.AddError(Res.GetString("57ACC1C4-7A1E-41FE-B079-BE8C4D0932BC", "Value must be greater than or equal to 0."));
			}
		}

		#endregion

		#region Xml Serialisation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DisbursementJobsClosureConfiguration();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.JobLevelOfShortfallUpTo, JobLevelOfShortfallUpTo.ToString());
			writer.WriteElementString(Schema.JobLevelOfSurplusUpTo, JobLevelOfSurplusUpTo.ToString());
			writer.WriteElementString(Schema.AggregatedLevelOfShortfallUpTo, AggregatedLevelOfShortfallUpTo.ToString());
			writer.WriteElementString(Schema.AggregatedLevelOfSurplusUpTo, AggregatedLevelOfSurplusUpTo.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			JobLevelOfShortfallUpTo = ZInt.Parse(reader.ReadElementString(Schema.JobLevelOfShortfallUpTo));
			JobLevelOfSurplusUpTo = ZInt.Parse(reader.ReadElementString(Schema.JobLevelOfSurplusUpTo));
			AggregatedLevelOfShortfallUpTo = ZInt.Parse(reader.ReadElementString(Schema.AggregatedLevelOfShortfallUpTo));
			AggregatedLevelOfSurplusUpTo = ZInt.Parse(reader.ReadElementString(Schema.AggregatedLevelOfSurplusUpTo));
		}

		#endregion
	}
}
