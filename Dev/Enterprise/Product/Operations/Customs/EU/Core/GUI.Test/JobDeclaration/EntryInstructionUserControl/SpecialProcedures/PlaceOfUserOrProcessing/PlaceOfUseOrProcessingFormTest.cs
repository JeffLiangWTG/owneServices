using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(PlaceOfUseOrProcessingForm))]
	sealed class PlaceOfUseOrProcessingFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var provider = Factory.New<PlaceOfUseOrProcessing>();
			using (var form = new PlaceOfUseOrProcessingForm(provider))
			{
				AssertEquals("Place of Use or Processing", form.FormCaption);
			}
		}

		public void TestDynamicPlaceOfUseOrProcessingPanel()
		{
			CombineAssertions(() =>
			{
				var provider = Factory.New<PlaceOfUseOrProcessing>();
				using (var form = new PlaceOfUseOrProcessingForm(provider))
				{
					AssertEquals("AutoScroll", true, form.DynamicPlaceOfUseOrProcessingPanel.AutoScroll);
					AssertEquals("Dock", DockStyle.Fill, form.DynamicPlaceOfUseOrProcessingPanel.Dock);
				}
			});
		}

		[RequiresSTA]
		public void TestUpdateDynamicPlaceOfUseOrProcessingPanelLayout()
		{
			var provider = Factory.New<PlaceOfUseOrProcessing>();
			provider.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			using (var form = new PlaceOfUseOrProcessingForm(provider))
			{
				form.Show();
				DynamicLayoutPanelTest.AssertControlsOrder(form.DynamicPlaceOfUseOrProcessingPanel,
					nameof(CusGoodsLocationControlBag.QualifierDropEdit),
					nameof(CusGoodsLocationControlBag.TypeDropEdit),
					nameof(CusGoodsLocationControlBag.AdditionalIdentifierTextBox),
					nameof(CusGoodsLocationControlBag.StreetAndNumberWithAddressValidationUserControl),
					nameof(CusGoodsLocationControlBag.CityTextBox),
					nameof(CusGoodsLocationControlBag.PostcodeTextBox),
					nameof(CusGoodsLocationControlBag.CountryCodeFindBox));
			}
		}

		public void TestOKButton_Click()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var provider = instruction.PlaceOfUseOrProcessingCollection.AddNew();
				using (var form = new PlaceOfUseOrProcessingForm(provider))
				{
					form.Show();
					AssertEquals("Caption", "OK", form.OKButton.CaptionResourceString.Caption);
					provider.Address.E2_Email = "aa";
					form.OKButton.PerformClick();
					AssertEquals("Please resolve all errors before saving.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Form is not closed because E2_Email has error", true, form.Visible);

					provider.Address.E2_Email = "aa@aa.com";
					form.OKButton.PerformClick();
					AssertEquals("Form is closed", false, form.Visible);
				}
			});
		}

		protected override Form GetFormToBashCore()
		{
			var provider = Factory.New<PlaceOfUseOrProcessing>();
			return new PlaceOfUseOrProcessingForm(provider);
		}
	}
}
