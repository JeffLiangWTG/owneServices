using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	sealed class DestinationLocationOfGoodsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var userControl = new DestinationLocationOfGoodsUserControl())
			{
				AssertEquals(typeof(TemporaryStorageHeader), userControl.BindingSource.DataSourceType);
			}
		}

		public void TestLocationOfGoodsDescription()
		{
			using (var userControl = new DestinationLocationOfGoodsUserControl())
			{
				AssertEquals("BindTo", nameof(TemporaryStorageHeader.DestinationGoodsLocationDescription), userControl.LocationOfGoodsDescription.BindTo);
			}
		}

		public void TestMoreButton()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<TemporaryStorageHeader>();
				header.DestinationGoodsLocation.CGL_Qualifier = "U";
				using (var form = new ZForm(header))
				using (var control = new DestinationLocationOfGoodsUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.MoreButton.AssertThisControl(x => x.WithCaption("More.."));
					control.MoreButton.PerformClick();
					AssertType<DestinationCusGoodsLocationForm>("Clicking button opens up CusGoodsLocationForm", ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("Given CGL_Qualifier", "U", header.DestinationGoodsLocation.CGL_Qualifier);
				}
			});
		}

		public void TestResourceStringBindingMember()
		{
			using (var control = new DestinationLocationOfGoodsUserControl())
			{
				AssertEquals(nameof(TemporaryStorageHeader.DestinationGoodsLocationDescription), control.ResourceStringBindingMember);
			}
		}

		public void TestIExtendedControl()
		{
			using (var control = new DestinationLocationOfGoodsUserControl())
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
			using (var control = new DestinationLocationOfGoodsUserControl())
			{
				control.CusGoodsLocationProviderType = typeof(CusEntryInstruction);
				AssertEquals("TopLevelDataSourceType", typeof(CusEntryInstruction), ((ITopLevelDataSourceType)control).DataSourceType);
			}
		}

		public void TestCusGoodsLocationProviderTypeBrowsableAttribute()
		{
			using (var control = new DestinationLocationOfGoodsUserControl())
			{
				AssertEquals(true, TypeDescriptor.GetAttributes(control.CusGoodsLocationProviderType)[typeof(BrowsableAttribute)] != null);
			}
		}
	}
}
