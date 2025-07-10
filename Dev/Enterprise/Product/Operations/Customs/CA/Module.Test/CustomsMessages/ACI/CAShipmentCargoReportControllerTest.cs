using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CAShipmentCargoReportController))]
	sealed class CAShipmentCargoReportControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(CAShipmentCargoReportController);

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.CAShipmentCargoReport;
	}
}
