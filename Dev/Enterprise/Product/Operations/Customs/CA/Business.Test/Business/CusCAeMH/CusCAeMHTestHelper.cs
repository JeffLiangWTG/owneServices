using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHTestHelper : ConsolPluginTestHelper
	{
		public CusCAeMHTestHelper()
		{ }

		public CusCAeMHTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public CusCAeMHMaster MasterBill
		{
			get
			{
				if (fMasterBill == null)
				{
					fMasterBill = Factory.New<CusCAeMHMaster>();
					fMasterBill.BP_ParentID = Consol.PK;
					fMasterBill.BP_ParentTableCode = Consol.TablePrefix;
				}
				return fMasterBill;
			}
		}
		CusCAeMHMaster fMasterBill;

		public CusCAeMHHouse House
		{
			get
			{
				if (fHouse == null)
				{
					fHouse = MasterBill.HouseBills.AddNew();
					fHouse.BW_ParentID = Shipment.PK;
					fHouse.BW_ParentTableCode = Shipment.TablePrefix;
				}
				return fHouse;
			}
		}
		CusCAeMHHouse fHouse;
	}
}
