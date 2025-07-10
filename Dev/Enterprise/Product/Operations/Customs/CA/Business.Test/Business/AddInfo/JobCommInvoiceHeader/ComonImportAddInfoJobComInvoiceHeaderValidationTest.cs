using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.CA.Business.Testing
{
	abstract class ComonImportAddInfoJobComInvoiceHeaderValidationTest : CAAddInfoValidationTest<AddInfoJobComInvoiceHeader>
	{
		#region TestCheckCA_USStateOfExport

		public void TestCheckCA_USStateOfExport()
		{
			invoiceHeader.CA_RN_NKExport = Constants.CountryCodes.Canada;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USStateOfExportInfo);

			invoiceHeader.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceHeader.CA_USStateOfExportInfo, "12", USStatesList.Codes.Wyoming);
		}

		#endregion

		public void TestCheckCA_RL_NKLastPortExceptExport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfLoading = "CABCD";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CA";
			invoiceHeader.AddInfoValidation.ValidateCA_RL_NKLastPort();
			AssertHasMessageErrorContaining(invoiceHeader.CA_RL_NKLastPortInfo, CAAddInfoValidation.PlaceOfDirectShipmentCannotBeCanada);

			invoiceHeader.CA_RL_NKLastPort = "AUBNE";
			invoiceHeader.AddInfoValidation.ValidateCA_RL_NKLastPort();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceHeader.CA_RL_NKLastPort = "CABCD";
			invoiceHeader.AddInfoValidation.ValidateCA_RL_NKLastPort();
			AssertNoMessageErrorContaining(invoiceHeader.CA_RL_NKLastPortInfo, CAAddInfoValidation.PlaceOfDirectShipmentCannotBeCanada);
		}

		#region TestCheckCA_TimeLimit

		public virtual void TestCheckCA_TimeLimit()
		{
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(invoiceHeader.CA_TimeLimitInfo);

			invoiceHeader.CA_TimeLimit = 100;
			var messageError = "Time Limit value should not be greater than 99";
			AssertHasMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, messageError);

			invoiceHeader.CA_TimeLimit = 99;
			AssertNoMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, messageError);

			invoiceHeader.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			invoiceHeader.CA_TimeLimit = 0;
			AssertHasMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, "You must enter a time limit when you have entered a time limit code");
			invoiceHeader.CA_TimeLimit = 1;
			AssertNoMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, "You must enter a time limit when you have entered a time limit code");
		}

		#endregion

		#region TestCheckCA_TimeLimitCode

		public void TestCheckCA_TimeLimitCode()
		{
			invoiceHeader.CA_TimeLimit = 10;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceHeader.CA_TimeLimitCodeInfo, "A", TimeLimitUnitCodes.Codes.Month);

			invoiceHeader.CA_TimeLimit = 0;
			invoiceHeader.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			AssertHasMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, "You must enter a time limit when you have entered a time limit code");
			invoiceHeader.CA_TimeLimitCode = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, "You must enter a time limit when you have entered a time limit code");
		}

		#endregion

		#region Implementation

		protected override AddInfoJobComInvoiceHeader GetNewAddInfo()
		{
			return new AddInfoJobComInvoiceHeader(invoiceHeader.JZ_AddInfoInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = GetMessageType();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
		}

		protected abstract ZString GetMessageType();

		protected JobComInvoiceHeader invoiceHeader;
		protected JobDeclaration declaration;

		#endregion
	}
}
