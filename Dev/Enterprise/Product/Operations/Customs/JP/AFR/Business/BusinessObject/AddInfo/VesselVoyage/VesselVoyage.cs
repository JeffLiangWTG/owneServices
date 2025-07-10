using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class VesselVoyage : AutoVesselVoyage
	{
		public VesselVoyage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString JP_PortOfLoadingCode
		{
			get { return base.JP_PortOfLoadingCode; }
			set
			{
				base.JP_PortOfLoadingCode = value;
				JP_PortOfLoadingName = Loading?.RL_PortName ?? ZString.Empty;
			}
		}

		public RefUNLOCO Loading => (RefUNLOCO)Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, JP_PortOfLoadingCode);
	}
}
