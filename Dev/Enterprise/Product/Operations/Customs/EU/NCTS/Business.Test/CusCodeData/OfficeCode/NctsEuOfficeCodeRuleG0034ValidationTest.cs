using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes;
using CoreCountryCodes = Enterprise.Core.Constants.CountryCodes;
using CoreRefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using EUNctsTypeOfPreviousDocument = Enterprise.Customs.EU.NCTS.Business.NctsConstants.NctsTypeOfPreviousDocument;
using EUUniversalReferenceTestDataHelper = Enterprise.Customs.EU.NCTS.Business.Testing.UniversalReferenceTestDataHelper;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsEuOfficeCodeRuleG0034ValidationTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When customsOffice is null", () => new NctsEuOfficeCodeRuleG0034Validation(null));
			AssertExceptionThrown<ArgumentNullException>("When customsOffice Without Header", () => new NctsEuOfficeCodeRuleG0034Validation(Factory.New<NctsEuOfficeCode>()));
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			officeCode = nctsHeader.MovementHeader.CustomsOffices.AddNew();

			AssertNoExceptionThrown("When customsOffice With Header", () => new NctsEuOfficeCodeRuleG0034Validation(officeCode));
		}

		public void TestCheckCY_Data_WhenDeclarationTypeIsT2_WhenG0034Active()
		{
			var header = nctsHeader;
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var movementHeader = header.MovementHeader;

			var officeOfDestination = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			var cY_DataInfo = officeOfDestination.CY_DataInfo;
			using (var ruleTestContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(rule => rule.IsRuleG0034Active);

				using (TemporarilySetTransitionPeriod(isActive: false))
				{
					CombineAssertions("When NCTSTP is OFF andDeclaration Type is T2", () =>
					{
						movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
						officeOfDestination.CY_Data = $"{nonInCountryCode}275100";
						AssertHasMessageErrorContaining($"And Office Of Destination Not In Any", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = ZString.Empty;
						AssertNoMessageErrorContaining($"And Office Of Destination is Empty", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = $"{cl112CountryCode}275100";
						AssertNoMessageErrorContaining($"And Office Of Destination IN CL112", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = $"{notCL112AndNotInCl010CountryCode}275100";
						AssertHasMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in Cl010", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = notCL112AndNotInCL172CustomsOffice;
						AssertHasMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in CL172", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = notCL112AndNotInCL294CustomsOffice;
						AssertHasMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in CL294", cY_DataInfo, ErrorMessageG0034);
					});

					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
					officeOfDestination.CY_Data = $"{nonInCountryCode}275100";
					AssertNoMessageErrorContaining($"When NCTSTP is OFF and Declaration Type is Not T2 And OfficeOfDestination Not In Any", cY_DataInfo, ErrorMessageG0034);
				}

				using (TemporarilySetTransitionPeriod(isActive: true))
				{
					CombineAssertions("When NCTSTP is ON and Declaration Type is T2", () =>
					{
						movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
						officeOfDestination.CY_Data = $"{nonInCountryCode}275100";
						AssertHasMessageErrorContaining($"And Office Of Destination Not In Any", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = ZString.Empty;
						AssertNoMessageErrorContaining($"And Office Of Destination is Empty", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = $"{cl112CountryCode}275100";
						AssertNoMessageErrorContaining($"And Office Of Destination IN CL112 ", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = $"{notCL112AndNotInCl010CountryCode}275100";
						AssertHasMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in Cl010", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = notCL112AndNotInCL172CustomsOffice;
						AssertHasMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in CL172", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = notCL112AndNotInCL294CustomsOffice;
						AssertHasMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in CL294", cY_DataInfo, ErrorMessageG0034);
					});

					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
					officeOfDestination.CY_Data = $"{nonInCountryCode}275100";
					AssertNoMessageErrorContaining($"When NCTSTP is ON and Declaration Type is not T2 And OfficeOfDestination Not In Any", cY_DataInfo, ErrorMessageG0034);
				}
			}
		}

		public void TestCheckCY_Data_WhenDeclarationTypeIsT2_WhenG0034Inactive()
		{
			var header = nctsHeader;
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var movementHeader = header.MovementHeader;

			using (TemporarilySetTransitionPeriod(isActive: false))
			{
				CombineAssertions("When NCTSTP is OFF andDeclaration Type is T2", () =>
				{
					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
					CheckDefaultCasesWhenhInactiveRule(header);
				});
			}

			using (TemporarilySetTransitionPeriod(isActive: true))
			{
				CombineAssertions("When NCTSTP is ON and Declaration Type is T2", () =>
				{
					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;

					CheckDefaultCasesWhenhInactiveRule(header);
				});
			}
		}

		public void TestCheckCY_Data_WhenCommonPreviousDocumentIsInHouseConsignment_WhenG0034Active()
		{
			var header = nctsHeader;
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var nctsBill = header.Bills.AddNew();
			var houseConsignmentPreviousDocument = nctsBill.PreviousDocuments.AddNew();
			var movementHeader = header.MovementHeader;

			var officeOfDestination = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			var cY_DataInfo = officeOfDestination.CY_DataInfo;
			using (var ruleTestContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(rule => rule.IsRuleG0034Active);

				using (TemporarilySetTransitionPeriod(isActive: false))
				{
					CombineAssertions("When Transition period is OFF And HouseConsignment > PreviousDocument > Type = N830", () =>
					{
						houseConsignmentPreviousDocument.CSI_Code = EUNctsTypeOfPreviousDocument.Codes.N830;
						officeOfDestination.CY_Data = $"{nonInCountryCode}275100";
						AssertHasMessageErrorContaining($"And Office Of Destination Not In Any", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = ZString.Empty;
						AssertNoMessageErrorContaining($"And Office Of Destination is empty", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = $"{cl112CountryCode}275100";
						AssertNoMessageErrorContaining($"And Office Of Destination IN CL112 ", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = $"{notCL112AndNotInCl010CountryCode}275100";
						AssertHasMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in Cl010", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = notCL112AndNotInCL172CustomsOffice;
						AssertHasMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in CL172", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = notCL112AndNotInCL294CustomsOffice;
						AssertHasMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in CL294", cY_DataInfo, ErrorMessageG0034);
					});

					houseConsignmentPreviousDocument.CSI_Code = "N235";
					officeOfDestination.CY_Data = $"{nonInCountryCode}275100";
					AssertNoMessageErrorContaining($"When Transition period is OFF And HouseConsignment > PreviousDocument > Type <> N830 And Office Of Destination Not In Any", cY_DataInfo, ErrorMessageG0034);
				}

				using (TemporarilySetTransitionPeriod(isActive: true))
				{
					CombineAssertions("When Transition period is ON And HouseConsignment > PreviousDocument > Type = N830", () =>
					{
						houseConsignmentPreviousDocument.CSI_Code = EUNctsTypeOfPreviousDocument.Codes.N830;
						officeOfDestination.CY_Data = $"{nonInCountryCode}275100";
						AssertNoMessageErrorContaining($"And Office Of Destination Not In Any", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = $"{notCL112AndNotInCl010CountryCode}275100";
						AssertNoMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in Cl010", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = notCL112AndNotInCL172CustomsOffice;
						AssertNoMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in CL172", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = notCL112AndNotInCL294CustomsOffice;
						AssertNoMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in CL294", cY_DataInfo, ErrorMessageG0034);
					});
				}
			}
		}

		public void TestCheckCY_Data_WhenCommonPreviousDocumentIsInHouseConsignment_WhenG0034Inactive()
		{
			var header = nctsHeader;
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var nctsBill = header.Bills.AddNew();
			var houseConsignmentPreviousDocument = nctsBill.PreviousDocuments.AddNew();
			var movementHeader = header.MovementHeader;

			using (TemporarilySetTransitionPeriod(isActive: false))
			{
				CombineAssertions("When Transition period is OFF And HouseConsignment > PreviousDocument > Type = N830", () =>
				{
					CheckDefaultCasesWhenhInactiveRule(header);
				});
			}
		}

		public void TestCheckCY_Data_WhenPreviousDocumentIsInGoodsItem_WhenG0034Active()
		{
			var header = nctsHeader;
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var nctsBill = header.Bills.AddNew();
			var goodsItem = nctsBill.GoodsItems.AddNew();
			var goodsItemPreviousDocument = goodsItem.PreviousDocuments.AddNew();

			var officeOfDestination = header.MovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			var cY_DataInfo = officeOfDestination.CY_DataInfo;
			using (var ruleTestContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(rule => rule.IsRuleG0034Active);

				using (TemporarilySetTransitionPeriod(isActive: true))
				{
					CombineAssertions("When Transition period is ON And GoodsItem > PreviousDocument > Type = N830", () =>
					{
						goodsItemPreviousDocument.CSI_Code = EUNctsTypeOfPreviousDocument.Codes.N830;
						officeOfDestination.CY_Data = $"{nonInCountryCode}275100";
						AssertHasMessageErrorContaining($"And Office Of Destination Not In Any", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = ZString.Empty;
						AssertNoMessageErrorContaining($"And Office Of Destination is Empty", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = $"{cl112CountryCode}275100";
						AssertNoMessageErrorContaining($"And Office Of Destination IN CL112 ", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = $"{notCL112AndNotInCl010CountryCode}275100";
						AssertHasMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in Cl010", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = notCL112AndNotInCL172CustomsOffice;
						AssertHasMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in CL172", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = notCL112AndNotInCL294CustomsOffice;
						AssertHasMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in CL294", cY_DataInfo, ErrorMessageG0034);
					});

					goodsItemPreviousDocument.CSI_Code = "N235";
					officeOfDestination.CY_Data = $"{nonInCountryCode}275100";
					AssertNoMessageErrorContaining($"When Transition period is ON And GoodsItem > PreviousDocument > Type <> N830 And Office Of Destination Not In Any", cY_DataInfo, ErrorMessageG0034);
				}

				using (TemporarilySetTransitionPeriod(isActive: false))
				{
					CombineAssertions("When Transition period is OFF And GoodsItem > PreviousDocument > Type = N830", () =>
					{
						goodsItemPreviousDocument.CSI_Code = EUNctsTypeOfPreviousDocument.Codes.N830;
						officeOfDestination.CY_Data = $"{nonInCountryCode}275100";
						AssertNoMessageErrorContaining($"And Office Of Destination Not In Any", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = $"{notCL112AndNotInCl010CountryCode}275100";
						AssertNoMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in Cl010", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = notCL112AndNotInCL172CustomsOffice;
						AssertNoMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in CL172", cY_DataInfo, ErrorMessageG0034);

						officeOfDestination.CY_Data = notCL112AndNotInCL294CustomsOffice;
						AssertNoMessageErrorContaining($"And Office Of Destination Not in CL112 And Not in CL294", cY_DataInfo, ErrorMessageG0034);
					});
				}
			}
		}

		public void TestCheckCY_Data_WhenPreviousDocumentIsInGoodsItem_WhenG0034Inactive()
		{
			var header = nctsHeader;
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var nctsBill = header.Bills.AddNew();
			var goodsItem = nctsBill.GoodsItems.AddNew();
			var goodsItemPreviousDocument = goodsItem.PreviousDocuments.AddNew();

			using (TemporarilySetTransitionPeriod(isActive: true))
			{
				CombineAssertions("When Transition period is ON And GoodsItem > PreviousDocument > Type = N830", () =>
				{
					CheckDefaultCasesWhenhInactiveRule(header);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			officeCode = nctsHeader.MovementHeader.CustomsOffices.AddNew();

			(nonInCountryCode, cl112CountryCode, notCL112AndNotInCl010CountryCode, notCL112AndNotInCL172CustomsOffice, notCL112AndNotInCL294CustomsOffice) = SetupG0034CountryCodes();
		}

		(string nonInCountryCode, string cl112CountryCode, string notCL112AndNotInCl010CountryCode, string notCL112AndNotInCL172CustomsOffice, string notCL112AndNotInCL294CustomsOffice) SetupG0034CountryCodes()
		{
			const string codeOfficeOfExitCL294 = "IESNN400";
			const string codeOfficeOfDestinationCL172 = "DE004204";
			var factory = Factory;
			EUUniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(factory, Code_CL112, (CoreCountryCodes.Switzerland, "Switzerland"), (CoreCountryCodes.Norway, "Norway"), (CoreCountryCodes.Serbia, "Serbia"));
			EUUniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(factory, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, (CoreCountryCodes.Estonia, "Estonia"));

			var helper = new EUUniversalReferenceTestDataHelper(factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(CoreCountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(CoreRefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var codeListOfficeOfExitCL294 = helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: CoreCountryCodes.Ireland,
				codeType: CoreRefCusCodeListTypes.Codes.CustomsOffice,
				code: codeOfficeOfExitCL294,
				description: "Shannon Airport",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateCusCodeListAttribute(codeListOfficeOfExitCL294.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);

			helper.CreateNewOrGetExistingDataGrouping(CoreCountryCodes.Germany, parent: eun);
			var codeListCL172 = helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: CoreCountryCodes.Germany,
				codeType: CoreRefCusCodeListTypes.Codes.CustomsOffice,
				code: codeOfficeOfDestinationCL172,
				description: "Laufenburg",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateCusCodeListAttribute(codeListCL172.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfDestination);

			Factory.Save();
			return (nonInCountryCode: CoreCountryCodes.Iran, cl112CountryCode: CoreCountryCodes.Switzerland, notCL112AndNotInCl010CountryCode: CoreCountryCodes.Germany, notCL112AndNotInCL172CustomsOffice: codeOfficeOfExitCL294, notCL112AndNotInCL294CustomsOffice: codeOfficeOfDestinationCL172);
		}

		IDisposable TemporarilySetTransitionPeriod(bool isActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);

		void CheckDefaultCasesWhenhInactiveRule(NctsHeader header)
		{
			var cyDataValues = new string[]
			{
				$"{nonInCountryCode}275100",
				$"{notCL112AndNotInCl010CountryCode}275100",
				notCL112AndNotInCL172CustomsOffice,
				notCL112AndNotInCL294CustomsOffice,
			};

			using (var ruleTestContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				var officeOfDestination = header.MovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
				var cY_DataInfo = officeOfDestination.CY_DataInfo;

				ruleTestContext.DisableRule(rule => rule.IsRuleG0034Active);

				foreach (var cyDataValue in cyDataValues)
				{
					officeOfDestination.CY_Data = cyDataValue;
					Assertion.AssertCollectionNotContains($"Expected notifications would not contain {ErrorMessageG0034}", cY_DataInfo.Notifications.Select(e => e.Message), x => x.Contains(ErrorMessageG0034));

					ruleTestContext.AssertRuleChecked(v => v.IsRuleG0034Active);
				}
			}
		}

		const string ErrorMessageG0034 = "[G0034] Office of Destination is not appropriate";

		NctsEuOfficeCode officeCode;

		NctsHeader nctsHeader;

		string nonInCountryCode;

		string cl112CountryCode;

		string notCL112AndNotInCl010CountryCode;

		string notCL112AndNotInCL172CustomsOffice;

		string notCL112AndNotInCL294CustomsOffice;
	}
}
