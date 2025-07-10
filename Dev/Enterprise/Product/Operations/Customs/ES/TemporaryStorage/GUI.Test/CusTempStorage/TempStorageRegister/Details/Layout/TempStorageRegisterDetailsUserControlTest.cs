using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	class TempStorageRegisterDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestGrossWeightUQTextBox()
		{
			CombineAssertions(() =>
			{
				var assertedControl = control.AssertContainsControl<ZTextBox>("GrossWeightUQTextBox", x => x
					.WithBindTo("CusTempStorageRegLines." + nameof(CusTempStorageRegLine.SRL_GrossWeightUQ))
				);
				AssertSame(assertedControl, control.GrossWeightUQTextBox);
			});
		}

		public void TestGoodsOwnerIdentifierTextBox()
		{
			CombineAssertions(() =>
			{
				var assertedControl = control.AssertContainsControl<ZTextBox>("GoodsOwnerIdentifierTextBox", x => x
					.WithBindTo("CusTempStorageRegLines." + nameof(CusTempStorageRegLine.SRL_GoodsOwnerIdentifier))
				);
				AssertSame(assertedControl, control.GoodsOwnerIdentifierTextBox);
			});
		}

		[RequiresSTA]
		public void TestPackageMarksTextBox()
		{
			CombineAssertions(() =>
			{
				var assertedControl = control.AssertContainsControl<ZTextBox>("PackageMarksTextBox", x => x
					.WithBindTo("CusTempStorageRegLines." + nameof(CusTempStorageRegLine.SRL_PackageMarks))
				);
				AssertSame(assertedControl, control.PackageMarksTextBox);
			});
		}

		public void TestUnionStatusDropEdit()
		{
			CombineAssertions(() =>
			{
				var assertedControl = control.AssertContainsControl<ZDropEdit>("UnionStatusDropEdit", x => x
					.WithBindTo("CusTempStorageRegLines." + nameof(CusTempStorageRegLine.SRL_UnionStatus))
				);
				AssertSame(assertedControl, control.UnionStatusDropEdit);
			});
		}

		public void TestGrossWeightRemainingCalcEdit()
		{
			CombineAssertions(() =>
			{
				var assertedControl = control.AssertContainsControl<ZCalcEdit>("GrossWeightRemainingCalcEdit", x => x
					.WithBindTo("CusTempStorageRegLines." + nameof(CusTempStorageRegLine.GrossWeightRemainingCalculated))
				);
				AssertSame(assertedControl, control.GrossWeightRemainingCalcEdit);
			});
		}

		public void TestPackagesRemainingCalcEdit()
		{
			CombineAssertions(() =>
			{
				var assertedControl = control.AssertContainsControl<ZCalcEdit>("PackagesRemainingCalcEdit", x => x
					.WithBindTo("CusTempStorageRegLines." + nameof(CusTempStorageRegLine.PackagesRemainingCalculated))
				);
				AssertSame(assertedControl, control.PackagesRemainingCalcEdit);
			});
		}

		public void TestLineNumberCalcEdit()
		{
			CombineAssertions(() =>
			{
				var assertedControl = control.AssertContainsControl<ZCalcEdit>("LineNumberCalcEdit", x => x
					.WithBindTo("CusTempStorageRegLines." + nameof(CusTempStorageRegLine.SRL_LineNumber))
				);
				AssertSame(assertedControl, control.LineNumberCalcEdit);
			});
		}

		public void TestPackageTypeDropEdit()
		{
			CombineAssertions(() =>
			{
				var assertedControl = control.AssertContainsControl<ZDropEdit>("PackageTypeDropEdit", x => x
					.WithBindTo("CusTempStorageRegLines." + nameof(CusTempStorageRegLine.SRL_PackageType))
				);
				AssertSame(assertedControl, control.PackageTypeDropEdit);
			});
		}

		public void TestLimitDateEdit()
		{
			CombineAssertions(() =>
			{
				var assertedControl = control.AssertContainsControl<ZDateEdit>("LimitDateEdit", x => x
					.WithBindTo("CusTempStorageRegLines." + nameof(CusTempStorageRegLine.SRL_LimitDate))
				);
				AssertSame(assertedControl, control.LimitDateEdit);
			});
		}

		public void TestGoodsDescriptionTextBox()
		{
			CombineAssertions(() =>
			{
				var assertedControl = control.AssertContainsControl<ZTextBox>("GoodsDescriptionTextBox", x => x
					.WithBindTo("CusTempStorageRegLines." + nameof(CusTempStorageRegLine.SRL_GoodsDescription))
				);
				AssertSame(assertedControl, control.GoodsDescriptionTextBox);
			});
		}

		public void TestLocationOfGoodsTextBox()
		{
			CombineAssertions(() =>
			{
				var assertedControl = control.AssertContainsControl<ZTextBox>("LocationOfGoodsTextBox", x => x
					.WithBindTo("CusTempStorageRegLines." + nameof(CusTempStorageRegLine.SRL_LocationOfGoods))
				);
				AssertSame(assertedControl, control.LocationOfGoodsTextBox);
			});
		}

		public void TestOwnerReferenceNumberTextBox()
		{
			CombineAssertions(() =>
			{
				var assertedControl = control.AssertContainsControl<ZTextBox>("OwnerReferenceNumberTextBox", x => x
					.WithBindTo("CusTempStorageRegLines." + nameof(CusTempStorageRegLine.SRL_OwnerReference))
				);
				AssertSame(assertedControl, control.OwnerReferenceNumberTextBox);
			});
		}

		public void TestBondAmountRemainingCalculatedCalcEdit()
		{
			CombineAssertions(() =>
			{
				var assertedControl = control.AssertContainsControl<ZCalcEdit>("BondAmountRemainingCalculatedCalcEdit", x => x
					.WithBindTo("CusTempStorageRegLines." + nameof(CusTempStorageRegLine.BondAmountRemainingCalculated))
				);
				AssertSame(assertedControl, control.BondAmountRemainingCalculatedCalcEdit);
			});
		}

		public void TestCustomsStatusDropEdit()
		{
			CombineAssertions(() =>
			{
				var assertedControl = control.AssertContainsControl<ZDropEdit>("CustomsStatusDropEdit", x => x
					.WithBindTo("CusTempStorageRegLines." + nameof(CusTempStorageRegLine.SRL_CustomsStatus))
				);
				AssertSame(assertedControl, control.CustomsStatusDropEdit);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TempStorageRegisterDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TempStorageRegisterDetailsUserControl control;
	}
}
