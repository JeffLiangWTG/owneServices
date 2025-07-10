using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	interface IDummyUpdateConditionStrategy
	{
	}

	class DummyUpdateConditionStrategy : DeferTriggerOnUpdateConditionStrategy, IDummyUpdateConditionStrategy
	{
		public DummyUpdateConditionStrategy()
		{
			ShouldInsertsBeDeferredForTesting = base.ShouldInsertsBeDeferred;
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
		{
			get
			{
				return new SchemaColumn[]
				{
					DummyBizoSchema.Z0_Decimal,
					DummyBizoSchema.Z0_DateTimeOffset,
					DummyBizoSchema.Z0_Guid
				};
			}
		}

		protected override bool ShouldInsertsBeDeferred => ShouldInsertsBeDeferredForTesting;

		public bool ShouldInsertsBeDeferredForTesting { get; set; }
	}
}
