using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public class WorkflowInfo
	{
		public ICodeDescriptionDataObject EventType { get; set; }
		public ZString EventReference { get; set; }
		public ICodeNameDataObject EventUser { get; set; }
		public ICodeNameDataObject EventBranch { get; set; }
		public ICodeNameDataObject EventDepartment { get; set; }
		public ICodeDescriptionDataObject ActionPurpose { get; set; }
		public ZString TriggerDescription { get; set; }
		public ZInt TriggerCount { get; set; }
		public ZDateTimeOffset TriggerDate { get; set; }
		public ZString TriggerReference { get; set; }
		public TriggerType TriggerType { get; set; }
		public IEnumerable<RecipientRoleDetail> RecipientRoles { get; set; }
	}
}
