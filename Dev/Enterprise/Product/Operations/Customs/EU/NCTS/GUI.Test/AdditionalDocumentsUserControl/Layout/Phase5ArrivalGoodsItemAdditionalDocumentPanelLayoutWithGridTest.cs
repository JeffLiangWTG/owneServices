using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid))]
	sealed class Phase5ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGridTest : LayoutsAbstractTest
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

		protected override Type ExpectedGridUserControlType => typeof(ArrivalGoodsItemAdditionalDocumentsGridUserControl);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new AdditionalDocumentLayoutBuilder<NctsAdditionalInfo>();

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (AdditionalDocumentControlBag.Instance.LineNoCalcEdit, ControlWidthClass.Auto);
				yield return (AdditionalDocumentControlBag.Instance.StatusLabel, ControlWidthClass.Long);
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

		public void TestStatusLabelCaption()
		{
			var layout = new Phase5ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid().Layout;
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

		public void TestStatusLabelVisibility()
		{
			var layout = new Phase5ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid().Layout;
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType("A");
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var arrivalAdditionalInfo = arrivalHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().AdditionalInfos.AddNew();
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.SetMovementType("D");
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var departureHeaderAdditionalInfo = departureHeader.Bills.AddNew().GoodsItems.AddNew().AdditionalInfos.AddNew();
			var control = AdditionalDocumentControlBag.Instance.StatusLabel;
			CombineAssertions(() =>
			{
				arrivalAdditionalInfo.CSI_Status = ZString.Empty;
				departureHeaderAdditionalInfo.CSI_Status = ZString.Empty;
				AssertEquals("CSI_Status empty arrival", false, layout.IsVisible(control, arrivalAdditionalInfo));
				AssertEquals("CSI_Status empty departure", false, layout.IsVisible(control, departureHeaderAdditionalInfo));
				arrivalAdditionalInfo.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				departureHeaderAdditionalInfo.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("CSI_Status = 'DEC' arrival", true, layout.IsVisible(control, arrivalAdditionalInfo));
				AssertEquals("CSI_Status = 'DEC' departure", false, layout.IsVisible(control, departureHeaderAdditionalInfo));
				arrivalAdditionalInfo.CSI_Status = NctsUnloadedStateList.Codes.NEW;
				departureHeaderAdditionalInfo.CSI_Status = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("CSI_Status = 'NEW' arrival", true, layout.IsVisible(control, arrivalAdditionalInfo));
				AssertEquals("CSI_Status = 'NEW' departure", false, layout.IsVisible(control, departureHeaderAdditionalInfo));
				arrivalAdditionalInfo.CSI_Status = "-X-";
				departureHeaderAdditionalInfo.CSI_Status = "-X-";
				AssertEquals("CSI_Status invalid arrival", false, layout.IsVisible(control, arrivalAdditionalInfo));
				AssertEquals("CSI_Status invalid departure", false, layout.IsVisible(control, departureHeaderAdditionalInfo));
			});
		}

		public void TestLineNoVisibility()
		{
			var layout = new Phase5ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid().Layout;
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType("A");
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var arrivalAdditionalInfo = arrivalHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().AdditionalInfos.AddNew();
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.SetMovementType("D");
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var departureHeaderAdditionalInfo = departureHeader.Bills.AddNew().GoodsItems.AddNew().AdditionalInfos.AddNew();
			var control = AdditionalDocumentControlBag.Instance.LineNoCalcEdit;
			CombineAssertions(() =>
			{
				AssertEquals("arrival", true, layout.IsVisible(control, arrivalAdditionalInfo));
				AssertEquals("departure", false, layout.IsVisible(control, departureHeaderAdditionalInfo));
			});
		}

		public void TestReferenceNumberTextBoxVisibility()
		{
			var layout = new Phase5ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid().Layout;
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

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				AssertEquals("CSI_SubType = 'TRA'", true, layout.IsVisible(control, additionalInfo));
			});
		}

		public void TestDescriptionMultilineTextBoxVisibility()
		{
			var layout = new Phase5ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid().Layout;
			var additionalInfo = Factory.New<NctsAdditionalInfo>();
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
