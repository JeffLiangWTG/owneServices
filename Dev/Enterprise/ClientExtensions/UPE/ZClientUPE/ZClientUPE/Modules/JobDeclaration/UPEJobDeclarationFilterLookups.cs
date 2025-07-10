using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Module
{
	public class UPEJobDeclarationFilterLookups : JobDeclarationFilterLookups
	{
		public UPEJobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		#region Code Description Pair Lists

		public DefaultQueueCodeDescriptionPairList QueueNames_List
		{
			get { return new DeclarationQueueCodeDescriptionPairList(); }
		}

		public CodeDescriptionPairList ZoneNameList
		{
			get { return new PostcodeZoneCodeDescriptionPairList(Factory); }
		}

		#endregion

		#region FindBox Lists

		public GlbStaffCollection TaskAssignedToStaff_List
		{
			get
			{
				if (fTaskAssignedToStaff_List == null)
				{
					fTaskAssignedToStaff_List = new GlbStaffCollection(Factory);
				}
				return fTaskAssignedToStaff_List;
			}
		}
		GlbStaffCollection fTaskAssignedToStaff_List;

		public OrgDebtorGroupCollection AccountClassList
		{
			get
			{
				if (fAccountClassList == null)
				{
					fAccountClassList = new OrgDebtorGroupCollection(Factory);
				}
				return fAccountClassList;
			}
		}
		OrgDebtorGroupCollection fAccountClassList;

		#endregion
	}
}
