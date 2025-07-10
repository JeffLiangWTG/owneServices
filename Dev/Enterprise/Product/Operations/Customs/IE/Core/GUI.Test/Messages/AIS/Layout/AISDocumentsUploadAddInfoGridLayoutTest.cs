using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(AISDocumentsUploadAddInfoGridLayout))]
	sealed class AISDocumentsUploadAddInfoGridLayoutTest : LayoutsAbstractTest
	{
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
				yield return (AISDocumentsUploadAddInfoGridControlBag.Instance.AddInfosGrid, ControlWidthClass.Auto);
				yield return (AISDocumentsUploadAddInfoGridControlBag.Instance.AddInfosIM483Grid, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new AISDocumentsUploadAddInfoGridLayoutBuilder<UploadDocumentsSendingAction>();
	}
}
