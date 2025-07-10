using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class HouseConsignmentDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsBill), control.BindingSource.DataSourceType);
		}

		public void TestCountryOfDispatchDropEdit()
		{
			var countryOfDispatchDropEdit = control.CountryOfDispatchDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", countryOfDispatchDropEdit);
				AssertEquals("BindingMember", nameof(NctsBill.B0_RN_NKCountryOfExport), countryOfDispatchDropEdit.GetBindingMember());
			});
		}

		public void TestCountryOfDestinationDropEdit()
		{
			var countryOfDestinationDropEdit = control.CountryOfDestinationDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", countryOfDestinationDropEdit);
				AssertEquals("BindingMember", nameof(NctsBill.B0_RN_NKCountryOfDestination), countryOfDestinationDropEdit.GetBindingMember());
			});
		}

		[RequiresSTA]
		public void TestGrossWeightCalcDropEdit()
		{
			var grossWeightCalcDropEdit = control.GrossWeightCalcDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>("Type", grossWeightCalcDropEdit);
				AssertEquals("BindToAmount", nameof(NctsBill.B0_Weight), grossWeightCalcDropEdit.BindToAmount);
				AssertEquals("BindToUnit", nameof(NctsBill.B0_WeightUQ), grossWeightCalcDropEdit.BindToUnit);
				AssertEquals("Decimals", 6, grossWeightCalcDropEdit.Decimals);
			});
		}

		public void TestTransportMoPDropEdit()
		{
			var transportMoPDropEdit = control.TransportMoPDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", transportMoPDropEdit);
				AssertEquals("BindingMember", nameof(NctsBill.B0_TransportPaymentMethod), transportMoPDropEdit.GetBindingMember());
			});
		}

		[RequiresSTA]
		public void TestReferenceNumberUCRTextBox()
		{
			var referenceNumberUCRTextBox = control.ReferenceNumberUCRTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", referenceNumberUCRTextBox);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, referenceNumberUCRTextBox.CharacterCasing);
				AssertEquals("BindingMember", nameof(NctsBill.B0_ReferenceID), referenceNumberUCRTextBox.GetBindingMember());
			});
		}

		public void TestConsignorDocAddressControl()
		{
			var consignorDocAddressControl = control.ConsignorDocAddressControl;
			CombineAssertions(() =>
			{
				AssertType<ZDocAddressControl>("Type", consignorDocAddressControl);
				AssertEquals("BindToOrganisations",
					nameof(NctsHeader.Lookups) + "." + nameof(NctsHeaderLookups.Consignors),
					consignorDocAddressControl.BindToOrganisations);
				AssertEquals(ZDocAddressControlDisplayMode.CompactWithContactTab, consignorDocAddressControl.DisplayMode);
			});
		}

		public void TestConsigneeDocAddressControl()
		{
			var consigneeDocAddressControl = control.ConsigneeDocAddressControl;
			CombineAssertions(() =>
			{
				AssertType<ZDocAddressControl>("Type", consigneeDocAddressControl);
				AssertEquals("BindToOrganisations", nameof(NctsHeader.Lookups) + "." + nameof(NctsHeaderLookups.Consignees), consigneeDocAddressControl.BindToOrganisations);
				AssertEquals(ZDocAddressControlDisplayMode.CompactWithContactTab, consigneeDocAddressControl.DisplayMode);
			});
		}

		public void TestConsignorDocAddressControl_Caption()
		{
			AssertEquals("Consignor", control.ConsignorDocAddressControl.CaptionResourceString.Caption);
		}

		[RequiresSTA]
		public void TestConsigneeDocAddressControl_Caption()
		{
			AssertEquals("Consignee", control.ConsigneeDocAddressControl.CaptionResourceString.Caption);
		}

		public void TestLinePriceCurrencyDropEdit()
		{
			var linePriceCurrencyDropEdit = control.LinePriceCurrencyDropEdit;

			AssertEquals("BindingMember", nameof(NctsBill.B0_RX_NKLinePriceCurrency), linePriceCurrencyDropEdit.GetBindingMember());
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new HouseConsignmentDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		HouseConsignmentDetailsUserControl control;
	}
}
