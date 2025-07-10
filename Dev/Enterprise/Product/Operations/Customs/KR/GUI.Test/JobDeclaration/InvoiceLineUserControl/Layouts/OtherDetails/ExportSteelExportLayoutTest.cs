using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ExportSteelExportLayout))]
	sealed class ExportSteelExportLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new InvoiceLineOtherDetailsLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (InvoiceLineOtherDetailsControlBag.Instance.ApprovalNoTextBox, ControlWidthClass.Long);
				yield return (InvoiceLineOtherDetailsControlBag.Instance.SteelExportEffectiveDateUserControl, ControlWidthClass.Auto);
			}
		}
	}
}
