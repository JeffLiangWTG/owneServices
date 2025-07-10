using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(VehicleControlBag))]
	public class VehicleControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(VehicleControlBag.NameTextBox);
				yield return nameof(VehicleControlBag.VehicleIDNumberTextBox);
				yield return nameof(VehicleControlBag.ExhaustVolumeCalcEdit);
				yield return nameof(VehicleControlBag.ModelYearTextBox);
				yield return nameof(VehicleControlBag.ManufacturingCountryCodeFindBox);
				yield return nameof(VehicleControlBag.SeatCapacityCalcEdit);
				yield return nameof(VehicleControlBag.FirstRegistrationDateEdit);
				yield return nameof(VehicleControlBag.CurrentRegistrationDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => VehicleControlBag.Instance;
	}
}
