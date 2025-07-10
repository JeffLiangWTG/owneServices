using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5HouseConsignmentDifferencesSupportingDocumentLayoutWithGrid))]
	sealed class Phase5HouseConsignmentDifferencesSupportingDocumentLayoutWithGridTest : LayoutsAbstractTest
	{
		public void TestStatusLabelCaption()
		{
			var document = Factory.New<NctsSupportingDocument>();
			var layout = ((IPanelLayoutProvider)new Phase5HouseConsignmentDifferencesSupportingDocumentLayoutWithGrid()).Layout;
			var control = SupportingDocumentControlBag.Instance.StatusLabel;

			CombineAssertions(() =>
			{
				document.CSI_Status = SupportingDocumentStatusList.Codes.DEC;
				layout.TryGetCaption(control, document, out var resourceStringData);
				AssertEquals("CSI_Status = 'DEC'", "Declared Value", resourceStringData.Caption);

				document.CSI_Status = SupportingDocumentStatusList.Codes.NEW;
				layout.TryGetCaption(control, document, out resourceStringData);
				AssertEquals("CSI_Status = 'NEW'", "New Value", resourceStringData.Caption);

				document.CSI_Status = "INV";
				layout.TryGetCaption(control, document, out resourceStringData);
				AssertEquals("CSI_Status Invalid", "Value", resourceStringData.Caption);
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

		protected override Type ExpectedGridUserControlType => typeof(HouseConsignmentDifferencesSupportingDocumentGridUserControl);

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (SupportingDocumentControlBag.Instance.LineNoCalcEdit, ControlWidthClass.Auto);
				yield return (SupportingDocumentControlBag.Instance.StatusLabel, ControlWidthClass.Long);
				yield return (SupportingDocumentControlBag.Instance.TypeCodeFindBox, ControlWidthClass.Long);
				yield return (SupportingDocumentControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (SupportingDocumentControlBag.Instance.ComplementTextBox, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new SupportingDocumentLayoutBuilder<NctsSupportingDocument>();
	}
}
