using System.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.PK, typeof(IDummyUpdateConditionStrategy))]
	public class DummyBizOWithDeferredUpdateTrigger : DummyBusinessObject
	{
		public const string StoredProc = "DummyUpdateStoredProc";
		public const string TriggerName = "TG_DummyUpdateTrigger";
		public DummyBizOWithDeferredUpdateTrigger(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZDecimal ColumnThatRequiresTriggerDeferral
		{
			get => Z0_Decimal;
			set => Z0_Decimal = value;
		}
		public ZDecimal ColumnThatDoesNotRequireTriggerDeferral
		{
			get => Z0_AnotherDecimal;
			set => Z0_AnotherDecimal = value;
		}
	}
}
