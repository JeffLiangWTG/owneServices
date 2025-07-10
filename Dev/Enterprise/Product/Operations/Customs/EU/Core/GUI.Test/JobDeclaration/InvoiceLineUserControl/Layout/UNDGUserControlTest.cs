using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class UNDGUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestDGGuidFindBox()
		{
			var dGGuidFindBox = control.DGGuidFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZGuidFindBox>("Type", dGGuidFindBox);
				AssertEquals("BindingMember", $"{nameof(JobDeclaration.FilteredInvoiceLines)}.{nameof(JobComInvoiceLine.UNDGs)}+{nameof(UNDGDataItemCollection.FirstItemForBinding)}.{nameof(UNDGDataItem.DI_DG)}", dGGuidFindBox.GetBindingMember());
			});
		}

		public void TestUNDangerousGoodsGuidFindBox_Captions()
		{
			CombineAssertions(() =>
			{
				var captionResourceStringData = control.DGGuidFindBox.CaptionResourceString;
				AssertEquals("UNDangerousGoodsGuidFindBox: Caption", "UNDG Number", captionResourceStringData.Caption);
				AssertEquals("UNDangerousGoodsGuidFindBox: MediumCaption", "UNDG No.", captionResourceStringData.MediumCaption);
				AssertEquals("UNDangerousGoodsGuidFindBox: ShortCaption", "UNDG", captionResourceStringData.ShortCaption);
			});
		}

		public void TestFlashpointUserControl()
		{
			var flashpointUserControl = control.FlashpointUserControl;
			CombineAssertions(() =>
			{
				AssertType<UNDGFlashpointUserControl>("Type", flashpointUserControl);
				AssertEquals("BindingMember", $"{nameof(JobDeclaration.FilteredInvoiceLines)}.{nameof(JobComInvoiceLine.UNDGs)}+{nameof(UNDGDataItemCollection.FirstItemForBinding)}", flashpointUserControl.GetBindingMember());
			});
		}

		public void TestUNDangerousGoodsLinkLabel()
		{
			AssertType<ZLinkLabel>(control.DGLinkLabel);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new UNDGUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		UNDGUserControl control;
	}
}
