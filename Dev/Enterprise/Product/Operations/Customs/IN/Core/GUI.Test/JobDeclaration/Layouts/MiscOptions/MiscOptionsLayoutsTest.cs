using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(MiscOptionsLayouts))]
sealed class MiscOptionsLayoutsTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn 
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonBag.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (CommonBag.BranchGuidFindBox, ControlWidthClass.Auto);
			yield return (CommonBag.BrokerCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonBag.PaymentPartyDropEdit, ControlWidthClass.Auto);
			yield return (CommonBag.PaidByDropEdit, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (INBag.NonStandardExchangeRateGroupBox, ControlWidthClass.LongControl);
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new MiscOptionsLayoutBuilder();

	CommonMiscOptionsControlBag CommonBag => CommonMiscOptionsControlBag.Instance;

	MiscOptionsControlBag INBag => MiscOptionsControlBag.Instance;
}
