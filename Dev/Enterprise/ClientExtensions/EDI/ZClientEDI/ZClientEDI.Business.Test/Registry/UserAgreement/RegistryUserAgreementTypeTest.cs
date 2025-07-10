using CargoWise.EntityFramework;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Testing
{
	[TestedType(typeof(RegistryUserAgreementType))]
	public class RegistryUserAgreementTypeTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestCodeShouldBeUnique()
		{
			var collection = new RegistryUserAgreementTypeCollection();
			var type1 = (RegistryUserAgreementType)collection.AddNew();
			type1.Code = "ACA";
			var type2 = (RegistryUserAgreementType)collection.AddNew();
			type2.Code = "ABA";
			AssertNoErrors(type2.CodeInfo);
			type2.Code = "ACA";
			AssertHasError(type2.CodeInfo, "The Code has been duplicated and must be unique.");
		}

		public void TestShouldNotDeleteExistingAgreement()
		{
			var collection = new RegistryUserAgreementTypeCollection();
			var type1 = (RegistryUserAgreementType)collection.AddNew();
			type1.Code = "ACA";
			AssertEquals(true, type1.CanDelete);

			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Type = "ACA";
			Factory.Save();

			AssertEquals(false, type1.CanDelete);
		}

		public void TestLevel()
		{
			var collection = new RegistryUserAgreementTypeCollection();
			var type1 = (RegistryUserAgreementType)collection.AddNew();
			type1.Code = "ACA";
			type1.RunPreSaveValidation();
			AssertHasError(type1.LevelInfo, "Please enter a Level.");
			type1.Level = "###";
			AssertHasError(type1.LevelInfo, "Enter a valid Level.");
			type1.Level = EdiUserAgreementLevelList.Codes.User;
			AssertNoErrors(type1.LevelInfo);
			AssertEquals(false, type1.Level_ReadOnly);

			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Type = type1.Code;
			Factory.Save();
			AssertEquals(true, type1.Level_ReadOnly);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new RegistryUserAgreementType();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new RegistryUserAgreementType();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RegistryUserAgreementType();
		}

		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;
	}
}
