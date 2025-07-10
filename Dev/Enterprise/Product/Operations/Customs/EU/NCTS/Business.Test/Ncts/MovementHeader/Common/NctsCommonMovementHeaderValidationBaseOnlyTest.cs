using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsCommonMovementHeaderValidationBaseOnlyTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBM_PlaceOfUnloading_ListValidation()
		{
			var movementHeaderForTest = CreateTestMovementHeader();
			movementHeaderForTest.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;

			CombineAssertions(() =>
			{
				movementHeaderForTest.ShouldListValidatePlaceOfUnloading = true;
				movementHeaderForTest.BM_PlaceOfUnloading = "XYZ";
				AssertHasMessageErrorContaining("When ShouldListValidatePlaceOfUnloading", movementHeaderForTest.BM_PlaceOfUnloadingInfo, ListValidation.InvalidCodeMessageError.ToString());

				movementHeaderForTest.ShouldListValidatePlaceOfUnloading = false;
				movementHeaderForTest.BM_PlaceOfUnloading = "XYZ";
				AssertNoMessageErrorContaining("When !ShouldListValidatePlaceOfUnloading", movementHeaderForTest.BM_PlaceOfUnloadingInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		NctsCommonMovementHeaderForTest CreateTestMovementHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var movementHeaderForTest = NctsCommonMovementHeader.LoadOrCreate<NctsCommonMovementHeaderForTest>(header, Common.EU.NctsMoveHeaderType.Codes.Departure);
			return movementHeaderForTest;
		}

		public void TestCheckBM_PlaceOfUnloading_RuleC0191()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var movementHeaderForTest = CreateTestMovementHeader();
			const string errorMessage = "[C0191] You have not entered a Place of Unloading.";
			using var ruleTestContext = new MovementHeaderValidationDeciderTestContext<INctsMovementHeaderValidationDecider>(Factory);
			ruleTestContext.EnableRule(c => c.IsRuleC0191Active);
			movementHeaderForTest.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, "EUN", ZDate.Today, false))
			{
				movementHeaderForTest.BM_PlaceOfUnloading = "XYZ";
				AssertNoMessageErrorContaining("When security NON and BM_PlaceOfUnloading !Mandatory and not empty", movementHeaderForTest.BM_PlaceOfUnloadingInfo, errorMessage);

				movementHeaderForTest.BM_PlaceOfUnloading = "";
				AssertNoMessageErrorContaining("When security NON and BM_PlaceOfUnloading !Mandatory and empty", movementHeaderForTest.BM_PlaceOfUnloadingInfo, errorMessage);

				movementHeaderForTest.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				movementHeaderForTest.BM_PlaceOfUnloading = "XYZ";
				AssertNoMessageErrorContaining("When security !NON and BM_PlaceOfUnloading Mandatory and not empty", movementHeaderForTest.BM_PlaceOfUnloadingInfo, errorMessage);

				movementHeaderForTest.BM_PlaceOfUnloading = "";
				AssertHasMessageErrorContaining("When security !NON and BM_PlaceOfUnloading Mandatory and empty", movementHeaderForTest.BM_PlaceOfUnloadingInfo, errorMessage);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, "EUN", ZDate.Today, true))
			{
				movementHeaderForTest.Validation.ValidateBM_PlaceOfUnloading();
				AssertNoMessageErrorContaining("In Phase5 Transition Period.", movementHeaderForTest.BM_PlaceOfUnloadingInfo, errorMessage);
			}

			ruleTestContext.DisableRule(c => c.IsRuleC0191Active);
			movementHeaderForTest.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			movementHeaderForTest.BM_PlaceOfUnloading = "";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, "EUN", ZDate.Today, false))
			{
				movementHeaderForTest.Validation.ValidateBM_PlaceOfUnloading();
				AssertNoMessageErrorContaining("When security !NON and BM_PlaceOfUnloading Mandatory and empty", movementHeaderForTest.BM_PlaceOfUnloadingInfo, errorMessage);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, "EUN", ZDate.Today, true))
			{
				movementHeaderForTest.Validation.ValidateBM_PlaceOfUnloading();
				AssertNoMessageErrorContaining("In Phase5 Transition Period.", movementHeaderForTest.BM_PlaceOfUnloadingInfo, errorMessage);
			}
		}

		public void TestCheckBM_PlaceOfUnloading_RuleB1858_PlaceOfUnloadingCannotBeEntered()
		{
			const string errorMessage = "[B1858] Place of Unloading cannot be entered if Security = 'NON'.";
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, "EUN", ZDate.Today, true))
			{
				using var ruleTestContext = new MovementHeaderValidationDeciderTestContext<INctsMovementHeaderValidationDecider>(Factory);
				ruleTestContext.EnableRule(c => c.IsRuleB1858Active);
				AssertCheckBM_PlaceOfUnloading_RuleB1858_PlaceOfUnloadingCannotBeEntered(true, errorMessage, NotificationTypes.MessageError);
				ruleTestContext.DisableRule(c => c.IsRuleB1858Active);
				AssertCheckBM_PlaceOfUnloading_RuleB1858_PlaceOfUnloadingCannotBeEntered(false, errorMessage, NotificationTypes.MessageError);
			}
		}

		public void TestCheckBM_PlaceOfUnloading_RuleB1858_2_PlaceOfUnloadingCannotBeEntered()
		{
			const string warningMessage = "[B1858-2] Place of Unloading is not transmitted to customs if Security = 'NON'.";
			using var ruleTestContext = new MovementHeaderValidationDeciderTestContext<INctsMovementHeaderValidationDecider>(Factory, typeof(IRuleB1858_2Decider));
			ruleTestContext.DisableRule(c => c.IsRuleB1858Active);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, "EUN", ZDate.Today, true))
			{
				ruleTestContext.EnableRuleDecider<IRuleB1858_2Decider>(c => c.IsActive);
				AssertCheckBM_PlaceOfUnloading_RuleB1858_PlaceOfUnloadingCannotBeEntered(true, warningMessage, NotificationTypes.Warning);
				ruleTestContext.DisableRuleDecider<IRuleB1858_2Decider>(c => c.IsActive);
				AssertCheckBM_PlaceOfUnloading_RuleB1858_PlaceOfUnloadingCannotBeEntered(false, warningMessage, NotificationTypes.Warning);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, "EUN", ZDate.Today, false))
			{
				ruleTestContext.EnableRuleDecider<IRuleB1858_2Decider>(c => c.IsActive);
				AssertCheckBM_PlaceOfUnloading_RuleB1858_PlaceOfUnloadingCannotBeEntered(false, warningMessage, NotificationTypes.Warning);
				ruleTestContext.DisableRuleDecider<IRuleB1858_2Decider>(c => c.IsActive);
				AssertCheckBM_PlaceOfUnloading_RuleB1858_PlaceOfUnloadingCannotBeEntered(false, warningMessage, NotificationTypes.Warning);
			}
		}

		void AssertCheckBM_PlaceOfUnloading_RuleB1858_PlaceOfUnloadingCannotBeEntered(bool isRuleActive, string errorMessage, NotificationTypes notificationType)
		{
			var movementHeaderForTest = CreateTestMovementHeader();

			const string testPortCode = "ESBCN";
			const string testPortName = "Barcelona";

			var info = movementHeaderForTest.BM_PlaceOfUnloadingInfo;

			if (isRuleActive)
			{
				movementHeaderForTest.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				movementHeaderForTest.BM_ForeignDestPortKCode = testPortCode;
				movementHeaderForTest.BM_PlaceOfUnloading = testPortName;
				AssertNoNotification("Security is not NON.");

				movementHeaderForTest.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				movementHeaderForTest.BM_ForeignDestPortKCode = ZString.Empty;
				movementHeaderForTest.BM_PlaceOfUnloading = ZString.Empty;
				AssertNoNotification("Place of Unloading code and Place Of Unloading name are empty.");

				movementHeaderForTest.BM_ForeignDestPortKCode = testPortCode;
				movementHeaderForTest.Validation.ValidateBM_PlaceOfUnloading();
				AssertHasNotification("Security is NON and Place Of Unloading code is defined.");

				movementHeaderForTest.BM_ForeignDestPortKCode = ZString.Empty;
				movementHeaderForTest.BM_PlaceOfUnloading = testPortName;
				AssertHasNotification("Security is NON and Place Of Unloading name is defined.");
			}
			else
			{
				movementHeaderForTest.Validation.ValidateBM_PlaceOfUnloading();
				AssertNoNotification("The rule is inactive.");
			}

			void AssertHasNotification(string assertionMessage)
			{
				if (notificationType == NotificationTypes.Warning)
				{
					AssertHasWarningContaining(assertionMessage, info, errorMessage);
				}
				else
				{
					AssertHasMessageErrorContaining(assertionMessage, info, errorMessage);
				}
			}

			void AssertNoNotification(string assertionMessage)
			{
				if (notificationType == NotificationTypes.Warning)
				{
					AssertNoWarningContaining(assertionMessage, info, errorMessage);
				}
				else
				{
					AssertNoMessageErrorContaining(assertionMessage, info, errorMessage);
				}
			}
		}

		public void TestCheckBM_PlaceOfUnloading_RuleB1858_PlaceOfUnloadingIsRequired()
		{
			const string errorMessage = "[B1858] Place of Unloading is required when Circumstance is declared as other than XXX.";
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, "EUN", ZDate.Today, true))
			{
				using var ruleTestContext = new MovementHeaderValidationDeciderTestContext<INctsMovementHeaderValidationDecider>(Factory);
				ruleTestContext.EnableRule(c => c.IsRuleB1858Active);
				AssertCheckBM_PlaceOfUnloading_RuleB1858(true, errorMessage);
				ruleTestContext.DisableRule(c => c.IsRuleB1858Active);
				AssertCheckBM_PlaceOfUnloading_RuleB1858(false, errorMessage);
			}
		}

		public void TestCheckBM_PlaceOfUnloading_RuleB1858_2_PlaceOfUnloadingIsRequired() => CombineAssertions(() =>
		{
			const string errorMessage = "[B1858-2] Place of Unloading is required when Circumstance is declared as other than XXX.";
			using var ruleTestContext = new MovementHeaderValidationDeciderTestContext<INctsMovementHeaderValidationDecider>(Factory, typeof(IRuleB1858_2Decider));
			ruleTestContext.DisableRule(c => c.IsRuleB1858Active);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, "EUN", ZDate.Today, true))
			{
				ruleTestContext.EnableRuleDecider<IRuleB1858_2Decider>(c => c.IsActive);
				AssertCheckBM_PlaceOfUnloading_RuleB1858(true, errorMessage);
				ruleTestContext.DisableRuleDecider<IRuleB1858_2Decider>(c => c.IsActive);
				AssertCheckBM_PlaceOfUnloading_RuleB1858(false, errorMessage);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, "EUN", ZDate.Today, false))
			{
				ruleTestContext.EnableRuleDecider<IRuleB1858_2Decider>(c => c.IsActive);
				AssertCheckBM_PlaceOfUnloading_RuleB1858(false, errorMessage);
				ruleTestContext.DisableRuleDecider<IRuleB1858_2Decider>(c => c.IsActive);
				AssertCheckBM_PlaceOfUnloading_RuleB1858(false, errorMessage);
			}
		});

		void AssertCheckBM_PlaceOfUnloading_RuleB1858(bool isRuleActive, string errorMessage)
		{
			var movementHeaderForTest = CreateTestMovementHeader();

			const string testPortCode = "ES";
			const string testPortCode_UNLOCODE = "ESBCN";
			const string testPortName = "Barcelona";

			var info = movementHeaderForTest.BM_PlaceOfUnloadingInfo;
			var assertionMessage = $"isRuleActive={isRuleActive} NC5TP={movementHeaderForTest.IsInPhase5TransitionPeriod}";

			if (isRuleActive)
			{
				movementHeaderForTest.BM_SpecificCircumstance = NctsSpecificCircumstanceIndicatorList.Codes.A20;
				movementHeaderForTest.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				movementHeaderForTest.BM_ForeignDestPortKCode = ZString.Empty;
				movementHeaderForTest.BM_PlaceOfUnloading = ZString.Empty;
				AssertNoMessageErrorContaining($"{assertionMessage}: Security is NON.", info, errorMessage);

				movementHeaderForTest.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				movementHeaderForTest.BM_ForeignDestPortKCode = testPortCode;
				movementHeaderForTest.BM_PlaceOfUnloading = testPortName;
				AssertNoMessageErrorContaining($"{assertionMessage}: Place Of Unloading code and Place Of Unloading name are not empty.", info, errorMessage);

				movementHeaderForTest.BM_SpecificCircumstance = NctsSpecificCircumstanceIndicatorList.Codes.XXX;
				movementHeaderForTest.BM_ForeignDestPortKCode = ZString.Empty;
				movementHeaderForTest.BM_PlaceOfUnloading = ZString.Empty;
				AssertNoMessageErrorContaining($"{assertionMessage}: Circumstance equals to XXX.", info, errorMessage);

				movementHeaderForTest.BM_SpecificCircumstance = NctsSpecificCircumstanceIndicatorList.Codes.A20;
				movementHeaderForTest.Validation.ValidateBM_PlaceOfUnloading();
				AssertHasMessageErrorContaining($"{assertionMessage}: Security is not NON and Circumstance != XXX and Place Of Unloading code and name are empty.", info, errorMessage);

				movementHeaderForTest.BM_PlaceOfUnloading = testPortName;
				AssertHasMessageErrorContaining($"{assertionMessage}: Security is not NON and Circumstance != XXX and Place Of Unloading code is empty.", info, errorMessage);

				movementHeaderForTest.BM_PlaceOfUnloading = ZString.Empty;
				AssertHasMessageErrorContaining($"{assertionMessage}: Security is not NON and Circumstance != XXX and Place Of Unloading name is empty.", info, errorMessage);

				movementHeaderForTest.BM_ForeignDestPortKCode = testPortCode_UNLOCODE;
				movementHeaderForTest.Validation.ValidateBM_PlaceOfUnloading();
				AssertNoMessageErrorContaining($"{assertionMessage}: Security is not NON and Circumstance != XXX and Place Of Unloading name is empty.", info, errorMessage);
			}
			else
			{
				movementHeaderForTest.Validation.ValidateBM_PlaceOfUnloading();
				AssertNoMessageErrorContaining($"{assertionMessage}: Rule is inactive.", movementHeaderForTest.BM_PlaceOfUnloadingInfo, errorMessage);
			}
		}

		public void TestCheckBM_InBondEntryType_IsInBondEntryTypeListValidationActive()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList();
			Factory.Save();
			var movement = CreateTestMovementHeader();
			CombineAssertions(() =>
			{
				using (var ruleTestContext = new MovementHeaderValidationDeciderTestContext<INctsMovementHeaderValidationDecider>(Factory))
				{
					ruleTestContext.EnableRule(g => g.IsInBondEntryTypeListValidationActive);
					ValidationTestHelper.AssertInvalidCodeMessageError(movement.BM_InBondEntryTypeInfo, "XX", NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure);

					ruleTestContext.DisableRule(g => g.IsInBondEntryTypeListValidationActive);
					movement.BM_InBondEntryType = "XY";
					AssertNoMessageError("Adding an invalid code does not invoke the list validation message error", movement.BM_InBondEntryTypeInfo, ListValidation.InvalidCodeMessageError);
				}
			});
		}

		public void TestICusGoodsLocationTypeSupporter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				AssertEquals("Enterprise.Customs.DE.NCTS.Business.CusGoodsLocation", (nctsHeader.MovementHeader as ICusGoodsLocationTypeSupporter).GoodsLocationType.FullName);
			}
		}

		#region Testing Classes

		class NctsCommonMovementHeaderForTest : NctsCommonMovementHeader
		{
			public NctsCommonMovementHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override Type CusInBondCargoDescTypeCore => null;

			public bool ShouldListValidatePlaceOfUnloading { get; set; }

			protected override CusInBondMoveHeaderValidation GetNewValidation() => new NctsCommonMovementHeaderValidationForTest(this);

			protected override CusInBondMoveHeaderLookups GetNewLookups() => new NctsCommonMovementHeaderLookupsForTest(this);
		}

		class NctsCommonMovementHeaderLookupsForTest : NctsCommonMovementHeaderLookups
		{
			public NctsCommonMovementHeaderLookupsForTest(NctsCommonMovementHeader parent) : base(parent)
			{
			}
		}

		class NctsCommonMovementHeaderValidationForTest : NctsCommonMovementHeaderValidation
		{
			public NctsCommonMovementHeaderValidationForTest(NctsCommonMovementHeader parent) : base(parent)
			{
			}

			NctsCommonMovementHeaderForTest ParentForTest => (NctsCommonMovementHeaderForTest)Parent;

			protected override bool ShouldListValidatePlaceOfUnloading => ParentForTest.ShouldListValidatePlaceOfUnloading;
		}

		#endregion
	}
}
