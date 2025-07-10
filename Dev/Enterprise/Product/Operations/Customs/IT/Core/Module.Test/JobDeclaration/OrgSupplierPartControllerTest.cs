using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Module.Testing;

[TestedType(typeof(OrgSupplierPartController))]
sealed class OrgSupplierPartControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
{
	protected override string CountryCode
	{
		get { return Enterprise.Core.Constants.CountryCodes.UnitedKingdom; }
	}

	protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
	{
		var part = Factory.New<Business.OrgSupplierPart>();
		part.OP_PartNum = part.PK.ToString().Replace("-", "");
		Factory.Save();
		return part;
	}

	protected override Type GetBusinessObjectType()
	{
		return typeof(Business.OrgSupplierPart);
	}

	public override Type ControllerToBashType
	{
		get { return typeof(OrgSupplierPartController); }
	}

	protected override ControllerID GetControllerID()
	{
		return ControllerIDs.Customs.SupplierPart;
	}

	protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
	{
		var part = base.GetBusinessObjectWithoutValidationErrors() as Business.OrgSupplierPart;
		part.OP_Desc = "Desc";
		var supplier = OrgHeader.New(Factory);
		supplier.OH_Code = "Supplier";
		var relation = part.RelatedOrganisations.AddNew();
		relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
		relation.OU_OH = supplier.PK;
		return part;
	}
}
