using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoiceLineAdditionalInformationDetailsLayout))]
	sealed class InvoiceLineAdditionalInformationDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new AdditionalInformationDetailsLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (AdditionalInformationDetailsControlBag.Instance.KindDropEdit, ControlWidthClass.Long);
				yield return (AdditionalInformationDetailsControlBag.Instance.FullTypeCodeFindBox, ControlWidthClass.Long);
				yield return (AdditionalInformationDetailsControlBag.Instance.ReferenceTextBox, ControlWidthClass.Long);
				yield return (AdditionalInformationDetailsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
				yield return (AdditionalInformationDetailsControlBag.Instance.DetailTextBox, ControlWidthClass.Long);
				yield return (AdditionalInformationDetailsControlBag.Instance.CurrencyDropEdit, ControlWidthClass.Long);
				yield return (AdditionalInformationDetailsControlBag.Instance.AmountCalcEdit, ControlWidthClass.Long);
			}
		}
	}
}
