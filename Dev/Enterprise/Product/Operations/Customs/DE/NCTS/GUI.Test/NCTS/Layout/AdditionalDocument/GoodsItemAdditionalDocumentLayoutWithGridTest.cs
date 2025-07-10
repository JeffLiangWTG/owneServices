using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	[TestedType(typeof(GoodsItemAdditionalDocumentLayoutWithGrid))]
	sealed class GoodsItemAdditionalDocumentLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(DepartureGoodsItemAdditionalDocumentsGridUserControl);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new AdditionalDocumentLayoutBuilder<Business.NctsAdditionalInfo>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (AdditionalDocumentControlBag.Instance.KindDropEdit, ControlWidthClass.Long);
				yield return (AdditionalDocumentControlBag.Instance.TypeCodeFindBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (AdditionalDocumentControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (AdditionalDocumentControlBag.Instance.DescriptionMultilineTextBox, ControlWidthClass.Long);
			}
		}

		public void TestReferenceNumberTextBoxVisibility()
		{
			var layout = new GoodsItemAdditionalDocumentLayoutWithGrid().Layout;
			var additionalInfo = Factory.New<Business.NctsAdditionalInfo>();
			var control = AdditionalDocumentControlBag.Instance.ReferenceNumberTextBox;

			CombineAssertions(() =>
			{
				additionalInfo.CSI_SubType = ZString.Empty;
				AssertEquals("CSI_SubType empty", false, layout.IsVisible(control, additionalInfo));

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("CSI_SubType = 'REF'", true, layout.IsVisible(control, additionalInfo));

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("CSI_SubType = 'INF'", false, layout.IsVisible(control, additionalInfo));

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				AssertEquals("CSI_SubType = 'TRA'", true, layout.IsVisible(control, additionalInfo));
			});
		}

		public void TestDescriptionMultilineTextBoxVisibility()
		{
			var layout = new GoodsItemAdditionalDocumentLayoutWithGrid().Layout;
			var additionalInfo = Factory.New<Business.NctsAdditionalInfo>();
			var control = AdditionalDocumentControlBag.Instance.DescriptionMultilineTextBox;

			CombineAssertions(() =>
			{
				AssertEquals("CSI_SubType empty", false, layout.IsVisible(control, additionalInfo));

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("CSI_SubType = 'REF'", false, layout.IsVisible(control, additionalInfo));

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("CSI_SubType = 'INF'", true, layout.IsVisible(control, additionalInfo));
			});
		}
	}
}
