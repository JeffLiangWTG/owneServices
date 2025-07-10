using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MiscRequestRelatedEntries5SGLayout))]
	sealed class MiscRequestRelatedEntries5SGLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new MiscRequestRelatedEntries5SGLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (MiscRequestRelatedEntriesControlBag.Instance.EntryNumberTextBox, ControlWidthClass.Long);
				yield return (MiscRequestRelatedEntriesControlBag.Instance.EntryDetailsTextBox, ControlWidthClass.Long);
			}
		}
	}
}
