using System;
using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	class AsycudaManifestHeaderDocWrapperTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			CombineAssertions(() =>
			{
				AssertEquals(Header, Wrapper.Manifest);
				AssertEquals("Company Logo", new Size(3, 3), Wrapper.CompanyLogo.Size);
				AssertEquals("ETD", Wrapper.ETD, new ZDateTime(2023, 03, 02).ToBestReadableDateString());
				AssertEquals("RegistrationDate", Wrapper.RegistrationDate, new ZDateTime(2023, 03, 02).ToString());
				AssertEquals("BillWrapper", Wrapper.AsycudaBillDocWrappers.Count, 2);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(3, 3));

			Header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Header.AMA_E_DEP = new ZDateTime(2023, 03, 02);
			Header.RegistrationDate = new ZDateTime(2023, 03, 02);
			Header.Bills.RemoveAll();
			Header.Bills.AddNew();
			Header.Bills.AddNew();

			Wrapper = new AsycudaManifestHeaderDocWrapper(Header);
		}

		public AsycudaManifestHeaderDocWrapper Wrapper;
		public AsycudaManifestHeader Header;
	}
}
