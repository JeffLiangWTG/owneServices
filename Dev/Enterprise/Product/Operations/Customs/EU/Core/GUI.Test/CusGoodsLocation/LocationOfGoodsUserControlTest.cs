using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq.Protected;
using CusGoodsLocation = Enterprise.Customs.EU.Business.CusGoodsLocation;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class LocationOfGoodsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var userControl = new LocationOfGoodsUserControl())
			{
				AssertEquals(typeof(ICusGoodsLocationProvider), userControl.BindingSource.DataSourceType);
			}
		}

		public void TestLocationOfGoodsDescription()
		{
			using (var userControl = new LocationOfGoodsUserControl())
			{
				AssertEquals("BindTo", nameof(ICusGoodsLocationProvider.GoodsLocationDescription), userControl.LocationOfGoodsDescription.BindTo);
			}
		}

		public void TestMoreButton()
		{
			CombineAssertions(() =>
			{
				var provider = Factory.New<CusGoodsLocationProviderForTest>();
				using (var form = new ZForm(provider))
				using (var control = new LocationOfGoodsUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					AssertEquals("Caption", "More..", control.MoreButton.CaptionResourceString.Caption);
					control.MoreButton.PerformClick();
					AssertType<CusGoodsLocationForm>("Clicking button opens up CusGoodsLocationForm", ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("Default CGL_Qualifier", "U", provider.GoodsLocation.CGL_Qualifier);
				}

				var cusGoodsLocation = Factory.New<CusGoodsLocationProviderForTest>();
				cusGoodsLocation.GoodsLocationExposed = null;
				using (var form = new ZForm(cusGoodsLocation))
				using (var control = new LocationOfGoodsUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.MoreButton.PerformClick();
					AssertStartsWith("error when Location of Goods is currently being edited by another user", "The Location of Goods is currently being edited by another user. Please try later.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			});
		}

		public void TestResourceStringBindingMember()
		{
			using (var control = new LocationOfGoodsUserControl())
			{
				AssertEquals(nameof(ICusGoodsLocationProvider.GoodsLocationDescription), control.ResourceStringBindingMember);
			}
		}

		public void TestIExtendedControl()
		{
			using (var control = new LocationOfGoodsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("Host", control, control.Host);
					AssertType<DefaultControlExtensionCollection>("Extensions", control.Extensions);
				});
			}
		}

		public void TestTopLevelDataSourceType()
		{
			using (var control = new LocationOfGoodsUserControl())
			{
				control.CusGoodsLocationProviderType = typeof(CusEntryInstruction);
				AssertEquals("TopLevelDataSourceType", typeof(CusEntryInstruction), ((ITopLevelDataSourceType)control).DataSourceType);
			}
		}

		public void TestCusGoodsLocationProviderTypeBrowsableAttribute()
		{
			using (var control = new LocationOfGoodsUserControl())
			{
				AssertEquals(true, TypeDescriptor.GetAttributes(control.CusGoodsLocationProviderType)[typeof(BrowsableAttribute)] != null);
			}
		}
	}

	sealed class CusGoodsLocationProviderForTest : DummyBaseBusinessObject, ICusGoodsLocationProviderWhichAllowsMixedCase
	{
		public CusGoodsLocationProviderForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			var mockCusGoodsLocation = Factory.NewMoq<CusGoodsLocation>();
			mockCusGoodsLocation.Protected().Setup("BeginEditCore").Callback(() => GoodsLocation.CGL_Qualifier = "U");
			GoodsLocationExposed = mockCusGoodsLocation.Object;
		}

		public CusGoodsLocation GoodsLocation => GoodsLocationExposed;
		public CusGoodsLocation GoodsLocationExposed;

		public ZString GoodsLocationDescription => new ZString();

		public ZPropertyInfo GoodsLocationDescriptionInfo => GetZPropertyInfo(nameof(GoodsLocationDescription));

		public void ValidateGoodsLocationDescription()
		{
			ValidateGoodsLocationDescriptionIsCalled = true;
		}

		public bool ValidateGoodsLocationDescriptionIsCalled { get; set; }

		public ZString ProviderKey => "XXX";

		public ZBool AllowMixedCaseAuthorisationNumbers { get; set; }
	}
}
