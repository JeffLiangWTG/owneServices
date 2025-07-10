using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(CusGoodsLocationForm))]
	sealed class CusGoodsLocationFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var provider = Factory.New<CusGoodsLocationProviderForTest>();
			using (var form = new CusGoodsLocationForm(provider))
			{
				AssertEquals("Location of Goods", form.FormCaption);
			}
		}

		public void TestAuthorizationCodeFindBoxCharacterCasing()
		{
			var provider = Factory.New<CusGoodsLocationProviderForTest>();
			provider.AllowMixedCaseAuthorisationNumbers = false;
			using (var form = new CusGoodsLocationForm(provider))
			{
				var authorizationCodeFindBox = (ZCodeFindBox)form.Controls.Find("AuthorizationCodeFindBox", true).Single();
				AssertEquals("Location of Goods - Upper Case", CharacterCasing.Upper, authorizationCodeFindBox.CodeBox.CharacterCasing);
			}

			provider.AllowMixedCaseAuthorisationNumbers = true;
			using (var form = new CusGoodsLocationForm(provider))
			{
				var authorizationCodeFindBox = (ZCodeFindBox)form.Controls.Find("AuthorizationCodeFindBox", true).Single();
				AssertEquals("Location of Goods - Mixed Case", CharacterCasing.Normal, authorizationCodeFindBox.CodeBox.CharacterCasing);
			}
		}

		public void TestDynamicGoodsLocationPanel()
		{
			CombineAssertions(() =>
			{
				var provider = Factory.New<CusGoodsLocationProviderForTest>();
				using (var form = new CusGoodsLocationForm(provider))
				{
					AssertEquals("AutoScroll", true, form.DynamicGoodsLocationPanel.AutoScroll);
					AssertEquals("Dock", DockStyle.Fill, form.DynamicGoodsLocationPanel.Dock);
				}
			});
		}

		public void TestUpdateDynamicGoodsLocationPanelLayout()
		{
			var provider = Factory.New<CusGoodsLocationProviderForTest>();
			provider.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			using (var form = new CusGoodsLocationForm(provider))
			{
				form.Show();
				DynamicLayoutPanelTest.AssertControlsOrder(form.DynamicGoodsLocationPanel,
					nameof(CusGoodsLocationControlBag.QualifierDropEdit),
					nameof(CusGoodsLocationControlBag.TypeDropEdit),
					nameof(CusGoodsLocationControlBag.StreetAndNumberWithAddressValidationUserControl),
					nameof(CusGoodsLocationControlBag.CityTextBox),
					nameof(CusGoodsLocationControlBag.PostcodeTextBox),
					nameof(CusGoodsLocationControlBag.CountryCodeFindBox),
					nameof(CusGoodsLocationControlBag.ContactTextBox),
					nameof(CusGoodsLocationControlBag.PhoneTextBox),
					nameof(CusGoodsLocationControlBag.EmailTextBox));
			}
		}

		[RequiresSTA]
		public void TestOKButton_Click()
		{
			CombineAssertions(() =>
			{
				var provider = Factory.New<CusGoodsLocationProviderForTest>();
				using (var form = new CusGoodsLocationForm(provider))
				{
					form.Show();
					AssertEquals("Caption", "OK", form.OKButton.CaptionResourceString.Caption);
					provider.GoodsLocation.Address.E2_Email = "aa";
					form.OKButton.PerformClick();
					AssertEquals("Please resolve all errors before saving.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Form is not closed because E2_Email has error", true, form.Visible);

					provider.GoodsLocation.Address.E2_Email = "aa@aa.com";
					form.OKButton.PerformClick();
					AssertEquals("Form is closed", false, form.Visible);
					AssertEquals("ValidateGoodsLocationDescription is called", true, provider.ValidateGoodsLocationDescriptionIsCalled);
				}
			});
		}

		public void TestGoodsLocationFormLayoutProvider()
		{
			var provider = Factory.New<CusGoodsLocationProviderForTest>();
			using (var form = new CusGoodsLocationFormForTest(provider))
			{
				AssertType<GoodsLocationFormLayoutProvider>(form.GoodsLocationFormLayoutProviderExposed);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var provider = Factory.New<CusGoodsLocationProviderForTest>();
			return new CusGoodsLocationForm(provider);
		}

		class CusGoodsLocationFormForTest : CusGoodsLocationForm
		{
			public CusGoodsLocationFormForTest(ICusGoodsLocationProvider provider) : base(provider)
			{
			}

			public IGoodsLocationFormLayoutProvider GoodsLocationFormLayoutProviderExposed => GoodsLocationFormLayoutProvider;
		}
	}
}
