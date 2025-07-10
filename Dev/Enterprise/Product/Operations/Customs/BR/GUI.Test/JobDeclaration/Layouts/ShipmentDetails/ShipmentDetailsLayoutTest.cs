using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(ShipmentDetailsLayout))]
	sealed class ShipmentDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestCargoArrivalUserControlVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				AssertCargoArrivalUserControlVisibility(true);

				declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertCargoArrivalUserControlVisibility(true);

				declaration.JE_TransportMode = TransportTypeList.Codes.River;
				AssertCargoArrivalUserControlVisibility(true);

				declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
				AssertCargoArrivalUserControlVisibility(true);

				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				AssertCargoArrivalUserControlVisibility(true);

				declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
				AssertCargoArrivalUserControlVisibility(false);

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertCargoArrivalUserControlVisibility(true);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				AssertCargoArrivalUserControlVisibility(false);

				void AssertCargoArrivalUserControlVisibility(bool visibility)
				{
					AssertEquals($"JE_MessageType={declaration.JE_MessageType}, JE_TransportMode={declaration.JE_TransportMode}",
						visibility, Layout.IsVisible(ShipmentDetailsControlBag.Instance.CargoArrivalUserControl, declaration));
				}
			});
		}

		public void TestGoodsOriginCodeFindBoxVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				AssertGoodsOriginCodeFindBoxVisibility(false);

				declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertGoodsOriginCodeFindBoxVisibility(true);

				declaration.JE_TransportMode = TransportTypeList.Codes.River;
				AssertGoodsOriginCodeFindBoxVisibility(false);

				declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
				AssertGoodsOriginCodeFindBoxVisibility(false);

				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				AssertGoodsOriginCodeFindBoxVisibility(true);

				declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
				AssertGoodsOriginCodeFindBoxVisibility(false);

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertGoodsOriginCodeFindBoxVisibility(true);

				declaration.JE_DispatchModality = DispatchModalityCodes.Codes.Normal;
				AssertGoodsOriginCodeFindBoxVisibility(false);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				AssertGoodsOriginCodeFindBoxVisibility(true);

				void AssertGoodsOriginCodeFindBoxVisibility(bool visibility)
				{
					AssertEquals($"JE_MessageType={declaration.JE_MessageType}, JE_TransportMode={declaration.JE_TransportMode}, JE_DispatchModality={declaration.JE_DispatchModality}",
						visibility, Layout.IsVisible(Customs.GUI.ShipmentDetailsControlBag.Instance.GoodsOriginCodeFindBox, declaration));
				}
			});
		}

		public void TestControlVisibilities()
		{
			CombineAssertions(() =>
			{
				AssertControlVisibilities(BRJobMessageTypeList.Codes.Import, ExpectedControlsVisibilities.Select(x => (x.controlReference, x.visibleForImport)));
				AssertControlVisibilities(BRJobMessageTypeList.Codes.ImportSiscomex, ExpectedControlsVisibilities.Select(x => (x.controlReference, x.visibleForImportSiscomex)));
				AssertControlVisibilities(BRJobMessageTypeList.Codes.ImportLicense, ExpectedControlsVisibilities.Select(x => (x.controlReference, x.visibleForImportLicense)));
				AssertControlVisibilities(BRJobMessageTypeList.Codes.Export, ExpectedControlsVisibilities.Select(x => (x.controlReference, x.visibleForExport)));
				AssertControlVisibilities(BRJobMessageTypeList.Codes.LPCO, ExpectedControlsVisibilities.Select(x => (x.controlReference, x.visibleForLPCO)));

				void AssertControlVisibilities(string messageType, IEnumerable<(ControlReference controlReference, bool visibility)> visibilities)
				{
					declaration.JE_MessageType = messageType;

					foreach (var (controlReference, visibility) in visibilities)
					{
						AssertEquals($"Visibility of {controlReference.Name} for {messageType}", visibility, Layout.IsVisible(controlReference, declaration));
					}
				}
			});

			var controlsNotTested = IncludedControlsPerColumn.SelectMany(x => x).Select(x => x.Item1).Except(ExpectedControlsVisibilities.Select(x => x.controlReference));
			Assert("Visibilities for the following controls should be defined in ExpectedControlsVisibilities:\r\n" + string.Join("\r\n\t", controlsNotTested.Select(x => x.Name)), !controlsNotTested.Any());
		}

		IEnumerable<(ControlReference controlReference, bool visibleForImport, bool visibleForImportSiscomex, bool visibleForImportLicense, bool visibleForExport, bool visibleForLPCO)> ExpectedControlsVisibilities
		{
			get
			{
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.HouseBillParcelPostTextBox, true, true, false, true, false);
				yield return (ShipmentDetailsControlBag.Instance.UcrAndBillTypeUserControl, false, true, false, false, false);
				yield return (ShipmentDetailsControlBag.Instance.CargoArrivalUserControl, false, true, false, false, false);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.GoodsOriginCodeFindBox, false, true, true, false, false);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsOriginUserControl, true, true, false, true, false);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsFinalDestinationUserControl, true, true, false, true, false);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.GoodsDescriptionTextBox, true, true, true, true, false);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.OwnersReferenceTextBox, true, true, true, true, false);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.WeightCalcDropEdit, true, true, false, true, false);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.VolumeCalcDropEdit, true, true, false, true, false);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.TotalNoOfPiecesCalcEdit, true, true, false, true, false);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ContainerCountCalcEdit, true, true, false, true, false);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.TotalNoOfPacksCalcDropEdit, true, true, false, true, false);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsUserControl, true, true, false, true, false);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl, true, true, false, true, true);
			}
		}

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstAndOnlyColumnControls;
			}
		}

		protected override int ControlBagCount => 2;

		IEnumerable<(ControlReference, ControlWidthClass)> FirstAndOnlyColumnControls
		{
			get
			{
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.UcrAndBillTypeUserControl, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.CargoArrivalUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.GoodsOriginCodeFindBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.OwnersReferenceTextBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.TotalNoOfPiecesCalcEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ContainerCountCalcEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.TotalNoOfPacksCalcDropEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		}
		JobDeclaration declaration;

		public PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new ShipmentDetailsLayout()).Layout);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentDetailsLayoutBuilder<JobDeclaration>();

		PanelLayout layout;
	}
}
