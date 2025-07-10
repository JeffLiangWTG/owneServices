using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(EuOfficeCode))]
	class EuOfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<EuOfficeCode>
	{
		[ExpectNoExceptions]
		public void TestOfficeCodesUseDesInsteadOfCaa()
		{
			var declaration = Factory.New<JobDeclaration>();
			var office = declaration.CustomsOffices.AddNew();
			NUnit.Framework.Assert.That(!office.OfficeCodesUseDesInsteadOfCaa, NUnit.Framework.Is.True, "Should not use DES instead of CAA");
		}

		[ExpectNoExceptions]
		public void TestCY_OfficeAddress()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");

			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_Code = "FRXXXXXX";
			cusCodeList.ZZD_CodeType = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.France;
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddDays(2);
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddDays(-2);

			var officeAttribute = cusCodeList.Attributes.AddNew();
			officeAttribute.ZZE_ZXE_NKName = "Street";
			officeAttribute.ZZE_Value = "Impasse Lescuretie";

			var officeAttribute2 = cusCodeList.Attributes.AddNew();
			officeAttribute2.ZZE_ZXE_NKName = "PostCode";
			officeAttribute2.ZZE_Value = "24140";

			var officeAttribute3 = cusCodeList.Attributes.AddNew();
			officeAttribute3.ZZE_ZXE_NKName = "CITY";
			officeAttribute3.ZZE_Value = "MAURENS";

			var declaration = Factory.New<JobDeclaration>();
			var office = declaration.CustomsOffices.AddNew();
			office.CY_Code = Enterprise.Customs.EU.Business.EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep;
			office.CY_Data = cusCodeList.ZZD_Code;
			office.CY_Date = ZDateTime.Today;

			Factory.Save();

			NUnit.Framework.Assert.That(office.CY_OfficeAddress.ToString(), NUnit.Framework.Does.Contain("Impasse Lescuretie"), "Office address should not be empty");
			NUnit.Framework.Assert.That(office.CY_OfficeAddress.ToString(), NUnit.Framework.Does.Contain("24140"), "Office post code should not be empty");
			NUnit.Framework.Assert.That(office.CY_OfficeAddress.ToString(), NUnit.Framework.Does.Contain("MAURENS"), "Office city should not be empty");
		}

		[ExpectNoExceptions]
		public void TestIsLinkingToReadOnlyEMCSParent()
		{
			void AssertCanDeleteAndReadOnly(EuOfficeCode officeCode, bool expectedReadOnly, bool expectedCanDelete)
			{
				var messageForReadOnly = @"Should only be true with these three conditions:
a)The office code links to a EMCS Declaration.
b)The CY_Code is not DEL.
c)The message stauts of parent declaration is SNT or ACK.";

				var messageForCanDelete = @"Should only be false with these three conditions:
