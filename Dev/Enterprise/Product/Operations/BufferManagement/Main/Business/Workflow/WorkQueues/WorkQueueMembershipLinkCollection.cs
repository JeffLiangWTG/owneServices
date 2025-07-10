using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class WorkQueueMembershipLinkCollection : ActiveBusinessObjectCollection<WorkQueueMembershipLink>
	{
		public WorkQueueMembershipLinkCollection(WorkQueue queue)
			: base(queue.Factory, queue, new ZQuery(TagLinkSchema.TGL_ParentTableCode, SQLComparisonOperator.NotEqual, ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(TagRuleSchema.Constants.TableName)), TagLinkSchema.TGL_TGM_Magnitude)
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
