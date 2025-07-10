using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ImportAdditionalInformationDetailsLayout))]
	sealed class ImportAdditionalInformationDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new AdditionalInformationDetailsLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (AdditionalInformationDetailsControlBag.Instance.KindDropEdit, ControlWidthClass.Long);
				yield return (AdditionalInformationDetailsControlBag.Instance.FullTypeCodeFindBox, ControlWidthClass.Long);
				yield return (AdditionalInformationDetailsControlBag.Instance.ReferenceTextBox, ControlWidthClass.Long);
				yield return (AdditionalInformationDetailsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
			}
		}
	}
}
