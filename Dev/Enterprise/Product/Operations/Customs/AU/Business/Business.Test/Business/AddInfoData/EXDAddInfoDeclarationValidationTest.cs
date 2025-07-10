using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDAddInfoDeclarationValidationTest : TestCaseWithFactory
	{
		public void TestOwnerPartyIDFiresSupplierValidation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_OH_Supplier = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			AssertHasMessageErrors(declaration.JE_OH_SupplierInfo);
			declaration.ZA_GoodsOwnerPartyIDHidden = "12345";
			AssertNoMessageErrors(declaration.JE_OH_SupplierInfo);
		}

		public void TestConsigneeNameFiresImporterValidation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_OH_Importer = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			AssertHasMessageErrors(declaration.JE_OH_ImporterInfo);
			declaration.ZA_ConsigneeCityHidden = "Los Angeles";
			AssertHasMessageErrors(declaration.JE_OH_ImporterInfo);
			declaration.ZA_ConsigneeNameHidden = "Brett Shearer";
			AssertNoMessageErrors(declaration.JE_OH_ImporterInfo);
		}

		public void TestConsigneeCityFiresImporterValidation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_OH_Importer = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			AssertHasMessageErrors(declaration.JE_OH_ImporterInfo);
			declaration.ZA_ConsigneeNameHidden = "Brett Shearer";
			AssertHasMessageErrors(declaration.JE_OH_ImporterInfo);
			declaration.ZA_ConsigneeCityHidden = "Los Angeles";
			AssertNoMessageErrors(declaration.JE_OH_ImporterInfo);
		}

		public void TestCheckZA_ImporterToOrder_Hidden()
		{
			const string messageError = "City is required when 'To Order' box is checked.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			Assert("Pre-Condition", declaration.IsQuarantine);

			declaration.JE_ToOrder = false;
			AssertNoMessageErrors(declaration.JE_ToOrderCommentInfo);

			declaration.JE_ToOrder = true;
			AssertNoMessageError(declaration.JE_ToOrderCommentInfo, messageError);

			declaration.JE_ToOrderComment = "Consignee details to be announced";
			AssertNoMessageError(declaration.JE_ToOrderCommentInfo, messageError);

			declaration.JE_ToOrderComment = ZString.Empty;
			AssertHasMessageError(declaration.JE_ToOrderCommentInfo, messageError);
		}
	}
}
