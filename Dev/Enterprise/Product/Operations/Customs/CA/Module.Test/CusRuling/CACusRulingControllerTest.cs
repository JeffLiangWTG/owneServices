using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CACusRulingController))]
	sealed class CACusRulingControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(CACusRulingController);

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var ruling = Factory.New<CACusRuling>();
			ruling.ZZX_RulingNumber = "123";
			ruling.ZZX_Description = "TTG TEST";
			ruling.ZZX_RulingType = Universal.RefCusRulingTypeList.Codes._2;
			Factory.Save();
			return ruling;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.Universal.ZZRefCusRuling;

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}
