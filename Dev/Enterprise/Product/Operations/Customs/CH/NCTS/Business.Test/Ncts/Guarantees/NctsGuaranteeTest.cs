using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsGuarantee))]
sealed class NctsGuaranteeTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookups() => AssertType<NctsGuaranteeLookups>(Guarantee.Lookups);

	public void TestValidation() => AssertType<NctsGuaranteeValidation>(Guarantee.Validation);

	public void TestPW_RX_NKCurrencyDefault() => AssertEquals("PW_RX_NKCurrency", Core.Constants.CurrencyCodes.Switzerland, Guarantee.PW_RX_NKCurrency);

	public void TestPW_BondNumber_EmptyAndReadOnly()
	{
		CombineAssertions(() =>
		{
			foreach (var bondType in new EUNctsGuaranteeTypeList().GetAllCodes())
			{
				Guarantee.PW_BondNumber = "ref";
				Guarantee.PW_BondType = bondType;
				if (bondType == EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver || bondType == EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee || bondType == EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor || bondType == EUNctsGuaranteeTypeList.Codes.FlatRateVoucher || bondType == EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage)
				{
					AssertEquals($"PW_BondType {bondType}, value", ZString.Empty, Guarantee.PW_BondNumber);
					AssertEquals($"PW_BondType {bondType}, read-only", false, Guarantee.PW_BondNumberInfo.ReadOnly);
				}
				else
				{
					AssertEquals($"PW_BondType {bondType}, value", ZString.Empty, Guarantee.PW_BondNumber);
					AssertEquals($"PW_BondType {bondType}, read-only", false, Guarantee.PW_BondNumberInfo.ReadOnly);
				}
			}
		});
	}

	public void TestPW_Password_EmptyAndReadOnly()
	{
		CombineAssertions(() =>
		{
			foreach (var bondType in new EUNctsGuaranteeTypeList().GetAllCodes())
			{
				Guarantee.PW_Password = "1234";
				Guarantee.PW_BondType = bondType;
				if (bondType == EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver || bondType == EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee || bondType == EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor || bondType == EUNctsGuaranteeTypeList.Codes.FlatRateVoucher || bondType == EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage)
				{
					AssertEquals($"PW_BondType {bondType}, value", ZString.Empty, Guarantee.PW_Password);
					AssertEquals($"PW_BondType {bondType}, read-only", false, Guarantee.PW_PasswordInfo.ReadOnly);
				}
				else
				{
					AssertEquals($"PW_BondType {bondType}, value", ZString.Empty, Guarantee.PW_Password);
					AssertEquals($"PW_BondType {bondType}, read-only", false, Guarantee.PW_PasswordInfo.ReadOnly);
				}
			}
		});
	}

	public void TestPW_BondNumber_AutoPopulated()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CreateGuaranteeHeader("CHTRA01", EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee);
		CreateGuaranteeHeader("CHTRA02", EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee);
		CreateGuaranteeHeader("CHTRA03", EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor);
		CreateGuaranteeHeader("CHTRA04", EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee);
		Factory.Save();

		Guarantee.PW_BondNumber = "ref";

		CombineAssertions(() =>
		{
			Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee;
			AssertEquals("Multiple CusGuaranteeHeaders, value", string.Empty, Guarantee.PW_BondNumber);
			AssertEquals("Multiple CusGuaranteeHeaders, read-only", false, Guarantee.PW_BondNumberInfo.ReadOnly);

			Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor;
			AssertEquals("Single CusGuaranteeHeader, value", "CHTRA03", Guarantee.PW_BondNumber);
			AssertEquals("Single CusGuaranteeHeader, read-only", false, Guarantee.PW_BondNumberInfo.ReadOnly);

			Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.FlatRateVoucher;
			AssertEquals("Not a drop-down list, value", string.Empty, Guarantee.PW_BondNumber);
			AssertEquals("Not a drop-down list, read-only", false, Guarantee.PW_BondNumberInfo.ReadOnly);
		});

		CusGuaranteeHeader CreateGuaranteeHeader(ZString number, ZString subType)
		{
			var result = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			result.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			result.CPH_Number = number;
			result.CPH_OH_PermitHolder = orgHeader.PK;
			result.CPH_SubType = subType;
			return result;
		}
	}

	public void TestPW_Password_AutoPopulated()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CreateGuaranteeHeader("CHTRA01", EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee, "110123");
		CreateGuaranteeHeader("CHTRA02", EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor, "220345", "230345");
		CreateGuaranteeHeader("CHTRA03", EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee);
		CreateGuaranteeHeader("CHTRA04", EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee);

		Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee;
		AssertEquals("Single CusGuaranteeRules", "1234", Guarantee.PW_Password);

		Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor;
		AssertEquals("Single CusGuaranteeRules", "1234", Guarantee.PW_Password);

		Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee;
		Guarantee.PW_BondNumber = "CHTRA04";
		AssertNullOrEmpty("No CusGuaranteeRules", Guarantee.PW_Password);

		void CreateGuaranteeHeader(ZString number, ZString subType, params ZString[] passwords)
		{
			var result = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			result.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
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

	public void TestReferenceNumberFieldType()
	{
		CombineAssertions(() =>
		{
			foreach (var bondType in new EUNctsGuaranteeTypeList().GetAllCodes())
			{
				Guarantee.PW_BondType = bondType;
				if (bondType.In(new string[] { EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver, EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee, EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor, EUNctsGuaranteeTypeList.Codes.FlatRateVoucher, EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage }))
				{
					AssertEquals($"PW_BondType {bondType}", nameof(FieldType.TextDropEdit), Guarantee.ReferenceNumberFieldType);
				}
				else
				{
					AssertEquals($"PW_BondType {bondType}", nameof(FieldType.Text), Guarantee.ReferenceNumberFieldType);
				}
			}
		});
	}

	public void TestIsDropdownForReferenceNumberAndCode()
	{
		CombineAssertions(() =>
		{
			Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
			AssertEquals("PW_BondType is '0'", true, Guarantee.IsDropdownForReferenceNumberAndCode);

			Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee;
			AssertEquals("PW_BondType is '1'", true, Guarantee.IsDropdownForReferenceNumberAndCode);

			Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor;
			AssertEquals("PW_BondType is '2'", true, Guarantee.IsDropdownForReferenceNumberAndCode);

			Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee;
			AssertEquals("PW_BondType isn't in ('0', '1', '2', '4', '9')", false, Guarantee.IsDropdownForReferenceNumberAndCode);

			Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.FlatRateVoucher;
			AssertEquals("PW_BondType is '4'", true, Guarantee.IsDropdownForReferenceNumberAndCode);

			Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage;
			AssertEquals("PW_BondType is '9'", true, Guarantee.IsDropdownForReferenceNumberAndCode);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => CreateGuarantee(Factory);
	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateGuarantee(factory);

	NctsGuarantee CreateGuarantee(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var guarantee = header.MovementHeader.Guarantees.AddNew();
		return guarantee;
	}

	NctsGuarantee Guarantee => guarantee ?? (guarantee = CreateGuarantee(Factory));
	NctsGuarantee guarantee;
}
