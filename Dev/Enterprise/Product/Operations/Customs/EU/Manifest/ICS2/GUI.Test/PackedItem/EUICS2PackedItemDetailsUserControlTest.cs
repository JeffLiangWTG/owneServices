using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	sealed class EUICS2PackedItemDetailsUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestCusCodeFindBox() => CombineAssertions(() =>
		{
			var codeFindBox = control.CusCodeFindBox;

			AssertType<ZCodeFindBox>(codeFindBox);
			AssertEquals("BindingMember", $"{nameof(AsycudaPack.PackedItem)}.{nameof(AsycudaPackedItem.API_ChemicalSubstanceCode)}", codeFindBox.GetBindingMember());
			AssertEquals(false, codeFindBox.ShowDescriptionBox);
			AssertEquals(25, codeFindBox.PreBoundMaxLength);
		});

		protected override void SetUp()
		{
			base.SetUp();

			control = new EUICS2PackedItemDetailsUserControl();
		}
		EUICS2PackedItemDetailsUserControl control;

		protected override void TearDown()
		{
			base.TearDown();

			control.Dispose();
		}
	}
}
