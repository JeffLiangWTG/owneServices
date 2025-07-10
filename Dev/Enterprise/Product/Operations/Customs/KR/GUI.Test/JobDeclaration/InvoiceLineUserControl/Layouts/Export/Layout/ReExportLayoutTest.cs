using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ReExportLayout))]
	sealed class ReExportLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

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
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Medium);
			}
		}
	}
}
