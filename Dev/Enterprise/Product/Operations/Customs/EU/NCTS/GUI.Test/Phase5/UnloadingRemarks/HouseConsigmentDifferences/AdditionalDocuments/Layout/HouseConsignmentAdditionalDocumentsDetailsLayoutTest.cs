using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentAdditionalDocumentsLayout))]
	sealed class HouseConsignmentAdditionalDocumentsDetailsLayoutTest : LayoutsAbstractTest
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
				yield return (HouseConsignmentAdditionalDocumentsControlBag.Instance.SequenceNumberTextBox, ControlWidthClass.Medium);
				yield return (HouseConsignmentAdditionalDocumentsControlBag.Instance.StatusLabel, ControlWidthClass.Long);
				yield return (HouseConsignmentAdditionalDocumentsControlBag.Instance.KindDropEdit, ControlWidthClass.Long);
				yield return (HouseConsignmentAdditionalDocumentsControlBag.Instance.DocTypeCodeFindBox, ControlWidthClass.Long);
				yield return (HouseConsignmentAdditionalDocumentsControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (HouseConsignmentAdditionalDocumentsControlBag.Instance.TextTextBox, ControlWidthClass.Long);
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(HouseConsignmentAdditionalDocumentsOverviewUserControl);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new HouseConsignmentAdditionalDocumentsLayoutBuilder<NctsBillAdditionalDocument>();
	}
}
