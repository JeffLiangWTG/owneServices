using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public abstract class NonPersistentProcessQueueLookups : ZLookups, IActiveProcessQueueLookups
	{
		public NonPersistentProcessQueueLookups(NonPersistentProcessQueue queue)
			: base(queue)
		{
		}

		#region IActiveProcessQueueLookups Members

		public CodeDescriptionPairList QueueList
		{
			get
			{
				if (fQueueList == null)
				{
					fQueueList = LookupsHelper.GetQueueNameList();
				}
				return fQueueList;
			}
		}

		public CodeDescriptionPairList StatusList
		{
			get { return LookupsHelper.GetReasonCodeList((DefaultQueueCodeDescriptionPairList)QueueList); }
		}

		public CodeDescriptionPairList SubStatusList
		{
			get { return LookupsHelper.GetStatusCodeList(); }
		}

		public GlbStaffCollection TaskAssignedToList
		{
			get { return LookupsHelper.GetTaskAssignedToList(); }
		}

		CodeDescriptionPairList fQueueList;

		#endregion

		protected new NonPersistentProcessQueue Parent
		{
			get { return (NonPersistentProcessQueue)base.Parent; }
		}

		internal UPEProcessQueueLookupsHelper LookupsHelper
		{
			get
			{
				if (fLookupsHelper == null)
				{
					fLookupsHelper = GetNewUPEProcessQueueLookupsHelper();
				}
				return fLookupsHelper;
			}
		}

		protected abstract UPEProcessQueueLookupsHelper GetNewUPEProcessQueueLookupsHelper();

		UPEProcessQueueLookupsHelper fLookupsHelper;
	}
}
