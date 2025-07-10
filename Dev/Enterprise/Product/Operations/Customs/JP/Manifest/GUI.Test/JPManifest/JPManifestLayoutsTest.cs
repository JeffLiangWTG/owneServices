using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(JPManifestLayouts))]
	sealed class JPManifestLayoutsTest : LayoutsAbstractTest
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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new JPManifestLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.ManifestTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.NatureDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
				yield return (JPManifestControlBag.Instance.IsCoLoadedCheckBox, ControlWidthClass.Auto);
				yield return (JPManifestControlBag.Instance.IsSubConsolidationCheckBox, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.CustomsOfficeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.GoodsLocationCodeFindBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (JPManifestControlBag.Instance.CustomsAgentCodeFindBox, ControlWidthClass.Long);
				yield return (JPManifestControlBag.Instance.CustomsAgentCredentialGuidDropEdit, ControlWidthClass.Long);
				yield return (JPManifestControlBag.Instance.ViaLocationCodeFindBox, ControlWidthClass.Long);
				yield return (JPManifestControlBag.Instance.PortOfLoadingUserControl, ControlWidthClass.Long);
				yield return (JPManifestControlBag.Instance.PortOfDischargeUserControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.RadioCallSignTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.EstDepartureDateEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (JPManifestControlBag.Instance.InputReferenceTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierCodeTextBox, ControlWidthClass.Medium);
				yield return (JPManifestControlBag.Instance.ConsolidatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (JPManifestControlBag.Instance.BookingNumberTextBox, ControlWidthClass.Medium);
				yield return (JPManifestControlBag.Instance.MoveInDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (JPManifestControlBag.Instance.MasterBillMessageStatusDropEdit, ControlWidthClass.Long);
				yield return (JPManifestControlBag.Instance.MasterBillCustomsStatusDropEdit, ControlWidthClass.Long);
			}
		}

		public void TestVisibility()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var layout = new JPManifestLayouts().Layout;
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
							AssertEquals($"{control.Key.ControlName} should be visible for {manifestType}", true, layout.IsVisible(control.Key, header));
						}
						else
						{
							AssertEquals($"{control.Key.ControlName} should not be visible for {manifestType}", false, layout.IsVisible(control.Key, header));
						}
					});
				});
			}
		}

		readonly Dictionary<ControlReference, ImmutableHashSet<string>> controlBagsVisibility = new Dictionary<ControlReference, ImmutableHashSet<string>>()
		{
			{
				CommonManifestControlBag.Instance.ManifestTypeDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				CommonManifestControlBag.Instance.TransportModeDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				CommonManifestControlBag.Instance.NatureDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				CommonManifestControlBag.Instance.ManifestNumberFromMasterBillTextBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				JPManifestControlBag.Instance.IsCoLoadedCheckBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				JPManifestControlBag.Instance.IsSubConsolidationCheckBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.HCH
				)
			},
			{
				CommonManifestControlBag.Instance.CustomsOfficeDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH
				)
			},
			{
				CommonManifestControlBag.Instance.GoodsLocationCodeFindBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				JPManifestControlBag.Instance.CustomsAgentCodeFindBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				JPManifestControlBag.Instance.CustomsAgentCredentialGuidDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				JPManifestControlBag.Instance.ViaLocationCodeFindBox, ImmutableHashSet.Create<string>()
			},
			{
				JPManifestControlBag.Instance.PortOfLoadingUserControl, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				JPManifestControlBag.Instance.PortOfDischargeUserControl, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				CommonManifestControlBag.Instance.VoyageFlightTextBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.HCH
				)
			},
			{
				CommonManifestControlBag.Instance.RadioCallSignTextBox, ImmutableHashSet.Create<string>()
			},
			{
				CommonManifestControlBag.Instance.EstDepartureDateEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.HCH
				)
			},
			{
				JPManifestControlBag.Instance.InputReferenceTextBox, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				CommonManifestControlBag.Instance.MessageStatusDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				CommonManifestControlBag.Instance.CustomsStatusDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				CommonManifestControlBag.Instance.CarrierAddressControl, ImmutableHashSet.Create<string>()
			},
			{
				CommonManifestControlBag.Instance.CarrierCodeTextBox, ImmutableHashSet.Create<string>()
			},
			{
				JPManifestControlBag.Instance.ConsolidatorUserControl, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.HCH
				)
			},
			{
				JPManifestControlBag.Instance.BookingNumberTextBox, ImmutableHashSet.Create<string>()
			},
			{
				JPManifestControlBag.Instance.MoveInDestinationCodeFindBox, ImmutableHashSet.Create<string>()
			},
			{
				JPManifestControlBag.Instance.MasterBillCustomsStatusDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.HDF,
					JPManifestTypeCodeList.Codes.HCH
				)
			},
			{
				JPManifestControlBag.Instance.MasterBillMessageStatusDropEdit, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.HDF
				)
			},
		};
	}
}
