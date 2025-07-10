using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(JobDocAddressProvider))]
	sealed class JobDocAddressProviderTest : TestCaseWithFactory
	{
		public void TestCode_JapanImporterExporter()
		{
			CombineAssertions(() =>
			{
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					declaration.JE_MessageType = "IMP";

					var docAddress = declaration.ImporterDocumentaryAddress;
					AssertNullOrEmpty(new JobDocAddressProvider(docAddress).Code);

					docAddress.E2_OA_Address = Factory.New<OrgAddress>().PK;
					docAddress.Address.OA_OH = Factory.New<OrgHeader>().PK;

					var jas = docAddress.Address.CustomsCodes.AddNew();
					jas.OK_CodeType = JapanCodeTypes.JAS;
					jas.OK_CustomsRegNo = "JAS12345";
					AssertEquals("JAS123450000", new JobDocAddressProvider(docAddress).Code);

					var cie = docAddress.Address.CustomsCodes.AddNew();
					cie.OK_CodeType = JapanCodeTypes.CIE;
					cie.OK_CustomsRegNo = "CIE123450001";
					AssertEquals("CIE123450001", new JobDocAddressProvider(docAddress).Code);

					var lpc = docAddress.Address.CustomsCodes.AddNew();
					lpc.OK_CodeType = JapanCodeTypes.LPC;
					lpc.OK_CustomsRegNo = "LPC1234567890";
					AssertEquals("LPC12345678900000", new JobDocAddressProvider(docAddress).Code);

					docAddress.E2_AddressOverride = true;
					docAddress.E2_GovRegNum = "12345666";
					AssertEquals("123456660000", new JobDocAddressProvider(docAddress).Code);
				}

				{
					var declaration = Factory.New<BaseJobDeclaration>();
					declaration.JE_MessageType = "EXP";

					var docAddress = declaration.SupplierDocumentaryAddress;
					AssertNullOrEmpty(new JobDocAddressProvider(docAddress).Code);

					docAddress.E2_OA_Address = Factory.New<OrgAddress>().PK;
					docAddress.Address.OA_OH = Factory.New<OrgHeader>().PK;

					var jas = docAddress.Address.CustomsCodes.AddNew();
					jas.OK_CodeType = JapanCodeTypes.JAS;
					jas.OK_CustomsRegNo = "JAS12345";
					AssertEquals("JAS123450000", new JobDocAddressProvider(docAddress).Code);

					var cie = docAddress.Address.CustomsCodes.AddNew();
					cie.OK_CodeType = JapanCodeTypes.CIE;
					cie.OK_CustomsRegNo = "CIE12345";
					AssertEquals("CIE123450000", new JobDocAddressProvider(docAddress).Code);

					var lpc = docAddress.Address.CustomsCodes.AddNew();
					lpc.OK_CodeType = JapanCodeTypes.LPC;
					lpc.OK_CustomsRegNo = "LPC12345678900009";
					AssertEquals("LPC12345678900009", new JobDocAddressProvider(docAddress).Code);
				}
			});
		}

		public void TestCode_ForeignSupplierBuyer()
		{
			CombineAssertions(() =>
			{
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					declaration.JE_MessageType = "IMP";

					var docAddress = declaration.SupplierDocumentaryAddress;
					AssertNullOrEmpty(new JobDocAddressProvider(docAddress).Code);

					docAddress.E2_OA_Address = Factory.New<OrgAddress>().PK;
					docAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
					docAddress.Address.OA_OH = Factory.New<OrgHeader>().PK;

					var fsb = docAddress.Organisation.CustomsCodes.AddNew();
					fsb.OK_CodeType = JapanCodeTypes.FSB;
					fsb.OK_CustomsRegNo = "FSB123";
					AssertEquals("FSB123", new JobDocAddressProvider(docAddress).Code);
				}

				{
					var declaration = Factory.New<BaseJobDeclaration>();
					declaration.JE_MessageType = "EXP";

					var docAddress = declaration.ImporterDocumentaryAddress;
					AssertNullOrEmpty(new JobDocAddressProvider(docAddress).Code);

					docAddress.E2_OA_Address = Factory.New<OrgAddress>().PK;
					docAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
					docAddress.Address.OA_OH = Factory.New<OrgHeader>().PK;

					var fsb = docAddress.Organisation.CustomsCodes.AddNew();
					fsb.OK_CodeType = JapanCodeTypes.FSB;
					fsb.OK_CustomsRegNo = "FSB123";
					AssertEquals("FSB123", new JobDocAddressProvider(docAddress).Code);
				}
			});
		}

		public void TestCode_CustomsControlledPremises()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = "EXP";

				AssertNullOrEmpty(new JobDocAddressProvider(declaration.DepotDocAddress).Code);
				AssertNullOrEmpty(new JobDocAddressProvider(declaration.ContainerYardDocAddress).Code);

				declaration.DepotDocAddress.E2_OA_Address = Factory.New<OrgAddress>().PK;
				declaration.DepotDocAddress.Address.OA_OH = Factory.New<OrgHeader>().PK;
				declaration.ContainerYardDocAddress.E2_OA_Address = Factory.New<OrgAddress>().PK;
				declaration.ContainerYardDocAddress.Address.OA_OH = Factory.New<OrgHeader>().PK;

				var ccp = declaration.DepotDocAddress.Organisation.CustomsCodes.AddNew();
				ccp.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
				ccp.OK_CustomsRegNo = "CCP123";
				AssertEquals("CCP123", new JobDocAddressProvider(declaration.DepotDocAddress).Code);

				var ccp2 = declaration.ContainerYardDocAddress.Organisation.CustomsCodes.AddNew();
				ccp2.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
				ccp2.OK_CustomsRegNo = "CCP456";
				AssertEquals("CCP456", new JobDocAddressProvider(declaration.ContainerYardDocAddress).Code);
			});
		}

		public void TestNACCSUserCode()
		{
			CombineAssertions(() =>
			{
				var address = Factory.New<JobDocAddress>();
				address.E2_OA_Address = Factory.New<OrgAddress>().PK;
				address.Address.OA_OH = Factory.New<OrgHeader>().PK;
				var anyCode = address.Organisation.CustomsCodes.AddNew();
				anyCode.OK_CodeType = OrgCusCode.JapanCodeTypes.FSB;
				anyCode.OK_CustomsRegNo = "FSB123";
				anyCode.OK_OA_PremisesAddress = address.PK;
				AssertNullOrEmpty("No NUC configured", new JobDocAddressProvider(address).NACCSUserCode);

				var nuc = address.Organisation.CustomsCodes.AddNew();
				nuc.OK_CodeType = OrgCusCode.JapanCodeTypes.NUC;
				nuc.OK_CustomsRegNo = "NUP123";
				AssertEquals("Default to the only NUC on org", "NUP123", new JobDocAddressProvider(address).NACCSUserCode);

				var nuc2 = address.Organisation.CustomsCodes.AddNew();
				nuc2.OK_CodeType = OrgCusCode.JapanCodeTypes.NUC;
				nuc2.OK_CustomsRegNo = "NUP456";
				nuc2.OK_OA_PremisesAddress = address.PK;
				AssertEquals("Find NUC at correct address", "NUP456", new JobDocAddressProvider(address).NACCSUserCode);

				var address2 = Factory.New<JobDocAddress>();
				address2.E2_OA_Address = address.Organisation.Addresses.AddNew().PK;
				AssertStartsWith("Any NUC at wrong address", "NUP", new JobDocAddressProvider(address2).NACCSUserCode);
			});
		}

		public void TestJapanAddress()
		{
			CombineAssertions(() =>
			{
				var jobDocAddress = Factory.New<JobDocAddress>();
				jobDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
				IJapaneseAddress provider = new JobDocAddressProvider(jobDocAddress);
				jobDocAddress.E2_CompanyName = "Com";
				AssertEquals(nameof(IJapaneseAddress.Name), "Com", provider.Name);
				jobDocAddress.E2_Postcode = "PC";
				AssertEquals(nameof(IJapaneseAddress.PostCode), "PC", provider.PostCode);
				jobDocAddress.E2_Address1 = "A1";
				jobDocAddress.E2_Address2 = "A2";
				AssertEquals(nameof(IJapaneseAddress.Street), "A1 A2", provider.Street);
				jobDocAddress.E2_City = "Tokyo";
				AssertEquals(nameof(IJapaneseAddress.City), "Tokyo", provider.City);
				jobDocAddress.State = "01";
				AssertEquals(nameof(IJapaneseAddress.Prefecture), "Hokkaido", provider.Prefecture);
				jobDocAddress.E2_Phone = "1234";
				AssertEquals(nameof(IJapaneseAddress.Phone), "1234", provider.Phone);
				jobDocAddress.E2_OA_Address = Factory.New<OrgAddress>().PK;
				jobDocAddress.Address.AdditionalInfos.AddNew().OAI_AdditionalInfo = "Build";
				jobDocAddress.Address.AdditionalInfos.AddNew().OAI_AdditionalInfo = "XXX";
				AssertEquals(nameof(IJapaneseAddress.AdditionalInformation) + "from Note", "Build XXX", provider.AdditionalInformation);
				jobDocAddress.E2_AddressOverride = true;
				jobDocAddress.UnrestrictedAdditionalAddressInformation = "Build YYY";
				AssertEquals(nameof(IJapaneseAddress.AdditionalInformation) + "from Additional Information", "Build YYY", provider.AdditionalInformation);
			});
		}

		public void TestJapanAddress_TrimPhone()
		{
			CombineAssertions(() =>
			{
				var jobDocAddress = Factory.New<JobDocAddress>();
				jobDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
				IJapaneseAddress provider = new JobDocAddressProvider(jobDocAddress, true);
				jobDocAddress.E2_Phone = "12345678901";
				AssertEquals("Should be '12345678901'", "12345678901", provider.Phone);

				jobDocAddress.E2_Phone = "+81 903 248 7136";
				provider = new JobDocAddressProvider(jobDocAddress, true);
				AssertEquals("Should be '09032487136'", "09032487136", provider.Phone);

				jobDocAddress.E2_Phone = "+1 (273) 549521";
				provider = new JobDocAddressProvider(jobDocAddress, true);
				AssertEquals("Should be '+1273549521'", "+1273549521", provider.Phone);

				provider = new JobDocAddressProvider(jobDocAddress);
				AssertEquals("Should be '+1 (273) 549521'", "+1 (273) 549521", provider.Phone);

				jobDocAddress.E2_Phone = "+81 903 248 71";
				provider = new JobDocAddressProvider(jobDocAddress, true);
				AssertEquals("Should be '8190324871'", "+8190324871", provider.Phone);
			});
		}

		public void TestWesternAddress()
		{
			CombineAssertions(() =>
			{
				var jobDocAddress = Factory.New<JobDocAddress>();
				jobDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.China;
				IWesternAddress provider = new JobDocAddressProvider(jobDocAddress);
				jobDocAddress.E2_CompanyName = "Com";
				AssertEquals(nameof(IWesternAddress.Name), "Com", provider.Name);
				jobDocAddress.E2_Postcode = "PC";
				AssertEquals(nameof(IWesternAddress.PostCode), "PC", provider.PostCode);
				jobDocAddress.E2_Address1 = "A1";
				AssertEquals(nameof(IWesternAddress.Street1), "A1", provider.Street1);
				jobDocAddress.E2_Address2 = "A2";
				AssertEquals(nameof(IWesternAddress.Street2), "A2", provider.Street2);
				jobDocAddress.E2_City = "Tokyo";
				AssertEquals(nameof(IWesternAddress.City), "Tokyo", provider.City);
				jobDocAddress.State = "11";
				AssertEquals(nameof(IWesternAddress.State), "Beijing", provider.State);
			});

			CombineAssertions("English Address", () =>
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				header.OH_Code = "org1";
				var jpAddress = header.Addresses.AddNew();
				jpAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
				jpAddress.OA_Language = Core.Constants.Languages.Japanese;
				jpAddress.CompanyName = "ゐ";
				jpAddress.OA_Address1 = "ゐゐゐ1";
				jpAddress.OA_Address2 = "ゐゐゐ2";

				var enTranslatedAddress = jpAddress.TranslatedAddresses.AddNew();
				enTranslatedAddress.OTA_Language = Core.Constants.Languages.English;
				enTranslatedAddress.OTA_CompanyName = "Com";
				enTranslatedAddress.OTA_Address1 = "A1";
				enTranslatedAddress.OTA_Address2 = "A2";

				var jobDocAddress = Factory.New<JobDocAddress>();
				jobDocAddress.E2_OA_Address = jpAddress.PK;
				IWesternAddress provider = new JobDocAddressProvider(jobDocAddress);
				AssertEquals(nameof(IWesternAddress.Name), "Com", provider.Name);
				jobDocAddress.E2_Address1 = "A1";
				AssertEquals(nameof(IWesternAddress.Street1), "A1", provider.Street1);
				jobDocAddress.E2_Address2 = "A2";
				AssertEquals(nameof(IWesternAddress.Street2), "A2", provider.Street2);
			});
		}
	}
}
