using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCATestHelper : ConsolPluginTestHelper
	{
		public BusinessObjectFactory HelperFactory
		{
			get { return Factory; }
		}

		public CusSCAOceanBill OceanBill
		{
			get
			{
				if (fOceanBill == null)
				{
					fOceanBill = Factory.New<CusSCAOceanBill>();
					fOceanBill.CB_ParentId = Consol.PK;
					fOceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
					fOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea;
				}
				return fOceanBill;
			}
		}
		CusSCAOceanBill fOceanBill;

		public CusSCAHouse House
		{
			get
			{
				if (fHouse == null)
				{
					fHouse = OceanBill.HouseBills.AddNew();
					fHouse.CA_JS = Shipment.PK;
					OceanBill.EnableAndSynchronise();
					fHouse.EnableAndSynchronise();
				}
				return fHouse;
			}
		}
		CusSCAHouse fHouse;

		public CusSCAPivot PackLine1
		{
			get
			{
				if (fPackLine1 == null)
				{
					fPackLine1 = House.PackLines.AddNew();
				}
				return fPackLine1;
			}
		}
		CusSCAPivot fPackLine1;

		public CusSCAPivot PackLine2
		{
			get
			{
				if (fPackLine2 == null)
				{
					fPackLine2 = House.PackLines.AddNew();
				}
				return fPackLine2;
			}
		}
		CusSCAPivot fPackLine2;

		public CusSCAPivot PackLine3
		{
			get
			{
				if (fPackLine3 == null)
				{
					fPackLine3 = House.PackLines.AddNew();
				}
				return fPackLine3;
			}
		}
		CusSCAPivot fPackLine3;

		public CusSCAPivot PackLine4
		{
			get
			{
				if (fPackLine4 == null)
				{
					fPackLine4 = House.PackLines.AddNew();
				}
				return fPackLine4;
			}
		}
		CusSCAPivot fPackLine4;
	}
}
