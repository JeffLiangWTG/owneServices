using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class LocationOfGoodsUserControlTest : TestCaseWithFactory
	{
		public void TestAllowMixedCaseAuthorisationNumbers()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var provider = Factory.New<CusGoodsLocationProviderForTest>();
			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfigurationAllowMixedCaseAuthorisationNumbers(Factory, false))
			using (var form = new ZForm(provider))
			using (var control = new LocationOfGoodsUserControl())
			{
				control.SetDataBinding(header, "");
				AssertEquals("Location of Goods - Upper Case", CharacterCasing.Upper, control.LocationOfGoodsDescription.CharacterCasing);
			}

			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfigurationAllowMixedCaseAuthorisationNumbers(Factory, true))
			using (var form = new ZForm(provider))
			using (var control = new LocationOfGoodsUserControl())
			{
				control.SetDataBinding(header, "");
				AssertEquals("Location of Goods - Normal Case", CharacterCasing.Normal, control.LocationOfGoodsDescription.CharacterCasing);
			}
		}

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

		public void TestCusGoodsLocationProviderTypeBrowsableAttribute()
		{
			using (var control = new LocationOfGoodsUserControl())
			{
				AssertEquals(true, TypeDescriptor.GetAttributes(control.CusGoodsLocationProviderType)[typeof(BrowsableAttribute)] != null);
			}
		}
	}

	sealed class CusGoodsLocationProviderForTest : DummyBaseBusinessObject, ICusGoodsLocationProvider
	{
		public CusGoodsLocationProviderForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			var mockCusGoodsLocation = Factory.NewMoq<Business.CusGoodsLocation>();
			mockCusGoodsLocation.Protected().Setup("BeginEditCore").Callback(() => GoodsLocation.CGL_Qualifier = "U");
			GoodsLocationExposed = mockCusGoodsLocation.Object;
		}

		public EU.Business.CusGoodsLocation GoodsLocation => GoodsLocationExposed;

		public EU.Business.CusGoodsLocation GoodsLocationExposed;

		public ZString GoodsLocationDescription => new ZString();

		public ZPropertyInfo GoodsLocationDescriptionInfo => GetZPropertyInfo(nameof(GoodsLocationDescription));

		public ZString ProviderKey => "XXX";

		public void ValidateGoodsLocationDescription()
		{
			ValidateGoodsLocationDescriptionIsCalled = true;
		}

		public bool ValidateGoodsLocationDescriptionIsCalled { get; set; }

		public ZBool AllowMixedCaseAuthorisationNumbers { get; set; }
	}
}
