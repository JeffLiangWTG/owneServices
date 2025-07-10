using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(CommonEntryDetailsLayouts))]
	sealed class CommonEntryDetailsLayoutsTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonEntryDetailsLayoutBuilder<JobDeclaration>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonEntryDetailsControlBag.Instance.NoPacksCalcEdit, ControlWidthClass.Auto);
				yield return (CommonEntryDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonEntryDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonEntryDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonEntryDetailsControlBag.Instance.DutyCalcEdit, ControlWidthClass.Auto);
				yield return (CommonEntryDetailsControlBag.Instance.VatCalcEdit, ControlWidthClass.Auto);
				yield return (CommonEntryDetailsControlBag.Instance.EntryLinesCountCalcEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonEntryDetailsControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Auto);
				yield return (CommonEntryDetailsControlBag.Instance.SubmittedDateDateEdit, ControlWidthClass.Auto);
				yield return (CommonEntryDetailsControlBag.Instance.MRNTextBox, ControlWidthClass.Auto);
				yield return (CommonEntryDetailsControlBag.Instance.ReleaseDateDateEdit, ControlWidthClass.Auto);
				yield return (CommonEntryDetailsControlBag.Instance.EntryStatusDropEdit, ControlWidthClass.Auto);
			}
		}
	}
}
