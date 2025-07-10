using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentGoodItemsSupportingDocumentsLayout))]
	sealed class HouseConsignmentGoodItemsSupportingDocumentsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (HouseConsignmentGoodItemsSupportingDocumentsControlBag.Instance.SequenceNumberTextBox, ControlWidthClass.Medium);
				yield return (HouseConsignmentGoodItemsSupportingDocumentsControlBag.Instance.StatusLabel, ControlWidthClass.Long);
				yield return (HouseConsignmentGoodItemsSupportingDocumentsControlBag.Instance.DocTypeCodeFindBox, ControlWidthClass.Long);
				yield return (HouseConsignmentGoodItemsSupportingDocumentsControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (HouseConsignmentGoodItemsSupportingDocumentsControlBag.Instance.ComplementInfoTextBox, ControlWidthClass.Long);
			}
		}

		public void TestStatusLabelCaption()
		{
			var layout = new HouseConsignmentGoodItemsSupportingDocumentsLayout().Layout;
			var supportingDocument = Factory.New<NctsSupportingDocument>();
			var control = HouseConsignmentGoodItemsSupportingDocumentsControlBag.Instance.StatusLabel;
			CombineAssertions(() =>
			{
				supportingDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				layout.TryGetCaption(control, supportingDocument, out var resourceStringData);
				AssertEquals("CSI_Status = 'DEC'", "Declared Value", resourceStringData.Caption);
				supportingDocument.CSI_Status = NctsUnloadedStateList.Codes.NEW;
				layout.TryGetCaption(control, supportingDocument, out resourceStringData);
				AssertEquals("CSI_Status = 'NEW'", "New Value", resourceStringData.Caption);
			});
		}

		public void TestStatusLabelVisibility()
		{
			var layout = new HouseConsignmentGoodItemsSupportingDocumentsLayout().Layout;
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType("A");
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var arrivalSupportingDocument = arrivalHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().SupportingDocuments.AddNew();
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.SetMovementType("D");
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var departureSupportingDocument = departureHeader.Bills.AddNew().GoodsItems.AddNew().SupportingDocuments.AddNew();
			var control = HouseConsignmentGoodItemsSupportingDocumentsControlBag.Instance.StatusLabel;
			CombineAssertions(() =>
			{
				arrivalSupportingDocument.CSI_Status = ZString.Empty;
				departureSupportingDocument.CSI_Status = ZString.Empty;
				AssertEquals("CSI_Status empty arrival", false, layout.IsVisible(control, arrivalSupportingDocument));
				AssertEquals("CSI_Status empty departure", false, layout.IsVisible(control, departureSupportingDocument));
				arrivalSupportingDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				departureSupportingDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("CSI_Status = 'DEC' arrival", true, layout.IsVisible(control, arrivalSupportingDocument));
				AssertEquals("CSI_Status = 'DEC' departure", false, layout.IsVisible(control, departureSupportingDocument));
				arrivalSupportingDocument.CSI_Status = NctsUnloadedStateList.Codes.NEW;
				departureSupportingDocument.CSI_Status = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("CSI_Status = 'NEW' arrival", true, layout.IsVisible(control, arrivalSupportingDocument));
				AssertEquals("CSI_Status = 'NEW' departure", false, layout.IsVisible(control, departureSupportingDocument));
				arrivalSupportingDocument.CSI_Status = "-X-";
				departureSupportingDocument.CSI_Status = "-X-";
				AssertEquals("CSI_Status invalid arrival", false, layout.IsVisible(control, arrivalSupportingDocument));
				AssertEquals("CSI_Status invalid departure", false, layout.IsVisible(control, departureSupportingDocument));
			});
		}

		protected override Type ExpectedGridUserControlType => typeof(HouseConsignmentGoodItemsSupportingDocumentsGridUserControl);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new HouseConsignmentGoodItemsSupportingDocumentsLayoutBuilder<NctsSupportingDocument>();
	}
}
