using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManTranHeadLookups : Customs.Business.CusSeaManTranHeadLookups
	{
		public CusSeaManTranHeadLookups(AutoCusSeaManTranHead parent)
			: base(parent)
		{
			this.head = parent;
		}
		readonly AutoCusSeaManTranHead head;

		protected CodeDescriptionPairList GetNewImpendingArrivalStatusList()
		{
			return new CMRBaseStatuses();
		}

		public CodeDescriptionPairList ImpendingArrivalStatusList
		{
			get
			{
				if (fImpendingArrivalStatusList == null)
				{
					fImpendingArrivalStatusList = GetNewImpendingArrivalStatusList();
				}

				return fImpendingArrivalStatusList;
			}
		}
		CodeDescriptionPairList fImpendingArrivalStatusList;

		#region ImportManifest

		#region Arrival Ports

		public CodeDescriptionPairList AllArrivalPortsWithAllValue
		{
			get
			{
				CodeDescriptionPairList arrivalPorts = AllArrivalPorts;
				arrivalPorts.AddPair("ALL", "All Arrival Ports");

				return arrivalPorts;
			}
		}

		#endregion

		#region Selected Port

		public ZString SelectedPort
		{
			get { return selectedPort; }
			set
			{
				selectedPort = value;
			}
		}
		ZString selectedPort = "ALL";

		#endregion

		public RefVesselCollection VesselNames => Factory.GetCachedValue("CusSeaManTranHeadLookups.Vessels_" + head.BT_VesselName + "_" + head.BT_LloydsIMO, delegate
		{
			RefVesselCollection vesselNames = new RefVesselCollection(Factory);
			vesselNames.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Vessel Name", "Property", head.BT_VesselName));
			vesselNames.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Lloyds Number", "Property", head.BT_LloydsIMO));
			return vesselNames;
		});

		#endregion
	}
}
