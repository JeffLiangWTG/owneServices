using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class NctsPackageUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals(typeof(Business.NctsPackage), userControl.BindingSource.DataSourceType);
		}

		public void TestSequenceNumber()
		{
			var sequenceNumber = userControl.SequenceNumberCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>(sequenceNumber);
				AssertEquals("Binding", "B5_SequenceNumber", userControl.SequenceNumberCalcEdit.GetBindingMember());
			});
		}

		public void TestUnitCount()
		{
			var unitCount = userControl.UnitCountCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>(unitCount);
				AssertEquals("Binding", "B5_UnitCount", userControl.UnitCountCalcEdit.GetBindingMember());
			});
		}

		public void TestUnitType()
		{
			var unitType = userControl.UnitTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(unitType);
				AssertEquals("Binding", "B5_UnitType", userControl.UnitTypeDropEdit.GetBindingMember());
			});
		}

		public void TestMarksAndNumbersTextBox()
		{
			var marksAndNumbersTextBox = userControl.MarksAndNumbersTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(marksAndNumbersTextBox);
				AssertEquals("Binding", "B5_MarksAndNumbers", userControl.MarksAndNumbersTextBox.GetBindingMember());
			});
		}

		public void TestPackageIDTextBox()
		{
			var packageIDTextBox = userControl.PackageIDTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(packageIDTextBox);
				AssertEquals("Binding", "B5_PackageID", userControl.PackageIDTextBox.GetBindingMember());
			});
		}

		public void TestBrandTextBox()
		{
			var brandTextBox = userControl.BrandTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(brandTextBox);
				AssertEquals("Binding", "B5_Brand", userControl.BrandTextBox.GetBindingMember());
			});
		}

		public void TestModelTextBox()
		{
			var modelTextBox = userControl.ModelTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(modelTextBox);
				AssertEquals("Binding", "B5_Model", userControl.ModelTextBox.GetBindingMember());
			});
		}

		public void TestDifUnitCount()
		{
			var unitCount = userControl.DifUnitCountCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>(unitCount);
				AssertEquals("Binding", "PackDifference.B5_UnitCount", userControl.DifUnitCountCalcEdit.GetBindingMember());
			});
		}

		public void TestDifUnitType()
		{
			var unitType = userControl.DifUnitTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(unitType);
				AssertEquals("Binding", "PackDifference.B5_UnitType", userControl.DifUnitTypeDropEdit.GetBindingMember());
			});
		}

		public void TestDifMarksAndNumbersTextBox()
		{
			var marksAndNumbersTextBox = userControl.DifMarksAndNumbersTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(marksAndNumbersTextBox);
				AssertEquals("Binding", "PackDifference.B5_MarksAndNumbers", userControl.DifMarksAndNumbersTextBox.GetBindingMember());
			});
		}

		public void TestDifPackageIDTextBox()
		{
			var packageIDTextBox = userControl.DifMarksAndNumbersTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(packageIDTextBox);
				AssertEquals("Binding", "PackDifference.B5_PackageID", userControl.DifPackageIDTextBox.GetBindingMember());
			});
		}

		public void TestDifBrandTextBox()
		{
			var brandTextBox = userControl.DifBrandTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(brandTextBox);
				AssertEquals("Binding", "PackDifference.B5_Brand", userControl.DifBrandTextBox.GetBindingMember());
			});
		}

		public void TestDifModelTextBox()
		{
			var modelTextBox = userControl.DifModelTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(modelTextBox);
				AssertEquals("Binding", "PackDifference.B5_Model", userControl.DifModelTextBox.GetBindingMember());
			});
		}

		public void TestPlaceHolder1Label()
		{
			var control = userControl.PlaceHolder1Label;
			CombineAssertions(() =>
			{
				AssertType<ZLabel>("Type", control);
				AssertEquals(false, new LabelCaptionRenderProvider().GetLabelCaptionVisible(control));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new NctsPackageUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		NctsPackageUserControl userControl;
	}
}
