using System;

namespace Enterprise.Accounting.Integration
{
	public enum ExRateSourceType
	{
		Voyage,
		BillingJob
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class SupportExRateSourceAttribute : Attribute
	{
		public SupportExRateSourceAttribute(ExRateSourceType sourceType)
		{
			this.sourceType = sourceType;
		}

		public static ExRateSourceType[] SupportedExRateSources(Type type)
		{
			SupportExRateSourceAttribute[] attributes = (SupportExRateSourceAttribute[])Attribute.GetCustomAttributes(type, typeof(SupportExRateSourceAttribute));
			return Array.ConvertAll(attributes, (c) => c.SourceType);
		}

		#region Implementation

		readonly ExRateSourceType sourceType;
		public ExRateSourceType SourceType
		{
			get { return sourceType; }
		}

		#endregion
	}
}
