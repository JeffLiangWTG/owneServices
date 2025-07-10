using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ConsignmentItemUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ExitControlBase.Business.ICusExitConsignmentItemCollection<CusExitConsignmentItem>), userControl.BindingSource.DataSourceType);
		}

		public void TestConsignmentItemPackingDetailsUserControl()
		{
			var control = userControl.ConsignmentItemPackingDetailsUserControl;
			CombineAssertions(() =>
			{
				AssertType<ConsignmentItemPackingDetailsUserControl>("Type", control);
				AssertEquals("BindingMember", nameof(CusExitConsignmentItem.CusExitConsignmentPackagePivots), userControl.BindingSource.GetBindingMember(control));
			});
		}

		public void TestReportAdditionalDocumentsGridUserControl()
		{
			var control = userControl.ReportAdditionalDocumentsGridUserControl;
			CombineAssertions(() =>
			{
				AssertType<ReportAdditionalDocumentsGridUserControl>("Type", control);
				AssertEquals("BindingMember", nameof(CusExitConsignmentItem.AdditionalInfos), userControl.BindingSource.GetBindingMember(control));
			});
		}

		public void TestAdditionalDocumentsLabel()
		{
			var control = userControl.AdditionalDocumentsLabel;
			CombineAssertions(() =>
			{
				AssertType<ZLabel>("Type", control);
				AssertEquals("Caption", "Additional Document", control.CaptionResourceString.Caption);
				AssertEquals("FontType", (ZArchitecture.Core.OFontTypes.Medium | ZArchitecture.Core.OFontTypes.Bolded), control.FontType);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ConsignmentItemUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
		ConsignmentItemUserControl userControl;
	}
}
