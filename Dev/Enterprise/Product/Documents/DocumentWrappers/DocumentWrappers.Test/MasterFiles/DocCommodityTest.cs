using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocCommodity))]
	public class DocCommodityTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocCommodity.New(Commodity, Factory)
			};
		}

		public void TestNMFCClass()
		{
			AssertEquals(ZString.Empty, CommodityWrapper.NMFCClass);

			RefNMFC nMFC = Factory.New<RefNMFC>();
			nMFC.FN_Class = "45";
			nMFC.FN_ItemNo = "23";
			Commodity.RH_FN_NKNMFC = "23|45";

			AssertEquals("45", CommodityWrapper.NMFCClass);
		}

		public void TestCode()
		{
			Commodity.RH_Code = "TST";
			AssertEquals("TST", CommodityWrapper.Code);
		}

		public void TestDescription()
		{
			Commodity.RH_Description = "TEST DESCRIPTION";
			AssertEquals("TEST DESCRIPTION", CommodityWrapper.Description);
		}

		public void TestIsActive()
		{
			Commodity.RH_IsActive = ZBool.True;
			Assert(CommodityWrapper.IsActive);
			Commodity.RH_IsActive = ZBool.False;
			AssertEquals(ZBool.False, CommodityWrapper.IsActive);
		}

		public void TestIsFlammable()
		{
			Commodity.RH_IsFlammable = ZBool.True;
			Assert(CommodityWrapper.IsFlammable);
			Commodity.RH_IsFlammable = ZBool.False;
			AssertEquals(ZBool.False, CommodityWrapper.IsFlammable);
		}

		public void TestIsHazardous()
		{
			Commodity.RH_IsHazardous = ZBool.True;
			Assert(CommodityWrapper.IsHazardous);
			Commodity.RH_IsHazardous = ZBool.False;
			AssertEquals(ZBool.False, CommodityWrapper.IsHazardous);
		}

		public void TestIsPerishable()
		{
			Commodity.RH_IsPerishable = ZBool.True;
			Assert(CommodityWrapper.IsPerishable);
			Commodity.RH_IsPerishable = ZBool.False;
			AssertEquals(ZBool.False, CommodityWrapper.IsPerishable);
		}

		public void TestIsTimber()
		{
			Commodity.RH_IsTimber = ZBool.True;
			Assert(CommodityWrapper.IsTimber);
			Commodity.RH_IsTimber = ZBool.False;
			AssertEquals(ZBool.False, CommodityWrapper.IsTimber);
		}

		#region Implementation

		protected override void SetUp()
		{
			Commodity = Factory.New<RefCommodityCode>();
			CommodityWrapper = DocCommodity.New(Commodity, Factory);
			AssertNotNull("PreCondition: Valid DocCommodity", CommodityWrapper);

			base.SetUp();
		}

		DocCommodity CommodityWrapper;
		RefCommodityCode Commodity;

		#endregion
	}
}
