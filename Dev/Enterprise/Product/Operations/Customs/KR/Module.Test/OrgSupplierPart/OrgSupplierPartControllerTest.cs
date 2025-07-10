using System;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartController))]
	sealed class OrgSupplierPartControllerTest : Customs.Module.Testing.OrgSupplierPartControllerTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(OrgSupplierPartController); }
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.KoreaSouth; }
		}
	}
}
