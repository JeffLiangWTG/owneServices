using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class ViewComponentChangeLogLookups : AutoViewComponentChangeLogLookups
	{
		public ViewComponentChangeLogLookups(AutoViewComponentChangeLog parent)
			: base(parent)
		{
		}

		public ICodeDescriptionPairList TransferTypes
		{
			get { return Factory.GetCachedValue<TransferTypeList>(); }
		}

		public ICodeDescriptionPairList CcrStatusCodes
		{
			get { return Factory.GetCachedValue<ConstraintStatusList>(); }
		}

		public ICodeDescriptionPairList WorkflowStatusCodes
		{
			get { return Factory.GetCachedValue<WorkflowStatusList>(); }
		}

		public ICodeDescriptionPairList DeferralReasonsList
		{
			get { return Factory.GetCachedValue("DeferralReasonsList", () => BMSRegistry.Instance.DeferralReasons.Value); }
		}

		public IBusinessObjectCollection Components
		{
			get { return Factory.GetCachedValue("ViewComponentChangeLogLookups.Components", () => new BMComponentCollection(Factory)); }
		}
	}
}
