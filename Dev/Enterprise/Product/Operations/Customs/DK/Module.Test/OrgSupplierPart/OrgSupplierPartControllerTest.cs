using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DK.Business.MasterFiles;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.DK.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartController))]
	sealed class OrgSupplierPartControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(OrgSupplierPartController);

		protected override string CountryCode => Core.Constants.CountryCodes.Denmark;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var part = OrgSupplierPart.New(Factory);
			part.OP_PartNum = part.PK.ToString().Replace("-", "");
			Factory.Save();
			return part;
		}

		protected override Type GetBusinessObjectType() => typeof(OrgSupplierPart);

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
	}
}
