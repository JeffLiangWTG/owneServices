using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ImportImporterWrapperTest : WrapperHelperTest<ImportImporterWrapper>
	{
		public void TestGetNewPDIImporterWrapperIfNotNull()
		{
			CombineAssertions(() =>
			{
				AssertNull("JobDocAddress null", ImportImporterWrapper.New(null, declaration));

				var docAddress = Factory.New<JobDocAddress>();
				AssertNull("JobDocAddress with no org address null", ImportImporterWrapper.New(docAddress, declaration));

				var address = Factory.New<OrgAddress>();
				docAddress.E2_OA_Address = address.PK;
				AssertNull("JobDocAddress with org address but no OrgHeader null", ImportImporterWrapper.New(docAddress, declaration));

				var org = Factory.New<OrgHeader>();
				address.OA_OH = org.PK;
				AssertNotNull("JobDocAddress not null", ImportImporterWrapper.New(docAddress, declaration));
			});
		}

		public void TestIsIndividual()
		{
			CombineAssertions(() =>
			{
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("IsIndividual", true, wrapper.IsIndividual);

				orgHeader.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Not IsIndividual", false, wrapper.IsIndividual);
			});
		}

		public void TestCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "MQ", "FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
			Factory.Save();

			CombineAssertions(() =>
			{
				orgAddress.OA_RN_NKCountryCode = "RS";
				AssertEquals("Expected filled Country with Default Territory (XS)", "XS", wrapper.Country);

				orgAddress.OA_RN_NKCountryCode = "ES";
				AssertEquals("Expected filled Country with given code since there is no Default Territory", "ES", wrapper.Country);

				orgAddress.OA_RN_NKCountryCode = "MQ";
				AssertEquals("Expected filled Country with Default Territory (FR)", "FR", wrapper.Country);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			var docAddress = Factory.New<JobDocAddress>();
			orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "Address";
			docAddress.E2_OA_Address = orgAddress.PK;
			docAddress.E2_ParentTableCode = declaration.TablePrefix;
			docAddress.E2_ParentID = declaration.PK;
			orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderData.ImporterCode;
			orgAddress.OA_OH = orgHeader.PK;

			wrapper = ImportImporterWrapper.New(docAddress, declaration);
		}
		JobDeclaration declaration;
		OrgHeader orgHeader;
		OrgAddress orgAddress;
		ImportImporterWrapper wrapper;

		protected override ImportImporterWrapper GetProvider() => wrapper;
	}
}
