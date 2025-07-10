using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.GB.Business.Testing
{
	public class GBExtensionsTests : TestCaseWithFactory
	{
		public void TestIsPhase2Active()
		{
			AssertEquals("Not Active", false, GBExtensions.IsPhase2Active);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CDS_ILE_PHASE2, CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				Assert(GBExtensions.IsPhase2Active);
			}
		}

		public void TestIsPhase1Active()
		{
			AssertEquals("Not Active", false, GBExtensions.IsPhase1Active);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CDS_ILE_PHASE1, CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				Assert(GBExtensions.IsPhase1Active);
			}
		}

		public void TestUpdateStatusIfNotEmpty()
		{
			var entry = Factory.New<CusEntryHeader>();
			entry.UpdateStatusIfNotEmpty("XX");
			AssertEquals("XX", entry.CH_Status);
			entry.UpdateStatusIfNotEmpty(ZString.Empty);
			AssertEquals("XX", entry.CH_Status);
		}

		public void TestEqualsAny()
		{
			ZString str1 = "Abc";
			ZString str2 = "abc";
			var arr = new ZString[]
			{
				"Abc", "Bcd", "Cde"
			};
			Assert(str1.EqualsAny(arr));
			Assert(!str2.EqualsAny(arr));
		}

		public void TestEqualsAnyIgnoringCase()
		{
			ZString str1 = "Abc";
			ZString str2 = "abc";
			var arr = new ZString[]
			{
				"Abc", "Bcd", "Cde"
			};
			Assert(str1.EqualsAnyIgnoringCase(arr));
			Assert(str2.EqualsAnyIgnoringCase(arr));
		}

		public void TestGetCredentialsSettingByBadgeCode()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.JE_CustomsProfile = "ABC";
			dec.JE_DeclarationReference = "DEC123";

			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
			{
				new BadgeCodeSetting
				{
					BadgeCode = "ABC",
					RL_PortCode = "GBLBA",
					Direction = "IMP",
					CSPCode = "CCSUK",
					MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Ccsuk,
					ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services
				}
			}))
			using (GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(dec.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, new CredentialsSettingCollection
			{
				new CredentialsSetting
				{
					BadgeCode = "ABC",
					Printer = "Location",
					Company = "Role",
					Password = "Password"
				}
			}))
			{
				var actual = dec.GetCredentialsSettingByBadgeCode();
				AssertEquals("ABC", actual.BadgeCode);
				AssertEquals("Location", actual.Printer);
				AssertEquals("Role", actual.Company);
				AssertEquals("Password", "Password", actual.Password);
			}
		}

		public void TestGetCredentialsKey()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";

			var declaration = Factory.New<JobDeclaration>();

			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
			{
				new BadgeCodeSetting
				{
					BadgeCode = "ABC",
					RL_PortCode = "GBLBA",
					Direction = "IMP",
					CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW,
					MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Ccsuk,
					ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services
				}
			}))
			using (GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, new CredentialsSettingCollection
			{
				new CredentialsSetting
				{
					BadgeCode = "ABC",
					Printer = "Location",
					Company = "Role",
					Username = "Username",
					Password = "Password"
				}
			}))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_DeclarationReference = "VICTEST";
				declaration.JE_CustomsProfile = "ABC";
				declaration.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888");

				AssertEquals("HYECMT.GB999999999888.ABC", declaration.GetCredentialsKey());
			}
		}

		public void TestGetFullProfile()
		{
			Func<string, string> genProfileCode = (badge) => $"12345123451234.{badge}";

			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "CD1", "12345123451234", PasswordTypesList.Codes.CDS);
			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "CD2", "12345123451234", PasswordTypesList.Codes.CDS);
			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "CH1", "12345123451234", PasswordTypesList.Codes.CDS);
			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "CH2", "12345123451234", PasswordTypesList.Codes.CDS);
			var testCompany = TestDataHelper.CreateCompanyAndBranch(Factory, "X");
			TestDataHelper.CreateCredentials(Factory, testCompany.PK, "CD3", "12345123451234", PasswordTypesList.Codes.CDS);
			TestDataHelper.CreateCredentials(Factory, testCompany.PK, "CD4", "12345123451234", PasswordTypesList.Codes.CDS);
			TestDataHelper.CreateCredentials(Factory, testCompany.PK, "CH3", "12345123451234", PasswordTypesList.Codes.CDS);
			TestDataHelper.CreateCredentials(Factory, testCompany.PK, "CH4", "12345123451234", PasswordTypesList.Codes.CDS);

			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
			{
				new BadgeCodeSetting { BadgeCode = "CD1", CSPCode = "CDS", ApplicationCode = "CDS" },
				new BadgeCodeSetting { BadgeCode = "CD2", CSPCode = "CDS", ApplicationCode = "CDS" },
				new BadgeCodeSetting { BadgeCode = "CH1", CSPCode = "MCP" },
				new BadgeCodeSetting { BadgeCode = "CH2", CSPCode = "CNS", ApplicationCode = "CHF" },
				new BadgeCodeSetting { BadgeCode = "XX1", ApplicationCode = "CDS" }
			}))
			{
				var profiles = GBExtensions.GetFullBadgeProfile();
				AssertEquals("Expecting 4", 4, profiles.Count);
				Assert("CD1", profiles.Contains(genProfileCode("CD1")));
				Assert("CD2", profiles.Contains(genProfileCode("CD2")));
				Assert("CH1", profiles.Contains(genProfileCode("CH1")));
				Assert("CH2", profiles.Contains(genProfileCode("CH2")));
			}
		}

		public void TestGetEori()
		{
			var dec = Factory.New<JobDeclaration>();
			OrgHeader org = Factory.New<OrgHeader>();
			RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedKingdom);
			OrgCusCode cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = country.Code;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = "PR";
			dec.Branch.GB_OH_OrgProxy = org.PK;
			AssertEquals("GBPR", dec.GetEori());
		}

		public void TestGetCredentialsKeyForCurrentCompany()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";

			AssertEquals("Empty CredentialsKey", ZString.Empty, GBExtensions.GetCredentialsKeyForCurrentCompany());

			var password1 = Factory.New<GlbExternalPassword_GB>();
			password1.GP_GC = GlbCompany.CurrentCompany.PK;
			password1.Badge = "ABC";
			password1.EORI = "GB11111";
			password1.Status = PasswordStatusList.Codes.Valid;
			password1.GP_IssueDate = new ZDate(2025, 1, 1);
			password1.GP_PasswordType = PasswordTypesList.Codes.CDS;

			var password2 = Factory.New<GlbExternalPassword_GB>();
			password2.GP_GC = GlbCompany.CurrentCompany.PK;
			password2.Badge = "CBA";
			password2.EORI = "GB22222";
			password2.Status = PasswordStatusList.Codes.Deactivated;
			password2.GP_IssueDate = new ZDate(2025, 1, 10);
			password2.GP_PasswordType = PasswordTypesList.Codes.CDS;

			var password3 = Factory.New<GlbExternalPassword_GB>();
			password3.GP_GC = GlbCompany.CurrentCompany.PK;
			password3.Badge = "CDS";
			password3.EORI = "GB12345";
			password3.Status = PasswordStatusList.Codes.Valid;
			password3.GP_IssueDate = new ZDate(2025, 1, 5);
			password3.GP_PasswordType = PasswordTypesList.Codes.CDS;

			Factory.Save();

			AssertEquals("CredentialsKey", "HYECMT.GB12345.CDS", GBExtensions.GetCredentialsKeyForCurrentCompany());
		}

		public void TestGetEuIdentificationNumberForCDS()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = org.MainAddress.PK;

			var cusCode1 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "0001", "AU");
			cusCode1.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
			AssertEquals("AU0001", docAddress.GetEuIdentificationNumberForCDS());

			var cusCode2 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "0002", "DE");
			cusCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 11);
			AssertEquals("DE0002", docAddress.GetEuIdentificationNumberForCDS());

			var cusCode3 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "XI0003", "GB");
			cusCode3.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 12);
			AssertEquals("XI0003", docAddress.GetEuIdentificationNumberForCDS());

			var cusCode4 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "0004", "GB");
			cusCode4.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 11);
			AssertEquals("GB0004", docAddress.GetEuIdentificationNumberForCDS());

			docAddress.E2_AddressOverride = ZBool.True;
			docAddress.E2_GovRegNum = "AB123";
			AssertEquals("AB123", docAddress.GetEuIdentificationNumberForCDS());
		}
	}
}
