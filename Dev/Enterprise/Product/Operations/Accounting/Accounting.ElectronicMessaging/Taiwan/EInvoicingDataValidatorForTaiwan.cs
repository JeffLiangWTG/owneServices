using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Taiwan
{
	public class EInvoicingDataValidatorForTaiwan : BaseEInvoicingDataValidator
	{
		public EInvoicingDataValidatorForTaiwan(GlbCompany company)
			: base(company)
		{
		}

		char[] InvalidCharacters => new[] { '<', '>', '&', '\'', '"', ':', '|' };

		protected override void RunCore(ILogger logger)
		{
			var pivots = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_Status, Core.Constants.EInvoicingPivotState.Batched));
			var complianceDocuments = Factory.Load<AccComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.PK, pivots.Select(p => p.AIP_ParentID)));

			var errorMessages = Validate(complianceDocuments, pivots);
			if (errorMessages.Any())
			{
				foreach (var errorMessageWithDocumentNumber in errorMessages)
				{
					logger.Log(LogType.Error, FormattableString.Invariant($"[{errorMessageWithDocumentNumber.documentNumber}] : Failed to send this compliance document due to validation error. \r\n {errorMessageWithDocumentNumber.errorMessage}"));
				}
			}
		}

		List<(ZString documentNumber, ZString errorMessage)> Validate(AccComplianceDocumentHeader[] complianceDocuments, AccEInvoicingTransactionPivot[] pivots)
		{
			var errorDescriptions = new List<(ZString documentNumber, ZString errorMessage)>();
			var needToCallSave = false;
			var logCollector = new SimpleLogger();

			foreach (var complianceDocument in complianceDocuments)
			{
				var errorDescription = Validate(complianceDocument);
				if (!errorDescription.IsEmpty)
				{
					var pivot = pivots.FirstOrDefault(p => p.AIP_ParentID == complianceDocument.PK);
					pivot.AIP_Status = Core.Constants.EInvoicingPivotState.BatchedWithError;
					pivot.AIP_ErrorDescription = errorDescription;
					needToCallSave = true;

					if (!AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.Value)
					{
						pivot.AIP_AIB = ZGuid.Empty;
					}

					errorDescriptions.Add((complianceDocument.ADH_DocumentNumber, errorDescription));
					SendErrorNotificationEmail(complianceDocument, errorDescription, logCollector);
				}
			}

			if (needToCallSave)
			{
				Factory.Save();
			}
			return errorDescriptions;
		}

		void SendErrorNotificationEmail(AccComplianceDocumentHeader complianceDocument, ZString errorDescription, SimpleLogger logCollector)
		{
			var emailCreator = new GEIEmailNotificationCreator(null, complianceDocument, new List<ZString>() { errorDescription }, logCollector);
			emailCreator.SendEmail();
		}

		ZString Validate(AccComplianceDocumentHeader complianceDocument)
		{
			List<string> validationResults = new List<string>();

			var validationResult = ValidateComplianceDocumentDescription(complianceDocument);
			if (!string.IsNullOrEmpty(validationResult))
			{
				validationResults.Add(validationResult);
			}

			validationResult = ValidComplianceDocumentLineDescription(complianceDocument);
			if (!string.IsNullOrEmpty(validationResult))
			{
				validationResults.Add(validationResult);
			}

			validationResult = ValidateAmount(complianceDocument);
			if (!string.IsNullOrEmpty(validationResult))
			{
				validationResults.Add(validationResult);
			}

			validationResult = ValidateComplianceDocumentVATRegistrationNum(complianceDocument);
			if (!string.IsNullOrEmpty(validationResult))
			{
				validationResults.Add(validationResult);
			}

			validationResult = ValidateComplianceDocumentCompanyVATRegistrationNum(complianceDocument);
			if (!string.IsNullOrEmpty(validationResult))
			{
				validationResults.Add(Res.GetString("705A1170-6F5E-48E5-8A48-3BBD67AD98AD", "Please check System Company VAT number. {0}", validationResult));
			}

			return string.Join("\r\n", validationResults);
		}

		ZString ValidateComplianceDocumentVATRegistrationNum(AccComplianceDocumentHeader complianceDocument)
		{
			var result = ZString.Empty;

			if (complianceDocument.Organisation != null
				&& complianceDocument.Organisation.OH_Category == OrgConstants.Category.Business
				&& complianceDocument.Organisation.CountryCode == CountryCodes.Taiwan)
			{
				result = TaiwanUnifiedBusinessNumberValidator.GetValidateNotifyInformation(complianceDocument.VATRegistrationNum);
			}

			return result;
		}

		ZString ValidateComplianceDocumentCompanyVATRegistrationNum(AccComplianceDocumentHeader complianceDocument)
		{
			return TaiwanUnifiedBusinessNumberValidator.GetValidateNotifyInformation(complianceDocument.CompanyVATRegistrationNum);
		}

		ZString ValidateComplianceDocumentDescription(AccComplianceDocumentHeader complianceDocument)
		{
			var result = ZString.Empty;

			if (complianceDocument.ADH_Description.ToString().ToArray().Any(x => InvalidCharacters.Contains(x)))
			{
				result = Res.GetString("4576DDCA-1151-43EA-806B-082F5EB3C8B4", "Compliance Document's description cannot contains any invalid characters.");
				return result;
			}
			return result;
		}

		ZString ValidComplianceDocumentLineDescription(AccComplianceDocumentHeader complianceDocument)
		{
			var result = ZString.Empty;

			foreach (AccComplianceDocumentLine line in complianceDocument.ComplianceDocumentLines)
			{
				if (line.ADL_Description.ToString().ToArray().Any(x => InvalidCharacters.Contains(x)))
				{
					result = Res.GetString("C7E075E9-E914-492D-8310-65E8D4196CF6", "Compliance Document Line's description cannot contains any invalid characters.");
					return result;
				}
			}
			return result;
		}

		ZString ValidateAmount(AccComplianceDocumentHeader complianceDocument)
		{
			var result = ZString.Empty;
			foreach (AccComplianceDocumentLine line in complianceDocument.ComplianceDocumentLines)
			{
				if (line.Amount < 0)
				{
					result = Res.GetString("1A3FEB42-B071-4039-B53E-C09BC39ADCB1", "Compliance Document Line's amount cannot less than 0.");
				}
			}
			return result;
		}
	}
}
