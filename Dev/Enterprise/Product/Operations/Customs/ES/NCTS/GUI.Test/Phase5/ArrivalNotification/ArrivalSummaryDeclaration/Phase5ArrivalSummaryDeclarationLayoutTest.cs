using System.Collections.Generic;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5ArrivalSummaryDeclarationLayout))]
	class Phase5ArrivalSummaryDeclarationLayoutTest : LayoutsAbstractTest
	{
		public void TestPreviousSummaryDeclarationTextBoxVisibility()
		{
			var previousSummaryReference = Phase5ArrivalSummaryDeclarationControlBag.Instance.PreviousSummaryDeclarationTextBox;
			var header = Factory.New<NctsHeader>();
			var esHeader = header.ESNctsHeader;
			CombineAssertions(() =>
			{
				esHeader.CEN_SummaryType = "AH";
				AssertEquals("PreviousSummaryDeclarationTextBox not visible when not SP Summary Type", false, LayoutForTesting.IsVisible(previousSummaryReference, header));

				esHeader.CEN_SummaryType = "SP";
				AssertEquals("PreviousSummaryDeclarationTextBox visible when SP Summary Type", true, LayoutForTesting.IsVisible(previousSummaryReference, header));
			});
		}

		public void TestG4PreviousDocumentGroupBoxVisibility()
		{
			var previousSummaryReference = Phase5ArrivalSummaryDeclarationControlBag.Instance.G4PreviousDocumentGroupBox;
			var header = Factory.New<NctsHeader>();
			var esHeader = header.ESNctsHeader;
			CombineAssertions(() =>
			{
				esHeader.CEN_SummaryType = "AH";
				AssertEquals("G4PreviousDocumentGroupBox not visible when not GP Summary Type", false, LayoutForTesting.IsVisible(previousSummaryReference, header));

				esHeader.CEN_SummaryType = "GP";
				AssertEquals("G4PreviousDocumentGroupBox visible when GP Summary Type", true, LayoutForTesting.IsVisible(previousSummaryReference, header));
			});
		}

		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Phase5ArrivalSummaryDeclarationControlBag.Instance.SummaryTypeDropEdit, ControlWidthClass.Auto);
				yield return (Phase5ArrivalSummaryDeclarationControlBag.Instance.PreviousSummaryDeclarationTextBox, ControlWidthClass.Auto);
			}
		}

		static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (Phase5ArrivalSummaryDeclarationControlBag.Instance.G4PreviousDocumentGroupBox, ControlWidthClass.LongNoCaption);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new Phase5ArrivalSummaryDeclarationLayoutBuilder<NctsHeader>();
	}
}
