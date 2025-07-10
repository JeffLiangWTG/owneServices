using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	[TestedType(typeof(ESH7BillLayout))]
	sealed class ESH7BillLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 3;

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<Business.AsycudaBill>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
				yield return (EUH7BillControlBag.Instance.AdditionalProcedureDropEdit, ControlWidthClass.Long);
				yield return (ESH7AsycudaBillControlBag.Instance.H7MovementReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (ESH7AsycudaBillControlBag.Instance.G3MovementReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OriginCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FinalDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);
				yield return (EUH7BillControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Long);
				yield return (EUH7BillControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.BillStatusDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (ESH7AsycudaBillControlBag.Instance.G3LocalReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.MarksAndNumbersTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.UCRNumberTextBox, ControlWidthClass.Long);
				yield return (EUH7BillControlBag.Instance.GoodsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (ESH7AsycudaBillControlBag.Instance.DocumentationRequiredTextBox, ControlWidthClass.Long);
				yield return (EUH7BillControlBag.Instance.StandAloneDeclarationUserControl, ControlWidthClass.Long);
			}
		}

		public void TestCaptions()
		{
			CombineAssertions(() =>
			{
				AssertCaption(EUH7BillControlBag.Instance.AdditionalProcedureDropEdit, expectedCaption: "Add. Procedure(s)");
				AssertCaption(CommonBillControlBag.Instance.GrossWeightCalcDropEdit, expectedCaption: "Gross Mass");
				AssertCaption(EUH7BillControlBag.Instance.LocationOfGoodsUserControl, expectedCaption: "Location of Goods");
			});

			void AssertCaption(ControlReference controlReference, string expectedCaption)
			{
				LayoutForTesting.TryGetCaption(controlReference, null, out var captionData);
				AssertEquals($"Caption for {controlReference.ControlName}", expectedCaption, captionData?.Caption);
			}
		}
	}
}
