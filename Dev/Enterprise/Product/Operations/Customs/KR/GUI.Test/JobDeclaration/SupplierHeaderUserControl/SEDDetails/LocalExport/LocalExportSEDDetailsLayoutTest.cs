using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(LocalExportSEDDetailsLayout))]
	sealed class LocalExportSEDDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new SEDDetailsLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (SEDDetailsGroupBoxControlBag.Instance.SupplierGroupBox, ControlWidthClass.Auto);
				yield return (SEDDetailsGroupBoxControlBag.Instance.ManufacturerGroupBox, ControlWidthClass.Auto);
				yield return (SEDDetailsGroupBoxControlBag.Instance.ImporterGroupBox, ControlWidthClass.Auto);
			}
		}
	}
}
