using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Module.AirCargo;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Module
{
	public abstract class UPEAirCargoCalloutFilterLookups : AUCustomsAirCargoLists
	{
		public UPEAirCargoCalloutFilterLookups(UPEAirCargoCalloutBaseFilterBusinessObject filterBizO)
			: base(filterBizO.Factory)
		{
			this.FilterBizO = filterBizO;
		}

		#region Code Description Pair Lists

		public DefaultQueueCodeDescriptionPairList QueueNames_List
		{
			get
			{
				if (fQueueNames_List == null)
				{
					fQueueNames_List = NewQueueNamesList();
				}
				return fQueueNames_List;
			}
		}
		DefaultQueueCodeDescriptionPairList fQueueNames_List;

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

		public RefUNLOCOCollection PortList
		{
			get
			{
				if (fPortList == null)
				{
					fPortList = new RefUNLOCOCollection(Factory);
				}
				return fPortList;
			}
		}
		RefUNLOCOCollection fPortList;

		public LocationCollection LocationList
		{
			get
			{
				if (fLocationList == null)
				{
					fLocationList = new LocationCollection(Factory);
				}
				return fLocationList;
			}
		}
		LocationCollection fLocationList;

		public RefServiceLevelCollection ServiceLevelList
		{
			get
			{
				if (fServiceLevelList == null)
				{
					fServiceLevelList = new RefServiceLevelCollection(Factory);
				}
				return fServiceLevelList;
			}
		}
		RefServiceLevelCollection fServiceLevelList;

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

		protected abstract DefaultQueueCodeDescriptionPairList NewQueueNamesList();
		protected readonly UPEAirCargoCalloutBaseFilterBusinessObject FilterBizO;
	}
}
