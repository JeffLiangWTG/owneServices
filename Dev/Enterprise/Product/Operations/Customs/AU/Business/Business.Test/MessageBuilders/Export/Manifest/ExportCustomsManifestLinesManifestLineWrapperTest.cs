using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ExportCustomsManifestLinesManifestLineWrapperTest : TestCaseWithFactory
	{
		public void TestGoodsOwner()
		{
			AssertEquals("GoodsOwner", ZString.Empty, wrapper.GoodsOwner);
			line.EL_GoodsOwner = "Goods Owner";
			AssertEquals("GoodsOwner", line.EL_GoodsOwner, wrapper.GoodsOwner);
			line.EL_OH_Owner = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			AssertEquals("GoodsOwner", line.Owner.OH_FullName, wrapper.GoodsOwner);
		}

		public void TestAirWayBill()
		{
			AssertEquals("AirWayBill", ZString.Empty, wrapper.AirWaybillNumber);
			line.EL_AirWayBill = "23352334236";
			AssertEquals("AirWayBill", "23352334236", wrapper.AirWaybillNumber);
		}

		public void TestGoodsOwnerPartyID()
		{
			AssertEquals("GoodsOwner", ZString.Empty, wrapper.GoodsOwnerPartyID);
			line.EL_GoodsOwnerPartyID = "Goods Owner ID";
			AssertEquals("GoodsOwner", line.EL_GoodsOwnerPartyID, wrapper.GoodsOwnerPartyID);
			line.EL_OH_Owner = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			line.Owner.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "12345");

			AssertEquals("GoodsOwner", line.Owner.LocalBusinessRegNo, wrapper.GoodsOwnerPartyID);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			line = header.Lines.AddNew();
			wrapper = new ExportCustomsManifestLinesManifestLineWrapper(line, line.EL_LineNo);
		}

		ExportCustomsManifestLines line;
		ExportCustomsManifestLinesManifestLineWrapper wrapper;

		#endregion

	}
}
