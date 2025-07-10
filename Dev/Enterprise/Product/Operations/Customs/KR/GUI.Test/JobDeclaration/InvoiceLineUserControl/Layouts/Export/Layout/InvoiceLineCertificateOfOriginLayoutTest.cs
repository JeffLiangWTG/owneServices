using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(InvoiceLineCertificateOfOriginLayout))]
	sealed class InvoiceLineCertificateOfOriginLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new InvoiceLineDetailsLongCaptionLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
				yield return (InvoiceLineCertificateOfOriginControlBag.Instance.COOIndicatorDropEdit, ControlWidthClass.Long);
				yield return (InvoiceLineCertificateOfOriginControlBag.Instance.COODeterminationRuleDropEdit, ControlWidthClass.Long);
				yield return (InvoiceLineCertificateOfOriginControlBag.Instance.COOLabelLocationDropEdit, ControlWidthClass.Long);
				yield return (InvoiceLineCertificateOfOriginControlBag.Instance.FTATypeDropEdit, ControlWidthClass.Long);
			}
		}
	}
}
