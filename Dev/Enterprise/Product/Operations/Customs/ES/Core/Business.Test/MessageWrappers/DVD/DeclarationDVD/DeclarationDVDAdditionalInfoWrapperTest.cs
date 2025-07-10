using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationDVDAdditionalInfoWrapperTest : WrapperHelperTest<DeclarationDVDAdditionalInfoWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if doc is null", typeof(NullReferenceException),
			"Object reference not set to an instance of an object.", () => new DeclarationDVDAdditionalInfoWrapper(null));
		}

		public void TestEUCode()
		{
			CombineAssertions(() =>
			{
				document.CSI_Code = "A123";
				document.CSI_NctsExportFromEC = true;
				wrapper = GetWrapper(document);
				AssertEquals("Expected filled EUCode when CSI_NctsExportFromEC is true", "A123", wrapper.EUCode);

				document.CSI_NctsExportFromEC = false;
				wrapper = GetWrapper(document);
				AssertEquals("Expected empty EUCode when CSI_NctsExportFromEC is false", ZString.Empty, wrapper.EUCode);
			});
		}

		public void TestNationalCode()
		{
			CombineAssertions(() =>
			{
				document.CSI_Code = "A123";
				document.CSI_NctsExportFromEC = false;
				wrapper = GetWrapper(document);
				AssertEquals("Expected filled NationalCode when CSI_NctsExportFromEC is false", "A123", wrapper.NationalCode);

				document.CSI_NctsExportFromEC = true;
				wrapper = GetWrapper(document);
				AssertEquals("Expected empty NationalCode when CSI_NctsExportFromEC is true", ZString.Empty, wrapper.NationalCode);
			});
		}

		public void TestDescription()
		{
			CombineAssertions(() =>
			{
				document.CSI_Description = "description";
				document.CSI_Code = ZString.Empty;
				wrapper = GetWrapper(document);
				AssertEquals("Expected filled Description when code is empty", "description", wrapper.Description);

				document.CSI_Code = "1ABC";
				wrapper = GetWrapper(document);
				AssertEquals("Expected empty Description when code is not empty", ZString.Empty, wrapper.Description);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			document = Factory.New<AdditionalInfo>();
			wrapper = GetWrapper(document);
		}

		AdditionalInfo document;
		DeclarationDVDAdditionalInfoWrapper wrapper;

		DeclarationDVDAdditionalInfoWrapper GetWrapper(AdditionalInfo doc) => new DeclarationDVDAdditionalInfoWrapper(doc);

		protected override DeclarationDVDAdditionalInfoWrapper GetProvider() => wrapper;
	}
}
