using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CFSLoadListConsolCreator : CFSRecordCreator
	{
		public CFSLoadListConsolCreator(DepotCusOutturn outturn, BusinessObjectFactory factory)
			: base(outturn, factory)
		{
			fConsol = CreateConsol(outturn);
		}

		public CFSLoadListConsol Consol
		{
			get { return fConsol; }
		}

		#region Implementation

		CFSLoadListConsol CreateConsol(DepotCusOutturn outturn)
		{
			CFSLoadListConsol result;
			CusOutturnHeader header = outturn.Header;

			if (header != null)
			{
				result = Factory.New<CFSLoadListConsol>();

				Transport transport = result.Transports[0];
				transport.JW_IsLinked = false;
				transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
				transport.JW_Vessel = VesselFromLloyds(header.C6_LloydsIMO);
				transport.JW_VoyageFlight = header.C6_VoyageNum;

				if (GlbBranch.CurrentBranch.OrgProxy != null)
				{
					transport.JW_RL_NKDiscPort = GlbBranch.CurrentBranch.OrgProxy.OH_RL_NKClosestPort;
				}
			}
			else
			{
				result = null;
			}

			return result;
		}

		#endregion

		readonly CFSLoadListConsol fConsol;
	}
}
