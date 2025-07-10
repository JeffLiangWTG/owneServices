using Enterprise.MasterFiles.Business;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public abstract class UPEProcessQueueLookups : ProcessQueueLookups
	{
		public UPEProcessQueueLookups(UPEProcessQueue parent)
			: base(parent)
		{
		}

		#region Overrides

		public override GlbStaffCollection CustomsTaskAssignedTos
		{
			get { return CustomsQueueLookupsHelper.GetTaskAssignedToList(); }
		}

		protected override bool CustomsStatusListShouldBeCached
		{
			get { return false; }
		}

		protected override bool CustomsSubStatusListShouldBeCached
		{
			get { return false; }
		}

		protected override bool CommercialStatusListShouldBeCached
		{
			get { return false; }
		}

		protected override bool CommercialSubStatusListShouldBeCached
		{
			get { return false; }
		}

		protected override CodeDescriptionPairList GetCustomsQueueList()
		{
			return CustomsQueueLookupsHelper.GetQueueNameList();
		}

		protected override CodeDescriptionPairList GetCustomsStatusList()
		{
			return CustomsQueueLookupsHelper.GetReasonCodeList((CustomsQueueCodeDescriptionPairList)CustomsQueueList);
		}

		protected override CodeDescriptionPairList GetCustomsSubStatusList()
		{
			return CustomsQueueLookupsHelper.GetStatusCodeList();
		}

		#endregion

		#region Helpers

		internal UPECustomsQueueLookupsHelper CustomsQueueLookupsHelper
		{
			get
			{
				if (fCustomsQueueLookupsHelper == null)
				{
					fCustomsQueueLookupsHelper = GetNewCustomsProcessQueueLookupsHelper();
				}
				return fCustomsQueueLookupsHelper;
			}
		}

		protected abstract UPECustomsQueueLookupsHelper GetNewCustomsProcessQueueLookupsHelper();
		UPECustomsQueueLookupsHelper fCustomsQueueLookupsHelper;

		#endregion

		protected new ProcessQueue Parent
		{
			get { return (ProcessQueue)base.Parent; }
		}
	}
}
