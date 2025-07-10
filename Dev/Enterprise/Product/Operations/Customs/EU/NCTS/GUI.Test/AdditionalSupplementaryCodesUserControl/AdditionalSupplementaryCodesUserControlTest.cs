using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(AdditionalSupplementaryCodesUserControl))]
	sealed class AdditionalSupplementaryCodesUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsCommonCargoDesc), control.BindingSource.DataSourceType);
		}

		public void TestAdditionalSupplementaryCodesTextBox()
		{
			var additionalSupplementaryCodesTextBox = control.AdditionalSupplementaryCodesTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(additionalSupplementaryCodesTextBox);
				AssertEquals("BindTo", nameof(NctsCommonCargoDesc.BY_Supplements), additionalSupplementaryCodesTextBox.BindTo);
				AssertNull("CaptionResourceString", additionalSupplementaryCodesTextBox.CaptionResourceString.Caption);
				var labelCaptionRenderProvider = new LabelCaptionRenderProvider();
				AssertEquals("AdditionalSupplementaryCodesTextBox label is visible?", false, labelCaptionRenderProvider.GetLabelCaptionVisible(additionalSupplementaryCodesTextBox));
			});
		}

		[RequiresSTA]
		public void TestAdditionalSupplementaryCodesEditButton_Departure() => CombineAssertions(() =>
		{
			var button = control.AdditionalSupplementaryCodesEditButton;
			var goodsItem = Factory.New<NctsDepartureCargoDesc>();
			using var form = new ZForm(goodsItem);
			form.Controls.Add(control);
			form.Show();
			AssertAdditionalSupplementaryCodesEditButton(button);
		});

		public void TestAdditionalSupplementaryCodesEditButton_Arrival() => CombineAssertions(() =>
		{
			var button = control.AdditionalSupplementaryCodesEditButton;
			var goodsItem = Factory.New<NctsArrivalCargoDesc>();
			using var form = new ZForm(goodsItem);
			form.Controls.Add(control);
			form.Show();
			AssertAdditionalSupplementaryCodesEditButton(button);
		});

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
			AssertEquals("ResourceStringBindingMember", "BY_Supplements", additionalSupplementaryCodesUserControlBindingMember.ResourceStringBindingMember);
		}

		void AssertAdditionalSupplementaryCodesEditButton(ZButton button)
		{
			AssertEquals("Caption", "Additional codes...", button.GetExtension<ILabelCaptionRenderer>().Caption);
			AssertNoExceptionThrown(button.PerformClick);
			AssertType<AdditionalSupplementaryCodesForm>(ZFormModaliser.LastFormShownDialogForTest);
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
