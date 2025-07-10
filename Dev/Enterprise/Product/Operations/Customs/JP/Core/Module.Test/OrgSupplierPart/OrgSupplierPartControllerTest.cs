using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.GUI;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartController))]
	public class OrgSupplierPartControllerTest : Customs.Module.Testing.OrgSupplierPartControllerTest
	{
		public override Type ControllerToBashType => typeof(OrgSupplierPartController);

		protected override string CountryCode => Core.Constants.CountryCodes.Japan;

		public void TestGetPlugIn()
		{
			var part = Factory.New<OrgSupplierPart>();
			var controller = new OrgSupplierPartControllerForTest();
			using (var partPlugIn = controller.GetPlugIn(part))
			{
				AssertType<OrgSupplierPartFormCustomsPlugin>(partPlugIn);
			}
		}
	}

	class OrgSupplierPartControllerForTest : OrgSupplierPartController
	{
		public new ZPlugIn GetPlugIn(IBusiness businessEntity) => base.GetPlugIn(businessEntity);
	}
}
