using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.MasterFiles;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Module.Testing;

[TestedType(typeof(OrgSupplierPartController))]
public class OrgSupplierPartControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
{
	protected override string CountryCode
	{
		get { return Core.Constants.CountryCodes.Belgium; }
	}

	protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
	{
		var part = OrgSupplierPart.New(Factory);
		part.OP_PartNum = part.PK.ToString().Replace("-", "");
		Factory.Save();
		return part;
	}

	protected override Type GetBusinessObjectType()
	{
		return typeof(OrgSupplierPart);
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
		var part = base.GetBusinessObjectWithoutValidationErrors() as OrgSupplierPart;
		part.OP_Desc = "Desc";
		var supplier = MasterFiles.Business.OrgHeader.New(Factory);
		supplier.OH_Code = "Supplier";
		var relation = part.RelatedOrganisations.AddNew();
		relation.OU_Relationship = MasterFiles.Business.OrgPartRelation.RelationshipTypes.Supplier;
		relation.OU_OH = supplier.PK;
		return part;
	}
}
