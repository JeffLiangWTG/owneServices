using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	public class EMCSInvoiceLineCusOutturnValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckC5_RejectedQuantity()
		{
			outturn.C5_RejectedQuantity = -1m;
			AssertHasErrorContaining(outturn.C5_RejectedQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
			outturn.C5_RejectedQuantity = 0m;
			AssertNoNotifications(outturn.C5_RejectedQuantityInfo);
		}

		public void TestCheckC5_RejectedQuantity_NoReason()
		{
			var propertyInfo = outturn.C5_RejectedQuantityInfo;
			const string errorMessage = "You have not entered a Reason.";
			outturn.C5_RejectedQuantity = 1;

			CombineAssertions(() =>
			{
				outturn.Validation.ValidateC5_RejectedQuantity();
				AssertHasMessageError("No reason", propertyInfo, errorMessage);

				outturn.ReportOfReceiptReasons.AddNew();
				outturn.Validation.ValidateC5_RejectedQuantity();
				AssertNoMessageError("Has reason", propertyInfo, errorMessage);
			});
		}

		public void TestCheckC5_RejectedQuantity_NotGreaterThanCustomsQuantity()
		{
			const string errorMessage = "The entered Refused Quantity must be less than or equal the Customs Quantity.";
			invoiceLine.JI_CustomsQuantity = 100;

			CombineAssertions(() =>
			{
				outturn.C5_RejectedQuantity = 1;
				AssertNoMessageError("RejectedQuantity < CustomsQuantity", outturn.C5_RejectedQuantityInfo, errorMessage);

				outturn.C5_RejectedQuantity = 100;
				AssertNoMessageError("RejectedQuantity = CustomsQuantity", outturn.C5_RejectedQuantityInfo, errorMessage);

				outturn.C5_RejectedQuantity = 101;
				AssertHasMessageError("RejectedQuantity > CustomsQuantity", outturn.C5_RejectedQuantityInfo, errorMessage);
			});
		}

		public void TestCheckC5_OutturnResultReason_WithObservedDifference()
		{
			var propertyInfo = outturn.C5_OutturnResultReasonInfo;
			CombineAssertions(() =>
			{
				invoiceLine.JI_CustomsQuantity = 2;
				invoiceLine.ZG_DeclaredValue = 1;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo, ExplanationForExcessOnShortageNotificationText);

				declaration.ZG_ExplanationOnReasonForShortageValidation = true;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo, ExplanationForExcessOnShortageNotificationText);
			});
		}

		public void TestCheckC5_OutturnResultReason_WithoutObservedDifference()
		{
			var propertyInfo = outturn.C5_OutturnResultReasonInfo;
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo, ExplanationForExcessOnShortageNotificationText);

				declaration.ZG_ExplanationOnReasonForShortageValidation = true;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo, ExplanationForExcessOnShortageNotificationText);
			});
		}

		public void TestCheckC5_OutturnResultReason_AllCharactersAllowed()
		{
			outturn.C5_OutturnResultReason = "Unicodetest-ÂÜ§$㐿㪳çËŠïÔчШĢøÅşŢǁǂ№™€";
			AssertNoErrors(outturn.C5_OutturnResultReasonInfo);
		}

		public void TestValidateObservedDifference()
		{
			declaration.JE_DeclarantType = "2";
			invoiceLine.JI_CustomsQuantity = 6;
			invoiceLine.ZG_DeclaredValue = 4;
			outturn.ReportOfReceiptReasons.AddNew().CY_Code = "1";
			outturn.Validation.ValidateAll();
			AssertNoMessageError(outturn.ObservedDifferenceInfo, "Please provide a reason");
			outturn.ReportOfReceiptReasons.RemoveAndDeleteAll();
			outturn.Validation.ValidateAll();
			AssertHasMessageError(outturn.ObservedDifferenceInfo, "Please provide a reason");
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<EMCSJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = Factory.New<EMCSJobComInvoiceLine>();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			outturn = invoiceLine.Outturn;
		}
		EMCSJobDeclaration declaration;
		EMCSJobComInvoiceLine invoiceLine;
		EMCSInvoiceLineCusOutturn outturn;

		const string ExplanationForExcessOnShortageNotificationText = "You have not entered an Explanation for Excess or Shortage.";
	}
}
