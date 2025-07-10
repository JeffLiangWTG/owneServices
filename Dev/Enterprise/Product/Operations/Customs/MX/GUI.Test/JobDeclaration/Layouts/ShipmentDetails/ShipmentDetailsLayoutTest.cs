using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(ShipmentDetailsLayout))]
	sealed class ShipmentDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestGoodsOriginRegionCodeDropEditVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertGoodsOriginRegionCodeDropEditVisibility(false);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertGoodsOriginRegionCodeDropEditVisibility(true);

				void AssertGoodsOriginRegionCodeDropEditVisibility(bool visibility)
				{
					AssertEquals($"JE_MessageType={declaration.JE_MessageType}",
						visibility, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsOriginDropEdit, declaration));
				}
			});
		}

		public void TestGoodsDestinationRegionCodeDropEditVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertGoodsDestinationRegionCodeDropEditVisibility(true);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertGoodsDestinationRegionCodeDropEditVisibility(false);

				void AssertGoodsDestinationRegionCodeDropEditVisibility(bool visibility)
				{
					AssertEquals($"JE_MessageType={declaration.JE_MessageType}",
						visibility, Layout.IsVisible(ShipmentDetailsControlBag.Instance.GoodsDestinationDropEdit, declaration));
				}
			});
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
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.GoodsDestinationDropEdit, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.GoodsOriginDropEdit, ControlWidthClass.Auto);
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
		}
		JobDeclaration declaration;

		public PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new ShipmentDetailsLayout()).Layout);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentDetailsLayoutBuilder<JobDeclaration>();

		PanelLayout layout;
	}
}
