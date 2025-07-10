using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	class NctsArrivalUserControlTest : TestCaseWithFactory
	{
		public void TestFrenchSpecificControlsVisibility()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			using (var form = new ZForm(header))
			{
				form.Controls.Add(new NctsArrivalUserControl());
				form.Show();
				var detailedStatusDropEdit = (ZDropEdit)form.Controls.Find("DetailedStatusDropEdit", true).First();
				Assert(detailedStatusDropEdit.Visible);
			}
		}

		public void TestAgreedLocationOfGoodsCodeCodeTextBoxVisibility()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			using (var form = new ZForm(header))
			{
				form.Controls.Add(new NctsArrivalUserControl());
				form.Show();
				var agreedLocationOfGoodsCodeCodeTextBox = (ZTextBox)form.Controls.Find("AgreedLocationOfGoodsCodeCodeTextBox", true).First();
				Assert(!agreedLocationOfGoodsCodeCodeTextBox.Visible);
			}
		}

		public void TestAgreedLocationOfGoodsCodeCodeFindBox()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			using (var form = new ZForm(header))
			{
				form.Controls.Add(new NctsArrivalUserControl());
				form.Show();
				var codeFindBox = (ZCodeFindBox)form.Controls.Find("AgreedLocationOfGoodsCodeCodeFindBox", true).First();
				var codeTextBox = (ZTextBox)form.Controls.Find("AgreedLocationOfGoodsCodeCodeTextBox", true).First();
				Assert(codeFindBox.Visible);
				AssertEquals("ArrivalMovementHeader.BM_LocationOfGoodsCode", codeFindBox.BindTo);
				AssertSame(ModuleIDs.Customs.CusAuthorisations, codeFindBox.ModuleID);
				AssertEquals(codeTextBox.Location, codeFindBox.Location);
			}
		}
	}
}
