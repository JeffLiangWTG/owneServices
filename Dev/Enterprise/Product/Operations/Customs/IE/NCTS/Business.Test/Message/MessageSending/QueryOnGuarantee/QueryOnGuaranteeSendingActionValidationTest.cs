using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class QueryOnGuaranteeSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckQueryIdentifier_R0261()
		{
			var helper = new EU.NCTS.Business.Testing.UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.NGUAQ, "NCTS Query Identifier");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: UniversalReferenceConstants.RefCusCodeListTypes.Codes.NGUAQ,
				code: "1",
				description: "1",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: UniversalReferenceConstants.RefCusCodeListTypes.Codes.NGUAQ,
				code: "2",
				description: "2",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: UniversalReferenceConstants.RefCusCodeListTypes.Codes.NGUAQ,
				code: "4",
				description: "4",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			Factory.Save();

			var errorMessage = "Query Identifier should be 1 or 4, when the Guarantee type is 2 or 4.";

			var guarantee = header.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondType = "1";
			var sendingAction = new QueryOnGuaranteeSendingAction(header);
			sendingAction.QueryIdentifier = "2";
			sendingAction.Validation.ValidateAll();
			AssertNoMessageErrors("Should pass the Validation with QueryIdentifier.", sendingAction.QueryIdentifierInfo);

			guarantee.PW_BondType = "2";
			sendingAction = new QueryOnGuaranteeSendingAction(header);
			sendingAction.QueryIdentifier = "2";
			sendingAction.Validation.ValidateAll();
			AssertNoMessageErrors("Should pass the Validation with QueryIdentifier.", sendingAction.QueryIdentifierInfo);
			sendingAction.AllGuarantees[0].ShouldSend = true;
			sendingAction.Validation.ValidateAll();
			AssertHasMessageError("Query Identifier should be 1 or 4, when PW_BondType is 2", sendingAction.QueryIdentifierInfo, errorMessage);

			guarantee.PW_BondType = "4";
			sendingAction = new QueryOnGuaranteeSendingAction(header);
			sendingAction.QueryIdentifier = "2";
			sendingAction.Validation.ValidateAll();
			AssertNoMessageErrors("Should pass the Validation with QueryIdentifier.", sendingAction.QueryIdentifierInfo);
			sendingAction.AllGuarantees[0].ShouldSend = true;
			sendingAction.Validation.ValidateAll();
			AssertHasMessageError("Query Identifier should be 1 or 4, when PW_BondType is 4", sendingAction.QueryIdentifierInfo, errorMessage);

			sendingAction.QueryIdentifier = "1";
			sendingAction.Validation.ValidateAll();
			AssertNoMessageErrors("Should pass the Validation with QueryIdentifier.", sendingAction.QueryIdentifierInfo);
			sendingAction.QueryIdentifier = "4";
			sendingAction.Validation.ValidateAll();
			AssertNoMessageErrors("Should pass the Validation with QueryIdentifier.", sendingAction.QueryIdentifierInfo);
		}

		public void TestCheckQueryIdentifier()
		{
			var helper = new EU.NCTS.Business.Testing.UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.NGUAQ, "NCTS Query Identifier");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: UniversalReferenceConstants.RefCusCodeListTypes.Codes.NGUAQ,
				code: "100",
				description: "Description 1",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);

			Factory.Save();

			var validation = queryOnGuaranteeSendingAction.Validation;
			var targetInfo = queryOnGuaranteeSendingAction.QueryIdentifierInfo;
			validation.ValidateAll();

			AssertHasMessageError("QueryIdentifier required", targetInfo, $"Please provide {targetInfo.HumanReadableName}.");

			queryOnGuaranteeSendingAction.QueryIdentifier = "100";
			validation.ValidateAll();
			AssertNoMessageErrors("Should pass the Validation with QueryIdentifier.", targetInfo);

			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "XXXXXXXX", "100");
		}

		public void TestCheckPeriodFrom()
		{
			var validation = queryOnGuaranteeSendingAction.Validation;
			var targetInfo = queryOnGuaranteeSendingAction.PeriodFromInfo;

			var error = "Please enter a valid date.";
			queryOnGuaranteeSendingAction.PeriodFrom = ZDate.Invalid;
			validation.ValidateAll();
			AssertHasMessageError("Date is invalid", targetInfo, error);

			queryOnGuaranteeSendingAction.PeriodFrom = ZDate.Empty;
			validation.ValidateAll();
			AssertNoMessageErrors("Date is valid", targetInfo);
		}

		public void TestCheckPeriodTo()
		{
			var validation = queryOnGuaranteeSendingAction.Validation;
			var targetInfo = queryOnGuaranteeSendingAction.PeriodToInfo;
			var periodFromInfo = queryOnGuaranteeSendingAction.PeriodFromInfo;

			var error = "Please enter a valid date.";
			queryOnGuaranteeSendingAction.PeriodFrom = ZDate.Today;
			queryOnGuaranteeSendingAction.PeriodTo = ZDate.Invalid;
			validation.ValidateAll();
			AssertHasMessageError("Date is invalid", targetInfo, error);

			queryOnGuaranteeSendingAction.PeriodFrom = ZDate.Empty;
			queryOnGuaranteeSendingAction.PeriodTo = ZDate.Empty;
			validation.ValidateAll();
			AssertNoMessageErrors("Should pass the Validation with both dates empty.", targetInfo);

			error = $"Both {periodFromInfo.HumanReadableName} date and {targetInfo.HumanReadableName} date must be entered or blank.";
			queryOnGuaranteeSendingAction.PeriodFrom = ZDate.Today;
			queryOnGuaranteeSendingAction.PeriodTo = ZDate.Empty;
			validation.ValidateAll();
			AssertHasMessageError("If Period From is populated but Period To is Empty then error", targetInfo, error);

			queryOnGuaranteeSendingAction.PeriodFrom = ZDate.Empty;
			queryOnGuaranteeSendingAction.PeriodTo = ZDate.Today;
			validation.ValidateAll();
			AssertHasMessageError("If Period To is populated but Period From is Empty then error", targetInfo, error);

			queryOnGuaranteeSendingAction.PeriodFrom = ZDate.Today;
			queryOnGuaranteeSendingAction.PeriodTo = ZDate.Today.AddDays(-1);
			validation.ValidateAll();
			error = $"{targetInfo.HumanReadableName} date must be after {periodFromInfo.HumanReadableName} date.";
			AssertHasMessageError("Period To must be after Period From", targetInfo, error);

			queryOnGuaranteeSendingAction.PeriodFrom = ZDate.Today;
			queryOnGuaranteeSendingAction.PeriodTo = ZDate.Today.AddDays(1);
			validation.ValidateAll();
			AssertNoMessageErrors("Should pass Validation.", targetInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			queryOnGuaranteeSendingAction = new QueryOnGuaranteeSendingAction(header);
		}
		NctsHeader header;
		QueryOnGuaranteeSendingAction queryOnGuaranteeSendingAction;
	}
}
