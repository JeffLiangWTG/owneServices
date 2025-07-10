using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7PackDetailsFieldsUserControl))]
	class EUH7PackDetailsFieldsUserControlTest : TestCaseWithFactory
	{
		public void TestGoodsDescriptionTextBox()
		{
			using (var control = new EUH7PackDetailsFieldsUserControl())
			{
				control.Show();

				var goodsDescriptionTextBox = control.FindSingle<ZTextBox>("GoodsDescriptionTextBox");

				AssertEquals("BindingMember", "APA_GoodsDescription", goodsDescriptionTextBox.GetBindingMember());
			}
		}

		public void TestPackUQDropEdit()
		{
			using (var control = new EUH7PackDetailsFieldsUserControl())
			{
				control.Show();

				var packUQDropEdit = control.FindSingle<ZDropEdit>("PackUQDropEdit");

				AssertEquals("BindingMember", "APA_PackUQ", packUQDropEdit.GetBindingMember());
			}
		}

		public void TestPackQtyCalcEdit()
		{
			using (var control = new EUH7PackDetailsFieldsUserControl())
			{
				control.Show();

				var packQtyCalcEdit = control.FindSingle<ZCalcEdit>("PackQtyCalcEdit");

				AssertEquals("BinidngMember", "APA_PackQty", packQtyCalcEdit.GetBindingMember());
			}
		}

		public void TestMarksAndNumbersTextBox()
		{
			using (var control = new EUH7PackDetailsFieldsUserControl())
			{
				control.Show();

				var marksAndNumbersTextBox = control.FindSingle<ZTextBox>("MarksAndNumbersTextBox");

				AssertEquals("BindingMember", "APA_MarksAndNumbers", marksAndNumbersTextBox.GetBindingMember());
			}
		}

		public void TestWeightCalcDropEdit()
		{
			using (var control = new EUH7PackDetailsFieldsUserControl())
			{
				control.Show();

				var weightCalcDropEdit = control.FindSingle<ZCalcDropEdit>("WeightCalcDropEdit");

				CombineAssertions("BindingMember", () =>
				{
					AssertEquals("BindToAmount", "APA_Weight", weightCalcDropEdit.BindToAmount);
					AssertEquals("BindToUnit", "APA_WeightUQ", weightCalcDropEdit.BindToUnit);
				});
			}
		}

		public void TestVolumeCalcDropEdit()
		{
			using (var control = new EUH7PackDetailsFieldsUserControl())
			{
				control.Show();

				var volumeCalcDropEdit = control.FindSingle<ZCalcDropEdit>("VolumeCalcDropEdit");

				CombineAssertions("BindingMember", () =>
				{
					AssertEquals("BindToAmount", "APA_Volume", volumeCalcDropEdit.BindToAmount);
					AssertEquals("BindToUnit", "APA_VolumeUQ", volumeCalcDropEdit.BindToUnit);
				});
			}
		}
	}
}
