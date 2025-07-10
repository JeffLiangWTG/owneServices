using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(ServiceControlBag))]
	sealed class ServiceControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ServiceUserControl.MeasurementBasisDropEdit);
				yield return nameof(ServiceUserControl.ContractorCodeFindBox);
				yield return nameof(ServiceUserControl.NotesTextBox);
				yield return nameof(ServiceUserControl.ReferenceTextBox);
				yield return nameof(ServiceUserControl.DurationTimeEdit);
				yield return nameof(ServiceUserControl.ServiceCountCalcEdit);
				yield return nameof(ServiceUserControl.SubLocationTextBox);
				yield return nameof(ServiceUserControl.ServiceLocationAddressControl);
				yield return nameof(ServiceUserControl.CompletedDateEdit);
				yield return nameof(ServiceUserControl.BookedDateEdit);
				yield return nameof(ServiceUserControl.ServiceTypeDropEdit);
				yield return nameof(ServiceUserControl.RateAndCurrencyCalcFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ServiceControlBag.Instance;
	}
}
