using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(ArrivalNotificationDetailsControlBag))]
	sealed class ArrivalNotificationDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ArrivalNotificationDetailsUserControl.ArrivalGoodsLocationCodeFindBox);
				yield return nameof(ArrivalNotificationDetailsUserControl.RepresentativeTraderZDocAddressControl);
				yield return nameof(ArrivalNotificationDetailsUserControl.BrokerCodeFindBox);
				yield return nameof(ArrivalNotificationDetailsUserControl.CertificateDropEdit);
				yield return nameof(ArrivalNotificationDetailsControlBag.AdditionalArrivalNotificationDetailsUserControl);
				yield return nameof(ArrivalNotificationDetailsControlBag.TrainingCheckBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ArrivalNotificationDetailsControlBag.Instance;
	}
}
