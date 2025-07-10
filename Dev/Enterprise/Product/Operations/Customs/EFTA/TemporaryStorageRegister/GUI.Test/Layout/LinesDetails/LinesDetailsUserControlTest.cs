using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Testing;

[TestedType(typeof(LinesDetailsUserControl))]
sealed class LinesDetailsUserControlTest : TestCaseWithFactory
{
	public void TestControls() => CombineAssertions(() =>
	{
		using var control = new LinesDetailsUserControl();
		_ = control.AssertContainsControl<ZDropEdit>(nameof(control.UnionStatusDropEdit), x => x.WithBindTo("CusTempStorageRegLines.SRL_UnionStatus"));
		_ = control.AssertContainsControl<ZCalcEdit>(nameof(control.PackagesRemainingCalcEdit), x => x.WithBindTo("CusTempStorageRegLines.SRL_PackagesRemaining"));
		_ = control.AssertContainsControl<ZCalcEdit>(nameof(control.LineNumberCalcEdit), x => x.WithBindTo("CusTempStorageRegLines.SRL_LineNumber"));
		_ = control.AssertContainsControl<ZDropEdit>(nameof(control.CustomsStatusDropEdit), x => x.WithBindTo("CusTempStorageRegLines.SRL_CustomsStatus"));
		_ = control.AssertContainsControl<ZDropEdit>(nameof(control.PackageTypeDropEdit), x => x.WithBindTo("CusTempStorageRegLines.SRL_PackageType"));
		_ = control.AssertContainsControl<ZDropEdit>(nameof(control.OwnerReferenceTypeDropEdit), x => x.WithBindTo("CusTempStorageRegLines.SRL_OwnerReferenceType"));
		_ = control.AssertContainsControl<ZDateEdit>(nameof(control.LimitDateEdit), x => x.WithBindTo("CusTempStorageRegLines.SRL_LimitDate"));
		_ = control.AssertContainsControl<ZUserControl>(nameof(control.CustodianEORIBranchUserControl));
		_ = control.AssertContainsControl<ZUserControl>(nameof(control.DisposalEntitledTraderEORIBranchUserControl));
		_ = control.AssertContainsControl<ZTextBox>(nameof(control.GoodsDescriptionTextBox), x => x.WithBindTo("CusTempStorageRegLines.SRL_GoodsDescription"));
		_ = control.AssertContainsControl<ZTextBox>(nameof(control.LocationofGoodsTextBox), x => x.WithBindTo("CusTempStorageRegLines.SRL_LocationOfGoods"));
		_ = control.AssertContainsControl<ZTextBox>(nameof(control.OwnerReferenceNumberTextBox), x => x.WithBindTo("CusTempStorageRegLines.SRL_OwnerReference"));
	});

	public void TestLineNumberCalcEditDecimals() => CombineAssertions(() =>
	{
		using var control = new LinesDetailsUserControl();
		var lineNumberCalcEdit = control.FindSingle<ZCalcEdit>(nameof(control.LineNumberCalcEdit));

		AssertEquals("Decimal Places", 0, lineNumberCalcEdit.DecimalPlaces);
		AssertEquals("Decimals", 0, lineNumberCalcEdit.Decimals);
	});

	public void TestTextBoxesNormalCasing()
	{
		using var control = new LinesDetailsUserControl();

		CombineAssertions(() =>
		{
			AssertTextBoxNormalCasing(nameof(control.GoodsDescriptionTextBox));
			AssertTextBoxNormalCasing(nameof(control.OwnerReferenceNumberTextBox));
		});

		void AssertTextBoxNormalCasing(string textBoxName)
		{
			var textBox = control.FindSingle<ZTextBox>(textBoxName);

			AssertEquals($"{textBoxName} casing should be normal", CharacterCasing.Normal, textBox.CharacterCasing);
		}
	}
}
