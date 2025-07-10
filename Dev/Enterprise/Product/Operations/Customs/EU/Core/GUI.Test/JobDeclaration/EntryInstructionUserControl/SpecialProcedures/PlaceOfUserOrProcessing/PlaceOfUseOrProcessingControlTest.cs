using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class PlaceOfUseOrProcessingControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var userControl = new PlaceOfUseOrProcessingControl())
			{
				AssertEquals(typeof(IFirstPlaceOfUseOrProcessingProvider), userControl.BindingSource.DataSourceType);
			}
		}

		public void TestPlaceOfUseOrProcessingDescription()
		{
			using (var userControl = new PlaceOfUseOrProcessingControl())
			{
				AssertEquals("BindTo", nameof(IFirstPlaceOfUseOrProcessingProvider.FirstPlaceOfUseOrProcessingDescription), userControl.PlaceOfUseOrProcessingDescription.BindTo);
				CombineAssertions(() =>
				{
					var resString = userControl.PlaceOfUseOrProcessingDescription.CaptionResourceString;
					AssertEquals("Caption", string.Empty, resString.Caption);
					AssertEquals("FullDescription", "[4/5] First Place of Processing", resString.FullDescription);
				});
			}
		}

		public void TestMoreButton()
		{
			CombineAssertions(() =>
			{
				using (var form = new ZForm(Factory.New<PlaceOfUseOrProcessingProviderForTest>()))
				using (var control = new PlaceOfUseOrProcessingControl())
				{
					form.Controls.Add(control);
					form.Show();
					AssertEquals("Caption", "More..", control.MoreButton.CaptionResourceString.Caption);
					control.MoreButton.PerformClick();
				}

				var placeOfUseOrProcessingProvider = Factory.New<PlaceOfUseOrProcessingProviderForTest>();
				placeOfUseOrProcessingProvider.PlaceOfUseOrProcessingExposed = null;
				using (var form = new ZForm(placeOfUseOrProcessingProvider))
				using (var control = new PlaceOfUseOrProcessingControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.MoreButton.PerformClick();
					AssertStartsWith("error when The Place of Use or Processing is currently being edited by another user", "The Place of Use or Processing is currently being edited by another user. Please try later.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			});
		}

		public void TestIExtendedControl()
		{
			using (var control = new PlaceOfUseOrProcessingControl())
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
			using (var control = new PlaceOfUseOrProcessingControl())
			{
				AssertEquals("TopLevelDataSourceType", typeof(IFirstPlaceOfUseOrProcessingProvider), ((ITopLevelDataSourceType)control).DataSourceType);
			}
		}
	}

	sealed class PlaceOfUseOrProcessingProviderForTest : DummyBaseBusinessObject, IFirstPlaceOfUseOrProcessingProvider
	{
		public PlaceOfUseOrProcessingProviderForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			PlaceOfUseOrProcessingExposed = Factory.New<PlaceOfUseOrProcessing>();
		}

		public PlaceOfUseOrProcessing FirstPlaceOfUseOrProcessing => PlaceOfUseOrProcessingExposed;
		public PlaceOfUseOrProcessing PlaceOfUseOrProcessingExposed;

		public ZString FirstPlaceOfUseOrProcessingDescription => new ZString();

		public ZPropertyInfo FirstPlaceOfUseOrProcessingDescriptionInfo => GetZPropertyInfo(nameof(FirstPlaceOfUseOrProcessingDescription));
	}
}
