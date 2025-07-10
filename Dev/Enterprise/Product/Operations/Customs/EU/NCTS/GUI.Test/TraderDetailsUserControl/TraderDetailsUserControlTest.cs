using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class TraderDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
		}

		public void TestPrincipalDocAddressControl()
		{
			CombineAssertions(() =>
			{
				var principalDocAddressControl = control.PrincipalDocAddressControl;
				AssertEquals("PrincipalDocAddressControl.BindTo", "Principal", principalDocAddressControl.BindTo);
				AssertEquals("PrincipalDocAddressControl.BindToOrganisations", "Lookups.Organisations", principalDocAddressControl.BindToOrganisations);
				AssertEquals("PrincipalDocAddressControl.DisplayMode", ZDocAddressControlDisplayMode.CompactWithContactTab, principalDocAddressControl.DisplayMode);
				AssertEquals("PrincipalDocAddressControl.Caption", "Principal", principalDocAddressControl.CaptionResourceString.Caption);
			});
		}

		[RequiresSTA]
		public void TestConsignorDocAddressControl()
		{
			CombineAssertions(() =>
			{
				var consignorDocAddressControl = control.ConsignorDocAddressControl;
				AssertEquals("ConsignorDocAddressControl.BindTo", "Consignor", consignorDocAddressControl.BindTo);
				AssertEquals("ConsignorDocAddressControl.BindToOrganisations", "Lookups.Consignors", consignorDocAddressControl.BindToOrganisations);
				AssertEquals("ConsignorDocAddressControl.DisplayMode", ZDocAddressControlDisplayMode.CompactWithContactTab, consignorDocAddressControl.DisplayMode);
				AssertEquals("ConsignorDocAddressControl.Caption", "Consignor", consignorDocAddressControl.CaptionResourceString.Caption);
			});
		}

		public void TestConsigneeDocAddressControl()
		{
			CombineAssertions(() =>
			{
				var consigneeDocAddressControl = control.ConsigneeDocAddressControl;
				AssertEquals("ConsigneeDocAddressControl.BindTo", "Consignee", consigneeDocAddressControl.BindTo);
				AssertEquals("ConsigneeDocAddressControl.BindToOrganisations", "Lookups.Consignees", consigneeDocAddressControl.BindToOrganisations);
				AssertEquals("ConsigneeDocAddressControl.DisplayMode", ZDocAddressControlDisplayMode.CompactWithContactTab, consigneeDocAddressControl.DisplayMode);
				AssertEquals("ConsigneeDocAddressControl.Caption", "Consignee", consigneeDocAddressControl.CaptionResourceString.Caption);
			});
		}

		public void TestRepresentativeDocAddressControl()
		{
			CombineAssertions(() =>
			{
				var representativeDocAddressControl = control.RepresentativeDocAddressControl;
				AssertEquals("RepresentativeDocAddressControl.BindTo", "MovementHeader.Representative", representativeDocAddressControl.BindTo);
				AssertEquals("RepresentativeDocAddressControl.BindToOrganisations", "Lookups.Organisations", representativeDocAddressControl.BindToOrganisations);
				AssertEquals("RepresentativeDocAddressControl.DisplayMode", ZDocAddressControlDisplayMode.CompactWithContactTab, representativeDocAddressControl.DisplayMode);
				AssertEquals("RepresentativeDocAddressControl.Caption", "Representative", representativeDocAddressControl.CaptionResourceString.Caption);
			});
		}

		public void TestFromWarehouseGroupBox()
		{
			var fromWarehouseGroupBox = control.FromWarehouseGroupBox;
			AssertEquals("FromWarehouseGroupBox.Caption", "From Warehouse", fromWarehouseGroupBox.CaptionResourceString.Caption);
		}

		public void TestFromWarehouseAddressControl()
		{
			var fromWarehouseAddressControl = control.FromWarehouseAddressControl;
			CombineAssertions(() =>
			{
				AssertEquals("FromWarehouseAddressControl.BindTo", "MovementHeader.BM_OA_WarehouseAddress", fromWarehouseAddressControl.BindTo);
				AssertEquals("FromWarehouseAddressControl.BindToOrgList", "MovementHeader.Lookups.BondedWarehouseCollection", fromWarehouseAddressControl.BindToOrgList);
			});
		}

		public void TestFromWarehouseCodeTextBox()
		{
			var fromWarehouseCodeTextBox = control.FromWarehouseCodeTextBox;
			AssertEquals("FromWarehouseCodeTextBox.BindTo", "MovementHeader.FromWarehouseCode", fromWarehouseCodeTextBox.BindTo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TraderDetailsUserControl();
		}
		TraderDetailsUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
