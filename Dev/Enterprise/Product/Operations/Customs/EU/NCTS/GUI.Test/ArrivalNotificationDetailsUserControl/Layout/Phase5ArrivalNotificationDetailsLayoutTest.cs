using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5ArrivalNotificationDetailsLayout))]
	sealed class Phase5ArrivalNotificationDetailsLayoutTest : LayoutsAbstractTest
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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ArrivalNotificationDetailsLayoutBuilder<Business.NctsHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (ArrivalNotificationDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
				yield return (ArrivalNotificationDetailsControlBag.Instance.LocalReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (ArrivalNotificationDetailsControlBag.Instance.MrnTextBox, ControlWidthClass.Long);
				yield return (ArrivalNotificationDetailsControlBag.Instance.DestinationCustomsOfficeCodeCodeFindBox, ControlWidthClass.Long);
				yield return (ArrivalNotificationDetailsControlBag.Instance.ArrivalDateDateTimeOffsetEdit, ControlWidthClass.Medium);
				yield return (ArrivalNotificationDetailsControlBag.Instance.AuthorizationCodeDropEdit, ControlWidthClass.Long);
				yield return (ArrivalNotificationDetailsControlBag.Instance.NumberCodeFindBox, ControlWidthClass.Long);
				yield return (ArrivalNotificationDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Auto);
				yield return (ArrivalNotificationDetailsControlBag.Instance.IncidentFlagDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (ArrivalNotificationDetailsControlBag.Instance.DestinationTraderDocAddressControl, ControlWidthClass.Auto);
			}
		}
	}
}
