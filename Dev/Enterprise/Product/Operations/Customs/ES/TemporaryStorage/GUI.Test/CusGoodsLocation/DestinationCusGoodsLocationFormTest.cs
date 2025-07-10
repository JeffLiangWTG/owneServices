using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(DestinationCusGoodsLocationForm))]
	sealed class DestinationCusGoodsLocationFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			using (var form = new DestinationCusGoodsLocationForm(header))
			{
				AssertEquals("Location of Goods", form.FormCaption);
			}
		}

		public void TestDynamicGoodsLocationPanel()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<TemporaryStorageHeader>();
				using (var form = new DestinationCusGoodsLocationForm(header))
				{
					AssertEquals("AutoScroll", true, form.DynamicGoodsLocationPanel.AutoScroll);
					AssertEquals("Dock", DockStyle.Fill, form.DynamicGoodsLocationPanel.Dock);
				}
			});
		}

		public void TestUpdateDynamicGoodsLocationPanelLayout()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.DestinationGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			using (var form = new DestinationCusGoodsLocationForm(header))
			{
				form.Show();
				DynamicLayoutPanelTest.AssertControlsOrder(form.DynamicGoodsLocationPanel,
					nameof(EU.GUI.CusGoodsLocationControlBag.QualifierDropEdit),
					nameof(EU.GUI.CusGoodsLocationControlBag.TypeDropEdit),
					nameof(EU.GUI.CusGoodsLocationControlBag.StreetAndNumberWithAddressValidationUserControl),
					nameof(EU.GUI.CusGoodsLocationControlBag.CityTextBox),
					nameof(EU.GUI.CusGoodsLocationControlBag.PostcodeTextBox),
					nameof(EU.GUI.CusGoodsLocationControlBag.CountryCodeFindBox),
					nameof(EU.GUI.CusGoodsLocationControlBag.ContactTextBox),
					nameof(EU.GUI.CusGoodsLocationControlBag.PhoneTextBox),
					nameof(EU.GUI.CusGoodsLocationControlBag.EmailTextBox));
			}
		}

		public void TestOKButton_Click()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<TemporaryStorageHeader>();
				using (var form = new DestinationCusGoodsLocationForm(header))
				{
					form.Show();
					form.OKButton.AssertThisControl(x => x.WithCaption("OK"));
					header.DestinationGoodsLocation.Address.E2_Email = "aa";
					form.OKButton.PerformClick();
					AssertEquals("Please resolve all errors before saving.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Form is not closed because E2_Email has error", true, form.Visible);

					header.DestinationGoodsLocation.Address.E2_Email = "aa@aa.com";
					form.OKButton.PerformClick();
					AssertEquals("Form is closed", false, form.Visible);
				}
			});
		}

		public void TestGoodsLocationFormLayoutProvider()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			using (var form = new DestinationCusGoodsLocationFormForTest(header))
			{
				AssertType<ES.GUI.GoodsLocationFormLayoutProvider>(form.GoodsLocationFormLayoutProviderExposed);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			return new DestinationCusGoodsLocationForm(header);
		}

		class DestinationCusGoodsLocationFormForTest : DestinationCusGoodsLocationForm
		{
			public DestinationCusGoodsLocationFormForTest(TemporaryStorageHeader header) : base(header)
			{
			}

			public EU.GUI.IGoodsLocationFormLayoutProvider GoodsLocationFormLayoutProviderExposed => GoodsLocationFormLayoutProvider;
		}
	}
}
