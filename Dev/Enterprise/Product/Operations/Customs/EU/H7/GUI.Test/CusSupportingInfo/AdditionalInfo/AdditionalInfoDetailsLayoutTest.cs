using System.Collections.Generic;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(AdditionalInfoDetailsLayout))]
	sealed class AdditionalInfoDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new AdditionalInfoDetailsLayoutBuilder<AdditionalInfo>();

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
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
				yield return (AdditionalInfoDetailsControlBag.Instance.AddInfoTypeCodeDropEdit, ControlWidthClass.Long);
				yield return (AdditionalInfoDetailsControlBag.Instance.AddInfoDescriptionTextBox, ControlWidthClass.Long);
			}
		}
	}
}
