using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(ArrivalNotificationDetailsControlBag))]
	sealed class ArrivalNotificationDetailsUserControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ArrivalNotificationDetailsControlBag.OverrideFreightDetailsCheckBox);
				yield return nameof(ArrivalNotificationDetailsControlBag.MrnTextBox);
				yield return nameof(ArrivalNotificationDetailsControlBag.LocalReferenceNumberTextBox);
				yield return nameof(ArrivalNotificationDetailsControlBag.DestinationCustomsOfficeCodeCodeFindBox);
				yield return nameof(ArrivalNotificationDetailsControlBag.ArrivalDateDateTimeOffsetEdit);
				yield return nameof(ArrivalNotificationDetailsControlBag.AuthorizationCodeDropEdit);
				yield return nameof(ArrivalNotificationDetailsControlBag.NumberCodeFindBox);
				yield return nameof(ArrivalNotificationDetailsControlBag.OwnerZGuidFindBox);
				yield return nameof(ArrivalNotificationDetailsControlBag.DischargeTypeDropEdit);
				yield return nameof(ArrivalNotificationDetailsControlBag.CarnetTotalPagesDropEdit);
				yield return nameof(ArrivalNotificationDetailsControlBag.DestinationTraderDocAddressControl);
				yield return nameof(ArrivalNotificationDetailsControlBag.LocationOfGoodsUserControl);
				yield return nameof(ArrivalNotificationDetailsControlBag.IncidentFlagDropEdit);
				yield return nameof(ArrivalNotificationDetailsControlBag.CommunicationLanguageDropEdit);
				yield return nameof(ArrivalNotificationDetailsControlBag.TransportMeansLabel);
				yield return nameof(ArrivalNotificationDetailsControlBag.TransportAtArrivalTypeDropEdit);
				yield return nameof(ArrivalNotificationDetailsControlBag.TransportAtArrivalIDTextBox);
				yield return nameof(ArrivalNotificationDetailsControlBag.TransportNationalityCodeFindBox);
				yield return nameof(ArrivalNotificationDetailsControlBag.StateOfSealsDropEdit);
				yield return nameof(ArrivalNotificationDetailsControlBag.AdditionalTextTextBox);
				yield return nameof(ArrivalNotificationDetailsControlBag.NationalInfoSeparatorUserControl);
				yield return nameof(ArrivalNotificationDetailsControlBag.GoodsLocationFromAuthorizationCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ArrivalNotificationDetailsControlBag.Instance;
	}
}
