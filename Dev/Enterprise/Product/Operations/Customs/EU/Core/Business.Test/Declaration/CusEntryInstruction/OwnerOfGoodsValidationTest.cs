using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class OwnerOfGoodsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOrganisationPK()
		{
			var expectedMessageErrorMessage = "Owner must be unique, no duplicate allowed.";
			var cei = Factory.New<CusEntryInstruction>();
			var owner1 = Factory.New<OrgAddress>();
			var owner2 = Factory.New<OrgAddress>();
			var owner3 = Factory.New<OrgAddress>();
			cei.CEI_OH_Owner = owner1.PK;
			var oog1 = cei.OwnerOfGoodsCollection.AddNew();
			oog1.OrganisationPK = owner2.PK;

			CombineAssertions(() =>
			{
				oog1.Validation.ValidateAll();
				AssertNoMessageError("Single first entry", oog1.OrganisationPKInfo, expectedMessageErrorMessage);
				var oog2 = cei.OwnerOfGoodsCollection.AddNew();
				oog2.OrganisationPK = owner2.PK;
				oog2.Validation.ValidateAll();
				AssertHasMessageError("Duplicate with secondary", oog2.OrganisationPKInfo, expectedMessageErrorMessage);
				oog2.OrganisationPK = owner3.PK;
				oog2.Validation.ValidateAll();
				AssertNoMessageError("Single second entry", oog2.OrganisationPKInfo, expectedMessageErrorMessage);
				oog2.OrganisationPK = owner1.PK;
				oog2.Validation.ValidateAll();
				AssertHasMessageError("Duplicate with primary", oog2.OrganisationPKInfo, expectedMessageErrorMessage);
				cei.CEI_OH_Owner = owner2.PK;
				cei.OwnerOfGoodsCollection.RunPreSaveValidation();
				AssertHasMessageError("Duplicate with primary set from CEI", oog1.OrganisationPKInfo, expectedMessageErrorMessage);
				AssertNoMessageError("Single second entry message error is removed", oog2.OrganisationPKInfo, expectedMessageErrorMessage);
			});
		}

		public void TestCheckEmptyOrganisationPK()
		{
			var emptyErrorMessage = "You have not entered an Organization.";
			var cei = Factory.New<CusEntryInstruction>();
			var oog1 = cei.OwnerOfGoodsCollection.AddNew();
			oog1.OrganisationPK = Guid.Empty;

			oog1.Validation.ValidateAll();
			AssertHasMessageError("oog1 OrganisationPK is empty", oog1.OrganisationPKInfo, emptyErrorMessage);
		}
	}
}
