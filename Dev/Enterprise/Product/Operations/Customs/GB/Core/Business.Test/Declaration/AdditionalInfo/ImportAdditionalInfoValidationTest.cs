using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public sealed class ImportAdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_NIAID_RiskCode()
		{
			lineAdditionalInfo.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIAID;
			AssertNoRowMessageError(invoiceLine, AdditionalInfoValidation.NIAIDExclusionMessage.ToString());
			AssertHasRowMessageError(invoiceLine, AdditionalInfoValidation.RiskingStatementWithoutIMPorDOM_Message.ToString());
			invoiceLine.ClearRowNotifications();

			var lineAdditionalInfos = invoiceLine.AdditionalInfos.AddNew();
			lineAdditionalInfos.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIQUO;
			AssertHasRowMessageError(invoiceLine, AdditionalInfoValidation.NIAIDExclusionMessage.ToString());
			AssertHasRowMessageError(invoiceLine, AdditionalInfoValidation.NIQUOExclusionMessage.ToString());
			AssertHasRowMessageError(invoiceLine, AdditionalInfoValidation.RiskingStatementWithoutIMPorDOM_Message.ToString());

			invoiceLine.ClearRowNotifications();
			lineAdditionalInfos.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIPRO;
			AssertNoRowMessageError(invoiceLine, AdditionalInfoValidation.NIQUOExclusionMessage.ToString());
			AssertNoRowMessageError(invoiceLine, AdditionalInfoValidation.NIAIDExclusionMessage.ToString());
			AssertHasRowMessageError(invoiceLine, AdditionalInfoValidation.RiskingStatementWithoutIMPorDOM_Message.ToString());

			invoiceLine.ClearRowNotifications();
			var lineAdditionalInfos2 = invoiceLine.AdditionalInfos.AddNew();
			lineAdditionalInfos2.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIPRO;
			AssertHasRowMessageError(invoiceLine, AdditionalInfoValidation.NIAIDExclusionMessage.ToString());
			AssertHasRowMessageError(invoiceLine, AdditionalInfoValidation.RiskingStatementWithoutIMPorDOM_Message.ToString());

			invoiceLine.ClearRowNotifications();
			lineAdditionalInfos2.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIREM;
			AssertHasRowMessageError(invoiceLine, AdditionalInfoValidation.NIAIDExclusionMessage.ToString());
			AssertHasRowMessageError(invoiceLine, AdditionalInfoValidation.RiskingStatementWithoutIMPorDOM_Message.ToString());

			invoiceLine.ClearRowNotifications();
			lineAdditionalInfos2.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIIMP;
			AssertNoRowMessageError(invoiceLine, AdditionalInfoValidation.NIAIDExclusionMessage.ToString());
			AssertNoRowMessageError(invoiceLine, AdditionalInfoValidation.RiskingStatementWithoutIMPorDOM_Message.ToString());
		}

		public void TestCheckCSI_IMPorDOM_RiskCode()
		{
			lineAdditionalInfo.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIHIS;
			AssertNoRowMessageError(invoiceLine, AdditionalInfoValidation.NIHISExclusionMessage.ToString());
			AssertHasRowMessageError(invoiceLine, AdditionalInfoValidation.RiskingStatementWithoutIMPorDOM_Message.ToString());
			invoiceLine.ClearRowNotifications();

			var lineAdditionalInfos = invoiceLine.AdditionalInfos.AddNew();
			lineAdditionalInfos.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIDOM;
			AssertNoRowMessageError(invoiceLine, AdditionalInfoValidation.NIHISExclusionMessage.ToString());
			AssertNoRowMessageError(invoiceLine, AdditionalInfoValidation.RiskingStatementWithoutIMPorDOM_Message.ToString());
		}

		public void TestCheckCSI_NIQUO_RiskCode()
		{
			const string messageError = "NIQUO cannot be used on a job whose NI Protocol field is N2G";
			lineAdditionalInfo.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIQUO;
			AssertNoMessageError(lineAdditionalInfo.CSI_CodeInfo, messageError);
			AssertNoRowMessageError(invoiceLine, AdditionalInfoValidation.NIQUOExclusionMessage.ToString());

			var lineAdditionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			lineAdditionalInfo1.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIREM;
			AssertNoMessageError(lineAdditionalInfo1.CSI_CodeInfo, messageError);
			AssertHasRowMessageError(invoiceLine, AdditionalInfoValidation.NIQUOExclusionMessage.ToString());

			invoiceLine.ClearRowNotifications();
			declaration.JE_NorthernIrelandMode = NIModeList.Codes.MovementFromNiToGreatBritain;
			lineAdditionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageError(lineAdditionalInfo.CSI_CodeInfo, messageError);
			AssertHasRowMessageError(invoiceLine, AdditionalInfoValidation.NIQUOExclusionMessage.ToString());
		}

		public void TestCheckCSI_NIHIS_RiskCode()
		{
			lineAdditionalInfo.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIHIS;
			AssertNoRowMessageError(invoiceLine, AdditionalInfoValidation.NIHISExclusionMessage.ToString());

			var lineAdditionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			lineAdditionalInfo1.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIREM;
			AssertNoRowMessageError(invoiceLine, AdditionalInfoValidation.NIHISExclusionMessage.ToString());

			var lineAdditionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			lineAdditionalInfo2.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIOVR;
			AssertHasRowMessageError(invoiceLine, AdditionalInfoValidation.NIHISExclusionMessage.ToString());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			lineAdditionalInfo = invoiceLine.AdditionalInfos.AddNew();
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceHeader invoiceHeader;
		JobDeclaration declaration;
		AdditionalInfo lineAdditionalInfo;
	}
}
