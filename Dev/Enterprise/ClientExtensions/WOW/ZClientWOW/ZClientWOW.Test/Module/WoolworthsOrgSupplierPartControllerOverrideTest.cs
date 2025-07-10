using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow
{
	[TestedType(typeof(WoolworthsOrgSupplierPartControllerOverride))]
	public class WoolworthsOrgSupplierPartControllerOverrideTest : ZControllerBasherTest
	{
		protected override Enterprise.ZArchitecture.Modules.ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.SupplierPart;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			AUOrgSupplierPart part = Factory.New<AUOrgSupplierPart>();
			part.OP_PartNum = part.PK.ToString().Replace("-", "");
			Factory.Save();
			return part;
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var part = base.GetBusinessObjectWithoutValidationErrors() as AUOrgSupplierPart;
			part.OP_Desc = "Desc";
			part.OP_PartNum = part.PK.ToString().Replace("-", "");
			var supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "Supplier";
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation.OU_OH = supplier.PK;
			return part;
		}
	}
}
