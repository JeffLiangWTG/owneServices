using System.Data;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	[DeferTriggerAndRunBeforeCommit(TriggerName1, StoredProc, DummyDependentBizoSchema.Constants.PK, typeof(IDummyDependentUpdateConditionStrategy))]
	[DeferTriggerAndRunBeforeCommit(TriggerName2, StoredProc, DummyDependentBizoSchema.Constants.ZD1_Z0, typeof(IDummyDependentUpdateConditionStrategy))]
	public class DummyChildBizOWithDeferredUpdateTrigger : DummyDependantBusinessObject
	{
		public const string StoredProc = "DummyUpdateStoredProc";
		public const string TriggerName1 = "TG_DummyUpdateTrigger1";
		public const string TriggerName2 = "TG_DummyUpdateTrigger2";
		public DummyChildBizOWithDeferredUpdateTrigger(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
