using Enterprise.Registry.Business;

namespace Enterprise.StabilityChecker
{
	sealed class StabilityResultsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<StabilityResults>
	{
		public StabilityResultsRegistryDataType()
			: base(new StabilityResults())
		{ }

		protected override StabilityResults CloneValue(StabilityResults value)
		{
			var clone = new StabilityResults();
			clone.DateTimeCalculated = value.DateTimeCalculated;
			clone.Results.AddRange(value.Results);
			return clone;
		}

		protected override bool ValuesAreEqualCore(StabilityResults a, StabilityResults b)
		{
			return Equals(a, b);
		}
	}
}
