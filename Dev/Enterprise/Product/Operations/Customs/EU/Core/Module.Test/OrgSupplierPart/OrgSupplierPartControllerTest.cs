using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartController))]
	class OrgSupplierPartControllerTest : Customs.Module.Testing.OrgSupplierPartControllerTest
	{
		public override Type ControllerToBashType => typeof(OrgSupplierPartController);

		protected override string CountryCode => Core.Constants.CountryCodes.Latvia;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var part = OrgSupplierPart.New(Factory);
			part.OP_PartNum = part.PK.ToString().Replace("-", "");
			Factory.Save();
			return part;
		}

		protected override Type GetBusinessObjectType() => typeof(OrgSupplierPart);

		protected override MasterFiles.Module.OrgSupplierPartController GetOrgSupplierPartController() => new OrgSupplierPartController();
	}
}
