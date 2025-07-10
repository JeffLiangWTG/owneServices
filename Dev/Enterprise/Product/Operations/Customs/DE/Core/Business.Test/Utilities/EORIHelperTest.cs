using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	public sealed class EORIHelperTest : TestCaseWithFactory
	{
		public void TestValidEoriLength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Too Short", false, EORIHelper.ValidEoriLength("D"));
				AssertEquals("Country Code", true, EORIHelper.ValidEoriLength("DE"));
				AssertEquals("Country Code and numeric", true, EORIHelper.ValidEoriLength("DE1"));
				AssertEquals("Maximum Length", true, EORIHelper.ValidEoriLength("DE123456789012345"));
				AssertEquals("Too Large", false, EORIHelper.ValidEoriLength("DE1234567890123456"));
			});
		}

		public void TestGetOrgHeaderFromEoriCode()
		{
			CombineAssertions(() =>
			{
				AssertNull("Invalid Eori Code", EORIHelper.GetOrgHeaderFromEoriCode(Factory, "INVALID"));
				var cusCode = Factory.New<OrgCusCode>();
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Greece;
				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				cusCode.OK_CustomsRegNo = "1234";
				AssertNull("No Organsation", EORIHelper.GetOrgHeaderFromEoriCode(Factory, "GR1234"));
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.CustomsCodes.Add(cusCode);
				AssertSame("Organisation Found", orgHeader, EORIHelper.GetOrgHeaderFromEoriCode(Factory, "GR1234"));
			});
		}

		public void TestIsCW1Organization()
		{
			const string eoriNumber = "BE123456789";
			const string eoriBranch = "0001";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var eoriNumberOrgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			eoriNumberOrgCusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Core.Constants.CountryCodes.Belgium, "123456789");
			var eoriBranchOrgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			eoriBranchOrgCusCode.ModifyOrgCusCode(orgHeader.PK, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Core.Constants.CountryCodes.Germany, eoriBranch, orgHeader.MainAddress.PK);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Is a CW1 organization", true, EORIHelper.IsCW1Organization(Factory, eoriNumber, eoriBranch));
				AssertEquals("Invalid Branch", false, EORIHelper.IsCW1Organization(Factory, eoriNumber, "0002"));
				AssertEquals("Invalid Eori Code", false, EORIHelper.IsCW1Organization(Factory, "DE987654321", eoriBranch));
			});
		}

		public void TestIsCW1MessagingOrganization()
		{
			const string eoriNumber = "BE123456789";
			const string eoriBranch = "0001";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var eoriNumberOrgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			eoriNumberOrgCusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Core.Constants.CountryCodes.Belgium, "123456789");
			var eoriBranchOrgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			eoriBranchOrgCusCode.ModifyOrgCusCode(orgHeader.PK, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Core.Constants.CountryCodes.Germany, eoriBranch, orgHeader.MainAddress.PK);
			var apiOrgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			apiOrgCusCode.ModifyOrgCusCode(orgHeader.PK, GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, Core.Constants.CountryCodes.Germany, "API", orgHeader.MainAddress.PK);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Is a CW1 Messaging organization", true, EORIHelper.IsCW1MessagingOrganization(Factory, eoriNumber, eoriBranch));
				AssertEquals("Invalid Branch", false, EORIHelper.IsCW1MessagingOrganization(Factory, eoriNumber, "0002"));
				AssertEquals("Invalid Eori Code", false, EORIHelper.IsCW1MessagingOrganization(Factory, "DE987654321", eoriBranch));

				apiOrgCusCode.Delete();
				Factory.Save();
				AssertEquals("No API Existing", false, EORIHelper.IsCW1MessagingOrganization(Factory, eoriNumber, eoriBranch));
			});
		}

		public void TestIsCW1MessagingOrganization_APIHasNoSameAddressAsEBS()
		{
			const string eoriNumber = "BE123456789";
			const string eoriBranch = "0001";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = orgHeader.Addresses.AddNew();
			address1.Address1 = "Address1";
			var eoriNumberOrgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			eoriNumberOrgCusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Core.Constants.CountryCodes.Belgium, "123456789");
			var eoriBranchOrgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			eoriBranchOrgCusCode.ModifyOrgCusCode(orgHeader.PK, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Core.Constants.CountryCodes.Germany, eoriBranch, orgHeader.MainAddress.PK);
			var api1 = Factory.NewWithValidTestData<OrgCusCode>();
			api1.ModifyOrgCusCode(orgHeader.PK, GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, Core.Constants.CountryCodes.Germany, "API", address1.PK);
			Factory.Save();

			AssertEquals(false, EORIHelper.IsCW1MessagingOrganization(Factory, eoriNumber, eoriBranch));
		}

		public void TestIsCW1MessagingOrganization_MultipleAPIs()
		{
			const string eoriNumber = "BE123456789";
			const string eoriBranch = "0001";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = orgHeader.Addresses.AddNew();
			address1.Address1 = "Address1";
			var eoriNumberOrgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			eoriNumberOrgCusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Core.Constants.CountryCodes.Belgium, "123456789");
			var eoriBranchOrgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			eoriBranchOrgCusCode.ModifyOrgCusCode(orgHeader.PK, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Core.Constants.CountryCodes.Germany, eoriBranch, orgHeader.MainAddress.PK);
			var api1 = Factory.NewWithValidTestData<OrgCusCode>();
			api1.ModifyOrgCusCode(orgHeader.PK, GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, Core.Constants.CountryCodes.Germany, "API", address1.PK);
			var api2 = Factory.NewWithValidTestData<OrgCusCode>();
			api2.ModifyOrgCusCode(orgHeader.PK, GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, Core.Constants.CountryCodes.Germany, "API", orgHeader.MainAddress.PK);
			Factory.Save();

			AssertEquals(true, EORIHelper.IsCW1MessagingOrganization(Factory, eoriNumber, eoriBranch));
		}

		public void TestSplitEoriDetails_Empty()
		{
			AssertEoriDetails(ZString.Empty, ZString.Empty, ZString.Empty);
		}

		public void TestSplitEoriDetails_Short()
		{
			AssertEoriDetails("J873", "J8", "73");
		}

		public void TestSplitEoriDetails_Valid()
		{
			AssertEoriDetails("NK987654321", "NK", "987654321");
		}

		public void TestSplitEoriDetails_Long()
		{
			AssertEoriDetails("NK9876543210123456", "NK", "987654321012345");
		}

		void AssertEoriDetails(ZString eoriDetails, ZString expectedCountryPrefix, ZString expectedEoriNumber)
		{
			var (countryPrefix, eoriNumber) = eoriDetails.SplitEoriDetails();
			CombineAssertions(() =>
			{
				AssertEquals("Country Prefix", expectedCountryPrefix, countryPrefix);
				AssertEquals("Eori Number", expectedEoriNumber, eoriNumber);
			});
		}

		public void TestGetImportMessageSenderAndBinDetails_DIR_API()
		{
			TestHelper.CreateCL010CoutryList(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var representative = GetOrgWithEORNumberAndEORIBranch(Factory, "EOR1", "EBS1", Core.Constants.CountryCodes.Latvia);
			representative.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);
			declaration.JE_OA_Representative = representative.PK;

			(IPartyID sender, ZString bin) = EORIHelper.GetImportMessageSenderAndBinDetails(declaration);
			CombineAssertions(() =>
			{
				AssertEquals("sender.EoriNumber", "LVEOR1", sender.EoriNumber);
				AssertEquals("sender.EoriBranchSuffix", "EBS1", sender.EoriBranchSuffix);
				AssertEquals("Bin", "9877700310012345123456000", bin);
			});
		}

		public void TestGetImportMessageSenderAndBinDetails_DIR_NoAPI()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var representative = GetOrgWithEORNumberAndEORIBranch(Factory, "EOR1", "EBS1");
			declaration.JE_OA_Representative = representative.PK;

			AssertUsingRegistry(declaration);
		}

		public void TestGetImportMessageSenderAndBinDetails_SEL_API()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			var declarant = GetOrgWithEORNumberAndEORIBranch(Factory, "EOR1", "EBS1");
			declarant.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);
			declaration.JE_OA_DeclarantAddress = declarant.PK;

			(IPartyID sender, ZString bin) = EORIHelper.GetImportMessageSenderAndBinDetails(declaration);
			CombineAssertions(() =>
			{
				AssertEquals("sender.EoriNumber", "DEEOR1", sender.EoriNumber);
				AssertEquals("sender.EoriBranchSuffix", "EBS1", sender.EoriBranchSuffix);
				AssertEquals("Bin", "9877700310012345123456000", bin);
			});
		}

		public void TestGetImportMessageSenderAndBinDetails_SEL_NoAPI()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			var declarant = GetOrgWithEORNumberAndEORIBranch(Factory, "EOR1", "EBS1");
			declaration.JE_OA_DeclarantAddress = declarant.PK;

			AssertUsingRegistry(declaration);
		}

		public void TestInterchangeSender_IND_NoAPI_Fallback()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;

			AssertUsingRegistryWithFallback(declaration);
		}

		#region MonthlyClosing
		public void TestGetMonthlyClosingMessageSenderAndBinDetails_DIR_API()
		{
			TestHelper.CreateCL010CoutryList(Factory);

			var declaration = Factory.New<CusReconDeclaration>();
			declaration.CRD_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var representative = GetOrgWithEORNumberAndEORIBranch(Factory, "EOR1", "EBS1", Core.Constants.CountryCodes.Latvia);
			representative.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);
			declaration.CRD_OA_RepresentativeAddress = representative.PK;

			(IPartyID sender, ZString bin) = EORIHelper.GetImportMessageSenderAndBinDetails(declaration);
			CombineAssertions(() =>
			{
				AssertEquals("sender.EoriNumber", "LVEOR1", sender.EoriNumber);
				AssertEquals("sender.EoriBranchSuffix", "EBS1", sender.EoriBranchSuffix);
				AssertEquals("Bin", "9877700310012345123456000", bin);
			});
		}

		public void TestGetMonthlyClosingMessageSenderAndBinDetails_DIR_NoAPI()
		{
			var declaration = Factory.New<CusReconDeclaration>();
			declaration.CRD_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var representative = GetOrgWithEORNumberAndEORIBranch(Factory, "EOR1", "EBS1");
			declaration.CRD_OA_RepresentativeAddress = representative.PK;

			AssertUsingRegistry(declaration);
		}

		public void TestGetMonthlyClosingMessageSenderAndBinDetails_SEL_API()
		{
			TestHelper.CreateCL010CoutryList(Factory);

			var declaration = Factory.New<CusReconDeclaration>();
			declaration.CRD_DeclarantType = RepresentationTypeList.Codes._1Self;
			var declarant = GetOrgWithEORNumberAndEORIBranch(Factory, "EOR1", "EBS1");
			declarant.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);
			declaration.CRD_OA_DeclarantAddress = declarant.PK;

			(IPartyID sender, ZString bin) = EORIHelper.GetImportMessageSenderAndBinDetails(declaration);
			CombineAssertions(() =>
			{
				AssertEquals("sender.EoriNumber", "DEEOR1", sender.EoriNumber);
				AssertEquals("sender.EoriBranchSuffix", "EBS1", sender.EoriBranchSuffix);
				AssertEquals("Bin", "9877700310012345123456000", bin);
			});
		}

		public void TestGetMonthlyClosingMessageSenderAndBinDetails_SEL_NoAPI()
		{
			var declaration = Factory.New<CusReconDeclaration>();
			declaration.CRD_DeclarantType = RepresentationTypeList.Codes._1Self;
			var declarant = GetOrgWithEORNumberAndEORIBranch(Factory, "EOR1", "EBS1");
			declaration.CRD_OA_DeclarantAddress = declarant.PK;

			AssertUsingRegistry(declaration);
		}

		public void TestMonthlyClosingInterchangeSender_IND_NoAPI_Fallback()
		{
			var declaration = Factory.New<CusReconDeclaration>();
			declaration.CRD_DeclarantType = RepresentationTypeList.Codes._3Indirect;

			AssertUsingRegistryWithFallback(declaration);
		}
		#endregion

		public void TestGetMissingEoriMessage()
		{
			CombineAssertions(() =>
			{
				AssertEquals("eoriCodeMissing: True, eoriBranchMissing: True", "EORI number and branch.", EORIHelper.GetMissingEoriMessage(true, true));
				AssertEquals("eoriCodeMissing: True, eoriBranchMissing: False", "EORI number.", EORIHelper.GetMissingEoriMessage(true, false));
				AssertEquals("eoriCodeMissing: False, eoriBranchMissing: True", "EORI branch.", EORIHelper.GetMissingEoriMessage(false, true));
			});
		}

		public void TestGetSenderDetailsFromRegistry()
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DE0123456REG"))
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6777"))
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6800000310012345123456000"))
			{
				(IPartyID sender, ZString bin) = EORIHelper.GetSenderDetailsFromRegistry();
				CombineAssertions(() =>
				{
					AssertEquals("sender.EoriNumber", "DE0123456REG", sender.EoriNumber);
					AssertEquals("sender.EoriBranchSuffix", "6777", sender.EoriBranchSuffix);
					AssertEquals("Bin", "6800000310012345123456000", bin);
				});
			}
		}

		public static OrgAddress GetOrgWithEORNumberAndEORIBranch(BusinessObjectFactory factory, ZString eoriNumber, ZString ebsNumber, string eoriNumberCountry = Core.Constants.CountryCodes.Germany)
		{
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriNumber, eoriNumberCountry);
			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "TEST";
			address.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			var ebsCode = orgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, ebsNumber, Core.Constants.CountryCodes.Germany);
			ebsCode.OK_OA_PremisesAddress = address.PK;
			return address;
		}

		public static OrgAddress GetOrgWithEORNumberAndEORIBranchAndAPINumber(BusinessObjectFactory factory, ZString eoriNumber, ZString ebsNumber, ZString apiNumber, string eoriNumberCountry = Core.Constants.CountryCodes.Germany)
		{
			var address = GetOrgWithEORNumberAndEORIBranch(factory, eoriNumber, ebsNumber);
			var apiCode = address.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, apiNumber, Core.Constants.CountryCodes.Germany);
			apiCode.OK_OA_PremisesAddress = address.PK;
			return address;
		}

		void AssertUsingRegistry(JobDeclaration declaration)
		{
			var currentBranchPK = GlbBranch.CurrentBranch.PK.ToGuid();

			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "DE0123456REG"))
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(Guid.Empty, currentBranchPK, Guid.Empty, "7777"))
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(Guid.Empty, currentBranchPK, Guid.Empty, "9800000310012345123456000"))
			{
				(IPartyID sender, ZString bin) = EORIHelper.GetImportMessageSenderAndBinDetails(declaration);
				CombineAssertions(() =>
				{
					AssertEquals("sender.EoriNumber", "DE0123456REG", sender.EoriNumber);
					AssertEquals("sender.EoriBranchSuffix", "7777", sender.EoriBranchSuffix);
					AssertEquals("Bin", "9800000310012345123456000", bin);
				});
			}
		}

		void AssertUsingRegistryWithFallback(JobDeclaration declaration)
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();

			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DE0123456REG"))
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6777"))
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6800000310012345123456000"))
			{
				(IPartyID sender, ZString bin) = EORIHelper.GetImportMessageSenderAndBinDetails(declaration);
				CombineAssertions(() =>
				{
					AssertEquals("sender.EoriNumber", "DE0123456REG", sender.EoriNumber);
					AssertEquals("sender.EoriBranchSuffix", "6777", sender.EoriBranchSuffix);
					AssertEquals("Bin", "6800000310012345123456000", bin);
				});
			}
		}

		void AssertUsingRegistry(CusReconDeclaration declaration)
		{
			var currentBranchPK = GlbBranch.CurrentBranch.PK.ToGuid();

			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "DE0123456REG"))
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(Guid.Empty, currentBranchPK, Guid.Empty, "7777"))
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(Guid.Empty, currentBranchPK, Guid.Empty, "9800000310012345123456000"))
			{
				(IPartyID sender, ZString bin) = EORIHelper.GetImportMessageSenderAndBinDetails(declaration);
				CombineAssertions(() =>
				{
					AssertEquals("sender.EoriNumber", "DE0123456REG", sender.EoriNumber);
					AssertEquals("sender.EoriBranchSuffix", "7777", sender.EoriBranchSuffix);
					AssertEquals("Bin", "9800000310012345123456000", bin);
				});
			}
		}

		void AssertUsingRegistryWithFallback(CusReconDeclaration declaration)
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();

			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DE0123456REG"))
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6777"))
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6800000310012345123456000"))
			{
				(IPartyID sender, ZString bin) = EORIHelper.GetImportMessageSenderAndBinDetails(declaration);
				CombineAssertions(() =>
				{
					AssertEquals("sender.EoriNumber", "DE0123456REG", sender.EoriNumber);
					AssertEquals("sender.EoriBranchSuffix", "6777", sender.EoriBranchSuffix);
					AssertEquals("Bin", "6800000310012345123456000", bin);
				});
			}
		}
	}
}
