using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocVoyage : DocumentWrapper
	{
		DocVoyage(JobVoyage jobVoyage, BusinessObjectFactory factoryToWrap)
			: base(jobVoyage, factoryToWrap)
		{
		}

		public static DocVoyage New(JobVoyage jobVoyage, BusinessObjectFactory factoryToWrap)
		{
			if (jobVoyage == null)
			{
				return null;
			}
			else
			{
				return new DocVoyage(jobVoyage, factoryToWrap);
			}
		}

		JobVoyage JobVoyage
		{
			get { return (JobVoyage)WrappedObject; }
		}

		public override string ToString()
		{
			return "";
		}
		public ZString Code
		{
			get { return JobVoyage.JV_VoyageFlight; }
		}

		public ZString Description
		{
			get { return JobVoyage.JV_RV_NKVessel; }
		}

		public ZString VesselConsortiumCode => JobVoyage.JV_Calc_VesselConsortiumCode;

		public ZString AirSeaRoad
		{
			get { return JobVoyage.JV_AirSeaRoad; }
		}

		public DocOrganisation Line
		{
			get { return DocOrganisation.New(JobVoyage.Line, Factory); }
		}

		public DocVessel NKVessel
		{
			get { return DocVessel.New(JobVoyage.Vessel, Factory); }
		}

		public ZString VoyageFlight
		{
			get { return JobVoyage.JV_VoyageFlight; }
		}
	}
}
