using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using UniversalReferenceConstants = Enterprise.Customs.IE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	class InformationOnNonExitedExportAlternativeEvidenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			(var alternativeEvidence, var report, _, _) = AlternativeEvidenceTest.GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			alternativeEvidence.AdditionalInfos.RemoveAndDeleteAll();
			var requiredMessageError = "At least one Transport Document is required";
			CombineAssertions("Required", () =>
			{
				foreach (var code in new[]
				{
					UniversalReferenceConstants.ExitReportAlternativeEvidenceTypes.Codes.DeliveryNoteSignedByConsigneeOutsideCustomsTerritory,
					UniversalReferenceConstants.ExitReportAlternativeEvidenceTypes.Codes.DeliveryNote,
					UniversalReferenceConstants.ExitReportAlternativeEvidenceTypes.Codes.DocumentSignedByOperatorTakingGoodsOutOfUnion,
					UniversalReferenceConstants.ExitReportAlternativeEvidenceTypes.Codes.OperatorsRecordsOfGoodsSuppliedToShipsAircraftOffshore,
				})
				{
					alternativeEvidence.CY_Code = code;
					var additionalInfo = alternativeEvidence.AdditionalInfos.AddNew();
					additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					alternativeEvidence.Validation.ValidateCY_Code();
					AssertHasMessageError(alternativeEvidence.CY_CodeInfo, requiredMessageError);
					additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
					alternativeEvidence.Validation.ValidateCY_Code();
					AssertNoMessageError(alternativeEvidence.CY_CodeInfo, requiredMessageError);
					additionalInfo.Delete();
					alternativeEvidence.Validation.ValidateCY_Code();
					AssertHasMessageError(alternativeEvidence.CY_CodeInfo, requiredMessageError);
				}
			});
			CombineAssertions("Not Allowed", () =>
			{
				var notAllowedMessageError = "Transport Document is not allowed for this selection";
				var additionalInfo = alternativeEvidence.AdditionalInfos.AddNew();
				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				alternativeEvidence.CY_Code = "!";
				AssertNoMessageError(alternativeEvidence.CY_CodeInfo, notAllowedMessageError);
				AssertNoMessageError(alternativeEvidence.CY_CodeInfo, requiredMessageError);

				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
				alternativeEvidence.Validation.ValidateCY_Code();
				AssertHasMessageError(alternativeEvidence.CY_CodeInfo, notAllowedMessageError);
				AssertNoMessageError(alternativeEvidence.CY_CodeInfo, requiredMessageError);

				additionalInfo.Delete();
				alternativeEvidence.Validation.ValidateCY_Code();
				AssertNoMessageError(alternativeEvidence.CY_CodeInfo, notAllowedMessageError);
				AssertNoMessageError(alternativeEvidence.CY_CodeInfo, requiredMessageError);
			});
		}
	}
}