a)The office code links to a EMCS Declaration.
b)The CY_Code is not DEL.
c)The message stauts of parent declaration is SNT or ACK.";

				NUnit.Framework.Assert.That(officeCode.CanDelete, NUnit.Framework.Is.EqualTo(expectedCanDelete), messageForCanDelete);
				NUnit.Framework.Assert.That(officeCode.CY_CodeInfo.ReadOnly, NUnit.Framework.Is.EqualTo(expectedReadOnly), messageForReadOnly);
				NUnit.Framework.Assert.That(officeCode.CY_DataInfo.ReadOnly, NUnit.Framework.Is.EqualTo(expectedReadOnly), messageForReadOnly);
			}

			var defaultOffice = Factory.New<EuOfficeCode>();
			AssertCanDeleteAndReadOnly(defaultOffice, false, true);

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.EUEMCS.IJobDeclaration>();

			var officeWithEMCSParent = ((IEuOfficeCodeCollectionSupporter)declaration).CustomsOffices.AddNew();
			officeWithEMCSParent.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDelivery;
			Factory.InvalidateCachedProperties();

			AssertCanDeleteAndReadOnly(officeWithEMCSParent, false, true);

			declaration.JE_MessageStatus = EDIMessage.Status.Sent;
			Factory.InvalidateCachedProperties();

			AssertCanDeleteAndReadOnly(officeWithEMCSParent, false, true);

			declaration.JE_MessageStatus = EDIMessage.Status.Acknowledged;
			Factory.InvalidateCachedProperties();

			AssertCanDeleteAndReadOnly(officeWithEMCSParent, false, true);

			officeWithEMCSParent.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDispatch;
			Factory.InvalidateCachedProperties();

			AssertCanDeleteAndReadOnly(officeWithEMCSParent, true, false);

			declaration.JE_MessageStatus = EDIMessage.Status.Sent;
			Factory.InvalidateCachedProperties();

			AssertCanDeleteAndReadOnly(officeWithEMCSParent, true, false);
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var dec = Factory.New<JobDeclaration>();
			var officeCode = dec.CustomsOffices.AddNew();
			NUnit.Framework.Assert.That(officeCode.CY_Type, NUnit.Framework.Is.EqualTo(CusCodeDataTypeList.Codes.OfficeCode).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLookups()
		{
			var dec = Factory.New<JobDeclaration>();
			var officeCode = dec.CustomsOffices.AddNew();
			NUnit.Framework.Assert.That(officeCode.Lookups, NUnit.Framework.Is.TypeOf<EuOfficeCodeLookups>());
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			var dec = Factory.New<JobDeclaration>();
			var officeCode = dec.CustomsOffices.AddNew();
			NUnit.Framework.Assert.That(officeCode.Validation, NUnit.Framework.Is.TypeOf<EuOfficeCodeValidation>());
		}

		[ExpectNoExceptions]
		public void TestCY_Code()
		{
			var dec = Factory.New<JobDeclaration>();
			var officeCode = dec.CustomsOffices.AddNew();
			officeCode.CY_Code = "MAN";
			NUnit.Framework.Assert.That(officeCode.CY_Code, NUnit.Framework.Is.EqualTo("MAN").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(officeCode.CY_CodeInfo.MaxLength, NUnit.Framework.Is.EqualTo(3));
		}

		[ExpectNoExceptions]
		public void TestCY_Data()
		{
			var dec = Factory.New<JobDeclaration>();
			var officeCode = dec.CustomsOffices.AddNew();
			officeCode.CY_Data = "123";
			NUnit.Framework.Assert.That(officeCode.CY_Data, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(officeCode.CY_DataInfo.MaxLength, NUnit.Framework.Is.EqualTo(10));
		}

		[ExpectNoExceptions]
		public void TestCY_Date()
		{
			var dec = Factory.New<JobDeclaration>();
			var officeCode = dec.CustomsOffices.AddNew();
			officeCode.CY_Date = new ZDateTime(2017, 10, 29);
			NUnit.Framework.Assert.That(officeCode.CY_Date, NUnit.Framework.Is.EqualTo(new ZDateTime(2017, 10, 29)));
		}

		[ExpectNoExceptions]
		public void TestCY_OfficeDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				"123", "123 test only",
				ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			var officeCode = dec.CustomsOffices.AddNew();
			officeCode.CY_Data = "123";
			NUnit.Framework.Assert.That(officeCode.CY_OfficeDescription, NUnit.Framework.Is.EqualTo("123 test only").Using(CustomComparers.TypeComparison));

			Factory.Save();
			dec = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			officeCode = dec.CustomsOffices.Cast<EuOfficeCode>().First(x => x.CY_Data == "123");
			NUnit.Framework.Assert.That(officeCode.CY_OfficeDescription, NUnit.Framework.Is.EqualTo("123 test only").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLoad()
		{
			var dec = Factory.New<JobDeclaration>();
			NUnit.Framework.Assert.That(EuOfficeCode.Load<EuOfficeCode>(dec, ""), NUnit.Framework.Is.EqualTo(default(EuOfficeCode)));
			var officeCode1 = dec.CustomsOffices.AddNew("123");
			officeCode1.CY_SystemCreateTimeUtc = new ZDateTime(2022, 1, 4);
			var officeCode2 = dec.CustomsOffices.AddNew("123");
			officeCode2.CY_SystemCreateTimeUtc = new ZDateTime(2022, 1, 2);
			var officeCode3 = dec.CustomsOffices.AddNew("123");
			officeCode3.CY_SystemCreateTimeUtc = new ZDateTime(2022, 1, 3);
			var officeCode4 = dec.CustomsOffices.AddNew("123");
			officeCode4.CY_SystemCreateTimeUtc = new ZDateTime(2022, 1, 1);
			officeCode4.CY_Type = CusCodeDataTypeList.Codes.SupplementaryCode;
			NUnit.Framework.Assert.That(EuOfficeCode.Load<EuOfficeCode>(dec, "123"), NUnit.Framework.Is.SameAs(officeCode2));
		}

		[ExpectNoExceptions]
		public void TestCY_RoleCodeReturnsFromOfficeRoleForLookup()
		{
			var declaration = Factory.New<JobDeclarationForCustomsOfficeRequirementTest>();
			var officeHelper = declaration.CustomsOfficeRequirementHelper;
			officeHelper.SetOtherRequirements(new List<CustomsOfficeRequirement>
			{
				new CustomsOfficeRequirement
				{
					OfficeRole = "EAM",
					OfficeRolesForLookup = new ZString[] { "EXP" }
				}
			});

			CombineAssertions(() =>
			{
				var office1 = declaration.CustomsOffices.AddNew();
				office1.CY_Code = "EAM";
				NUnit.Framework.Assert.That(office1.CY_RoleCodes.First(), NUnit.Framework.Is.EqualTo("EXP").Using(CustomComparers.TypeComparison), "CY_RoleCode returns from OfficeRoleForLookup");

				var office2 = declaration.CustomsOffices.AddNew();
				office2.CY_Code = "AEC";
				NUnit.Framework.Assert.That(office2.Requirement, NUnit.Framework.Is.EqualTo(default(CustomsOfficeRequirement)), "We don't have a requirement with code AEC - should be [null]");
				NUnit.Framework.Assert.That(office2.CY_RoleCodes.First(), NUnit.Framework.Is.EqualTo("AEC").Using(CustomComparers.TypeComparison), "CY_RoleCode falls back to CY_Code");
			});
		}

		[ExpectNoExceptions]
		public void TestICustomsOfficeMembers()
		{
			var dec = Factory.New<JobDeclaration>();
			var officeCode = dec.CustomsOffices.AddNew();
			officeCode.CY_Data = "123";
			officeCode.CY_Date = new ZDateTime(2022, 02, 02);

			var customsOffice = officeCode as ICustomsOffice;
			NUnit.Framework.Assert.That(customsOffice, NUnit.Framework.Is.Not.EqualTo(default(ICustomsOffice)), "OfficeCode as ICustomsOffice - should not be [null]");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(customsOffice.OfficeCode, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison), nameof(ICustomsOffice.OfficeCode));
				NUnit.Framework.Assert.That(customsOffice.ArrivalTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2022, 02, 02)), nameof(ICustomsOffice.ArrivalTime));
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateAuthorisations()
		{
			var declaration = Factory.New<JobDeclaration>();
			var office = declaration.CustomsOffices.AddNew();

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			office.CY_Code = EuOfficeCodesTypes.Codes.SupervisingOffice;

			var entry1 = declaration.CustomsEntryInstructions.AddNew();
			var authorization1 = entry1.CusAuthorizationUsages.AddNew();
			authorization1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;

			var entry2 = declaration.CustomsEntryInstructions.AddNew();

			var entry3 = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				using (var testContext = new CusAuthorizationUsageValidationDeciderTestContext(declaration, isUCC6: true, isPopulateAuthorisationsForOfficeOfPresentationEnabled: false))
				{
					office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
					NUnit.Framework.Assert.That(entry2.CusAuthorizationUsages.Count, NUnit.Framework.Is.EqualTo(0), "IsPopulateAuthorisationsForOfficeOfPresentationEnabled is disabled");
				}

				using (var testContext = new CusAuthorizationUsageValidationDeciderTestContext(declaration, isUCC6: true, isPopulateAuthorisationsForOfficeOfPresentationEnabled: true))
				{
					office.CY_Code = EuOfficeCodesTypes.Codes.SupervisingOffice;
					office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
					NUnit.Framework.Assert.That(entry1.CusAuthorizationUsages.Count, NUnit.Framework.Is.EqualTo(1), "CusAuthorizationUsages should not be generated if CCL already exists");
					NUnit.Framework.Assert.That(entry2.CusAuthorizationUsages.Count, NUnit.Framework.Is.EqualTo(1), "Should populate CusAuthorizationUsages");
					NUnit.Framework.Assert.That(entry2.CusAuthorizationUsages[0].AGC_Code, NUnit.Framework.Is.EqualTo(CusAuthorizationHeaderTypeList.Codes.CentralizedClearance).Using(CustomComparers.TypeComparison), "Should populate CentralizedClearance CusAuthorizationUsage");
					NUnit.Framework.Assert.That(entry3.CusAuthorizationUsages.Count, NUnit.Framework.Is.EqualTo(1), "Should populate CusAuthorizationUsages");

					var entry4 = declaration.CustomsEntryInstructions.AddNew();
					((IBusinessObjectInternals)office).IsCopying = true;
					office.CY_Code = EuOfficeCodesTypes.Codes.SupervisingOffice;
					office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
					NUnit.Framework.Assert.That(entry4.CusAuthorizationUsages.Count, NUnit.Framework.Is.EqualTo(0), "Should not populate CusAuthorizationUsages when copy data happens");

					((IBusinessObjectInternals)office).IsCopying = false;
					office.CY_Code = EuOfficeCodesTypes.Codes.SupervisingOffice;
					office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
					NUnit.Framework.Assert.That(entry4.CusAuthorizationUsages.Count, NUnit.Framework.Is.EqualTo(1), "Should populate CusAuthorizationUsages when copy data does not happen");
				}
			});
		}

		protected override IEnumerable<EuOfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().CustomsOffices.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<JobDeclaration>().CustomsOffices.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().CustomsOffices.AddNew();
		}
	}
}
