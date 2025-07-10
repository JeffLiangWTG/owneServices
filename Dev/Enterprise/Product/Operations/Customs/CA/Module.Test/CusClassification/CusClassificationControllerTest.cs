using System;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CusClassificationController))]
	sealed class CusClassificationControllerTest : Customs.Module.Testing.ImportClassificationControllerTest
	{
		public new void TestModuleID()
		{
			AssertNull("ModuleID", Controller.ModuleID);
		}

		public override Type ControllerToBashType => typeof(CusClassificationController);

		protected override Type GetBusinessObjectType() => typeof(CusClassification);

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.CACusClassification;
	}
}
