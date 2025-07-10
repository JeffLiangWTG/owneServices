using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(Guarantee))]
	sealed class GuaranteeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<GuaranteeLookups>(CreateGuarantee().Lookups);
		}

		public void TestValidation()
		{
			AssertType<GuaranteeValidation>(CreateGuarantee().Validation);
		}

		public void TestPW_BondNumber_EmptyAndReadOnly()
		{
			var guarantee = CreateGuarantee();
			CombineAssertions(() =>
			{
				foreach (var bondType in new NctsGuaranteeTypeList().GetAllCodes())
				{
					guarantee.PW_BondNumber = "ref";
					guarantee.PW_BondType = bondType;
					if (bondType == NctsGuaranteeTypeList.Codes._0 || bondType == NctsGuaranteeTypeList.Codes._1 || bondType == NctsGuaranteeTypeList.Codes._2 || bondType == NctsGuaranteeTypeList.Codes._4)
					{
						AssertEquals($"PW_BondType {bondType}, value", ZString.Empty, guarantee.PW_BondNumber);
						AssertEquals($"PW_BondType {bondType}, read-only", false, guarantee.PW_BondNumberInfo.ReadOnly);
					}
					else
					{
						AssertEquals($"PW_BondType {bondType}, value", ZString.Empty, guarantee.PW_BondNumber);
						AssertEquals($"PW_BondType {bondType}, read-only", true, guarantee.PW_BondNumberInfo.ReadOnly);
					}
				}
			});
		}

		public void TestPW_Password_EmptyAndReadOnly()
		{
			var guarantee = CreateGuarantee();
			CombineAssertions(() =>
			{
				foreach (var bondType in new NctsGuaranteeTypeList().GetAllCodes())
				{
					guarantee.PW_Password = "1234";
					guarantee.PW_BondType = bondType;
					if (bondType == NctsGuaranteeTypeList.Codes._0 || bondType == NctsGuaranteeTypeList.Codes._1 || bondType == NctsGuaranteeTypeList.Codes._2 || bondType == NctsGuaranteeTypeList.Codes._4)
					{
						AssertEquals($"PW_BondType {bondType}, value", ZString.Empty, guarantee.PW_Password);
						AssertEquals($"PW_BondType {bondType}, read-only", false, guarantee.PW_PasswordInfo.ReadOnly);
					}
					else
					{
						AssertEquals($"PW_BondType {bondType}, value", ZString.Empty, guarantee.PW_Password);
						AssertEquals($"PW_BondType {bondType}, read-only", true, guarantee.PW_PasswordInfo.ReadOnly);
					}
				}
			});
		}

		public void TestPW_BondNumber_ShouldBeReadonly_WhenSingleCusGuaranteeHeaderAndBondTypeNot0()
		{
			var guarantee = CreateGuarantee();
			var principalOrg = Factory.NewWithValidTestData<OrgHeader>();
			guarantee.NctsHeader.Principal.OrganisationPK = principalOrg.PK;
			var testCases = new[] { (NctsGuaranteeTypeList.Codes._0, false), (NctsGuaranteeTypeList.Codes._1, true), (NctsGuaranteeTypeList.Codes._2, true) };

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_OH_PermitHolder = principalOrg.PK;

			CombineAssertions(() =>
			{
				foreach (var (guaranteeType, expectedReadonly) in testCases)
				{
					guaranteeHeader.CPH_SubType = guaranteeType;
					guarantee.PW_BondType = guaranteeType;

					AssertEquals($"PW_BondType {guaranteeType}", expectedReadonly, guarantee.PW_BondNumberInfo.ReadOnly);
				}
			});
		}

		public void TestPW_Password_ShouldBeReadonly_WhenSingleCusGuaranteeHeaderAndBondTypeNot0()
		{
			var guarantee = CreateGuarantee();
			var principalOrg = Factory.NewWithValidTestData<OrgHeader>();
			guarantee.NctsHeader.Principal.OrganisationPK = principalOrg.PK;
			var testCases = new[] { (NctsGuaranteeTypeList.Codes._0, false), (NctsGuaranteeTypeList.Codes._1, true), (NctsGuaranteeTypeList.Codes._2, true) };

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_OH_PermitHolder = principalOrg.PK;

			CombineAssertions(() =>
			{
				foreach (var (guaranteeType, expectedReadonly) in testCases)
				{
					guaranteeHeader.CPH_SubType = guaranteeType;
					guarantee.PW_BondType = guaranteeType;

					AssertEquals($"PW_BondType {guaranteeType}", expectedReadonly, guarantee.PW_PasswordInfo.ReadOnly);
				}
			});
		}

		public void TestPW_Password_ShouldBeReadonly_WhenSingleCusGuaranteeHeaderAndBondTypeNot0AndHasMoreThan1AccessCode()
		{
			var guarantee = CreateGuarantee();
			var testCases = new[] { (NctsGuaranteeTypeList.Codes._0, false), (NctsGuaranteeTypeList.Codes._1, false), (NctsGuaranteeTypeList.Codes._2, false) };

			var guaranteeHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader1.CPH_Number = "DETRA01";
			guaranteeHeader1.CPH_SubType = GuaranteeSubTypeList.Codes._1;

			var guaranteeRule1 = guaranteeHeader1.CusGuaranteeRules.AddNew();
			guaranteeRule1.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber;
			guaranteeRule1.CPR_ValueFrom = "123";
			guaranteeRule1.CPR_ValueTo = "abc";

			var guaranteeRule2 = guaranteeHeader1.CusGuaranteeRules.AddNew();
			guaranteeRule2.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber;
			guaranteeRule2.CPR_ValueFrom = "456";
			guaranteeRule2.CPR_ValueTo = "def";

			Factory.Save();

			CombineAssertions(() =>
			{
				foreach (var (guaranteeType, expectedReadonly) in testCases)
				{
					guarantee.PW_BondType = guaranteeType;
					guarantee.PW_BondNumber = guaranteeHeader1.CPH_Number;
					guaranteeHeader1.CPH_SubType = guaranteeType;

					AssertEquals($"PW_BondType {guaranteeType}", expectedReadonly, guarantee.PW_PasswordInfo.ReadOnly);
				}
			});
		}

		public void TestPW_BondNumber_AutoPopulated()
		{
			var guarantee = CreateGuarantee();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			guarantee.NctsHeader.Principal.OrganisationPK = orgHeader.PK;
			CreateGuaranteeHeader("DETRA01", GuaranteeSubTypeList.Codes._1);
			CreateGuaranteeHeader("DETRA02", GuaranteeSubTypeList.Codes._1);
			CreateGuaranteeHeader("DETRA03", GuaranteeSubTypeList.Codes._2);
			CreateGuaranteeHeader("DETRA04", GuaranteeSubTypeList.Codes._4);
			Factory.Save();

			guarantee.PW_BondNumber = "ref";
			CombineAssertions(() =>
			{
				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._1;
				AssertEquals("Multiple CusGuaranteeHeaders, value", string.Empty, guarantee.PW_BondNumber);
				AssertEquals("Multiple CusGuaranteeHeaders, read-only", false, guarantee.PW_BondNumberInfo.ReadOnly);

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._2;
				AssertEquals("Single CusGuaranteeHeader, value", "DETRA03", guarantee.PW_BondNumber);
				AssertEquals("Single CusGuaranteeHeader, read-only", true, guarantee.PW_BondNumberInfo.ReadOnly);

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._4;
				AssertEquals("Not a drop-down list, value", string.Empty, guarantee.PW_BondNumber);
				AssertEquals("Not a drop-down list, read-only", false, guarantee.PW_BondNumberInfo.ReadOnly);
			});

			CusGuaranteeHeader CreateGuaranteeHeader(ZString number, ZString subType)
			{
				var result = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				result.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
				result.CPH_Number = number;
				result.CPH_OH_PermitHolder = orgHeader.PK;
				result.CPH_SubType = subType;
				return result;
			}
		}

		public void TestPW_BondNumber_ShouldBeEmpty_WhenGuaranteeHolderDiffersFromPrincipal()
		{
			var guarantee = CreateGuarantee();
			var principalOrg = Factory.NewWithValidTestData<OrgHeader>();
			guarantee.NctsHeader.Principal.OrganisationPK = principalOrg.PK;

			var differentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_OH_PermitHolder = differentOrg.PK;
			guaranteeHeader.CPH_SubType = GuaranteeSubTypeList.Codes._2;

			guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._2;
			AssertEquals("PW_BondNumber should be empty when holder differs from principal", ZString.Empty, guarantee.PW_BondNumber);
		}

		public void TestPW_Password_AutoPopulated()
		{
			var guarantee = CreateGuarantee();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			guarantee.NctsHeader.Principal.OrganisationPK = orgHeader.PK;
			CreateGuaranteeHeader("DETRA01", GuaranteeSubTypeList.Codes._1, "110123");
			CreateGuaranteeHeader("DETRA02", GuaranteeSubTypeList.Codes._2, "220345", "330345");
			CreateGuaranteeHeader("DETRA03", GuaranteeSubTypeList.Codes._4);
			CreateGuaranteeHeader("DETRA04", GuaranteeSubTypeList.Codes._4);

			guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._1;
			AssertEquals("Single CusGuaranteeRules", "1101", guarantee.PW_Password);

			guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._2;
			AssertNullOrEmpty("Multiple CusGuaranteeRules", guarantee.PW_Password);

			guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._4;
			guarantee.PW_BondNumber = "DETRA04";
			AssertNullOrEmpty("No CusGuaranteeRules", guarantee.PW_Password);

			void CreateGuaranteeHeader(ZString number, ZString subType, params ZString[] passwords)
			{
				var result = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				result.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
				result.CPH_Number = number;
				result.CPH_OH_PermitHolder = orgHeader.PK;
				result.CPH_SubType = subType;
				result.MainAccessCode = "1234";
				foreach (var password in passwords)
				{
					var rule = result.CusGuaranteeRules.AddNew();
					rule.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber;
					rule.CPR_ValueFrom = password;
				}
			}
		}

		public void TestPW_BondFiledPortEmptyWhenCopyCustomsOfficeFromGuaranteeHeaderIsFalse()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "DE address";
			address.OA_OH = org.PK;

			var guaranteeHeader = CreateCusGuarantee("GUARANTEE123", org, EUGuaranteeTypeList.Codes.TRA, "1");
			var permitRule = guaranteeHeader.CusGuaranteeRules.AddNew();
			permitRule.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.CUS;
			permitRule.CPR_ValueFrom = "DE1234";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var movementHeader = nctsHeader.MovementHeader;

			var guaranteeWithCopy = Factory.New<NctsGuaranteeForCopyCustomsOfficeFromGuaranteeHeaderTrueTest>();
			guaranteeWithCopy.Parent = movementHeader;
			guaranteeWithCopy.PW_BondType = "1";
			guaranteeWithCopy.PW_BondNumber = "GUARANTEE123";

			var guaranteeWithoutCopy = Factory.New<Guarantee>();
			guaranteeWithoutCopy.Parent = movementHeader;
			guaranteeWithoutCopy.PW_BondType = "1";
			guaranteeWithoutCopy.PW_BondNumber = "GUARANTEE123";

			AssertEquals("Filed port should be copied when CopyCustomsOfficeFromGuaranteeHeader = true", "DE1234", guaranteeWithCopy.PW_BondFiledPort.ToString());
			AssertEquals("Filed port should NOT be copied when CopyCustomsOfficeFromGuaranteeHeader = false", ZString.Empty, guaranteeWithoutCopy.PW_BondFiledPort);
		}

		public void TestPW_RX_NKCurrencyReadOnly()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AAA";
			CreateCusGuarantee("GUARANTEE1", org1, EUGuaranteeTypeList.Codes.TRA, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondType = "1";

			nctsGuarantee.PW_BondNumber = "GUARANTEE1";
			AssertEquals("Currency must be readonly, currency is filled from CusGuarantee", true, nctsGuarantee.PW_RX_NKCurrencyInfo.ReadOnly);

			nctsGuarantee.PW_RX_NKCurrency = ZString.Empty;
			AssertEquals("Currency must not be readonly, currency is empty", false, nctsGuarantee.PW_RX_NKCurrencyInfo.ReadOnly);
		}

		public void TestAccessCodeFieldType()
		{
			var guarantee = CreateGuarantee();
			var guaranteeHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader1.CPH_Number = "DETRA01";
			guaranteeHeader1.CPH_SubType = GuaranteeSubTypeList.Codes._1;

			var guaranteeRule1 = guaranteeHeader1.CusGuaranteeRules.AddNew();
			guaranteeRule1.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber;
			guaranteeRule1.CPR_ValueFrom = "123";
			guaranteeRule1.CPR_ValueTo = "abc";

			var guaranteeRule2 = guaranteeHeader1.CusGuaranteeRules.AddNew();
			guaranteeRule2.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber;
			guaranteeRule2.CPR_ValueFrom = "456";
			guaranteeRule2.CPR_ValueTo = "def";

			var guaranteeHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader2.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader2.CPH_Number = "DETRA01";
			guaranteeHeader2.CPH_SubType = GuaranteeSubTypeList.Codes._2;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("By default, PW_Password Field Type is Text", nameof(FieldType.Text), guarantee.AccessCodeFieldType);

				guarantee.PW_BondType = GuaranteeSubTypeList.Codes._1;
				guarantee.PW_BondNumber = guaranteeHeader1.CPH_Number;
				AssertEquals("When there are multiple AdditionalAccessCodes, PW_Password Field Type is DropEdit", nameof(FieldType.TextDropEdit), guarantee.AccessCodeFieldType);

				guarantee.PW_BondType = GuaranteeSubTypeList.Codes._2;
				AssertEquals("When there's one AdditionalAccessCode, PW_Password Field Type is Text", nameof(FieldType.Text), guarantee.AccessCodeFieldType);
			});
		}

		public void TestApportionmentType()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			AssertApportionmentType(nctsGuarantee, NctsGuaranteeTypeList.Codes._0, GuaranteeApportionmentType.EqualShare);
			AssertApportionmentType(nctsGuarantee, NctsGuaranteeTypeList.Codes._1, GuaranteeApportionmentType.EqualShare);
			AssertApportionmentType(nctsGuarantee, NctsGuaranteeTypeList.Codes._2, GuaranteeApportionmentType.None);
			AssertApportionmentType(nctsGuarantee, NctsGuaranteeTypeList.Codes._3, GuaranteeApportionmentType.None);
			AssertApportionmentType(nctsGuarantee, NctsGuaranteeTypeList.Codes._4, GuaranteeApportionmentType.Voucher);
			AssertApportionmentType(nctsGuarantee, NctsGuaranteeTypeList.Codes._8, GuaranteeApportionmentType.None);
			AssertApportionmentType(nctsGuarantee, NctsGuaranteeTypeList.Codes.B, GuaranteeApportionmentType.None);
			AssertApportionmentType(nctsGuarantee, NctsGuaranteeTypeList.Codes.R, GuaranteeApportionmentType.None);
			AssertApportionmentType(nctsGuarantee, "X", GuaranteeApportionmentType.None);
		}

		public void TestPW_BondNumber2_ReadOnly()
		{
			var guarantee = CreateGuarantee();
			CombineAssertions(() =>
			{
				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._8;
				AssertEquals("PW_BondType is '8'", false, guarantee.PW_BondNumber2Info.ReadOnly);

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes.R;
				AssertEquals("PW_BondType is 'R'", false, guarantee.PW_BondNumber2Info.ReadOnly);

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._0;
				AssertEquals("PW_BondType isn't in ('8', 'R')", true, guarantee.PW_BondNumber2Info.ReadOnly);
			});
		}

		public void TestIsDropdownForReferenceNumberAndCode()
		{
			var guarantee = CreateGuarantee();
			CombineAssertions(() =>
			{
				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._0;
				AssertEquals("PW_BondType is '0'", true, guarantee.IsDropdownForReferenceNumberAndCode);

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._1;
				AssertEquals("PW_BondType is '1'", true, guarantee.IsDropdownForReferenceNumberAndCode);

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._2;
				AssertEquals("PW_BondType is '2'", true, guarantee.IsDropdownForReferenceNumberAndCode);

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._3;
				AssertEquals("PW_BondType isn't in ('0', '1', '2')", false, guarantee.IsDropdownForReferenceNumberAndCode);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => CreateGuarantee();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateGuarantee(factory);

		Guarantee CreateGuarantee() => CreateGuarantee(Factory);
		Guarantee CreateGuarantee(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.MovementHeader.Guarantees.AddNew();
		}

		CusGuaranteeHeader CreateCusGuarantee(ZString number, OrgHeader permitHolder, ZString type, ZString subType)
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_Number = number;
			guarantee.CPH_OH_PermitHolder = permitHolder.PK;
			guarantee.CPH_Type = type;
			guarantee.CPH_SubType = subType;
			guarantee.CPH_StartDate = ZDate.BrettsBirthday;
			return guarantee;
		}

		void AssertApportionmentType(NctsGuarantee guarantee, ZString guaranteeType, GuaranteeApportionmentType apportionmentType)
		{
			guarantee.PW_BondType = guaranteeType;
			AssertEquals(apportionmentType, guarantee.ApportionmentType);
		}

		class NctsGuaranteeForCopyCustomsOfficeFromGuaranteeHeaderTrueTest : Guarantee
		{
			public NctsGuaranteeForCopyCustomsOfficeFromGuaranteeHeaderTrueTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool CopyCustomsOfficeFromGuaranteeHeader => true;
		}
	}
}
