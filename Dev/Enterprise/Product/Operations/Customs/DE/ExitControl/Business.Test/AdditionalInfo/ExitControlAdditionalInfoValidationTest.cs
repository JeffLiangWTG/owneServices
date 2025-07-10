using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	public class ExitControlAdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code_ListValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euGrouping);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AI44X, "AI44X DESC");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AI44X, "DE001", "DE001 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			additionalInfo.CSI_Status = EU.Business.AdditionalInfoIssuerList.Codes.Other;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(additionalInfo.CSI_CodeInfo, "INV", "DE001");
		}

		public void TestCheckCSI_Code_Unique()
		{
			CombineAssertions(() =>
			{
				additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.X1002;
				AssertNoMessageErrorContaining("Full Type unique", additionalInfo.CSI_CodeInfo, "has already been entered.");

				var additionalInfo2 = consignment.AdditionalInfos.AddNew();
				additionalInfo2.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.X1002;
				AssertHasMessageErrorContaining("Full Type not unique", additionalInfo2.CSI_CodeInfo, "has already been entered.");

				additionalInfo2.CSI_Code = "X1003";
				AssertNoMessageErrorContaining("2 different CSI_Code, no message error", additionalInfo2.CSI_CodeInfo, "has already been entered.");
			});
		}

		public void TestCheckCSI_Code_MovementReference_DE()
		{
			const string message = "The selected Full Type may not be entered, if the MRN does not contain 'DE' as digit 3 and 4.";

			CombineAssertions(() =>
			{
				consignment.CXC_MovementReference = "21DE27364916384836";
				additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.X1002;
				AssertNoMessageError("Valid MRN country code", additionalInfo.CSI_CodeInfo, message);

				consignment.CXC_MovementReference = "21CH27364916384834";
				additionalInfo.Validation.ValidateCSI_Code();
				AssertHasMessageError("Invalid MRN country code", additionalInfo.CSI_CodeInfo, message);

				consignment.CXC_MovementReference = ZString.Empty;
				additionalInfo.Validation.ValidateCSI_Code();
				AssertNoMessageError("Empty MRN", additionalInfo.CSI_CodeInfo, message);

				consignment.CXC_MovementReference = "21CH27364916384834";
				additionalInfo.CSI_Code = "X1003";
				AssertNoMessageError("CSI_Code isn't 'X1002'", additionalInfo.CSI_CodeInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_JobReference = exitHeader.PK.ToString().Substring(0, 35);
			consignment = exitHeader.CusExitConsignments.AddNew();
			consignment.CXC_Status = "REJ";
			additionalInfo = consignment.AdditionalInfos.AddNew();
		}
		CusExitConsignment consignment;
		ExitControlAdditionalInfo additionalInfo;
	}
}
