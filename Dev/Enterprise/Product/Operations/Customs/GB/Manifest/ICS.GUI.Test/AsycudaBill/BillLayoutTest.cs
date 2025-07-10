using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.GUI.Testing
{
	[TestedType(typeof(BillLayouts))]
	sealed class BillLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<EU.Manifest.Business.AsycudaBill>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OriginCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FinalDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GoodsLocationDropEditWithFixedWidth, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.LocationInformationTextBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.MarksAndNumbersTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.RemarksTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.AgentAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CarrierReferenceTextBox, ControlWidthClass.Long);
				yield return (BillControlBag.Instance.SpecialMentionsDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.PrepaidCollectDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.DiscountValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OtherChargesValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			}
		}

		public void TestFieldsVisibility()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EUImportControlSystem, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var bill = manifest.Bills.AddNew();
				bill.ABL_BillNumber = "123";

				using (var form = new ManifestForm(manifest))
				{
					form.Show();

					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
					var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
					mainTabControl.SelectedTab = billsAndPacksTabPage;

					var billsAndPacksTabControl = billsAndPacksTabPage.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
					var billsTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsTabPage");
					billsAndPacksTabControl.SelectedTab = billsTabPage;

					var asycudaBillUserControl = billsAndPacksTabPage.FindSingle<AsycudaBillUserControl>(c => c.Name == "asycudaBillUserControl");
					AssertEquals("SpecialMentionsDropEdit visible", true, asycudaBillUserControl.FindSingle<ZDropEdit>("SpecialMentionsDropEdit").Visible);
				}
			}
		}
	}
}
