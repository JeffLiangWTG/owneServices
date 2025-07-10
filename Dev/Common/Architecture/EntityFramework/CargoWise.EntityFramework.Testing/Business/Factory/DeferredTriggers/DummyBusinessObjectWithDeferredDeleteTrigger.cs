using System.Data;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.PK, typeof(IDeferTriggerOnDeleteConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Original)]
	public class DummyBizOWithDeferredDeleteTrigger : DummyBusinessObject
	{
		public const string StoredProc = "DummyDeleteStoredProc";
		public const string TriggerName = "TG_DummyDeleteTrigger";

		public DummyBizOWithDeferredDeleteTrigger(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
