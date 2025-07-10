using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class WorkQueueTagRule : TagRule
	{
		public WorkQueueTagRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			TGR_ActionType = TagRuleActionTypeList.Codes.MaintainMagnitude;
			TGR_IsSystem = ZBool.True;
		}

		protected override TagRuleValidation GetNewValidation()
		{
			return new WorkQueueTagRuleValidation(this);
		}

		#endregion

		#region TagRule Overrides

		protected override bool CanDeleteSystemRule
		{
			get { return TagTemplate.Magnitude is WorkQueue; }
		}

		#endregion
	}
}
