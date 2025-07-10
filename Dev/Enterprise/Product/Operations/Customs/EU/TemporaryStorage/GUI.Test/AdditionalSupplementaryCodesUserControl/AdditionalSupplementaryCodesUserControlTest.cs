using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	sealed class AdditionalSupplementaryCodesUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(TemporaryStoragePackedItem), control.BindingSource.DataSourceType);
		}

		public void TestAdditionalSupplementaryCodesTextBox()
		{
			var additionalSupplementaryCodesTextBox = control.AdditionalSupplementaryCodesTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(additionalSupplementaryCodesTextBox);
				AssertEquals("BindTo", nameof(TemporaryStoragePackedItem.API_Supplements), additionalSupplementaryCodesTextBox.BindTo);
				AssertNull("CaptionResourceString", additionalSupplementaryCodesTextBox.CaptionResourceString.Caption);
				AssertEquals("ReadOnly true", true, additionalSupplementaryCodesTextBox.ReadOnly);
				var labelCaptionRenderProvider = new LabelCaptionRenderProvider();
				AssertEquals("AdditionalSupplementaryCodesTextBox label is visible?", false, labelCaptionRenderProvider.GetLabelCaptionVisible(additionalSupplementaryCodesTextBox));
			});
		}

		[RequiresSTA]
		public void TestAdditionalSupplementaryCodesEditButton() => CombineAssertions(() =>
		{
			var button = control.AdditionalSupplementaryCodesEditButton;
			var goodsItem = Factory.New<TemporaryStoragePackedItem>();
			using var form = new ZForm(goodsItem);
			form.Controls.Add(control);
			form.Show();
			AssertAdditionalSupplementaryCodesEditButton(button);
		});

		void AssertAdditionalSupplementaryCodesEditButton(ZButton button)
		{
			AssertEquals("Caption", "Additional codes...", button.GetExtension<ILabelCaptionRenderer>().Caption);
			AssertNoExceptionThrown(() => button.PerformClick());
			AssertType<AdditionalSupplementaryCodesForm>(ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestImplementIExtendedControl()
		{
			var additionalSupplementaryCodesUserControlExtendedControl = control as IExtendedControl;

			AssertNotNull("AdditionalSupplementaryCodesUserControl implements IExtendedControl?", additionalSupplementaryCodesUserControlExtendedControl);

			AssertEquals("Host", additionalSupplementaryCodesUserControlExtendedControl, additionalSupplementaryCodesUserControlExtendedControl.Host);
			AssertNotNull("Extensions", additionalSupplementaryCodesUserControlExtendedControl.Extensions);
		}

		public void TestImplementResourceStringBindingMember()
		{
			var additionalSupplementaryCodesUserControlBindingMember = control as IResourceStringBindingMember;

			AssertNotNull("AdditionalSupplementaryCodesUserControl implements IResourceStringBindingMember?", additionalSupplementaryCodesUserControlBindingMember);
			AssertEquals("ResourceStringBindingMember", "API_Supplements", additionalSupplementaryCodesUserControlBindingMember.ResourceStringBindingMember);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new AdditionalSupplementaryCodesUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		AdditionalSupplementaryCodesUserControl control;
	}
}
