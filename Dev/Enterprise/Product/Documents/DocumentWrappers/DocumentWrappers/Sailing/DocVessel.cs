using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocVessel : DocBaseWrapper
	{
		DocVessel(RefVessel refVessel, BusinessObjectFactory factoryToWrap)
			: base(refVessel, factoryToWrap)
		{
		}

		public static DocVessel New(RefVessel refVessel, BusinessObjectFactory factoryToWrap)
		{
			if (refVessel == null)
			{
				return null;
			}
			else
			{
				return new DocVessel(refVessel, factoryToWrap);
			}
		}

		public static DocVessel New(BusinessObjectFactory factory, ZString vesselName)
		{
			return New(factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, vesselName), factory);
		}

		public override string ToString()
		{
			return Code;
		}

		RefVessel RefVessel
		{
			get { return (RefVessel)WrappedObject; }
		}

		public ZString Code
		{
			get { return RefVessel.RV_Code; }
		}

		public ZBool IsActive
		{
			get { return RefVessel.RV_IsActive; }
		}

		public ZString LloydsNumber
		{
			get { return RefVessel.RV_LloydsNumber; }
		}

		public ZInt NetRegisterTon
		{
			get { return RefVessel.RV_NetRegisterTon; }
		}

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(RefVessel.Header, Factory); }
		}

		public DocCountry Country
		{
			get { return DocCountry.New(RefVessel.CountryOfReg, Factory); }
		}

		public ZString VesselType
		{
			get { return RefVessel.RV_VesselType; }
		}

		protected override ZString DocManagerUniqueID
		{
			get { return Code; }
		}
	}
}
