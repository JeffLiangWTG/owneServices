using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class MessageSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckRuleNR0053() => CombineAssertions(() =>
		{
			var errorMessage = "[NR0053] When additional declaration type is A, the qualifier of the location must be U and its type must be C. When the additional declaration type is D, the qualifier must be V or U or blank and its type must be A or C or blank.";
			var movementHeader = nctsHeader.MovementHeader;
			var goodsLocation = movementHeader.GoodsLocation;

			var combinations = new List<(string entryType, string additionalDeclarationType, string locationQualifier, string type, bool errorExpected)>
			{
				("DEC", "A", "U", "C", false),
				("DEC", "D", "V", "A", false),
				("DEC", "D", "V", "C", false),
				("DEC", "D", "V", "", false),
				("DEC", "D", "U", "A", false),
				("DEC", "D", "U", "C", false),
				("DEC", "D", "U", "", false),
				("DEC", "D", "", "A", false),
				("DEC", "D", "", "C", false),
				("DEC", "D", "", "", false),
				("DEC", "A", "V", "C", true),
				("DEC", "A", "", "C", true),
				("DEC", "A", "U", "A", true),
				("DEC", "A", "U", "", true),
				("DEC", "D", "X", "", true),
				("DEC", "D", "", "X", true),

				("AMD", "A", "U", "C", false),
				("AMD", "D", "V", "A", false),
				("AMD", "D", "V", "C", false),
				("AMD", "D", "V", "", false),
				("AMD", "D", "U", "A", false),
				("AMD", "D", "U", "C", false),
				("AMD", "D", "U", "", false),
				("AMD", "D", "", "A", false),
				("AMD", "D", "", "C", false),
				("AMD", "D", "", "", false),
				("AMD", "A", "V", "C", true),
				("AMD", "A", "", "C", true),
				("AMD", "A", "U", "A", true),
				("AMD", "A", "U", "", true),
				("AMD", "D", "X", "", true),
				("AMD", "D", "", "X", true),

				("XXX", "A", "U", "C", false),
				("XXX", "D", "V", "A", false),
				("XXX", "D", "V", "C", false),
				("XXX", "D", "V", "", false),
				("XXX", "D", "U", "A", false),
				("XXX", "D", "U", "C", false),
				("XXX", "D", "U", "", false),
				("XXX", "D", "", "A", false),
				("XXX", "D", "", "C", false),
				("XXX", "D", "", "", false),
				("XXX", "A", "V", "C", false),
				("XXX", "A", "", "C", false),
				("XXX", "A", "U", "A", false),
				("XXX", "A", "U", "", false),
				("XXX", "D", "X", "", false),
				("XXX", "D", "", "X", false),
			};

			using (var testContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				testContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleNR0053Active));
				foreach (var combination in combinations)
				{
					movementHeader.BM_AdditionalDeclarationType = combination.additionalDeclarationType;
					goodsLocation.CGL_Qualifier = combination.locationQualifier;
					goodsLocation.CGL_Type = combination.type;
					messageSendingAction.EntryType = combination.entryType;
					AssertNoError($"Rule Inactive, Entry Type '{combination.entryType}', Additional Declaration Type '{combination.additionalDeclarationType}', Location Qualifier '{combination.locationQualifier}', Type '{combination.type}'", messageSendingAction.EntryTypeInfo, errorMessage);
				}
				testContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleNR0053Active));
				foreach (var combination in combinations)
				{
					movementHeader.BM_AdditionalDeclarationType = combination.additionalDeclarationType;
					goodsLocation.CGL_Qualifier = combination.locationQualifier;
					goodsLocation.CGL_Type = combination.type;
					messageSendingAction.EntryType = combination.entryType;
					if (combination.errorExpected)
					{
						AssertHasError($"Rule Active, Entry Type '{combination.entryType}', Additional Declaration Type '{combination.additionalDeclarationType}', Location Qualifier '{combination.locationQualifier}', Type '{combination.type}'", messageSendingAction.EntryTypeInfo, errorMessage);
					}
					else
					{
						AssertNoError($"Rule Active, Entry Type '{combination.entryType}', Additional Declaration Type '{combination.additionalDeclarationType}', Location Qualifier '{combination.locationQualifier}', Type '{combination.type}'", messageSendingAction.EntryTypeInfo, errorMessage);
					}
				}
			}
		});

		public void TestCheckRuleNR0054() => CombineAssertions(() =>
		{
			var errorMessage = "[NR0054] The qualifier of the location must be U and its type C OR the qualifier must be V and its type A when sending a presentation notification.";
			var movementHeader = nctsHeader.MovementHeader;
			var goodsLocation = movementHeader.GoodsLocation;
			var combinations = new List<(string entryType, string additionalDeclarationType, string locationQualifier, string type, bool errorExpected)>
			{
				("PRN", "D", "V", "A", false),
				("PRN", "D", "U", "C", false),
				("DEC", "D", "V", "A", false),
				("DEC", "D", "U", "C", false),
				("PRN", "D", "V", "C", true),
				("PRN", "D", "U", "A", true),
				("PRN", "D", "X", "A", true),
				("PRN", "A", "V", "A", true),
			};

			using (var testContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				testContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleNR0054Active));
				foreach (var combination in combinations)
				{
					movementHeader.BM_AdditionalDeclarationType = combination.additionalDeclarationType;
					goodsLocation.CGL_Qualifier = combination.locationQualifier;
					goodsLocation.CGL_Type = combination.type;
					messageSendingAction.EntryType = combination.entryType;
					AssertNoError($"Rule Inactive, Entry Type '{combination.entryType}', Additional Declaration Type '{combination.additionalDeclarationType}', Location Qualifier '{combination.locationQualifier}', Type '{combination.type}'", messageSendingAction.EntryTypeInfo, errorMessage);
				}
				testContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleNR0054Active));
				foreach (var combination in combinations)
				{
					movementHeader.BM_AdditionalDeclarationType = combination.additionalDeclarationType;
					goodsLocation.CGL_Qualifier = combination.locationQualifier;
					goodsLocation.CGL_Type = combination.type;
					messageSendingAction.EntryType = combination.entryType;
					if (combination.errorExpected)
					{
						AssertHasError($"Rule Active, Entry Type '{combination.entryType}', Additional Declaration Type '{combination.additionalDeclarationType}', Location Qualifier '{combination.locationQualifier}', Type '{combination.type}'", messageSendingAction.EntryTypeInfo, errorMessage);
					}
					else
					{
						AssertNoError($"Rule Active, Entry Type '{combination.entryType}', Additional Declaration Type '{combination.additionalDeclarationType}', Location Qualifier '{combination.locationQualifier}', Type '{combination.type}'", messageSendingAction.EntryTypeInfo, errorMessage);
					}
				}
			}
		});

		public void TestCheckRuleTR0021_EntryTypeRNM_ActualOfficeOfDestination()
		{
			messageSendingAction.EntryType = NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			CombineAssertions(() =>
			{
				messageSendingAction.QueryInformation = "entered";
				messageSendingAction.ActualOfficeOfDestination = ZString.Empty;
				AssertHasError("QueryInformation is filled, Consignee is empty, ActualOfficeOfDestination is empty", messageSendingAction.ActualOfficeOfDestinationInfo, NctsHeaderValidationHelper.TR0021ValidationMessage);

				messageSendingAction.QueryInformation = ZString.Empty;
				messageSendingAction.ActualOfficeOfDestination = ZString.Empty;
				AssertNoError("QueryInformation is empty, Consignee is empty, ActualOfficeOfDestination is empty", messageSendingAction.ActualOfficeOfDestinationInfo, NctsHeaderValidationHelper.TR0021ValidationMessage);

				messageSendingAction.QueryInformation = "entered";
				messageSendingAction.ActualConsignee.OrganisationPK = orgHeader.PK;
				messageSendingAction.ActualOfficeOfDestination = ZString.Empty;
				AssertNoError("QueryInformation is filled, Consignee is filled, ActualOfficeOfDestination is empty", messageSendingAction.ActualOfficeOfDestinationInfo, NctsHeaderValidationHelper.TR0021ValidationMessage);

				messageSendingAction.QueryInformation = "entered";
				messageSendingAction.ActualConsignee.OrganisationPK = ZGuid.Empty;
				messageSendingAction.ActualOfficeOfDestination = "entered";
				AssertNoError("QueryInformation is filled, Consignee is empty, ActualOfficeOfDestination is filled", messageSendingAction.ActualOfficeOfDestinationInfo, NctsHeaderValidationHelper.TR0021ValidationMessage);
			});
		}

		public void TestCheckQueryInformationRuleC0220_EntryTypeRNM()
		{
			const string message = "[C0220] If 'TC11 Delivery Date' is filled, then 'Query Information' must be filled.";

			messageSendingAction.EntryType = NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement;

			CombineAssertions(() =>
			{
				messageSendingAction.TCI11 = ZDateTime.BrettsBirthday;
				messageSendingAction.QueryInformation = ZString.Empty;
				AssertHasError("QueryInformation is empty, TCI11 is filled", messageSendingAction.QueryInformationInfo, message);

				messageSendingAction.QueryInformation = "entered";
				AssertNoError("QueryInformation is filled, TCI11 is filled", messageSendingAction.QueryInformationInfo, message);

				messageSendingAction.TCI11 = ZDateTime.Empty;
				messageSendingAction.QueryInformation = ZString.Empty;
				AssertNoError("QueryInformation is empty, TCI11 is empty", messageSendingAction.QueryInformationInfo, message);

				messageSendingAction.QueryInformation = "entered";
				AssertNoError("QueryInformation is filled, TCI11 is empty", messageSendingAction.QueryInformationInfo, message);
			});
		}

		public void TestCheckActualOfficeOfDestinationRuleC0315_EntryTypeRNM()
		{
			const string message = "[C0315] If 'TC11 Delivery date' is filled, then 'Actual Office of Destination' must be filled.";

			messageSendingAction.EntryType = NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement;

			CombineAssertions(() =>
			{
				messageSendingAction.TCI11 = ZDateTime.BrettsBirthday;
				messageSendingAction.ActualOfficeOfDestination = ZString.Empty;
				AssertHasError("ActualOfficeOfDestination is empty, TCI11 is filled", messageSendingAction.ActualOfficeOfDestinationInfo, message);

				messageSendingAction.ActualOfficeOfDestination = "entered";
				AssertNoError("ActualOfficeOfDestination is filled, TCI11 is filled", messageSendingAction.ActualOfficeOfDestinationInfo, message);

				messageSendingAction.TCI11 = ZDateTime.Empty;
				messageSendingAction.ActualOfficeOfDestination = ZString.Empty;
				AssertNoError("ActualOfficeOfDestination is empty, TCI11 is empty", messageSendingAction.ActualOfficeOfDestinationInfo, message);

				messageSendingAction.ActualOfficeOfDestination = "entered";
				AssertNoError("ActualOfficeOfDestination is filled, TCI11 is empty", messageSendingAction.ActualOfficeOfDestinationInfo, message);
			});
		}

		public void TestCheckActualOfficeOfDestination_MandatoryWhenTCI11IsEntered()
		{
			ValidationTestHelper.AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(messageSendingAction.ActualOfficeOfDestinationInfo, messageSendingAction.TCI11Info);
		}

		public void TestCheckQueryInformation_DestinationOrConsigneeMandatoryWhenQueryInformationIsEntered() => CombineAssertions(() =>
		{
			var errorMessage = "Actual Consignee or Actual Office of Destination must be filled if query information is filled.";

			messageSendingAction.Validation.ValidateAll();
			AssertNoError("QueryInformation empty, both empty", messageSendingAction.QueryInformationInfo, errorMessage);

			messageSendingAction.QueryInformation = "entered";
			AssertHasError("QueryInformation entered, both empty", messageSendingAction.QueryInformationInfo, errorMessage);

			messageSendingAction.ActualOfficeOfDestination = "entered";
			messageSendingAction.Validation.ValidateAll();
			AssertNoError("QueryInformation entered, Destination entered", messageSendingAction.QueryInformationInfo, errorMessage);

			messageSendingAction.Consignee = "entered";
			messageSendingAction.ActualOfficeOfDestination = ZString.Empty;
			messageSendingAction.Validation.ValidateAll();
			AssertNoError("QueryInformation entered, Consignee entered", messageSendingAction.QueryInformationInfo, errorMessage);
			
			AssertEquals("Number of notifications should be 2 (a message error and an error)", 2, messageSendingAction.Notifications.Count());
		});

		public void TestCheckEntryType()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertInvalidCodeMessageError(messageSendingAction.EntryTypeInfo, "XX", NctsMessageTypeList.Codes.Declaration);
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(messageSendingAction.EntryTypeInfo);
			});
		}

		[TestDate(2022, 02, 06, 13, 48, 00)]
		public void TestCheckPresentationDateTime_NotInThePast()
		{
			var error = "Presentation Date cannot be in the past.";
			var yesterday = new ZDateTime(2022, 02, 05, 10, 21, 00);
			var sameDayEarlierTime = new ZDateTime(2022, 02, 06, 13, 40, 00);
			var sameDayLaterTime = new ZDateTime(2022, 02, 06, 13, 50, 00);
			void SetDepartureMovementHeaderPresentationDateTime(bool enabled, ZDateTime value)
			{
				commonMovement.BM_CustomsStatus = enabled ? "DAC" : "XXX";
				commonMovement.BM_Phase = enabled ? "XXX" : "MAM";
				commonMovement.BM_ArrivalDate = value;
				messageSendingAction.Validation.ValidateAll();
			}

			CombineAssertions(() =>
			{
				SetDepartureMovementHeaderPresentationDateTime(true, ZDateTime.Empty);
				AssertNoError("PresentationDateTime is not filled in", messageSendingAction.PresentationDateTimeInfo, error);
				SetDepartureMovementHeaderPresentationDateTime(false, yesterday);
				AssertNoError("PresentationDateTime is not enabled", messageSendingAction.PresentationDateTimeInfo, error);
				SetDepartureMovementHeaderPresentationDateTime(false, sameDayLaterTime);
				AssertNoError("PresentationDateTime is not in the past", messageSendingAction.PresentationDateTimeInfo, error);
				SetDepartureMovementHeaderPresentationDateTime(true, yesterday);
				AssertHasError("PresentationDateTime is in the past, enabled and filled in.", messageSendingAction.PresentationDateTimeInfo, error);
				SetDepartureMovementHeaderPresentationDateTime(true, sameDayEarlierTime);
				AssertHasError("PresentationDateTime is in the past (same day earlier time), enabled and filled in.", messageSendingAction.PresentationDateTimeInfo, error);
			});
		}

		public void TestCheckLRN()
		{
			var expectedMessage = "Customer Reference (LRN) is mandatory. If 'Customer Reference' is not enabled, check if the logon company has an EORI number.";
			CombineAssertions(() =>
			{
				messageSendingAction.Validation.ValidateLRN();
				AssertHasError("Departure - No LRN", messageSendingAction.LRNInfo, expectedMessage);
				commonMovement.BM_PaperlessInbondNum = "123LRN";
				messageSendingAction.Validation.ValidateLRN();
				AssertNoErrorContaining("Departure - LRN filled", messageSendingAction.LRNInfo, expectedMessage);
			});
		}

		public void TestCheckMRN()
		{
			var expectedMessage = "MRN is mandatory.";
			CombineAssertions(() =>
			{
				messageSendingAction.Validation.ValidateMRN();
				AssertNoErrorContaining("Departure - no validation for MRN", messageSendingAction.MRNInfo, expectedMessage);

				CreateNctsHeader(NctsMovementType.Codes.Arrival);
				messageSendingAction.Validation.ValidateMRN();
				AssertHasErrorContaining("Arrival - No MRN", messageSendingAction.MRNInfo, expectedMessage);
				nctsHeader.ArrivalMrnFromUser = "123MRN";
				messageSendingAction.Validation.ValidateMRN();
				AssertNoErrorContaining("Arrival - MRN filled", messageSendingAction.MRNInfo, expectedMessage);
			});
		}

		public void TestCheckAvailableBalance()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			CombineAssertions(() =>
			{
				CreateGuarantee(601m, 600m, "0000001");
				var errorMessage = "The available guarantee balance is ";
				var info = messageSendingAction.EntryTypeInfo;
				messageSendingAction.ShouldSend = true;
				messageSendingAction.EntryType = NctsMessageTypeList.Codes.Declaration;
				AssertHasErrorContaining(info, errorMessage);

				messageSendingAction.EntryType = NctsMessageTypeList.Codes.Amendment;
				AssertHasErrorContaining(info, errorMessage);

				messageSendingAction.EntryType = NctsMessageTypeList.Codes.ArrivalNotification;
				AssertNoErrorContaining("Entry type is not DEC or AMD", info, errorMessage);
				messageSendingAction.ShouldSend = false;
				messageSendingAction.EntryType = NctsMessageTypeList.Codes.Declaration;
				AssertNoMessageErrorContaining(info, errorMessage);

				CreateNctsHeader(NctsMovementType.Codes.Departure);
				CreateGuarantee(1200m, 600m, "0000002");
				messageSendingAction.ShouldSend = true;
				messageSendingAction.EntryType = NctsMessageTypeList.Codes.Declaration;
				AssertNoMessageErrorContaining(info, errorMessage);
			});
		}

		public void TestCheckGoodsLocation()
		{
			var error = "You have not entered a complete Location of Goods. This is required when sending a Presentation message.";
			
			nctsHeader.BH_MessageStatus = "INV";
			commonMovement.BM_CustomsStatus = "170";
			commonMovement.BM_Phase = "PRN";
			messageSendingAction.EntryType = "INV";

			CombineAssertions(() =>
			{
				messageSendingAction.Validation.ValidateAll();
				AssertNoError("GoodsLocation not mandatory for this EntryType", messageSendingAction.EntryTypeInfo, error);

				messageSendingAction.EntryType = "PRN";
				messageSendingAction.Validation.ValidateAll();
				AssertHasError("GoodsLocation is not filled in", messageSendingAction.EntryTypeInfo, error);

				((NctsDepartureMovementHeader)commonMovement).GoodsLocation.CGL_Type = "C";
				((NctsDepartureMovementHeader)commonMovement).GoodsLocation.CGL_Qualifier = "U";

				messageSendingAction.Validation.ValidateAll();
				AssertHasError("GoodsLocation is not completly filled", messageSendingAction.EntryTypeInfo, error);

				((NctsDepartureMovementHeader)commonMovement).GoodsLocation.CGL_AdditionalIdentifier = "BEANR001";
				messageSendingAction.Validation.ValidateAll();
				AssertNoError("GoodsLocation is filled in", messageSendingAction.EntryTypeInfo, error);
			});
		}

		public void TestCheckRepresentativeCBRNumber()
		{
			var expectedMessage = "The representative of the declaration has no CBR number registered in the config tab of the organization (Master Data)";
			CombineAssertions(() =>
			{
				messageSendingAction.Validation.ValidateRepresentativeCBRNumber();
				AssertNoErrorContaining("Departure - no validation when there is no representative nor the entry type is filled", messageSendingAction.RepresentativeCBRNumberInfo, expectedMessage);

				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "REP", commonMovement.Representative, "1");
				messageSendingAction.Validation.ValidateRepresentativeCBRNumber();
				AssertHasErrorContaining("Departure - Validation should go off when there is a representative without CBR Number", messageSendingAction.RepresentativeCBRNumberInfo, expectedMessage);

				commonMovement.Representative.Organisation.SetCustomsCode(OrgCusCode.CodeTypes.BrokerageRegistration, RefCountry.LoadFromCountryCode(Factory, "BE"), "CBR123");
				messageSendingAction.Validation.ValidateRepresentativeCBRNumber();
				AssertNoErrorContaining("Departure - no Validation when there is a representative with CBR Number", messageSendingAction.RepresentativeCBRNumberInfo, expectedMessage);

				CreateNctsHeader(NctsMovementType.Codes.Arrival);
				messageSendingAction.Validation.ValidateRepresentativeCBRNumber();
				AssertNoErrorContaining("Arrival - no validation", messageSendingAction.RepresentativeCBRNumberInfo, expectedMessage);
			});
		}

		void CreateGuarantee(ZDecimal openingBalance, ZDecimal pendingBalance, ZString guaranteeReference)
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			var guaranteeHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader1.CPH_Number = guaranteeReference;
			guaranteeHeader1.CPH_OH_PermitHolder = org1.PK;

			var openingBalanceTransaction = guaranteeHeader1.CusGuaranteeLineTransactions.AddNew();
			openingBalanceTransaction.CPL_Reference = "Ref";
			openingBalanceTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			openingBalanceTransaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			openingBalanceTransaction.CPL_TranValue = openingBalance;

			guaranteeHeader1.CPH_Balance = openingBalance - pendingBalance;

			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = guaranteeReference;
			guarantee.PW_BondAmount = 10;

			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateNctsHeader(NctsMovementType.Codes.Departure);
		}

		void CreateNctsHeader(ZString movementType)
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(movementType);

			if (movementType == NctsMovementType.Codes.Departure)
			{
				commonMovement = nctsHeader.MovementHeader;
				commonMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			}

			if (movementType == NctsMovementType.Codes.Arrival)
			{
				commonMovement = nctsHeader.ArrivalMovementHeader;
				commonMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			}
			messageSendingAction = new MessageSendingAction(commonMovement);
		}

		NctsHeader nctsHeader;
		NctsCommonMovementHeader commonMovement;
		MessageSendingAction messageSendingAction;
	}
}
