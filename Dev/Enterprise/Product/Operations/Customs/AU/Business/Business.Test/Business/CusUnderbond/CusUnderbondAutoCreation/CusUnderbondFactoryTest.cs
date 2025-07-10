using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CusUnderbondFactoryTest : CMRSeaCargoDepotTestCase
	{
		#region GetOutturnLine

		const string House1 = "HOUSE1";
		const string House2 = "HOUSE2";

		const string Ocean1 = "OBL100";
		const string Ocean2 = "OBL200";

		const string Container1 = "CONT001";
		const string Container2 = "CONT002";

		public void TestGetOutturnLineBreakBulk()
		{
			CheckHouseOcean(CMRImportCargoTypes.Codes.BreakBulk);
		}

		public void TestGetOutturnLineBulk()
		{
			CheckHouseOcean(CMRImportCargoTypes.Codes.Bulk);
		}

		void CheckHouseOcean(ZString cargoType)
		{
			CusOutturnHeader header = GetOutturnHeader();
			CusOutturn outturn1 = header.Outturns.AddNew();
			CusOutturn outturn2 = header.Outturns.AddNew();

			outturn1.C5_CargoType = cargoType;
			outturn2.C5_CargoType = cargoType;

			outturn1.C5_MasterBill = Ocean1;
			outturn2.C5_MasterBill = Ocean2;

			outturn1.C5_HouseBill = House1;
			outturn2.C5_HouseBill = House2;

			AssertEquals(outturn1, UnderbondFactory.GetOutturnLine(header, cargoType, ZString.Empty, House1, Ocean1));
			AssertEquals(outturn2, UnderbondFactory.GetOutturnLine(header, cargoType, ZString.Empty, House2, Ocean2));
		}

		public void TestGetOutturnLineFCL()
		{
			CheckContainer(CMRImportCargoTypes.Codes.FullContainerLoad);
		}

		public void TestGetOutturnLineFCX()
		{
			CheckContainer(CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills);
		}

		void CheckContainer(ZString cargoType)
		{
			CusOutturnHeader header = GetOutturnHeader();
			CusOutturn outturn1 = header.Outturns.AddNew();
			CusOutturn outturn2 = header.Outturns.AddNew();

			outturn1.C5_CargoType = cargoType;
			outturn2.C5_CargoType = cargoType;

			outturn1.C5_ContainerNumber = Container1;
			outturn2.C5_ContainerNumber = Container2;

			AssertEquals(outturn1, UnderbondFactory.GetOutturnLine(header, cargoType, Container1, ZString.Empty, ZString.Empty));
			AssertEquals(outturn2, UnderbondFactory.GetOutturnLine(header, cargoType, Container2, ZString.Empty, ZString.Empty));
		}

		public void TestGetOutturnLineLCL()
		{
			CusOutturnHeader header = GetOutturnHeader();
			CusOutturn outturn1 = header.Outturns.AddNew();
			CusOutturn outturn2 = header.Outturns.AddNew();

			outturn1.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			outturn2.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;

			outturn1.C5_MasterBill = Ocean1;
			outturn2.C5_MasterBill = Ocean2;

			outturn1.C5_HouseBill = House1;
			outturn2.C5_HouseBill = House2;

			outturn1.C5_ContainerNumber = Container1;
			outturn2.C5_ContainerNumber = Container2;

			AssertEquals(outturn1, UnderbondFactory.GetOutturnLine(header, CMRImportCargoTypes.Codes.LessThanContainerLoad, Container1, House1, Ocean1));
			AssertEquals(outturn2, UnderbondFactory.GetOutturnLine(header, CMRImportCargoTypes.Codes.LessThanContainerLoad, Container2, House2, Ocean2));
		}

		protected CusOutturnHeader GetOutturnHeader()
		{
			return CusOutturnHeader.New(Factory);
		}

		#endregion

		#region GetOrCreateOutturnHeader

		public void TestGetOrCreateOutturnHeaderCreate()
		{
			CusOutturnHeader createdHeader = UnderbondFactory.GetOrCreateOutturnHeader(Factory, "123", "321", "FOO", true);

			AssertEquals("not in db", false, createdHeader.IsInDatabase);
			AssertEquals("correct lloyds", "123", createdHeader.C6_LloydsIMO);
			AssertEquals("correct voyage", "321", createdHeader.C6_VoyageNum);
			AssertEquals("correct premise", "FOO", createdHeader.C6_OutturningPremiseID);
		}

		public void TestGetOrCreateOutturnHeaderLoad()
		{
			CusOutturnHeader header = CusOutturnHeader.New(Factory);
			header.C6_LloydsIMO = "123";
			header.C6_VoyageNum = "321";
			header.C6_OutturningPremiseID = "FOO";

			Factory.Save();

			CusOutturnHeader loadedHeader = UnderbondFactory.GetOrCreateOutturnHeader(Factory, "123", "321", "FOO", true);
			AssertEquals("is in db", true, loadedHeader.IsInDatabase);
			AssertEquals("is header", header.PK, loadedHeader.PK);
		}

		#endregion

		#region TestMatchingGoodsReceiptLine

		public void TestMatchingGoodsRecieptLineBothExists()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			CusOutturn lCLOutturn = CreateLCLOutturn(header);
			CusOutturn goodsReciept = CreateGoodsRecieptOutturn(header);
			AssertEquals("GoodsReciept Line should match", goodsReciept, UnderbondFactory.GetOutturnLine(header, CMRImportCargoTypes.Codes.FullContainerLoad, OBL250805001_Container1, ZString.Empty, ZString.Empty));
			AssertEquals("GoodsReciept Line should match", goodsReciept, UnderbondFactory.GetOutturnLine(header, CMRImportCargoTypes.Codes.FullContainerLoad, OBL250805001_Container1, ZString.Empty, OBL250805001_OceanBillNum));
		}

		public void TestMatchingGoodsRecieptLineHouseStatusExists()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			CusOutturn lCLOutturn = CreateLCLOutturn(header);
			Assert("GoodsReciept Line should match", lCLOutturn != UnderbondFactory.GetOutturnLine(header, CMRImportCargoTypes.Codes.FullContainerLoad, OBL250805001_Container1, "", ""));
			Assert("GoodsReciept Line should match", lCLOutturn != UnderbondFactory.GetOutturnLine(header, CMRImportCargoTypes.Codes.FullContainerLoad, OBL250805001_Container1, "", OBL250805001_OceanBillNum));
		}

		public void TestMatchingGoodsRecieptLineNothingExists()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			AssertNull("GoodsReciept Line should not match", UnderbondFactory.GetOutturnLine(header, CMRImportCargoTypes.Codes.FullContainerLoad, OBL250805001_Container1, "", ""));
			AssertNull("GoodsReciept Line should not match", UnderbondFactory.GetOutturnLine(header, CMRImportCargoTypes.Codes.FullContainerLoad, OBL250805001_Container1, "", OBL250805001_OceanBillNum));
		}

		public void TestMatchingGoodsRecieptLineWithNoStatusMessages()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			CusOutturn goodsReciept = CreateGoodsRecieptOutturn(header);
			AssertEquals("GoodsReciept Line should match", goodsReciept, UnderbondFactory.GetOutturnLine(header, CMRImportCargoTypes.Codes.FullContainerLoad, OBL250805001_Container1, "", ""));
			AssertEquals("GoodsReciept Line should match", goodsReciept, UnderbondFactory.GetOutturnLine(header, CMRImportCargoTypes.Codes.FullContainerLoad, OBL250805001_Container1, "", OBL250805001_OceanBillNum));
		}

		#endregion

		#region Implementation

		protected CusOutturnHeader CreateOutturnHeader()
		{
			CusOutturnHeader result = Factory.New<CusOutturnHeader>();
			result.C6_OutturningPremiseID = "9914N";
			result.C6_VoyageNum = TestScenarioVoyageNum;
			result.C6_LloydsIMO = TestScenarioLloydsNumber;
			result.C6_ResponsiblePartyID = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			return result;
		}

		protected CusOutturn CreateLCLOutturn(CusOutturnHeader header)
		{
			CusOutturn result = header.Outturns.AddNew();
			result.C5_HouseBill = OBL250805001_HouseBill1;
			result.C5_ContainerNumber = OBL250805001_Container1;
			result.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			return result;
		}

		protected CusOutturn CreateGoodsRecieptOutturn(CusOutturnHeader header)
		{
			CusOutturn result = header.Outturns.AddNew();
			result.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			result.C5_ContainerNumber = OBL250805001_Container1;
			return result;
		}

		#region TestHelper

		class CusUnderbondFactoryTestHelper : CusUnderbondFactory
		{
		}

		#endregion

		protected CusUnderbondFactory UnderbondFactory
		{
			get
			{
				if (fUnderbondFactory == null)
				{
					fUnderbondFactory = new CusUnderbondFactoryTestHelper();
				}
				return fUnderbondFactory;
			}
		}
		CusUnderbondFactory fUnderbondFactory;

		#endregion
	}
}
