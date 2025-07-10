using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MY.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartController))]
	class OrgSupplierPartControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(OrgSupplierPartController);

		protected override string CountryCode => Core.Constants.CountryCodes.Malaysia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.SupplierPart;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var factory = new BusinessObjectFactory();
			var product = factory.New<Business.OrgSupplierPart>();
			product.OP_PartNum = product.PK.ToString().Replace("-", "");
			factory.Save();
			return product;
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var part = base.GetBusinessObjectWithoutValidationErrors() as Business.OrgSupplierPart;
			part.OP_Desc = "Desc";
			var supplier = MasterFiles.Business.OrgHeader.New(Factory);
			supplier.OH_Code = "Supplier";
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = MasterFiles.Business.OrgPartRelation.RelationshipTypes.Supplier;
			relation.OU_OH = supplier.PK;
			return part;
		}
	}
}
