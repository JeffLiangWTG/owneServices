using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class CFSRecordCreatorTest : TestCaseWithFactory
	{
		#region Implementation

		protected const string TestLloydsNum = "8610033";
		protected const string TestPremiseID = "9914N";
		protected const string TestVoyageNum = "421S";

		protected const string TestContainerNumber1 = "CTRL0000011";
		protected const string TestHouseBill1 = "HB393029";
		protected const string TestOceanBill1 = "OBL20934802";

		protected CusOutturnHeader CreateOutturnHeader()
		{
			CusOutturnHeader header = Factory.New<CusOutturnHeader>();
			header.C6_LloydsIMO = TestLloydsNum;
			header.C6_VoyageNum = TestVoyageNum;
			header.C6_OutturningPremiseID = TestPremiseID;
			return header;
		}

		protected DepotCusOutturn AddFCXContainerLine(CusOutturnHeader header, CusUnderbond underbond, ZString containerNumber, ZString masterBill)
		{
			DepotCusOutturn result = AddContainerLine(header, underbond, containerNumber);
			result.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills;
			result.C5_MasterBill = masterBill;
			return result;
		}

		protected DepotCusOutturn AddContainerLine(CusOutturnHeader header, CusUnderbond underbond, ZString containerNumber)
		{
			DepotCusOutturn result = header.Outturns.AddNew();
			result.C5_ContainerNumber = containerNumber;
			result.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			if (underbond != null)
			{
				result.C5_C4_Underbond = underbond.PK;
			}
			return result;
		}

		protected DepotCusOutturn AddLCLLine(CusOutturnHeader header, CusUnderbond underbond, ZString containerNumber, ZString houseBill, ZString oceanBillNum)
		{
			DepotCusOutturn result = header.Outturns.AddNew();
			result.C5_ContainerNumber = containerNumber;
			result.C5_HouseBill = houseBill;
			result.C5_MasterBill = oceanBillNum;
			result.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			if (underbond != null)
			{
				result.C5_C4_Underbond = underbond.PK;
			}
			return result;
		}

		protected DepotCusOutturn AddBreakBulkLine(CusOutturnHeader header, CusUnderbond underbond, ZString houseBill, ZString oceanBill)
		{
			DepotCusOutturn result = header.Outturns.AddNew();
			result.C5_HouseBill = houseBill;
			result.C5_MasterBill = oceanBill;
			result.C5_CargoType = CMRImportCargoTypes.Codes.BreakBulk;
			result.C5_GoodsDescription = "FOO";
			result.C5_MarksAndNumbers = "BAR";
			result.C5_C4_Underbond = underbond.PK;
			if (underbond != null)
			{
				result.C5_C4_Underbond = underbond.PK;
			}
			return result;
		}

		protected ZString VesselFromLloyds(ZString lloydsNumber)
		{
			RefVessel vessel = RefVessel.LookupVesselByLloyds(lloydsNumber, Factory);
			return vessel.RV_Code;
		}

		#endregion
	}
}
