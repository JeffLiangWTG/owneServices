using System.Collections.Generic;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(ConsolidatedDeclarationLayoutProvider))]
	sealed class ConsolidatedDeclarationLayoutProviderTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new Customs.GUI.ConsolidatedDeclarationLayoutBuilder<ConsolidatedDeclaration>();

		IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.ConsolidatedDeclarationControlBag.Instance.JobNumberTextBox, ControlWidthClass.Medium);
				yield return (ConsolidatedDeclarationControlBag.Instance.ConsolidatedDeclarationDetailsUserControl, ControlWidthClass.Auto);
				yield return (ConsolidatedDeclarationControlBag.Instance.PaymentStatusTextBox, ControlWidthClass.Medium);
				yield return (Customs.GUI.ConsolidatedDeclarationControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (ConsolidatedDeclarationControlBag.Instance.EntryStyleDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ConsolidatedDeclarationControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ConsolidatedDeclarationControlBag.Instance.ImporterGuidFindBox, ControlWidthClass.Long);
				yield return (ConsolidatedDeclarationControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Long);
				yield return (ConsolidatedDeclarationControlBag.Instance.VoyageFlightNoTextBox, ControlWidthClass.Medium);
				yield return (Customs.GUI.ConsolidatedDeclarationControlBag.Instance.DischargeETADateEdit, ControlWidthClass.Medium);
				yield return (Customs.GUI.ConsolidatedDeclarationControlBag.Instance.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
				yield return (Customs.GUI.ConsolidatedDeclarationControlBag.Instance.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
			}
		}
	}
}
