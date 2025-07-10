using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.MasterFiles;
using Enterprise.Customs.IE.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartController))]
	class OrgSupplierPartControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Ireland;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var part = OrgSupplierPart.New(Factory);
			part.OP_PartNum = part.PK.ToString().Replace("-", "");
			Factory.Save();
			return part;
		}

		protected override Type GetBusinessObjectType() => typeof(OrgSupplierPart);

		public override Type ControllerToBashType => typeof(OrgSupplierPartController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.SupplierPart;

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var part = (OrgSupplierPart)base.GetBusinessObjectWithoutValidationErrors();
			part.OP_Desc = "DESCRIPTION";
			var supplier = MasterFiles.Business.OrgHeader.New(Factory);
			supplier.OH_Code = "SUPPLIER";
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = MasterFiles.Business.OrgPartRelation.RelationshipTypes.Supplier;
			relation.OU_OH = supplier.PK;
			return part;
		}

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
