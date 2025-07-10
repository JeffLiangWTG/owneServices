using Enterprise.MasterFiles.Business;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public class UPECargoReportQueueLookups : UPEProcessQueueLookups
	{
		public UPECargoReportQueueLookups(UPECargoReportQueue parent) : base(parent)
		{
		}

		protected new UPECargoReportQueue Parent
		{
			get { return (UPECargoReportQueue)base.Parent; }
		}

		public CodeDescriptionPairList ShipmentTypeList
		{
			get { return new ShipmentTypeCodeDescriptionPairList(); }
		}

		#region Overrides

		public override GlbStaffCollection TaskAssignedTos
		{
			get { return CommercialQueueLookupsHelper.GetTaskAssignedToList(); }
		}

		protected override CodeDescriptionPairList GetCommercialQueueList()
		{
			return CommercialQueueLookupsHelper.GetQueueNameList();
		}

		protected override CodeDescriptionPairList GetCommercialStatusList()
		{
			return CommercialQueueLookupsHelper.GetReasonCodeList((CommercialQueueCodeDescriptionPairList)CommercialQueueList);
		}

		protected override CodeDescriptionPairList GetCommercialSubStatusList()
		{
			return CommercialQueueLookupsHelper.GetStatusCodeList();
		}

		#endregion

		#region Helpers

		protected override UPECustomsQueueLookupsHelper GetNewCustomsProcessQueueLookupsHelper()
		{
			return new UPECargoReportQueueLookupsHelper(Parent);
		}

		internal UPECommercialQueueLookupsHelper CommercialQueueLookupsHelper
		{
			get
			{
				if (fCommercialQueueLookupsHelper == null)
				{
					fCommercialQueueLookupsHelper = GetNewCommercialQueueLookupsHelper();
				}
				return fCommercialQueueLookupsHelper;
			}
		}

		protected virtual UPECommercialQueueLookupsHelper GetNewCommercialQueueLookupsHelper()
		{
			return new UPECommercialQueueLookupsHelper(Parent);
		}

		UPECommercialQueueLookupsHelper fCommercialQueueLookupsHelper;

		#endregion					
	}
}
