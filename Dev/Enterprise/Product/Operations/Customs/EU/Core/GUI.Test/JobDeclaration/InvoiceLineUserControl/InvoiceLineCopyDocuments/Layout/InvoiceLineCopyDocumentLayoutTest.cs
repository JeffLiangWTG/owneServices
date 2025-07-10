using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoiceLineCopyDocumentLayout))]
	sealed class InvoiceLineCopyDocumentLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				var euBag = InvoiceLineCopyDocumentControlBag.Instance;

				yield return new List<(ControlReference, ControlWidthClass)>
				{
					(euBag.InvoiceNumberDropEditGroupBox, ControlWidthClass.LongNoCaption),
				};
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new InvoiceLineCopyDocumentLayoutBuilder();
	}
}
