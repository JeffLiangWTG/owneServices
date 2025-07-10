using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(FTAEntryLineDetailsLayout))]
	sealed class FTAEntryLineDetailsLayoutTest : LayoutsAbstractTest
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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new FTAEntryLineDetailsLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (FTAEntryLineDetailsControlBag.Instance.EntryLineNumberCalcEdit, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.FTASeqNoCalcEdit, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.COSplitOrderCalcEdit, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.COTotalNetWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.CONetWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.CONoTextBox, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.COIssueDateTextBox, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.CountryOfOriginCodeFindBox, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.AssociatedCOCountryCodeFindBox, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.TarrifRateCalcEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (FTAEntryLineDetailsControlBag.Instance.PreferenceCodeDropEdit, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.TariffTextBox, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.FTACOProductTypeDropEdit, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.ThirdCountryInvIssuedDropEdit , ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.ThirdCountryCodeCodeFindBox, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.COExporterNumberTextBox, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.COIssuingAgencyTypeDropEdit, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.COIssueAgencyNameTextBox, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.SupportingDocTypeDropEdit, ControlWidthClass.Auto);
				yield return (FTAEntryLineDetailsControlBag.Instance.COIssuerTypeDropEdit, ControlWidthClass.Auto);
			}
		}
	}
}
