using System;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(B2AdjustmentsController))]
	sealed class B2AdjustmentsControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(B2AdjustmentsController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.B2Adjustments;

		protected override Type GetBusinessObjectType() => typeof(JobDeclaration);

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}
