using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using AuthorizationCodes = Enterprise.Customs.Business.CusAuthorizationHeaderTypeList.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusAuthorizationUsagePhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAGC_Code_Calls_ValidateBM_ReducedDatasetIndicator()
		{
			const string errorMessage = "[R0350, R0352] An Authorization for Code=TRD is required when Reduced Data Set Indicator is Yes and Inland M.O.T. is one of these - 1(Sea Transport) or 2(Rail Transport) or 4(Air Transport).";

			var nctsHeader = CreateNctsHeader();

			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_ReducedDatasetIndicator = true;
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;

			var authorizationUsage = movementHeader.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset;

			CombineAssertions(() =>
			{
				AssertNoMessageError("Initial state without error", movementHeader.BM_ReducedDatasetIndicatorInfo, errorMessage);
				const string codeToRaiseError = Customs.Business.CusAuthorizationHeaderTypeList.Codes.SpecialSeals;
				authorizationUsage.AGC_Code = codeToRaiseError;
				AssertHasMessageError("Setter of AGC_Code triggers ValidateBM_ReducedDatasetIndicator", movementHeader.BM_ReducedDatasetIndicatorInfo, errorMessage);
			});
		}

		public void TestCheckAGC_Number_Mandatory()
		{
			const string validationErrorMessagePrefix = "[TR0005]";
			var nctsHeader = CreateNctsHeader();

			using (var deciderTestContext = new CusAuthorizationUsageValidationDeciderTestContext<ICusAuthorizationUsagePhase5ValidationDecider>(Factory))
			{
				deciderTestContext.EnableRule(c => c.IsRuleTR0005Active);
				var cusAuthorizationUsage = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();

				cusAuthorizationUsage.AGC_Number = string.Empty;
				AssertHasErrorContaining(cusAuthorizationUsage.AGC_NumberInfo, validationErrorMessagePrefix);

				deciderTestContext.DisableRule(c => c.IsRuleTR0005Active);
				cusAuthorizationUsage.Validation.ValidateAGC_Number();
				AssertNoErrorContaining(cusAuthorizationUsage.AGC_NumberInfo, validationErrorMessagePrefix);
				deciderTestContext.EnableRule(c => c.IsRuleTR0005Active);

				cusAuthorizationUsage.AGC_Number = "A";
				AssertNoErrorContaining(cusAuthorizationUsage.AGC_NumberInfo, validationErrorMessagePrefix);
			}
		}

		public void TestCheckAGC_Code_G0114Rule()
		{
			const string messageError = "[G0114] When Authorization ACR is provided, Simplified Procedure must be selected.";

			var nctsHeader = CreateNctsHeader();
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.IsSimplifiedNctsProcedure = false;
			var cusAuthorizationUsage = movementHeader.CusAuthorizationUsages.AddNew();

			using var deciderTestContext = new CusAuthorizationUsageValidationDeciderTestContext<ICusAuthorizationUsagePhase5ValidationDecider>(Factory);

			CombineAssertions("When Rule G0114 Is Active", () =>
			{
				deciderTestContext.EnableRule(rule => rule.IsRuleG0114Active);
				cusAuthorizationUsage.AGC_Code = AuthorizationCodes.AuthorizedConsignorTransit;
				AssertHasMessageError("And AGC_Code is ACR and IsSimplifiedNctsProcedure is false", cusAuthorizationUsage.AGC_CodeInfo, messageError);

				cusAuthorizationUsage.AGC_Code = AuthorizationCodes.SpecialSeals;
				AssertNoMessageError("And AGC_Code is SSE and IsSimplifiedNctsProcedure is false", cusAuthorizationUsage.AGC_CodeInfo, messageError);

				cusAuthorizationUsage.AGC_Code = AuthorizationCodes.TransitReducedDataset;
				AssertNoMessageError("And AGC_Code is TRD and IsSimplifiedNctsProcedure is false", cusAuthorizationUsage.AGC_CodeInfo, messageError);

				movementHeader.IsSimplifiedNctsProcedure = true;
				cusAuthorizationUsage = movementHeader.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = AuthorizationCodes.AuthorizedConsignorTransit;
				AssertNoMessageError("And AGC_Code is ACR and IsSimplifiedNctsProcedure is true", cusAuthorizationUsage.AGC_CodeInfo, messageError);
			});

			CombineAssertions("When Rule G0114 Is Disabled ", () =>
			{
				deciderTestContext.DisableRule(rule => rule.IsRuleG0114Active);

				movementHeader.IsSimplifiedNctsProcedure = false;
				cusAuthorizationUsage = movementHeader.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = AuthorizationCodes.AuthorizedConsignorTransit;

				AssertNoMessageError("And AGC_Code is ACR and IsSimplifiedNctsProcedure is false", cusAuthorizationUsage.AGC_CodeInfo, messageError);
			});
		}

		public void TestCheckAGC_Code_NR0063Rule()
		{
			var expectedMessageError = "[NR0063] When there is an authorization with type ACR, the type of location must be B. If there is no authorization with type ACR, the type of location must be A.";
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = header.MovementHeader;
			var goodsLocation = movementHeader.GoodsLocation;
			var authorization = movementHeader.CusAuthorizationUsages.AddNew();

			using (var ruleTestContext = new CusGoodsLocationValidationDeciderTestContext<IDeparturePhase5CusGoodsLocationValidationDecider>(Factory))
			{
				CombineAssertions(() =>
				{
					ruleTestContext.DisableRule(r => r.IsRuleNR0063Active);
					authorization.AGC_Code = AuthorizationCodes.AuthorizedConsignorTransit;
					goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
					AssertNoMessageError("Authorization code 'ACR' & GoodsLocation type 'A', no message error expected (rule disabled)", authorization.AGC_CodeInfo, expectedMessageError);

					ruleTestContext.EnableRule(r => r.IsRuleNR0063Active);
					authorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
					goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
					AssertHasMessageError("Authorization code 'ACR' & GoodsLocation type 'A', message error expected", authorization.AGC_CodeInfo, expectedMessageError);

					authorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
					goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
					AssertNoMessageError("Authorization code <> 'ACR' & GoodsLocation type 'A', no message error expected", authorization.AGC_CodeInfo, expectedMessageError);

					authorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
					goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
					AssertNoMessageError("Authorization code <> 'ACR' & GoodsLocation type 'B', no message error expected", authorization.AGC_CodeInfo, expectedMessageError);
				});
			}
		}

		NctsHeader CreateNctsHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader;
		}
	}
}
