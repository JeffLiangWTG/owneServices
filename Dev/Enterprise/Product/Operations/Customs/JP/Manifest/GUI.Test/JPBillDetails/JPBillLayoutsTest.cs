using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(JPBillLayouts))]
	sealed class JPBillLayoutsTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
				yield return (JPBillControlBag.Instance.FinalDestinationUserControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.DischargePortCodeFindBox, ControlWidthClass.Long);
				yield return (JPBillControlBag.Instance.TariffFindBox, ControlWidthClass.Long);
				yield return (JPBillControlBag.Instance.RepresentativeHSCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (JPBillControlBag.Instance.CustomsWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (JPBillControlBag.Instance.CustomsNetWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);
				yield return (JPBillControlBag.Instance.CustomsVolumeCalcDropEdit, ControlWidthClass.Long);
				yield return (JPBillControlBag.Instance.GoodsOriginCodeFindBox, ControlWidthClass.Long);
				yield return (JPBillControlBag.Instance.SpecialCargoCodeFindBox, ControlWidthClass.Long);
				yield return (JPBillControlBag.Instance.GoodsLocationCodeFindBox, ControlWidthClass.Long);
				yield return (JPBillControlBag.Instance.CargoTypeDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.MarksAndNumbersTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.RemarksTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			}
		}

		public void TestVisibility()
		{
			Factory.CreateTariffData(Universal.Constants.TariffTypes.Import, "010121000");
			Factory.CreateTariffData(Universal.Constants.TariffTypes.Export, "010121000");
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_Tariff = "010121000";
			var layout = new JPBillLayouts().Layout;
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			AssertVisibility();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			AssertVisibility();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			AssertVisibility();

			void AssertVisibility()
			{
				CombineAssertions("Control Visibilities", () =>
				{
					controlBagsVisibility.ForEach(control =>
					{
						var manifestType = header.AMA_ManifestType;
						if (control.Value.Contains(manifestType))
						{
							AssertEquals($"{control.Key.ControlName} should be visible for {manifestType}", true, layout.IsVisible(control.Key, bill));
						}
						else
						{
							AssertEquals($"{control.Key.ControlName} should not be visible for {manifestType}", false, layout.IsVisible(control.Key, bill));
						}
					});
				});
			}
		}

		readonly Dictionary<ControlReference, ImmutableHashSet<string>> controlBagsVisibility = new Dictionary<ControlReference, ImmutableHashSet<string>>()
		{
			{
				CommonBillControlBag.Instance.BillNumberTextBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				JPBillControlBag.Instance.FinalDestinationUserControl, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				JPBillControlBag.Instance.TariffFindBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				JPBillControlBag.Instance.RepresentativeHSCodeFindBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH
				)
			},
			{
				CommonBillControlBag.Instance.DischargePortCodeFindBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				CommonBillControlBag.Instance.ManifestQtyCalcDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				JPBillControlBag.Instance.CustomsWeightCalcDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				CommonBillControlBag.Instance.GoodsDescriptionTextBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				CommonBillControlBag.Instance.NetWeightCalcDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				JPBillControlBag.Instance.CustomsNetWeightCalcDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				CommonBillControlBag.Instance.VolumeCalcDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				JPBillControlBag.Instance.CustomsVolumeCalcDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				JPBillControlBag.Instance.SpecialCargoCodeFindBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				CommonBillControlBag.Instance.GoodsLocationCodeFindBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				JPBillControlBag.Instance.GoodsOriginCodeFindBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				CommonBillControlBag.Instance.CargoTypeDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				CommonBillControlBag.Instance.MarksAndNumbersTextBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				CommonBillControlBag.Instance.RemarksTextBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				CommonBillControlBag.Instance.FreightValueConvertToLocalCurrencyControl, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				CommonBillControlBag.Instance.TransportValueConvertToLocalCurrencyControl, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
		};
	}
}
