using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(MessageAndCustomsStatusWithOverrideLayout))]
sealed class MessageAndCustomsStatusWithOverrideLayoutTest : LayoutsAbstractTest
{
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
			yield return (commonBag.MessageAndCustomsStatusGroupBox, ControlWidthClass.LongControl);
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new MessageAndCustomsStatusWithOverrideLayoutBuilder();

	EntryInstructionDetailsControlBag commonBag => EntryInstructionDetailsControlBag.Instance;
}
