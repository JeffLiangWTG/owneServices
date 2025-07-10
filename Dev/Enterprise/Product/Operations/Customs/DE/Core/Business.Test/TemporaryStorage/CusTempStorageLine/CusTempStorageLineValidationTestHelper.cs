using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	public static class CusTempStorageLineValidationTestHelper
	{
		public static void AssertEORIBranchHas4Digits(ZPropertyInfo eoriBranchNoInfo)
		{
			AssertionWithHtml.CombineAssertions(() =>
			{
				eoriBranchNoInfo.Value = new ZString("003");
				TestCaseWithFactory.AssertHasMessageError("3 digits", eoriBranchNoInfo, EoriBranch4DigitsMessageError);

				eoriBranchNoInfo.Value = new ZString("ZZDF");
				TestCaseWithFactory.AssertHasMessageError("Not numeric", eoriBranchNoInfo, EoriBranch4DigitsMessageError);

				eoriBranchNoInfo.Value = new ZString("0001");
				TestCaseWithFactory.AssertNoMessageErrors("Valid", eoriBranchNoInfo);
			});
		}

		public static void AssertEORIBranchHas4DigitsIfIsAWB(ZPropertyInfo eoriBranchNoInfo)
		{
			var storageDec = GetDeclaration(eoriBranchNoInfo);

			AssertionWithHtml.CombineAssertions(() =>
			{
				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				eoriBranchNoInfo.Value = new ZString("003");
				TestCaseWithFactory.AssertHasMessageError("AWB - 3 digits", eoriBranchNoInfo, EoriBranch4DigitsMessageError);

				eoriBranchNoInfo.Value = new ZString("ZZDF");
				TestCaseWithFactory.AssertHasMessageError("AWB - Not numeric", eoriBranchNoInfo, EoriBranch4DigitsMessageError);

				eoriBranchNoInfo.Value = new ZString("0001");
				TestCaseWithFactory.AssertNoMessageError("AWB - Numeric and 4", eoriBranchNoInfo, EoriBranch4DigitsMessageError);

				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				eoriBranchNoInfo.Value = new ZString("003");
				TestCaseWithFactory.AssertNoMessageError("REG - 3 digits", eoriBranchNoInfo, EoriBranch4DigitsMessageError);

				eoriBranchNoInfo.Value = new ZString("ZZDF");
				TestCaseWithFactory.AssertNoMessageError("REG - Not numeric", eoriBranchNoInfo, EoriBranch4DigitsMessageError);
			});
		}

		public static void AssertLineNoIsUnique(ZPropertyInfo linNoInfo)
		{
			const string messageError = "The combination of Line No. 1 and ATB No. AT/B/15/000001/03/2000/6000 already exists for this declaration";
			var storageDec = GetDeclaration(linNoInfo);
			storageDec.STH_OwnerReferenceNumber = "AT/B/15/000001/03/2000/6000";
			linNoInfo.Value = (ZInt)1;

			AssertionWithHtml.CombineAssertions(() =>
			{
				var storageLine2 = storageDec.CusTempStorageLines.AddNew();
				storageLine2.TSL_LineNo = 1;
				TestCaseWithFactory.AssertHasMessageError("Not unique", storageLine2.TSL_LineNoInfo, messageError);

				storageLine2.TSL_LineNo = 2;
				TestCaseWithFactory.AssertNoMessageError("Unique", storageLine2.TSL_LineNoInfo, messageError);
			});
		}

		public static void AssertEORIBranchEqualsAddressesEORIBranch(ZPropertyInfo eoriBranchInfo, ZPropertyInfo matchingOrgAddressInfo)
		{
			const string warningMessage = "does not match the organization registration number 0001 of type EBS entered for address BRANCH 1";
			var storageLine = GetLine(eoriBranchInfo);

			var orgHeader = storageLine.Factory.GetOrgHeaderWithEORIBranch("TESTORG", "0001");
			var mainAddress = orgHeader.MainAddress;
			mainAddress.OA_Code = "BRANCH 1";
			matchingOrgAddressInfo.Value = mainAddress.PK;

			AssertionWithHtml.CombineAssertions(() =>
			{
				eoriBranchInfo.Value = new ZString("0002");
				TestCaseWithFactory.AssertHasWarningContaining("Missmatch", eoriBranchInfo, warningMessage);

				eoriBranchInfo.Value = new ZString("0001");
				TestCaseWithFactory.AssertNoWarningContaining("Match", eoriBranchInfo, warningMessage);
			});
		}

		public static void AssertEORIBranchEqualsAddressesEORIBranchIfIsAWB(ZPropertyInfo eoriBranchInfo, Action eoriBranchValidator, ZPropertyInfo matchingOrgAddressInfo)
		{
			const string warningMessage = "The entered branch code 0002, does not match the organization registration number 0001 of type EBS entered for address BRANCH 1";
			var storageLine = GetLine(eoriBranchInfo);
			var storageDec = GetDeclaration(eoriBranchInfo);
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;

			var orgHeader = storageDec.Factory.GetOrgHeaderWithEORIBranch("TESTORG", "0001");
			var mainAddress = orgHeader.MainAddress;
			mainAddress.OA_Code = "BRANCH 1";
			matchingOrgAddressInfo.Value = mainAddress.PK;

			AssertionWithHtml.CombineAssertions(() =>
			{
				eoriBranchInfo.Value = new ZString("0002");
				TestCaseWithFactory.AssertHasWarning("AWB - Missmatch", eoriBranchInfo, warningMessage);

				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				eoriBranchValidator();
				TestCaseWithFactory.AssertNoWarning("REG - Missmatch", eoriBranchInfo, warningMessage);
			});
		}

		public static void AssertEORINumberMatchesOrganizationsEORINumber(ZPropertyInfo eoriNumberInfo, ZPropertyInfo matchingOrgAddressInfo, string organizationType)
		{
			var warningMessage = $"EORI GR1234567890 does not match organization registration number GR123456789 from {organizationType} TESTORG";
			var orgHeader = eoriNumberInfo.BizObj.Factory.GetOrgHeaderWithEori("TESTORG", "123456789", Core.Constants.CountryCodes.Greece);
			matchingOrgAddressInfo.Value = orgHeader.MainAddress.PK;

			AssertionWithHtml.CombineAssertions(() =>
			{
				eoriNumberInfo.Value = new ZString("GR1234567890");
				TestCaseWithFactory.AssertHasWarning("Mismatched", eoriNumberInfo, warningMessage);

				eoriNumberInfo.Value = new ZString("GR123456789");
				TestCaseWithFactory.AssertNoWarning("Matching", eoriNumberInfo, warningMessage);
			});
		}

		public static void AssertEORINumberMatchesOrganizationsEORINumberIfIsAWB(ZPropertyInfo eoriNumberInfo, Action eoriNumberValidator, ZPropertyInfo matchingOrgAddressInfo, string organizationType)
		{
			var warningMessage = $"EORI GR1234567890 does not match organization registration number GR123456789 from {organizationType} TESTORG";
			var storageDec = GetDeclaration(eoriNumberInfo);
			var orgHeader = storageDec.Factory.GetOrgHeaderWithEori("TESTORG", "123456789", Core.Constants.CountryCodes.Greece);
			matchingOrgAddressInfo.Value = orgHeader.MainAddress.PK;

			AssertionWithHtml.CombineAssertions(() =>
			{
				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				eoriNumberInfo.Value = new ZString("GR1234567890");
				TestCaseWithFactory.AssertHasWarning("AWB - Mismatched", eoriNumberInfo, warningMessage);

				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				eoriNumberValidator();
				TestCaseWithFactory.AssertNoWarning("REG - Mismatched", eoriNumberInfo, warningMessage);
			});
		}

		public static void AssertEORINumberExistsInOrganization(ZPropertyInfo eoriNumberInfo, Action eoriNumberValidator, ZPropertyInfo matchingOrgAddressInfo)
		{
			const string warningMessage = "No EORI code (EOR) exists in the organization registration numbers for organization TESTORG";
			var orgHeader = eoriNumberInfo.BizObj.Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			matchingOrgAddressInfo.Value = orgHeader.MainAddress.PK;

			AssertionWithHtml.CombineAssertions(() =>
			{
				eoriNumberInfo.Value = new ZString("GR123456789");
				TestCaseWithFactory.AssertHasWarning("No Eori", eoriNumberInfo, warningMessage);

				var cusCode = orgHeader.CustomsCodes.AddNew();
				cusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Core.Constants.CountryCodes.Greece, "123456789");
				eoriNumberValidator();
				TestCaseWithFactory.AssertNoWarning("Has Eori", eoriNumberInfo, warningMessage);
			});
		}

		public static void AssertEORINumberExistsInOrganizationIfIsAWB(ZPropertyInfo eoriNumberInfo, Action eoriNumberValidator, ZPropertyInfo matchingOrgAddressInfo)
		{
			const string warningMessage = "No EORI code (EOR) exists in the organization registration numbers for organization TESTORG";
			var storageDec = GetDeclaration(eoriNumberInfo);
			var orgHeader = storageDec.Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			matchingOrgAddressInfo.Value = orgHeader.MainAddress.PK;

			AssertionWithHtml.CombineAssertions(() =>
			{
				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				eoriNumberInfo.Value = new ZString("GR123456789");
				TestCaseWithFactory.AssertHasWarning("AWB - No Eori", eoriNumberInfo, warningMessage);

				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				eoriNumberValidator();
				TestCaseWithFactory.AssertNoWarning("REG - No Eori", eoriNumberInfo, warningMessage);
			});
		}

		public static void AssertEORIBranchExistsInPremiseAddress(ZPropertyInfo eoriBranchInfo, Action eoriBranchValidator, ZPropertyInfo matchingOrgAddressInfo)
		{
			const string warningMessage = "No branch code (EBS) for DE exists in the organization registration numbers for premise address";
			var orgHeader = eoriBranchInfo.BizObj.Factory.GetOrgHeaderWithEORIBranch("TESTORG", "0001");
			var mainAddress = orgHeader.MainAddress;
			mainAddress.OA_Code = "MainAddress";
			var address = orgHeader.Addresses.AddNew();
			address.OA_Code = "SecondAddress";
			matchingOrgAddressInfo.Value = address.PK;
			eoriBranchInfo.Value = new ZString("0001");

			AssertionWithHtml.CombineAssertions(() =>
			{
				TestCaseWithFactory.AssertHasWarningContaining("Premises Address without EORIBranch", eoriBranchInfo, warningMessage);

				matchingOrgAddressInfo.Value = mainAddress.PK;
				eoriBranchValidator();
				TestCaseWithFactory.AssertNoWarningContaining("Premises Address with EORIBranch", eoriBranchInfo, warningMessage);
			});
		}

		public static void AssertEORIBranchExistsInPremiseAddressIfIsAWB(ZPropertyInfo eoriBranchInfo, Action eoriBranchValidator, ZPropertyInfo matchingOrgAddressInfo)
		{
			const string warningMessage = "No branch code (EBS) for DE exists in the organization registration numbers for premise address MainAddress";
			var storageDec = GetDeclaration(eoriBranchInfo);
			var orgHeader = storageDec.Factory.New<OrgHeader>();
			var mainAddress = orgHeader.MainAddress;
			mainAddress.OA_Code = "MainAddress";
			matchingOrgAddressInfo.Value = mainAddress.PK;

			AssertionWithHtml.CombineAssertions(() =>
			{
				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				eoriBranchInfo.Value = new ZString("0001");
				TestCaseWithFactory.AssertHasWarning("AWB - Premises Address without EORIBranch", eoriBranchInfo, warningMessage);

				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				eoriBranchValidator();
				TestCaseWithFactory.AssertNoWarning("REG - Premises Address with EORIBranch", eoriBranchInfo, warningMessage);
			});
		}

		public static void AssertPackageQtyIsBetween1And99999(ZPropertyInfo packageQtyInfo)
		{
			const string messageError = "Package count must be between 1 and 99999.";

			AssertionWithHtml.CombineAssertions(() =>
			{
				packageQtyInfo.Value = (ZInt)100000;
				TestCaseWithFactory.AssertHasMessageError("Too big", packageQtyInfo, messageError);

				packageQtyInfo.Value = (ZInt)99999;
				TestCaseWithFactory.AssertNoMessageError("Upper bound", packageQtyInfo, messageError);

				packageQtyInfo.Value = (ZInt)0;
				TestCaseWithFactory.AssertHasMessageError("Too short", packageQtyInfo, messageError);

				packageQtyInfo.Value = (ZInt)1;
				TestCaseWithFactory.AssertNoMessageError("Lower bound", packageQtyInfo, messageError);
			});
		}

		public static void AssertPackageQtyIs1IfIsULD(ZPropertyInfo packageQtyInfo, Action packageQtyValidator)
		{
			const string messageError = "Owner reference type of 'ULD' requires package count to be 1.";
			var storageLine = GetLine(packageQtyInfo);
			AssertionWithHtml.CombineAssertions(() =>
			{
				storageLine.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.ULD;
				storageLine.TSL_PackageType = "1A";
				packageQtyInfo.Value = (ZInt)2;
				TestCaseWithFactory.AssertHasMessageError("NoSingleCountPackageType, ULD", packageQtyInfo, messageError);

				storageLine.TSL_PackageType = "VG";
				packageQtyValidator();
				TestCaseWithFactory.AssertNoMessageError("IsSingleCountPackageType, ULD", packageQtyInfo, messageError);

				storageLine.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.REG;
				storageLine.TSL_PackageType = "1A";
				packageQtyValidator();
				TestCaseWithFactory.AssertNoMessageError("IsSingleCountPackageType, REG", packageQtyInfo, messageError);
			});
		}

		public static void AssertPackageQtyIs1IfIsSingleCountPackageType(ZPropertyInfo packageQtyInfo)
		{
			const string messageError = "Package type of 'VG' requires package count to be 1.";
			var storageLine = GetLine(packageQtyInfo);
			storageLine.TSL_PackageType = "VG";

			AssertionWithHtml.CombineAssertions(() =>
			{
				packageQtyInfo.Value = (ZInt)2;
				TestCaseWithFactory.AssertHasMessageError("PackageQuantity: '2'", packageQtyInfo, messageError);

				packageQtyInfo.Value = (ZInt)1;
				TestCaseWithFactory.AssertNoMessageError("PackageQuantity: '1'", packageQtyInfo, messageError);
			});
		}

		static CusTempStorageLine GetLine(ZPropertyInfo cusTempStorageLineChildInfo) => (CusTempStorageLine)cusTempStorageLineChildInfo.BizObj;

		static CusTempStorageDec GetDeclaration(ZPropertyInfo cusTempStorageLineChildInfo) => ((CusTempStorageLine)cusTempStorageLineChildInfo.BizObj).Dec;

		const string EoriBranch4DigitsMessageError = "EORI Branch Suffix should be 4 digits and different to the EORI code, e.g. 0001";
	}
}
