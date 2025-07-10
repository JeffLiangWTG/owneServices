using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	sealed class EUICS2ManifestFieldsUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(AsycudaManifestHeader), control.BindingSource.DataSourceType);
		}

		public void TestActualDepartureDateEdit()
		{
			AssertType<ZDateEdit>(control.ActualDepartureDateEdit);
		}

		public void TestOriginCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.OriginCodeFindBox);
		}

		public void TestFinalDestinationCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.FinalDestinationCodeFindBox);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new EUICS2ManifestFieldsUserControl();
		}
		EUICS2ManifestFieldsUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
