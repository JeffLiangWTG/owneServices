using System.Collections.Generic;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	[TestedType(typeof(Phase5ArrivalNotificationDetailsLayout))]
	sealed class Phase5ArrivalNotificationDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.NCTS.GUI.ArrivalNotificationDetailsLayoutBuilder<NctsHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				var euBag = EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance;
				var frBag = ArrivalNotificationDetailsControlBag.Instance;

				yield return (euBag.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
				yield return (euBag.LocalReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (euBag.MrnTextBox, ControlWidthClass.Long);
				yield return (euBag.DestinationCustomsOfficeCodeCodeFindBox, ControlWidthClass.Long);
				yield return (euBag.ArrivalDateDateTimeOffsetEdit, ControlWidthClass.Medium);
				yield return (euBag.AuthorizationCodeDropEdit, ControlWidthClass.Long);
				yield return (euBag.NumberCodeFindBox, ControlWidthClass.Long);
				yield return (euBag.LocationOfGoodsUserControl, ControlWidthClass.Auto);
				yield return (euBag.IncidentFlagDropEdit, ControlWidthClass.Long);
				yield return (frBag.ExpectedNextCustomsProcedureDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.DestinationTraderDocAddressControl, ControlWidthClass.Auto);
			}
		}
	}
}
