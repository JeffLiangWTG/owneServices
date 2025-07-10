using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	public class DeclarationImporterWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationImporter>
	{
		public void TestNewOrNull()
		{
			AssertNull(DeclarationImporterWrapper.NewOrNull(null));
			AssertNotNull(Provider);
		}

		public void TestDmExtensions()
		{
			AssertType<DeclarationImporterDmExtensionsWrapper>(Provider.DmExtensions);
		}

		public void TestID()
		{
			AssertEquals("IL0001", Provider.ID.Value);
			AssertEquals("1", Provider.ID.SchemeID);
		}

		protected override IDeclarationImporter GetProvider()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "IL0001", Core.Constants.CountryCodes.Israel);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			return DeclarationImporterWrapper.NewOrNull(declaration);
		}
	}
}
