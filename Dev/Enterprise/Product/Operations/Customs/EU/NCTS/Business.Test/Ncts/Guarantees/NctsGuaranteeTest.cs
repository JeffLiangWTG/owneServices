using System;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;
using GuaranteeTypeList = Enterprise.Customs.EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList;
using NctsGuaranteeCodes = Enterprise.Customs.EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsGuarantee))]
	public class NctsGuaranteeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultLiabilityAmountWhenZeroDuties()
		{
			var propertyInfo = typeof(NctsGuarantee).GetProperty("Phase5DefaultLiabilityAmount", BindingFlags.NonPublic | BindingFlags.Instance);
			var nctsGuarantee = (NctsGuarantee)Factory.New(TestedTypeHelper.GetTestedType(GetType()));
			AssertEquals(ExpectedPhase5DefaultLiabilityAmount, propertyInfo.GetValue(nctsGuarantee));
		}
		protected virtual ZDecimal ExpectedPhase5DefaultLiabilityAmount => ZDecimal.Zero;

		public void TestSetLiabilityAmountWithConsumeAll()
		{
			var nctsGuarantee = Factory.New<NctsGuarantee>();
			nctsGuarantee.SetLiabilityAmountWithConsumeAllIfPositive(200);
			AssertEquals("When CPH_Calc_OpeningBalance is not available then PW_BondAmount is equal to set amount", 200m, nctsGuarantee.PW_BondAmount);

			var principal = Factory.New<OrgHeader>();

			var guaranteeHeader = CreateCusGuarantee("GUA1", principal, EUGuaranteeTypeList.Codes.TRA, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.FillWithValidTestData();
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;
			nctsHeader.Principal.Address.OA_OH = principal.PK;

			guaranteeHeader.CPH_Number = "1234";
			guaranteeHeader.CPH_OH_PermitHolder = principal.PK;
			guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;

			nctsGuarantee = nctsHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "1234";

			guaranteeHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 100m, ZDecimal.Zero, transactionType: PermitTransactionTypeList.Codes.OBL);
			AssertEquals("Default CPH_Calc_OpeningBalance", new ZDecimal(100), guaranteeHeader.CPH_Calc_OpeningBalance);
			nctsGuarantee.SetLiabilityAmountWithConsumeAllIfPositive(200);
			AssertEquals("When CPH_Calc_OpeningBalance is available then PW_BondAmount is equal to CPH_Calc_OpeningBalance", guaranteeHeader.CPH_Calc_OpeningBalance, nctsGuarantee.PW_BondAmount);
		}

		public void TestNctsHeader_Phase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = Factory.New<NctsGuarantee>();
			guarantee.Parent = nctsHeader;
			AssertSame(nctsHeader, guarantee.NctsHeader);
		}

		public void TestNctsHeader_Phase5_Departure()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = Factory.New<NctsGuarantee>();
			guarantee.Parent = nctsHeader.MovementHeader;
			AssertSame(nctsHeader, guarantee.NctsHeader);
		}

		public void TestNctsHeader_Phase5_Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var guarantee = Factory.New<NctsGuarantee>();
			guarantee.Parent = nctsHeader.ArrivalMovementHeader;
			AssertSame(nctsHeader, guarantee.NctsHeader);
		}

		public void TestNctsHeader_Phase5Departure()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = Factory.New<NctsGuarantee>();
			guarantee.Parent = nctsHeader.MovementHeader;
			AssertSame(nctsHeader, guarantee.NctsHeader);
		}

		public void TestNewValues()
		{
			var cusBondData = Factory.New<NctsGuarantee>();
			AssertEquals("NCT", cusBondData.PW_ApplicationCode);
		}

		public void TestLookups()
		{
			var cusBondData = Factory.New<NctsGuarantee>();
			AssertEquals(typeof(NctsGuaranteeLookups), cusBondData.Lookups.GetType());

			// There has to be an easier way than this....
			var systemAttribute = (from Attribute a in cusBondData.PW_BondTypeInfo.PropertyDescriptor.Attributes where a is ListAttribute select a).FirstOrDefault();
			if (systemAttribute != null)
			{
				var listAttribute = systemAttribute as ListAttribute;
				if (listAttribute != null)
				{
					var memberName = listAttribute.ListDataSourceMember;
					AssertStartsWith("", "Lookups.", memberName);
					var listName = memberName.Replace("Lookups.", "");
					var lookups = cusBondData.Lookups;
					var propertyInfo = lookups.GetType().GetProperty(listName);
					var codeDescriptinPairList = propertyInfo.GetValue(lookups, Array.Empty<object>());
					AssertType(typeof(GuaranteeTypeList), codeDescriptinPairList);
				}
			}
		}

		public void TestValidation_Phase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsGuarantee = nctsHeader.Guarantees.AddNew();

			AssertType<NctsGuaranteeValidation>(nctsGuarantee.Validation);
		}

		public void TestValidation_Phase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();

			AssertType<NctsGuaranteePhase5Validation>(nctsGuarantee.Validation);
		}

		public void TestValidationDeciderPhase4Arrival()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var nctsGuarantee = header.Guarantees.AddNew();
			AssertNull("Phase4 Arrival", nctsGuarantee.ValidationDecider);
		}

		public void TestValidationDeciderPhase4Departure()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsGuarantee = header.Guarantees.AddNew();
			AssertNull("Phase4 Departure", nctsGuarantee.ValidationDecider);
		}

		public void TestValidationDeciderPhase5Departure()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsGuarantee = header.MovementHeader.Guarantees.AddNew();
			AssertType<NctsGuaranteeDeparturePhase5ValidationDecider>("Phase5 Departure", nctsGuarantee.ValidationDecider);
		}

		public void TestCusGuarantee_ShouldMatchByPermitNumber_Phase4()
		{
			var org1 = Factory.New<OrgHeader>();
			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var nctsGuarantee = nctsHeader.Guarantees.AddNew();

			CombineAssertions(() =>
			{
				nctsGuarantee.PW_BondNumber = "GUA0";
				AssertNull("CusGuarantee not matched due to the wrong permit number.", nctsGuarantee.CusGuarantee);

				nctsGuarantee.PW_BondNumber = "GUA1";
				AssertEquals("CusGuarantee matched by permit number.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);
			});
		}

		public void TestCusGuarantee_ShouldMatchByPermitNumber_Phase5Departure()
		{
			var org1 = Factory.New<OrgHeader>();
			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();

			CombineAssertions(() =>
			{
				nctsGuarantee.PW_BondNumber = "GUA0";
				AssertNull("CusGuarantee not matched due to the wrong permit number.", nctsGuarantee.CusGuarantee);

				nctsGuarantee.PW_BondNumber = "GUA1";
				AssertEquals("CusGuarantee matched by permit number.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);
			});
		}

		public void TestCusGuarantee_ShouldMatchedByHolder()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);

			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "GUA1";

			CombineAssertions(() =>
			{
				nctsHeader.Principal.E2_OA_Address = org2.MainAddress.PK;
				AssertNull("CusGuarantee not matched due to the wrong permit holder.", nctsGuarantee.CusGuarantee);

				nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
				AssertEquals("CusGuarantee matched by permit holder.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);
			});
		}

		public void TestCusGuarantee_ShouldOrderByStartDate_Phase4()
		{
			var org1 = Factory.New<OrgHeader>();

			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");
			guaranteeHeader1.CPH_StartDate = new ZDate(2020, 1, 1);
			var guaranteeHeader2 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");
			guaranteeHeader2.CPH_StartDate = new ZDate(2020, 2, 1);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var nctsGuarantee = nctsHeader.Guarantees.AddNew();

			CombineAssertions(() =>
			{
				nctsGuarantee.PW_BondNumber = "GUA1";
				AssertEquals("guarantee2 is fresher than guarantee 1, order by start date.", guaranteeHeader2.PK, nctsGuarantee.CusGuarantee.PK);

				guaranteeHeader2.CPH_StartDate = new ZDate(2019, 12, 1);
				AssertEquals("guarantee1 is fresher than guarantee 2, order by start date.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);
			});
		}

		public void TestCusGuarantee_ShouldOrderByStartDate_Phase5Departure()
		{
			var org1 = Factory.New<OrgHeader>();

			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");
			guaranteeHeader1.CPH_StartDate = new ZDate(2020, 1, 1);
			var guaranteeHeader2 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");
			guaranteeHeader2.CPH_StartDate = new ZDate(2020, 2, 1);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();

			CombineAssertions(() =>
			{
				nctsGuarantee.PW_BondNumber = "GUA1";
				AssertEquals("guarantee2 is fresher than guarantee 1, order by start date.", guaranteeHeader2.PK, nctsGuarantee.CusGuarantee.PK);

				guaranteeHeader2.CPH_StartDate = new ZDate(2019, 12, 1);
				AssertEquals("guarantee1 is fresher than guarantee 2, order by start date.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);
			});
		}
		public void TestCusGuarantee_ShouldOrderByCreateTime_IfStartDateAreSame_Phase4()
		{
			var org1 = Factory.New<OrgHeader>();

			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");
			guaranteeHeader1.CPH_StartDate = new ZDate(2020, 1, 1);
			guaranteeHeader1.CPH_SystemCreateTimeUtc = new ZDateTime(2020, 6, 1);
			var guaranteeHeader2 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");
			guaranteeHeader2.CPH_StartDate = new ZDate(2020, 2, 1);
			guaranteeHeader2.CPH_SystemCreateTimeUtc = new ZDateTime(2020, 5, 1);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			CombineAssertions(() =>
			{
				nctsGuarantee.PW_BondNumber = "GUA1";
				AssertEquals("guarantee 2 is fresher than guarantee 1, order by start date, even guarantee 1 has fresher creation time than guarantee 2.", guaranteeHeader2.PK, nctsGuarantee.CusGuarantee.PK);

				guaranteeHeader2.CPH_StartDate = new ZDate(2020, 1, 1);
				AssertEquals("guarantee 1 is fresher than guarantee 2, order by creation time, only if they have same start date.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);
			});
		}

		public void TestCusGuarantee_ShouldOrderByCreateTime_IfStartDateAreSame_Phase5Departure()
		{
			var org1 = Factory.New<OrgHeader>();

			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");
			guaranteeHeader1.CPH_StartDate = new ZDate(2020, 1, 1);
			guaranteeHeader1.CPH_SystemCreateTimeUtc = new ZDateTime(2020, 6, 1);
			var guaranteeHeader2 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");
			guaranteeHeader2.CPH_StartDate = new ZDate(2020, 2, 1);
			guaranteeHeader2.CPH_SystemCreateTimeUtc = new ZDateTime(2020, 5, 1);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			CombineAssertions(() =>
			{
				nctsGuarantee.PW_BondNumber = "GUA1";
				AssertEquals("guarantee 2 is fresher than guarantee 1, order by start date, even guarantee 1 has fresher creation time than guarantee 2.", guaranteeHeader2.PK, nctsGuarantee.CusGuarantee.PK);

				guaranteeHeader2.CPH_StartDate = new ZDate(2020, 1, 1);
				AssertEquals("guarantee 1 is fresher than guarantee 2, order by creation time, only if they have same start date.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);
			});
		}

		public void TestCusGuarantee_ShouldMatchGuaranteeOnly_Phase4()
		{
			var org1 = Factory.New<OrgHeader>();

			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "GUA1";

			CombineAssertions(() =>
			{
				guaranteeHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Permit;
				AssertNull("CusGuarantee not matched due to the wrong application code.", nctsGuarantee.CusGuarantee);

				guaranteeHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
				AssertNull("CusGuarantee not matched due to the wrong application code.", nctsGuarantee.CusGuarantee);

				guaranteeHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				AssertEquals("CusGuarantee matched.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);
			});
		}

		public void TestCusGuarantee_ShouldMatchGuaranteeOnly_Phase5Departure()
		{
			var org1 = Factory.New<OrgHeader>();

			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "GUA1";

			CombineAssertions(() =>
			{
				guaranteeHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Permit;
				AssertNull("CusGuarantee not matched due to the wrong application code.", nctsGuarantee.CusGuarantee);

				guaranteeHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
				AssertNull("CusGuarantee not matched due to the wrong application code.", nctsGuarantee.CusGuarantee);

				guaranteeHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				AssertEquals("CusGuarantee matched.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);
			});
		}

		public void TestCusGuarantee_ShouldMatchEUAndCTCountriesOnly_Phase4()
		{
			var org1 = Factory.New<OrgHeader>();

			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "GUA1";

			CombineAssertions(() =>
			{
				guaranteeHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				AssertEquals("CusGuarantee matched from EU countries.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);

				guaranteeHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				AssertEquals("CusGuarantee matched from CT countries.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);

				guaranteeHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
				AssertNull("CusGuarantee not matched because ZA is neither EU nor CT countries.", nctsGuarantee.CusGuarantee);
			});
		}

		public void TestCusGuarantee_ShouldMatchEUAndCTCountriesOnly_Phase5Departure()
		{
			var org1 = Factory.New<OrgHeader>();

			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "GUA1";

			CombineAssertions(() =>
			{
				guaranteeHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				AssertEquals("CusGuarantee matched from EU countries.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);

				guaranteeHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				AssertEquals("CusGuarantee matched from CT countries.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);

				guaranteeHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
				AssertNull("CusGuarantee not matched because ZA is neither EU nor CT countries.", nctsGuarantee.CusGuarantee);
			});
		}

		public void TestCusGuarantee_ShouldMatchByPermitNumber_Phase5Arrival()
		{
			var org1 = Factory.New<OrgHeader>();
			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TST, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			var nctsGuarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();

			CombineAssertions(() =>
			{
				nctsGuarantee.PW_BondNumber = "GUA0";
				AssertNull("CusGuarantee not matched due to the wrong permit number.", nctsGuarantee.CusGuarantee);

				nctsGuarantee.PW_BondNumber = "GUA1";
				AssertEquals("CusGuarantee matched by permit number.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);
			});
		}

		public void TestCusGuarantee_ShouldOrderByStartDate_Phase5Arrival()
		{
			var org1 = Factory.New<OrgHeader>();

			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TST, "1");
			guaranteeHeader1.CPH_StartDate = new ZDate(2020, 1, 1);
			var guaranteeHeader2 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TST, "1");
			guaranteeHeader2.CPH_StartDate = new ZDate(2020, 2, 1);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			var nctsGuarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();

			CombineAssertions(() =>
			{
				nctsGuarantee.PW_BondNumber = "GUA1";
				AssertEquals("guarantee2 is fresher than guarantee 1, order by start date.", guaranteeHeader2.PK, nctsGuarantee.CusGuarantee.PK);

				guaranteeHeader2.CPH_StartDate = new ZDate(2019, 12, 1);
				AssertEquals("guarantee1 is fresher than guarantee 2, order by start date.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);
			});
		}

		public void TestCusGuarantee_ShouldOrderByCreateTime_IfStartDateAreSame_Phase5Arrival()
		{
			var org1 = Factory.New<OrgHeader>();

			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TST, "1");
			guaranteeHeader1.CPH_StartDate = new ZDate(2020, 1, 1);
			guaranteeHeader1.CPH_SystemCreateTimeUtc = new ZDateTime(2020, 6, 1);
			var guaranteeHeader2 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TST, "1");
			guaranteeHeader2.CPH_StartDate = new ZDate(2020, 2, 1);
			guaranteeHeader2.CPH_SystemCreateTimeUtc = new ZDateTime(2020, 5, 1);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			var nctsGuarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
			CombineAssertions(() =>
			{
				nctsGuarantee.PW_BondNumber = "GUA1";
				AssertEquals("guarantee 2 is fresher than guarantee 1, order by start date, even guarantee 1 has fresher creation time than guarantee 2.", guaranteeHeader2.PK, nctsGuarantee.CusGuarantee.PK);

				guaranteeHeader2.CPH_StartDate = new ZDate(2020, 1, 1);
				AssertEquals("guarantee 1 is fresher than guarantee 2, order by creation time, only if they have same start date.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);
			});
		}

		public void TestCusGuarantee_ShouldMatchGuaranteeOnly_Phase5Arrival()
		{
			var org1 = Factory.New<OrgHeader>();

			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TST, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			var nctsGuarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
			nctsGuarantee.PW_BondNumber = "GUA1";

			CombineAssertions(() =>
			{
				guaranteeHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Permit;
				AssertNull("CusGuarantee not matched due to the wrong application code.", nctsGuarantee.CusGuarantee);

				guaranteeHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
				AssertNull("CusGuarantee not matched due to the wrong application code.", nctsGuarantee.CusGuarantee);

				guaranteeHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				AssertEquals("CusGuarantee matched.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);
			});
		}

		public void TestCusGuarantee_ShouldMatchEUAndCTCountriesOnly_Phase5Arrival()
		{
			var org1 = Factory.New<OrgHeader>();

			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TST, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			var nctsGuarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
			nctsGuarantee.PW_BondNumber = "GUA1";

			CombineAssertions(() =>
			{
				guaranteeHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				AssertEquals("CusGuarantee matched from EU countries.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);

				guaranteeHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				AssertEquals("CusGuarantee matched from CT countries.", guaranteeHeader1.PK, nctsGuarantee.CusGuarantee.PK);

				guaranteeHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
				AssertNull("CusGuarantee not matched because ZA is neither EU nor CT countries.", nctsGuarantee.CusGuarantee);
			});
		}

		public virtual void TestApportionmentType()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			new GuaranteeTypeList().GetAllCodes().ForEach(x =>
			{
				nctsGuarantee.PW_BondType = x;
				AssertEquals(GuaranteeApportionmentType.None, nctsGuarantee.ApportionmentType);
			});
		}

		public void TestHumanReadableName()
		{
			var nctsGuarantee = Factory.New<NctsGuarantee>();
			AssertEquals("HumanReadableName", "Guarantee", nctsGuarantee.HumanReadableName);
		}

		public void TestPW_SuretyCodeResourceStringData()
		{
			var nctsGuarantee = Factory.New<NctsGuarantee>();
			var resStringData = nctsGuarantee.PW_SuretyCodeInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Liability Fraction", resStringData.Caption);
				AssertEquals("ShortCaption", "Liability Fraction", resStringData.ShortCaption);
				AssertEquals("FullDescription", "Fraction for the Liability calculation", resStringData.FullDescription);
			});
		}

		public void TestCheckPW_OverrideResourceStringData()
		{
			var nctsGuarantee = Factory.New<NctsGuarantee>();
			var resStringData = nctsGuarantee.PW_OverrideInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Override", resStringData.Caption);
				AssertEquals("ShortCaption", "Liability Amount Override", resStringData.ShortCaption);
				AssertEquals("FullDescription", "Tick to override calculated Liability Amount and Fraction", resStringData.FullDescription);
			});
		}

		public void TestPW_BondAmount_OverrideResourceStringData()
		{
			var nctsGuarantee = Factory.New<NctsGuarantee>();
			var resStringData = nctsGuarantee.PW_BondAmountInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Liability Amount", resStringData.Caption);
				AssertEquals("ShortCaption", "Liability Amt.", resStringData.ShortCaption);
			});
		}

		public void TestLiabilityAmount_Phase5Arrival()
		{
			const decimal rate = 0.1m;
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var nctsGuarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
			var bill = nctsHeader.Bills.AddNew();
			_ = AddGoodsItemLiability(240m);

			AssertEquals("[PRE-CONDITION] IsPhase5Arrival", expected: true, nctsHeader.IsPhase5Arrival);
			AssertEquals("[PRE-CONDITION] PW_BondAmount", expected: 0m, nctsGuarantee.PW_BondAmount);
			CombineAssertions(() =>
			{
				Factory.Save();
				AssertEquals("LiabilityAmount (on save)", 24m, nctsGuarantee.LiabilityAmount);
				AssertEquals("PW_BondAmount (on save)", 24m, nctsGuarantee.PW_BondAmount);

				var goodsItem2 = AddGoodsItemLiability(1000m);
				Factory.Save();
				AssertEquals("LiabilityAmount (on adding bill)", 124m, nctsGuarantee.LiabilityAmount);
				AssertEquals("PW_BondAmount (on adding bill)", 124m, nctsGuarantee.PW_BondAmount);

				goodsItem2.BY_MonetaryValue = 1200m;
				Factory.Save();
				AssertEquals("LiabilityAmount (on updating bill)", 144m, nctsGuarantee.LiabilityAmount);
				AssertEquals("PW_BondAmount (on updating bill)", 144m, nctsGuarantee.PW_BondAmount);

				nctsGuarantee.PW_BondAmount = 5m;
				nctsGuarantee.PW_Override = true;
				AssertEquals("LiabilityAmount (with override)", 144m, nctsGuarantee.LiabilityAmount);
				AssertEquals("PW_BondAmount (with override)", 5m, nctsGuarantee.PW_BondAmount);

				nctsGuarantee.PW_BondAmount = 5m;
				nctsGuarantee.PW_Override = false;
				AssertEquals("LiabilityAmount (without override)", 144m, nctsGuarantee.LiabilityAmount);
				AssertEquals("PW_BondAmount (without override)", 144m, nctsGuarantee.PW_BondAmount);
			});
			return;

			NctsArrivalCargoDesc AddGoodsItemLiability(ZDecimal monetaryValue)
			{
				var mock = Factory.NewMoq<NctsArrivalCargoDesc>();
				var goodsItem = mock.Object;
				goodsItem.BY_MonetaryValue = monetaryValue;
				_ = mock
					.Setup(x => x.LiabilityAmount)
					.Returns(() => goodsItem.BY_MonetaryValue * rate);
				_ = bill.ArrivalGoodsItems.Add(goodsItem);
				return goodsItem;
			}
		}

		public void TestLiabilityAmount_Phase5Departure()
		{
			NCTSTestHelper.SetUpTariff(Factory, countrycode: Core.Constants.CountryCodes.Spain);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondAmount = 5m;

			AssertEquals("[PRE-CONDITION] IsPhase5Departure", expected: true, nctsHeader.IsPhase5Departure);
			AssertEquals("[PRE-CONDITION] PW_BondAmount", expected: 5m, nctsGuarantee.PW_BondAmount);
			CombineAssertions(() =>
			{
				Factory.Save();
				AssertEquals("PW_BondAmount (on save)", 5m, nctsGuarantee.PW_BondAmount);

				nctsGuarantee.PW_Override = true;
				AssertEquals("PW_BondAmount (with override)", 5m, nctsGuarantee.PW_BondAmount);

				nctsGuarantee.PW_Override = false;
				AssertEquals("PW_BondAmount (without override)", 5m, nctsGuarantee.PW_BondAmount);
			});
		}

		public void TestLiabilityAmount_LockUnlockState_with_Override()
		{
			var org1 = Factory.New<OrgHeader>();
			var guaranteeHeader = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			var nctsGuarantee = nctsHeader.Guarantees.AddNew();

			using (NctsConfigurationTestHelper.TemporarilySetGuaranteeOverrideSupportConfiguration(Factory, true))
			{
				nctsGuarantee.PW_Override = false;
				Assert("When Override is Unticked and without Guarantee, Bond Amount should not be readonly", !nctsGuarantee.PW_BondAmountInfo.ReadOnly);

				nctsGuarantee.PW_BondNumber = "GUA1";
				Assert("When Override is Unticked and with Guarantee, Bond Amount should be readonly", nctsGuarantee.PW_BondAmountInfo.ReadOnly);

				nctsGuarantee.PW_Override = true;
				Assert("When Override is Ticked with Guarantee, Bond Amount should not be readonly", !nctsGuarantee.PW_BondAmountInfo.ReadOnly);

				nctsGuarantee.PW_BondNumber = "XYZ";
				Assert("When Override is Ticked without Guarantee, Bond Amount should not be readonly", !nctsGuarantee.PW_BondAmountInfo.ReadOnly);
			}
		}

		public void TestLiabilityAmountAndFraction_LockState_Without_OverrideSupport()
		{
			var org1 = Factory.New<OrgHeader>();
			var guaranteeHeader = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");
			var guaranteeRule = guaranteeHeader.CusGuaranteeRules.AddNew();
			guaranteeRule.CPR_RuleCode = "LAP";
			guaranteeRule.CPR_ValueFrom = "THI";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			var nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "GUA1";
			nctsGuarantee.PW_Override = false;

			using (NctsConfigurationTestHelper.TemporarilySetGuaranteeOverrideSupportConfiguration(Factory, false))
			{
				AssertEquals("Pre-requisite: GuaranteeConfiguration OverrideSupport", false, nctsHeader.Configuration.GuaranteeConfiguration.OverrideSupport(nctsHeader));
				AssertEquals("When Override is Unticked, with matching LAP rule and without Override support. Surety Code should be readonly", false, nctsGuarantee.PW_SuretyCodeInfo.ReadOnly);
				AssertEquals("When Override is Unticked, with Guarantee and without Override support. Bond Amount should be readonly", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
			}
		}

		public void TestLiabilityAmountAndFraction_LockState_With_OverrideSupport()
		{
			var org1 = Factory.New<OrgHeader>();
			var guaranteeHeader = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");
			var guaranteeRule = guaranteeHeader.CusGuaranteeRules.AddNew();
			guaranteeRule.CPR_RuleCode = "LAP";
			guaranteeRule.CPR_ValueFrom = "THI";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "GUA1";
			nctsGuarantee.PW_Override = false;

			using (NctsConfigurationTestHelper.TemporarilySetGuaranteeOverrideSupportConfiguration(Factory, true))
			{
				AssertEquals("After modification: GuaranteeConfiguration OverrideSupport", true, nctsHeader.Configuration.GuaranteeConfiguration.OverrideSupport(nctsHeader));
				AssertEquals("When Override is Unticked, with matching LAP rule and with Override support. Surety Code should be readonly", true, nctsGuarantee.PW_SuretyCodeInfo.ReadOnly);
				AssertEquals("When Override is Unticked, with Guarantee and with Override support. Bond Amount should be readonly", true, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
			}
		}

		public void TestLiabilityFraction_LockUnlockState_with_Override()
		{
			var org1 = Factory.New<OrgHeader>();
			var guaranteeHeader = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "GUA1";

			using (NctsConfigurationTestHelper.TemporarilySetGuaranteeOverrideSupportConfiguration(Factory, true))
			{
				nctsGuarantee.PW_Override = true;
				Assert("When Override is Ticked without LAP rule, Surety Code should be readonly", nctsGuarantee.PW_SuretyCodeInfo.ReadOnly);

				nctsGuarantee.PW_Override = false;
				Assert("When Override is Unticked and without LAP rule, Surety Code should not be readonly", !nctsGuarantee.PW_SuretyCodeInfo.ReadOnly);

				var guaranteeRule = guaranteeHeader.CusGuaranteeRules.AddNew();
				guaranteeRule.CPR_RuleCode = "LAP";
				guaranteeRule.CPR_ValueFrom = "THI";
				Assert("When Override is Unticked and with matching LAP rule, Surety Code should be readonly", nctsGuarantee.PW_SuretyCodeInfo.ReadOnly);

				nctsGuarantee.PW_Override = true;
				Assert("When Override is Ticked with LAP rule, Surety Code should be readonly", nctsGuarantee.PW_SuretyCodeInfo.ReadOnly);
			}
		}

		public void TestLiabilityFraction_ClearState_with_Override()
		{
			var org1 = Factory.New<OrgHeader>();
			var guaranteeHeader = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "GUA1";

			nctsGuarantee.PW_SuretyCode = "XYZ";
			AssertEquals("Precondition: Liability fraction", "XYZ", nctsGuarantee.PW_SuretyCode);

			nctsGuarantee.PW_Override = true;
			AssertEquals("When Override is Ticked clear SuretyCode, Liability fraction", "", nctsGuarantee.PW_SuretyCode);

			nctsGuarantee.PW_Override = false;
			AssertEquals("When Override is Unticked, SuretyCode if no LAP rule. Liability fraction", "", nctsGuarantee.PW_SuretyCode);
		}

		public void TestLiabilityFractionReload_fromLAP_with_Override()
		{
			var org1 = Factory.New<OrgHeader>();
			var guaranteeHeader = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "GUA1";

			nctsGuarantee.PW_Override = false;
			AssertEquals("When Override is Unticked, blank SuretyCode if no LAP rule. Liability fraction", "", nctsGuarantee.PW_SuretyCode);

			var guaranteeRule = guaranteeHeader.CusGuaranteeRules.AddNew();
			guaranteeRule.CPR_RuleCode = "LAP";
			guaranteeRule.CPR_ValueFrom = "THI";

			nctsGuarantee.PW_SuretyCode = "XYZ";
			AssertEquals("Precondition: Liability fraction", "XYZ", nctsGuarantee.PW_SuretyCode);

			nctsGuarantee.PW_Override = true;
			AssertEquals("When Override is Ticked clear SuretyCode, Liability fraction", "", nctsGuarantee.PW_SuretyCode);

			nctsGuarantee.PW_Override = false;
			AssertEquals("When Override is Unticked, reload SuretyCode from LAP rule. Liability fraction", "THI", nctsGuarantee.PW_SuretyCode);
		}

		public void TestLiabilityFractionReload_WithGoodsItems_OverrideChange_Phase4()
		{
			NCTSTestHelper.SetUpTariff(Factory);

			var org1 = Factory.New<OrgHeader>();
			var guaranteeHeader = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");
			var guaranteeRule = guaranteeHeader.CusGuaranteeRules.AddNew();
			guaranteeRule.CPR_RuleCode = "LAP";
			guaranteeRule.CPR_ValueFrom = "THI";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var guarantee = nctsHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "GUA1";

			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem.BY_MonetaryValue = 100;

			guarantee.PW_Override = true;
			AssertEquals("When Override is Ticked clear SuretyCode, Liability fraction", "", guarantee.PW_SuretyCode);

			guarantee.PW_Override = false;
			AssertEquals("When Override is Unticked, reload SuretyCode from LAP rule. Liability fraction", "THI", guarantee.PW_SuretyCode);
		}

		public void TestLiabilityFractionReload_WithGoodsItems_OverrideChange_Phase5Departure()
		{
			NCTSTestHelper.SetUpTariff(Factory);

			var org1 = Factory.New<OrgHeader>();
			var guaranteeHeader = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");
			var guaranteeRule = guaranteeHeader.CusGuaranteeRules.AddNew();
			guaranteeRule.CPR_RuleCode = "LAP";
			guaranteeRule.CPR_ValueFrom = "THI";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "GUA1";

			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem.BY_MonetaryValue = 100;

			guarantee.PW_Override = true;
			AssertEquals("When Override is Ticked clear SuretyCode, Liability fraction", "", guarantee.PW_SuretyCode);

			guarantee.PW_Override = false;
			AssertEquals("When Override is Unticked, reload SuretyCode from LAP rule. Liability fraction", "THI", guarantee.PW_SuretyCode);
		}

		public void TestUpdateLiabilityFractionFromGurantee_Phase5()
		{
			CombineAssertions(() =>
			{
				var guarantee = CreateNctsGuarantee(CusInBondApplicationCodeList.Codes.NCTS5, "THI", NctsMovementType.Codes.Departure);
				guarantee.PW_BondNumber = "GUA1";
				AssertEquals("Update Liability Fraction From Gurantee", "THI", guarantee.PW_SuretyCode);
			});
		}

		public void TestUpdateLiabilityFractionFromGurantee_Phase4()
		{
			var guarantee = CreateNctsGuarantee(CusInBondApplicationCodeList.Codes.NCTS4, "THI", NctsMovementType.Codes.DepartureAndArrival);
			guarantee.PW_BondNumber = "GUA1";
			AssertEquals("Do not update Liability Fraction From Gurantee", "", guarantee.PW_SuretyCode);
		}

		public void TestGuaranteeAmountIsRecalculatedOnChangeOfLiabilityFraction()
		{
			var guarantee = CreateNctsGuarantee(CusInBondApplicationCodeList.Codes.NCTS5, "THI", NctsMovementType.Codes.Departure);
			var goodsItem = guarantee.NctsHeader.Bills.AddNew().GoodsItems.AddNew();

			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem.BY_MonetaryValue = 1_000m;

			var guaranteeHeader = CreateCusGuarantee("GUA2", guarantee.NctsHeader.Principal.Organisation, EUGuaranteeTypeList.Codes.TRA, "1");
			var guaranteeRule = guaranteeHeader.CusGuaranteeRules.AddNew();
			guaranteeRule.CPR_RuleCode = "LAP";
			guaranteeRule.CPR_ValueFrom = "HAL";

			CombineAssertions(() =>
			{
				AssertEquals("Duty Amount", 120m, goodsItem.DutyAmount);
				AssertEquals("Vat Amount", 224m, goodsItem.VatAmount);

				guarantee.PW_BondNumber = "GUA1";
				guarantee.PW_Override = false;
				Factory.Save();
				AssertEquals("Liability Amount for fraction THI (first guarantee)", new ZDecimal(103.20), guarantee.PW_BondAmount);

				guarantee.PW_BondNumber = "GUA2";
				AssertEquals("Liability Fraction should be changed to HAL", "HAL", guarantee.PW_SuretyCode);
				Factory.Save();
				AssertEquals("Liability Amount for fraction HAL (second guarantee)", 172m, guarantee.PW_BondAmount);
			});
		}

		public void TestSynchroniseWithGuaranteeHeader()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AAA";
			var guaranteeHeader = CreateCusGuarantee("GUARANTEE1", org1, EUGuaranteeTypeList.Codes.TRA, "1");
			guaranteeHeader.CPH_UnitOfMeasure = Core.Constants.CurrencyCodes.UnitedStates;
			guaranteeHeader.MainAccessCode = "1234";
			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "UNKNOWN";

			CombineAssertions(() =>
			{
				AssertEquals("Guarantee does not exist, Currency cannot be synchronised", "", guarantee.PW_RX_NKCurrency);
				AssertEquals("Guarantee does not exist, AccessCode cannot be synchronised", "", guarantee.PW_Password);
				AssertEquals("Guarantee does not exist, Type cannot be synchronised", "", guarantee.PW_BondType);

				guarantee.PW_BondNumber = guaranteeHeader.CPH_Number;
				AssertEquals("Currency must be synchronised from Guarantee", Core.Constants.CurrencyCodes.UnitedStates, guarantee.PW_RX_NKCurrency);
				AssertEquals("AccessCode must be synchronised from Guarantee", "1234", guarantee.PW_Password);
				AssertEquals("Type must be synchronised from Guarantee", "1", guarantee.PW_BondType);
			});
		}

		public void TestCopyCustomsOfficeFromGuaranteeHeader()
		{
			var propertyInfo = typeof(NctsGuarantee).GetProperty("CopyCustomsOfficeFromGuaranteeHeader", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertEquals(true, propertyInfo.GetValue(Factory.New<NctsGuarantee>()));
		}

		public void TestPW_RX_NKCurrency()
		{
			var nctsGuarantee = Factory.New<NctsGuarantee>();
			NCTSTestHelper.AssertCaptionsAndFullDescription(nctsGuarantee.PW_RX_NKCurrencyInfo, "Currency", string.Empty, "Curr.", "[99 03 012 000] Currency");
		}

		public void TestPW_RX_NKCurrency_ClearCurrencyRelatedPropertiesIfOverridden_IsOverridden()
		{
			var nctsGuarantee = Factory.New<NctsGuarantee>();
			nctsGuarantee.PW_Override = true;
			nctsGuarantee.PW_BondAmount = 1;
			nctsGuarantee.PW_RX_NKCurrency = "USD";
			CombineAssertions(() =>
			{
				AssertEquals(expected: false, nctsGuarantee.PW_Override);
				AssertEquals(ZDecimal.Zero, nctsGuarantee.PW_BondAmount);
			});
		}

		public void TestPW_RX_NKCurrency_ClearCurrencyRelatedPropertiesIfOverridden_IsNotOverridden()
		{
			var nctsGuarantee = Factory.New<NctsGuarantee>();
			nctsGuarantee.PW_BondAmount = 1;
			nctsGuarantee.PW_RX_NKCurrency = "USD";
			CombineAssertions(() =>
			{
				AssertEquals(expected: false, nctsGuarantee.PW_Override);
				AssertEquals(1m, nctsGuarantee.PW_BondAmount);
			});
		}

		public void TestPW_RX_NKCurrencyReadOnly()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AAA";
			var guaranteeHeader = CreateCusGuarantee("GUARANTEE1", org1, EUGuaranteeTypeList.Codes.TRA, "1");
			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();

			CombineAssertions(() =>
			{
				nctsGuarantee.PW_BondNumber = "UNKNOWN";
				AssertEquals("Currency must not be readonly, unknown guarantee is entered", false, nctsGuarantee.PW_RX_NKCurrencyInfo.ReadOnly);

				nctsGuarantee.PW_BondNumber = "GUARANTEE1";
				AssertEquals("Currency must be readonly, currency is filled from CusGuarantee", true, nctsGuarantee.PW_RX_NKCurrencyInfo.ReadOnly);
			});
		}

		public void TestIsComprehensiveGuaranteeOrIndividualGuaranteeByGuarantor_Phase4() => CombineAssertions(() =>
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			var guarantee = nctsHeader.Guarantees.AddNew();

			guarantee.PW_BondType = GuaranteeTypeList.Codes.ComprehensiveGuarantee;
			Assert("BondType is ComprehensiveGuarantee", guarantee.IsComprehensiveGuaranteeOrIndividualGuaranteeByGuarantor);

			guarantee.PW_BondType = GuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor;
			Assert("BondType is IndividualGuaranteeByGuarantor", guarantee.IsComprehensiveGuaranteeOrIndividualGuaranteeByGuarantor);

			guarantee.PW_BondType = GuaranteeTypeList.Codes.GuaranteeWaiver;
			AssertEquals("BondType not ComprehensiveGuarantee or IndividualGuaranteeByGuarantor", false, guarantee.IsComprehensiveGuaranteeOrIndividualGuaranteeByGuarantor);
		});

		public void TestIsComprehensiveGuaranteeOrIndividualGuaranteeByGuarantor_Phase5Departure() => CombineAssertions(() =>
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();

			guarantee.PW_BondType = GuaranteeTypeList.Codes.ComprehensiveGuarantee;
			Assert("BondType is ComprehensiveGuarantee", guarantee.IsComprehensiveGuaranteeOrIndividualGuaranteeByGuarantor);

			guarantee.PW_BondType = GuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor;
			Assert("BondType is IndividualGuaranteeByGuarantor", guarantee.IsComprehensiveGuaranteeOrIndividualGuaranteeByGuarantor);

			guarantee.PW_BondType = GuaranteeTypeList.Codes.GuaranteeWaiver;
			AssertEquals("BondType not ComprehensiveGuarantee or IndividualGuaranteeByGuarantor", false, guarantee.IsComprehensiveGuaranteeOrIndividualGuaranteeByGuarantor);
		});

		public void TestIsGuaranteeTypeWithAmountNR0067() => CombineAssertions(() =>
		{
			var bondTypesWithAmount = new[] {
				NctsGuaranteeCodes.GuaranteeWaiver,
				NctsGuaranteeCodes.ComprehensiveGuarantee,
				NctsGuaranteeCodes.IndividualGuaranteeByGuarantor,
				NctsGuaranteeCodes.FlatRateVoucher,
				NctsGuaranteeCodes.IndividualGuaranteeWithMultipleUsage,
			};

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();

			foreach (var bondType in new GuaranteeTypeList().GetAllCodes())
			{
				guarantee.PW_BondType = bondType;
				AssertEquals($"PW_BondType={bondType}", bondTypesWithAmount.Contains(bondType), guarantee.IsGuaranteeTypeWithAmountNR0067);
			}
		});

		public void TestPW_BondType_ReadOnlyAndEmptyReferenceGroup()
		{
			var allCodes = new GuaranteeTypeList().GetAllCodes();
			var guaranteeTypeWithReference = ImmutableHashSet.Create(
				NctsGuaranteeCodes.GuaranteeWaiver,
				NctsGuaranteeCodes.ComprehensiveGuarantee,
				NctsGuaranteeCodes.IndividualGuaranteeByGuarantor,
				NctsGuaranteeCodes.CashDepositGuarantee,
				NctsGuaranteeCodes.FlatRateVoucher,
				NctsGuaranteeCodes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur,
				NctsGuaranteeCodes.IndividualGuaranteeWithMultipleUsage
			);

			var guaranteeTypeNotWithReference = allCodes.Except(guaranteeTypeWithReference.Cast<string>());
			using (var context = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				context.EnableRule(r => r.IsRuleC0085_2Active);

				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

				CombineAssertions("When Departure NCTS5 and GuaranteeTypeOther, Fields are empty and ReadOnly", () =>
				{
					guaranteeTypeNotWithReference.ForEach(bondType =>
					{
						AssertIsEmptyAndIsReadOnly(nctsHeader, CusInBondApplicationCodeList.Codes.NCTS5, bondType, expectedIsEmptyAndReadOnly: true);
					});
				});

				CombineAssertions("When Departure NCTS5 and GuaranteeTypeWithReferenceCL076, Fields are not empty and not ReadOnly", () =>
				{
					guaranteeTypeWithReference.ForEach(bondType =>
					{
						AssertIsEmptyAndIsReadOnly(nctsHeader, CusInBondApplicationCodeList.Codes.NCTS5, bondType, expectedIsEmptyAndReadOnly: false);
					});
				});

				CombineAssertions("When Departure NCTS4, Fields are not empty and not ReadOnly", () =>
				{
					allCodes.ForEach(bondType =>
					{
						AssertIsEmptyAndIsReadOnly(nctsHeader, CusInBondApplicationCodeList.Codes.NCTS4, bondType, expectedIsEmptyAndReadOnly: false);
					});
				});
			}
		}

		public void TestPW_Status_ShouldDirtyStatusBeReset_WhenPW_AmountChanged()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_Status = NctsGuarantee.DirtyStatus;

			guarantee.PW_BondAmount += 1;

			AssertNullOrEmpty(guarantee.PW_Status);
		}

		void AssertIsEmptyAndIsReadOnly(NctsHeader nctsHeader, string applicationCode, string bondType, bool expectedIsEmptyAndReadOnly)
		{
			var header = nctsHeader;
			header.BH_ApplicationCode = applicationCode;
			var nctsGuarantee = header.IsPhase5Departure ? header.MovementHeader.Guarantees.AddNew() : header.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "111";
			nctsGuarantee.PW_Password = "1111";
			nctsGuarantee.PW_BondAmount = 1;
			nctsGuarantee.PW_BondFiledPort = "1111";

			nctsGuarantee.PW_BondType = bondType;

			AssertEquals($"Is PW_BondNumber Empty for Bond Type {bondType}?", expectedIsEmptyAndReadOnly, nctsGuarantee.PW_BondNumber.IsEmpty);
			AssertEquals($"Is PW_Password Empty for Bond Type {bondType}?", expectedIsEmptyAndReadOnly, nctsGuarantee.PW_Password.IsEmpty);
			AssertEquals($"Is PW_BondAmount Empty for Bond Type {bondType}?", expectedIsEmptyAndReadOnly, nctsGuarantee.PW_BondAmount.IsEmpty);
			AssertEquals($"is PW_BondFiledPort Empty for Bond Type {bondType}?", expectedIsEmptyAndReadOnly, nctsGuarantee.PW_BondFiledPort.IsEmpty);

			AssertEquals($"Is PW_BondNumber ReadOnly for Bond Type {bondType}?", expectedIsEmptyAndReadOnly, nctsGuarantee.PW_BondNumberInfo.ReadOnly);
			AssertEquals($"Is PW_Password ReadOnly for Bond Type {bondType}?", expectedIsEmptyAndReadOnly, nctsGuarantee.PW_PasswordInfo.ReadOnly);
			AssertEquals($"Is PW_BondAmount ReadOnly for Bond Type {bondType}?", expectedIsEmptyAndReadOnly, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
			AssertEquals($"Is PW_BondFiledPort ReadOnly for Bond Type {bondType}?", expectedIsEmptyAndReadOnly, nctsGuarantee.PW_BondFiledPortInfo.ReadOnly);
		}

		public void TestPW_BondFiledPortWhenGuaranteeReferenceNumberIsFilledInPhase5()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "IT address";
			address.OA_OH = org.PK;

			var guaranteeHeader1 = CreateCusGuarantee("GUARANTEE123", org, EUGuaranteeTypeList.Codes.TRA, "1");
			CreateCusGuaranteeRule(guaranteeHeader1, EU.Business.PermitRuleCodeList.Codes.CUS, "AT9300");

			var guaranteeHeader2 = CreateCusGuarantee("GUARANTEE456", org, EUGuaranteeTypeList.Codes.TRA, "1");
			CreateCusGuaranteeRule(guaranteeHeader2, EU.Business.PermitRuleCodeList.Codes.CUS, "AT9300");
			CreateCusGuaranteeRule(guaranteeHeader2, EU.Business.PermitRuleCodeList.Codes.LAP, "FUL");

			var guaranteeHeader3 = CreateCusGuarantee("GUARANTEE789", org, EUGuaranteeTypeList.Codes.TRA, "1");
			CreateCusGuaranteeRule(guaranteeHeader3, EU.Business.PermitRuleCodeList.Codes.CUS, "AT9300");
			CreateCusGuaranteeRule(guaranteeHeader3, EU.Business.PermitRuleCodeList.Codes.CUS, "BX7800");

			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var movementHeader = nctsHeader.MovementHeader;

			var guarantee1 = movementHeader.Guarantees.AddNew();
			guarantee1.PW_BondNumber = "GUARANTEE123";

			AssertEquals("PW_BondFiledPort when guarantee reference is filled having single CUS permit rule", "AT9300", movementHeader.Guarantees[0].PW_BondFiledPort);

			var guarantee2 = movementHeader.Guarantees.AddNew();
			guarantee2.PW_BondNumber = "GUARANTEE456";

			AssertEquals("PW_BondFiledPort when guarantee reference is filled having a CUS and LAP permit rule", "AT9300", movementHeader.Guarantees[1].PW_BondFiledPort);

			var guarantee3 = movementHeader.Guarantees.AddNew();
			guarantee3.PW_BondNumber = "GUARANTEE789";

			AssertEquals("PW_BondFiledPort when guarantee reference is filled having more than one CUS permit rules", "", movementHeader.Guarantees[2].PW_BondFiledPort);
		}

		public void TestPW_BondFiledPortEmptyWhenCopyCustomsOfficeFromGuaranteeHeaderIsFalse()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "IT address";
			address.OA_OH = org.PK;

			var guaranteeHeader1 = CreateCusGuarantee("GUARANTEE123", org, EUGuaranteeTypeList.Codes.TRA, "1");
			CreateCusGuaranteeRule(guaranteeHeader1, EU.Business.PermitRuleCodeList.Codes.CUS, "AT9300");

			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var movementHeader = nctsHeader.MovementHeader;

			var guaranteeWithCopyTrue = movementHeader.Guarantees.AddNew();
			guaranteeWithCopyTrue.PW_BondNumber = "GUARANTEE123";

			var guaranteeWithCopyFalse = Factory.New<NctsGuaranteeForCopyCustomsOfficeFromGuaranteeHeaderFalseTest>();
			guaranteeWithCopyFalse.PW_ParentID = guaranteeWithCopyTrue.PW_ParentID;
			guaranteeWithCopyFalse.PW_ParentTableCode = guaranteeWithCopyTrue.PW_ParentTableCode;
			guaranteeWithCopyFalse.PW_BondNumber = "GUARANTEE123";

			AssertEquals("guaranteeWithCopyTrue", "AT9300", guaranteeWithCopyTrue.PW_BondFiledPort);
			AssertEquals("guaranteeWithCopyFalse", string.Empty, guaranteeWithCopyFalse.PW_BondFiledPort);
		}

		class NctsGuaranteeForCopyCustomsOfficeFromGuaranteeHeaderFalseTest : NctsGuarantee
		{
			public NctsGuaranteeForCopyCustomsOfficeFromGuaranteeHeaderFalseTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool CopyCustomsOfficeFromGuaranteeHeader => false;
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

		void CreateCusGuaranteeRule(CusGuaranteeHeader guaranteeHeader, ZString ruleCode, ZString valueFrom)
		{
			var permitRule = guaranteeHeader.CusGuaranteeRules.AddNew();
			permitRule.CPR_RuleCode = ruleCode;
			permitRule.CPR_ValueFrom = valueFrom;
		}

		NctsGuarantee CreateNctsGuarantee(string phase, string rule, string movementType)
		{
			NCTSTestHelper.SetUpTariff(Factory);

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AAA";
			var guaranteeHeader = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");
			var guaranteeRule = guaranteeHeader.CusGuaranteeRules.AddNew();
			guaranteeRule.CPR_RuleCode = "LAP";
			guaranteeRule.CPR_ValueFrom = rule;

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(movementType);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			nctsHeader.BH_ApplicationCode = phase;

			var guarantee = nctsHeader.IsPhase5Departure ? nctsHeader.MovementHeader.Guarantees.AddNew() : nctsHeader.Guarantees.AddNew();

			return guarantee;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();

			return guarantee;
		}

		protected override void SetUp()
		{
			base.SetUp();
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		}
	}
}
