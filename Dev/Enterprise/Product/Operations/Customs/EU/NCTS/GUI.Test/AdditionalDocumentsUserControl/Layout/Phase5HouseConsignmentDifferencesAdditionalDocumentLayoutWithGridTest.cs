using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5HouseConsignmentDifferencesAdditionalDocumentLayoutWithGrid))]
	sealed class Phase5HouseConsignmentDifferencesAdditionalDocumentLayoutWithGridTest : LayoutsAbstractTest
	{
		public void TestReferenceNumberTextBoxVisibility()
		{
			var layout = new Phase5HouseConsignmentDifferencesAdditionalDocumentLayoutWithGrid().Layout;
			var additionalInfo = Factory.New<NctsAdditionalInfo>();
			var control = AdditionalDocumentControlBag.Instance.ReferenceNumberTextBox;

			CombineAssertions(() =>
			{
				additionalInfo.CSI_SubType = ZString.Empty;
				AssertEquals("CSI_SubType empty", false, layout.IsVisible(control, additionalInfo));

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("CSI_SubType = 'REF'", true, layout.IsVisible(control, additionalInfo));

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("CSI_SubType = 'INF'", false, layout.IsVisible(control, additionalInfo));
			});
		}

		public void TestDescriptionTextBoxVisibility()
		{
			var layout = new Phase5HouseConsignmentDifferencesAdditionalDocumentLayoutWithGrid().Layout;
			var additionalInfo = Factory.New<NctsAdditionalInfo>();
			var control = AdditionalDocumentControlBag.Instance.DescriptionTextBox;

			CombineAssertions(() =>
			{
				AssertEquals("CSI_SubType empty", false, layout.IsVisible(control, additionalInfo));

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("CSI_SubType = 'REF'", false, layout.IsVisible(control, additionalInfo));

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("CSI_SubType = 'INF'", true, layout.IsVisible(control, additionalInfo));
			});
		}

		public void TestStatusLabelCaption()
		{
			var layout = new Phase5HouseConsignmentDifferencesAdditionalDocumentLayoutWithGrid().Layout;
			var additionalInfo = Factory.New<NctsAdditionalInfo>();
			var control = AdditionalDocumentControlBag.Instance.StatusLabel;

			CombineAssertions(() =>
			{
				additionalInfo.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				layout.TryGetCaption(control, additionalInfo, out var resourceStringData);
				AssertEquals("CSI_Status = 'DEC'", "Declared Value", resourceStringData.Caption);

				additionalInfo.CSI_Status = NctsUnloadedStateList.Codes.NEW;
				layout.TryGetCaption(control, additionalInfo, out resourceStringData);
				AssertEquals("CSI_Status = 'NEW'", "New Value", resourceStringData.Caption);
			});
		}

		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(Phase5HouseConsignmentDifferencesAdditionalDocumentGridUserControl);

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (AdditionalDocumentControlBag.Instance.LineNoCalcEdit, ControlWidthClass.Auto);
				yield return (AdditionalDocumentControlBag.Instance.StatusLabel, ControlWidthClass.Long);
				yield return (AdditionalDocumentControlBag.Instance.KindDropEdit, ControlWidthClass.Long);
				yield return (AdditionalDocumentControlBag.Instance.TypeCodeFindBox, ControlWidthClass.Long);
				yield return (AdditionalDocumentControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (AdditionalDocumentControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new AdditionalDocumentLayoutBuilder<NctsAdditionalInfo>();
	}
}
